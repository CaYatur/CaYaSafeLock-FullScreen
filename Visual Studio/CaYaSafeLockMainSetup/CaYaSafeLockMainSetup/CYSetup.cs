using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CaYaSafeLockMainSetup
{
    public partial class CYSetup : Form
    {
        public CYSetup()
        {
            InitializeComponent();
        }

        private void CYSetup_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            Random rnd = new Random();
            int randomNumber = rnd.Next(10000000, 99999999); // 8 basamaklı bir sayı oluşturur
            textBox2.Text = randomNumber.ToString();
        }

        private void textBox2_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true; // Sadece sayıları kabul et
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (textBox2.TextLength == 8)
            {
                //verileri burda işler

                EncryptAndSave(textBox1, "CtL.cy");
                EncryptAndSave(textBox2, "log.cdat");


                MessageBox.Show("Yazılım verileri oluşturuldu! Kurulum için artık hazır!");

            }
            else
            {
                MessageBox.Show("Anahtar kodu eksiksiz girmeniz gerekmektedir.");
            }
        }











        // AES anahtarları kaynak kodda sabit yazılmamalıdır.
        // Bu değerleri App.config veya Windows DPAPI ile şifreli olarak saklayın.
        private static readonly byte[] Key = Encoding.UTF8.GetBytes(System.Configuration.ConfigurationManager.AppSettings["AES_KEY"] ?? "REPLACE_IN_APP_CONFIG");
        private static readonly byte[] IV = Encoding.UTF8.GetBytes(System.Configuration.ConfigurationManager.AppSettings["AES_IV"] ?? "REPLACE_IN_APP_CONFIG");

        private void EncryptAndSave(TextBox textBox, string fileName)
        {
            string plainText = textBox.Text;
            string encryptedText = Encrypt(plainText);

            string folderPath = Path.Combine(Application.StartupPath, "CYSYSTEM\\CYdata");
            string filePath = Path.Combine(folderPath, fileName);

            // Check if the directory exists, if not create it
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            // Write the encrypted text to the file
            File.WriteAllText(filePath, encryptedText);
        }

        private string Encrypt(string plainText)
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Key;
                aesAlg.IV = IV;

                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(plainText);
                        }
                    }
                    return Convert.ToBase64String(msEncrypt.ToArray());
                }
            }
        }

        private void CYSetup_FormClosing(object sender, FormClosingEventArgs e)
        {
            Environment.Exit(0);
        }
    }
}
