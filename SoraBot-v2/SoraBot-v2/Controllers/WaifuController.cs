using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Rest;
using Discord.WebSocket;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SoraBot_v2.Data;
using SoraBot_v2.Data.Entities.SubEntities;
using SoraBot_v2.Dtos;
using SoraBot_v2.Services;
using SoraBot_v2.WebApiModels;

namespace SoraBot_v2.Controllers
{
    
    [Route("/api/[controller]")]
    [EnableCors("AllowLocal")]
    public class WaifuController : Controller
    {

        private readonly WaifuRarity _currentSpecial;
        
        private readonly DiscordSocketClient _client;
        private readonly DiscordRestClient _restClient;

        public WaifuController(DiscordSocketClient client, DiscordRestClient restClient)
        {
            _client = client;
            _restClient = restClient;
            
            if (int.TryParse(ConfigService.GetConfigData("specialWaifuType"), out int specialType))
            {
                WaifuRarity rarity = WaifuService.GetRarityByInt(specialType);
                _currentSpecial = rarity;
            }
            else
            {
                _currentSpecial = WaifuRarity.Summer;
            }
        }

        [HttpPost("setRequestNotify", Name = "setRequestNotify")]
        public async Task<WaifuRequestResponse> SetRequestNotify([FromBody] RequestNotifyDto requestNotify)
        {
            using (var soraContext = new SoraContext())
            {
                // set the notify bool
                var user = await soraContext.Users.SingleOrDefaultAsync(x => x.UserId == requestNotify.UserId);
                user.NotifyOnWaifuRequest = requestNotify.NotifyOnWaifuRequest;
                await soraContext.SaveChangesAsync();
            }

            return new WaifuRequestResponse()
            {
                Success = true,
                Error = ""
            };
        }
        
