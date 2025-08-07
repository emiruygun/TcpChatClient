using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace TcpChatClient
{
    public partial class LoginForm : Form
    {
        private readonly HttpClient httpClient;

        public LoginForm()
        {
            InitializeComponent();
            httpClient = new HttpClient
            {
                BaseAddress = new Uri("http://localhost:5101") 
            };
        }

        private async void btnLogin_Click(object sender, EventArgs e)
        {
            string username = textUsername.Text.Trim();
            string password = textPassword.Text.Trim();

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Lütfen kullanıcı adı ve şifre girin.");
                return;
            }

            btnLogin.Enabled = false;
            Cursor.Current = Cursors.WaitCursor;

            bool loginSuccess = await TryLoginAsync(username, password);

            Cursor.Current = Cursors.Default;
            btnLogin.Enabled = true;

            if (loginSuccess)
            {
                Hide();
                Form1 chatForm = new Form1(username); // 👈 Sohbet formuna username gönderiliyor
                chatForm.Show();
            }
            else
            {
                MessageBox.Show("Giriş başarısız. Kullanıcı adı veya şifre hatalı.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private async Task<bool> TryLoginAsync(string username, string password)
        {
            var loginPayload = new
            {
                Username = username,
                Password = password
            };

            string jsonData = JsonConvert.SerializeObject(loginPayload);
            var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

            try
            {
                HttpResponseMessage response = await httpClient.PostAsync("/api/user/login", content);
                return response.IsSuccessStatusCode;
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show("API bağlantı hatası: " + ex.Message, "Sunucuya Ulaşılamıyor", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hata: " + ex.Message);
                return false;
            }
        }
    }
}
