using AutoMapper;
using SoraBot.Data.Models.SoraDb;
using SoraBot.WebApi.Dtos;

namespace SoraBot.WebApi.Extensions
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            this.AddRequestMaps();
            this.AddWaifuMaps();
        }

        private void AddWaifuMaps()
        {
            CreateMap<Waifu, WaifuDto>();
        }

        private void AddRequestMaps()
        {
            throw new System.NotImplementedException();
        }
    }
}