using AutoGiveMems;
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
        string FileSettings = "Settings.json";
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
                            if (!ToGame.SendAsync(Bearer).Result)
                            {
                                AutoGiveMems.Settings settings = new AutoGiveMems.Settings();
                                if (!File.Exists(FileSettings))
                                {
                                    if(File.Exists(".\\..\\Settings.json"))
                                    {
                                        FileSettings = ".\\..\\Settings.json";
                                    }
                                }
                                if (File.Exists(FileSettings))
                                {

                                    try
                                    {
                                        settings = JsonConvert.DeserializeObject<AutoGiveMems.Settings>(File.ReadAllText(FileSettings));
                                    }
                                    catch (Exception err)
                                    {
                                        MessageBox.Show($"{err.Message}: \r\n {err.StackTrace}\r\nСгенерирован новый файл Settings.json");
                                        settings = new AutoGiveMems.Settings();
                                    }
                                    settings.Authorization = Bearer;
                                    settings.StreamerID = GetStreamerID(settings);
                                    if (String.IsNullOrWhiteSpace(settings.StreamerID))
                                    {
                                        MessageBox.Show("Ошибка получения StreamerID.", "Мы не смоогли получить StreamerID, обратитесь к разработчику",MessageBoxButtons.OK, MessageBoxIcon.Error);
                                        this.Close();
                                    }
                                    File.WriteAllText(FileSettings, JsonConvert.SerializeObject(settings, Formatting.Indented));
                                }
                                else
                                {
                                    settings.Authorization = Bearer;
                                    File.WriteAllText(FileSettings, JsonConvert.SerializeObject(settings, Formatting.Indented));
                                }
                                this.Close();
                            }
                            else
                            {
                                this.Close();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }
        public static string GetStreamerID(Settings Settings)
        {
            try
            {
                HttpClient client = new HttpClient();

                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "https://memealerts.com/api/user/current");

                request.Headers.Add("accept", "*/*");
                request.Headers.Add("accept-language", "ru-RU,ru;q=0.8");
                request.Headers.Add("authorization", Settings.Authorization);
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
                var current = JsonConvert.DeserializeObject<Current>(responseBody);
                return current.id;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public string GetStreamerID()
        {
            HttpClient client = new HttpClient();

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "https://memealerts.com/api/user/current");

            request.Headers.Add("accept", "*/*");
            request.Headers.Add("accept-language", "ru-RU,ru;q=0.8");
            request.Headers.Add("authorization", Bearer);
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

        private void webView21_Click(object sender, EventArgs e)
        {

        }
    }
}
