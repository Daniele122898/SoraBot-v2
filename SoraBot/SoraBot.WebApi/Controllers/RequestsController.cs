using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SoraBot.Data.Dtos;
using SoraBot.Data.Repositories.Interfaces;
using SoraBot.Services.Users;
using SoraBot.WebApi.Dtos;

namespace SoraBot.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RequestsController : ControllerBase
    {
        private readonly IWaifuRequestRepository _waifuRequestRepo;
        private readonly IUserService _userService;
        private readonly IUserRepository _userRepo;
        private readonly ICoinRepository _coinRepository;
        private readonly IMapper _mapper;

        public RequestsController(
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
        
        [HttpPut("{requestId}/admin")]
        public async Task<IActionResult> EditWaifuRequestAdmin(ulong requestId,
            [FromBody] WaifuRequestEditDto waifuRequestEditDto)
        {
            // Check if request exists and belongs to the right user
            if (!ulong.TryParse(waifuRequestEditDto.UserId, out var userId))
                return BadRequest("UserId invalid");

            if (!await _waifuRequestRepo.RequestExists(requestId))
                return NotFound("User request was not found");

            await _waifuRequestRepo.EditWaifuRequest(waifuRequestEditDto);
            
            return Ok();
        }

        [HttpPut("{requestId}")]
        public async Task<IActionResult> EditWaifuRequest(ulong requestId,
            [FromBody] WaifuRequestEditDto waifuRequestEditDto)
        {
            // Check if request exists and belongs to the right user
            if (!ulong.TryParse(waifuRequestEditDto.UserId, out var userId))
                return BadRequest("UserId invalid");

            if (!await _waifuRequestRepo.RequestExistsAndBelongsToUser(requestId, userId))
                return NotFound("User request was not found");

            await _waifuRequestRepo.EditWaifuRequest(waifuRequestEditDto);
            
            return Ok();
        }
        
        [HttpPost("user/{userId}")]
        public async Task<IActionResult> PostWaifuRequest(ulong userId, [FromBody] WaifuRequestAddDto waifuRequestAddDto)
        {
            if (await _waifuRequestRepo.UserRequestCountLast24Hours(userId) >= 3)
                return this.BadRequest(
                    "You already have 3 pending requests in the last 24 hours. Wait for them to be processed or after 24 hours have passed.");
            
            // Check if waifu already exists
            waifuRequestAddDto.Name = waifuRequestAddDto.Name.Trim();
            if (await _waifuRequestRepo.WaifuExists(waifuRequestAddDto.Name.Trim()))
                return BadRequest("Waifu already exists");

            await _waifuRequestRepo.AddWaifuRequest(waifuRequestAddDto);
            
            return Ok(); 
        }
    }
}