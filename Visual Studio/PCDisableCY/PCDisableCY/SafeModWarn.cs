using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;

namespace PCDisableCY
{
    public partial class SafeModWarn : Form
    {

        private bool Closing = false;
        private string filePath = @"C:\Users\Default\AppData\Roaming\CYSYSTEM\CYdata\Dat.sf";
        string SafeBootSystem = Path.Combine(@"C:\Windows\CaYaLKSafeMode\Data\CYfail\", "CYSafeBootOK.Safe");


        public SafeModWarn()
        {
            InitializeComponent();
        }

        private void SafeModWarn_Load(object sender, EventArgs e)
        {

        }

        private async void button2_Click(object sender, EventArgs e)
        {

            try
            {

                DialogResult result = MessageBox.Show("Bilgisayar güvenli moddda yeniden başlatılsınmı?", "Uyarı", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                // Kullanıcıya bilgisayarın tekrar başlatılması gerektiğini bildiriyoruz.
                if (result == DialogResult.Yes)
                {
                    Closing = true;
                    DeleteFile();
                    // "msconfig" komutunu kullanarak sistem yapılandırma penceresini aç
                    //Process.Start("msconfig", "/safeboot:minimal");

                    

                   
                    try
                    {
                        // Dosya yolu içindeki klasörlerin mevcut olup olmadığını kontrol et
                        string directoryPath = Path.GetDirectoryName(SafeBootSystem);
                        if (!Directory.Exists(directoryPath))
                        {
                            // Klasör yoksa oluştur
                            Directory.CreateDirectory(directoryPath);
                        }

                        // Dosya oluştur
                        using (FileStream fs = File.Create(SafeBootSystem))
                        {
                            // Dosyaya başlangıçta yazılacak bir veri ekleyebilirsiniz (isteğe bağlı)
                            byte[] info = new UTF8Encoding(true).GetBytes("ERROR CaYaSafeBoot is failed.(successfuly)");
                            fs.Write(info, 0, info.Length);
                        }

                        Console.WriteLine($"Dosya başarıyla oluşturuldu: {SafeBootSystem}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Bir hata oluştu: {ex.Message}");
                    }



                    await Task.Delay(100);

                    // Komutun tanımlanması
                    string command = "bcdedit";
                    string arguments = "/set {default} safeboot network";

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

                    // Bilgisayarı yeniden başlatıyoruz.
                    System.Diagnostics.Process.Start("shutdown", "/r /t 5 /f /c \"CaYaSafe bilgisayarınızı yeniden başlatıyor...\"");
                }
                else
                {
                    Closing = true;
                    DeleteFile();
                    Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hata: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        private void button1_Click(object sender, EventArgs e)
        {
            Closing = true;
            DeleteFile();
            Close();
        }

        private void DeleteFile()
        {
            try
            {
                // Dosya mevcut mu kontrol et
                if (File.Exists(filePath))
                {
                    // Dosyayı sil
                    File.Delete(filePath);
                    Console.WriteLine("Dosya başarıyla silindi.");
                }
                else
                {
                    Console.WriteLine("Dosya zaten mevcut değil.");
                }
            }
            catch (Exception ex)
            {
                // Hata durumunda hata mesajını yazdır
                Console.WriteLine($"Bir hata oluştu: {ex.Message}");
            }
        }

        private void SafeModWarn_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (Closing == false)
            {

                DialogResult result = MessageBox.Show("İşlemi iptal etmek istediğinizden eminmisiniz? Bu işlemi onaylayana veya onaylamayana kadar bu mesaj görünücektir. İşlemi İptal etmeniz TAVSİYE EDİLMEZ.", "EYLEM GEREKLİ!", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                // Kullanıcıya bilgisayarın tekrar başlatılması gerektiğini bildiriyoruz.
                if (result == DialogResult.Yes)
                {
                    e.Cancel = false;
                }
                else
                {
                    e.Cancel = true;
                }
            }
            else
            {
                e.Cancel = false;
            }
        }
    }
}