        [HttpGet("getAdminRequests/{userId}", Name = "getAdminRequests")]
        public List<WaifuRequestWeb> GetAdminRequests(ulong userId)
        {
            try
            {
                // if its not the owner id we dont return anything...
                if (userId != Utility.OWNER_ID) return null;
                using (var soraContext = new SoraContext())
                {
                    var reqs = soraContext.WaifuRequests.ToList();

                    var resp = new List<WaifuRequestWeb>();

                    foreach (var req in reqs)
                    {
                        resp.Add(new WaifuRequestWeb()
                        {
                            Id = req.Id.ToString(),
                            ImageUrl = req.ImageUrl,
                            Name = req.Name,
                            Rarity = req.Rarity
                        });
                    }

                    return resp;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }
        
        [HttpGet("getAllRequests/{userId}", Name = "getAllRequests")]
        public GetAllRequestsWeb GetAllWaifuRequestsForUser(ulong userId)
        {
            try
            {
                using (var soraContext = new SoraContext())
                {
                    var reqs = soraContext.WaifuRequests.Where(x => x.UserId == userId).ToList();
                    // get waifu requests
                    var resp = new List<WaifuRequestWeb>();

                    foreach (var req in reqs)
                    {
                        resp.Add(new WaifuRequestWeb()
                        {
                            Id = req.Id.ToString(),
                            ImageUrl = req.ImageUrl,
                            Name = req.Name,
                            Rarity = req.Rarity
                        });
                    }
                    
                    // get logs
                    var logs = soraContext.RequestLogs.Where(x=> x.UserId == userId).ToList();
                    
                    // get user notification preference
                    var notify = soraContext.Users.SingleOrDefault(x => x.UserId == userId)?.NotifyOnWaifuRequest;

                    return new GetAllRequestsWeb
                    {
                        RequestLogs = logs,
                        WaifuRequests = resp,
                        NotifyOnWaifuRequest = notify ?? false
                    };

                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        [HttpPost("requestApproval/", Name = "requestApproval")]
        public async Task<WaifuRequestResponse> PostRequestApproval([FromBody] RequestApproval approval)
        {

            void CreateLog(SoraContext soraContext, WaifuRequest req, RequestApproval app)
            {
                // first get requests from user to see if we need to delete an old one
                var logs = soraContext.RequestLogs.Where(x => x.UserId == req.UserId).ToList();
                if (logs.Count == 10)
                {
                    // sort list so oldest is first element
                    logs.Sort((x, y) => DateTime.Compare(x.ProcessedTime, y.ProcessedTime));
                    // remove oldest element
                    soraContext.RequestLogs.Remove(logs[0]);
                }
                // create new log
                soraContext.RequestLogs.Add(new RequestLog
                {
                    Accepted = app.Accept,
                    ProcessedTime = DateTime.UtcNow,
                    UserId = req.UserId,
                    WaifuName = req.Name
                });
            }
            
            // function to notify user with error handling
            async Task NotifyUser(ulong userId, string waifuName, bool accepted) 
            {
                try
                {
                    // get user
                    var user = await _restClient.GetUserAsync(userId);
                    // try and send message
                    await user.SendMessageAsync("", embed: Utility.ResultFeedback(
                        (accepted ? Utility.GreenSuccessEmbed : Utility.RedFailiureEmbed),
                        Utility.SuccessLevelEmoji[(accepted ? 0 : 2)],
                        $"Your request for \"{waifuName}\" has been {(accepted ? "accepted" : "declined")}." +
                        $"{(accepted ? " You are awarded with 1000 SC." : "")}").Build());
                }
                catch (Exception e)
                {
                    // ignored
                }
            }
            
            try
            {
                if (ulong.Parse(approval.UserId) != Utility.OWNER_ID) return null;
                using (var soraContext = new SoraContext())
                {
                    var req = soraContext.WaifuRequests.FirstOrDefault(x => x.Id == int.Parse(approval.WaifuId));
                    if (req == null)
                    {
                        return new WaifuRequestResponse()
                        {
                            Success = false,
                            Error = "This request does not exist"
                        };
                    }
                    
                    // check if user wants to be notified
                    bool notify = (await soraContext.Users.SingleOrDefaultAsync(x => x.UserId == req.UserId)).NotifyOnWaifuRequest;
                    
                    // else check if accept or decline
                    if (!approval.Accept)
                    {
                        // create log
                        CreateLog(soraContext, req, approval);
                        // decline
                        soraContext.WaifuRequests.Remove(req);
                        await soraContext.SaveChangesAsync();
                        // notify user if wanted
                        if (notify)
                        {
                            await NotifyUser(req.UserId, req.Name, approval.Accept);
                        }
                        // return success
                        return new WaifuRequestResponse()
                        {
                            Success = true,
                            Error = ""
                        };
                    }
                    // check if already in DB
                    if (soraContext.Waifus.Any(x =>
                        x.Name.Equals(req.Name.Trim(), StringComparison.OrdinalIgnoreCase)))
                    {
                        return new WaifuRequestResponse()
                        {
                            Success = false,
                            Error = "This waifu already exists"
                        };
                    }
                    // create log
                    CreateLog(soraContext, req, approval);
                    // else we accepted
                    var userDb = Utility.GetOrCreateUser(req.UserId, soraContext);
                    // give him money
                    userDb.Money += 1000;
                    // now we add it to the waifu db
                    soraContext.Waifus.Add(new Waifu()
                    {
                        ImageUrl = req.ImageUrl,
                        Name = req.Name,
                        Rarity = GetOfficialRarity(req.Rarity)
                    });
                    // and remove it from the requests
                    soraContext.WaifuRequests.Remove(req);
                    // save all the changes
                    await soraContext.SaveChangesAsync();
                    
                    // notify user if wanted
                    if (notify)
                    {
                        await NotifyUser(req.UserId, req.Name, approval.Accept);
                    }
                    
                    return new WaifuRequestResponse()
                    {
                        Success = true,
                        Error = ""
                    };
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }

        [HttpPost("editWaifu/", Name = "editWaifu")]
        public async Task<WaifuRequestResponse> PostWaifuEdit([FromBody] WaifuRequestWeb request)
        {
            try
            {
                using (var soraContext = new SoraContext())
                {
                    // get user id
                    ulong uid = ulong.Parse(request.UserId);
                    // check if the user even has this request. otherwise error out
                    var req = soraContext.WaifuRequests.FirstOrDefault(x =>x.Id == int.Parse(request.Id));
                    // it straight out doesnt exist
                    if (req == null)
                    {
                        return new WaifuRequestResponse()
                        {
                            Success = false,
                            Error = "This request does not exist"
                        };
                    }
                    // now check if he's owner or its the bot owner
                    if (req.UserId != uid && uid != Utility.OWNER_ID) 
                        return new WaifuRequestResponse()
                            {
                                Success = false,
                                Error = "This is not your request"
                            };
                    // else we can edit the entry
                    req.Name = request.Name;
                    req.Rarity = request.Rarity;
                    req.ImageUrl = request.ImageUrl;
                    await soraContext.SaveChangesAsync();
                    //return success
                    return new WaifuRequestResponse()
                    {
                        Success = true,
                        Error = ""
                    };
                }
            }
            catch (Exception)
            {
                return new WaifuRequestResponse()
                {
                    Success = false,
                    Error = "Something went horribly wrong :("
                };
            }
        }

        [HttpPost("waifuRequest/", Name = "waifuRequest")]
        public async Task<WaifuRequestResponse> PostWaifuRequest([FromBody] WaifuRequestWeb request)
        {
            try
            {
                // lets check if the user has requests already
                using (var soraContext = new SoraContext())
                {
                    ulong uid = ulong.Parse(request.UserId);
                    var reqs = soraContext.WaifuRequests.Where(x => x.UserId == uid).ToList();
                    // if there are more than 2 requests we need to check if he did 3 in the last 24h.
                    // otherwise we can skip this step
                    if (reqs.Count > 2)
                    {
                        if (reqs.Count(x => x.TimeStamp.CompareTo(DateTime.UtcNow.Subtract(TimeSpan.FromHours(24))) > 0) >= 3)
                        {
                            // use has 3 or more requests from the past 24 hours. So we cannot fulfill this request
                            return new WaifuRequestResponse()
                            {
                                Success = false,
                                Error = "You already made 3 requests in the past 24 hours"
                            };
                        }
                    }
                    // now check if waifu already exists
                    if (soraContext.Waifus.Any(x =>
                        x.Name.Equals(request.Name.Trim(), StringComparison.OrdinalIgnoreCase)))
                    {
                        return new WaifuRequestResponse()
                        {
                            Success = false,
                            Error = "This waifu already exists"
                        };
                    }
                    
                    // else we can add the request
                    var req = new WaifuRequest()
                    {
                        ImageUrl = request.ImageUrl,
                        Name = request.Name,
                        TimeStamp = DateTime.UtcNow,
                        UserId = uid,
                        Rarity = request.Rarity
                    };
                    
                    // add to DB
                    soraContext.WaifuRequests.Add(req);
                    // save changes
                    await soraContext.SaveChangesAsync();
                    
                    return new WaifuRequestResponse()
                    {
                        Success = true,
                        Error = "",
                        RequestId = req.Id.ToString()
                    };
                }
            }
            catch (Exception)
            {
                return new WaifuRequestResponse()
                {
                    Success = false,
                    Error = "Something went horribly wrong :("
                };
            }
        }
        
        [HttpGet("GetAllWaifus/", Name = "GetAllWaifus")]
        [EnableCors("AllowLocal")]
        public AllWaifus GetAllWaifus(ulong userId)
        {
            try
            {
                using (var soraContext = new SoraContext())
                {
                    var waifus = new AllWaifus();
                    // add up all waifus
                    var sorted = soraContext.Waifus.OrderByDescending(x => x.Rarity);
                    waifus.Waifus = sorted.ToList();
                    // send all Waifus
                    return waifus;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            return null;
        }

        [HttpGet("GetUserWaifus/{userId}", Name = "GetUserWaifus")]
        [EnableCors("AllowLocal")]
        public async Task<UserWaifusAPI> GetUserWaifus(ulong userId)
        {
            try
            {
                IUser user = _client.GetUser(userId) ?? await _restClient.GetUserAsync(userId) as IUser;
                using (var soraContext = new SoraContext())
                {
                    var userwaifus = new UserWaifusAPI();
                    var userdb = Utility.OnlyGetUser(userId, soraContext);
                    if (userdb == null || userdb.UserWaifus.Count == 0)
                    {
                        userwaifus.Success = false;
                        return userwaifus;
                    }

                    userwaifus.Success = true;
                    userwaifus.Username = user?.Username ?? "Unknown";
                    userwaifus.AvatarUrl = user?.GetAvatarUrl() ?? _client.CurrentUser.GetAvatarUrl() ?? Utility.StandardDiscordAvatar;

                    foreach (var userWaifu in userdb.UserWaifus)
                    {
                        var waifu = soraContext.Waifus.FirstOrDefault(x => x.Id == userWaifu.WaifuId);
                        if (waifu == null)
                            continue;
                        
                        userwaifus.Waifus.Add(new UserWaifuAPI()
                        {
                            Count = userWaifu.Count,
                            Id = waifu.Id,
                            ImageUrl = waifu.ImageUrl,
                            Name = waifu.Name,
                            Rarity = waifu.Rarity
                        });
                    }

                    if (userwaifus.Waifus.Count ==0)
                    {
                        userwaifus.Success = false;
                        return userwaifus; 
                    }

                    userwaifus.Waifus = userwaifus.Waifus.OrderByDescending(x => x.Rarity).ToList();

                    return userwaifus;

                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return null;
        }
        
                

        private WaifuRarity GetOfficialRarity(short rarity)
        {
            switch (rarity)
            {
                case 0:
                    return WaifuRarity.Common;
                case 1:
                    return WaifuRarity.Uncommon;
                case 2:
                    return WaifuRarity.Rare;
                case 3:
                    return WaifuRarity.Epic;
                case 98:
                    return _currentSpecial;
                case 99:
                    return WaifuRarity.UltimateWaifu;
                default:
                    return WaifuRarity.Common;
            }
        }

        private short GetWebRarity(WaifuRarity rarity)
        {
            switch (rarity)
            {
                case WaifuRarity.Common:
                    return 0;
                case WaifuRarity.Uncommon:
                    return 1;
                case WaifuRarity.Rare:
                    return 2;
                case WaifuRarity.Epic:
                    return 3;
                case WaifuRarity.UltimateWaifu:
                    return 99;
                default:
                    return 98;
            }
        }
    }
}