﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Discord;
using Microsoft.AspNetCore.Mvc;
using SoraBot.Common.Extensions.Modules;
using SoraBot.Data.Dtos;
using SoraBot.Data.Models.SoraDb;
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
        private const int _REQUEST_REWARD = 2000;

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

        private async Task NotifyUser(ulong userId, string waifuName, bool accepted)
        {
            try
            {
                var user = await _userService.GetOrSetAndGet(userId);
                if (!user) return;
                var eb = new EmbedBuilder()
                {
                    Color = accepted ? SoraSocketCommandModule.Green : SoraSocketCommandModule.Red,
                    Title = accepted ?
                        $"You request for {waifuName} has been accepted. You've been awarded with {_REQUEST_REWARD.ToString()} SC." :
                        $"You request for {waifuName} has been rejected."
                };
                await (~user).SendMessageAsync(embed: eb.Build());
            }
            catch (Exception)
            {
                // Ignore since user could have DMs disabled 
            }
        }

        [HttpPatch("{requestId}/approve")]
        public async Task<IActionResult> ApproveRequest(ulong requestId)
        {
            // Check if request exists
            var req = await _waifuRequestRepo.GetWaifuRequest(requestId);
            if (!req)
                return NotFound("Request doesn't exist");

            var wr = ~req;
            
            // Check if waifu already exists!
            if (await _waifuRequestRepo.WaifuExists(wr.Name.Trim()))
                return BadRequest("Waifu already exists!");
            
            // Change the state 
            await _waifuRequestRepo.ChangeRequestStatus(requestId, RequestState.Accepted);
            // Give user reward
            await _coinRepository.GiveAmount(wr.UserId, _REQUEST_REWARD);
            // add waifu
            await _waifuRequestRepo.AddWaifu(wr);
            // check if user wants to be notified
            bool notify = await _waifuRequestRepo.UserHasNotificationOn(wr.UserId);
            // notify
            if (notify)
                await this.NotifyUser(wr.UserId, wr.Name, true);
            
            return Ok();
        }
        
        
        [HttpPatch("{requestId}/reject")]
        public async Task<IActionResult> RejectRequest(ulong requestId)
        {
            // Check if request exists
            var req = await _waifuRequestRepo.GetWaifuRequest(requestId);
            if (!req)
                return NotFound("Request doesn't exist");
            var wr = ~req;
    
            // Change the state 
            await _waifuRequestRepo.ChangeRequestStatus(requestId, RequestState.Rejected);
            // check if user wants to be notified
            bool notify = await _waifuRequestRepo.UserHasNotificationOn(wr.UserId);
            // notify
            if (notify)
                await this.NotifyUser(wr.UserId, wr.Name, false);
            
            return Ok();
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
            // Check if request exists
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