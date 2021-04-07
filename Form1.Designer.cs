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
            this.headerLabel = new System.Windows.Forms.Label();
            this.modInfo = new System.Windows.Forms.LinkLabel();
            this.footerPanel = new System.Windows.Forms.Panel();
            this.exitButton = new AstroModLoader.CoolButton();
            this.settingsButton = new AstroModLoader.CoolButton();
            this.playButton = new AstroModLoader.CoolButton();
            this.modPanel = new System.Windows.Forms.Panel();
            this.dataGridView1 = new AstroModLoader.CoolDataGridView();
            this.refresh = new AstroModLoader.CoolButton();
            this.loadButton = new AstroModLoader.CoolButton();
            this.syncButton = new AstroModLoader.CoolButton();
            this.PeriodicCheckTimer = new System.Windows.Forms.Timer(this.components);
            this.CheckAllDirty = new System.Windows.Forms.Timer(this.components);
            this.footerPanel.SuspendLayout();
            this.modPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.SuspendLayout();
            // 
            // headerLabel
            // 
            this.headerLabel.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.headerLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.headerLabel.Location = new System.Drawing.Point(26, 0);
            this.headerLabel.Margin = new System.Windows.Forms.Padding(0);
            this.headerLabel.Name = "headerLabel";
            this.headerLabel.Padding = new System.Windows.Forms.Padding(10, 10, 3, 3);
            this.headerLabel.Size = new System.Drawing.Size(416, 30);
            this.headerLabel.TabIndex = 0;
            this.headerLabel.Text = "Mods:";
            this.headerLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // modInfo
            // 
            this.modInfo.ActiveLinkColor = System.Drawing.Color.Red;
            this.modInfo.AutoSize = true;
            this.modInfo.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.5F);
            this.modInfo.LinkArea = new System.Windows.Forms.LinkArea(0, 0);
            this.modInfo.Location = new System.Drawing.Point(12, 320);
            this.modInfo.MaximumSize = new System.Drawing.Size(400, 0);
            this.modInfo.Name = "modInfo";
            this.modInfo.Size = new System.Drawing.Size(83, 17);
            this.modInfo.TabIndex = 3;
            this.modInfo.Text = "Testing 123";
            this.modInfo.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.modInfo_LinkClicked);
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
            this.footerPanel.Location = new System.Drawing.Point(0, 441);
            this.footerPanel.Name = "footerPanel";
            this.footerPanel.Size = new System.Drawing.Size(468, 50);
            this.footerPanel.TabIndex = 5;
            // 
            // exitButton
            // 
            this.exitButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.exitButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(51)))), ((int)(((byte)(51)))));
            this.exitButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.exitButton.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.exitButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(225)))), ((int)(((byte)(225)))), ((int)(((byte)(225)))));
            this.exitButton.Location = new System.Drawing.Point(381, 12);
            this.exitButton.MinimumSize = new System.Drawing.Size(0, 26);
            this.exitButton.Name = "exitButton";
            this.exitButton.Size = new System.Drawing.Size(75, 26);
            this.exitButton.TabIndex = 6;
            this.exitButton.Text = "Exit";
            this.exitButton.UseVisualStyleBackColor = false;
            this.exitButton.Click += new System.EventHandler(this.exitButton_Click);
            // 
            // settingsButton
            // 
            this.settingsButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.settingsButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(51)))), ((int)(((byte)(51)))));
            this.settingsButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.settingsButton.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.settingsButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(225)))), ((int)(((byte)(225)))), ((int)(((byte)(225)))));
            this.settingsButton.Location = new System.Drawing.Point(92, 12);
            this.settingsButton.MinimumSize = new System.Drawing.Size(0, 26);
            this.settingsButton.Name = "settingsButton";
            this.settingsButton.Size = new System.Drawing.Size(82, 26);
            this.settingsButton.TabIndex = 5;
            this.settingsButton.Text = "Settings...";
            this.settingsButton.UseVisualStyleBackColor = false;
            this.settingsButton.Click += new System.EventHandler(this.settingsButton_Click);
            // 
            // playButton
            // 
            this.playButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.playButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(51)))), ((int)(((byte)(51)))));
            this.playButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.playButton.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.playButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(225)))), ((int)(((byte)(225)))), ((int)(((byte)(225)))));
            this.playButton.Location = new System.Drawing.Point(12, 12);
            this.playButton.MinimumSize = new System.Drawing.Size(0, 26);
            this.playButton.Name = "playButton";
            this.playButton.Size = new System.Drawing.Size(75, 26);
            this.playButton.TabIndex = 4;
            this.playButton.Text = "Play";
            this.playButton.UseVisualStyleBackColor = false;
            this.playButton.Click += new System.EventHandler(this.playButton_Click);
            // 
            // modPanel
            // 
            this.modPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.modPanel.BackColor = System.Drawing.Color.Transparent;
            this.modPanel.Controls.Add(this.dataGridView1);
            this.modPanel.Controls.Add(this.refresh);
            this.modPanel.Controls.Add(this.loadButton);
            this.modPanel.Controls.Add(this.syncButton);
            this.modPanel.Location = new System.Drawing.Point(0, 35);
            this.modPanel.Name = "modPanel";
            this.modPanel.Padding = new System.Windows.Forms.Padding(1);
            this.modPanel.Size = new System.Drawing.Size(460, 270);
            this.modPanel.TabIndex = 1;
            // 
            // dataGridView1
            // 
            this.dataGridView1.AllowDrop = true;
            this.dataGridView1.AllowUserToAddRows = false;
            this.dataGridView1.AllowUserToDeleteRows = false;
            this.dataGridView1.AllowUserToResizeColumns = false;
            this.dataGridView1.AllowUserToResizeRows = false;
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
            this.dataGridView1.Location = new System.Drawing.Point(12, 0);
            this.dataGridView1.MultiSelect = false;
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.RowHeadersVisible = false;
            this.dataGridView1.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridView1.Size = new System.Drawing.Size(435, 225);
            this.dataGridView1.TabIndex = 0;
            this.dataGridView1.CellPainting += new System.Windows.Forms.DataGridViewCellPaintingEventHandler(this.dataGridView1_CellPainting);
            this.dataGridView1.KeyDown += new System.Windows.Forms.KeyEventHandler(this.dataGridView1_KeyDown);
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
            this.loadButton.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.loadButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(51)))), ((int)(((byte)(51)))));
            this.loadButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.loadButton.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.loadButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(225)))), ((int)(((byte)(225)))), ((int)(((byte)(225)))));
            this.loadButton.Location = new System.Drawing.Point(92, 236);
            this.loadButton.MinimumSize = new System.Drawing.Size(0, 26);
            this.loadButton.Name = "loadButton";
            this.loadButton.Size = new System.Drawing.Size(75, 26);
            this.loadButton.TabIndex = 2;
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
            this.syncButton.Location = new System.Drawing.Point(338, 236);
            this.syncButton.MinimumSize = new System.Drawing.Size(0, 26);
            this.syncButton.Name = "syncButton";
            this.syncButton.Size = new System.Drawing.Size(109, 26);
            this.syncButton.TabIndex = 3;
            this.syncButton.Text = "Sync from IP";
            this.syncButton.UseVisualStyleBackColor = false;
            this.syncButton.Click += new System.EventHandler(this.syncButton_Click);
            // 
            // PeriodicCheckTimer
            // 
            this.PeriodicCheckTimer.Interval = 8000;
            this.PeriodicCheckTimer.Tick += new System.EventHandler(this.PeriodicCheckTimer_Tick);
            // 
            // CheckAllDirty
            // 
            this.CheckAllDirty.Interval = 1500;
            this.CheckAllDirty.Tick += new System.EventHandler(this.CheckAllDirty_Tick);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(42)))), ((int)(((byte)(45)))));
            this.ClientSize = new System.Drawing.Size(469, 491);
            this.Controls.Add(this.modPanel);
            this.Controls.Add(this.footerPanel);
            this.Controls.Add(this.modInfo);
            this.Controls.Add(this.headerLabel);
            this.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(225)))), ((int)(((byte)(225)))), ((int)(((byte)(225)))));
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(900, 700);
            this.MinimumSize = new System.Drawing.Size(485, 515);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.Resize += new System.EventHandler(this.Form1_SizeChanged);
            this.footerPanel.ResumeLayout(false);
            this.modPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label headerLabel;
        private AstroModLoader.CoolButton refresh;
        private AstroModLoader.CoolButton playButton;
        private AstroModLoader.CoolButton loadButton;
        private AstroModLoader.CoolButton syncButton;
        private CoolButton settingsButton;
        private CoolButton exitButton;
        private System.Windows.Forms.Panel modPanel;
        private System.Windows.Forms.Timer PeriodicCheckTimer;
        private System.Windows.Forms.Timer CheckAllDirty;
        public System.Windows.Forms.LinkLabel modInfo;
    }
}

