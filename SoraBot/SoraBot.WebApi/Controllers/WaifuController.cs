using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SoraBot.Data.Models.SoraDb;
using SoraBot.Services.Waifu;

namespace SoraBot.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WaifuController : ControllerBase
    {
        private readonly IWaifuService _waifuService;

        // TODO this is quite heavy. might want to get a lighter way to fetch all waifus.
        public WaifuController(IWaifuService waifuService)
        {
            _waifuService = waifuService;
        }

        [HttpGet]
        public async Task<ActionResult<List<Waifu>>> GetAllWaifus()
        {
            var waifus = await _waifuService.GetAllWaifus().ConfigureAwait(false);
            if (waifus == null || waifus.Count == 0)
                return NotFound("No Waifus found");
            // Sort in-place by rarity descending
            waifus.Sort((x,y) => -x.Rarity.CompareTo(y.Rarity));
            return Ok(waifus);
        }
    }
}