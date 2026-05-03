using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PCDisableCY
{
    public partial class PCclosing : Form
    {
        [DllImport("user32.dll")]
        private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll")]
        private static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern IntPtr GetClassLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        private static extern IntPtr GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        private static extern IntPtr LoadImage(IntPtr hInst, string name, uint type, int width, int height, uint loadFlags);

        [DllImport("user32.dll")]
        private static extern bool DestroyIcon(IntPtr hIcon);

        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        const int GCL_HICON = -14; // Pencere simgesi için
        const uint IMAGE_ICON = 1;
        const uint LR_LOADTRANSPARENT = 0x00000020;
        const uint LR_LOADMAP3DCOLORS = 0x00001000;

        private System.Windows.Forms.Timer countdownTimer;
        private int countdownValue = 10; // Geri sayım değeri

        public PCclosing()
        {
            InitializeComponent();
        }

        private void PCclosing_Load(object sender, EventArgs e)
        {
            TopMost = true;
            TopMost = true;
            TopMost = true;
            Focus();
            Select();
            button1.Select();
            // ListView'i formun tam ortasına hizala
            listView1.Location = new Point(
                (this.ClientSize.Width - listView1.Width) / 2, // Yatay merkez
                (this.ClientSize.Height - listView1.Height) / 2 - 50 // Dikey merkez, 50 piksel yukarı al
            );
            listView1.Items.Clear(); // Önceki öğeleri temizle
            listView1.LargeImageList = new ImageList();
            listView1.LargeImageList.ImageSize = new Size(32, 32); // Simge boyutu
            EnumWindows(new EnumWindowsProc(EnumWindow), IntPtr.Zero);

            // Timer'ı oluştur ve yapılandır
            countdownTimer = new System.Windows.Forms.Timer();
            countdownTimer.Interval = 1000; // 1 saniye
            countdownTimer.Tick += CountdownTimer_Tick;

            // Geri sayımı başlat
            countdownTimer.Start();

            // Geri sayım değeri label'da göster
            label1.Text = countdownValue.ToString();
        }

        private void CountdownTimer_Tick(object sender, EventArgs e)
        {
            countdownValue--; // Geri sayım değerini azalt

            // Değeri label'a yaz
            label1.Text = countdownValue.ToString();

            // Eğer geri sayım 0 ise formu kapat
            if (countdownValue <= 0)
            {
                countdownTimer.Stop(); // Timer'ı durdur
                this.Close(); // Formu kapat
            }
        }

        private bool EnumWindow(IntPtr hWnd, IntPtr lParam)
        {
            if (IsWindowVisible(hWnd))
            {
                StringBuilder windowText = new StringBuilder(256);
                GetWindowText(hWnd, windowText, windowText.Capacity);

                if (!string.IsNullOrEmpty(windowText.ToString()))
                {
                    // Pencere simgesini al
                    IntPtr hIcon = GetClassLong(hWnd, GCL_HICON);
                    if (hIcon != IntPtr.Zero)
                    {
                        Icon icon = Icon.FromHandle(hIcon);
                        // Simgeyi boyutlandır
                        using (Bitmap bmp = icon.ToBitmap())
                        {
                            Bitmap resizedBmp = new Bitmap(bmp, new Size(32, 32)); // 32x32 boyutuna yeniden boyutlandır
                            listView1.LargeImageList.Images.Add(resizedBmp); // Simgeyi ekle
                            listView1.Items.Add(new ListViewItem(windowText.ToString(), listView1.LargeImageList.Images.Count - 1));
                        }
                    }
                }
            }
            return true; // Diğer pencereleri listelemeye devam et
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Close();

            //SharedData.Locked = true;
            ////Hide();
            ////LockScreen ls = new LockScreen();
            ////ls.Show();
            //LockScreen.Instance3.Show();
            //Hide();
            //Close();
            //GC.Collect();
            //Dispose();
        }

        private void PCclosing_FormClosing(object sender, FormClosingEventArgs e)
        {
            //e.Cancel = true;
        }
    }
}
