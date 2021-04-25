namespace AstroModLoader
{
    partial class AboutPopup
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
            this.closeButton = new AstroModLoader.CoolButton();
            this.licenseButton = new AstroModLoader.CoolButton();
            this.thirdPartyButton = new AstroModLoader.CoolButton();
            this.SuspendLayout();
            // 
            // closeButton
            // 
            this.closeButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.closeButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(51)))), ((int)(((byte)(51)))));
            this.closeButton.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(231)))), ((int)(((byte)(149)))));
            this.closeButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.closeButton.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.closeButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(225)))), ((int)(((byte)(225)))), ((int)(((byte)(225)))));
            this.closeButton.Location = new System.Drawing.Point(12, 226);
            this.closeButton.MinimumSize = new System.Drawing.Size(0, 26);
            this.closeButton.Name = "closeButton";
            this.closeButton.Size = new System.Drawing.Size(75, 26);
            this.closeButton.TabIndex = 0;
            this.closeButton.Text = "Close";
            this.closeButton.UseVisualStyleBackColor = false;
            this.closeButton.Click += new System.EventHandler(this.closeButton_Click);
            // 
            // licenseButton
            // 
            this.licenseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.licenseButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(51)))), ((int)(((byte)(51)))));
            this.licenseButton.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(231)))), ((int)(((byte)(149)))));
            this.licenseButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.licenseButton.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.licenseButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(225)))), ((int)(((byte)(225)))), ((int)(((byte)(225)))));
            this.licenseButton.Location = new System.Drawing.Point(271, 226);
            this.licenseButton.MinimumSize = new System.Drawing.Size(0, 26);
            this.licenseButton.Name = "licenseButton";
            this.licenseButton.Size = new System.Drawing.Size(92, 26);
            this.licenseButton.TabIndex = 2;
            this.licenseButton.Text = "View license";
            this.licenseButton.UseVisualStyleBackColor = false;
            this.licenseButton.Click += new System.EventHandler(this.licenseButton_Click);
            // 
            // thirdPartyButton
            // 
            this.thirdPartyButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.thirdPartyButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(51)))), ((int)(((byte)(51)))));
            this.thirdPartyButton.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(231)))), ((int)(((byte)(149)))));
            this.thirdPartyButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.thirdPartyButton.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.thirdPartyButton.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(225)))), ((int)(((byte)(225)))), ((int)(((byte)(225)))));
            this.thirdPartyButton.Location = new System.Drawing.Point(109, 226);
            this.thirdPartyButton.MinimumSize = new System.Drawing.Size(0, 26);
            this.thirdPartyButton.Name = "thirdPartyButton";
            this.thirdPartyButton.Size = new System.Drawing.Size(156, 26);
            this.thirdPartyButton.TabIndex = 1;
            this.thirdPartyButton.Text = "View 3rd-party software";
            this.thirdPartyButton.UseVisualStyleBackColor = false;
            this.thirdPartyButton.Click += new System.EventHandler(this.thirdPartyButton_Click);
            // 
            // AboutPopup
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(42)))), ((int)(((byte)(45)))));
            this.ClientSize = new System.Drawing.Size(372, 264);
            this.Controls.Add(this.thirdPartyButton);
            this.Controls.Add(this.licenseButton);
            this.Controls.Add(this.closeButton);
            this.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(225)))), ((int)(((byte)(225)))), ((int)(((byte)(225)))));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "AboutPopup";
            this.Text = "About";
            this.ResumeLayout(false);

        }

        #endregion

        private CoolButton closeButton;
        private CoolButton licenseButton;
        private CoolButton thirdPartyButton;
    }
}