using System;
using SoraBot.Data.Models.SoraDb;

namespace SoraBot.WebApi.Dtos
{
    public class WaifuRequestDto
    {
        public uint Id { get; set; }
        public RequestState RequestState { get; set; }
        public DateTime RequestTime { get; set; }

        public DateTime? ProcessedTime { get; set; }
        
        public string Name { get; set; }
        public string ImageUrl { get; set; }
        public WaifuRarity Rarity { get; set; }
        public string RejectReason { get; set; }

        public ulong UserId { get; set; }

    }
}