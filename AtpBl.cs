using System;
using System.Data;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Timers;

namespace de.thm.fsi.atp
{
    /// <summary>
    /// Business logic for every connected RFID reader.
    /// Provides TCP/IP connection.
    /// Performs matching.
    /// </summary>
    internal class AtpBl
    {
        ////////
        // For demo purposes some attributes are hard coded!
        ////////
        // Attributes for TCP/IP connection to RFID reader:
        private static readonly IPAddress localAddr = IPAddress.Parse("192.168.178.101");
        private static readonly int portPc = 8890;
        private static IPAddress readerAddr;
        private static readonly int portReader = 10001;
        private static string dataReceive;
        private static TcpListener server;
        private static TcpClient clientOut;
        private static TcpClient clientIn;
        // Attributes for matching and datagrid output:
        private static bool checkForStudents = false;
        private static DataController dc;
        private static int idLecture;
        private static int idGroup;
        private static int currIdLectureDate;
        private static int prevIdLectureDate;
        private static string nameLecture;
        private static string nameCourse;
        private static string nameSpecialty;
        private static DataTable lecturesTable;
        private static DataTable studentTable;
        private static DataTable gridTable;
        private static DataTable currStudentTable;
        private static DataTable currLectTable;
        private static DataTable currDocTable;
        private static bool checkForPrevious = false;
        private static DataTable prevStudentTable;
        private static DataTable prevLectTable;
        private static DataTable prevDocTable;
        // Attributes for user interface:
        private static GuiController gc;

        public AtpBl(string iReaderAddr)
        {
            readerAddr = IPAddress.Parse(iReaderAddr);

            dc = new DataController();
            gc = new GuiController(this);

            // Start own thread for TCP/IP listening loop
            Thread tcpThread = new Thread(() => StartReaderConnection());
            tcpThread.Start();

            // Fill initial view
            lecturesTable = dc.GetAllLecturesGroups();
            gc.FillComboBox(lecturesTable);
            PrepareMatching();
            // Start timer thread to check for lecture to be over
            Thread timerThread = new Thread(() => StartTimerChecker());
            timerThread.Start();

            // For demo output
            WriteRoom();
            WriteLecture();

            gc.StartGui();
        }

        /// <summary>
        /// This finds current lecture of the room.
        /// </summary>
        private void FindCurrentLecture()
        {
            DateTime date = DateTime.Now;
            string strDate = date.ToString("yyyyMMdd");
            string strTime = date.ToString("HHmmss");
            currLectTable = dc.GetCurrLectForRoom(readerAddr.ToString(), strDate, strTime);
        }

        /// <summary>
        /// This sets attributes for matching algorithem and gets a list of all students of the current lecture.
        /// </summary>
        private void PrepareMatching()
        {
            FindCurrentLecture();
            foreach (DataRow row in currLectTable.Rows)
            {
                int idGroup = Convert.ToInt32(row["idStudiengruppe"]);
                int idLecture = Convert.ToInt32(row["idLehrveranstaltung"]);
                currIdLectureDate = Convert.ToInt32(row["idLehrveranstaltungstermin"]);
                currStudentTable = dc.GetStudentsPerLecture(idGroup, idLecture);
                currDocTable = dc.GetDocent(idGroup, idLecture);
                // Save in class attributes for previous lecture
                prevStudentTable = currStudentTable;
                prevLectTable = currLectTable;
                prevDocTable = currDocTable;
                prevIdLectureDate = currIdLectureDate;
            }
        }

