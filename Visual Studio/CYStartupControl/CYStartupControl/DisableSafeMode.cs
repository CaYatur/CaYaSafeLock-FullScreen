using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CYStartupControl
{
    public partial class DisableSafeMode : Form
    {
        string SafeBootSystem = Path.Combine(@"C:\Windows\CaYaLKSafeMode\Data\CYfail\", "CYSafeBootOK.Safe");
        private string filePath = @"C:\Users\Default\AppData\Roaming\CYSYSTEM\CYdata\Dat.sf";


        public DisableSafeMode()
        {
            InitializeComponent();
        }

        private void DisableSafeMode_Load(object sender, EventArgs e)
        {

            // Dosyanın varlığını kontrol et
            if (File.Exists(SafeBootSystem))
            {
                Process.Start("userinit.exe");
                Opacity = 0;
                Hide();
                this.ShowInTaskbar = false;

                SafeBootEnabled sbe = new SafeBootEnabled();
                sbe.Show();
            }
            else
            {
                CreateFile();

                DisableOtherProccess dop = new DisableOtherProccess();
                dop.Show();
            }
           
            //this.WindowState = FormWindowState.Maximized;
        }

        private void label4_Click(object sender, EventArgs e)
        {
            Detail dl = new Detail();
            dl.ShowDialog();
        }

        private void DisableSafeMode_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
        }



        private void CreateFile()
        {
            try
            {
                // Dosya mevcut mu kontrol et
                if (File.Exists(filePath))
                {
                    Console.WriteLine("Dat.sf dosyası zaten mevcut, işlem gerçekleştirilmeyecek.");
                }
                else
                {
                    // Dosya mevcut değilse, dosyayı oluştur
                    Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                    File.Create(filePath).Dispose(); // File.Create bir FileStream döndürür, bu yüzden Dispose çağrılır.
                    Console.WriteLine("Dat.sf dosyası oluşturuldu.");
                }
            }
            catch (Exception ex)
            {
                // Hata durumunda hata mesajını yazdır
                Console.WriteLine($"Bir hata oluştu: {ex.Message}");
            }
        }
    }
}
