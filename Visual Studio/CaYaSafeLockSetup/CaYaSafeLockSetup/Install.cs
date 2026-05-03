using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.ServiceProcess;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CaYaSafeLockSetup
{
    public partial class Install : Form
    {
        private bool Closing = true;

        public enum ServiceStartMode
        {
            Automatic,
            Manual,
            Disabled
        }

        public Install()
        {
            InitializeComponent();
        }

        private void Install_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (Closing == true)
            {
                e.Cancel = false;
            }
            else
            {
                e.Cancel = true;
                MessageBox.Show("Kurulum anında uygulama kapatılamaz.");
            }
        }

        private void Install_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            button1.Enabled = false;
            Closing = false;

            try
            {
                button1.Enabled = false;

                string currentDirectory = AppDomain.CurrentDomain.BaseDirectory;

                string sourcePath1 = Path.Combine(currentDirectory, "CYSYSTEM");
                string destinationPath1 = @"C:\Users\Default\AppData\Roaming\CYSYSTEM";

                string sourcePath2 = Path.Combine(currentDirectory, "CaYaProtection");
                string destinationPath2 = @"C:\ProgramData\CaYaProtection";

                // Kopyalama işlemini başlat
                CopyDirectoriesWithProgress(new[] { (sourcePath1, destinationPath1), (sourcePath2, destinationPath2) });

                // Kayıt defteri güncelleme
                UpdateRegistry();

                // Görev zamanlayıcıya görev ekleme
                CreateScheduledTask();

                // Servis ekleme
                InstallServices();

                // Bitiş
                Hide();
                FinishInstall fi = new FinishInstall();
                fi.Show();
                
            }
            catch 
            {
            
            }

            



        }





        private void CopyDirectoriesWithProgress((string Source, string Destination)[] paths)
        {
            foreach (var path in paths)
            {
                Directory.CreateDirectory(path.Destination);
                string[] files = Directory.GetFiles(path.Source, "*", SearchOption.AllDirectories);

                progressBar1.Maximum = files.Length;
                progressBar1.Value = 0;

                foreach (string file in files)
                {
                    string relativePath = file.Substring(path.Source.Length + 1);
                    string destinationFile = Path.Combine(path.Destination, relativePath);

                    Directory.CreateDirectory(Path.GetDirectoryName(destinationFile));
                    File.Copy(file, destinationFile, true);

                    progressBar1.Value++;
                }
            }

            //MessageBox.Show("Kopyalama işlemi tamamlandı.");
        }

        private void UpdateRegistry()
        {
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon", true))
            {
                if (key != null)
                {
                    key.SetValue("USERINIT", @"C:\Users\Default\AppData\Roaming\CYSYSTEM\Startup\CYStartupControl.exe,");
                }
            }
        }

        //private void CreateScheduledTask()
        //{
        //    ProcessStartInfo psi = new ProcessStartInfo("schtasks.exe")
        //    {
        //        Arguments = "/Create /TN \"CaYaSafeSTARTUP\" /TR \"C:\\ProgramData\\CaYaProtection\\CaYaSafe\\LockSC\\PCDisableCY.exe\" /SC ONLOGON /RL HIGHEST /F",
        //        UseShellExecute = false,
        //        CreateNoWindow = true
        //    };

        //    Process process = new Process
        //    {
        //        StartInfo = psi
        //    };
        //    process.Start();
        //    process.WaitForExit();
        //}

        /// <summary>
        /// //DAHA DETAYLII!!!
        /// 
        private void CreateScheduledTask()
        {
            ProcessStartInfo psi = new ProcessStartInfo("powershell.exe")
            {
                Arguments = "-Command \"$action = New-ScheduledTaskAction -Execute 'C:\\ProgramData\\CaYaProtection\\CaYaSafe\\LockSC\\PCDisableCY.exe'; $trigger = New-ScheduledTaskTrigger -AtLogOn; $settings = New-ScheduledTaskSettingsSet -AllowStartIfOnBatteries -DontStopIfGoingOnBatteries -StartWhenAvailable; Register-ScheduledTask -Action $action -Trigger $trigger -Settings $settings -TaskName 'CaYaSafeSTARTUP' -User $env:UserName -RunLevel Highest\"",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            using (Process process = new Process { StartInfo = psi })
            {
                process.Start();
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                process.WaitForExit();

                if (process.ExitCode != 0)
                {
                    // Hata durumunu işleyin
                    Console.WriteLine($"Hata: {error}");
                }
                else
                {
                    Console.WriteLine(output);
                }
            }
        }


        /// </summary>

        private void InstallServices()
        {
            InstallService(@"C:\ProgramData\CaYaProtection\CaYaSafe\Services\CaYaSer\CYSADS.exe", "CYSADS", "CYSADS", ServiceStartMode.Manual);
            InstallService(@"C:\ProgramData\CaYaProtection\CaYaSafe\Services\CaYaPCCheck\CaYaControlSystem.exe", "CaYaControlSystem", "CaYaControlSystem", ServiceStartMode.Automatic);
            //InstallService(@"C:\ProgramData\CaYaProtection\CaYaSafe\Services\Start\CYSL.exe", "CYSL", "CYSL", ServiceStartMode.Automatic);
            //SetServiceRecoveryOptions("CaYaControlSystem");
        }

        private void InstallService(string exePath, string serviceName, string displayName, ServiceStartMode startMode)
        {
            string startModeString = startMode == ServiceStartMode.Automatic ? "auto" : "demand";

            // Servisi oluştur
            ProcessStartInfo psi = new ProcessStartInfo("sc.exe")
            {
                Arguments = $"create {serviceName} binPath= \"{exePath}\" DisplayName= \"{displayName}\" start= {startModeString}",
                UseShellExecute = false,
                CreateNoWindow = true
            };

            Process process = new Process
            {
                StartInfo = psi
            };
            process.Start();
            process.WaitForExit();

            // Servis hata durumunda bilgisayarı yeniden başlatacak şekilde ayarla
            ProcessStartInfo failurePsi = new ProcessStartInfo("sc.exe")
            {
                Arguments = $"failure {serviceName} reset= 0 actions= reboot/1000", // 5 saniye sonra yeniden başlat
                UseShellExecute = false,
                CreateNoWindow = true
            };

            Process failureProcess = new Process
            {
                StartInfo = failurePsi
            };
            failureProcess.Start();
            failureProcess.WaitForExit();
        }



        private void SetServiceRecoveryOptions(string serviceName)
        {
            ProcessStartInfo psi = new ProcessStartInfo("sc.exe")
            {
                Arguments = $"failure {serviceName} reset= 0 actions= restart/60000",
                UseShellExecute = false,
                CreateNoWindow = true
            };

            Process process = new Process
            {
                StartInfo = psi
            };
            process.Start();
            process.WaitForExit();
        }




    }
}
