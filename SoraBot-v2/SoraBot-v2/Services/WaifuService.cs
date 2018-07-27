using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SoraBot_v2.Data;
using SoraBot_v2.Data.Entities.SubEntities;

namespace SoraBot_v2.Services
{
    
    public class WaifuService
    {
        // TODO unbox, sell, maybe trade, maybe fav
        private List<Waifu> _boxCache = new List<Waifu>();
        
        public void Initialize()
        {
            // initial setup
            CreateRandomCache(null);
            // setup timer
        }

        public void CreateRandomCache(Object stateInfo)
        {
            using (var soraContext = new SoraContext())
            {
                
            }
        }
        
        public async Task UnboxWaifu()
        {
            
        }
    }
}