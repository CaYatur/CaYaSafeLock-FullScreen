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

namespace PCDisableCY
{
    public partial class LockSystem : Form
    {
        public LockSystem()
        {
            InitializeComponent();
        }



        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn
        (
            int nLeftRect,     // x-coordinate of upper-left corner
            int nTopRect,      // y-coordinate of upper-left corner
            int nRightRect,    // x-coordinate of lower-right corner
            int nBottomRect,   // y-coordinate of lower-right corner
            int nWidthEllipse, // height of ellipse
            int nHeightEllipse // width of ellipse
        );

        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GlobalMemoryStatusEx(ref MEMORYSTATUSEX lpBuffer);

        private void LockSystem_Load(object sender, EventArgs e)
        {
            Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, 10, 10));
            SetFormLocationToBottomRight();
            //GC.Collect();
            //GC.WaitForPendingFinalizers();

            ClearMemory();


            Thread bgThread = new Thread(() =>
            {


                while (true)
                {
                    try
                    {
                        if (!this.IsDisposed)
                        {
                            Invoke(new MethodInvoker(() =>
                            {
                                TopLevel = true;
                                TopMost = true;
                                //Focus();
                            }));
                        }
                    }
                    catch (Exception ex)
                    {
                        NotifyERR(ex.Message);
                    }



                    System.Threading.Thread.Sleep(1);
                }


            });
            //bgThread.IsBackground = true;
            //bgThread.Start();

            TopLevel = true;
            TopMost = true;
            TopLevel = true;
            TopMost = true;

            Main.CloseSystem += Remove;
            //NotifyERR("Test");
        }
        private void Remove(object sender, EventArgs e)
        {
            // Form2'deki işlem tamamlandığında burası çalışır
            // Gerekirse başka işlemler gerçekleştirilebilir

            if (!this.IsDisposed)
            {
                Invoke(new MethodInvoker(() =>
                {
                    Hide();
                    GC.Collect();
                }));
            }

        }






        private void NotifyERR(string errorMessage)
        {
            // Bildirim simgesine ait metinleri ayarlama
            Notif.BalloonTipTitle = "CaYaSafe SİSTEMSEL HATA!";
            Notif.BalloonTipText = "Bilinmeyen bir nedenle hata oluştu! Detaylar için tıklayınız.";
            Notif.BalloonTipIcon = ToolTipIcon.Error;
            Notif.Visible = true;

            // Kullanıcıya gösterilecek hata mesajını bildirim simgesine ekleyin
            Notif.Tag = errorMessage;
            Notif.ShowBalloonTip(100);
        }

        private void Notif_Click(object sender, EventArgs e)
        {
            // Bildirim simgesine tıklandığında yapılacak işlemler
            NotifyIcon notifyIcon = (NotifyIcon)sender;
            string errorMessage = (string)notifyIcon.Tag;

            // Hata mesajını göstermek için yeni bir form oluşturun
            ErrorDetailsForm errorForm = new ErrorDetailsForm(errorMessage);
            errorForm.ShowDialog();
        }

        static void ClearMemory()
        {
            MEMORYSTATUSEX memStatus = new MEMORYSTATUSEX();
            memStatus.dwLength = (uint)Marshal.SizeOf(typeof(MEMORYSTATUSEX));
            GlobalMemoryStatusEx(ref memStatus);
            int availablePhysicalMemory = (int)memStatus.ullAvailPhys;

            // Belleği serbest bırakma
            EmptyWorkingSet(Process.GetCurrentProcess().Handle);
        }

        [DllImport("psapi.dll")]
        static extern bool EmptyWorkingSet(IntPtr hProcess);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        struct MEMORYSTATUSEX
        {
            public uint dwLength;
            public uint dwMemoryLoad;
            public ulong ullTotalPhys;
            public ulong ullAvailPhys;
            public ulong ullTotalPageFile;
            public ulong ullAvailPageFile;
            public ulong ullTotalVirtual;
            public ulong ullAvailVirtual;
            public ulong ullAvailExtendedVirtual;
            public void Init()
            {
                this.dwLength = (uint)Marshal.SizeOf(typeof(MEMORYSTATUSEX));
            }
        }

        private void SetFormLocationToBottomRight()
        {
            // Ekranın sağ alt köşesinin koordinatlarını al
            int screenWidth = Screen.PrimaryScreen.WorkingArea.Width;
            int screenHeight = Screen.PrimaryScreen.WorkingArea.Height;

            // Formun boyutunu ve konumunu ayarla
            //this.Width = 164; // Formun genişliğini isteğinize göre ayarlayabilirsiniz
            //this.Height = 260; // Formun yüksekliğini isteğinize göre ayarlayabilirsiniz

            int formX = screenWidth - this.Width;
            int formY = screenHeight - this.Height;

            this.Location = new System.Drawing.Point(formX, formY);
        }

        private void Lock_Click(object sender, EventArgs e)
        {
            try
            {
                Closing = true;
                //Hide();
                Close();
                GC.Collect();
                LockScreen ls = new LockScreen();
                ls.Show();
            }
            catch (Exception ex)
            {
                NotifyERR(ex.Message);
            }



        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }


        private bool Drag = false;
        private Point lastLocation;

        private void pictureBox2_MouseDown(object sender, MouseEventArgs e)
        {
            Drag = true;
            lastLocation = e.Location;
        }

        private void pictureBox2_MouseMove(object sender, MouseEventArgs e)
        {
            if (Drag)
            {
                int deltaX = e.X - lastLocation.X;
                int deltaY = e.Y - lastLocation.Y;
                int newX = this.Location.X + deltaX;
                int newY = this.Location.Y + deltaY;

                // Ekranın kenarlarını kontrol et
                Screen screen = Screen.FromControl(this);
                Rectangle workingArea = screen.WorkingArea;

                newX = Math.Max(workingArea.Left, Math.Min(newX, workingArea.Right - this.Width));
                newY = Math.Max(workingArea.Top, Math.Min(newY, workingArea.Bottom - this.Height));

                this.Location = new Point(newX, newY);
            }

        }

        private void pictureBox2_MouseUp(object sender, MouseEventArgs e)
        {
            Drag = false;
        }

        private void pictureBox4_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private bool Closing = false;
        private void LockSystem_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (Closing == false)
            {
                e.Cancel = true;
            }
            else
            {
                e.Cancel = false;
                GC.Collect();
                GC.WaitForPendingFinalizers();
                Dispose();
            }

        }
    }
}
