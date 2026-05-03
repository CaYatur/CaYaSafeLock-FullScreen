using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PCDisableCY
{
    public partial class warnforlocking : Form
    {
        private int slideSpeed = 5;

        private const string themeDosyaAdi = "theme.txt";

        private static warnforlocking instance8;
        public static warnforlocking Instance8
        {
            get
            {
                if (instance8 == null || instance8.IsDisposed)
                {
                    instance8 = new warnforlocking();
                }
                return instance8;
            }
        }

        public warnforlocking()
        {
            InitializeComponent();
            // Form2'den gelen olaya abone olun.
            settings.RequestEvent += HandleForm2Request;
        }

        private void cancel_Load(object sender, EventArgs e)
        {
            TopMost = true;
            BackColor = ColorTranslator.FromHtml("#212121");
            SetFormLocationToBottomLeft();
            StartSlideAnimationAsync();
            KontrolEtVeIslemYap();
        }



        private void HandleForm2Request(object sender, EventArgs e)
        {
            // Form2'den gelen isteği işleyin.
            // Bu metot, Form2'de bir buton tıklandığında çağrılacak.
            //MessageBox.Show("Form2'den istek alındı ve işlem gerçekleştirildi.");

            KontrolEtVeIslemYap();
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
                            BackColor = ColorTranslator.FromHtml("#c74693");
                            label1.ForeColor = ColorTranslator.FromHtml("#ffffff");
                            label2.ForeColor = ColorTranslator.FromHtml("#ffffff");
                            break;
                        case 2:
                            BackColor = ColorTranslator.FromHtml("#212121");
                            break;
                        case 3:
                            BackColor = ColorTranslator.FromHtml("#efefef");
                            label1.ForeColor = ColorTranslator.FromHtml("#212121");
                            break;
                        default:
                            // Bilinmeyen tema değeri, varsayılan işlemi gerçekleştir
                            BackColor = ColorTranslator.FromHtml("#c74693");
                            break;
                    }
                }
                else
                {
                    // Geçersiz tema değeri, varsayılan işlemi gerçekleştir
                    BackColor = ColorTranslator.FromHtml("#c74693");
                    label1.ForeColor = ColorTranslator.FromHtml("#ffffff");
                    label2.ForeColor = ColorTranslator.FromHtml("#ffffff");
                }
            }
            else
            {
                // Dosya bulunamadı, varsayılan işlemi gerçekleştir
                BackColor = ColorTranslator.FromHtml("#c74693");
                label1.ForeColor = ColorTranslator.FromHtml("#ffffff");
                label2.ForeColor = ColorTranslator.FromHtml("#ffffff");
            }
        }




        private void SetFormLocationToBottomLeft()
        {
            // Ekranın sol alt köşesinin koordinatlarını al
            int screenWidth = Screen.PrimaryScreen.WorkingArea.Width;
            int screenHeight = Screen.PrimaryScreen.WorkingArea.Height;

            int formX = 0; // Ekranın sol tarafından başla
            int formY = screenHeight - this.Height; // Ekranın alt tarafından başla

            this.Location = new System.Drawing.Point(formX, formY);
        }


        private async void StartSlideAnimationAsync()
        {
            await Task.Delay(3000);
            int targetY = Screen.PrimaryScreen.WorkingArea.Height - this.Height; // Ekranın alt kenarı

            for (int i = 0; i < 100; i++)
            {
                // Form hedefe henüz ulaşmadıysa devam et
                this.Location = new System.Drawing.Point(this.Location.X, this.Location.Y + slideSpeed);
                //if (this.Opacity > 0)
                //this.Opacity -= 0.02; // İsteğe bağlı olarak ayarlayabilirsiniz
                this.Opacity -= 0.02;

                // Bir süre bekleyerek asenkron işlemi gerçekleştir
                await Task.Delay(20); // 20 milisaniye, ayarlayabilirsiniz
            }
            Close();
        }


    }
}
