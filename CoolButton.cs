using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AstroModLoader
{
    public class CoolButton : Button
    {
        private static Font _defaultFont = new Font("Arial", 8F, FontStyle.Bold, GraphicsUnit.Point, (byte)0);

        public CoolButton() : base()
        {
            RefreshToDefaults();
        }

        public void RefreshToDefaults()
        {
            base.Font = _defaultFont;
            this.FlatStyle = FlatStyle.Flat;
            this.ForeColor = AMLPalette.ForeColor;
            base.FlatAppearance.BorderColor = AMLPalette.AccentColor;
            this.FlatAppearance.BorderSize = 1;
            this.BackColor = AMLPalette.ButtonBackColor;
            this.MinimumSize = new Size(0, 26);
        }
        
        protected override void OnControlAdded(ControlEventArgs e)
        {
            base.OnControlAdded(e);
            UseVisualStyleBackColor = false;
        }
    }
}
