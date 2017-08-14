using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Humanizer;
using ImageSharp;
using ImageSharp.ColorSpaces.Conversion.Implementation.Rgb;
using ImageSharp.Processing;
using SixLabors.Primitives;
using SoraBot_v2.Data;
using SoraBot_v2.Data.Entities;

namespace SoraBot_v2.Services
{
    public class EpService
    {
        private DiscordSocketClient _client;
        private SoraContext _soraContext;

        public EpService(DiscordSocketClient client, SoraContext soraContext)
        {
            _client = client;
            _soraContext = soraContext;
        }

        public async Task ToggleEpGain(SocketCommandContext context)
        {
            var userDb = Utility.GetOrCreateUser(context.User, _soraContext);
            userDb.Notified = !userDb.Notified;
            _soraContext.SaveChanges();
            if (userDb.Notified)
            {
                await context.Channel.SendMessageAsync("",
                    embed: Utility.ResultFeedback(Utility.GreenSuccessEmbed, Utility.SuccessLevelEmoji[0], "You will now be notified on level up!"));
                return;
            }
            await context.Channel.SendMessageAsync("",
                embed: Utility.ResultFeedback(Utility.GreenSuccessEmbed, Utility.SuccessLevelEmoji[0],
                    "You will NOT be notified on level up!"));
        }

