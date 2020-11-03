namespace AstroModLoader
{
    partial class SettingsForm
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
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.gamePathBox = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.setPathButton = new AstroModLoader.CoolButton();
            this.exitButton = new AstroModLoader.CoolButton();
            this.accentComboBox = new System.Windows.Forms.ComboBox();
            this.themeComboBox = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.label1.Location = new System.Drawing.Point(13, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(342, 21);
            this.label1.TabIndex = 0;
            this.label1.Text = "Settings:";
            this.label1.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.label2.Location = new System.Drawing.Point(13, 45);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(134, 15);
            this.label2.TabIndex = 1;
            this.label2.Text = "Game Installation Path:";
            // 
            // gamePathBox
            // 
            this.gamePathBox.Location = new System.Drawing.Point(153, 44);
            this.gamePathBox.Name = "gamePathBox";
            this.gamePathBox.Size = new System.Drawing.Size(202, 20);
            this.gamePathBox.TabIndex = 2;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.label3.Location = new System.Drawing.Point(69, 98);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(78, 15);
            this.label3.TabIndex = 6;
            this.label3.Text = "Accent Color:";
            // 
            // setPathButton
            // 
            this.setPathButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(51)))), ((int)(((byte)(51)))));
            this.setPathButton.FlatAppearance.BorderColor = System.Drawing.Color.Black;
            this.setPathButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.setPathButton.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.setPathButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(225)))), ((int)(((byte)(225)))), ((int)(((byte)(225)))));
            this.setPathButton.Location = new System.Drawing.Point(361, 41);
            this.setPathButton.MinimumSize = new System.Drawing.Size(0, 26);
            this.setPathButton.Name = "setPathButton";
            this.setPathButton.Size = new System.Drawing.Size(40, 26);
            this.setPathButton.TabIndex = 3;
            this.setPathButton.Text = "Set";
            this.setPathButton.UseVisualStyleBackColor = false;
            this.setPathButton.Click += new System.EventHandler(this.setPathButton_Click);
            // 
            // exitButton
            // 
            this.exitButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.exitButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(51)))), ((int)(((byte)(51)))));
            this.exitButton.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(231)))), ((int)(((byte)(149)))));
            this.exitButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.exitButton.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.exitButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(225)))), ((int)(((byte)(225)))), ((int)(((byte)(225)))));
            this.exitButton.Location = new System.Drawing.Point(16, 144);
            this.exitButton.MinimumSize = new System.Drawing.Size(0, 26);
            this.exitButton.Name = "exitButton";
            this.exitButton.Size = new System.Drawing.Size(75, 26);
            this.exitButton.TabIndex = 8;
            this.exitButton.Text = "Close";
            this.exitButton.UseVisualStyleBackColor = false;
            this.exitButton.Click += new System.EventHandler(this.exitButton_Click);
            // 
            // accentComboBox
            // 
            this.accentComboBox.FormattingEnabled = true;
            this.accentComboBox.Location = new System.Drawing.Point(153, 97);
            this.accentComboBox.Name = "accentComboBox";
            this.accentComboBox.Size = new System.Drawing.Size(151, 21);
            this.accentComboBox.TabIndex = 7;
            this.accentComboBox.SelectedIndexChanged += new System.EventHandler(this.accentComboBox_UpdateColor);
            this.accentComboBox.Leave += new System.EventHandler(this.accentComboBox_UpdateColor);
            // 
            // themeComboBox
            // 
            this.themeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.themeComboBox.FormattingEnabled = true;
            this.themeComboBox.Location = new System.Drawing.Point(153, 70);
            this.themeComboBox.Name = "themeComboBox";
            this.themeComboBox.Size = new System.Drawing.Size(151, 21);
            this.themeComboBox.TabIndex = 5;
            this.themeComboBox.SelectedIndexChanged += new System.EventHandler(this.themeBox_SelectedIndexChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.label4.Location = new System.Drawing.Point(98, 71);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(49, 15);
            this.label4.TabIndex = 4;
            this.label4.Text = "Theme:";
            // 
            // SettingsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(42)))), ((int)(((byte)(45)))));
            this.ClientSize = new System.Drawing.Size(413, 182);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.themeComboBox);
            this.Controls.Add(this.accentComboBox);
            this.Controls.Add(this.exitButton);
            this.Controls.Add(this.setPathButton);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.gamePathBox);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(225)))), ((int)(((byte)(225)))), ((int)(((byte)(225)))));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "SettingsForm";
            this.Text = "Settings";
            this.Load += new System.EventHandler(this.SettingsForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox gamePathBox;
        private System.Windows.Forms.Label label3;
        private CoolButton setPathButton;
        private CoolButton exitButton;
        private System.Windows.Forms.ComboBox accentComboBox;
        private System.Windows.Forms.ComboBox themeComboBox;
        private System.Windows.Forms.Label label4;
    }
}