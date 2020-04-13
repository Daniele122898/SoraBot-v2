using SoraBot.Data.Models.SoraDb;

namespace SoraBot.Services.Waifu
{
    public static class WaifuFormatter
    {
        public static string GetRarityString(WaifuRarity rarity)
        {
            switch (rarity)
            {
                case WaifuRarity.Common:
                    return "Common";
                case WaifuRarity.Uncommon:
                    return "Uncommon";
                case WaifuRarity.Rare:
                    return "Rare";
                case WaifuRarity.Epic:
                    return "Epic";
                case WaifuRarity.UltimateWaifu:
                    return "Ultimate Waifu";
                case WaifuRarity.Halloween:
                    return "Halloween";
                case WaifuRarity.Christmas:
                    return "Christmas";
                case WaifuRarity.Summer:
                    return "Summer";
            }
            return "";
        }

        
        public static WaifuRarity GetRarityByInt(int rarity)
        {
            switch (rarity)
            {
                case 0:
                    return WaifuRarity.Common;
                case 1:
                    return WaifuRarity.Uncommon;
                case 2:
                    return WaifuRarity.Rare;
                case 3:
                    return WaifuRarity.Epic;
                case 99:
                    return WaifuRarity.UltimateWaifu;
                case 5:
                    return WaifuRarity.Halloween;
                case 6:
                    return WaifuRarity.Christmas;
                case 7:
                    return WaifuRarity.Summer;
            }
            return WaifuRarity.Common;
        }
    }
}