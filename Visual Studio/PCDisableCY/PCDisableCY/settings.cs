using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Button;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Diagnostics;
using System.ServiceProcess;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ToolBar;
using System.Runtime.ConstrainedExecution;
using static System.Windows.Forms.DataFormats;
using System.Diagnostics.Metrics;

namespace PCDisableCY
{
    public partial class settings : Form
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


        public static event EventHandler RequestEvent;
        public static event EventHandler RequestEventD;
        public static event EventHandler RequestEventCloseCY;


        private bool isTimerRunning = false;
        private int remainingTimeInSeconds;
        private System.Windows.Forms.Timer timer;



        
        public settings()
        {
            this.MinimizeBox = false;
            this.MaximizeBox = false;


            InitializeComponent();
            remainingTimeInSeconds = SharedData.TimerSystem;
            checkBox1.Checked = SharedData.TimerSystemEnabled;
            checkBox2.Checked = SharedData.TimerSystemEnabledForce;

            timer = new System.Windows.Forms.Timer(); // Timer'ın tam adını belirt
            timer.Interval = 1000;
            timer.Tick += Timer_Tick;


            if (checkBox1.Checked)
            {
                // CheckBox işaretlendiğinde Timer'ı başlat
                timer.Start();
            }
            else
            {
                // CheckBox işareti kaldırıldığında Timer'ı durdur
                timer.Stop();
            }

            // CheckBox durumuna göre Timer'ı başlat veya durdur
            checkBox1.CheckedChanged += (s, e) =>
            {
                if (checkBox1.Checked)
                {
                    // CheckBox işaretlendiğinde Timer'ı başlat
                    timer.Start();
                }
                else
                {
                    // CheckBox işareti kaldırıldığında Timer'ı durdur
                    timer.Stop();
                }
            };
            RequestEventD?.Invoke(this, EventArgs.Empty);
            

            Kilit.RequestEvent2 += HandleForm3Request;
            Kilit.Checkstatus += checkstatus;
            //EdgeBugRemoval();
        }


