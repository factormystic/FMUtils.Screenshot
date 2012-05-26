using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;

namespace FMUtils.Screenshot
{
    public class DWMBackingForm : Form
    {
        const int WS_EX_NOACTIVATE = 0x08000000;
        const int WS_EX_TOOLWINDOW = 0x00000080;
        const int WS_EX_TOPMOST = 0x00000008;

        protected override bool ShowWithoutActivation
        {
            get
            {
                return true;
            }
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams baseParams = base.CreateParams;
                baseParams.ExStyle |= (int)(WS_EX_NOACTIVATE | WS_EX_TOOLWINDOW | WS_EX_TOPMOST);

                return baseParams;
            }
        }

        public DWMBackingForm(Color color, Rectangle position)
        {
            Trace.WriteLine(string.Format("Creating backing form at {0}", position), "DWMBackingForm.ctor");

            FormBorderStyle = FormBorderStyle.None;
            BackColor = color;

            Location = position.Location;
            Size = position.Size;

            StartPosition = FormStartPosition.Manual;
            WindowState = FormWindowState.Normal;

            ShowInTaskbar = false;
            TopMost = true;
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // DWMBackingForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(278, 245);
            this.Name = "DWMBackingForm";
            this.ResumeLayout(false);

        }
    }
}
