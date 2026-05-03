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
using static System.Windows.Forms.DataFormats;
using System.ServiceProcess;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Button;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Reflection.Emit;
using System.Timers;
using Microsoft.VisualBasic.ApplicationServices;
using System.Threading;
using System.Runtime.Intrinsics.Arm;


namespace PCDisableCY
{
    public partial class Kilit : Form
    {

        private static settings instance;

        public static settings Instance
        {
            get
            {
                if (instance == null || instance.IsDisposed)
                {
                    instance = new settings();
                }
                return instance;
            }
        }





        private static Kilit instance2;

        public static Kilit Instance2
        {
            get
            {
                if (instance2 == null || instance.IsDisposed)
                {
                    instance2 = new Kilit();
                }
                return instance2;
            }
        }



        private static LockScreen instance3;

        public static LockScreen Instance3
        {
            get
            {
                if (instance3 == null || instance3.IsDisposed)
                {
                    instance3 = new LockScreen();
                }
                return instance3;
            }
        }

        private static Kilit instance4;

        public static Kilit Instance4
        {
            get
            {
                if (instance4 == null || instance4.IsDisposed)
                {
                    instance4 = new Kilit();
                }
                return instance4;
            }
        }


        private bool Drag = false;
        private Point lastLocation;


        private void Kilit_MouseDown(object sender, MouseEventArgs e)
        {
            Drag = true;
            lastLocation = e.Location;
        }

        private void Kilit_MouseMove(object sender, MouseEventArgs e)
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

        private void Kilit_MouseUp(object sender, MouseEventArgs e)
        {
            Drag = false;
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






        public static event EventHandler RequestEvent2;
        public static event EventHandler Checkstatus;
        public static event EventHandler Focus;


        private float initialDpiX;
        private float initialDpiY;


        public Kilit()
        {

            InitializeComponent();

            // İlk DPI değerlerini kaydet(örn. 96 DPI referans alınır)
            using (Graphics g = this.CreateGraphics())
            {
                initialDpiX = g.DpiX;
                initialDpiY = g.DpiY;
            }



            // Form yeniden boyutlandırıldığında da ölçeklendirme yapılacak
            this.Resize += DpiAwareForm_Resize;

            this.ControlBox = false;

            Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, 10, 10));
            // Panelin köşelerini yuvarla
            //PanelColor.Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, PanelColor.Width, PanelColor.Height, 5, 5));
            SetFormLocationToBottomRight();

            //settings sett = new settings();
            //sett.Show();
            TopMost = true;
            settings.Instance.Show();
            settings.Instance.Hide();

            Checkstatus?.Invoke(this, EventArgs.Empty);

            // Form2'den gelen olaya abone olun.
            settings.RequestEvent += HandleForm2Request;
            settings.RequestEventCloseCY += HandleFormRequest2;
            //settings.RequestEvent2 += HandleForm3Request;



            GC.Collect();


            //EdgeBugRemoval();

            //notifyIcon1.Text = "Teas";
            //notifyIcon1.Visible = true;
            //notifyIcon1.BalloonTipTitle = "CaYaSafe";
            //notifyIcon1.BalloonTipText = "Kilit açıldı!";
            //notifyIcon1.ShowBalloonTip(100);
        }


