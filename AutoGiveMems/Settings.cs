using System;
using System.Collections.Generic;
using System.Text;

namespace AutoGiveMems
{
    public class Settings
    {
        public string Authorization="";
        public string StreamerID="";
        public uint Curse = 100;
        public uint MinExchange = 1;
        public float Chanse = 1.0f;
        public bool IgnoringAlreadyCast = false;
        public bool FixTimeOutPostAnim = false;
        public string ValuteFormat1 = "енотик";
        public string ValuteFormat2 = "енотика";
        public string ValuteFormat5 = "енотиков";
        public string CommandExchange = "!exchange";
        public string MessageOnErrorMemeAlerts = "Ошибка подключения к MemeAlerts, передайте информацию стримеру";
        public string MessageNotFoundUser = "К сожалению, мы не обнаружили ваш аккаунт на MemeAlerts. Ваш ник на Twitch должен быть такой же, как на MemeAlerts или вам необходимо получить бонус на MemeAlerts";
        public string MessageManyUsers = "К сожалению, с вашим ником 2 или более саппортера, вам необходимо использовать комманду: {0} [ID]";
        public string MessageNotFoundIDAndName = "К сожалению, с таким ником и ID не существует саппортера.";
        public string MessagePlayerNotFound = "Вы ещё не поймали ни одной рыбки :(";
        public string MessageNotEnoughGold = "У вас не достаточно золота в игре. (Необходимо {0}, а у вас {1} золота)";
        public string MessageGived = "Ваши {0} уже на вашем счету!";
        public string MessageNotGived = "Ожидайте, в течении 15 минут, вам будет начислено {0} на MemeAlerts.";
    }
}
