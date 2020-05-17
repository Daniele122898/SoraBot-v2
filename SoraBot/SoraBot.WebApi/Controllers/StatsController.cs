using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace SoraBot.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("AllowLocal")]
    public class StatsController : ControllerBase
    {
        
    }
}