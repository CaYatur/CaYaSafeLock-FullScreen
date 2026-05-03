using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Management;
using System.Net;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using System.Xml.Serialization;
using QRCoder;
using Newtonsoft.Json;
using Microsoft.Win32;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static PCDisableCY.LockScreen;
using Microsoft.Diagnostics.Tracing.Parsers.MicrosoftWindowsWPF;


namespace PCDisableCY
{
    public partial class LockScreen : Form
    {
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


        public static event EventHandler RequestEvent;
        public static event EventHandler FatalError;


        // Sunucu URL'si App.config içinde tanımlanmalıdır - kaynak kodda hardcode edilmemelidir.
        private string URIQRCHECK = System.Configuration.ConfigurationManager.AppSettings["QR_SERVER_URL"] ?? "http://localhost:8080/";



        Form closeForm = new Form();

        // Windows API kullanarak gerekli tanımlamalar
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        //private static LowLevelKeyboardProc _proc = HookCallback;
        private static IntPtr _hookID = IntPtr.Zero;

        private bool Closing = false;
        private bool TimeOut = false;
        private string enteredPasscode = "";


        private Rectangle lastScreenBounds;
        public LockScreen()
        {
            InitializeComponent();
            lastScreenBounds = Screen.PrimaryScreen.Bounds;
            SystemEvents.DisplaySettingsChanged += SystemEvents_DisplaySettingsChanged;
        }


        private void SystemEvents_DisplaySettingsChanged(object sender, EventArgs e)
        {
            // Masaüstü değişikliği algılandığında, formu yeniden konumlandır
            foreach (Screen screen in Screen.AllScreens)
            {
                if (!screen.Bounds.Equals(lastScreenBounds))
                {
                    // Eski masaüstü boyutunu güncelle
                    lastScreenBounds = screen.Bounds;

                    // Yeni masaüstü boyutunu ve konumunu al
                    Rectangle newBounds = screen.Bounds;

                    // Formu yeni masaüstüne taşı
                    this.Location = new Point(newBounds.Left + (newBounds.Width - this.Width) / 2,
                                               newBounds.Top + (newBounds.Height - this.Height) / 2);
                    break;
                }
            }
        }


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
        private static extern int ShowWindow(int hwnd, int command);

        private const int SW_HIDE = 0;
        private const int SW_SHOW = 1;




        // Windows API'deki SetWindowPos fonksiyonunu bildiriyoruz.
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]


        static extern bool GetCursorPos(out POINT lpPoint);

        // Kullanacağımız sabitler
        static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        const uint SWP_NOSIZE = 0x0001;
        const uint SWP_NOMOVE = 0x0002;
        const uint SWP_SHOWWINDOW = 0x0040;

