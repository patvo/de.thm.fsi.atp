using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace de.thm.fsi.atp
{
    /// <summary>
    /// Business logic for every connected RFID reader.
    /// Provides TCP/IP connection.
    /// Performs matching.
    /// </summary>
    internal class AtpBl
    {
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

            // Fill initial view
            lecturesTable = dataController.GetAllLecturesGroups();
            guiController.FillComboBox(lecturesTable);
            guiController.StartGui();
        }



        private void FillDataGrid()
        {
            // different dates for one lecture
            DataTable diffDatesTable = dataController.GetDiffDatesPerLecture(idGroup, idLecture);
            // add named column for every single date
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

                //gridTable.Columns.Add(strDate, System.Type.GetType("System.Boolean"));
                ////
                //string idLecture = row["idLehrveranstaltungstermin"].ToString();
                DataColumn column = new DataColumn();
                column.DataType = System.Type.GetType("System.Boolean");
                //column.AllowDBNull = false;
                column.Caption = row["idLehrveranstaltungstermin"].ToString();
                column.ColumnName = strDate;
                //column.Prefix = 1;
                //column.Unique = true;
                column.ReadOnly = false;
                //column.DefaultValue = 25;
                gridTable.Columns.Add(column);
                ////
            }

            // student table: student + attendance dates
            DataTable studTable = new DataTable();
            studTable.Columns.Add("idStudent", typeof(int));
            studTable.Columns.Add("concat", typeof(string));
            studTable.Columns.Add("date", typeof(DataTable));

            //get student list for lecture
            DataTable studentTable = dataController.GetStudentsPerLecture(idGroup, idLecture);
            foreach (DataRow row in studentTable.Rows)
            {
                int matrikelnummer = int.Parse(row["matrikelnummer"].ToString());
                studTable.Rows.Add(matrikelnummer);
            }

            // all attendances for one lecture
            DataTable attTable = dataController.GetAttendancesPerLecture(idGroup, idLecture);

            // extract students from attendance table, concatenate full name + id, fill distinct name table
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

            int index = 0;
            foreach (DataRow rowSum in studTable.Rows)
            {
                gridTable.Rows.Add(rowSum["idStudent"].ToString(), rowSum["concat"].ToString());

                foreach (DataRow row in attTable.Rows)
                {
                    string strDate = DateTime.Parse(row["datum"].ToString()).ToString("dd/MM/yyyy");

                    if (int.Parse(rowSum["idStudent"].ToString()) == int.Parse(row["matrikelnummer"].ToString())
                        && gridTable.Columns.Contains(strDate)) // TODO: Contains not suitable for same lectures on same date -> needs compare of date ID!
                    {
                        ////
                        int idxCol = 0;
                        foreach (DataColumn col in gridTable.Columns)
                        {
                            //if ((int.Parse(col.Caption.ToString()) == int.Parse(row["idLehrveranstaltungstermin"].ToString())))

                            //int cap = int.Parse(col.Caption.ToString());
                            //int id = int.Parse(row["idLehrveranstaltungstermin"].ToString());
                            if (idxCol >= 2 && (int.Parse(col.Caption.ToString()) == int.Parse(row["idLehrveranstaltungstermin"].ToString())))
                            //if (idxCol >= 2 && cap == id)
                            {
                                //int x = gridTable.Columns.IndexOf(strDate);
                                gridTable.Rows[index][idxCol] = true;
                            }
                            //else if (idxCol > 2) //&& (int.Parse(col.Caption.ToString()) != int.Parse(row["idLehrveranstaltungstermin"].ToString())))
                            //{
                            //    gridTable.Rows[index][idxCol] = false;
                            //}

                            idxCol++;
                        }
                        ////
                        //gridTable.Rows[index][gridTable.Columns.IndexOf(strDate)] = true;
                    }
                    //else
                    //{
                    //    gridTable.Rows[index][gridTable.Columns.IndexOf(strDate)] = false;
                    //}
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


            string x = gridTable.Columns[columnIdx].Caption.ToString();
            string y = gridTable.Columns[columnIdx].ToString();
            Console.WriteLine(y);
            Console.WriteLine(x);
            Console.WriteLine(value);

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


    }
}
