using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;


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
        private static IPAddress localAddr = IPAddress.Parse("192.168.178.101");
        private static Int32 portPc = 8890;
        private static IPAddress readerAddr;
        private static Int32 portReader = 10001;
        private static String dataReceive = null;
        // Attributes for matching:
        private static DataController dataController;
        private static int idLecture;
        private static int idGroup;
        private static string nameLecture;
        private static string nameCourse;
        private static string nameSpecialty;
        private static DataTable lecturesTable;
        private static DataTable gridTable;
        // Attributes for user interface:
        private static GuiController guiController;

        public AtpBl(String cReaderAddr)
        {
            readerAddr = IPAddress.Parse(cReaderAddr);

            dataController = new DataController();
            guiController = new GuiController(this);

            // Start own thread for TCP/IP listener
            Thread t = new Thread(() => StartReaderConnection()); // TODO
            t.Start();

            // Fill initial view and start gui
            lecturesTable = dataController.GetAllLecturesGroups();
            guiController.FillComboBox(lecturesTable);
            guiController.StartGui();


        }


        /// <summary>
        /// This gathers data from the database and fills the datagrid table.
        /// </summary>
        private void FillDataGrid()
        {
            // different dates for one lecture
            DataTable diffDatesTable = dataController.GetDiffDatesPerLecture(idGroup, idLecture);
            // Add named column for every single date
            gridTable = new DataTable();
            gridTable.Columns.Add("idStudent", typeof(int));
            gridTable.Columns.Add("Studierende", typeof(string));
            DateTime date = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            int i = 1;
            foreach (DataRow row in diffDatesTable.Rows)
            {
                string strDate;
                // check if date is already used as column name, if add number to name
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

                DataColumn column = new DataColumn();
                column.DataType = Type.GetType("System.Boolean");
                column.Caption = row["idLehrveranstaltungstermin"].ToString();
                column.ColumnName = strDate;
                column.ReadOnly = false;
                gridTable.Columns.Add(column);
            }

            // Student table: student + attendance dates
            DataTable studTable = new DataTable();
            studTable.Columns.Add("idStudent", typeof(int));
            studTable.Columns.Add("concat", typeof(string));
            studTable.Columns.Add("date", typeof(DataTable));

            // Get student list for lecture
            DataTable studentTable = dataController.GetStudentsPerLecture(idGroup, idLecture);
            foreach (DataRow row in studentTable.Rows)
            {
                int matrikelnummer = int.Parse(row["matrikelnummer"].ToString());
                studTable.Rows.Add(matrikelnummer);
            }

            // All attendances for one lecture
            DataTable attTable = dataController.GetAttendancesPerLecture(idGroup, idLecture);

            // Extract students from attendance table, concatenate full name + id, fill distinct name table
            foreach (DataRow row in attTable.Rows)
            {
                int matrikelnummer = int.Parse(row["matrikelnummer"].ToString());
                string nachname = row["nachname"].ToString();
                string vorname = row["vorname"].ToString();

                string concat = vorname + " " + nachname + "\n" + matrikelnummer.ToString();
                foreach (DataRow rowSum in studTable.Rows)
                {
                    if (int.Parse(rowSum["idStudent"].ToString()) == matrikelnummer)
                    {
                        rowSum["concat"] = concat;
                    }
                }
            }

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
            guiController.SetTitle(nameSpecialty, nameCourse, nameLecture);
            guiController.UpdateDgv(gridTable);
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
            FillDataGrid();
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
                dataController.DeleteAttendance(idStudent, idLectureDate);
            }
            else
            {
                gridTable.Rows[rowIdx][columnIdx] = true;
                dataController.InsertAttendance(idStudent, idLectureDate);
            }
            guiController.UpdateDgv(gridTable);
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
                Byte[] bytes = new Byte[256]; // 256 byte buffer for reading data

                // Enter listening loop
                while (true)
                {
                    guiController.AddToListbox("Waiting for a connection... ");
                    Console.Write("Waiting for a connection... ");
                    TcpClient clientIn = server.AcceptTcpClient();
                    Console.WriteLine("## " + readerAddr + " connected!");
                    guiController.AddToListbox("## " + readerAddr + " connected!");

                    dataReceive = null;
                    // ASCII encoding for reader communication
                    Byte[] data_green = System.Text.Encoding.ASCII.GetBytes("000000010101"); // NO additional buzzer, green light
                    Byte[] data_red = System.Text.Encoding.ASCII.GetBytes("000010100101"); // additional buzzer, red light

                    // Stream objects for reading and writing
                    NetworkStream streamIn = clientIn.GetStream();
                    NetworkStream streamOut = clientOut.GetStream();

                    // Loop to receive all the data sent by client
                    int i;
                    while ((i = streamIn.Read(bytes, 0, bytes.Length)) != 0)
                    {
                        // Translate data bytes to ASCII string
                        dataReceive = System.Text.Encoding.ASCII.GetString(bytes, 0, i);
                        Console.WriteLine("Received: {0}", dataReceive);
                        guiController.AddToListbox("Received: {0}" + dataReceive.ToString());

                        //
                        if (Check() == true)
                        {
                            // Send back positive response
                            streamOut.Write(data_green, 0, data_green.Length);
                            Console.WriteLine("Accepted!");
                        }
                        else
                        {
                            // Send back a negative response
                            streamOut.Write(data_red, 0, data_red.Length);
                            Console.WriteLine("Denied!");
                        }
                        //

                    }

                    // Shutdown and end connection
                    clientIn.Close();
                    streamIn.Close();

                }
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }
            finally
            {
                // Stop listening
                server.Stop();
            }

            Console.WriteLine("\n## Controller crashed.");
            Console.Read();

            //Console.SetOut(writer);
        }

        /// <summary>
        /// This checks if there is a match of scanned card UID in database
        /// </summary>
        /// <returns>Bool</returns>
        private bool Check()
        {
            // TODO
            return false;
        }

    }
}
