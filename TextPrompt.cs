using Ookii.Dialogs.WinForms;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace AstroModLoader
{
    public enum BrowseMode
    {
        File,
        Folder
    }


    public partial class TextPrompt : Form
    {
        public string DisplayText;
        public string OutputText;
        public bool AllowBrowse = true;
        public BrowseMode BrowseMode = BrowseMode.Folder;

        public TextPrompt()
        {
            InitializeComponent();
        }

        private void InitialPathPrompt_Load(object sender, EventArgs e)
        {
            mainLabel.Text = DisplayText;
            if (this.Owner is Form1 parentForm)
            {
                this.Text = parentForm.Text;
                AMLPalette.RefreshTheme(parentForm);
            }
            AMLPalette.RefreshTheme(this);
            if (!AllowBrowse)
            {
                browseButton.Hide();
                gamePathBox.Size = new Size(this.ClientSize.Width - 24, gamePathBox.ClientSize.Height);
            }
        }

        private void browseButton_Click(object sender, EventArgs e)
        {
            if (VistaFolderBrowserDialog.IsVistaFolderDialogSupported)
            {
                if (BrowseMode == BrowseMode.File)
                {
                    var dialog = new VistaOpenFileDialog();
                    dialog.Title = DisplayText;
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        gamePathBox.Text = dialog.FileName;
                    }
                }
                else
                {
                    var dialog = new VistaFolderBrowserDialog();
                    dialog.Description = DisplayText;
                    dialog.UseDescriptionForTitle = true;
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        gamePathBox.Text = dialog.SelectedPath;
                    }
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
            if (gamePathBox.Text != null && gamePathBox.Text.Length > 0)
            {
                OutputText = gamePathBox.Text;
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            OutputText = null;
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
