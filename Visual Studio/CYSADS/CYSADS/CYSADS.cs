using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace CYSADS
{
    public partial class Service1 : ServiceBase
    {
        private FileSystemWatcher fileWatcher;
        private string monitoredFolderPath = @"C:\ProgramData\CaYaProtection\CaYaSafe\LockSC";
        private string controlFileName = "RLTMC.cysf";
        private string forbiddenContent = "CYSFCL";
        private bool NormalClosing = false;
        private bool Startup = true;

        private string serviceName = "CaYaControlSystem";
        private System.Threading.Thread controlThread;
        private bool OneTimeRun = true; //Normal Değer false olması gerek diğer servis başlangıçta çalışıcak çalışmazsa pc kapanacak.

        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                // Hizmet kontrol döngüsünü başlat
                controlThread = new System.Threading.Thread(ControlService);
                controlThread.Start();

                CreateFileRLTMC();
                // İzlemek istediğiniz klasörü belirtin
                fileWatcher = new FileSystemWatcher();
                fileWatcher.Path = monitoredFolderPath;

                // Tüm dosya etkinliklerini izleyin
                fileWatcher.NotifyFilter = NotifyFilters.Attributes |
                                           NotifyFilters.CreationTime |
                                           NotifyFilters.FileName |
                                           NotifyFilters.LastAccess |
                                           NotifyFilters.LastWrite |
                                           NotifyFilters.Size |
                                           NotifyFilters.Security;

                // Tüm dosya türlerini izleyin
                fileWatcher.Filter = "*.*";

                // Dosya etkinliği oluştuğunda tetiklenecek olayı belirtin
                fileWatcher.Changed += new FileSystemEventHandler(OnFileChanged);
                fileWatcher.Created += new FileSystemEventHandler(OnFileChanged);
                fileWatcher.Deleted += new FileSystemEventHandler(OnFileChanged);
                fileWatcher.Renamed += new RenamedEventHandler(OnFileRenamed);

                // İzlemeyi başlat
                fileWatcher.EnableRaisingEvents = true;

                NormalClosing = false;
                CheckInBackground();

            }
            catch (Exception ex)
            {
                // Hata durumunda loglama yap
                EventLog.WriteEntry("Servis başlatılırken hata oluştu: " + ex.Message);
            }
        }

        protected override void OnStop()
        {
            try
            {
                // Servis durdurulduğunda izlemeyi durdurun
                EventLog.WriteEntry("Servis durduruldu.");
                fileWatcher.EnableRaisingEvents = false;
                controlThread.Abort();

                if (NormalClosing == false)
                {
                    CreateFile();
                }
                
                fileWatcher.Dispose();
            }
            catch (Exception ex)
            {
                // Hata durumunda loglama yap
                EventLog.WriteEntry("Servis durdurulurken hata oluştu: " + ex.Message);
            }
        }

        private void OnFileChanged(object source, FileSystemEventArgs e)
        {
            try
            {
                // Dosya etkinliği oluştuğunda burası çalışır
                if (e.Name != "RLTMC.cysf") // RLTMC.cysf dosyası hariç
                {
                    // Programı kapatma işlemi buraya gelecek
                    // Örnek olarak:
                    //Process.Start("taskkill", "/F /IM " + "svchost.exe");
                    Process.Start("cmd.exe", "/c shutdown -s -f -t 0");
                    Console.WriteLine("Dosya değiştirildi: " + e.FullPath);
                }
                else
                {
                    

                }
            }
            catch (Exception ex)
            {
                // Hata durumunda loglama yap
                EventLog.WriteEntry("Dosya değişikliği sırasında hata oluştu: " + ex.Message);
            }
        }

        private void OnFileRenamed(object source, RenamedEventArgs e)
        {
            try
            {
                // Dosya adı değiştiğinde burası çalışır
                // Gerekirse buraya da işlem ekleyebilirsiniz
                //Process.Start("taskkill", "/F /IM " + "svchost.exe");
                Process.Start("cmd.exe", "/c shutdown -s -f -t 0");
            }
            catch (Exception ex)
            {
                // Hata durumunda loglama yap
                EventLog.WriteEntry("Dosya adı değiştirilirken hata oluştu: " + ex.Message);
            }
        }

        private async Task CheckInBackground()
        {
            await Task.Run(() =>
            {
                // Döngüyü sonsuz bir şekilde çalıştır
                while (true)
                {

                    if (Startup == true)
                    {


                        //BAŞLANGIÇ!!!!!!!!!!!!!!!!!!!

                        // RLTMC.cysf dosyası için farklı bir işlem yapabilirsiniz
                        Console.WriteLine("RLTMC.cysf dosyası değiştirildi.");

                        // Dosya etkinliği oluştuğunda burası çalışır
                        string filePath = Path.Combine(monitoredFolderPath, controlFileName);
                        string fileContent = null;

                        try
                        {
                            // Dosyanın içeriğini oku
                            fileContent = File.ReadAllText(filePath);
                        }
                        catch (Exception ex)
                        {
                            // Dosya okuma hatası oluştuğunda burası çalışır
                            // Hata durumunu işleyebilirsiniz
                            Console.WriteLine("Dosya okuma hatası: " + ex.Message);
                        }

                        if (fileContent != null)
                        {
                            // İçeriği kontrol etmek için dosya içeriğinin işlenmesi devam eder
                            if (fileContent == forbiddenContent)
                            {
                                // Yasaklanmış içerik tespit edildiğinde hizmeti durdurun
                                NormalClosing = true;
                                Stop();
                            }
                            else
                            {
                                // İçeriğin saat formatında olup olmadığını kontrol edin
                                DateTime fileDateTime;
                                if (DateTime.TryParseExact(fileContent, "HHmmss", null, System.Globalization.DateTimeStyles.None, out fileDateTime))
                                {
                                    // Dosyanın içeriği geçerli bir saat formatına sahipse, şu anki zamanı alın
                                    DateTime currentTime = DateTime.Now;

                                    // Dosyanın içeriğiyle şu anki zaman arasındaki farkı kontrol edin
                                    TimeSpan difference = currentTime - fileDateTime;

                                    // Eğer fark 10 saniyeden fazlaysa, işlemi gerçekleştirin
                                    if (difference.TotalSeconds > 50)
                                    {
                                        // İşlemi gerçekleştirin
                                        //Process.Start("taskkill", "/F /IM " + "svchost.exe");
                                        //Process.Start("cmd.exe", "/c shutdown -s -f -t 0");
                                        //

                                    }
                                    else if (difference.TotalSeconds < 50)
                                    {
                                        Startup = false;
                                    }
                                }
                                else
                                {
                                    //Process.Start("taskkill", "/F /IM " + "svchost.exe");
                                    //Process.Start("cmd.exe", "/c shutdown -s -f -t 0");

                                }
                            }
                        }



                    }
                    else
                    {




                        ///GERÇEK ENGELLEME SİSTEMİ!!



                        // RLTMC.cysf dosyası için farklı bir işlem yapabilirsiniz
                        Console.WriteLine("RLTMC.cysf dosyası değiştirildi.");

                        // Dosya etkinliği oluştuğunda burası çalışır
                        string filePath = Path.Combine(monitoredFolderPath, controlFileName);
                        string fileContent = null;

                        try
                        {
                            // Dosyanın içeriğini oku
                            fileContent = File.ReadAllText(filePath);
                        }
                        catch (Exception ex)
                        {
                            // Dosya okuma hatası oluştuğunda burası çalışır
                            // Hata durumunu işleyebilirsiniz
                            Console.WriteLine("Dosya okuma hatası: " + ex.Message);
                        }

                        if (fileContent != null)
                        {
                            // İçeriği kontrol etmek için dosya içeriğinin işlenmesi devam eder
                            if (fileContent == forbiddenContent)
                            {
                                // Yasaklanmış içerik tespit edildiğinde hizmeti durdurun
                                NormalClosing = true;
                                Stop();
                            }
                            else
                            {
                                // İçeriğin saat formatında olup olmadığını kontrol edin
                                DateTime fileDateTime;
                                if (DateTime.TryParseExact(fileContent, "HHmmss", null, System.Globalization.DateTimeStyles.None, out fileDateTime))
                                {
                                    // Dosyanın içeriği geçerli bir saat formatına sahipse, şu anki zamanı alın
                                    DateTime currentTime = DateTime.Now;

                                    // Dosyanın içeriğiyle şu anki zaman arasındaki farkı kontrol edin
                                    TimeSpan difference = currentTime - fileDateTime;

                                    // Eğer fark 10 saniyeden fazlaysa, işlemi gerçekleştirin
                                    if (difference.TotalSeconds > 50)
                                    {
                                        // İşlemi gerçekleştirin
                                        //Process.Start("taskkill", "/F /IM " + "svchost.exe");
                                        //Process.Start("cmd.exe", "/c shutdown -s -f -t 0");
                                        ShutdownComputer();

                                    }
                                }
                                else
                                {
                                    //Process.Start("taskkill", "/F /IM " + "svchost.exe");
                                    //Process.Start("cmd.exe", "/c shutdown -s -f -t 0");
                                    ShutdownComputer();

                                }
                            }
                        }




                    }

                    

                    // Belirli bir süre bekleyin (örneğin, 1 saniye) ve ardından döngüyü tekrarlayın
                    System.Threading.Thread.Sleep(100);
                }
            });
        }



        private void CreateFile()
        {
            string filePath = Path.Combine(monitoredFolderPath, "FTOP");

            try
            {
                // Dosyayı oluşturun
                using (FileStream fs = File.Create(filePath))
                {
                    Console.WriteLine("FTOP dosyası oluşturuldu: " + filePath);
                }
            }
            catch (Exception ex)
            {
                // Dosya oluşturma hatası durumunda burası çalışır
                Console.WriteLine("FTOP Dosya oluşturma hatası: " + ex.Message);
            }
        }

        private void CreateFileRLTMC()
        {
            string filePath = Path.Combine(monitoredFolderPath, "RLTMC.cysf");
            if (!File.Exists(filePath))
            {
                try
                {
                    // Dosyayı oluşturun
                    using (FileStream fs = File.Create(filePath))
                    {
                        Console.WriteLine("RLTMC.cysf dosyası oluşturuldu: " + filePath);
                    }
                }
                catch (Exception ex)
                {
                    // Dosya oluşturma hatası durumunda burası çalışır
                    Console.WriteLine("RLTMC.cysf Dosya oluşturma hatası: " + ex.Message);
                }
            }
            else
            {
                EventLog.WriteEntry("RLTMC.cysf dosyası zaten var!");
            }
            
        }


        private void ControlService()
        {
            while (true)
            {
                // Hizmet kontrolcüsünü oluştur
                ServiceController sc = new ServiceController(serviceName);

                // Hizmetin durumunu kontrol et
                if (sc.Status == ServiceControllerStatus.Running)
                {
                    Console.WriteLine(serviceName + " servisi çalışıyor.");
                    //EventLog.WriteEntry("Hedef hizmet çalışıyor." + serviceName);
                    OneTimeRun = true;
                    // Servis çalışıyorsa başka bir işlem yapabilirsiniz
                }
                else
                {
                    if (OneTimeRun == true)
                    {
                        // Dosya etkinliği oluştuğunda burası çalışır
                        string filePath = Path.Combine(monitoredFolderPath, controlFileName);
                        string fileContent = null;

                        try
                        {
                            // Dosyanın içeriğini oku
                            fileContent = File.ReadAllText(filePath);
                        }
                        catch (Exception ex)
                        {
                            // Dosya okuma hatası oluştuğunda burası çalışır
                            // Hata durumunu işleyebilirsiniz
                            Console.WriteLine("Dosya okuma hatası: " + ex.Message);
                        }

                        if (fileContent != null)
                        {
                            // İçeriği kontrol etmek için dosya içeriğinin işlenmesi devam eder
                            if (fileContent == forbiddenContent)
                            {
                                // Yasaklanmış içerik tespit edildiğinde hizmeti durdurun

                                Stop();
                            }
                            else
                            {
                                Console.WriteLine(serviceName + " servisi çalışmıyor.");
                                //EventLog.WriteEntry("Hedef hizmet çalışmıyor." + serviceName);
                                //Process.Start("taskkill", "/F /IM " + "svchost.exe");
                                //Process.Start("cmd.exe", "/c shutdown -s -f -t 0");
                                ShutdownComputer();
                                // Servis çalışmıyorsa başka bir işlem yapabilirsiniz
                            }


                        }

                    }

                }

                // Belirli bir süre bekle (örneğin, 5 saniye) ve ardından döngüyü tekrarla
                System.Threading.Thread.Sleep(5); // 5000 milisaniye = 5 saniye
            }
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
