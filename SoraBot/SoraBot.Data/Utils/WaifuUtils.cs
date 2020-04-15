using SoraBot.Data.Models.SoraDb;

namespace SoraBot.Data.Utils
{
    public static class WaifuUtils
    {
        public static uint GetWaifuQuickSellCost(WaifuRarity rarity)
        {
            switch (rarity)
            {
                case WaifuRarity.Common:
                    return 50;
                case WaifuRarity.Uncommon:
                    return 100;
                case WaifuRarity.Rare:
                    return 200;
                case WaifuRarity.Epic:
                    return 500;
                case WaifuRarity.UltimateWaifu:
                    return 1500;
                case WaifuRarity.Halloween:
                case WaifuRarity.Christmas:
                case WaifuRarity.Summer:
                    return 400;
                default:
                    return 200;
            }
        }

        public static bool IsSpecialOrUltiWaifu(WaifuRarity rarity)
            => (rarity != WaifuRarity.Common && rarity != WaifuRarity.Uncommon
                                             && rarity != WaifuRarity.Rare && rarity != WaifuRarity.Epic);
    }
}