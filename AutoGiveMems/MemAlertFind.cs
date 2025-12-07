using System;
using System.Collections.Generic;
using System.Text;

namespace AutoGiveMems
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class Datum
    {
        public string _id { get; set; }
        public int balance { get; set; }
        public bool welcomeBonusEarned { get; set; }
        public bool newbieActionUsed { get; set; }
        public int spent { get; set; }
        public int purchased { get; set; }
        public long joined { get; set; }
        public long lastSupport { get; set; }
        public bool isMutedByStreamer { get; set; }
        public string supporterName { get; set; }
        public string supporterAvatar { get; set; }
        public string supporterLink { get; set; }
        public string supporterId { get; set; }
        public List<object> mutes { get; set; }
        public bool mutedByStreamer { get; set; }
    }

    public class MemAlertFind
    {
        public List<Datum> data { get; set; }
        public int total { get; set; }
    }
}
