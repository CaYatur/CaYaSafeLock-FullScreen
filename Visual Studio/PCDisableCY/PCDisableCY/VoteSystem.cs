using System;
using System.Text;
using System.Net.Http;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Mail;
using Microsoft.Diagnostics.Tracing.Parsers.Clr;

namespace PCDisableCY
{
    public partial class VoteSystem : Form
    {
        private static readonly HttpClient client = new HttpClient();

        private Socket socket;
        private string anketKodu;

        private System.Windows.Forms.Timer timer;
        private HttpClient httpClient;

        public VoteSystem()
        {
            InitializeComponent();
            httpClient = new HttpClient();
        }

        private async void btnAnketOlustur_Click(object sender, EventArgs e)
        {
            var secenekler = new string[listBoxSecenekler.Items.Count];
            listBoxSecenekler.Items.CopyTo(secenekler, 0);
            var anketVerisi = new { secenekler = secenekler };

            var content = new StringContent(JsonConvert.SerializeObject(anketVerisi), System.Text.Encoding.UTF8, "application/json");

            string voteServerUrl = System.Configuration.ConfigurationManager.AppSettings["VOTE_SERVER_URL"] ?? "http://localhost:3000";
            var response = await client.PostAsync($"{voteServerUrl}/anket-olustur", content);
            var responseString = await response.Content.ReadAsStringAsync();

            var result = JsonConvert.DeserializeObject<dynamic>(responseString);
            anketKodu = result.kod;
            lblAnketKodu.Text = $"Anket Kodu: {result.kod}";

            StartVoteUpdateTimer();
        }

        private void btnEkle_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(textBoxSecenek.Text))
            {
                listBoxSecenekler.Items.Add(textBoxSecenek.Text);
                textBoxSecenek.Clear(); // Seçeneği ekledikten sonra TextBox'ı temizle
            }
        }

        private async void btnAnketBitir_Click(object sender, EventArgs e)
        {
            var kod = lblAnketKodu.Text.Replace("Anket Kodu: ", ""); // Koddan label'dan al
            string voteServerUrl = System.Configuration.ConfigurationManager.AppSettings["VOTE_SERVER_URL"] ?? "http://localhost:3000";
            var response = await client.PostAsync($"{voteServerUrl}/anket-bitir/{kod}", null);

            if (response.IsSuccessStatusCode)
            {
                MessageBox.Show("Anket başarıyla kapatıldı.");
            }
            else
            {
                MessageBox.Show("Anket kapatılırken bir hata oluştu.");
            }
        }






        private void StartVoteUpdateTimer()
        {
            timer = new System.Windows.Forms.Timer();
            timer.Interval = 1000; // Her 1 saniyede bir güncelle
            timer.Tick += (sender, e) => UpdateVotes(); // Zamanlayıcı her tick olduğunda UpdateVotes çağrılacak
            timer.Start(); // Zamanlayıcıyı başlat
        }

        private async void UpdateVotes()
        {
            try
            {
                string voteServerUrl = System.Configuration.ConfigurationManager.AppSettings["VOTE_SERVER_URL"] ?? "http://localhost:3000";
                var response = await client.GetStringAsync($"{voteServerUrl}/anket-sonuc/{anketKodu}");
                UpdateUI(response); // HTML yanıtını UI'ya ekle
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Bir hata oluştu: {ex.Message}");
            }
        }



        private void UpdateUI(string response)
        {
            JObject jsonResponse = JObject.Parse(response);
            resultListBox.Items.Clear(); // Önceki sonuçları temizle

            var secenekler = jsonResponse["secenekler"];
            var oylar = jsonResponse["oylar"];

            // Oylama sonuçlarını ekle
            if (oylar != null && secenekler != null)
            {
                foreach (var item in oylar.Children<JProperty>()) // JProperty kullanarak anahtar ve değerleri al
                {
                    string secimAnahtari = item.Name; // Anahtar (örneğin "0", "1")
                    int oySayisi = (int)item.Value; // Oy sayısını al

                    // İlgili seçeneği bul
                    int secenekIndex = int.Parse(secimAnahtari); // Anahtarı tam sayıya çevir
                    string secenek = secenekler[secenekIndex].ToString(); // Seçeneği al

                    // Listeye ekle
                    resultListBox.Items.Add($"{secenek} :> {oySayisi} oy");
                }
            }
            else
            {
                resultListBox.Items.Add("Hiç oy yok.");
            }
        }





    }
}
