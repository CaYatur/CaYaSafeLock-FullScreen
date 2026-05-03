using Microsoft.Win32;
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

namespace CaYaSafeLockSetup
{
    public partial class Remove : Form
    {
        private bool Closing = true;
        private bool Uninstalling = false;
        public Remove()
        {
            InitializeComponent();
        }

        private void Remove_FormClosing(object sender, FormClosingEventArgs e)
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

        private void Remove_Load(object sender, EventArgs e)
        {

        }

        private async void button1_Click(object sender, EventArgs e)
        {
            button1.Enabled = false;
            Closing = false;
            CYRemovePrepare crp = new CYRemovePrepare();
            crp.Show();

            string programName = "PCDisableCY";


            Thread bgThread = new Thread(() =>
            {
                while (true)
                {
                    if (!Process.GetProcessesByName(programName).Any())
                    {
                        Console.WriteLine($"{programName} çalışmıyor. İşlem gerçekleştiriliyor...");
                        try
                        {
                            this.BeginInvoke((MethodInvoker)delegate
                            {
                                if (Uninstalling == false)
                                {
                                    Uninstalling = true;
                                    Uninstall();
                                }
                                    
                            });
                            
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Hata meydana geldi! {ex.Message}");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"{programName} zaten çalışıyor.");
                    }

                    Thread.Sleep(3000);
                }
            })
            {
                IsBackground = true
            };
            bgThread.Start();
            
            
        }



        private async void Uninstall()
        {
            await Task.Delay(8500);
            progressBar1.Value = 0;
            progressBar1.Maximum = 6; // Adım sayısı

            try
            {
                // Kayıt defteri ayarını geri al
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon", true))
                {
                    if (key != null)
                    {
                        key.SetValue("USERINIT", @"C:\Windows\system32\userinit.exe,");
                    }
                }
                progressBar1.Value++;
            }
            catch
            {
                MessageBox.Show("Kayıt defteri ayarını geri alırken bir hata oluştu.");
            }

            await Task.Delay(500);
            try
            {
                // Görev zamanlayıcıdan görevi kaldır
                ProcessStartInfo psi = new ProcessStartInfo("schtasks.exe")
                {
                    Arguments = "/Delete /TN \"CaYaSafeSTARTUP\" /F",
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                Process process = new Process
                {
                    StartInfo = psi
                };
                process.Start();
                process.WaitForExit();
                progressBar1.Value++;
            }
            catch
            {
                MessageBox.Show("Görev zamanlayıcıdan görevi kaldırırken bir hata oluştu.");
            }
            await Task.Delay(500);

            try
            {
                // Servisleri kaldır
                RemoveService("CYSADS");
                progressBar1.Value++;
            }
            catch
            {
                MessageBox.Show("CYSADS servisini kaldırırken bir hata oluştu.");
            }
            await Task.Delay(500);

            try
            {
                RemoveService("CaYaControlSystem");
                progressBar1.Value++;
            }
            catch
            {
                MessageBox.Show("CaYaControlSystem servisini kaldırırken bir hata oluştu.");
            }

            //try
            //{
            //    RemoveService("CYSL");
            //    progressBar1.Value++;
            //}
            //catch
            //{
            //    MessageBox.Show("CYSL servisini kaldırırken bir hata oluştu.");
            //}

            await Task.Delay(500);

            try
            {
                // Kopyalanan dosyaları sil
                string destinationPath1 = @"C:\Users\Default\AppData\Roaming\CYSYSTEM";
                string destinationPath2 = @"C:\ProgramData\CaYaProtection";
                try
                {
                    Directory.Delete(destinationPath1, true);
                    progressBar1.Value++;
                }
                catch
                {
                    progressBar1.Value++;
                }

                try
                {
                    Directory.Delete(destinationPath2, true);
                    progressBar1.Value++;
                }
                catch
                {
                    progressBar1.Value++;
                }
                /////////////////////////////EKSTRA ALAN!!!!!!!!!!!///♣♣♣ DOWN
                ///await Task.Delay(500);
                ///
                await Task.Delay(500);
                try
                {
                    Directory.Delete(destinationPath1, true);
                
                }
                catch
                {

                }

                try
                {
                    Directory.Delete(destinationPath2, true);

                }
                catch
                {

                }
                /////////////////////////////EKSTRA ALAN!!!!!!!!!!!///♣♣♣ UP

            }
            catch
            {
                MessageBox.Show("Kopyalanan dosyaları silerken bir hata oluştu.");
            }

            //MessageBox.Show("Kaldırma işlemi tamamlandı.");
            Hide();
            FinishUninstall fui = new FinishUninstall();
            fui.ShowDialog();
        }

        private void RemoveService(string serviceName)
        {
            try
            {
                // İlk olarak servisi durdurmak için "sc stop" komutunu çalıştırıyoruz
                ProcessStartInfo stopPsi = new ProcessStartInfo("sc.exe")
                {
                    Arguments = $"stop {serviceName}",
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                Process stopProcess = new Process
                {
                    StartInfo = stopPsi
                };
                stopProcess.Start();
                stopProcess.WaitForExit(); // Servisin durmasını bekle

                // Servisi durdurduktan sonra, "sc delete" komutunu çalıştırarak servisi siliyoruz
                ProcessStartInfo deletePsi = new ProcessStartInfo("sc.exe")
                {
                    Arguments = $"delete {serviceName}",
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                Process deleteProcess = new Process
                {
                    StartInfo = deletePsi
                };
                deleteProcess.Start();
                deleteProcess.WaitForExit(); // Silme işlemini bekle
            }
            catch
            {
                MessageBox.Show($"Servis {serviceName} kaldırılırken bir hata oluştu.");
            }
        }




    }
}
