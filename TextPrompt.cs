using Ookii.Dialogs.WinForms;
using System;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace AstroModLoader
{
    public enum BrowseMode
    {
        File,
        Folder
    }

    public enum VerifyPathMode
    {
        None,
        Base,
        Game
    }

    public partial class TextPrompt : Form
    {
        public string DisplayText = null;
        public string OutputText = null;
        public string PrefilledText = null;
        public bool AllowBrowse = true;
        public BrowseMode BrowseMode = BrowseMode.Folder;
        public VerifyPathMode VerifyMode = VerifyPathMode.None;

        public TextPrompt()
        {
            InitializeComponent();
            ShowWarning("");
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

            int debugLabelEndX = this.browseButton.Location.X + this.browseButton.Width - 5;
            this.debugLabel.ForeColor = AMLPalette.WarningColor;
            this.debugLabel.Width = debugLabelEndX - (this.cancelButton.Location.X + this.cancelButton.Width);
            this.debugLabel.Location = new Point(debugLabelEndX - this.debugLabel.Width, this.cancelButton.Location.Y);

            if (!string.IsNullOrEmpty(PrefilledText))
            {
                gamePathBox.Text = PrefilledText;
                PerformPathSubstitutions();
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

        private void ShowWarning(string txt)
        {
            this.debugLabel.Text = txt;
        }

        private static Regex isLocalAppdata = new Regex(@"%localappdata%", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private void PerformPathSubstitutions()
        {
            gamePathBox.Text = isLocalAppdata.Replace(gamePathBox.Text, Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData));
        }

        private void RunOKButton()
        {
            PerformPathSubstitutions();

            if (AllowBrowse && !AMLUtils.IsValidPath(gamePathBox.Text))
            {
                ShowWarning("This is not a valid path!");
                return;
            }

            if (gamePathBox.Text != null && gamePathBox.Text.Length > 0)
            {
                var doneText = gamePathBox.Text;
                if (AllowBrowse && doneText[doneText.Length - 1] == Path.DirectorySeparatorChar) doneText = doneText.Substring(0, doneText.Length - 1);
                switch(VerifyMode)
                {
                    case VerifyPathMode.Base:
                        doneText = AMLUtils.FixBasePath(doneText);
                        if (string.IsNullOrEmpty(doneText))
                        {
                            ShowWarning("This is not the correct path!");
                            OutputText = null;
                            return;
                        }
                        break;
                    case VerifyPathMode.Game:
                        doneText = AMLUtils.FixGamePath(doneText);
                        if (doneText == null)
                        {
                            ShowWarning("This is not the correct path!");
                            OutputText = null;
                            return;
                        }
                        break;
                }

                OutputText = doneText;
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
