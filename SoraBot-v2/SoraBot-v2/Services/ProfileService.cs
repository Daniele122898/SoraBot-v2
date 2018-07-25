using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Transforms;
using SixLabors.Primitives;
using SoraBot_v2.Data;

namespace SoraBot_v2.Services
{
    public class ProfileService
    {
        
        public async Task RemoveBg(SocketCommandContext context)
        {
            using (var soraContext = new SoraContext())
            {
                var userDb = Utility.GetOrCreateUser(context.User.Id, soraContext);
                if (!userDb.HasBg)
                {
                    await context.Channel.SendMessageAsync("",
                        embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2],
                            "You don't even have a BG set..."));
                    return;
                }
                userDb.HasBg = false;
                await soraContext.SaveChangesAsync();
            }
            if (File.Exists($"ProfileData/{context.User.Id}BGF.png"))
            {
                File.Delete($"ProfileData/{context.User.Id}BGF.png");
            }
            await context.Channel.SendMessageAsync("",
                embed: Utility.ResultFeedback(Utility.GreenSuccessEmbed, Utility.SuccessLevelEmoji[0],
                    "Successfully delete the BG!"));
        }
        
        public async Task SetCustomBg(string url, SocketCommandContext context)
        {
            using (var soraContext = new SoraContext())
            {
                var userDb = Utility.GetOrCreateUser(context.User.Id, soraContext);
                //cooldown
                if (userDb.UpdateBgAgain.CompareTo(DateTime.UtcNow) < 0)
                {
                    userDb.UpdateBgAgain = DateTime.UtcNow.AddSeconds(45);
                }
                else
                {
                    var timeRemaining = userDb.UpdateBgAgain.Subtract(DateTime.UtcNow.TimeOfDay).Second;
                    await context.Channel.SendMessageAsync("",
                        embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2],
                            $"Dont break me >.< Please wait another {timeRemaining} seconds!"));
                    return;
                }
                try
                {
                    Uri requestUri = new Uri(url);
                    
                    //DOWNLOAD IMAGE
                    using (var client = new HttpClient())
                    using (var request = new HttpRequestMessage(HttpMethod.Get, requestUri))
                    using (Stream contentStream = await (await client.SendAsync(request)).Content.ReadAsStreamAsync(),
                        stream = new FileStream($"ProfileData/{context.User.Id}BG.png", FileMode.Create,
                            FileAccess.Write, FileShare.None, 3145728, true))
                    {
                        await contentStream.CopyToAsync(stream);
                        await contentStream.FlushAsync();
                        contentStream.Dispose();
                        await stream.FlushAsync();
                        stream.Dispose();
                    }
                    //download was successfull so remove old one
                    if (File.Exists($"ProfileData/{context.User.Id}BGF.png"))
                    {
                        File.Delete($"ProfileData/{context.User.Id}BGF.png");
                    }

                    using (var input = Image.Load($"ProfileData/{context.User.Id}BG.png"))
                    {
                        input.Mutate(x=> x.Resize(new ResizeOptions()
                        {
                            Size = new Size(470,265),
                            Mode = ResizeMode.Crop
                        }));
                        input.Save($"ProfileData/{context.User.Id}BGF.png");
                    }
                    //dispose of temp file
                    if (File.Exists($"ProfileData/{context.User.Id}BG.png"))
                    {
                        File.Delete($"ProfileData/{context.User.Id}BG.png");
                    }
                    
                    userDb.HasBg = true;
                    await soraContext.SaveChangesAsync();
                    await context.Channel.SendMessageAsync("",
                        embed: Utility.ResultFeedback(Utility.GreenSuccessEmbed, Utility.SuccessLevelEmoji[0],
                            "Successfully set new BG!"));
                    
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    await context.Channel.SendMessageAsync("",
                        embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2],
                            "Failed to Download the Image! Try another one, gomen"));
                }
            }
        }
        
        public async Task DrawProfileCard(SocketCommandContext context, SocketUser user)
        {
            using (var soraContext = new SoraContext())
            {
                try
                {
                    var userDb = Utility.GetOrCreateUser(user.Id, soraContext);
                    //Check for cooldown
                    var requestorDb = Utility.GetOrCreateUser(context.User.Id, soraContext);
                    if (requestorDb.ShowProfileCardAgain.CompareTo(DateTime.UtcNow) < 0)
                    {
                        requestorDb.ShowProfileCardAgain = DateTime.UtcNow.AddSeconds(15);
                        await soraContext.SaveChangesAsync();
                    }
                    else
                    {
                        var remainingSeconds =
                            requestorDb.ShowProfileCardAgain.Subtract(DateTime.UtcNow.TimeOfDay).Second;
                        await context.Channel.SendMessageAsync("",
                            embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2],
                                $"Dont break me >.< Please wait another {remainingSeconds} seconds!"));
                        return;
                    }
                    
                    Uri requestUri = new Uri(user.GetAvatarUrl() ?? Utility.StandardDiscordAvatar);
                    //remove temporary avatar file if it already exists
                    if (File.Exists($"ProfileData/{user.Id}Avatar.png"))
                    {
                        File.Delete($"ProfileData/{user.Id}Avatar.png");
                    }
                    //Get the user avatar
                    using (var client = new HttpClient())
                    using (var request = new HttpRequestMessage(HttpMethod.Get, requestUri))
                    using (Stream contentStream = await (await client.SendAsync(request)).Content.ReadAsStreamAsync(),
                        stream = new FileStream($"ProfileData/{user.Id}Avatar.png", FileMode.Create, FileAccess.Write,
                            FileShare.None, 3145728, true))
                    {
                        await contentStream.CopyToAsync(stream);
                        await contentStream.FlushAsync();
                        contentStream.Dispose();
                        await stream.FlushAsync();
                        stream.Dispose();
                    }
    
                    var username = (user.Username.Length > 18 ? user.Username.Remove(18) + "..." : user.Username);
                    //Get Local Rank
                    var guildDb = Utility.GetOrCreateGuild(context.Guild.Id, soraContext);
                    var sortedUsers = guildDb.Users.OrderByDescending(x => x.Exp).ToList();
                    var localRank = sortedUsers.FindIndex(x => x.UserId == user.Id)+1;
                    //Get local LVL
                    var guildUser = Utility.GetOrCreateGuildUser(user.Id, context.Guild.Id, soraContext);
                    var localLevel = ExpService.CalculateLevel(guildUser.Exp);
                    //get global rank
                    var sortedGloablUsers = soraContext.Users.OrderByDescending(x => x.Exp).ToList();
                    var globalRank = sortedGloablUsers.FindIndex(x => x.UserId == user.Id)+1;
                    //Get global lvl
                    var globalLevel = ExpService.CalculateLevel(userDb.Exp);
                    //calculate needed exp for next lvl
                    int localNeededExp = ExpService.CalculateNeededExp(localLevel+1);
                    int globalNeededExp = ExpService.CalculateNeededExp(globalLevel+1);
                    //Get clan
                    var clanName = (string.IsNullOrWhiteSpace(userDb.ClanName) ? "" : userDb.ClanName);
                    //get background image
                    var bgImage = (userDb.HasBg ? $"ProfileData/{user.Id}BGF.png" : $"ProfileCreation/defaultBG.png");
                    //Draw profile card
                    ProfileImageGeneration.GenerateProfile($"ProfileData/{user.Id}Avatar.png",bgImage, username, clanName, globalRank, globalLevel, (int)userDb.Exp, 
                        globalNeededExp, localRank, localLevel, (int)guildUser.Exp, localNeededExp, $"ProfileData/{user.Id}.png");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
                
                
                if (File.Exists($"ProfileData/{user.Id}.png"))
                {
                    await context.Channel.SendFileAsync($"ProfileData/{user.Id}.png");
                    File.Delete($"ProfileData/{user.Id}.png");
                    File.Delete($"ProfileData/{user.Id}Avatar.png");
                }
                else
                {
                    await context.Channel.SendMessageAsync("",
                        embed: Utility
                            .ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2],
                                "Failed to create profile card! Maybe try to get a new Background? Or contact the creator here")
                            .WithUrl(Utility.DISCORD_INVITE));
                }
            }
        }
    }
}