        private async void EdgeBugRemoval()
        {
            //Thread bgThread = new Thread(() =>
            //{
            //    while (true)
            //    {
            //        // İlgili işlemler
            //        this.Invoke((MethodInvoker)delegate
            //        {
            //            // İşlem yapılacak kodlar

            //            Rectangle screenBounds = Screen.FromControl(this).Bounds; // Formun bulunduğu ekranın boyutunu alıyoruz
            //            int edgeProximityThreshold = -50; // Kenara olan mesafe eşiği (pixel cinsinden)

            //            int leftDistance = this.Left; // Sol kenara olan mesafe
            //            int topDistance = this.Top; // Üst kenara olan mesafe
            //            int rightDistance = screenBounds.Width - (this.Left + this.Width); // Sağ kenara olan mesafe
            //            int bottomDistance = screenBounds.Height - (this.Top + this.Height); // Alt kenara olan mesafe
            //            GC.Collect();
            //            // Eğer formun herhangi bir köşesi ekranın kenarına yeterince yakınsa işlem gerçekleştir
            //            if (leftDistance < edgeProximityThreshold ||
            //                topDistance < edgeProximityThreshold ||
            //                rightDistance < edgeProximityThreshold ||
            //                bottomDistance < edgeProximityThreshold)
            //            {
            //                //MessageBox.Show("Form ekranın bir kenarına çok yakın!");

            //                SetFormLocationToBottomRight();
            //                GC.Collect();
            //            }
            //        });

            //        Thread.Sleep(5000); // 5 saniye bekletme
            //        GC.Collect();
            //    }

            //});

            //await Task.Delay(5000);

            //bgThread.IsBackground = true;
            //bgThread.Start();
        }











        private const int MinWidth = 225;
        private const int MinHeight = 300;
        private const int MaxWidth = 225;
        private const int MaxHeight = 300;

        // WM_SIZE mesajını ele almak için override edilmiş method
        protected override void WndProc(ref Message m)
        {
            const int WM_SIZE = 0x0005;

            if (m.Msg == WM_SIZE)
            {
                // Yeni boyutu al
                int newWidth = (int)(m.LParam.ToInt64() & 0xFFFF);
                int newHeight = (int)(m.LParam.ToInt64() >> 16);

                // Minimum genişlik kontrolü
                if (newWidth < MinWidth)
                {
                    m.LParam = (IntPtr)((newHeight << 16) | MinWidth);
                }

                // Minimum yükseklik kontrolü
                if (newHeight < MinHeight)
                {
                    m.LParam = (IntPtr)((MinHeight << 16) | newWidth);
                }

                // Maksimum genişlik kontrolü
                if (newWidth > MaxWidth)
                {
                    m.LParam = (IntPtr)((newHeight << 16) | MaxWidth);
                }

                // Maksimum yükseklik kontrolü
                if (newHeight > MaxHeight)
                {
                    m.LParam = (IntPtr)((MaxHeight << 16) | newWidth);
                }
            }

            // Ana sınıfın WndProc metodunu çağır
            base.WndProc(ref m);
        }






        private void HandleForm2Request(object sender, EventArgs e)
        {
            // Form2'den gelen isteği işleyin.
            // Bu metot, Form2'de bir buton tıklandığında çağrılacak.
            //MessageBox.Show("Form2'den istek alındı ve işlem gerçekleştirildi.");

            KontrolEtVeIslemYap();
        }

        private void HandleFormRequest2(object sender, EventArgs e)
        {
            LockingSystem();
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




        private async void button1_Click(object sender, EventArgs e)
        {


        }









        // Form2 içinde Form1'e erişim sağlayın
        //Form1 form1 = Application.OpenForms["Form1"] as Form1;
        //Lock locking = Application.OpenForms["locking"] as Lock;
        private async void Kilit_Load(object sender, EventArgs e)
        {
            SharedData.Locked = false;

            Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, 10, 10));
            SetFormLocationToBottomRight();
            GC.Collect();
            GC.WaitForPendingFinalizers();

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




