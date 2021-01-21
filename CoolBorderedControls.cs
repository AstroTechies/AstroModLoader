using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AstroModLoader
{
    public class CoolDataGridView : DataGridView
    {
        public CoolDataGridView()
        {
            BorderStyle = BorderStyle.None;
            this.Scroll += CoolDataGridView_Scroll;
        }

        private void CoolDataGridView_Scroll(object sender, ScrollEventArgs e)
        {
            for (int i = 0; i < this.Rows.Count; i++) this.InvalidateRow(i);
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            switch (m.Msg)
            {
                case AMLUtils.WM_PAINT:
                    using (Graphics g = this.CreateGraphics())
                    {
                        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.Default;
                        Rectangle rect = ClientRectangle;
                        using (var p = new Pen(AMLPalette.AccentColor, AMLPalette.BorderPenWidth) { Alignment = System.Drawing.Drawing2D.PenAlignment.Center })
                        {
                            g.DrawRectangle(p, rect);
                        }
                    }
                    break;
            }
        }
    }

    public class CoolListBox : ListBox
    {
        public CoolListBox()
        {
            BorderStyle = BorderStyle.None;
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            switch (m.Msg)
            {
                case AMLUtils.WM_PAINT:
                    using (Graphics g = this.CreateGraphics())
                    {
                        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.Default;
                        Rectangle rect = ClientRectangle;
                        using (var p = new Pen(AMLPalette.AccentColor, AMLPalette.BorderPenWidth) { Alignment = System.Drawing.Drawing2D.PenAlignment.Center })
                        {
                            g.DrawRectangle(p, rect);
                        }
                    }
                    break;
            }
        }
    }
}