        public async Task RemoveBg(SocketCommandContext context)
        {
            var userDb = Utility.GetOrCreateUser(context.User, _soraContext);
            if (!userDb.HasBg)
            {
                await context.Channel.SendMessageAsync("",
                    embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2],
                        "You don't even have a BG set..."));
                return;
            }
            userDb.HasBg = false;
            _soraContext.SaveChanges();
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
            var userDb = Utility.GetOrCreateUser(context.User, _soraContext);
            int userLevel = (int) Math.Round(0.15F * Math.Sqrt(userDb.Exp));
            if (userLevel < 20)
            {
                await context.Channel.SendMessageAsync("",
                    embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2],
                        "You need to be level 20 to use custom BGs!"));
                return;
            }
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

                if (File.Exists($"ProfileData/{context.User.Id}BGF.png"))
                {
                    File.Delete($"ProfileData/{context.User.Id}BGF.png");
                }
                
                using(var client = new HttpClient())
                using(var request = new HttpRequestMessage(HttpMethod.Get, requestUri))
                using (Stream contentStream = await (await client.SendAsync(request)).Content.ReadAsStreamAsync(),
                    stream = new FileStream($"ProfileData/{context.User.Id}BG.png",FileMode.Create, FileAccess.Write, FileShare.None, 3145728, true))
                {
                    await contentStream.CopyToAsync(stream);
                    await contentStream.FlushAsync();
                    contentStream.Dispose();
                    await stream.FlushAsync();
                    stream.Dispose();
                }
                
                Configuration.Default.AddImageFormat(ImageFormats.Png);

                using (var input = ImageSharp.Image.Load($"ProfileData/{context.User.Id}BG.png"))
                {
                    input.Resize(new ResizeOptions
                    {
                        Size = new Size(960,540),
                        Mode = ResizeMode.Crop
                    });
                    input.Save($"ProfileData/{context.User.Id}BGF.png");
                }//Dispose input
                
                if (File.Exists($"ProfileData/{context.User.Id}BG.png"))
                {
                    File.Delete($"ProfileData/{context.User.Id}BG.png");
                }

                userDb.HasBg = true;
                _soraContext.SaveChanges();
                await context.Channel.SendMessageAsync("",
                    embed: Utility.ResultFeedback(Utility.GreenSuccessEmbed, Utility.SuccessLevelEmoji[0],
                        "Successfully set new BG!"));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                await context.Channel.SendMessageAsync("",
                    embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], "Failed to Download the Image! Try another one, gomen"));
            }

        }

        public async Task DrawProfileCard(SocketCommandContext context, SocketUser user)
        {
            var userDb = Utility.GetOrCreateUser(user, _soraContext);
            var requesterDb = Utility.GetOrCreateUser(context.User, _soraContext);
            int userLevel = (int) Math.Round(0.15F * Math.Sqrt(userDb.Exp));
            //Check for cooldown!
            if (requesterDb.ShowProfileCardAgain.CompareTo(DateTime.UtcNow) < 0)
            {
                requesterDb.ShowProfileCardAgain = DateTime.UtcNow.AddSeconds(30);
                _soraContext.SaveChanges();
            }
            else
            {
                var remainingSeconds= requesterDb.ShowProfileCardAgain.Subtract(DateTime.UtcNow.TimeOfDay).Second;
                await context.Channel.SendMessageAsync("",
                    embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], $"Dont break me >.< Please wait another {remainingSeconds} seconds!"));
                return;
            }
            
            Uri requestUri = new Uri(user.GetAvatarUrl() ?? Utility.StandardDiscordAvatar);

            if (File.Exists($"ProfileData/{user.Id}Avatar.png"))
            {
                File.Delete($"ProfileData/{user.Id}Avatar.png");
            }
            
            using(var client = new HttpClient())
            using (var request = new HttpRequestMessage(HttpMethod.Get, requestUri))
            using (Stream contentStream = await(await client.SendAsync(request)).Content.ReadAsStreamAsync(), 
                stream = new FileStream($"ProfileData/{user.Id}Avatar.png", FileMode.Create, FileAccess.Write,FileShare.None,3145728,true))
            {
                await contentStream.CopyToAsync(stream);
                await contentStream.FlushAsync();
                contentStream.Dispose();
                await stream.FlushAsync();
                stream.Dispose();
            }

            var username = (user.Username.Length > 20 ? user.Username.Remove(20)+"..." : user.Username);
            
            //GET RANK
            List<User> users = new List<User>();
            foreach (var guildUser in context.Guild.Users)
            {
                var uDb = Utility.OnlyGetUser(guildUser, _soraContext);
                if(uDb != null)
                    users.Add(uDb);
            }
            var sortedUsers = users.OrderByDescending(x => x.Exp).ToList();

            int rank = GetIndexOfItem(sortedUsers, user.Id);
            rank++;
            
            //draw profile image
            if (userDb.HasBg)
            {
                await ProfileImageProcessing.GenerateProfileWithBg($"ProfileData/{user.Id}Avatar.png",$"ProfileData/{user.Id}BGF.png", username, rank, userLevel, (int)userDb.Exp, $"ProfileData/{user.Id}.png");
            }
            else
            {
                try
                {
                    await ProfileImageProcessing.GenerateProfile($"ProfileData/{user.Id}Avatar.png",username, rank, userLevel, (int)userDb.Exp, $"ProfileData/{user.Id}.png");

                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
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
                    embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], "Failed to create profile card! Maybe try to get a new Background? Or contact the creator here").WithUrl("https://discord.gg/Pah4yj5"));
            }

        }

        private int GetIndexOfItem(List<User> list, ulong key)
        {
            for (int index = 0; index < list.Count; index++)
            {
                if (list[index].UserId == key)
                    return index;
            }
            return -1;
        }

        public async Task GetLocalTop10List(SocketCommandContext context)
        {
            var eb = new EmbedBuilder()
            {
                Color = Utility.PurpleEmbed,
                Title = $"Top 10 in {context.Guild.Name}",
                Description = $"The EXP is gained globally. These rankings are based on the EXP from all users in this guild",
                Footer = Utility.RequestedBy(context.User),
                ThumbnailUrl = context.Guild.IconUrl ?? Utility.StandardDiscordAvatar
            };
            
            //Feed list
            //GET RANK
            List<User> users = new List<User>();
            foreach (var guildUser in context.Guild.Users)
            {
                var uDb = Utility.OnlyGetUser(guildUser, _soraContext);
                if(uDb != null)
                    users.Add(uDb);
            }
            var sortedUsers = users.OrderByDescending(x => x.Exp).ToList();
            
            var glopalsortedUsers = _soraContext.Users.OrderByDescending(x => x.Exp).ToList();
            for (int index = 0; index < 10; index++)
            {
                eb.AddField(x =>
                {
                    x.IsInline = false;
                    x.Name =
                        $"{index + 1}. {Utility.GiveUsernameDiscrimComb(_client.GetUser(sortedUsers[index].UserId))}";
                    x.Value =
                        $"Lvl. {CalculateLevel(sortedUsers[index].Exp)} \tEXP: {sortedUsers[index].Exp} \tGlobal Rank: {GetIndexOfItem(glopalsortedUsers, sortedUsers[index].UserId)+1}";//TODO GLOBAL RANK
                });
            }
            eb.AddField(x =>
            {
                int yourIndex = GetIndexOfItem(sortedUsers, context.User.Id);
                x.IsInline = false;
                x.Name = $"Your Rank: {yourIndex+1}";
                x.Value = $"Lvl. {CalculateLevel(sortedUsers[yourIndex].Exp)} \tEXP: {sortedUsers[yourIndex].Exp} \tYour Global Rank: {GetIndexOfItem(glopalsortedUsers, context.User.Id)+1}";
            });
            await context.Channel.SendMessageAsync("", embed: eb);
        }

        public async Task GetGlobalTop10(SocketCommandContext context)
        {
            var eb = new EmbedBuilder()
            {
                Color = Utility.PurpleEmbed,
                Title = $"Global Top 10",
                Description = $"Global Top 10 of all users connected to Sora",
                Footer = Utility.RequestedBy(context.User),
                ThumbnailUrl = context.Guild.IconUrl ?? Utility.StandardDiscordAvatar
            };
            //Feed list
            var sortedUsers = _soraContext.Users.OrderByDescending(x => x.Exp).ToList();
            
            for (int index = 0; index < 10; index++)
            {
                eb.AddField(x =>
                {
                    x.IsInline = false;
                    x.Name =
                        $"{index + 1}. {Utility.GiveUsernameDiscrimComb(_client.GetUser(sortedUsers[index].UserId))}";//TODO WHAT IF USER ISNT IN REACH OF SORA ANYMORE
                    x.Value =
                        $"Lvl. {CalculateLevel(sortedUsers[index].Exp)} \tEXP: {sortedUsers[index].Exp}";
                });
            }
            eb.AddField(x =>
            {
                int yourIndex = GetIndexOfItem(sortedUsers, context.User.Id);
                x.IsInline = false;
                x.Name = $"Your Rank: {yourIndex+1}";
                x.Value = $"Lvl. {CalculateLevel(sortedUsers[yourIndex].Exp)} \tEXP: {sortedUsers[yourIndex].Exp}";
            });
            await context.Channel.SendMessageAsync("", embed: eb);
        }

        private int CalculateLevel(float exp)
        {
            return (int) Math.Round(0.15F * Math.Sqrt(exp));
        }
        
        
        public async Task IncreaseEpOnMessageReceive(SocketMessage msg)
        {
            //Don't prcoess the command if it was a system message
            var message = msg as SocketUserMessage;
            if(message == null) return;
            
            //Create a command Context
            var context = new SocketCommandContext(_client, message);
            if(context.IsPrivate) return;
            if(context.User.IsBot) return;

            var userDb = Utility.GetOrCreateUser(context.User, _soraContext);
            //Check for cooldown
            if(userDb.CanGainAgain.CompareTo(DateTime.UtcNow)>0)
                return;
            
            userDb.CanGainAgain = DateTime.UtcNow.AddSeconds(10);
            int previousLevel = (int) Math.Round(0.15F * Math.Sqrt(userDb.Exp));
            var epGain = (int) Math.Round(context.Message.Content.Length / 10F);
            if (epGain > 50)
                epGain = 50;
            userDb.Exp += epGain;
            int currentLevel = (int) Math.Round(0.15F * Math.Sqrt(userDb.Exp));
            //Notifying
            if (previousLevel != currentLevel && userDb.Notified)
            {
                var eb = new EmbedBuilder()
                {
                    Color = new Color(255,204,77),
                    Title = $"🏆 You leveled up! You are now level {currentLevel} \\ (•◡•) /"
                };
                await (await context.User.GetOrCreateDMChannelAsync()).SendMessageAsync("" ,embed: eb);
            }
            _soraContext.SaveChanges();
        }
    }
}