using de.thm.fsi.atp.Properties;

namespace de.thm.fsi.atp
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.lblTitle = new System.Windows.Forms.Label();
            this.comboBoxLecture = new System.Windows.Forms.ComboBox();
            this.dataGridAttendance = new System.Windows.Forms.DataGridView();
            this.btnRefresh = new System.Windows.Forms.Button();
            this.listBoxdemoOutput = new System.Windows.Forms.ListBox();
            this.tableBindingAttendance = new System.Windows.Forms.BindingSource(this.components);
            this.btnAnalyze = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridAttendance)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tableBindingAttendance)).BeginInit();
            this.SuspendLayout();
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(243)))), ((int)(((byte)(245)))), ((int)(((byte)(247)))));
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 24F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(25)))), ((int)(((byte)(36)))), ((int)(((byte)(52)))));
            this.lblTitle.Location = new System.Drawing.Point(270, 70);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(0, 45);
            this.lblTitle.TabIndex = 17;
            // 
            // comboBoxLecture
            // 
            this.comboBoxLecture.Font = new System.Drawing.Font("Segoe UI Semibold", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.comboBoxLecture.FormattingEnabled = true;
            this.comboBoxLecture.Location = new System.Drawing.Point(277, 126);
            this.comboBoxLecture.Name = "comboBoxLecture";
            this.comboBoxLecture.Size = new System.Drawing.Size(507, 29);
            this.comboBoxLecture.TabIndex = 1;
            this.comboBoxLecture.Text = "Lehrveranstaltung wählen...";
            this.comboBoxLecture.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectedIndexChanged);
            // 
            // dataGridAttendance
            // 
            this.dataGridAttendance.AllowUserToAddRows = false;
            this.dataGridAttendance.AllowUserToDeleteRows = false;
            this.dataGridAttendance.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.dataGridAttendance.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
            this.dataGridAttendance.BackgroundColor = System.Drawing.SystemColors.ControlLightLight;
            this.dataGridAttendance.BorderStyle = System.Windows.Forms.BorderStyle.None;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.White;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridAttendance.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.dataGridAttendance.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGridAttendance.DefaultCellStyle = dataGridViewCellStyle2;
            this.dataGridAttendance.GridColor = System.Drawing.SystemColors.Desktop;
            this.dataGridAttendance.Location = new System.Drawing.Point(277, 161);
            this.dataGridAttendance.Name = "dataGridAttendance";
            this.dataGridAttendance.RowHeadersVisible = false;
            this.dataGridAttendance.RowTemplate.DefaultCellStyle.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridAttendance.Size = new System.Drawing.Size(1609, 561);
            this.dataGridAttendance.TabIndex = 2;
            this.dataGridAttendance.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView2_CellClick);
            // 
            // btnRefresh
            // 
            this.btnRefresh.Cursor = System.Windows.Forms.Cursors.Default;
            this.btnRefresh.FlatAppearance.BorderColor = System.Drawing.Color.Black;
            this.btnRefresh.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRefresh.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnRefresh.Location = new System.Drawing.Point(1561, 758);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(185, 37);
            this.btnRefresh.TabIndex = 3;
            this.btnRefresh.Text = "Aktualisieren";
            this.btnRefresh.UseVisualStyleBackColor = true;
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            // 
            // listBoxdemoOutput
            // 
            this.listBoxdemoOutput.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.listBoxdemoOutput.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.listBoxdemoOutput.FormattingEnabled = true;
            this.listBoxdemoOutput.ItemHeight = 21;
            this.listBoxdemoOutput.Location = new System.Drawing.Point(286, 758);
            this.listBoxdemoOutput.Name = "listBoxdemoOutput";
            this.listBoxdemoOutput.Size = new System.Drawing.Size(1239, 191);
            this.listBoxdemoOutput.TabIndex = 4;
            // 
            // btnAnalyze
            // 
            this.btnAnalyze.Cursor = System.Windows.Forms.Cursors.Default;
            this.btnAnalyze.FlatAppearance.BorderColor = System.Drawing.Color.Black;
            this.btnAnalyze.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAnalyze.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnAnalyze.Location = new System.Drawing.Point(1561, 815);
            this.btnAnalyze.Name = "btnAnalyze";
            this.btnAnalyze.Size = new System.Drawing.Size(185, 37);
            this.btnAnalyze.TabIndex = 18;
            this.btnAnalyze.Text = "Auswertung erstellen";
            this.btnAnalyze.UseVisualStyleBackColor = true;
            this.btnAnalyze.Click += new System.EventHandler(this.btnAnalyze_Click);
            // 
            // Form1
            // 
            this.AccessibleRole = System.Windows.Forms.AccessibleRole.None;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.BackgroundImage = global::de.thm.fsi.atp.Properties.Resources.OsPlusBG;
            this.ClientSize = new System.Drawing.Size(1880, 957);
            this.Controls.Add(this.btnAnalyze);
            this.Controls.Add(this.lblTitle);
            this.Controls.Add(this.comboBoxLecture);
            this.Controls.Add(this.dataGridAttendance);
            this.Controls.Add(this.btnRefresh);
            this.Controls.Add(this.listBoxdemoOutput);
            this.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.Text = "Attendance+";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridAttendance)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tableBindingAttendance)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        public System.Windows.Forms.Label lblTitle;
        public System.Windows.Forms.ComboBox comboBoxLecture;
        public System.Windows.Forms.DataGridView dataGridAttendance;
        private System.Windows.Forms.Button btnRefresh;
        public System.Windows.Forms.ListBox listBoxdemoOutput;
        private System.Windows.Forms.BindingSource tableBindingAttendance;
        private System.Windows.Forms.Button btnAnalyze;
    }
}

