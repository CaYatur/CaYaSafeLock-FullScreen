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
    public partial class CheckUninstaller : Form
    {
        public static event EventHandler StopService;


        public CheckUninstaller()
        {
            InitializeComponent();
        }

        private async void CheckUninstaller_Load(object sender, EventArgs e)
        {
            SetFormLocationToBottomRight();

            StopService?.Invoke(this, EventArgs.Empty);
        }


        private void SetFormLocationToBottomRight()
        {
            // Ekranın sağ alt köşesinin koordinatlarını al
            int screenWidth = Screen.PrimaryScreen.WorkingArea.Width;
            int screenHeight = Screen.PrimaryScreen.WorkingArea.Height;

            // Formun boyutunu ve konumunu ayarla
            //this.Width = 164; // Formun genişliğini isteğinize göre ayarlayabilirsiniz
            //this.Height = 260; // Formun yüksekliğini isteğinize göre ayarlayabilirsiniz

            int formX = screenWidth - this.Width;
            int formY = screenHeight - this.Height;

            this.Location = new System.Drawing.Point(formX, formY);
        }

        private void CheckUninstaller_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
        }
    }
}
