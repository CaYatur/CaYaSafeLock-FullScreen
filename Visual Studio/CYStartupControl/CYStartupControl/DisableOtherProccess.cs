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
    public partial class DisableOtherProccess : Form
    {
        private List<string> allowedProcesses;
        private System.Windows.Forms.Timer processChecker;
        public DisableOtherProccess()
        {
            InitializeComponent();
            InitializeProcessChecker();
        }

        private void DisableOtherProccess_Load(object sender, EventArgs e)
        {
            // Mevcut işlemleri al
            allowedProcesses = Process.GetProcesses().Select(p => p.ProcessName).ToList();

            // Zamanlayıcıyı başlat
            processChecker.Start();
        }

        private void InitializeProcessChecker()
        {
            processChecker = new System.Windows.Forms.Timer();
            processChecker.Interval = 350; // 5 saniyede bir kontrol et
            processChecker.Tick += ProcessChecker_Tick;
        }

        private void ProcessChecker_Tick(object sender, EventArgs e)
        {
            var currentProcesses = Process.GetProcesses();

            foreach (var process in currentProcesses)
            {
                if (!allowedProcesses.Contains(process.ProcessName))
                {
                    try
                    {
                        process.Kill();
                    }
                    catch (Exception ex)
                    {
                        // İşlem kapatılamadıysa hata yakalayın
                        Console.WriteLine($"Failed to kill process {process.ProcessName}: {ex.Message}");
                    }
                }
            }
        }

        private void DisableOtherProccess_Shown(object sender, EventArgs e)
        {
            Hide();
        }

        private void DisableOtherProccess_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
        }
    }
}
