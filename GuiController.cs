using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        public GuiController()
        {
            frm1 = Program.frm1;
            dataGridView = frm1.dataGridView2;
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
                frm1.comboBox1.Items.Add(group + " - " + lecture);
            }
        }

        public void UpdateDgv(DataTable iGridTable)
        {
            gridTable = iGridTable;
            BindingSource bSource = new BindingSource();
            bSource.DataSource = gridTable;
            dataGridView.DataSource = bSource;
            dataGridView.Columns["idStudent"].Visible = false;
            dataGridView.Columns["Studierende"].ReadOnly = true; // editing globally enabled in dgv properties
        }

        public static void NoUpdateCell()
        {
            BindingSource bSource = new BindingSource();
            bSource.DataSource = gridTable;
            dataGridView.DataSource = bSource;
            dataGridView.Columns["idStudent"].Visible = false;
            dataGridView.Columns["Studierende"].ReadOnly = true; // editing globally enabled in dgv properties
        }

        public void SetTitle(string nameSpecialty, string nameCourse, string nameLecture)
        {
            if (String.IsNullOrEmpty(nameSpecialty))
            {
                frm1.label2.Text = nameCourse + " – " + nameLecture;
            }
            else
            {
                frm1.label2.Text = nameCourse + " – " + nameSpecialty + " – " + nameLecture;
            }
        }

        public static void ClickOnDataGrid(object sender, DataGridViewCellEventArgs e)
        {
            //do nothing if header is clicked
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                // prevent action on column "Studierende"
                if (e.ColumnIndex > 1)
                {
                    //show bool value in popup
                    //string titlePopup = "Value (r" + (e.RowIndex + 1).ToString() + ",c" + e.ColumnIndex.ToString() + ")";
                    string strValue = dataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString();

                    if (String.IsNullOrEmpty(strValue))
                    {
                        strValue = "False";
                    }
                    //MessageBox.Show(strValue, titlePopup);
                    //MessageBox.Show(dataGridView2.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString(), "Bool value");

                    // show confirmation popup
                    string question = "Anwesenheit ändern für:" + "\n" + dataGridView.Rows[e.RowIndex].Cells[1].Value.ToString();
                    //DialogResult dr = MessageBox.Show("Anwesenheit ändern?", "Änderung der Anwesenheit",
                    DialogResult dr = MessageBox.Show(question, "Änderung der Anwesenheit",
                                        MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                                        MessageBoxDefaultButton.Button1);
                    if (dr == DialogResult.Yes)
                    {
                        //UpdateText("Yes");
                        AtpBl.UpdateCell(e.RowIndex, e.ColumnIndex, bool.Parse(strValue));
                    }
                    else if (dr == DialogResult.No)
                    {
                        //UpdateText("No");
                        NoUpdateCell();
                    }
                }
            }
        }
    }
}

