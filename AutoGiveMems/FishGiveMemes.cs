using Firesplash.UnityAssets.TwitchIntegration.DataTypes.IRC;
using HarmonyLib;
using MelonLoader;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
//using OBSWebsocketDotNet;
using ScoredProductions.StreamLinked.IRC;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using static MelonLoader.MelonLogger;
using static PlayersManager;

namespace AutoGiveMems
{
    public class FishGiveMemes : MelonMod
    {
        /// <summary>
        /// Статическая переменная для указанния на текущий класс
        /// </summary>
        static FishGiveMemes Instance;
        /// <summary>
        /// Статическая переменная для настроек мода
        /// </summary>
        public static Settings Settings;
        /// <summary>
        /// Переменная для контроля последнего удачного каста пользователя
        /// Необходима для убирания проблемы с тем, что КД начинает считаться только после анимации заброса
        /// </summary>
        public static Dictionary<string,DateTime> LastCast = new Dictionary<string, DateTime>();

        public override void OnApplicationStart()
        {
            Instance = this;
            // Патч приёма сообщений с Twitch
            MethodInfo methodOnMessageReceived = AccessTools.Method(typeof(TwitchConnectorEventSub), "OnMessageReceived");
            MethodInfo methodPrefixOnMessageReceived = this.GetType().GetMethod(nameof(OnMessageReceived));
            HarmonyMethod prefixOnMessageReceived = new HarmonyMethod(methodPrefixOnMessageReceived);
            this.HarmonyInstance.Patch(methodOnMessageReceived, prefixOnMessageReceived, null, null);

            // Патч шанса выпадения рыбы
            MethodInfo methodGetChance = AccessTools.Method(typeof(CatchLootItem), "GetChance");
            MethodInfo methodPostfixOnTotalChance = this.GetType().GetMethod(nameof(OnTotalChance));// Get the prefix here
            HarmonyMethod postfixOnTotalChance = new HarmonyMethod(methodPostfixOnTotalChance);
            this.HarmonyInstance.Patch(methodGetChance, null, postfixOnTotalChance, null);

            /* Патч уведомлений
             * Почему через него? Что бы не редактировать код Assembly_CSharp.dll
             * Просто ловим уведомление о том, что бросок уже добавлен, само уведомление не выводим и отсылаем запрос на ловлю без КД
            */
            MethodInfo methodQueueNotif = AccessTools.Method(typeof(NotificationController), "QueueNotif");
            MethodInfo methodPrefixQueueNotif = this.GetType().GetMethod(nameof(QueueNotif));// Get the prefix here
            HarmonyMethod prefixQueueNotif = new HarmonyMethod(methodPrefixQueueNotif);
            this.HarmonyInstance.Patch(methodQueueNotif, prefixQueueNotif, null, null);

            // Патч проверки КД заброса
            MethodInfo methodPushPlayer = AccessTools.Method(typeof(PlayersManager), "PushPlayer");
            MethodInfo methodPostfixOnPushPlayer = this.GetType().GetMethod(nameof(OnPushPlayer));// Get the prefix here
            HarmonyMethod postfixOnPushPlayer = new HarmonyMethod(methodPostfixOnPushPlayer);
            this.HarmonyInstance.Patch(methodPushPlayer, null, postfixOnPushPlayer, null);

            if (File.Exists("Settings.json"))
            {
                try
                {
                    Settings = JsonConvert.DeserializeObject<Settings>(File.ReadAllText("Settings.json"));
                }
                catch(Exception e)
                {
                    Instance.LoggerInstance.Msg($"Ошибка загрузки настроек. Обмен не будет работать. ({e.Message})\r\n{e.StackTrace}");
                }
                if(Settings.Curse <=0)
                    Settings.Curse = 1;
                if(Settings.MinExchange <=0)
                    Settings.MinExchange = 1;
                if(Settings.Chanse < 0)
                    Settings.Chanse = 0;
                File.WriteAllText("Settings.json", JsonConvert.SerializeObject(Settings, Formatting.Indented));
            }
            else
            {
                Instance.LoggerInstance.Msg($"Файл нестроек не найден, создаю новый (Settings.json)");
                Settings = new Settings();
                File.WriteAllText("Settings.json",JsonConvert.SerializeObject(Settings, Formatting.Indented));
            }
            if (GetStreamerID() == null)
            {
                Instance.LoggerInstance.Msg($"Ключ авторизации не действителен, вам необходимо переавторизоваться!");
                Thread th = new Thread(StartPipeServer);
                th.Start();
                Process.Start(".\\Authorization\\MemeAlerts.exe");
            }


            // Отлавливаем уведомление о изменении файла настроек, что бы обновлять их без перезапуска рыбалки
            FileSystemWatcher watcher = new FileSystemWatcher();
            watcher.Path = ".\\";
            watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite
       | NotifyFilters.FileName | NotifyFilters.DirectoryName;
            watcher.Filter = "Settings.json";
            watcher.Changed += new FileSystemEventHandler(OnChanged);
            watcher.EnableRaisingEvents = true;
        }
        private static void OnChanged(object source, FileSystemEventArgs e)
        {
            if (File.Exists("Settings.json"))
            {
                try
                {
                    Settings = JsonConvert.DeserializeObject<Settings>(File.ReadAllText("Settings.json"));
                }
                catch (Exception er)
                {
                    Instance.LoggerInstance.Msg($"Ошибка загрузки настроек. Обмен не будет работать. ({er.Message})\r\n{er.StackTrace}");
                }
                if (Settings.Authorization == "")
                    Instance.LoggerInstance.Msg($"У вас не установлен код авторизации. Выдача не будет работать.");
                if (Settings.StreamerID == "")
                    Instance.LoggerInstance.Msg($"У вас не установлен ID стримера. Выдача не будет работать.");

            }
        }
        public override void OnUpdate()
        {

        }
        /// <summary>
        /// Pipe Server, для получения кода авторизации от MemeAlerts
        /// </summary>
        private void StartPipeServer()
        {
            bool Stop = false;
            while (true)
            {
                using (var server = new NamedPipeServerStream("MemeAlertsAuth", PipeDirection.In))
                {
                    server.WaitForConnection();
                    using (var reader = new StreamReader(server))
                    {
                        string line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            //line
                            Settings.Authorization = line;
                            if (GetStreamerID() != null)
                            {
                                Settings.StreamerID = GetStreamerID();
                                File.WriteAllText("Settings.json", JsonConvert.SerializeObject(Settings, Formatting.Indented));
                                Instance.LoggerInstance.Msg($"Вы успешно авторизовались.");
                                Stop = true;
                            }
                            if (Stop)
                                return;
                        }
                    }
                }
                if (Stop)
                    return;
            }
        }
        /// <summary>
        /// Функция разчёта шанса выпадения
        /// </summary>
        public static void OnTotalChance(ref int __result, CatchLootItem __instance)
        {
            if(__instance.Catch.Rarity.boostable)
                __result = (int)Math.Round(__result * Settings.Chanse);
        }
        /// <summary>
        /// Функция перехвата уведомления, о том, что заброс уже есть в очереди
        /// </summary>
        public static bool QueueNotif(string message)
        {
            if (!Settings.IgnoringAlreadyCast)
                return true;

            if(message.StartsWith("Already in line @"))
            {
                string username = message.Replace("Already in line ","").Replace("@","");
                username = username.Substring(0, username.Length - 1);
                FishingQueueManager.Queue(username, QueueTrigger.Chat, true, 1);
                return false;
            }
            else
                return true;

        }

