using System;
using System.Data;
using System.Windows.Forms;

namespace de.thm.fsi.atp
{
    /// <summary>
    /// This is a GUI controller.
    /// Updates UI.
    /// Reacts to user events.
    /// </summary>
    internal class GuiController
    {
        private static Form1 frm1;
        private static DataGridView dataGridView;
        private static DataTable gridTable;
        private static ListBox listBox;
        private static AtpBl atpBl;

        public GuiController(AtpBl iAtpBl)
        {
            atpBl = iAtpBl;
            frm1 = new Form1();
            dataGridView = frm1.dataGridAttendance;
            listBox = frm1.listBoxdemoOutput;
        }

        /// <summary>
        /// This starts Winform gui.
        /// </summary>
        public void StartGui()
        {
            frm1.ShowDialog();
        }

        /// <summary>
        /// This refreshs the data grid view.
        /// </summary>
        public static void Refresh()
        {
            atpBl.RefreshGrid();
        }

        /// <summary>
        /// This finds all absentees and shows them in a MessageBox.
        /// </summary>
        public static void Analyze()
        {
            DataTable dt = atpBl.FindAbsentees();
            if (dt != null)
            {
                if (dt.Rows.Count == 0)
                {
                    MessageBox.Show("Keine Abwesenheiten gefunden.", "Abwesenheit");
                }
                else
                {
                    System.Text.StringBuilder b = new System.Text.StringBuilder();
                    foreach (DataRow dr in dt.Rows)
                    {
                        b.Append(dr["strAbs"].ToString() + "\n");
                    }
                    MessageBox.Show(b.ToString(), "Abwesenheit");
                }
            }
        }

        /// <summary>
        /// Get and fill lecture dropdown.
        /// </summary>
        public void FillComboBox(DataTable lecturesTable)
        {
            foreach (DataRow row in lecturesTable.Rows)
            {
                string group = row["nameStudiengruppe"].ToString();
                string lecture = row["bezeichnung"].ToString();
                frm1.comboBoxLecture.Items.Add(group + " - " + lecture);
            }
        }

        /// <summary>
        /// This adds a some text to listbox.
        /// </summary>
        /// <param name="text">Text string</param>
        public void AddToListbox(string text)
        {
            // For demo purposes cross-thread operations are permitted for ListBox!
            ListBox.CheckForIllegalCrossThreadCalls = false;
            listBox.Items.Add(text);
            // Autoscroll listbox
            listBox.SelectedIndex = listBox.Items.Count - 1;
            listBox.SelectedIndex = -1;
        }

        /// <summary>
        /// This binds attendance table to DataGridView and sets some properties.
        /// </summary>
        /// <param name="iGridTable">DataTable to bind to DataGridView.</param>
        public void UpdateDgv(DataTable iGridTable)
        {
            gridTable = iGridTable;
            BindingSource bSource = new BindingSource
            {
                DataSource = gridTable
            };
            dataGridView.DataSource = bSource;
            dataGridView.Columns["idStudent"].Visible = false;
            dataGridView.Columns["Studierende"].ReadOnly = true; // editing globally enabled in dgv properties
        }

        /// <summary>
        /// Updates datagrid if message box result is NO.
        /// </summary>
        public static void NoUpdateCell()
        {
            BindingSource bSource = new BindingSource
            {
                DataSource = gridTable
            };
            dataGridView.DataSource = bSource;
            dataGridView.Columns["idStudent"].Visible = false;
            dataGridView.Columns["Studierende"].ReadOnly = true; // editing globally enabled in dgv properties
        }

        /// <summary>
        /// Sets title of selected lecture.
        /// </summary>
        /// <param name="nameSpecialty">Fachrichtung</param>
        /// <param name="nameCourse">Studiengang</param>
        /// <param name="nameLecture">Lehrveranstaltung</param>
        public void SetTitle(string nameSpecialty, string nameCourse, string nameLecture)
        {
            if (string.IsNullOrEmpty(nameSpecialty))
            {
                frm1.lblTitle.Text = nameCourse + " – " + nameLecture;
            }
            else
            {
                frm1.lblTitle.Text = nameCourse + " – " + nameSpecialty + " – " + nameLecture;
            }
        }

        /// <summary>
        /// This reacts to click on datagrid cells.
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Cell data</param>
        public static void ClickOnDataGrid(object sender, DataGridViewCellEventArgs e)
        {
            //do nothing if header is clicked
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                // prevent action on column "Studierende"
                if (e.ColumnIndex > 1)
                {
                    string strValue = dataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString();
                    if (string.IsNullOrEmpty(strValue))
                    {
                        strValue = "False";
                    }

                    // show confirmation popup
                    string question = "Anwesenheit ändern für:" + "\n" + dataGridView.Rows[e.RowIndex].Cells[1].Value.ToString();
                    DialogResult dr = MessageBox.Show(question, "Änderung der Anwesenheit",
                                        MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                                        MessageBoxDefaultButton.Button1);
                    if (dr == DialogResult.Yes)
                    {
                        atpBl.UpdateCell(e.RowIndex, e.ColumnIndex, bool.Parse(strValue));
                    }
                    else if (dr == DialogResult.No)
                    {
                        NoUpdateCell();
                    }
                }
            }
        }

        /// <summary>
        /// Sets lecture according to dropdown menu selection.
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Selection Data</param>
        public static void DropdownSelect(object sender, EventArgs e)
        {
            object selectedItem = frm1.comboBoxLecture.SelectedItem;
            atpBl.SetLecture(frm1.comboBoxLecture.SelectedIndex);
        }
    }
}

