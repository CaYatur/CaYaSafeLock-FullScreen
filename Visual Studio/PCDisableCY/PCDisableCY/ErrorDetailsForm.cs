using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PCDisableCY
{
    public partial class ErrorDetailsForm : Form
    {
        public ErrorDetailsForm(string errorMessage)
        {
            InitializeComponent();

            // ErrorMessage güncellemesi yapılacak bir koşul
            if (this.InvokeRequired)
            {
                this.Invoke(new MethodInvoker(() =>
                {
                    ErrorMessage.Text = errorMessage;
                }));
            }
            else
            {
                ErrorMessage.Text = errorMessage;
            }
        }
    }
}
