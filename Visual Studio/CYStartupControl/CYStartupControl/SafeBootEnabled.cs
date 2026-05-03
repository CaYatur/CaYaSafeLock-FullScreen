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
    public partial class SafeBootEnabled : Form
    {
        string SafeBootSystem = Path.Combine(@"C:\Windows\CaYaLKSafeMode\Data\CYfail\", "CYSafeBootOK.Safe");

        public SafeBootEnabled()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Hide();
            // Dosyanın varlığını kontrol et
            if (File.Exists(SafeBootSystem))
            {
                // Dosya mevcut, sil
                try
                {
                    File.Delete(SafeBootSystem);
                    Console.WriteLine($"Dosya '{SafeBootSystem}' başarıyla silindi.");


                    // Komutun tanımlanması
                    string command = "bcdedit";
                    string arguments = "/deletevalue {default} safeboot";

                    // Yeni bir işlem oluştur
                    Process process = new Process();
                    process.StartInfo.FileName = command;
                    process.StartInfo.Arguments = arguments;
                    process.StartInfo.UseShellExecute = false; // Shell kullanma
                    process.StartInfo.RedirectStandardOutput = true; // Çıktıyı yönlendir
                    process.StartInfo.RedirectStandardError = true; // Hata çıktısını yönlendir
                    process.StartInfo.CreateNoWindow = true; // Pencere oluşturma

                    try
                    {
                        // Süreci başlat
                        process.Start();
                        // Sürecin bitmesini bekle
                        process.WaitForExit();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Bir hata oluştu: {ex.Message}");
                    }



                    System.Diagnostics.Process.Start("shutdown", "/r /t 5 /f /c \"CaYaSafe bilgisayarınızı yeniden başlatıyor...\"");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Dosya silinirken hata oluştu: {ex.Message}");
                    MessageBox.Show("HATA! GÜVENLİ MOD DEVRE DIŞI BIRAKILIRKEN BİR SORUN OLUŞTU! Hata: " + ex.Message);
                }
            }

        }

        private void SafeBootEnabled_Load(object sender, EventArgs e)
        {
            SetFormLocationToBottomRight();
        }

        private void SafeBootEnabled_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            MessageBox.Show("Bu menü kapatılamaz.");
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
    }
}
