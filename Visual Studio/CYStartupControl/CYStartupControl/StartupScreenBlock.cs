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
using System.Net;
using System.Runtime.InteropServices;


namespace CYStartupControl
{
    public partial class StartupScreenBlock : Form
    {
        private string filePath = @"C:\Users\Default\AppData\Roaming\CYSYSTEM\CYdata\Dat.sf";

        private const int WS_EX_NOACTIVATE = 0x08000000;
        private const int WS_EX_TOOLWINDOW = 0x00000080;
        private const int WS_VISIBLE = 0x10000000;

        [DllImport("user32.dll")]
        private static extern IntPtr SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        private static extern bool IsWindowVisible(IntPtr hWnd);


        public StartupScreenBlock()
        {
            InitializeComponent();
        }

        private void StartupScreenBlock_Load(object sender, EventArgs e)
        {
            //this.WindowState = FormWindowState.Maximized;
            //this.FormBorderStyle = FormBorderStyle.None; // Formun kenarlık stilini kaldırır
            //this.ShowInTaskbar = false; // Görev çubuğunda gösterme


            // Form penceresine uygun stilleri uygula
            int extendedStyle = GetWindowLong(this.Handle, -20);
            SetWindowLong(this.Handle, -20, extendedStyle | WS_EX_TOOLWINDOW | WS_EX_NOACTIVATE | WS_VISIBLE);

            // Formu tüm masaüstleri üzerinde göster
            IntPtr desktopHandle = FindWindow("Progman", "Program Manager");
            SetWindowPos(this.Handle, new IntPtr(-1), 0, 0, 0, 0, 0x0001 | 0x0040 | 0x0002);

            ProcessModule objCurrentModule = Process.GetCurrentProcess().MainModule;
            objKeyboardProcess = new LowLevelKeyboardProc(captureKey);
            ptrHook = SetWindowsHookEx(13, objKeyboardProcess, GetModuleHandle(objCurrentModule.ModuleName), 0);

            CheckFile();
            CheckCaYaLock();
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct KBDLLHOOKSTRUCT
        {
            public Keys key;
            public int scanCode;
            public int flags;
            public int time;
            public IntPtr extra;
        }

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int id, LowLevelKeyboardProc callback, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool UnhookWindowsHookEx(IntPtr hook);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hook, int nCode, IntPtr wp, IntPtr lp);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string name);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern short GetAsyncKeyState(Keys key);

        private IntPtr ptrHook;
        private LowLevelKeyboardProc objKeyboardProcess;

        private IntPtr captureKey(int nCode, IntPtr wp, IntPtr lp)
        {
            if (nCode >= 0)
            {
                KBDLLHOOKSTRUCT objKeyInfo = (KBDLLHOOKSTRUCT)Marshal.PtrToStructure(lp, typeof(KBDLLHOOKSTRUCT));

                // Windows tuşlarını ve Alt+Tab, Ctrl+Escape kombinasyonlarını devre dışı bırak
                if (objKeyInfo.key == Keys.RWin || objKeyInfo.key == Keys.LWin || objKeyInfo.key == Keys.Tab && HasAltModifier(objKeyInfo.flags) || objKeyInfo.key == Keys.Escape && (ModifierKeys & Keys.Control) == Keys.Control)
                {
                    return (IntPtr)1;
                }
            }
            return CallNextHookEx(ptrHook, nCode, wp, lp);
        }

        private bool HasAltModifier(int flags)
        {
            return (flags & 0x20) == 0x20;
        }

        private void StartupScreenBlock_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
        }

        private void CheckCaYaLock()
        {
            Thread bgThread = new Thread(() =>
            {
                while (true)
                {
                    string windowTitle = "LockScreen"; // Hedef başlık
                    IntPtr hWnd = FindWindow(null, windowTitle);

                    if (hWnd != IntPtr.Zero && IsWindowVisible(hWnd))
                    {
                        Console.WriteLine("Pencere bulundu: " + windowTitle);
                        Process[] processes = Process.GetProcessesByName("PCDisableCY");
                        if (processes.Length > 0)
                        {
                            Console.WriteLine("İlgili işlem bulundu: " + processes[0].ProcessName);
                            Environment.Exit(0);
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
            })
            {
                IsBackground = true
            };
            bgThread.Start();
        }






        private void CheckFile()
        {
            try
            {
                // Dosya mevcut mu kontrol et
                if (File.Exists(filePath))
                {
                    Console.WriteLine("Dat.sf dosyası mevcut.");
                    pictureBox1.Visible = true;
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

        private void StartupScreenBlock_Shown(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                while (true)
                {
                    if (InvokeRequired)
                    {
                        Invoke(new Action(() =>
                        {

                            // Her tick ile saati güncelle
                            FocusMet();

                        }));
                    }
                    else
                    {
                        // Her tick ile saati güncelle
                        FocusMet();
                    }
                }
                
            });

            //await Task.Delay(10000);
            //Environment.Exit(0);
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        private void FocusMet()
        {
            TopMost = true; // Formunuzu üstte tutun
            this.Show(); // Formu gösterin (eğer gizli ise)
            this.Activate(); // Formu aktif hale getirin
            SetForegroundWindow(this.Handle); // Formu ön plana getirin
        }

        private void label1_Click(object sender, EventArgs e)
        {
            //Environment.Exit(0);
        }
    }
}
