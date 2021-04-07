using Ookii.Dialogs.WinForms;
using System;
using System.Drawing;
using System.IO;
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
            }
            AMLPalette.RefreshTheme(this);
            this.AdjustFormPosition();
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

        private void RunOKButton()
        {
            if (AllowBrowse && !AMLUtils.IsValidPath(gamePathBox.Text))
            {
                this.ShowBasicButton("This is not a valid path!", "OK", null, null);
                return;
            }

            if (gamePathBox.Text != null && gamePathBox.Text.Length > 0)
            {
                OutputText = gamePathBox.Text;
                if (AllowBrowse && OutputText[OutputText.Length - 1] == Path.DirectorySeparatorChar) OutputText = OutputText.Substring(0, OutputText.Length - 1);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            RunOKButton();
        }

        private void RunCancelButton()
        {
            OutputText = null;
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            RunCancelButton();
        }

        private void TextPrompt_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
            {
                RunOKButton();
            }
            else if (e.KeyCode == Keys.Escape)
            {
                RunCancelButton();
            }
        }
    }
}