        private void checkstatus(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                if (timer.Enabled)
                {
                    return;
                }
                // CheckBox işaretlendiğinde Timer'ı başlat
                timer.Start();
            }
            else
            {
                // CheckBox işareti kaldırıldığında Timer'ı durdur
                timer.Stop();
            }
        }

        private void HandleForm3Request(object sender, EventArgs e)
        {

            // Formun boyutunu al
            int formWidth = this.Width;
            int formHeight = this.Height;

            // Ekranın boyutunu al
            Rectangle screenBounds = Screen.PrimaryScreen.Bounds;

            // Formun konumunu hesapla
            int x = (screenBounds.Width - formWidth) / 2;
            int y = (screenBounds.Height - formHeight) / 2;

            // Formun konumunu ayarla
            this.Location = new Point(x, y);

        }

        



        // Minimum ve maksimum boyutları belirleyin

        private const int MinWidth = 366;
        private const int MinHeight = 389;
        private const int MaxWidth = 366;
        private const int MaxHeight = 389;

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


     


        private async void Timer_Tick(object sender, EventArgs e)
        {
            await Task.Run(() =>
            {
                if (InvokeRequired)
                {
                    Invoke(new Action(() =>
                    {
                        TimerTickAsync();
                    }));
                }
                else
                {
                    TimerTickAsync();
                } 
            });                 
        }

        private void TimerTickAsync()
        {
            if (SharedData.Locked == false)
            {
                if (remainingTimeInSeconds > 0)
                {
                    remainingTimeInSeconds--;
                    UpdateRemainingTimeLabel();

                    if (remainingTimeInSeconds == 30)
                    {
                        ShowSettingsForm();
                        //TopMost = true;
                        //settings.Instance.Show();
                        //settings.Instance.TopMost = true;
                        //MessageBox.Show("Cihazın Kitlenmesine Kalan Süre Son 30 Saniye!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        //settings.Instance.TopMost = false;
                        warnforlocking.Instance8.Show();

                        TopMost = false;
                    }
                }
                else
                {
                    Locking();
                    //MessageBox.Show("Süre doldu!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    timer.Stop(); // Süre dolduğunda Timer'ı durdur
                }
            }
            else
            {
                timer.Stop();
                Locking();
            }
        }


        private void UpdateRemainingTimeLabel()
        {
            int minutes = remainingTimeInSeconds / 60;
            int seconds = remainingTimeInSeconds % 60;
            label1.Text = $"Kalan Süre: {minutes:D2}:{seconds:D2}";
        }

        private void ShowSettingsForm()
        {
            // Eğer settings formu zaten açıksa
            if (Application.OpenForms.OfType<settings>().Any())
            {
                // Formu öne çıkar
                settings.Instance.BringToFront();
            }
            else
            {
                // Formu aç
                settings.Instance.Show();
            }
        }


        private const string themeDosyaAdi = "theme.txt";
        

        private void settings_Load(object sender, EventArgs e)
        {
            KontrolEtVeIslemYap();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // Bu formun DPI farkına göre ölçeklenmesini devre dışı bırakır.
            this.AutoScaleMode = AutoScaleMode.None;

            // Varsayılan DPI değeri 96 olarak kabul edilir.
            using (Graphics g = this.CreateGraphics())
            {
                float dpiX = g.DpiX;
                float dpiY = g.DpiY;

                // DPI farkını dengelemek için formun boyutunu ayarlıyoruz.
                float scaleFactorX = dpiX / 96f;
                float scaleFactorY = dpiY / 96f;

                // Mevcut boyutu ile orijinal boyutu karşılaştırıyoruz.
                this.Width = (int)(this.Width * scaleFactorX);
                this.Height = (int)(this.Height * scaleFactorY);
            }


        }


        private void settings_FormClosed(object sender, FormClosedEventArgs e)
        {

        }

        private void settings_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            //settings.Instance.Hide();
            Hide();

        }

        private void button3_Click(object sender, EventArgs e)
        {
            // "5" girişi yapıldığında, kalan süreye 5 dakika ekleyelim
            remainingTimeInSeconds += 5 * 60;
            UpdateRemainingTimeLabel();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            // TextBox içine girilen sayıyı süre olarak güncelle
            if (int.TryParse(textBox1.Text, out int minutes))
            {
                remainingTimeInSeconds = minutes * 60;
                UpdateRemainingTimeLabel();
            }
            else
            {
                //MessageBox.Show("Geçerli bir sayı giriniz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }


        private async void Locking()
        {
            if (SharedData.TimerSystemResetEnabled == true)
            {
                if (SharedData.Locked == false)
                {
                    checkBox1.Enabled = true;
                    checkBox1.Checked = false;
                    checkBox2.Enabled = true;
                    checkBox2.Checked = false;
                    textBox1.Text = "";
                    textBox1.Enabled = true;
                    pictureBox2.Enabled = true;
                    pictureBox11.Enabled = true;
                    pictureBox14.Enabled = true;

                    remainingTimeInSeconds = SharedData.TimerSystem;

                    RequestEventCloseCY?.Invoke(this, EventArgs.Empty);
                }
                else
                {
                    checkBox1.Enabled = true;
                    checkBox1.Checked = false;
                    checkBox2.Enabled = true;
                    checkBox2.Checked = false;
                    textBox1.Text = "";
                    textBox1.Enabled = true;
                    pictureBox2.Enabled = true;
                    pictureBox11.Enabled = true;
                    pictureBox14.Enabled = true;

                    remainingTimeInSeconds = SharedData.TimerSystem;
                }
            }
            else
            {

                if (SharedData.Locked == false)
                {
                    remainingTimeInSeconds = SharedData.TimerSystem;

                    RequestEventCloseCY?.Invoke(this, EventArgs.Empty);
                }
                else
                {
                    remainingTimeInSeconds = SharedData.TimerSystem;
                }

            }
            
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            remainingTimeInSeconds += 5 * 60;
            UpdateRemainingTimeLabel();
        }
        private void pictureBox11_Click(object sender, EventArgs e)
        {
            remainingTimeInSeconds += 5 * 60;
            UpdateRemainingTimeLabel();
        }
        private void pictureBox14_Click(object sender, EventArgs e)
        {
            remainingTimeInSeconds += 5 * 60;
            UpdateRemainingTimeLabel();
        }

        private void ThemeOnR()
        {
            pictureBox4.Visible = true;
            pictureBox8.Visible = true;
            pictureBox7.Visible = true;
            pictureBox6.Visible = true;
            pictureBox5.Visible = true;
            pictureBox1.Visible = false;
            pictureBox3.Visible = false;
            pictureBox2.Visible = false;
            label1.Visible = false;
            checkBox1.Visible = false;
            checkBox2.Visible = false;
            textBox1.Visible = false;
        }
        private void ThemeOffR()
        {
            pictureBox8.Visible = false;
            pictureBox4.Visible = false;
            pictureBox7.Visible = false;
            pictureBox6.Visible = false;
            pictureBox5.Visible = false;
            pictureBox1.Visible = true;
            pictureBox3.Visible = true;
            pictureBox2.Visible = true;


            label1.Visible = true;
            checkBox1.Visible = true;
            checkBox2.Visible = true;
            textBox1.Visible = true;
        }


        private void ThemeOnB()
        {

            pictureBox12.Visible = true;

            pictureBox11.Visible = false;
            pictureBox13.Visible = false;

            pictureBox9.Visible = false;
            label1.Visible = false;
            checkBox1.Visible = false;
            checkBox2.Visible = false;
            textBox1.Visible = false;

            pictureBox7.Visible = true;
            pictureBox6.Visible = true;
            pictureBox5.Visible = true;
            pictureBox10.Visible = true;
        }
        private void ThemeOffB()
        {
            pictureBox10.Visible = false;
            pictureBox12.Visible = false;

            pictureBox11.Visible = true;
            pictureBox13.Visible = true;
            pictureBox9.Visible = true;


            label1.Visible = true;
            checkBox1.Visible = true;
            checkBox2.Visible = true;
            textBox1.Visible = true;

            pictureBox7.Visible = false;
            pictureBox6.Visible = false;
            pictureBox5.Visible = false;
        }


        private void ThemeOnW()
        {

            pictureBox16.Visible = true;

            pictureBox7.Visible = true;
            pictureBox6.Visible = true;
            pictureBox5.Visible = true;
            pictureBox18.Visible = true;
            label1.Visible = false;
            checkBox1.Visible = false;
            checkBox2.Visible = false;
            textBox1.Visible = false;
            pictureBox14.Visible = false;
            pictureBox15.Visible = false;
            pictureBox17.Visible = false;


        }
        private void ThemeOffW()
        {
            pictureBox7.Visible = false;
            pictureBox6.Visible = false;
            pictureBox5.Visible = false;

            pictureBox16.Visible = false;
            pictureBox18.Visible = false;

            label1.Visible = true;
            checkBox1.Visible = true;
            checkBox2.Visible = true;
            textBox1.Visible = true;
            pictureBox14.Visible = true;
            pictureBox15.Visible = true;
            pictureBox17.Visible = true;


        }


        private void pictureBox4_Click(object sender, EventArgs e)
        {
            ThemeOffR();
        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {
            //MessageBox.Show("Çok Yakında!");
            ThemeOnR();


        }
        private void pictureBox13_Click(object sender, EventArgs e)
        {
            ThemeOnB();
        }
        private void pictureBox12_Click(object sender, EventArgs e)
        {
            ThemeOffB();
        }
        private void pictureBox15_Click(object sender, EventArgs e)
        {
            ThemeOnW();
        }
        private void pictureBox16_Click(object sender, EventArgs e)
        {
            ThemeOffW();
        }
        ///VarsayılanRainbow
        private void pictureBox5_Click(object sender, EventArgs e)
        {
            //notifyIcon1.Visible = true;
            //notifyIcon1.BalloonTipTitle = "Tema";
            //notifyIcon1.BalloonTipText = "Rengarenk temaya geçiş yapıldı!";
            //notifyIcon1.ShowBalloonTip(100);

            YazThemeDosyasi(1);
            KontrolEtVeIslemYap();
            RequestEvent?.Invoke(this, EventArgs.Empty);
        }



        // /////KaranlıkMOD
        private void pictureBox6_Click(object sender, EventArgs e)
        {
            //notifyIcon1.Visible = true;
            //notifyIcon1.BalloonTipTitle = "Tema";
            //notifyIcon1.BalloonTipText = "Karanlık temaya geçiş yapıldı!";
            //notifyIcon1.ShowBalloonTip(100);

            YazThemeDosyasi(2);
            KontrolEtVeIslemYap();
            RequestEvent?.Invoke(this, EventArgs.Empty);
        }



        // /////AÇIK MOD
        private void pictureBox7_Click(object sender, EventArgs e)
        {
            //notifyIcon1.Visible = true;
            //notifyIcon1.BalloonTipTitle = "Tema";
            //notifyIcon1.BalloonTipText = "Açık temaya geçiş yapıldı!";
            //notifyIcon1.ShowBalloonTip(100);

            YazThemeDosyasi(3);
            KontrolEtVeIslemYap();
            RequestEvent?.Invoke(this, EventArgs.Empty);
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
            pictureBox8.Visible = false;
            pictureBox18.Visible = false;
            // 1. tema için özel işlemler burada
            // Örneğin: this.BackColor = Color.Red;
            await Task.Delay(10);

            label1.Visible = true;
            checkBox1.Visible = true;
            checkBox2.Visible = true;
            textBox1.Visible = true;

            pictureBox2.Visible = true;
            pictureBox3.Visible = true;
            pictureBox1.Visible = true;




            label1.BackColor = ColorTranslator.FromHtml("#373244");
            checkBox1.BackColor = ColorTranslator.FromHtml("#373244");
            checkBox2.BackColor = ColorTranslator.FromHtml("#373244");
            pictureBox2.BackColor = ColorTranslator.FromHtml("#f7f7f7");
            pictureBox5.BackColor = ColorTranslator.FromHtml("#373244");
            pictureBox6.BackColor = ColorTranslator.FromHtml("#373244");
            pictureBox7.BackColor = ColorTranslator.FromHtml("#373244");
        }
        private void ThemeOff1()
        {
            label1.Visible = false;
            checkBox1.Visible = false;
            checkBox2.Visible = false;
            textBox1.Visible = false;
            pictureBox1.Visible = false;
            pictureBox2.Visible = false;
            pictureBox3.Visible = false;

            pictureBox8.Visible = false;
            pictureBox4.Visible = false;
            pictureBox7.Visible = false;
            pictureBox6.Visible = false;
            pictureBox5.Visible = false;
        }

        private async void ThemeOn2()
        {
            ThemeOff1();
            ThemeOff3();
            pictureBox10.Visible = false;
            pictureBox18.Visible = false;
            await Task.Delay(10);


            pictureBox11.Visible = true;
            pictureBox13.Visible = true;
            pictureBox9.Visible = true;

            // 2. tema için özel işlemler burada
            // Örneğin: this.BackColor = Color.Green;

            label1.Visible = true;
            checkBox1.Visible = true;
            checkBox2.Visible = true;
            textBox1.Visible = true;

            pictureBox7.Visible = false;
            pictureBox6.Visible = false;
            pictureBox5.Visible = false;




            label1.BackColor = ColorTranslator.FromHtml("#373244");
            checkBox1.BackColor = ColorTranslator.FromHtml("#373244");
            checkBox2.BackColor = ColorTranslator.FromHtml("#373244");
            pictureBox2.BackColor = ColorTranslator.FromHtml("#f7f7f7");
            pictureBox5.BackColor = ColorTranslator.FromHtml("#373244");
            pictureBox6.BackColor = ColorTranslator.FromHtml("#373244");
            pictureBox7.BackColor = ColorTranslator.FromHtml("#373244");
            pictureBox13.BackColor = ColorTranslator.FromHtml("#373244");
            pictureBox11.BackColor = ColorTranslator.FromHtml("#ffffff");
            pictureBox12.BackColor = ColorTranslator.FromHtml("#373244");
        }



        private void ThemeOff2()
        {
            pictureBox7.Visible = false;
            pictureBox6.Visible = false;
            pictureBox5.Visible = false;
            pictureBox10.Visible = false;
            pictureBox12.Visible = false;
            pictureBox11.Visible = false;
            pictureBox13.Visible = false;
            label1.Visible = false;
            checkBox1.Visible = false;
            checkBox2.Visible = false;
            textBox1.Visible = false;
            pictureBox9.Visible = false;
        }


        private async void ThemeOn3()
        {
            ThemeOff1();
            ThemeOff2();
            pictureBox10.Visible = false;
            pictureBox8.Visible = false;
            pictureBox18.Visible = false;
            await Task.Delay(10);
            pictureBox17.Visible = true;
            // 3. tema için özel işlemler burada
            // Örneğin: this.BackColor = Color.Blue;
            pictureBox14.Visible = true;
            pictureBox15.Visible = true;
            pictureBox16.Visible = true;

            label1.Visible = true;
            checkBox1.Visible = true;
            checkBox2.Visible = true;
            textBox1.Visible = true;


            label1.BackColor = ColorTranslator.FromHtml("#373244");
            checkBox1.BackColor = ColorTranslator.FromHtml("#373244");
            checkBox2.BackColor = ColorTranslator.FromHtml("#373244");
            pictureBox2.BackColor = ColorTranslator.FromHtml("#f7f7f7");
            pictureBox5.BackColor = ColorTranslator.FromHtml("#373244");
            pictureBox6.BackColor = ColorTranslator.FromHtml("#373244");
            pictureBox7.BackColor = ColorTranslator.FromHtml("#373244");
            pictureBox15.BackColor = ColorTranslator.FromHtml("#ffffff");
            pictureBox14.BackColor = ColorTranslator.FromHtml("#ffffff");
            pictureBox16.BackColor = ColorTranslator.FromHtml("#ffffff");
        }

        private void ThemeOff3()
        {
            // 3. tema için özel işlemler burada
            // Örneğin: this.BackColor = Color.Blue;
            pictureBox7.Visible = false;
            pictureBox6.Visible = false;
            pictureBox5.Visible = false;

            pictureBox16.Visible = false;
            pictureBox18.Visible = false;

            label1.Visible = false;
            checkBox1.Visible = false;
            checkBox2.Visible = false;
            textBox1.Visible = false;
            pictureBox14.Visible = false;
            pictureBox15.Visible = false;
            pictureBox17.Visible = false;
        }



        private static cancel instance6;
        public static cancel Instance6
        {
            get
            {
                if (instance6 == null || instance6.IsDisposed)
                {
                    instance6 = new cancel();
                }
                return instance6;
            }
        }


        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox2.Checked)
            {
                if (SharedData.TimerSystemEnabledForceCY == true)
                {
                    Console.WriteLine("Kullanıcı evet dedi.");
                    checkBox1.Enabled = false;
                    checkBox2.Enabled = false;
                    pictureBox2.Enabled = false;
                    pictureBox11.Enabled = false;
                    pictureBox14.Enabled = false;
                    textBox1.Enabled = false;

                    return;
                }

                DialogResult result = MessageBox.Show("UYARI! bu işlem bir daha geri alınamaz şekilde zaman değiştirmeyi arttırmayı engeller. Onaylıyormusunuz?", "Kalıcı süre kilitleme onaylama", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                // Kullanıcının seçimini kontrol etme
                if (result == DialogResult.Yes)
                {
                    Console.WriteLine("Kullanıcı evet dedi.");
                    // Burada yapılacak işlemleri ekleyebilirsiniz
                    checkBox1.Enabled = false;
                    checkBox2.Enabled = false;
                    pictureBox2.Enabled = false;
                    pictureBox11.Enabled = false;
                    pictureBox14.Enabled = false;
                    textBox1.Enabled = false;
                }
                else
                {
                    Console.WriteLine("Kullanıcı hayır dedi.");
                    // Burada yapılacak işlemleri ekleyebilirsiniz
                    checkBox2.Checked = false;
                    cancel.Instance6.Show();
                }
            }
        }

        private void settings_Shown(object sender, EventArgs e)
        {
            StartPosition = FormStartPosition.CenterScreen;
            // Formun boyutunu al
            int formWidth = this.Width;
            int formHeight = this.Height;

            // Ekranın boyutunu al
            Rectangle screenBounds = Screen.PrimaryScreen.Bounds;

            // Formun konumunu hesapla
            int x = (screenBounds.Width - formWidth) / 2;
            int y = (screenBounds.Height - formHeight) / 2;

            // Formun konumunu ayarla
            this.Location = new Point(x, y);
        }
    }
}