        /// <summary>
        /// This gathers data from the database and fills the datagrid table.
        /// </summary>
        private void FillDataGridTable()
        {
            // Different dates for one lecture
            DataTable diffDatesTable = dc.GetDiffDatesPerLecture(idGroup, idLecture);
            // Add named column for every single date
            gridTable = new DataTable();
            gridTable.Columns.Add("idStudent", typeof(int));
            gridTable.Columns.Add("Studierende", typeof(string));
            DateTime date = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            int i = 1;
            foreach (DataRow row in diffDatesTable.Rows)
            {
                string strDate;
                // Check if date is already used as column name, if add number to name
                if (date != DateTime.Parse(row["datum"].ToString()))
                {
                    i = 1;
                    date = DateTime.Parse(row["datum"].ToString());
                    strDate = date.ToString("dd/MM/yyyy");
                }
                else
                {
                    i++;
                    date = DateTime.Parse(row["datum"].ToString());
                    strDate = date.ToString("dd/MM/yyyy") + " (" + i.ToString() + ")";
                }

                DataColumn column = new DataColumn
                {
                    DataType = Type.GetType("System.Boolean"),
                    Caption = row["idLehrveranstaltungstermin"].ToString(),
                    ColumnName = strDate,
                    ReadOnly = false
                };
                gridTable.Columns.Add(column);
            }

            // studTable = student + attendance dates
            DataTable studTable = new DataTable();
            studTable.Columns.Add("idStudent", typeof(int));
            studTable.Columns.Add("concat", typeof(string));
            studTable.Columns.Add("date", typeof(DataTable));
            // Get student list for lecture
            studentTable = dc.GetStudentsPerLecture(idGroup, idLecture);
            foreach (DataRow row in studentTable.Rows)
            {
                int matrikelnummer = int.Parse(row["matrikelnummer"].ToString());
                studTable.Rows.Add(matrikelnummer, row["nachname"].ToString() + ", " + row["vorname"].ToString() + " \n" + matrikelnummer.ToString());
            }

            // Get all attendances for one lecture
            DataTable attTable = dc.GetAttendancesPerLecture(idGroup, idLecture);
            // Set cell value true in gridTable if studend attended
            int index = 0;
            foreach (DataRow rowSum in studTable.Rows)
            {
                gridTable.Rows.Add(rowSum["idStudent"].ToString(), rowSum["concat"].ToString());

                foreach (DataRow row in attTable.Rows)
                {
                    string strDate = DateTime.Parse(row["datum"].ToString()).ToString("dd/MM/yyyy");

                    if (int.Parse(rowSum["idStudent"].ToString()) == int.Parse(row["matrikelnummer"].ToString()))
                    {
                        int idxCol = 0;
                        foreach (DataColumn col in gridTable.Columns)
                        {
                            if (idxCol >= 2 && (int.Parse(col.Caption.ToString()) == int.Parse(row["idLehrveranstaltungstermin"].ToString())))
                            {
                                gridTable.Rows[index][idxCol] = true;
                            }
                            idxCol++;
                        }
                    }
                }
                index++;
            }

            // Update UI
            gc.SetTitle(nameSpecialty, nameCourse, nameLecture);
            gc.UpdateDgv(gridTable);
        }

        /// <summary>
        /// This finds absentees of a selected lecture.
        /// </summary>
        /// <returns>Table of strings of abstentees + date</returns>
        public DataTable FindAbsentees()
        {
            DataTable absentTable = new DataTable();
            absentTable.Columns.Add("strAbs", typeof(string));
            if (gridTable != null)
            {
                if (gridTable.Rows.Count != 0)
                {
                    int idxRow = 0;
                    foreach (DataRow row in gridTable.Rows)
                    {
                        int idxCln = 0;
                        foreach (DataColumn column in gridTable.Columns)
                        {
                            object cellValue = gridTable.Rows[idxRow][idxCln];
                            if (cellValue is System.DBNull)
                            {
                                absentTable.Rows.Add(row["Studierende"].ToString() + " war abwesend am: " + column.ColumnName.ToString());
                            }
                            idxCln++;
                        }
                        idxRow++;
                    }
                    return absentTable;
                }
            }
            return absentTable;
        }

        /// <summary>
        /// Sets class attributes according to dropdown list selection.
        /// Initiates DataGrid fill.
        /// </summary>
        public void SetLecture(int index)
        {
            idLecture = int.Parse(lecturesTable.Rows[index]["idLehrveranstaltung"].ToString());
            idGroup = int.Parse(lecturesTable.Rows[index]["idStudiengruppe"].ToString());
            nameLecture = lecturesTable.Rows[index]["bezeichnung"].ToString();
            nameCourse = lecturesTable.Rows[index]["studiengangLang"].ToString();
            nameSpecialty = lecturesTable.Rows[index]["fachrichtungLang"].ToString();
            FillDataGridTable();
        }

        /// <summary>
        /// Changes cell value for datagrid selection.
        /// </summary>
        public void UpdateCell(int rowIdx, int columnIdx, bool value)
        {
            int idStudent = int.Parse(gridTable.Rows[rowIdx][0].ToString());
            int idLectureDate = int.Parse(gridTable.Columns[columnIdx].Caption.ToString());

            if (value == true)
            {
                gridTable.Rows[rowIdx][columnIdx] = System.DBNull.Value;
                dc.DeleteAttendance(idStudent, idLectureDate);
            }
            else
            {
                gridTable.Rows[rowIdx][columnIdx] = true;
                dc.InsertAttendance(idStudent, idLectureDate);
            }
            gc.UpdateDgv(gridTable);
        }

