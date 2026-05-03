namespace CaYaSafeLockSetup
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            string folderPath = Path.Combine(Application.StartupPath, "CYSYSTEM\\CYdata");
            string file1Path = Path.Combine(folderPath, "CtL.cy");
            string file2Path = Path.Combine(folderPath, "log.cdat");

            if (Directory.Exists(folderPath))
            {
                if (File.Exists(file1Path) && File.Exists(file2Path))
                {
                    Console.WriteLine("Tüm veri dosyalarý mevcut.");




                    string klasorYolu = @"C:\ProgramData\CaYaProtection";

                    if (Directory.Exists(klasorYolu))
                    {
                        Console.WriteLine("Yazýlýmý Kaldýrma Aþamasý");


                        richTextBox1.Visible = false;
                        button1.Visible = false;

                        this.Shown += Form1_Shown; // Doðru kullaným
                        Remove rm = new Remove();
                        rm.Show();
                        


                    }
                    else
                    {
                        Console.WriteLine("Yazýlýmý Yükleme Aþamasý");




                    }
                }
                else if (File.Exists(file1Path))
                {
                    Console.WriteLine("Yalnýzca CtL.cy adlý veri bulunuyor.");
                    MessageBox.Show("Anahtar verisi eksik. Lütfen ilk önce dosyalarý oluþturun.");
                    Environment.Exit(0);
                }
                else if (File.Exists(file2Path))
                {
                    Console.WriteLine("Yalnýzca log.cdat adlý veri bulunuyor.");
                    MessageBox.Show("Kilit ekraný yazý verisi eksik. Lütfen ilk önce dosyalarý oluþturun.");
                    Environment.Exit(0);
                }
                else
                {
                    MessageBox.Show("Tüm veri dosyalarý eksik. Lütfen ilk önce dosyalarý oluþturun.");
                    Environment.Exit(0);
                }
            }
            else
            {
                Console.WriteLine("Klasör eksik");
                MessageBox.Show("Tüm veri dosyalarý eksik. Lütfen ilk önce dosyalarý oluþturun.");
                Environment.Exit(0);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Hide();
            Install ins = new Install();
            ins.Show();
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            Hide();
        }
    }
}