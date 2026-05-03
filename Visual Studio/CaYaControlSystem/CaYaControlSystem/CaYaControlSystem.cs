using System;
using System.Diagnostics;
using System.IO;
using System.ServiceProcess;
using System.Threading.Tasks;

namespace CaYaControlSystem
{
    partial class CaYaControlSystem : ServiceBase
    {
        private string serviceName = "CYSADS";
        private System.Threading.Thread controlThread;
        private bool OneTimeRun = false;

        private FileSystemWatcher fileWatcher;
        private string monitoredFolderPath = @"C:\ProgramData\CaYaProtection\CaYaSafe\LockSC";
        private string controlFileName = "RLTMC.cysf";
        private string forbiddenContent = "CYSFCL";


        public CaYaControlSystem()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            // Hizmet kontrol döngüsünü başlat
            controlThread = new System.Threading.Thread(ControlService);
            controlThread.Start();
        }

        protected override void OnStop()
        {
            // Hizmet kontrol döngüsünü durdur
            controlThread.Abort();
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
