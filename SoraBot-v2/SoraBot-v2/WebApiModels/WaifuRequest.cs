using System;
using SoraBot_v2.Data.Entities.SubEntities;

namespace SoraBot_v2.WebApiModels
{

    public class WaifuRequest
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ImageUrl { get; set; }
        public WaifuRarity Rarity { get; set; }
        public ulong UserId { get; set; }
        public DateTime TimeStamp { get; set; }
    }
    
    public class WaifuRequestWeb
    {
        public string Name { get; set; }
        public string ImageUrl { get; set; }
        public short Rarity { get; set; }
        public string UserId { get; set; }
        public string Id { get; set; }
    }

    public class WaifuRequestResponse
    {
        public bool Success { get; set; }
        public string Error { get; set; }
        public string RequestId { get; set; }
    }
}