namespace AstroModLoader
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
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.label1 = new System.Windows.Forms.Label();
            this.modInfo = new System.Windows.Forms.LinkLabel();
            this.tablePanel = new System.Windows.Forms.Panel();
            this.footerPanel = new System.Windows.Forms.Panel();
            this.modPanel = new System.Windows.Forms.Panel();
            this.PeriodicCheckTimer = new System.Windows.Forms.Timer(this.components);
            this.refresh = new AstroModLoader.CoolButton();
            this.loadButton = new AstroModLoader.CoolButton();
            this.syncButton = new AstroModLoader.CoolButton();
            this.exitButton = new AstroModLoader.CoolButton();
            this.settingsButton = new AstroModLoader.CoolButton();
            this.playButton = new AstroModLoader.CoolButton();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.tablePanel.SuspendLayout();
            this.footerPanel.SuspendLayout();
            this.modPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // dataGridView1
            // 
            this.dataGridView1.AllowDrop = true;
            this.dataGridView1.AllowUserToAddRows = false;
            this.dataGridView1.AllowUserToDeleteRows = false;
            this.dataGridView1.AllowUserToResizeColumns = false;
            this.dataGridView1.AllowUserToResizeRows = false;
            this.dataGridView1.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.dataGridView1.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dataGridView1.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(42)))), ((int)(((byte)(45)))));
            this.dataGridView1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dataGridView1.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.None;
            this.dataGridView1.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(42)))), ((int)(((byte)(45)))));
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(225)))), ((int)(((byte)(225)))), ((int)(((byte)(225)))));
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(85)))), ((int)(((byte)(85)))), ((int)(((byte)(85)))));
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGridView1.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.DefaultCellStyle = this.dataGridView1.ColumnHeadersDefaultCellStyle;
            this.dataGridView1.EditMode = System.Windows.Forms.DataGridViewEditMode.EditOnEnter;
            this.dataGridView1.EnableHeadersVisualStyles = false;
            this.dataGridView1.Location = new System.Drawing.Point(1, 1);
            this.dataGridView1.MultiSelect = false;
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.RowHeadersVisible = false;
            this.dataGridView1.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridView1.Size = new System.Drawing.Size(432, 225);
            this.dataGridView1.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.label1.Location = new System.Drawing.Point(21, 0);
            this.label1.Margin = new System.Windows.Forms.Padding(0);
            this.label1.Name = "label1";
            this.label1.Padding = new System.Windows.Forms.Padding(10, 10, 3, 3);
            this.label1.Size = new System.Drawing.Size(416, 30);
            this.label1.TabIndex = 1;
            this.label1.Text = "Mods:";
            this.label1.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // modInfo
            // 
            this.modInfo.ActiveLinkColor = System.Drawing.Color.Red;
            this.modInfo.AutoSize = true;
            this.modInfo.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.5F);
            this.modInfo.Location = new System.Drawing.Point(17, 323);
            this.modInfo.MaximumSize = new System.Drawing.Size(400, 0);
            this.modInfo.Name = "modInfo";
            this.modInfo.Size = new System.Drawing.Size(0, 17);
            this.modInfo.TabIndex = 3;
            this.modInfo.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.modInfo_LinkClicked);
            // 
            // tablePanel
            // 
            this.tablePanel.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.tablePanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tablePanel.BackColor = System.Drawing.Color.Black;
            this.tablePanel.Controls.Add(this.dataGridView1);
            this.tablePanel.Location = new System.Drawing.Point(12, 4);
            this.tablePanel.Name = "tablePanel";
            this.tablePanel.Padding = new System.Windows.Forms.Padding(1);
            this.tablePanel.Size = new System.Drawing.Size(434, 227);
            this.tablePanel.TabIndex = 4;
            // 
            // footerPanel
            // 
            this.footerPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.footerPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.footerPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(36)))), ((int)(((byte)(38)))), ((int)(((byte)(40)))));
            this.footerPanel.Controls.Add(this.exitButton);
            this.footerPanel.Controls.Add(this.settingsButton);
            this.footerPanel.Controls.Add(this.playButton);
            this.footerPanel.Location = new System.Drawing.Point(0, 411);
            this.footerPanel.Name = "footerPanel";
            this.footerPanel.Size = new System.Drawing.Size(458, 50);
            this.footerPanel.TabIndex = 5;
            // 
            // modPanel
            // 
            this.modPanel.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.modPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.modPanel.BackColor = System.Drawing.Color.Transparent;
            this.modPanel.Controls.Add(this.tablePanel);
            this.modPanel.Controls.Add(this.refresh);
            this.modPanel.Controls.Add(this.loadButton);
            this.modPanel.Controls.Add(this.syncButton);
            this.modPanel.Location = new System.Drawing.Point(0, 35);
            this.modPanel.Name = "modPanel";
            this.modPanel.Padding = new System.Windows.Forms.Padding(1);
            this.modPanel.Size = new System.Drawing.Size(460, 270);
            this.modPanel.TabIndex = 1;
            // 
            // PeriodicCheckTimer
            // 
            this.PeriodicCheckTimer.Interval = 8000;
            this.PeriodicCheckTimer.Tick += new System.EventHandler(this.PeriodicCheckTimer_Tick);
            // 
            // refresh
            // 
            this.refresh.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.refresh.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(51)))), ((int)(((byte)(51)))));
            this.refresh.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.refresh.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.refresh.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(225)))), ((int)(((byte)(225)))), ((int)(((byte)(225)))));
            this.refresh.Location = new System.Drawing.Point(12, 236);
            this.refresh.MinimumSize = new System.Drawing.Size(0, 26);
            this.refresh.Name = "refresh";
            this.refresh.Size = new System.Drawing.Size(75, 26);
            this.refresh.TabIndex = 1;
            this.refresh.Text = "Refresh";
            this.refresh.UseVisualStyleBackColor = false;
            this.refresh.Click += new System.EventHandler(this.refresh_Click);
            // 
            // loadButton
            // 
            this.loadButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(51)))), ((int)(((byte)(51)))));
            this.loadButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.loadButton.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.loadButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(225)))), ((int)(((byte)(225)))), ((int)(((byte)(225)))));
            this.loadButton.Location = new System.Drawing.Point(92, 236);
            this.loadButton.MinimumSize = new System.Drawing.Size(0, 26);
            this.loadButton.Name = "loadButton";
            this.loadButton.Size = new System.Drawing.Size(75, 26);
            this.loadButton.TabIndex = 5;
            this.loadButton.Text = "Profiles...";
            this.loadButton.UseVisualStyleBackColor = false;
            this.loadButton.Click += new System.EventHandler(this.loadButton_Click);
            // 
            // syncButton
            // 
            this.syncButton.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.syncButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(51)))), ((int)(((byte)(51)))));
            this.syncButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.syncButton.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.syncButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(225)))), ((int)(((byte)(225)))), ((int)(((byte)(225)))));
            this.syncButton.Location = new System.Drawing.Point(337, 236);
            this.syncButton.MinimumSize = new System.Drawing.Size(0, 26);
            this.syncButton.Name = "syncButton";
            this.syncButton.Size = new System.Drawing.Size(109, 26);
            this.syncButton.TabIndex = 2;
            this.syncButton.Text = "Sync from IP";
            this.syncButton.UseVisualStyleBackColor = false;
            this.syncButton.Click += new System.EventHandler(this.syncButton_Click);
            // 
            // exitButton
            // 
            this.exitButton.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.exitButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(51)))), ((int)(((byte)(51)))));
            this.exitButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.exitButton.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.exitButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(225)))), ((int)(((byte)(225)))), ((int)(((byte)(225)))));
            this.exitButton.Location = new System.Drawing.Point(371, 12);
            this.exitButton.MinimumSize = new System.Drawing.Size(0, 26);
            this.exitButton.Name = "exitButton";
            this.exitButton.Size = new System.Drawing.Size(75, 26);
            this.exitButton.TabIndex = 7;
            this.exitButton.Text = "Exit";
            this.exitButton.UseVisualStyleBackColor = false;
            this.exitButton.Click += new System.EventHandler(this.exitButton_Click);
            // 
            // settingsButton
            // 
            this.settingsButton.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.settingsButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(51)))), ((int)(((byte)(51)))));
            this.settingsButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.settingsButton.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.settingsButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(225)))), ((int)(((byte)(225)))), ((int)(((byte)(225)))));
            this.settingsButton.Location = new System.Drawing.Point(92, 12);
            this.settingsButton.MinimumSize = new System.Drawing.Size(0, 26);
            this.settingsButton.Name = "settingsButton";
            this.settingsButton.Size = new System.Drawing.Size(82, 26);
            this.settingsButton.TabIndex = 6;
            this.settingsButton.Text = "Settings...";
            this.settingsButton.UseVisualStyleBackColor = false;
            this.settingsButton.Click += new System.EventHandler(this.settingsButton_Click);
            // 
            // playButton
            // 
            this.playButton.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.playButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(51)))), ((int)(((byte)(51)))));
            this.playButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.playButton.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.playButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(225)))), ((int)(((byte)(225)))), ((int)(((byte)(225)))));
            this.playButton.Location = new System.Drawing.Point(12, 12);
            this.playButton.MinimumSize = new System.Drawing.Size(0, 26);
            this.playButton.Name = "playButton";
            this.playButton.Size = new System.Drawing.Size(75, 26);
            this.playButton.TabIndex = 3;
            this.playButton.Text = "Play";
            this.playButton.UseVisualStyleBackColor = false;
            this.playButton.Click += new System.EventHandler(this.playButton_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(42)))), ((int)(((byte)(45)))));
            this.ClientSize = new System.Drawing.Size(459, 461);
            this.Controls.Add(this.modPanel);
            this.Controls.Add(this.footerPanel);
            this.Controls.Add(this.modInfo);
            this.Controls.Add(this.label1);
            this.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(225)))), ((int)(((byte)(225)))), ((int)(((byte)(225)))));
            this.MinimumSize = new System.Drawing.Size(475, 500);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.tablePanel.ResumeLayout(false);
            this.footerPanel.ResumeLayout(false);
            this.modPanel.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private AstroModLoader.CoolButton refresh;
        private AstroModLoader.CoolButton playButton;
        private AstroModLoader.CoolButton loadButton;
        private AstroModLoader.CoolButton syncButton;
        private CoolButton settingsButton;
        private CoolButton exitButton;
        private System.Windows.Forms.Panel modPanel;
        private System.Windows.Forms.Timer PeriodicCheckTimer;
        public System.Windows.Forms.LinkLabel modInfo;
    }
}