        // POINT struct'ını tanımlıyoruz
        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;
        }

        System.Windows.Forms.Timer timer;
        private async void LockScreen_Load(object sender, EventArgs e)
        {
            // Timer oluştur ve başlat
            timer = new System.Windows.Forms.Timer();
            timer.Interval = 1000; // 1 saniye (1000 ms)
            timer.Tick += Timer_Tick;
            timer.Start();
            UpdateClock();

            Kilit.Focus += FocusThisForm;

            //SetAllDesktopHook();

            //IntPtr taskbarHandle = FindWindow("Shell_TrayWnd", "");
            //ShowWindow(taskbarHandle, SW_SHOW); ////görev çubuğu gösterir
            //ShowWindow(taskbarHandle, SW_HIDE); ////görev çubuğu gizler

            Focus();

            this.FormBorderStyle = FormBorderStyle.None; // Formun kenarlık stilini kaldırır
            this.ShowInTaskbar = false; // Görev çubuğunda gösterme

            // Form penceresine uygun stilleri uygula
            int extendedStyle = GetWindowLong(this.Handle, -20);
            SetWindowLong(this.Handle, -20, extendedStyle | WS_EX_TOOLWINDOW | WS_EX_NOACTIVATE | WS_VISIBLE);

            // Formu tüm masaüstleri üzerinde göster
            IntPtr desktopHandle = FindWindow("Progman", "Program Manager");
            SetWindowPos(this.Handle, new IntPtr(-1), 0, 0, 0, 0, 0x0001 | 0x0040 | 0x0002);





            //_hookID = SetHook(_proc);

            ProcessModule objCurrentModule = Process.GetCurrentProcess().MainModule;
            objKeyboardProcess = new LowLevelKeyboardProc(captureKey);
            ptrHook = SetWindowsHookEx(13, objKeyboardProcess, GetModuleHandle(objCurrentModule.ModuleName), 0);

            //UnhookWindowsHookEx(ptrHook);
            //ptrHook = SetWindowsHookEx(13, objKeyboardProcess, GetModuleHandle(objCurrentModule.ModuleName), 0);

            TopMost = true;
            TopMost = false;
            TopMost = true;
            TopMost = false;
            TopMost = true;
            TopMost = true;
            TopLevel = true;
            TopMost = true;

            //////
            ///
            // Öncelikle, pencereyi tanımlayan bir IntPtr'e ihtiyacımız var.
            //IntPtr hwnd = Process.GetCurrentProcess().MainWindowHandle;

            // Pencereyi en üste çıkarmak için SetWindowPos'u çağırıyoruz.
            //SetWindowPos(hwnd, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE);



            // Öncelikle, pencere tanımlayıcıyı alıyoruz.
            IntPtr hwnd = Process.GetCurrentProcess().MainWindowHandle;

            // Pencereyi en üste çıkarmak ve pozisyonunu güncellemek için SetWindowPos'u çağırıyoruz.
            SetWindowPos(hwnd, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOSIZE | SWP_NOMOVE | SWP_SHOWWINDOW);



            Thread bgThread = new Thread(() =>
            {


                while (true)
                {
                    POINT cursorPos;
                    GetCursorPos(out cursorPos);
                    SetWindowPos(hwnd, HWND_TOPMOST, cursorPos.X, cursorPos.Y, 0, 0, SWP_NOSIZE | SWP_SHOWWINDOW);


                    if (InvokeRequired)
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
                        catch (ObjectDisposedException ex)
                        {
                            // Handle the exception, maybe log it or show a message to the user
                            Console.WriteLine($"ObjectDisposedException: {ex.Message}");
                        }


                    }
                    else
                    {
                        TopLevel = true;
                        TopMost = true;
                        //Focus();
                    }


                    System.Threading.Thread.Sleep(2000);
                }


            });
            bgThread.IsBackground = true;
            bgThread.Start();






            StartupSystemCY();




        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetForegroundWindow(IntPtr hWnd);
        private async void FocusThisForm(object sender, EventArgs e)
        {
            //await Task.Delay(1500);

            if (!this.IsDisposed)
            {
                Invoke(new Action(() =>
                {
                    FocusMet();
                }));
            }
        }

        private void FocusMet()
        {
            TopMost = true; // Formunuzu üstte tutun
            this.Show(); // Formu gösterin (eğer gizli ise)
            this.Activate(); // Formu aktif hale getirin
            SetForegroundWindow(this.Handle); // Formu ön plana getirin
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                if (InvokeRequired)
                {
                    Invoke(new Action(() =>
                    {

                        // Her tick ile saati güncelle
                        UpdateClock();

                    }));
                }
                else
                {
                    // Her tick ile saati güncelle
                    UpdateClock();
                }
            });



        }

        private void UpdateClock()
        {
            // Label üzerinde saati göster
            clockLabel.Text = DateTime.Now.ToString("HH:mm");
            dateLabel.Text = DateTime.Now.ToString("dd/MM/yyyy");
        }


        private string encryptedNumber;
        private string decryptedNumber;
        // AES anahtarları uygulama ayarlarından veya şifreli bir config dosyasından okunmalıdır.
        // Örnek: App.config içinde şifrelenmiş olarak tutun veya Windows DPAPI kullanın.
        private static readonly byte[] Key = Encoding.UTF8.GetBytes(System.Configuration.ConfigurationManager.AppSettings["AES_KEY"] ?? "REPLACE_IN_APP_CONFIG");
        private static readonly byte[] IV = Encoding.UTF8.GetBytes(System.Configuration.ConfigurationManager.AppSettings["AES_IV"] ?? "REPLACE_IN_APP_CONFIG");
        private string storedEncryptedNumber;


        private async void CheckQRInServer(CancellationToken cancellationToken = default)
        {
            string qrCode = encryptedNumber + CK; // QR kod verisi
            string url = $"{URIQRCHECK}kontrol/{qrCode}"; // URL oluştur

            using (HttpClient client = new HttpClient())
            {
                while (!cancellationToken.IsCancellationRequested) // Sonsuz döngü, iptal kontrolü
                {
                    if (TimeOut == false)
                    {
                        try
                        {
                            // GET isteği gönder
                            HttpResponseMessage response = await client.GetAsync(url, cancellationToken);

                            if (response.IsSuccessStatusCode)
                            {
                                string responseBody = await response.Content.ReadAsStringAsync();

                                // JSON yanıtını ayrıştır
                                var jsonResponse = JsonConvert.DeserializeObject<QRResponse>(responseBody);

                                // "exists" alanını kontrol et
                                if (jsonResponse.exists)
                                {
                                    // QR kod var, burada işlem yapabilirsiniz
                                    Console.WriteLine("QR kod mevcut, gerekli işlemi yapın.");
                                    //MessageBox.Show("QR kod bulundu!");

                                    // İşlem başarılı, şimdi silme isteği gönder
                                    string deleteUrl = $"{URIQRCHECK}sil/{qrCode}";
                                    //Process.Start(new ProcessStartInfo
                                    //{
                                    //    FileName = deleteUrl,
                                    //    UseShellExecute = true
                                    //}); // Varsayılan tarayıcıda açar
                                    //MessageBox.Show(deleteUrl);



                                    try
                                    {
                                        // GET isteğini gönder ve yanıtı al
                                        HttpResponseMessage response2 = await client.GetAsync(deleteUrl);

                                        // Yanıtın başarılı olup olmadığını kontrol et
                                        response2.EnsureSuccessStatusCode();

                                        // Yanıt içeriğini oku
                                        string responseBody2 = await response2.Content.ReadAsStringAsync();

                                        // Yanıtı ekrana yazdır
                                        Console.WriteLine(responseBody2);
                                        //MessageBox.Show(responseBody2);
                                    }
                                    catch (HttpRequestException e)
                                    {
                                        Console.WriteLine($"İstek sırasında bir hata oluştu: {e.Message}");
                                        //MessageBox.Show(e.Message);
                                    }

                                    AuthingApproved();

                                    // Döngüden çık
                                    break;
                                }
                                else
                                {
                                    // QR kod yok
                                    Console.WriteLine("QR kod bulunamadı.");
                                    //MessageBox.Show("QR kod bulunamadı!");
                                }
                            }
                            else
                            {
                                // Başarısız durum
                                //MessageBox.Show($"İstek başarısız oldu. HTTP Durum Kodu: {response.StatusCode}");
                            }
                        }
                        catch (HttpRequestException e)
                        {
                            Console.WriteLine($"İstek hatası: {e.Message}");
                            //MessageBox.Show($"Bir hata oluştu: {e.Message}");
                        }
                        catch (TaskCanceledException)
                        {
                            Console.WriteLine("İşlem iptal edildi.");
                            //MessageBox.Show("İstek iptal edildi.");
                            break;
                        }

                        // 5 saniye bekle
                        await Task.Delay(1000, cancellationToken); // İptal edilebilir bekleme
                    }
                    else
                    {
                        break;
                    }

                }
            }
        }

        // Gelen JSON verisini temsil eden sınıf
        public class QRResponse
        {
            public string qrData { get; set; }
            public bool exists { get; set; }
        }



        private async void StartupSystemCY()
        {


            USBblock.USBinserted += USBinserted;
            USBblock.USBremoved += USBremoved;

            USBL.Text = "";

            Thread bgThread = new Thread(() =>
            {

                // USB cihazı takma ve çıkarma olaylarını dinlemek için WMI olayları kur
                ManagementEventWatcher insertWatcher = new ManagementEventWatcher();
                ManagementEventWatcher removeWatcher = new ManagementEventWatcher();

                WqlEventQuery insertQuery = new WqlEventQuery("SELECT * FROM Win32_VolumeChangeEvent WHERE EventType = 2");
                WqlEventQuery removeQuery = new WqlEventQuery("SELECT * FROM Win32_VolumeChangeEvent WHERE EventType = 3");

                insertWatcher.EventArrived += new EventArrivedEventHandler(UsbInsertedEvent);
                removeWatcher.EventArrived += new EventArrivedEventHandler(UsbRemovedEvent);

                insertWatcher.Query = insertQuery;
                removeWatcher.Query = removeQuery;

                insertWatcher.Start();
                removeWatcher.Start();

                Console.WriteLine("USB takma ve çıkarma olayları dinleniyor... Çıkmak için Ctrl+C'ye basın.");
                while (true)
                {
                    Thread.Sleep(1000);
                }


            });
            bgThread.IsBackground = true;
            bgThread.Start();

            QRCodeSystem();
            err.Text = "";
            Passcode.Text = "";
            textBox1.Select();
            textBox1.MaxLength = 6;



            // Ana ekranı belirlemek için Screen.PrimaryScreen özelliğini kullanabilirsiniz
            Screen primaryScreen = Screen.PrimaryScreen;

            // Tüm ekranları alın
            Screen[] allScreens = Screen.AllScreens;

            // Ana ekran haricindeki tüm ekranları kapat
            foreach (Screen screen in allScreens)
            {
                if (screen != primaryScreen)
                {
                    CloseScreen(screen);
                }
            }

            DecryptAndDisplayText();
            //StartHttpServer();

            CheckQRInServer();
        }


        private void CloseScreen(Screen screen)
        {
            // Ekranı kapatmak için bir form oluşturun ve gösterin
            //Form closeForm = new Form(); //Bunu başka yere tanımladım!!!
            closeForm.FormBorderStyle = FormBorderStyle.None;
            closeForm.WindowState = FormWindowState.Maximized;
            closeForm.StartPosition = FormStartPosition.Manual;
            closeForm.Location = screen.Bounds.Location;
            closeForm.BackColor = Color.Black; // Ekranda siyah bir arka plan gösterilecek
            closeForm.TopMost = true; // Diğer pencerelerin üzerine çıkacak şekilde ayarlayın
            closeForm.ShowInTaskbar = false;

            // Ekran kapatıldığında bir yazı ekle
            System.Windows.Forms.Label label = new System.Windows.Forms.Label();
            label.Text = "CaYaSafeLockSystem";
            label.Font = new Font("Arial", 24, FontStyle.Bold);
            label.ForeColor = Color.White;
            label.AutoSize = true;
            label.Location = new Point((closeForm.ClientSize.Width - label.Width) / 2, (closeForm.ClientSize.Height - label.Height) / 2);
            closeForm.Controls.Add(label);

            closeForm.Show();


            //closeForm.FormBorderStyle = FormBorderStyle.None; // Formun kenarlık stilini kaldırır
            //closeForm.ShowInTaskbar = false; // Görev çubuğunda gösterme
            //closeForm.WindowState = FormWindowState.Maximized;

            // Form penceresine uygun stilleri uygula
            int extendedStyle = GetWindowLong(closeForm.Handle, -20);
            SetWindowLong(closeForm.Handle, -20, extendedStyle | WS_EX_TOOLWINDOW | WS_EX_NOACTIVATE | WS_VISIBLE);

            // Formu tüm masaüstleri üzerinde göster
            IntPtr desktopHandle = FindWindow("Progman", "Program Manager");
            SetWindowPos(closeForm.Handle, new IntPtr(-1), 0, 0, 0, 0, 0x0001 | 0x0040 | 0x0002);


            // Form yüklenirken kapatın
            closeForm.Load += (sender, e) =>
            {
                ((Form)sender).Close();
            };

            // Formun TopMost özelliğini sürekli olarak güncelle
            System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
            timer.Interval = 100; // 100 milisaniye olarak ayarlayın veya gerektiği gibi değiştirin
            timer.Tick += (sender, e) =>
            {
                closeForm.TopMost = true; // Formu her zaman en üstte tut
            };
            timer.Start();
        }


        private static void StartHttpServer()
        {
            try
            {
                HttpListener listener = new HttpListener();
                listener.Prefixes.Add("http://*:3452/");
                listener.Start();
                Console.WriteLine("HTTP Sunucusu başlatıldı.");

                Task.Run(() =>
                {
                    while (true)
                    {
                        var context = listener.GetContext();
                        var request = context.Request;
                        var response = context.Response;

                        if (request.HttpMethod == "OPTIONS") // Preflight isteğini ele al
                        {
                            response.Headers.Add("Access-Control-Allow-Origin", "*");
                            response.Headers.Add("Access-Control-Allow-Methods", "POST");
                            response.Headers.Add("Access-Control-Allow-Headers", "Content-Type");
                            response.StatusCode = 200;
                            response.Close();
                        }
                        else if (request.HttpMethod == "POST" && request.Url.AbsolutePath == "/api/bildirim")
                        {
                            using (var reader = new System.IO.StreamReader(request.InputStream, request.ContentEncoding))
                            {
                                string json = reader.ReadToEnd();
                                BildirimModel bildirim = JsonConvert.DeserializeObject<BildirimModel>(json);
                                Console.WriteLine("Bildirim alındı: " + bildirim.Mesaj);

                                MessageBox.Show(bildirim.Mesaj, "Bildirim Alındı");
                            }

                            string responseString = JsonConvert.SerializeObject(new { status = "Bildirim alındı." });
                            byte[] buffer = Encoding.UTF8.GetBytes(responseString);
                            response.ContentLength64 = buffer.Length;
                            response.OutputStream.Write(buffer, 0, buffer.Length);
                        }
                    }
                });
            }
            catch
            {


            }


        }



        private void UsbInsertedEvent(object sender, EventArrivedEventArgs e)
        {

            if (this.IsHandleCreated)
            {
                this.BeginInvoke((MethodInvoker)delegate
                {
                    USBL.Text = "USB Aygıt takılması nedeniyle çoğu işlem devre dışı kalmıştır.";
                });
            }


            // Ana formu ön plana getir
            if (this.InvokeRequired)
            {
                this.Invoke(new MethodInvoker(delegate { this.Focus(); }));
            }
            else
            {
                this.Focus();
            }



            Thread bgThread = new Thread(() =>
            {
                if (this.IsHandleCreated)
                {
                    this.BeginInvoke((MethodInvoker)delegate
                    {
                        FATALERROR fe = new FATALERROR();
                        fe.ShowDialog();
                    });
                }

            });
            bgThread.IsBackground = true;
            bgThread.Start();



            //Console.WriteLine("USB takıldı.");
            //if (this.IsHandleCreated)
            //{
            //    this.BeginInvoke((MethodInvoker)delegate
            //    {
            //        Hide();
            //    });
            //}

            //USBblock ub = new USBblock();
            //ub.ShowDialog();


            //BringToFrontForm();
        }



        private void UsbRemovedEvent(object sender, EventArrivedEventArgs e)
        {
            Console.WriteLine("USB çıkarıldı. İzleme durduruldu.");

            if (this.IsHandleCreated)
            {
                this.BeginInvoke((MethodInvoker)delegate
                {
                    USBL.Text = "";
                });
            }

            // Ana formu ön plana getir
            if (this.InvokeRequired)
            {
                this.Invoke(new MethodInvoker(delegate { this.Focus(); }));
            }
            else
            {
                this.Focus();
            }

            //if (this.IsHandleCreated)
            //{
            //    this.BeginInvoke((MethodInvoker)delegate
            //    {
            //        Show();
            //        //textBox1.Focus();
            //        textBox1.ResetText();
            //        textBox1.Select();

            //        this.Validate();
            //        this.Refresh();
            //        //textBox1.Focus();

            //    });
            //}

            //if (this.IsHandleCreated)
            //{
            //    this.BeginInvoke((MethodInvoker)delegate
            //    {
            //        FatalError?.Invoke(this, EventArgs.Empty);
            //    });
            //}

            //BringToFrontForm();

        }


        private void USBinserted(object sender, EventArgs e)
        {
            USBL.Text = "PROGRAM AÇILMADAN USB TAKILDIĞI TESPİT EDİLDİ! NORMALDEN FAZLA BİR ŞEKİLDE İŞLEMLER ENGELLENMEKTEDİR.";

            if (this.IsHandleCreated)
            {
                this.BeginInvoke((MethodInvoker)delegate
                {
                    //this.Enabled = false;
                });
            }
            //BringToFrontForm();


            Thread bgThread = new Thread(() =>
            {
                if (this.IsHandleCreated)
                {
                    this.BeginInvoke((MethodInvoker)delegate
                    {
                        FATALERROR fe = new FATALERROR();
                        fe.ShowDialog();
                    });
                }

            });
            bgThread.IsBackground = true;
            bgThread.Start();
        }

        private void USBremoved(object sender, EventArgs e)
        {
            if (this.IsHandleCreated)
            {
                this.BeginInvoke((MethodInvoker)delegate
                {
                    //this.Enabled = true;
                    USBL.Text = "";
                });
            }




            //if (this.IsHandleCreated)
            //{
            //    this.BeginInvoke((MethodInvoker)delegate
            //    {
            //        FatalError?.Invoke(this, EventArgs.Empty);
            //    });
            //}
            //BringToFrontForm();
        }

        private void BringToFrontForm()
        {
            if (InvokeRequired)
            {
                Invoke(new MethodInvoker(() => { this.BringToFront(); }));
            }
            else
            {
                this.BringToFront();
            }
        }


        private string CK = ""; //Giriş anahtar kodu. Şifreli hali!

        private async void QRCodeSystem()
        {
            LoadStoredEncryptedNumber();

            // Rastgele 6 basamaklı sayı üretme
            Random random = new Random();
            int number = random.Next(100000, 999999);

            // Sayıyı şifreleme
            decryptedNumber = number.ToString();
            encryptedNumber = Encrypt(decryptedNumber);


            string filePath = @"C:\Users\Default\AppData\Roaming\CYSYSTEM\CYdata\log.cdat";

            try
            {
                // Dosyadan metni oku
                using (StreamReader sr = new StreamReader(filePath))
                {
                    CK = sr.ReadToEnd();
                }

                // Metni consola yaz
                Console.WriteLine(CK);
            }
            catch (Exception ex)
            {
                // Hata durumunda işlemler
                Console.WriteLine("Hata: " + ex.Message);
            }

            //
            //
            //BUNU SİL!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            //CK = "12345678";


            // QR kod oluşturma
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(encryptedNumber + " " + CK, QRCodeGenerator.ECCLevel.Q);
            QRCode qrCode = new QRCode(qrCodeData);

            // PictureBox boyutuna göre QR kodunu yeniden boyutlandır
            int qrCodeSize = Math.Min(pictureBox1.Width, pictureBox1.Height);
            Bitmap qrCodeImage = await Task.Run(() => qrCode.GetGraphic(5));
            qrCodeImage = await Task.Run(() => ResizeImage(qrCodeImage, qrCodeSize, qrCodeSize));

            pictureBox1.Image = qrCodeImage;

            // Şifrenin çözülmüş halini Label1'de gösterme
            label1.Text = $"Çözülmüş Şifre: {decryptedNumber}";
        }
        private void LoadStoredEncryptedNumber()
        {
            if (File.Exists("data.cy"))
            {
                storedEncryptedNumber = File.ReadAllText("data.cy");
            }
        }

        private Bitmap ResizeImage(Image image, int width, int height)
        {
            Bitmap resizedImage = new Bitmap(width, height);
            using (Graphics graphics = Graphics.FromImage(resizedImage))
            {
                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                graphics.DrawImage(image, 0, 0, width, height);
            }
            return resizedImage;
        }


        //private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        //{
        //    UnhookWindowsHookEx(_hookID);
        //    UnhookWindowsHookEx(ptrHook); // KALDIRMAK İÇİN!!
        //}


        //private static IntPtr SetHook(LowLevelKeyboardProc proc)
        //{
        //    using (Process curProcess = Process.GetCurrentProcess())
        //    using (ProcessModule curModule = curProcess.MainModule)
        //    {
        //        return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
        //    }
        //}

        //private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        //private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        //{
        //    if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
        //    {
        //        int vkCode = Marshal.ReadInt32(lParam);

        //        // Alt+Tab veya Windows tuşlarına basıldığında işlemi engelle
        //        if (vkCode == 0x09 && (Control.ModifierKeys & Keys.Alt) == Keys.Alt) // Alt+Tab
        //        {
        //            return (IntPtr)1; // İşlemi engelle
        //        }
        //        if (vkCode == 0x5B || vkCode == 0x5C) // Sol veya sağ Windows tuşu
        //        {
        //            return (IntPtr)1; // İşlemi engelle
        //        }
        //        if ((Keys)vkCode == Keys.Tab && Control.ModifierKeys == Keys.Alt)
        //        {
        //            // Alt + Tab kombinasyonunu engellemek için işlem yapabilirsiniz.
        //            return (IntPtr)1; // 1, olayın işlendiğini ve geçilmemesi gerektiğini belirtir.
        //        }
        //    }
        //    return CallNextHookEx(_hookID, nCode, wParam, lParam);
        //}

        //[DllImport("user32.dll")]
        //private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        //[DllImport("user32.dll")]
        //[return: MarshalAs(UnmanagedType.Bool)]
        //private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        //[DllImport("user32.dll")]
        //private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        //[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        //private static extern IntPtr GetModuleHandle(string lpModuleName);



        //static async void SetAllDesktopHook()
        //{

        //    await Task.Run(() =>
        //    {
        //        while (true)
        //        {



        //            // Belirli bir süre uyumak
        //            Thread.Sleep(1000); // Örneğin, 5 saniye bekleyin ve işlemi tekrarlayın
        //        }
        //    });
        //}


        [StructLayout(LayoutKind.Sequential)]
        private struct KBDLLHOOKSTRUCT
        {
            public Keys key;
            public int scanCode;
            public int flags;
            public int time;
            public IntPtr extra;
        }
        //System level functions to be used for hook and unhook keyboard input  
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
        //Declaring Global objects     
        private IntPtr ptrHook;
        private LowLevelKeyboardProc objKeyboardProcess;

        private IntPtr captureKey(int nCode, IntPtr wp, IntPtr lp)
        {
            if (nCode >= 0)
            {
                KBDLLHOOKSTRUCT objKeyInfo = (KBDLLHOOKSTRUCT)Marshal.PtrToStructure(lp, typeof(KBDLLHOOKSTRUCT));

                // Disabling Windows keys 

                if (objKeyInfo.key == Keys.RWin || objKeyInfo.key == Keys.LWin || objKeyInfo.key == Keys.Tab && HasAltModifier(objKeyInfo.flags) || objKeyInfo.key == Keys.Escape && (ModifierKeys & Keys.Control) == Keys.Control)
                {
                    return (IntPtr)1; // if 0 is returned then All the above keys will be enabled
                }
            }
            return CallNextHookEx(ptrHook, nCode, wp, lp);
        }

        bool HasAltModifier(int flags)
        {
            return (flags & 0x20) == 0x20;
        }

        private string Encrypt(string plainText)
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Key;
                aesAlg.IV = IV;

                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                using (var msEncrypt = new System.IO.MemoryStream())
                {
                    using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    using (var swEncrypt = new System.IO.StreamWriter(csEncrypt))
                    {
                        swEncrypt.Write(plainText);
                    }
                    return Convert.ToBase64String(msEncrypt.ToArray());
                }
            }
        }




        private void DecryptAndDisplayText()
        {
            string filePath = @"C:\Users\Default\AppData\Roaming\CYSYSTEM\CYdata\CtL.cy";

            try
            {
                // Dosyadan şifreli metni oku
                string encryptedText;
                using (StreamReader sr = new StreamReader(filePath))
                {
                    encryptedText = sr.ReadToEnd();
                }

                // Şifreyi çöz
                string decryptedText = Decrypt(encryptedText);

                // Çözülen metni label2'ye yaz
                label2.Text = decryptedText;
            }
            catch (Exception ex)
            {
                // Hata durumunda işlemler
                //MessageBox.Show("Hata: " + ex.Message);
            }
        }




        private string Decrypt(string cipherText)
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Key;
                aesAlg.IV = IV;

                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                using (var msDecrypt = new System.IO.MemoryStream(Convert.FromBase64String(cipherText)))
                using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                using (var srDecrypt = new System.IO.StreamReader(csDecrypt))
                {
                    return srDecrypt.ReadToEnd();
                }
            }
        }



        private void emgrny_Click(object sender, EventArgs e)
        {
            //Hide();
            UnhookWindowsHookEx(ptrHook);

            Environment.Exit(0);
        }


        int Error = 5;

        private async void Auth()
        {
            if (!this.IsDisposed)
            {
                Invoke(new MethodInvoker(() =>
                {

                    // Kullanıcının girdiği şifreyi çözme
                    string userInput = enteredPasscode;

                    // TextBox1'deki değer ile karşılaştırma
                    if (userInput == decryptedNumber)
                    {
                        if (TimeOut == false)
                        {
                            AuthingApproved();
                        }
                    }
                    else
                    {
                        //MessageBox.Show("Yanlış Şifre!");
                        if (Error > 1)
                        {
                            Error--;
                            err.Text = "Hatalı kod! Kodun değişmesine: " + Error.ToString();
                        }
                        else
                        {
                            if (TimeOut == false)
                            {
                                TimeOut = true;
                                err.Text = "Zaman aşımına girdi.";
                                textBox1.Text = "HATA!";
                                TimeOut to = new TimeOut();
                                to.ShowDialog();
                                Error = 5;
                                QRCodeSystem();
                                TimeOut = false;
                                err.Text = "Kod değiştirildi.";
                                // Tüm girilen şifreyi sil
                                enteredPasscode = "";
                                // Label'i güncelle
                                UpdatePasscodeLabel();
                                CheckQRInServer();
                                FocusMet();

                            }

                        }
                    }

                }));
            }
        }


        private void AuthingApproved()
        {
            //this.Visible = false;
            UnhookWindowsHookEx(ptrHook);
            //USBblock ub = new USBblock();
            //ub.Close();

            //LockSystem LS = new LockSystem();
            //LS.Show();
            //Kilit LS = new Kilit();
            //LS.Show();
            Kilit.Instance4.Show();
            //settings.Instance.Hide();

            RequestEvent?.Invoke(this, EventArgs.Empty);
            Closing = true;
            closeForm.Close();
            GC.Collect();
            GC.WaitForPendingFinalizers();
            this.Dispose();
            this.Close();
            //MessageBox.Show("Doğru Şifre!");
            // İşlem gerçekleştirilir
        }


        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            Passcode.Text = textBox1.Text;
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                Auth();
            }
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            Auth();
        }

        private void Zero_Click(object sender, EventArgs e)
        {
            if (enteredPasscode.Length < 6)
            {
                // Tuşun değerini girilen şifreye ekle
                enteredPasscode += "0";
                // Label'i güncelle
                UpdatePasscodeLabel();
            }
        }

        private void One_Click(object sender, EventArgs e)
        {
            if (enteredPasscode.Length < 6)
            {
                // Tuşun değerini girilen şifreye ekle
                enteredPasscode += "1";
                // Label'i güncelle
                UpdatePasscodeLabel();
            }
        }

        private void Two_Click(object sender, EventArgs e)
        {
            if (enteredPasscode.Length < 6)
            {
                // Tuşun değerini girilen şifreye ekle
                enteredPasscode += "2";
                // Label'i güncelle
                UpdatePasscodeLabel();
            }
        }

        private void Three_Click(object sender, EventArgs e)
        {
            if (enteredPasscode.Length < 6)
            {
                // Tuşun değerini girilen şifreye ekle
                enteredPasscode += "3";
                // Label'i güncelle
                UpdatePasscodeLabel();
            }
        }

        private void Four_Click(object sender, EventArgs e)
        {
            if (enteredPasscode.Length < 6)
            {
                // Tuşun değerini girilen şifreye ekle
                enteredPasscode += "4";
                // Label'i güncelle
                UpdatePasscodeLabel();
            }
        }

        private void Five_Click(object sender, EventArgs e)
        {
            if (enteredPasscode.Length < 6)
            {
                // Tuşun değerini girilen şifreye ekle
                enteredPasscode += "5";
                // Label'i güncelle
                UpdatePasscodeLabel();
            }
        }

        private void Six_Click(object sender, EventArgs e)
        {
            if (enteredPasscode.Length < 6)
            {
                // Tuşun değerini girilen şifreye ekle
                enteredPasscode += "6";
                // Label'i güncelle
                UpdatePasscodeLabel();
            }
        }

        private void Seven_Click(object sender, EventArgs e)
        {
            if (enteredPasscode.Length < 6)
            {
                // Tuşun değerini girilen şifreye ekle
                enteredPasscode += "7";
                // Label'i güncelle
                UpdatePasscodeLabel();
            }
        }

        private void Eigth_Click(object sender, EventArgs e)
        {
            if (enteredPasscode.Length < 6)
            {
                // Tuşun değerini girilen şifreye ekle
                enteredPasscode += "8";
                // Label'i güncelle
                UpdatePasscodeLabel();
            }
        }

        private void Nine_Click(object sender, EventArgs e)
        {
            if (enteredPasscode.Length < 6)
            {
                // Tuşun değerini girilen şifreye ekle
                enteredPasscode += "9";
                // Label'i güncelle
                UpdatePasscodeLabel();
            }
        }

        private void Clear_Click(object sender, EventArgs e)
        {
            // Tüm girilen şifreyi sil
            enteredPasscode = "";
            // Label'i güncelle
            UpdatePasscodeLabel();
        }

        private void Passcode_Click(object sender, EventArgs e)
        {
            //textBox1.Select();
            this.Select();
            this.Focus();
        }

        private void LockScreen_Click(object sender, EventArgs e)
        {
            //textBox1.Select();
            this.Select();
            this.Focus();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            //textBox1.Select();
            this.Select();
            this.Focus();
        }

        private void LockScreen_Shown(object sender, EventArgs e)
        {
            // pictureBox4'ün ortasını hesapla
            int centerX = pictureBox4.Left + (pictureBox4.Width / 2);

            // clockLabel ve dateLabel'ın toplam yüksekliğini hesapla
            int totalHeight = clockLabel.Height + dateLabel.Height + 5; // 5 piksel boşluk ekle

            // clockLabel ve dateLabel'ı pictureBox4'ün tam ortasına yerleştir
            clockLabel.Location = new Point(centerX - (clockLabel.Width / 2),
                                             pictureBox4.Top + (pictureBox4.Height / 2) - (totalHeight / 2));

            // dateLabel'ı clockLabel'ın altına yerleştir
            dateLabel.Location = new Point(centerX - (dateLabel.Width / 2),
                                            clockLabel.Bottom - 15); // clockLabel'ın altına 5 piksel boşluk


            Focus();
            this.Validate();
            this.Refresh();
            //textBox1.Focus();
            this.Focus();

            Closing = false;
            USBblock ub = new USBblock();
            ub.Show();
            Focus();
            Focus();
            Focus();
            Focus();
            Focus();
            Focus();
            Focus();
            Focus();
            Focus();
            TopMost = true;
            TopMost = true;
            TopMost = true;
            Focus();
        }

        private void LockScreen_FormClosing(object sender, FormClosingEventArgs e)
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



        private void LockScreen_KeyDown(object sender, KeyEventArgs e)
        {
            // Eğer basılan tuş bir rakam ise ve girilen şifre 6 basamak değilse
            if (e.KeyCode >= Keys.D0 && e.KeyCode <= Keys.D9 && enteredPasscode.Length < 6)
            {
                // Tuşun değerini girilen şifreye ekle
                enteredPasscode += (e.KeyCode - Keys.D0).ToString();
                // Label'i güncelle
                UpdatePasscodeLabel();
            }
            // Eğer Backspace tuşuna basıldıysa ve girilen şifre boş değilse
            else if (e.KeyCode == Keys.Back && enteredPasscode.Length > 0)
            {
                // Son karakteri sil
                enteredPasscode = enteredPasscode.Substring(0, enteredPasscode.Length - 1);
                // Label'i güncelle
                UpdatePasscodeLabel();
            }
            // Eğer Delete tuşuna basıldıysa ve girilen şifre boş değilse
            else if (e.KeyCode == Keys.Delete && enteredPasscode.Length > 0)
            {
                // Tüm girilen şifreyi sil
                enteredPasscode = "";
                // Label'i güncelle
                UpdatePasscodeLabel();
            }
            else if (e.KeyCode == Keys.Enter)
            {
                Auth();
            }
            //// Eğer Enter tuşuna basıldıysa
            //else if (e.KeyCode == Keys.Enter)
            //{
            //    // Girilen şifre doğruysa
            //    if (enteredPasscode == passcode)
            //    {
            //        MessageBox.Show("Giriş Başarılı!");
            //        // Şifreyi sıfırla
            //        enteredPasscode = "";
            //        UpdatePasscodeLabel();
            //    }
            //    else
            //    {
            //        MessageBox.Show("Hatalı Şifre! Tekrar Deneyin.");
            //        // Şifreyi sıfırla
            //        enteredPasscode = "";
            //        UpdatePasscodeLabel();
            //    }
            //}

        }

        // Label'i güncelleyen metod
        private void UpdatePasscodeLabel()
        {
            Passcode.Text = enteredPasscode;
        }


        [DllImport("user32.dll")]
        private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll")]
        private static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern IntPtr GetClassLong(IntPtr hWnd, int nIndex);



        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        const int GCL_HICON = -14; // Pencere simgesi için

        private void pictureBox5_Click(object sender, EventArgs e)
        {
            //Hide();
            //UnhookWindowsHookEx(ptrHook);
            //RequestEvent?.Invoke(this, EventArgs.Empty);
            //Closing = true;
            //closeForm.Close();
            //GC.Collect();
            //GC.WaitForPendingFinalizers();
            //this.Dispose();
            //this.Close();

            PCclosing pcc = new PCclosing();
            pcc.ShowDialog();

            //label3.Visible = true;
            //button1.Visible = true;
            //button2.Visible = true;
            //listView1.Visible = true;

            ////listView1.Location = new Point((this.ClientSize.Width - listView1.Width) / 2,
            ////                            (this.ClientSize.Height - listView1.Height) / 2);

            ////listView1.BackColor = Color.Black; // ListView arka plan rengi
            //listView1.ForeColor = Color.White; // ListView arka plan rengi
            //listView1.View = View.LargeIcon; // Büyük simgeleri göster

            //listView1.Items.Clear(); // Önceki öğeleri temizle
            //listView1.LargeImageList = new ImageList();
            //listView1.LargeImageList.ImageSize = new Size(32, 32); // Simge boyutu


            ////// İki butonun toplam genişliği (butonların arasına boşluk ekleyelim)
            ////int totalButtonWidth = button1.Width + button2.Width + 20;

            ////// Button1'i ListView'in altına ve ortasına hizala
            ////button1.Location = new Point((listView1.Width - totalButtonWidth) / 2 + listView1.Left, listView1.Bottom + 10);

            ////// Button2'yi Button1'in yanına hizala
            ////button2.Location = new Point(button1.Right + 20, listView1.Bottom + 10);


            //EnumWindows(new EnumWindowsProc(EnumWindow), IntPtr.Zero);
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
                        listView1.LargeImageList.Images.Add(icon); // Simgeyi ekle
                        listView1.Items.Add(new ListViewItem(windowText.ToString(), listView1.LargeImageList.Images.Count - 1));
                    }
                }
            }
            return true; // Diğer pencereleri listelemeye devam et
        }

        private void button1_Click(object sender, EventArgs e)
        {
            label3.Visible = false;
            button1.Visible = false;
            button2.Visible = false;
            listView1.Visible = false;
        }

        private void LockScreen_SizeChanged(object sender, EventArgs e)
        {
            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;
        }
    }



    public class BildirimModel
    {
        public string Mesaj { get; set; }
    }



}
