﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace de.thm.fsi.atp
{
    /// <summary>
    /// Data controller to access needed data.
    /// In this demo case its a MySQL database controller for connection, queries, insertions and deletions.
    /// Connected to a local mysql db.
    /// </summary>
    internal class DataController
    {
        private static String dbConnectionString = "server=localhost;user id=user;database=atp;port=3306;persistsecurityinfo=True;password=bla123";
        private MySqlConnection connection;
        private MySqlDataAdapter dataAdapter;

    public DataController()
    {
       connection = new MySqlConnection(dbConnectionString);
       connection.Open();
       dataAdapter = new MySqlDataAdapter();
    }
        /// <summary>
        /// This returns a table of all distinct dates for one lecture.
        /// </summary>
        /// <param name="idGroup">idStudiengruppe</param>
        /// <param name="idLecture">idLehrveranstaltung</param>
        /// <returns>Query result DataTable</returns>
        public DataTable GetDiffDatesPerLecture(int idGroup, int idLecture)
        {
            DataTable dtDates = new DataTable();
            String sqlDates = "Select Distinct studiengruppe.studiengangKurz, lehrveranstaltung.bezeichnung, lehrveranstaltungstermin.datum, lehrveranstaltungstermin.zeitVon, lehrveranstaltungstermin.zeitBis, lehrveranstaltungstermin.idLehrveranstaltungstermin From lehrveranstaltung Inner Join lehrveranstaltungstermin On lehrveranstaltungstermin.idLehrveranstaltung = lehrveranstaltung.idLehrveranstaltung Inner Join studiengruppe_has_lehrveranstaltung On studiengruppe_has_lehrveranstaltung.idLehrveranstaltung = lehrveranstaltung.idLehrveranstaltung Inner Join studiengruppe On studiengruppe_has_lehrveranstaltung.idStudiengruppe = studiengruppe.idStudiengruppe Where lehrveranstaltungstermin.idLehrveranstaltung = " + idLecture + " And lehrveranstaltungstermin.idStudiengruppe = " + idGroup + " Order By lehrveranstaltungstermin.datum";
            dataAdapter.SelectCommand = new MySqlCommand(sqlDates, connection);
            dataAdapter.Fill(dtDates);
            return dtDates;
        }

        /// <summary>
        /// This returns a table of all stundets for one lecture.
        /// </summary>
        /// <param name="idGroup">idStudiengruppe</param>
        /// <param name="idLecture">idLehrveranstaltung</param>
        /// <returns>Query result DataTable</returns>
        public DataTable GetStudentsPerLecture(int idGroup, int idLecture)
        {
            DataTable dtStudents = new DataTable();
            String sqlStud = "Select student.matrikelnummer, studiengruppe.studiengangKurz, lehrveranstaltung.bezeichnung From lehrveranstaltung Inner Join studiengruppe_has_lehrveranstaltung On studiengruppe_has_lehrveranstaltung.idLehrveranstaltung = lehrveranstaltung.idLehrveranstaltung Inner Join studiengruppe On studiengruppe_has_lehrveranstaltung.idStudiengruppe = studiengruppe.idStudiengruppe Inner Join student On student.Studiengruppe_idStudiengruppe = studiengruppe.idStudiengruppe Where lehrveranstaltung.idLehrveranstaltung = " + idLecture + " And student.Studiengruppe_idStudiengruppe = " + idGroup + " Group By student.matrikelnummer, studiengruppe.studiengangKurz, lehrveranstaltung.bezeichnung Order By student.matrikelnummer";
            dataAdapter.SelectCommand = new MySqlCommand(sqlStud, connection);
            dataAdapter.Fill(dtStudents);
            return dtStudents;
        }

        /// <summary>
        /// This returns a table of all attendances of students for one lecture.
        /// </summary>
        /// <param name="idGroup">idStudiengruppe</param>
        /// <param name="idLecture">idLehrveranstaltung</param>
        /// <returns>Query result DataTable</returns>
        public DataTable GetAttendancesPerLecture(int idGroup, int idLecture)
        {
            DataTable dtAtt = new DataTable();
            String sqlAtt = "Select student.matrikelnummer, student.nachname, student.vorname, studiengruppe.studiengangKurz, lehrveranstaltung.bezeichnung, lehrveranstaltungstermin.datum, lehrveranstaltungstermin.idLehrveranstaltungstermin From anwesenheit Inner Join lehrveranstaltungstermin_has_raum On anwesenheit.idLehrveranstaltungstermin = lehrveranstaltungstermin_has_raum.idLehrveranstaltungstermin And anwesenheit.idLehrveranstaltung = lehrveranstaltungstermin_has_raum.idLehrveranstaltung And anwesenheit.idRaum = lehrveranstaltungstermin_has_raum.idRaum And anwesenheit.inventarnummer = lehrveranstaltungstermin_has_raum.inventarnummer Inner Join lehrveranstaltungstermin On lehrveranstaltungstermin_has_raum.idLehrveranstaltungstermin = lehrveranstaltungstermin.idLehrveranstaltungstermin And lehrveranstaltungstermin_has_raum.idLehrveranstaltung = lehrveranstaltungstermin.idLehrveranstaltung Inner Join student On anwesenheit.matrikelnummer = student.matrikelnummer And anwesenheit.idStudiengruppe = student.Studiengruppe_idStudiengruppe Inner Join studiengruppe On student.Studiengruppe_idStudiengruppe = studiengruppe.idStudiengruppe Inner Join lehrveranstaltung On lehrveranstaltungstermin.idLehrveranstaltung = lehrveranstaltung.idLehrveranstaltung Inner Join studiengruppe_has_lehrveranstaltung On studiengruppe_has_lehrveranstaltung.idStudiengruppe = studiengruppe.idStudiengruppe And studiengruppe_has_lehrveranstaltung.idLehrveranstaltung = lehrveranstaltung.idLehrveranstaltung Where lehrveranstaltung.idLehrveranstaltung = " + idLecture + " And student.Studiengruppe_idStudiengruppe = " + idGroup + " Order By student.matrikelnummer, lehrveranstaltungstermin.datum";
            dataAdapter.SelectCommand = new MySqlCommand(sqlAtt, connection);
            dataAdapter.Fill(dtAtt);
            return dtAtt;
        }

        /// <summary>
        /// This returns a table of all lecture groups.
        /// </summary>
        /// <returns>Query result DataTable</returns>
        public DataTable GetAllLecturesGroups()
        {
            DataTable dtLect = new DataTable();
            String sqlLect = "Select studiengruppe.*, lehrveranstaltung.idLehrveranstaltung, lehrveranstaltung.bezeichnung From studiengruppe Inner Join studiengruppe_has_lehrveranstaltung On studiengruppe_has_lehrveranstaltung.idStudiengruppe = studiengruppe.idStudiengruppe Inner Join lehrveranstaltung On studiengruppe_has_lehrveranstaltung.idLehrveranstaltung = lehrveranstaltung.idLehrveranstaltung";
            dataAdapter.SelectCommand = new MySqlCommand(sqlLect, connection);
            dataAdapter.Fill(dtLect);
            return dtLect;
        }

        /// <summary>
        /// This inserts an attendance of one student for a lecture.
        /// </summary>
        /// <param name="idStudent">Matrikelnummer</param>
        /// <param name="idLectureDate">idVeranstaltungstermin</param>
        public void InsertAttendance(int idStudent, int idLectureDate)
        {
            String sqlInsAtt = "Insert into anwesenheit(matrikelnummer, Student_idStudiengruppe, idRaum, inventarnummer, idLehrveranstaltungstermin, idStudiengruppe, idLehrveranstaltung) Select Distinct " + idStudent + " as matrikelnummer, student.Studiengruppe_idStudiengruppe As Student_idStudiengruppe, lehrveranstaltungstermin_has_raum.idRaum As idRaum, lehrveranstaltungstermin_has_raum.inventarnummer As inventarnummer, lehrveranstaltungstermin_has_raum.idLehrveranstaltungstermin As idLehrveranstaltungstermin, lehrveranstaltungstermin_has_raum.idStudiengruppe As idStudiengruppe, lehrveranstaltungstermin_has_raum.idLehrveranstaltung As idLehrveranstaltung From lehrveranstaltungstermin_has_raum Inner Join lehrveranstaltungstermin On lehrveranstaltungstermin_has_raum.idLehrveranstaltungstermin = lehrveranstaltungstermin.idLehrveranstaltungstermin And lehrveranstaltungstermin_has_raum.idLehrveranstaltung = lehrveranstaltungstermin.idLehrveranstaltung Inner Join lehrveranstaltung On lehrveranstaltungstermin.idLehrveranstaltung = lehrveranstaltung.idLehrveranstaltung Inner Join studiengruppe_has_lehrveranstaltung On studiengruppe_has_lehrveranstaltung.idLehrveranstaltung = lehrveranstaltung.idLehrveranstaltung Inner Join studiengruppe On studiengruppe_has_lehrveranstaltung.idStudiengruppe = studiengruppe.idStudiengruppe Inner Join student On student.Studiengruppe_idStudiengruppe = studiengruppe.idStudiengruppe Where lehrveranstaltungstermin.idLehrveranstaltungstermin = " + idLectureDate;
            MySqlCommand command = new MySqlCommand(sqlInsAtt, connection);
            command.ExecuteNonQuery();
        }

        /// <summary>
        /// This delets an attendance of one stundet for one lecture.
        /// </summary>
        /// <param name="idStudent">Matrikelnummer</param>
        /// <param name="idLectureDate">idVeranstaltungstermin</param>
        public void DeleteAttendance(int idStudent, int idLectureDate)
        {
            String sqlDelAtt = "Delete anwesenheit From anwesenheit Inner Join lehrveranstaltungstermin_has_raum On anwesenheit.idLehrveranstaltungstermin = lehrveranstaltungstermin_has_raum.idLehrveranstaltungstermin And anwesenheit.idLehrveranstaltung = lehrveranstaltungstermin_has_raum.idLehrveranstaltung And anwesenheit.idRaum = lehrveranstaltungstermin_has_raum.idRaum And anwesenheit.inventarnummer = lehrveranstaltungstermin_has_raum.inventarnummer Inner Join lehrveranstaltungstermin On lehrveranstaltungstermin_has_raum.idLehrveranstaltungstermin = lehrveranstaltungstermin.idLehrveranstaltungstermin And lehrveranstaltungstermin_has_raum.idLehrveranstaltung = lehrveranstaltungstermin.idLehrveranstaltung Inner Join lehrveranstaltung On lehrveranstaltungstermin.idLehrveranstaltung = lehrveranstaltung.idLehrveranstaltung WHERE anwesenheit.matrikelnummer = " + idStudent + " And anwesenheit.idLehrveranstaltungstermin = " + idLectureDate;
            MySqlCommand command = new MySqlCommand(sqlDelAtt, connection);
            command.ExecuteNonQuery();
        }

        /// <summary>
        /// This returns a table of RFID readers and their IP addresses.
        /// This is a static method and closes the db connection.
        /// </summary>
        /// <returns>Query result DataTable</returns>
        public static DataTable GetReader()
        {
            DataTable dtReader = new DataTable();
            String sqlRdr = "Select * From Lesegeraet";
            MySqlConnection connection = new MySqlConnection(dbConnectionString);
            connection.Open();
            MySqlDataAdapter dataAdapter = new MySqlDataAdapter();
            dataAdapter.SelectCommand = new MySqlCommand(sqlRdr, connection);
            dataAdapter.Fill(dtReader);
            connection.Close();
            return dtReader;
        }
    }

}