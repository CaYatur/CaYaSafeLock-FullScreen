using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CaYaSafeLockSetup
{
    public partial class CYRemovePrepare : Form
    {
        public CYRemovePrepare()
        {
            InitializeComponent();
        }

        private void CYRemovePrepare_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
        }

        private async void CYRemovePrepare_Shown(object sender, EventArgs e)
        {
            //Hide();
            await Task.Delay(5500);
        }

        private void CYRemovePrepare_Load(object sender, EventArgs e)
        {

        }
    }
}