        /// <summary>
        /// This starts a TCP/IP connection to a network RFID reader.
        /// </summary>
        private void StartReaderConnection()
        {
            try
            {
                server = new TcpListener(localAddr, portPc);
                server.Start();
                clientOut = new TcpClient(readerAddr.ToString(), portReader);
                byte[] bytes = new byte[256]; // 256 byte buffer for reading data

                // Enter listening loop
                while (true)
                {
                    clientIn = server.AcceptTcpClient();
                    dataReceive = null;
                    // ASCII encoding for reader communication
                    byte[] data_green = System.Text.Encoding.ASCII.GetBytes("000000010101"); // NO additional buzzer, green light
                    byte[] data_red = System.Text.Encoding.ASCII.GetBytes("000010100101"); // additional buzzer, red light

                    // Stream objects for reading and writing
                    NetworkStream streamIn = clientIn.GetStream();
                    NetworkStream streamOut = clientOut.GetStream();

                    // Loop to receive all the data sent by client
                    int i;
                    while ((i = streamIn.Read(bytes, 0, bytes.Length)) != 0)
                    {
                        // Translate data bytes to ASCII string
                        dataReceive = System.Text.Encoding.ASCII.GetString(bytes, 0, i);
                        Write("Chipkartennummer: " + dataReceive.ToString());

                        // Check for match
                        if (Matcher(dataReceive.ToString()))
                        {
                            // Send back to reader: NO additional buzzer, green light
                            streamOut.Write(data_green, 0, data_green.Length);
                        }
                        else
                        {
                            // Send back to reader: additional buzzer, red light
                            streamOut.Write(data_red, 0, data_red.Length);
                        }
                    }
                    // Shutdown and end connection
                    clientIn.Close();
                    streamIn.Close();

                }
            }
            catch (SocketException e)
            {
                Write("### SocketException: " + e.ToString());
            }
            finally
            {
                // Stop listening
                server.Stop();
            }
        }



        /// <summary>
        /// This checks if there is a match of scanned card UID in students of lecture.
        /// If card matches current lecture an attendance insert on the database is performed.
        /// Additionally there is an output for the demo purposes.
        /// </summary>
        /// <param name="dataReceive">Scanned card UID</param>
        /// <param name="dataTable">Current or previous student table</param>
        /// <param name="idLectureDate">Current or previous lecture date id</param>
        /// <returns>Bool</returns>
        private bool CheckStudentCard(string dataReceive, DataTable dataTable, int idLectureDate)
        {
            if (dataTable != null)
            {
                foreach (DataRow row in dataTable.Rows)
                {
                    if (string.Compare(row["chipkartennummer"].ToString(), dataReceive, CultureInfo.CurrentCulture, CompareOptions.IgnoreCase | CompareOptions.IgnoreSymbols) == 0)
                    {
                        Write("✔ " + row["vorname"].ToString() + " " + row["nachname"].ToString() + " akzeptiert!");
                        dc.InsertAttendance(Convert.ToInt32(row["matrikelnummer"]), idLectureDate);
                        return true;
                    }
                }
            }
            Write("❌ Abgelehnt!");
            return false;
        }

        /// <summary>
        /// This checks if there is a match of scanned card UID in docent of lecture.
        /// Additionally there is an output for the demo purposes.
        /// </summary>
        /// <param name="dataReceive">Scanned card UID</param>
        /// <param name="dataTable">Current or previous docent table</param>
        /// <returns>Bool</returns>
        private bool CheckDocentCard(string dataReceive, DataTable dataTable)
        {
            if (dataTable != null)
            {
                foreach (DataRow row in dataTable.Rows)
                {
                    if (string.Compare(row["chipkartennummer"].ToString(), dataReceive, CultureInfo.CurrentCulture, CompareOptions.IgnoreCase | CompareOptions.IgnoreSymbols) == 0)
                    {
                        checkForStudents = true; // after docent card is recognized student cards are getting scanned
                        Write("✔ " + row["anrede"].ToString() + " " + row["titel"].ToString() + " " + row["vorname"].ToString() + " " + row["nachname"].ToString() + " akzeptiert!");
                        return true;
                    }
                }
            }
            Write("❌ Abgelehnt!");
            return false;
        }

