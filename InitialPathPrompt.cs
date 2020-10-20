using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AstroModLoader
{
    public partial class InitialPathPrompt : Form
    {
        public string DisplayText;
        public string OutputPath;

        public InitialPathPrompt()
        {
            InitializeComponent();
        }

        private void InitialPathPrompt_Load(object sender, EventArgs e)
        {
            if (this.Owner is Form1 parentForm)
            {
                this.Text = parentForm.Text;
                AMLPalette.RefreshTheme(parentForm);
            }
            AMLPalette.RefreshTheme(this);
        }

        private void browseButton_Click(object sender, EventArgs e)
        {
            if (CommonFileDialog.IsPlatformSupported)
            {
                var dialog = new CommonOpenFileDialog();
                dialog.Title = DisplayText;
                dialog.IsFolderPicker = true;
                if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    gamePathBox.Text = dialog.FileName;
                }
            }
            else
            {
                FolderBrowserDialog folderBrowserDialog1 = new FolderBrowserDialog();
                if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
                {
                    gamePathBox.Text = folderBrowserDialog1.SelectedPath;
                }
            }
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            OutputPath = gamePathBox.Text;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            OutputPath = null;
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
