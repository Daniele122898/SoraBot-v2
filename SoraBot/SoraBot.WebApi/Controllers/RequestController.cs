using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SoraBot.Data.Repositories.Interfaces;
using SoraBot.Services.Users;
using SoraBot.WebApi.Dtos;

namespace SoraBot.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RequestController : ControllerBase
    {
        private readonly IWaifuRequestRepository _waifuRequestRepo;
        private readonly IUserService _userService;
        private readonly IUserRepository _userRepo;
        private readonly ICoinRepository _coinRepository;
        private readonly IMapper _mapper;

        public RequestController(
            IWaifuRequestRepository waifuRequestRepo,
            IUserService userService,
            IUserRepository userRepo,
            ICoinRepository coinRepository,
            IMapper mapper)
        {
            _waifuRequestRepo = waifuRequestRepo;
            _userService = userService;
            _userRepo = userRepo;
            _coinRepository = coinRepository;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<WaifuRequestDto>>> GetAllRequests()
        {
            var reqs = await _waifuRequestRepo.AllActiveRequests();
            if (!reqs)
                return NotFound("There are currently no active requests");

            var reqsToReturn = _mapper.Map<IEnumerable<WaifuRequestDto>>(reqs.Some());

            return Ok(reqsToReturn);
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

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<WaifuRequestDto>>> GetAllUserRequests(ulong userId)
        {
            var reqs = await _waifuRequestRepo.GetUserWaifuRequests(userId);
            if (!reqs)
                return NotFound("User has no requests");

            var reqsToReturn = _mapper.Map<IEnumerable<WaifuRequestDto>>(reqs.Some());
            
            return Ok(reqsToReturn); 
        }
    }
}