using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static PCDisableCY.LockScreen;
using System.Security.Cryptography;
using System.Threading;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Button;

namespace PCDisableCY
{
    public partial class Main : Form
    {
        public static event EventHandler CloseSystem;

        private string filePath = @"C:\Users\Default\AppData\Roaming\CYSYSTEM\CYdata\Dat.sf";
        private static bool isShutdownTriggered = false;

        public Main()
        {
            InitializeComponent();
            //Task.Run(() => MonitorMemory());
        }

        private async void Main_Load(object sender, EventArgs e)
        {
            Hide();
            CheckerUninstall();
            WatcherANT wa = new WatcherANT();
            wa.Show();
            wa.Hide();
            await Task.Delay(100);

            //SharedData.TimerSystem = 10;
            SharedData.TimerSystem = 2400;
            SharedData.TimerSystemEnabled = true;
            SharedData.TimerSystemEnabledForce = true;
            SharedData.TimerSystemEnabledForceCY = true;
            SharedData.TimerSystemResetEnabled = false;

            LockScreen lc = new LockScreen();
            lc.Show();
            CheckFile();


        }

        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
        }


        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool IsWindowVisible(IntPtr hWnd);

        private void CheckerUninstall()
        {
            Thread bgThread = new Thread(() =>
            {

                while (true)
                {


                    string windowTitle = "CYRemovePrepare"; // Hedef başlık

                    // Belirli bir başlığa sahip pencereyi bul
                    IntPtr hWnd = FindWindow(null, windowTitle);

                    if (hWnd != IntPtr.Zero && IsWindowVisible(hWnd))
                    {
                        // Pencere bulundu ve görünürse
                        Console.WriteLine("Pencere bulundu: " + windowTitle);

                        // Pencereyi açık olan bir işleme ilişkilendir
                        Process[] processes = Process.GetProcessesByName("CaYaSafeLockSetup");
                        if (processes.Length > 0)
                        {
                            // İlgili işlemi bulduysa işlemi gerçekleştir
                            Console.WriteLine("İlgili işlem bulundu: " + processes[0].ProcessName);
                            // Burada belirli bir pencere başlığına sahip bir pencere bulunduğunda yapılacak işlemi gerçekleştirin
                            //Environment.Exit(0);
                            CloseSystem?.Invoke(this, EventArgs.Empty);




                            CheckUninstaller cu = new CheckUninstaller();
                            cu.ShowDialog();

                            // İşlemleri burada gerçekleştirin
                        }
                        else
                        {
                            Console.WriteLine("İlgili işlem bulunamadı.");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Pencere bulunamadı veya gizli.");
                    }

                    Thread.Sleep(3000);


                }


            });
            bgThread.IsBackground = true;
            bgThread.Start();


            ProgramCheck();
        }


      







        private void ProgramCheck()
        {
            // Mevcut programın adını al
            string programName = Process.GetCurrentProcess().ProcessName;

            // Çalışan tüm işlemleri kontrol edin
            Process[] runningProcesses = Process.GetProcessesByName(programName);

            if (runningProcesses.Length > 1)
            {
                // Birden fazla örnek çalışıyor
                Console.WriteLine($"{programName} programının birden fazla örneği çalışıyor.");

                // Burada başka bir işlem gerçekleştirebilirsiniz
                // Örneğin, tüm örnekleri kapatmak istiyorsanız:
                //foreach (var process in runningProcesses)
                //{
                //    if (process.Id != Process.GetCurrentProcess().Id) // Kendi sürecinizi kapatmamak için kontrol
                //    {
                //        process.Kill();
                //        Console.WriteLine($"{process.ProcessName} kapatıldı. (ID: {process.Id})");
                //    }
                //}

                ShutdownComputer();

            }
            else if (runningProcesses.Length == 1)
            {
                // Sadece bir örnek çalışıyor
                Console.WriteLine($"{programName} programının bir örneği çalışıyor.");
            }
            else
            {
                // Hiçbir örnek çalışmıyor
                Console.WriteLine($"{programName} programının çalışmadığı tespit edildi. Bu durumun olması beklenmiyor.");
            }
        }

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


        private void CheckFile()
        {
            try
            {
                // Dosya mevcut mu kontrol et
                if (File.Exists(filePath))
                {
                    Console.WriteLine("Dat.sf dosyası mevcut.");
                    SafeModWarn smw = new SafeModWarn();
                    smw.Show();


                }
                else
                {
                    Console.WriteLine("Dat.sf dosyası mevcut değil.");
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
