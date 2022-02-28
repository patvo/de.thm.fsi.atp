﻿using System;
using System.Data;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Threading;


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
        private static string dataReceive = null;
        // Attributes for matching datagrid output:
        private static DataController dc;
        private static int idLecture;
        private static int idGroup;
        private static int idCurrLectureDate;
        private static string nameLecture;
        private static string nameCourse;
        private static string nameSpecialty;
        private static DataTable lecturesTable;
        private static DataTable studentTable;
        private static DataTable gridTable;
        private static DataTable currStudentTable;
        private static DataTable currLectTable;
        // Attributes for user interface:
        private static GuiController gc;

        public AtpBl(string iReaderAddr)
        {
            readerAddr = IPAddress.Parse(iReaderAddr);

            dc = new DataController();
            gc = new GuiController(this);

            // Start own thread for TCP/IP listening loop
            Thread t = new Thread(() => StartReaderConnection());
            t.Start();

            // Fill initial view
            lecturesTable = dc.GetAllLecturesGroups();
            gc.FillComboBox(lecturesTable);
            PrepareMatching();

            // For demo output
            WriteRoomAndLecture();

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
                idCurrLectureDate = Convert.ToInt32(row["idLehrveranstaltungstermin"]);
                currStudentTable = dc.GetStudentsPerLecture(idGroup, idLecture);
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
                studTable.Rows.Add(matrikelnummer, row["nachname"].ToString() + ", " + row["vorname"].ToString() + "\n" + matrikelnummer.ToString());
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
                gridTable.Rows[rowIdx][columnIdx] = false;
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
            TcpListener server = null;
            TcpClient clientOut = null;
            try
            {
                server = new TcpListener(localAddr, portPc);
                server.Start();
                clientOut = new TcpClient(readerAddr.ToString(), portReader);
                byte[] bytes = new byte[256]; // 256 byte buffer for reading data

                // Enter listening loop
                while (true)
                {
                    TcpClient clientIn = server.AcceptTcpClient();
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
                        if (CheckStudentCard(dataReceive.ToString()) == true)
                        {
                            // Send back back to reader: NO additional buzzer, green light
                            streamOut.Write(data_green, 0, data_green.Length);
                        }
                        else
                        {
                            // Send back a to reader: additional buzzer, red light
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
                Write("SocketException: " + e.ToString());
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
        /// <returns>Bool</returns>
        private bool CheckStudentCard(string dataReceive)
        {
            foreach (DataRow row in currStudentTable.Rows)
            {
                if (string.Compare(row["chipkartennummer"].ToString(), dataReceive, CultureInfo.CurrentCulture, CompareOptions.IgnoreCase | CompareOptions.IgnoreSymbols) == 0)
                {
                    Write("✔ Matrikelnummer " + row["matrikelnummer"].ToString() + " akzeptiert!");
                    dc.InsertAttendance(Convert.ToInt32(row["matrikelnummer"]), idCurrLectureDate);
                    return true;
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
        /// This writes the room, ip and current lecture to listbox.
        /// For demo purposes.
        /// </summary>
        private void WriteRoomAndLecture()
        {
            DataTable room = dc.GetRoom(readerAddr.ToString());
            foreach (DataRow row in room.Rows)
            {
                Write("Lesegerät mit IP " + readerAddr.ToString() + " in " + row["bezeichnung"].ToString() + ".");
            }

            foreach (DataRow row in currLectTable.Rows)
            {
                Write("Aktuelle Lehrveranstaltung: " + row["bezeichnung"].ToString() + " (" + row["zeitVon"].ToString() + " - " + row["zeitBis"].ToString() + ")");
            }

        }

    }
}
