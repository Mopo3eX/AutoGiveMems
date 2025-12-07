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
            // Инициализируем движок Edge
            await webView21.EnsureCoreWebView2Async(null);

            // Теперь у нас есть доступ к CoreWebView2:
            webView21.CoreWebView2.WebResourceRequested += CoreWebView2_WebResourceRequested;
            // Можно фильтровать запросы, чтобы ловить ВСЕ (по умолчанию ловятся не все типы)
            webView21.CoreWebView2.AddWebResourceRequestedFilter("*", CoreWebView2WebResourceContext.All);

            // Загружаем страницу MemeAlerts
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
                MessageBox.Show($"Ошибка: {ex.Message}");
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
            //request.Headers.Add("referer", "https://memealerts.com/settings");
            request.Headers.Add("sec-ch-ua", "\"Chromium\";v=\"142\", \"Brave\";v=\"142\", \"Not_A Brand\";v=\"99\"");
            request.Headers.Add("sec-ch-ua-mobile", "?0");
            request.Headers.Add("sec-ch-ua-platform", "\"Windows\"");
            request.Headers.Add("sec-fetch-dest", "empty");
            request.Headers.Add("sec-fetch-mode", "cors");
            request.Headers.Add("sec-fetch-site", "same-origin");
            request.Headers.Add("sec-gpc", "1");
            //request.Headers.Add("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/142.0.0.0 Safari/537.36");
            //request.Headers.Add("cookie", "__ddg1_=eChzS6lZ2WTuevS45Fuj; __ddg9_=95.31.21.200; __ddg10_=1762019749; __ddg8_=gxkth15nsqXD0nu4");

            HttpResponseMessage response = client.SendAsync(request).Result;
            response.EnsureSuccessStatusCode();
            string responseBody = response.Content.ReadAsStringAsync().Result;
            return responseBody;
        }
    }
}
