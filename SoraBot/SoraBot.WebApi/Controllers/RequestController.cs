using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SoraBot.Data.Repositories.Interfaces;
using SoraBot.Services.Users;

namespace SoraBot.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RequestController : ControllerBase
    {
        private readonly IWaifuRequestRepository _waifuRequestRepo;
        private readonly IUserService _userService;
        private readonly IUserRepository _userRepo;

        public RequestController(
            IWaifuRequestRepository waifuRequestRepo,
            IUserService userService,
            IUserRepository userRepo)
        {
            _waifuRequestRepo = waifuRequestRepo;
            _userService = userService;
            _userRepo = userRepo;
        }

        [HttpPost("user/{userId}/notify")]
        public async Task<IActionResult> SetUserNotify(ulong userId, bool notify)
        {
            // Check if user exists
            var user = await _userService.GetOrSetAndGet(userId);
            if (!user)
                return NotFound("User doesn't exist in Sora's reach");

            // Make sure he exists in DB
            await _userRepo.GetOrCreateUser(userId);
            
            // Now we set the notification
            if (notify)
                await _waifuRequestRepo.ActivateUserNotification(userId);
            else
                await _waifuRequestRepo.RemoveUserNotification(userId);
            
            return Ok();
        }
    }
}