        /// <summary>
        /// This passes text output to listbox.
        /// Mainly used for simple text output for demo.
        /// </summary>
        /// <param name="text">Text string</param>
        private void Write(string text)
        {
            gc.AddToListbox(text);
        }

        /// <summary>
        /// This writes IP address and room to listbox.
        /// </summary>
        private void WriteRoom()
        {
            DataTable room = dc.GetRoom(readerAddr.ToString());
            foreach (DataRow row in room.Rows)
            {
                Write("Lesegerät mit IP " + readerAddr.ToString() + " in " + row["bezeichnung"].ToString() + ".");
            }
        }

        /// <summary>
        /// This writes current lecture to listbox.
        /// For demo purposes.
        /// </summary>
        private void WriteLecture()
        {
            if (currLectTable.Rows.Count == 0)
            {
                Write("Aktuell findet keine Veranstaltung statt.");
            }
            else
            {
                foreach (DataRow row in currLectTable.Rows)
                {
                    Write("Aktuelle Lehrveranstaltung: " + row["bezeichnung"].ToString() + " (" + row["zeitVon"].ToString() + " - " + row["zeitBis"].ToString() + ")");
                }
            }
        }

        /// <summary>
        /// This refreshs datagrid table.
        /// Only used in demo.
        /// </summary>
        public void RefreshGrid()
        {
            if (gridTable != null)
            {
                FillDataGridTable();
            }
        }

        /// <summary>
        /// This checks for a proper match in docent or studend tables depending on the current or previous lecture.
        /// </summary>
        /// <param name="dataReceive">Scanned card UID</param>
        /// <returns>Bool</returns>
        private bool Matcher(string dataReceive)
        {
            // Check for match in previous oder current lecture tables
            if (checkForPrevious == false)
            {
                if ((checkForStudents == false && CheckDocentCard(dataReceive, currDocTable) == true) ||
                (checkForStudents == true && CheckStudentCard(dataReceive, currStudentTable, currIdLectureDate) == true))
                {
                    return true;
                }
            }
            else if (checkForPrevious == true)
            {
                if ((checkForStudents == false && CheckDocentCard(dataReceive, prevDocTable) == true) ||
                (checkForStudents == true && CheckStudentCard(dataReceive, prevStudentTable, prevIdLectureDate) == true))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// This starts a 15 seconds timer to check if a lecture is already over.
        /// </summary>
        private void StartTimerChecker()
        {
            System.Timers.Timer currLectTimer = new System.Timers.Timer(15 * 1000);
            currLectTimer.Elapsed += new ElapsedEventHandler(CheckLectureOver);
            currLectTimer.Start();
        }

        /// <summary>
        /// This checks if there is a lecture going on.
        /// If lecture has ended will start a 15 minutes timer to book all scans on previous lecture.
        /// </summary>
        /// <param name="source">Object</param>
        /// <param name="e">Data for elapsed event</param>
        private void CheckLectureOver(object source, ElapsedEventArgs e)
        {
            PrepareMatching();
            if (currLectTable.Rows.Count == 0 && checkForPrevious == false && prevIdLectureDate > 0)
            {
                checkForPrevious = true;
                WriteLecture();
                Thread timerThread = new Thread(() => StartTimerLectureOver());
                timerThread.Start();
            }
        }

        /// <summary>
        /// This starts a 15 minutes timer to not check for previous lecture anymore.
        /// </summary>
        private void StartTimerLectureOver()
        {
            System.Timers.Timer prevLectTimer = new System.Timers.Timer(15 * 60 * 1000);
            prevLectTimer.Elapsed += new ElapsedEventHandler(NotCheckPrevious);
            prevLectTimer.Start();
        }

        /// <summary>
        /// This resets flags to not check for previous lecture anymore.
        /// </summary>
        /// <param name="source">Object</param>
        /// <param name="e">Data for elapsed event</param>
        private void NotCheckPrevious(object source, ElapsedEventArgs e)
        {
            PrepareMatching();
            WriteLecture();
            checkForPrevious = false;
            checkForStudents = false;
        }

        /// <summary>
        /// This closes all TCP/IP and database connections.
        /// Then exits whole application.
        /// </summary>
        public void Shutdown()
        {
            dc.CloseConnection();
            if (clientIn != null)
            {
                clientIn.Close();
            }
            if (clientOut != null)
            {
                clientOut.Close();
            }
            if (server != null)
            {
                server.Stop();
            }
            System.Windows.Forms.Application.Exit();
        }
    }
}
