using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PCDisableCY
{
    public partial class TimeOut : Form
    {
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        public TimeOut()
        {
            InitializeComponent();
        }
        int TO = 60;
        private bool Closing = false;
        private async void TimeOut_Load(object sender, EventArgs e)
        {
            TopMost = true; // Formunuzu üstte tutun
            this.Show(); // Formu gösterin (eğer gizli ise)
            this.Activate(); // Formu aktif hale getirin
            SetForegroundWindow(this.Handle); // Formu ön plana getirin
            //this.Opacity = 70;
            while (TO > 0)
            {
                TO--;
                label3.Text = TO.ToString();
                await Task.Delay(1000);
            }
            //Hide();
            Closing = true;
            Close();

        }

        private void TimeOut_FormClosing(object sender, FormClosingEventArgs e)
        {
            if(Closing == false)
            {
                e.Cancel = true;
            }
            else
            {
                e.Cancel = false;
                Dispose();
            }
            
        }
    }
}