        /// <summary>
        /// Функция перехвата уведомления, о сообщение на Twitch
        /// </summary>
        public static bool OnMessageReceived(TwitchMessage com)
        {
            try
            {
                if (Settings == null)
                    return true;
                if (Settings.Authorization == "")
                {
                    Instance.LoggerInstance.Msg($"У вас не установлен код авторизации. Выдача не работаеть.");
                    return true;
                }
                if (Settings.StreamerID == "")
                {
                    Instance.LoggerInstance.Msg($"У вас не установлен ID стримера. Выдача не работаеть.");
                    return true;
                }
                if (com.ChatMessage != null && com.ChatMessage.ToLower().StartsWith(Settings.CommandExchange))
                {

                    if (com.Username != null)
                    {
                        string ForceID = null;
                        if (com.ChatMessage.ToLower().Split(" ").Length >= 2)
                        {
                            ForceID = com.ChatMessage.ToLower().Split(" ")[1];
                        }
                        var FindResults = FindUser(com.Username);
                        if (FindResults == null)
                        {
                            TwitchConnectorEventSub.Instance.SendTwitchMessage(Settings.MessageOnErrorMemeAlerts);
                            return false;
                        }
                        else if (FindResults.data.Count == 0)
                        {
                            Instance.LoggerInstance.Msg($"Пользователь {com.Username} не обнаружен на MemeAlerts.");
                            TwitchConnectorEventSub.Instance.SendTwitchMessage(Settings.MessageNotFoundUser);
                            return false;
                        }
                        else if (FindResults.data.Count > 1 && ForceID == null)
                        {
                            Instance.LoggerInstance.Msg($"С ником {com.Username} обнаружено 2 или более пользователей на MemeAlerts.");
                            TwitchConnectorEventSub.Instance.SendTwitchMessage(String.Format(Settings.MessageManyUsers, Settings.CommandExchange));
                            return false;
                        }
                        else
                        {
                            if (ForceID == null)
                            {
                                ForceID = FindResults.data[0].supporterId;
                            }
                            else
                            {
                                bool found = false;
                                foreach (var item in FindResults.data)
                                {
                                    if (item.supporterId == ForceID)
                                        found = true;
                                }
                                if (!found)
                                {
                                    TwitchConnectorEventSub.Instance.SendTwitchMessage(Settings.MessageNotFoundIDAndName);
                                }
                            }

                            if (!PlayersManager.Instance.Players.ContainsKey(com.Username.ToLower()))
                            {
                                Instance.LoggerInstance.Msg($"В рыбалке не обранужен игрок с ником {com.Username}.");
                                TwitchConnectorEventSub.Instance.SendTwitchMessage(Settings.MessagePlayerNotFound);
                                return false;
                            }
                            else
                            {
                                try
                                {
                                    int countGold = 0;
                                    countGold = PlayersManager.Instance.Players[com.Username.ToLower()].GetGold();
                                    if (countGold < (Settings.Curse * Settings.MinExchange))
                                    {
                                        Instance.LoggerInstance.Msg($"У игрока {com.Username} не достаточно Gold. У игрока {countGold}, а необходимо минимум {(Settings.Curse * Settings.MinExchange)}.");
                                        TwitchConnectorEventSub.Instance.SendTwitchMessage(String.Format(Settings.MessageNotEnoughGold, (Settings.Curse * Settings.MinExchange), countGold));
                                        return false;
                                    }
                                    int giveMems = countGold / (int)Settings.Curse;
                                    int PlayerGold = PlayersManager.Instance.Players[com.Username.ToLower()].gold;
                                    int CashBack = PlayerGold - (giveMems * (int)Settings.Curse);
                                    PlayersManager.Instance.Players[com.Username.ToLower()].gold = CashBack;
                                    Thread.Sleep(1000);
                                    string CountGive = FormatWithCount(giveMems, Settings.ValuteFormat1, Settings.ValuteFormat2, Settings.ValuteFormat5);
                                    if (GiveBonus(ForceID, giveMems))
                                    {

                                        TwitchConnectorEventSub.Instance.SendTwitchMessage(String.Format(Settings.MessageGived, CountGive));
                                    }
                                    else
                                    {
                                        TwitchConnectorEventSub.Instance.SendTwitchMessage(String.Format(Settings.MessageNotGived, CountGive));
                                    }
                                }
                                catch
                                (Exception e)
                                {
                                    Instance.LoggerInstance.Msg($"{e.Message}\r\n{e.StackTrace}");
                                }
                                return false;
                            }
                        }
                    }
                    Instance.LoggerInstance.Msg(JsonConvert.SerializeObject(PlayersManager.Instance.Players));
                    return false;
                }
            }
            catch { }
            return true;
        }

