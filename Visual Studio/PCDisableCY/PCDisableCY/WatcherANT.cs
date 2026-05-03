using System;
using System.IO;
using System.Diagnostics;
using System.Management;
using System.Security.AccessControl;
using System.Windows.Forms;
using System.Security.Principal;
using System.Runtime.InteropServices;
using System.ServiceProcess;

namespace PCDisableCY
{
    public partial class WatcherANT : Form
    {
        public WatcherANT()
        {
            InitializeComponent();
        }
        private string filePathRLTMC = @"C:\ProgramData\CaYaProtection\CaYaSafe\LockSC\RLTMC.cysf";
        string filePathFtop = @"C:\ProgramData\CaYaProtection\CaYaSafe\LockSC\FTOP";
        private bool writeClock = true;
        bool Closing = false;
        bool Opened = false;
        string programName = "CYPCcheck";


        private async void WatcherANT_Load(object sender, EventArgs e)
        {
            try
            {
                // Dosyayı sil
                File.Delete(filePathFtop);
                Console.WriteLine("Dosya başarıyla silindi.");
            }
            catch (Exception ex)
            {
                // Dosya silme hatası durumunda burası çalışır
                Console.WriteLine("Dosya silme hatası: " + ex.Message);
            }


            

            CheckUninstaller.StopService += StopingServices;
            CheckShutdown();

            



            try
            {
                Task.Run(() => WriteClock());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            Hide();

            //try
            //{
            //    await Task.Delay(500);

            //    string serviceName = "CYSADS";

            //    ServiceController service = new ServiceController(serviceName);

            //    try
            //    {
            //        if (service.Status != ServiceControllerStatus.Running)
            //        {
            //            Console.WriteLine($"{serviceName} servisi başlatılıyor...");
            //            service.Start();
            //            service.WaitForStatus(ServiceControllerStatus.Running);
            //            Console.WriteLine($"{serviceName} servisi başarıyla başlatıldı.");
            //        }
            //        else
            //        {
            //            Console.WriteLine($"{serviceName} servisi zaten çalışıyor.");
            //        }
            //    }
            //    catch (Exception ex)
            //    {
            //        Console.WriteLine($"{serviceName} servisi başlatılamadı: {ex.Message}");
            //    }
            //}
            //catch 
            //{ 
            
            //}
            

        }

        private async void StopingServices(object sender, EventArgs e)
        {
            Closing = true;
            // Dosyanın içeriğini "CYSFCL" olarak değiştir
            writeClock = false; // Saat yazma işlemini durdur
            await Task.Delay(1500);
            File.WriteAllText(filePathRLTMC, "CYSFCL");

            Process.Start("taskkill", "/F /IM " + programName + ".exe");

            await Task.Delay(1500);
            Environment.Exit(0);
        }


        private async Task WriteClock()
        {
            while (writeClock)
            {
                string currentTime = DateTime.Now.ToString("HHmmss");
                await Task.Delay(1000); // 1 saniye bekle

                // Dosyayı kontrol et, eğer yoksa oluştur
                if (!File.Exists(filePathRLTMC))
                {
                    FileStream fs = File.Create(filePathRLTMC);
                    fs.Close();
                }

                File.WriteAllText(filePathRLTMC, currentTime);
            }
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            // Dosyanın içeriğini "CYSFCL" olarak değiştir
            writeClock = false; // Saat yazma işlemini durdur
            await Task.Delay(1500);
            File.WriteAllText(filePathRLTMC, "CYSFCL");
        }

        private void WatcherANT_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
        }




        private void CheckShutdown()
        {
            //// Açmak istediğiniz programın dosya yolunu belirtin
            //string programPath = @"C:\ProgramData\CaYaProtection\CaYaSafe\PCcheck\CYPCcheck.exe";

            //try
            //{
            //    // ProcessStartInfo nesnesi oluşturun ve gerekli bilgileri sağlayın
            //    ProcessStartInfo startInfo = new ProcessStartInfo(programPath);

            //    // Yeni bir işlem başlatın
            //    using (Process process = Process.Start(startInfo))
            //    {
            //        // İşlemin başlatıldığını belirten mesajı gösterin
            //        Console.WriteLine("Program başlatıldı.");
            //    }
            //}
            //catch (Exception ex)
            //{
            //    // Hata durumunda kullanıcıya bilgi verin
            //    Console.WriteLine($"Program başlatılamadı: {ex.Message}");
            //    MessageBox.Show($"Program başlatılamadı: {ex.Message}");
            //}


            string exePath = @"C:\ProgramData\CaYaProtection\CaYaSafe\PCcheck\CYPCcheck.exe";

            try
            {
                Process.Start(exePath);
                Console.WriteLine($"{exePath} başarıyla başlatıldı.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{exePath} başlatılamadı: {ex.Message}");
            }


            Thread bgThread = new Thread(() =>
            {

                while (Closing == false)
                {

                    // Belirli bir isme sahip çalışan tüm işlemleri al
                    Process[] processes = Process.GetProcessesByName(programName);


                    if (Opened == false)
                    {  
                        // Eğer belirtilen program çalışmıyorsa
                        if (processes.Length == 0)
                        {
                            // İşlemi gerçekleştir

                            //Console.WriteLine(programName + " programı çalışmıyor, işlem gerçekleştirildi.");
                        }
                        else
                        {
                            //Console.WriteLine(programName + " programı çalışıyor.");
                            Opened = true;
                        }
                    }
                    

                    if (Opened == true)
                    {
                        // Eğer belirtilen program çalışmıyorsa
                        if (processes.Length == 0)
                        {
                            // İşlemi gerçekleştir
                            //Process.Start("taskkill", "/F /IM " + programName); // Örnek olarak notepad.exe'yi kapatır

                            //Process.Start("cmd.exe", "/c shutdown -s -f -t 0");
                            ShutdownComputer();

                            Console.WriteLine(programName + " programı çalışmıyor, işlem gerçekleştirildi.");
                        }
                        else
                        {
                            //Console.WriteLine(programName + " programı çalışıyor.");
                        }
                    }

                    Thread.Sleep(5);
                }


            });
            bgThread.IsBackground = true;
            bgThread.Start();
           
            
        }

        private static bool isShutdownTriggered = false;
        static void ShutdownComputer()
        {
            if (!isShutdownTriggered) // Kapatma işlemi daha önce tetiklenmemişse
            {
                isShutdownTriggered = true; // Kapatma işlemini tetikle
                ProcessStartInfo processInfo = new ProcessStartInfo("shutdown", "/s /f /t 0")
                {
                    CreateNoWindow = true, // Pencereyi oluşturma
                    UseShellExecute = false // Shell kullanma
                };

                Process.Start(processInfo);
            }
            else
            {
                Console.WriteLine("Bilgisayar zaten kapanma işlemi için tetiklendi.");
            }
        }

    }
}
