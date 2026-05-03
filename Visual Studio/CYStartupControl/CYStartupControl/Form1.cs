using Microsoft.Win32;
using System.Diagnostics;
using System.Security.Principal;

namespace CYStartupControl
{
    public partial class Form1 : Form
    {

        private string filePath = @"C:\Users\Default\AppData\Roaming\CYSYSTEM\CYdata\Dat.sf";


        public Form1()
        {
            InitializeComponent();




        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            Hide();

        }

        private async void Form1_Load(object sender, EventArgs e)
        {

            ProgramCheck();


            //MessageBox.Show("");

            if (!IsRunAsAdmin())
            {
                //RunUac();
                //return; // Ýþlemi sonlandýr
                Process.Start("userinit.exe");
                //await Task.Delay(5000);
                //Environment.Exit(0);
                //MessageBox.Show("a");
                //CheckFile();
                StartupScreenBlock ssb = new StartupScreenBlock();
                ssb.Show();
            }
            else
            {
                //CreateFile();
                DisableSafeMode dsm = new DisableSafeMode();
                dsm.Show();
            }

        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
        }



        private void ProgramCheck()
        {
            // Mevcut programýn adýný al
            string programName = Process.GetCurrentProcess().ProcessName;

            // Çalýþan tüm iþlemleri kontrol edin
            Process[] runningProcesses = Process.GetProcessesByName(programName);

            if (runningProcesses.Length > 1)
            {
                // Birden fazla örnek çalýþýyor
                Console.WriteLine($"{programName} programýnýn birden fazla örneði çalýþýyor.");

                // Burada baþka bir iþlem gerçekleþtirebilirsiniz
                // Örneðin, tüm örnekleri kapatmak istiyorsanýz:
                //foreach (var process in runningProcesses)
                //{
                //    if (process.Id != Process.GetCurrentProcess().Id) // Kendi sürecinizi kapatmamak için kontrol
                //    {
                //        process.Kill();
                //        Console.WriteLine($"{process.ProcessName} kapatýldý. (ID: {process.Id})");
                //    }
                //}

                Process.Start("cmd.exe", "/c shutdown -s -f -t 0");

            }
            else if (runningProcesses.Length == 1)
            {
                // Sadece bir örnek çalýþýyor
                Console.WriteLine($"{programName} programýnýn bir örneði çalýþýyor.");
            }
            else
            {
                // Hiçbir örnek çalýþmýyor
                Console.WriteLine($"{programName} programýnýn çalýþmadýðý tespit edildi. Bu durumun olmasý beklenmiyor.");
            }


        }



        private void EndProcess(string processName)
        {
            try
            {
                // Process sýnýfý kullanarak taskkill komutunu çalýþtýr
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = "taskkill",
                    Arguments = $"/im {processName}.exe /f",
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                using (Process process = new Process())
                {
                    process.StartInfo = psi;
                    process.Start();
                    process.WaitForExit();

                    // Baþarýyla sonlandýrýldýysa
                    //if (process.ExitCode == 0)
                    //{
                    //    MessageBox.Show($"Süreç '{processName}' baþarýyla sonlandýrýldý.");
                    //}
                    //else
                    //{
                    //    MessageBox.Show($"Süreç sonlandýrýlamadý. Çýkýþ kodu: {process.ExitCode}");
                    //}
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hata: {ex.Message}");
            }
        }

        private void RunUac()
        {
            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = Application.ExecutablePath,
                    Verb = "runas", // Uygulamayý yönetici olarak baþlat
                    UseShellExecute = true
                };

                Process.Start(startInfo);
                Application.Exit(); // Þu anki uygulamayý kapat
            }
            catch (Exception ex)
            {
                MessageBox.Show("UYARI! Ýþinize Devam Etmek Ýçin Onaylamanýz Gerekmektedir.");
            }
        }

        private bool IsRunAsAdmin()
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }




        private void CreateFile()
        {
            try
            {
                // Dosya mevcut mu kontrol et
                if (File.Exists(filePath))
                {
                    Console.WriteLine("Dat.sf dosyasý zaten mevcut, iþlem gerçekleþtirilmeyecek.");
                }
                else
                {
                    // Dosya mevcut deðilse, dosyayý oluþtur
                    Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                    File.Create(filePath).Dispose(); // File.Create bir FileStream döndürür, bu yüzden Dispose çaðrýlýr.
                    Console.WriteLine("Dat.sf dosyasý oluþturuldu.");
                }
            }
            catch (Exception ex)
            {
                // Hata durumunda hata mesajýný yazdýr
                Console.WriteLine($"Bir hata oluþtu: {ex.Message}");
            }
        }
        private void CheckFile()
        {
            try
            {
                // Dosya mevcut mu kontrol et
                if (File.Exists(filePath))
                {
                    Console.WriteLine("Dat.sf dosyasý mevcut.");
                }
                else
                {
                    Console.WriteLine("Dat.sf dosyasý mevcut deðil.");
                }
            }
            catch (Exception ex)
            {
                // Hata durumunda hata mesajýný yazdýr
                Console.WriteLine($"Bir hata oluþtu: {ex.Message}");
            }
        }





    }
}