            KontrolEtVeIslemYap();




        }


        private void DpiAwareForm_Resize(object sender, EventArgs e)
        {
            // Form boyutlandırıldığında veya DPI değişikliğinde ölçeklendir
            AdjustFormScale();
        }

        private void AdjustFormScale()
        {
            // Geçerli DPI değerini al
            using (Graphics g = this.CreateGraphics())
            {
                float currentDpiX = g.DpiX;
                float currentDpiY = g.DpiY;

                // DPI farkını hesapla
                float scaleFactorX = currentDpiX / initialDpiX;
                float scaleFactorY = currentDpiY / initialDpiY;

                // Sadece formu ölçeklendir (global değişiklik yapmadan)
                this.Scale(new SizeF(scaleFactorX, scaleFactorY));

                // Formun içindeki kontrolleri de ölçeklendir
                foreach (Control control in this.Controls)
                {
                    control.Scale(new SizeF(scaleFactorX, scaleFactorY));

                    // Yazı tipi de ölçeklendirilsin
                    control.Font = new Font(control.Font.FontFamily, control.Font.Size * scaleFactorX, control.Font.Style);
                }
            }
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


        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GlobalMemoryStatusEx(ref MEMORYSTATUSEX lpBuffer);
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



        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == (Keys.Alt | Keys.F4))
            {
                //DialogResult result = MessageBox.Show("Bu Bilgisayar KİTLENMİŞTİR!", "CaYa©", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                // Alt+F4 tuş kombinasyonunu engelle
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Minimized;
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void ShowSettingsForm()
        {
            // Eğer settings formu zaten açıksa
            if (Application.OpenForms.OfType<settings>().Any())
            {
                // Formu öne çıkar
                settings.Instance.BringToFront();
                settings.Instance.TopMost = true;
                settings.Instance.TopMost = false;
            }
            else
            {
                // Formu aç
                settings.Instance.Show();
            }
        }

        private async void pictureBox3_Click(object sender, EventArgs e)
        {
            //settings.Instance.Show();
            //settings.Instance.TopMost = true;
            //await Task.Delay(500);
            //settings.Instance.TopMost = false;
            //ShowSettingsForm();
            //TopMost = false;
            settings.Instance.Show();
            //settings.Instance.BringToFront();
            settings.Instance.TopMost = true;
            settings.Instance.TopMost = false;
            settings.Instance.Enabled = true;

            RequestEvent2?.Invoke(this, EventArgs.Empty);

            await Task.Delay(5000);
            //TopMost = true;
        }


        private async void pictureBox12_Click(object sender, EventArgs e)
        {
            settings.Instance.Show();
            settings.Instance.TopMost = true;
            settings.Instance.TopMost = false;
            settings.Instance.Enabled = true;

            RequestEvent2?.Invoke(this, EventArgs.Empty);

            await Task.Delay(5000);
        }


        private async void pictureBox13_Click(object sender, EventArgs e)
        {
            settings.Instance.Show();
            settings.Instance.TopMost = true;
            settings.Instance.TopMost = false;
            settings.Instance.Enabled = true;

            RequestEvent2?.Invoke(this, EventArgs.Empty);

            await Task.Delay(5000);
        }


        private bool Closing = false;
        private void Kilit_FormClosing(object sender, FormClosingEventArgs e)
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

        private void pictureBox7_Click(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Minimized;
        }

        private void pictureBox6_Click(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Minimized;
        }

        private async void pictureBox5_Click(object sender, EventArgs e)
        {
            LockingSystem();
        }












        private void pictureBox11_Click(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Minimized;
        }

        private void pictureBox9_Click(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Minimized;
        }

        private async void pictureBox8_Click(object sender, EventArgs e)
        {
            LockingSystem();
        }




        private void pictureBox16_Click(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Minimized;
        }

        private void pictureBox14_Click(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Minimized;
        }

        private async void pictureBox15_Click(object sender, EventArgs e)
        {

            LockingSystem();

        }



        private async void LockingSystem()
        {
            try
            {
                SharedData.Locked = true;
                Closing = true;
                //Hide();
                //LockScreen ls = new LockScreen();
                //ls.Show();
                LockScreen.Instance3.Show();
                Focus?.Invoke(this, EventArgs.Empty);
                Hide();
                Close();
                GC.Collect();
                Dispose();               
            }
            catch (Exception ex)
            {
                NotifyERR(ex.Message);
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


        private const string themeDosyaAdi = "theme.txt";
        private void ThemeOnR()
        {

            pictureBox6.Visible = true;
            pictureBox7.Visible = true;
            pictureBox18.Visible = true;
            pictureBox3.Visible = true; //PC
            pictureBox5.Visible = true;
            pictureBox4.Visible = true;

        }


        private void ThemeOnB()
        {

            pictureBox11.Visible = true;
            pictureBox22.Visible = true;
            pictureBox12.Visible = true; //PC
            pictureBox8.Visible = true;
            pictureBox9.Visible = true;
            pictureBox10.Visible = true;





            pictureBox8.BackColor = ColorTranslator.FromHtml("#191919");
            pictureBox9.BackColor = ColorTranslator.FromHtml("#191919");
            pictureBox10.BackColor = ColorTranslator.FromHtml("#191919");
            pictureBox11.BackColor = ColorTranslator.FromHtml("#191919");
            pictureBox12.BackColor = ColorTranslator.FromHtml("#191919");
            pictureBox22.BackColor = ColorTranslator.FromHtml("#191919");
        }


        private void ThemeOnW()
        {
            pictureBox17.Visible = true;
            pictureBox16.Visible = true;
            pictureBox15.Visible = true;
            pictureBox14.Visible = true;
            pictureBox13.Visible = true;
            pictureBox21.Visible = true;

            pictureBox17.BackColor = ColorTranslator.FromHtml("#e6e6e6");
            pictureBox16.BackColor = ColorTranslator.FromHtml("#e6e6e6");
            pictureBox15.BackColor = ColorTranslator.FromHtml("#e6e6e6");
            pictureBox14.BackColor = ColorTranslator.FromHtml("#e6e6e6");
            pictureBox13.BackColor = ColorTranslator.FromHtml("#e6e6e6");
            pictureBox21.BackColor = ColorTranslator.FromHtml("#e6e6e6");
        }










        private void KontrolEtVeIslemYap()
        {
            // Dosyadan tema değerini oku
            if (File.Exists(themeDosyaAdi))
            {
                string temaDegeriStr = File.ReadAllText(themeDosyaAdi);

                // Tema değerini int'e çevir
                if (int.TryParse(temaDegeriStr, out int temaDegeri))
                {
                    // Tema değerine göre ilgili işlemi gerçekleştir
                    switch (temaDegeri)
                    {
                        case 1:
                            ThemeOn1();
                            break;
                        case 2:
                            ThemeOn2();
                            break;
                        case 3:
                            ThemeOn3();
                            break;
                        default:
                            // Bilinmeyen tema değeri, varsayılan işlemi gerçekleştir
                            ThemeOn1();
                            break;
                    }
                }
                else
                {
                    // Geçersiz tema değeri, varsayılan işlemi gerçekleştir
                    ThemeOn1();
                }
            }
            else
            {
                // Dosya bulunamadı, varsayılan işlemi gerçekleştir
                ThemeOn1();
            }
        }


        private void YazThemeDosyasi(int temaDegeri)
        {
            // Tema dosyasına belirtilen değeri yaz
            try
            {
                using (StreamWriter writer = new StreamWriter(themeDosyaAdi))
                {
                    writer.Write(temaDegeri);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Tema dosyasına yazılırken hata oluştu: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void ThemeOn1()
        {
            ThemeOff2();
            ThemeOff3();

            ThemeOnR();

        }
        private void ThemeOff1()
        {

            pictureBox6.Visible = false;
            pictureBox7.Visible = false;
            pictureBox3.Visible = false; //PC
            pictureBox5.Visible = false;
            pictureBox4.Visible = false;
            pictureBox18.Visible = false;

        }

        private async void ThemeOn2()
        {
            ThemeOff1();
            ThemeOff3();

            ThemeOnB();

        }



        private void ThemeOff2()
        {

            pictureBox8.Visible = false;
            pictureBox9.Visible = false;
            pictureBox10.Visible = false;
            pictureBox11.Visible = false;
            pictureBox12.Visible = false; //PC
            pictureBox22.Visible = false;

        }


        private async void ThemeOn3()
        {
            ThemeOff1();
            ThemeOff2();

            ThemeOnW();

        }

        private void ThemeOff3()
        {

            pictureBox17.Visible = false;
            pictureBox16.Visible = false;
            pictureBox15.Visible = false;
            pictureBox14.Visible = false;
            pictureBox13.Visible = false;
            pictureBox21.Visible = false;

        }




     


        private void Kilit_Shown(object sender, EventArgs e)
        {

            
        }
    }
}
