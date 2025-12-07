using Microsoft.Web.WebView2.Core;
using Newtonsoft.Json;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace MemeAlerts
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        PipeServer ToGame;
        private void Form1_Load(object sender, EventArgs e)
        {
            InitializeWebViewAsync();
            ToGame = new PipeServer("MemeAlertsAuth");
        }

        private async void InitializeWebViewAsync()
        {
            await webView21.EnsureCoreWebView2Async(null);

            webView21.CoreWebView2.WebResourceRequested += CoreWebView2_WebResourceRequested;
            webView21.CoreWebView2.AddWebResourceRequestedFilter("*", CoreWebView2WebResourceContext.All);
            webView21.Source = new Uri("https://memealerts.com/");
        }

        string Bearer = null;
        private void CoreWebView2_WebResourceRequested(object sender, CoreWebView2WebResourceRequestedEventArgs e)
        {
            if (Bearer != null)
                return;
            try
            {
                var headers = e.Request.Headers;
                foreach (var header in headers)
                {
                    if (header.Key.Equals("Authorization", StringComparison.OrdinalIgnoreCase) && header.Value != "Bearer null")
                    {
                        if (Bearer == null)
                        {
                            Bearer = header.Value;
                            ToGame.SendAsync(Bearer);
                            this.Close();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Îøèáêà: {ex.Message}");
            }
        }
        public string GetStreamerID()
        {
            HttpClient client = new HttpClient();

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "https://memealerts.com/api/user/current");

            request.Headers.Add("accept", "*/*");
            request.Headers.Add("accept-language", "ru-RU,ru;q=0.8");
            request.Headers.Add("authorization", Bearer);
            request.Headers.Add("if-none-match", "W/\"1312-tjmKxqHt28SF1yj4VQxHjDvTsWA\"");
            request.Headers.Add("priority", "u=1, i");
            request.Headers.Add("sec-ch-ua", "\"Chromium\";v=\"142\", \"Brave\";v=\"142\", \"Not_A Brand\";v=\"99\"");
            request.Headers.Add("sec-ch-ua-mobile", "?0");
            request.Headers.Add("sec-ch-ua-platform", "\"Windows\"");
            request.Headers.Add("sec-fetch-dest", "empty");
            request.Headers.Add("sec-fetch-mode", "cors");
            request.Headers.Add("sec-fetch-site", "same-origin");
            request.Headers.Add("sec-gpc", "1");

            HttpResponseMessage response = client.SendAsync(request).Result;
            response.EnsureSuccessStatusCode();
            string responseBody = response.Content.ReadAsStringAsync().Result;
            return responseBody;
        }
    }
}