        /// <summary>
        /// Функция исправления КД. Оно начинает отсчёт только после анимации заброса
        /// </summary>
        public static void OnPushPlayer(ref bool __result, string username, bool ignoreTimeout)
        {
            if (ignoreTimeout || !Settings.FixTimeOutPostAnim)
                return;
            if(!__result && LastCast.ContainsKey(username))
            {
                if((DateTime.UtcNow - LastCast[username]).TotalSeconds> Preferences.Prefs.timeoutDuration)
                {
                    __result = PlayersManager.Instance.PushPlayer(username,true);
                }
            }
            if(__result)
            {
                if(LastCast.ContainsKey(username))
                {
                    LastCast[username] = DateTime.UtcNow;
                }
                else
                {
                    LastCast.Add(username, DateTime.UtcNow);
                }
            }
        }

        public string GetStreamerID()
        {
            try
            {
                HttpClient client = new HttpClient();

                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "https://memealerts.com/api/user/current");

                request.Headers.Add("accept", "*/*");
                request.Headers.Add("accept-language", "ru-RU,ru;q=0.8");
                request.Headers.Add("authorization", Settings.Authorization);
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
                var current = JsonConvert.DeserializeObject<Current>(responseBody);
                return current.id;
            }
            catch (Exception ex) 
            {
                return null;
            }
        }

        public static MemAlertFind FindUser(string UserName)
        {
            if (Settings == null)
                return null;
            try
            {
                HttpClient client = new HttpClient();
                client.Timeout = new TimeSpan(0, 0, 15);

                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "https://memealerts.com/api/supporters");

                request.Headers.Add("accept", "*/*");
                request.Headers.Add("accept-language", "ru-RU,ru;q=0.8");
                request.Headers.Add("authorization", Settings.Authorization);
                request.Headers.Add("origin", "https://memealerts.com");
                request.Headers.Add("priority", "u=1, i");
                request.Headers.Add("sec-ch-ua", "\"Brave\";v=\"141\", \"Not?A_Brand\";v=\"8\", \"Chromium\";v=\"141\"");
                request.Headers.Add("sec-ch-ua-mobile", "?0");
                request.Headers.Add("sec-ch-ua-platform", "\"Windows\"");
                request.Headers.Add("sec-fetch-dest", "empty");
                request.Headers.Add("sec-fetch-mode", "cors");
                request.Headers.Add("sec-fetch-site", "same-origin");
                request.Headers.Add("sec-gpc", "1");

                request.Content = new StringContent("{\"limit\":20,\"skip\":0,\"query\":\"" + UserName + "\",\"filters\":[0]}");
                request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                HttpResponseMessage response = client.SendAsync(request).Result;
                response.EnsureSuccessStatusCode();
                string responseBody = response.Content.ReadAsStringAsync().Result;
                MemAlertFind respon = JsonConvert.DeserializeObject<MemAlertFind>(responseBody);
                return respon;
            }
            catch { }
            return null;
        }

        public static bool GiveBonus(string id, int Count)
        {
            if (Settings == null)
                return false;
            try
            {
                HttpClient client = new HttpClient();
                client.Timeout = new TimeSpan(0, 0, 15);
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "https://memealerts.com/api/user/give-bonus");
                
                request.Headers.Add("accept", "*/*");
                request.Headers.Add("accept-language", "ru-RU,ru;q=0.8");
                request.Headers.Add("authorization", Settings.Authorization);
                request.Headers.Add("origin", "https://memealerts.com");
                request.Headers.Add("priority", "u=1, i");
                request.Headers.Add("sec-ch-ua", "\"Brave\";v=\"141\", \"Not?A_Brand\";v=\"8\", \"Chromium\";v=\"141\"");
                request.Headers.Add("sec-ch-ua-mobile", "?0");
                request.Headers.Add("sec-ch-ua-platform", "\"Windows\"");
                request.Headers.Add("sec-fetch-dest", "empty");
                request.Headers.Add("sec-fetch-mode", "cors");
                request.Headers.Add("sec-fetch-site", "same-origin");
                request.Content = new StringContent("{\"userId\":\"" + id + "\",\"streamerId\":\""+ Settings.StreamerID + "\",\"value\":" + Count + "}");
                request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                HttpResponseMessage response = client.SendAsync(request).Result;
                response.EnsureSuccessStatusCode();
                string responseBody = response.Content.ReadAsStringAsync().Result;
                return responseBody == "true";
            }
            catch {  return false; }
        }

        public static string FormatWithCount(int count, string form1, string form2, string form5)
        {
            // form1 — для 1 (например, "енотик", "яблоко")
            // form2 — для 2, 3, 4 (например, "енотика", "яблока")
            // form5 — для 5 и больше (например, "енотиков", "яблок")

            int lastTwo = count % 100;
            int last = count % 10;

            string word;

            if (lastTwo >= 11 && lastTwo <= 19)
            {
                word = form5;
            }
            else
            {
                switch (last)
                {
                    case 1:
                        word = form1;
                        break;
                    case 2:
                    case 3:
                    case 4:
                        word = form2;
                        break;
                    default:
                        word = form5;
                        break;
                }
            }

            return $"{count} {word}";
        }
    }
}
