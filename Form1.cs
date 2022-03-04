using System;
using System.Windows.Forms;

namespace de.thm.fsi.atp
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Event handling load form.
        /// </summary>
        private void Form1_Load(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// Event handling close button clicked.
        /// </summary>
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            GuiController.CloseApplication();
        }

        /// <summary>
        /// Event handling cell in datagridview clicked.
        /// </summary>
        private void dataGridView2_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            GuiController.ClickOnDataGrid(sender, e);
        }

        /// <summary>
        /// Event handling item in combobox/ drop-down list selected.
        /// </summary>
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            GuiController.DropdownSelect(sender, e);
        }

        /// <summary>
        /// Event handling refresh button clicked.
        /// </summary>
        private void btnRefresh_Click(object sender, EventArgs e)
        {
            GuiController.Refresh();
        }

        /// <summary>
        /// Event handling analyze button clicked.
        /// </summary>
        private void btnAnalyze_Click(object sender, EventArgs e)
        {
            GuiController.Analyze();
        }
    }
}
