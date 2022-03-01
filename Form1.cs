﻿using System;
using System.Windows.Forms;

namespace de.thm.fsi.atp
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void dataGridView2_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            GuiController.ClickOnDataGrid(sender, e);
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            GuiController.DropdownSelect(sender, e);
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            GuiController.Refresh();
        }

        private void btnAnalyze_Click(object sender, EventArgs e)
        {
            GuiController.Analyze();
        }
    }
}
