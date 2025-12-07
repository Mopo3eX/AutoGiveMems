using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace AutoGiveMems
{
    public class Asset
    {
        public string assetId { get; set; }
        public List<object> colorMap { get; set; }
    }

    public class AvatarAssets
    {
        public List<Asset> assets { get; set; }
        public string faceId { get; set; }
        public string faceEmotionId { get; set; }
        public string eyeColorId { get; set; }
        public string bodyColorId { get; set; }
        public object customEyeColor { get; set; }
        public object customBodyColor { get; set; }
    }

    public class Channel
    {
        public string name { get; set; }
        public string link { get; set; }
        public string uniqueLink { get; set; }
        public string donationAlertsUsername { get; set; }
        public int donationAlertsUserId { get; set; }
        public bool onlyChannelStickers { get; set; }
        public string avatarUrl { get; set; }
        public bool onlyTwitchFriendly { get; set; }
        public int backgroundMode { get; set; }
        public string currencyImageUrl { get; set; }
        public string currencyName { get; set; }
        public CurrencyNameDeclensions currencyNameDeclensions { get; set; }
        public bool memePartyActive { get; set; }
        public bool memePartySoundEnabled { get; set; }
        public bool memePartyOwnSoundEnabled { get; set; }
        public bool memePartyConfetti { get; set; }
        public MemePartyProgressPosition memePartyProgressPosition { get; set; }
        public int memePartyGoal { get; set; }
        public int memePartyDuration { get; set; }
        public int memePartySendDelay { get; set; }
        public bool isPopupNickNameEnabled { get; set; }
        public bool isPopupNewSaleEnabled { get; set; }
        public bool isBonusAlertEnabled { get; set; }
        public int popupNickNameAreaMask { get; set; }
        public int etmaRating { get; set; }
        public bool fullscreenStickersEnabled { get; set; }
        public string refCode { get; set; }
        public bool welcomeBonusEnabled { get; set; }
        public bool welcomeBonusVerificationEnabled { get; set; }
        public int welcomeBonus { get; set; }
        public bool newbieActionEnabled { get; set; }
        public int newbieAction { get; set; }
        public int stickerVoicingPrice { get; set; }
        public string twitchLink { get; set; }
        public string paywallText { get; set; }
        public bool masteredObsSettings { get; set; }
        public bool disableStickers { get; set; }
        public bool isAntiSwearEnabled { get; set; }
        public string antiSwearBadWords { get; set; }
        public bool antiSwearRemoveLinks { get; set; }
        public bool isStickersMessagesEnabled { get; set; }
        public bool isStickersMessagesTtsEnabled { get; set; }
        public List<object> forbiddenTags { get; set; }
        public bool isNewSupporterAlertEnable { get; set; }
        public bool isSoundOnlyStickersEnabled { get; set; }
        public string supporterName { get; set; }
        public SupporterNameDeclensions supporterNameDeclensions { get; set; }
        public List<string> forbiddenContentMarks { get; set; }
        public int preModerationType { get; set; }
        public string directMessageMode { get; set; }
        public int directMessagePrice { get; set; }
        public int directMessageWithAttachmentPrice { get; set; }
        public string directMessageWelcomeText { get; set; }
        public int stickerPriceLevel { get; set; }
        public string fullscreenPriceLevel { get; set; }
        public string memeCannonPriceLevel { get; set; }
        public bool isSupportersTopEnabled { get; set; }
        public bool isStickersGlobalBlacklistEnabled { get; set; }
    }

    public class CurrencyNameDeclensions
    {
        public string genitive { get; set; }
        public string dative { get; set; }
        public string accusative { get; set; }
        public string instrumental { get; set; }
        public string prepositional { get; set; }
        public Multiple multiple { get; set; }
        public bool successProcessed { get; set; }
    }

    public class MemePartyProgressPosition
    {
        public double x { get; set; }
        public double y { get; set; }
    }

    public class Multiple
    {
        public string nominative { get; set; }
        public string genitive { get; set; }
        public string dative { get; set; }
        public string accusative { get; set; }
        public string instrumental { get; set; }
        public string prepositional { get; set; }
    }

    public class NotificationSettings
    {
        public Telegram telegram { get; set; }
    }

    public class Parameters
    {
        public string provider { get; set; }
        public string output { get; set; }
    }

    public class Current
    {
        public string _id { get; set; }
        public string id { get; set; }
        public string name { get; set; }
        public string username { get; set; }
        public string avatar { get; set; }
        public DateTime createdAt { get; set; }
        public Channel channel { get; set; }
        public long lastVisit { get; set; }
        public AvatarAssets avatarAssets { get; set; }
        public Voice voice { get; set; }
        public bool isUserVerified { get; set; }
        public NotificationSettings notificationSettings { get; set; }
    }

    public class SupporterNameDeclensions
    {
        public string genitive { get; set; }
        public string dative { get; set; }
        public string accusative { get; set; }
        public string instrumental { get; set; }
        public string prepositional { get; set; }
        public Multiple multiple { get; set; }
        public bool successProcessed { get; set; }
    }

    public class Telegram
    {
        [JsonProperty("buy-currency")]
        public bool buycurrency { get; set; }

        [JsonProperty("bonus-earned")]
        public bool bonusearned { get; set; }

        [JsonProperty("moderation-status")]
        public bool moderationstatus { get; set; }
        public bool generic { get; set; }

        [JsonProperty("direct-message")]
        public bool directmessage { get; set; }
    }

    public class Voice
    {
        public string id { get; set; }
        public string provider { get; set; }
        public string voice { get; set; }
        public string name { get; set; }
        public Parameters parameters { get; set; }
        public List<string> tags { get; set; }
        public string phrase { get; set; }
    }
}
