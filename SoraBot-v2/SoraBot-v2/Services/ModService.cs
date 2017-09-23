using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using SoraBot_v2.Data;
using SoraBot_v2.Data.Entities;
using SoraBot_v2.Data.Entities.SubEntities;

namespace SoraBot_v2.Services
{
    public class ModService
    {
        private IServiceProvider _services;
        private readonly Color _yellowWarning = new Color(255,204,77);
        private readonly Color _redBan = new Color(221,46,68);
        private readonly Color _orangeKick = new Color(219, 132, 19);
        
        public void Initialize(IServiceProvider services)
        {
            _services = services;
        }

        public enum Case
        {
            Warning = 0,
            Kick = 1,
            Ban = 2
        }
        
        
        public async Task ClientOnUserBanned(SocketUser socketUser, SocketGuild socketGuild)
        {
            //Make sure we dont logg twice
            Task.Run(async () =>
            {
                await Task.Delay(1000);
                //Check if case is already present
                using (SoraContext soraContext = _services.GetService<SoraContext>())
                {
                    var guildDb = Utility.GetOrCreateGuild(socketGuild.Id, soraContext);
                    if(guildDb.Cases.Any(x=> x.Type == Case.Ban && x.UserId == socketUser.Id))
                        return;
                }
                await Log(socketGuild, socketUser, Case.Ban); 
            });
        }
        
        public async Task ClientOnUserUnbanned(SocketUser socketUser, SocketGuild socketGuild)
        {
            await PardonUser(socketUser, socketGuild);
        }

        public async Task<bool> CheckPermissions(SocketCommandContext context, Case modCase, SocketGuildUser sora
            ,SocketGuildUser user)
        {
            var mod = (SocketGuildUser)context.User;
            string punishment = "";
            switch (modCase)
            {
                case Case.Ban:
                    //Check sora's perms
                    if (!sora.GuildPermissions.Has(GuildPermission.BanMembers) && !sora.GuildPermissions.Has(GuildPermission.Administrator))
                    {
                        await context.Channel.SendMessageAsync("", embed: Utility.ResultFeedback(
                            Utility.RedFailiureEmbed,
                            Utility.SuccessLevelEmoji[2], "I don't have ban permissions!"));
                        return false;
                    }
                    //Check user perms
                    if (!mod.GuildPermissions.Has(GuildPermission.BanMembers) && !Utility.IsSoraAdmin(mod) && !mod.GuildPermissions.Has(GuildPermission.Administrator))
                    {
                        await context.Channel.SendMessageAsync("", embed: Utility.ResultFeedback(
                            Utility.RedFailiureEmbed,
                            Utility.SuccessLevelEmoji[2], $"You don't have ban permissions nor the {Utility.SORA_ADMIN_ROLE_NAME} role!"));
                        return false;
                    }
                    punishment = "ban";
                    break;
                case Case.Kick:
                    //Check sora's perms
                    if (!sora.GuildPermissions.Has(GuildPermission.KickMembers) && !sora.GuildPermissions.Has(GuildPermission.Administrator))
                    {
                        await context.Channel.SendMessageAsync("", embed: Utility.ResultFeedback(
                            Utility.RedFailiureEmbed,
                            Utility.SuccessLevelEmoji[2], "I don't have kick permissions!"));
                        return false;
                    }
                    //Check user perms
                    if (!mod.GuildPermissions.Has(GuildPermission.KickMembers) && !Utility.IsSoraAdmin(mod)&& !mod.GuildPermissions.Has(GuildPermission.Administrator))
                    {
                        await context.Channel.SendMessageAsync("", embed: Utility.ResultFeedback(
                            Utility.RedFailiureEmbed,
                            Utility.SuccessLevelEmoji[2], $"You don't have kick permissions nor the {Utility.SORA_ADMIN_ROLE_NAME} role!"));
                        return false;
                    }
                    punishment = "kick";
                    break;
                case Case.Warning:
                    //Check user perms
                    if (!mod.GuildPermissions.Has(GuildPermission.KickMembers) && !mod.GuildPermissions.Has(GuildPermission.BanMembers) && !Utility.IsSoraAdmin(mod)&& !mod.GuildPermissions.Has(GuildPermission.Administrator))
                    {
                        await context.Channel.SendMessageAsync("", embed: Utility.ResultFeedback(
                            Utility.RedFailiureEmbed,
                            Utility.SuccessLevelEmoji[2], $"You don't have kick or ban permissions nor the {Utility.SORA_ADMIN_ROLE_NAME} role!"));
                        return false;  
                    }
                    punishment = "warn";
                     break;
            }
            var modHighestRole = mod.Roles.OrderByDescending(x => x.Position).FirstOrDefault();
            var userHighestRole = user.Roles.OrderByDescending(x => x.Position).FirstOrDefault();

            if (userHighestRole.Position > modHighestRole.Position)
            {
                await context.Channel.SendMessageAsync("", embed: Utility.ResultFeedback(
                    Utility.RedFailiureEmbed,
                    Utility.SuccessLevelEmoji[2], $"You can't {punishment} somebody above you in the role hierarchy!"));
                return false; 
            }
            var soraHighestRole = sora.Roles.OrderByDescending(x => x.Position).FirstOrDefault();
            if (userHighestRole.Position > soraHighestRole.Position)
            {
                await context.Channel.SendMessageAsync("", embed: Utility.ResultFeedback(
                    Utility.RedFailiureEmbed,
                    Utility.SuccessLevelEmoji[2], $"I can't {punishment} somebody above me in the role hierarchy!"));
                return false; 
            }
            return true;
        }

        public async Task SetPunishLogsChannel(SocketCommandContext context, SocketTextChannel channel)
        {
            //check perms
            if(await Utility.HasAdminOrSoraAdmin(context) == false)
                return;
            using (SoraContext soraContext = _services.GetService<SoraContext>())
            {
                var guildDb = Utility.GetOrCreateGuild(context.Guild.Id, soraContext);
                if (guildDb.PunishLogsId == channel.Id)
                {
                    await context.Channel.SendMessageAsync("", embed: Utility.ResultFeedback(Utility.RedFailiureEmbed,
                        Utility.SuccessLevelEmoji[2], "This already is the punishlogs channel..."));
                    return;
                }
                guildDb.PunishLogsId = channel.Id;
                await soraContext.SaveChangesAsync();
            }
            await context.Channel.SendMessageAsync("", embed: Utility.ResultFeedback(Utility.GreenSuccessEmbed,
                Utility.SuccessLevelEmoji[0], "Successfully set punishlogs channel to").WithDescription($"<#{channel.Id}>"));
        }

        public async Task DeletePunishLogsChannel(SocketCommandContext context)
        {
            //check perms
            if(await Utility.HasAdminOrSoraAdmin(context) == false)
                return;
            using (SoraContext soraContext = _services.GetService<SoraContext>())
            {
                var guildDb = Utility.GetOrCreateGuild(context.Guild.Id, soraContext);
                guildDb.PunishLogsId = 0;
                await soraContext.SaveChangesAsync();
            }
            await context.Channel.SendMessageAsync("", embed: Utility.ResultFeedback(Utility.GreenSuccessEmbed,
                Utility.SuccessLevelEmoji[0], "Successfully removed punishlogs channel"));
        }

        public async Task KickUser(SocketCommandContext context, SocketGuildUser user, string reason)
        {
            var sora = context.Guild.CurrentUser;
            if (await CheckPermissions(context, Case.Kick, sora, user) == false)
                return;
            //Everything alright so kick that mofo
            try
            {
                await user.KickAsync(reason);
            }
            catch (Exception)
            {
                await context.Channel.SendMessageAsync("", embed: Utility.ResultFeedback(Utility.RedFailiureEmbed,
                    Utility.SuccessLevelEmoji[2], "Something went wrong :( I probably lack permissions!"));
                return;
            }
            //log
            bool logged = await Log(context.Guild, user, Case.Kick, context.User as SocketGuildUser, reason);
            await context.Channel.SendMessageAsync("", embed: Utility.ResultFeedback(Utility.GreenSuccessEmbed,
                    Utility.SuccessLevelEmoji[0], $"Successfully kicked user {Utility.GiveUsernameDiscrimComb(user)}")
                .WithDescription($"{(string.IsNullOrWhiteSpace(reason) ? "*no reason added yet*" : reason)}{(logged ? "":"\nThis action wasn't logged nor saved bcs the Punishlogs channel isn't set up!")}"));
        }

        public async Task BanUser(SocketCommandContext context, SocketGuildUser user, string reason)
        {
            var sora = context.Guild.CurrentUser;
            if (await CheckPermissions(context, Case.Ban, sora, user) == false)
                return;
            //Everything alright so ban that mofo
            try
            {
                await context.Guild.AddBanAsync(user, 7, reason);
            }
            catch (Exception)
            {
                await context.Channel.SendMessageAsync("", embed: Utility.ResultFeedback(Utility.RedFailiureEmbed,
                    Utility.SuccessLevelEmoji[2], "Something went wrong :( I probably lack permissions!"));
                return;
            }
            bool logged = await Log(context.Guild, user, Case.Ban, context.User as SocketGuildUser, reason);
            await context.Channel.SendMessageAsync("", embed: Utility.ResultFeedback(Utility.GreenSuccessEmbed,
                    Utility.SuccessLevelEmoji[0], $"Successfully banned user {Utility.GiveUsernameDiscrimComb(user)}")
                .WithDescription($"{(string.IsNullOrWhiteSpace(reason) ? "*no reason added yet*" : reason)}{(logged ? "":"\nThis action wasn't logged nor saved bcs the Punishlogs channel isn't set up!")}"));
        }

        public async Task<bool> PardonUser(SocketUser user, SocketGuild guild ,SocketUser mod = null)
        {
            //MAKE SURE YOU CHECK PERMS IN MODULE U CUCK ♥
            using (SoraContext soraContext = _services.GetService<SoraContext>())
            {
                var guildDb = Utility.GetOrCreateGuild(guild.Id, soraContext);
                var cases = guildDb.Cases.Where(x => x.UserId == user.Id)?.ToList();
                if (cases == null || cases.Count == 0)
                    return false;
                cases.ForEach(x=> guildDb.Cases.Remove(x));
                
                //Check if punishlogs exists
                var channel = guild.GetTextChannel(guildDb.PunishLogsId);
                if (channel == null)
                {
                    guildDb.PunishLogsId = 0;
                    await soraContext.SaveChangesAsync();
                    return true;
                }
                await soraContext.SaveChangesAsync();
                //check readwrite perms of sora
                if (await Utility.CheckReadWritePerms(guild, channel) == false)
                    return true;
                
                var eb = new EmbedBuilder()
                {
                    Color = Utility.PurpleEmbed,
                    Timestamp = DateTime.UtcNow,
                    Title = "User Pardoned 🎈",
                    Description = "All cases this user was involved with where removed from the guild's database. He starts fresh again."
                };
                eb.AddField(x =>
                {
                    x.IsInline = false;
                    x.Name = "User";
                    x.Value = $"**{Utility.GiveUsernameDiscrimComb(user)}** ({user.Id})";
                });

                if (mod != null)
                {
                    eb.AddField(x =>
                    {
                        x.IsInline = false;
                        x.Name = "Moderator";
                        x.Value = $"**{Utility.GiveUsernameDiscrimComb(mod)}** ({mod.Id})";
                    });
                    eb.ThumbnailUrl = mod.GetAvatarUrl() ?? Utility.StandardDiscordAvatar;
                }

                await channel.SendMessageAsync("", embed: eb);

            }
            return true;
        }

        private async Task PostWarningRemovalLog(SocketTextChannel channel, SocketGuildUser user, SocketUser mod, int amountDeleted, int initialAmount)
        {
            var eb = new EmbedBuilder()
            {
                Color = Utility.PurpleEmbed,
                Timestamp = DateTime.UtcNow,
                Title = "Warnings Removed 📂"
            };
            eb.AddField(x=>
            {
                x.IsInline = false;
                x.Name = "User";
                x.Value = $"**{Utility.GiveUsernameDiscrimComb(user)}** ({user.Id})";
            });
            eb.AddField(x=>
            {
                x.IsInline = false;
                x.Name = "Moderator";
                x.Value = $"**{Utility.GiveUsernameDiscrimComb(mod)}** ({mod.Id})";
            });
            eb.AddField(x=>
            {
                x.IsInline = false;
                x.Name = "Warnings";
                x.Value = $"{amountDeleted} out of {initialAmount} were removed";
            });

            await channel.SendMessageAsync("", embed: eb);
        }
        
        public async Task RemoveWarnings(SocketCommandContext context, SocketGuildUser user, int warnNr, bool all)
        {
            var sora = context.Guild.CurrentUser;
            //Check if user has at least some perms
            if (await CheckPermissions(context, Case.Warning, sora, user) == false)
                return;
            using (SoraContext soraContext = _services.GetService<SoraContext>())
            {
                var guildDb = Utility.GetOrCreateGuild(context.Guild.Id, soraContext);
                //make sure punish logs channel is still available
                var channel = context.Guild.GetTextChannel(guildDb.PunishLogsId);
                if (channel == null)
                {
                    await context.Channel.SendMessageAsync("", embed: Utility.ResultFeedback(Utility.RedFailiureEmbed,
                        Utility.SuccessLevelEmoji[2], "Can't remove warnings without a punishlogs channel! Please set one up"));
                    return;
                }
                if (await Utility.CheckReadWritePerms(context.Guild, channel) == false)
                {
                    await context.Channel.SendMessageAsync("", embed: Utility.ResultFeedback(Utility.RedFailiureEmbed,
                        Utility.SuccessLevelEmoji[2], "Sora is missing crucial perms. Owner has been notified!"));
                    return;
                }
                //search for warnings with him
                if (all)
                {
                    var userCases = guildDb.Cases.Where(x => x.UserId == user.Id && x.Type == Case.Warning)?.ToList();
                    if (userCases == null || userCases.Count == 0)
                    {
                        await context.Channel.SendMessageAsync("", embed: Utility.ResultFeedback(Utility.RedFailiureEmbed,
                            Utility.SuccessLevelEmoji[2], "User has no logged warnings!"));
                        return;
                    }
                    int initialCount = userCases.Count;
                    userCases.ForEach(x=> guildDb.Cases.Remove(x));
                    await soraContext.SaveChangesAsync();
                    await context.Channel.SendMessageAsync("", embed: Utility.ResultFeedback(Utility.GreenSuccessEmbed,
                        Utility.SuccessLevelEmoji[0], "Removed all warnings from user"));
                    await PostWarningRemovalLog(channel, user, context.User, userCases.Count,initialCount);
                    return;
                }
                    var warning = guildDb.Cases.FirstOrDefault(x => x.UserId == user.Id && x.WarnNr == warnNr);
                    if (warning == null)
                    {
                        await context.Channel.SendMessageAsync("", embed: Utility.ResultFeedback(Utility.RedFailiureEmbed,
                            Utility.SuccessLevelEmoji[2], "Couldn't find specified warning!"));
                        return;
                    }
                int totalWarnings = guildDb.Cases.Count(x => x.UserId == user.Id && x.Type == Case.Warning);
                guildDb.Cases.Remove(warning);
                await soraContext.SaveChangesAsync();
                await context.Channel.SendMessageAsync("", embed: Utility.ResultFeedback(Utility.GreenSuccessEmbed,
                    Utility.SuccessLevelEmoji[0], "Removed specified warning"));
                await PostWarningRemovalLog(channel, user, context.User, 1,totalWarnings);
            }
        }

        public async Task ListAllCasesWithUser(SocketCommandContext context, SocketGuildUser user)
        {
            var sora = context.Guild.CurrentUser;
            //Check if user has at least some perms
            if (await CheckPermissions(context, Case.Warning, sora, user) == false)
                return;
            using (SoraContext soraContext = _services.GetService<SoraContext>())
            {
                var guildDb = Utility.GetOrCreateGuild(context.Guild.Id, soraContext);
                //search for cases with him
                var userCases = guildDb.Cases.Where(x => x.UserId == user.Id)?.ToList();
                if (userCases == null || userCases.Count == 0)
                {
                    await context.Channel.SendMessageAsync("", embed: Utility.ResultFeedback(Utility.RedFailiureEmbed,
                        Utility.SuccessLevelEmoji[2], "User has no logged cases!"));
                    return;
                }
                var eb = new EmbedBuilder()
                {
                    Color = Utility.PurpleEmbed,
                    Footer = Utility.RequestedBy(context.User),
                    ThumbnailUrl = user.GetAvatarUrl() ?? Utility.StandardDiscordAvatar,
                    Title = $"Cases of {Utility.GiveUsernameDiscrimComb(user)}"
                };
                int count = 0;
                foreach (var userCase in userCases)
                {
                    if (count >= 22)
                    {
                        eb.AddField(x =>
                        {
                            x.IsInline = false;
                            x.Name = "Can't show more";
                            x.Value = "Honestly... You should ban him...";
                        });
                        break;
                    }
                    string title ="";
                    switch (userCase.Type)
                    {
                        case Case.Ban:
                            title = $"Case #{userCase.CaseNr} | Ban 🔨";
                            break;
                        case Case.Kick:
                            title = $"Case #{userCase.CaseNr} | Kick 👢";
                            break;
                        case Case.Warning:
                            title = $"Case #{userCase.CaseNr} | Warning #{userCase.WarnNr} ⚠";
                            break;
                        default:
                            title = "Undefined";
                            break;
                    }
                    var mod = context.Guild.GetUser(userCase.ModId);
                    eb.AddField(x =>
                    {
                        x.IsInline = false;
                        x.Name = title;
                        x.Value = $"{(string.IsNullOrWhiteSpace(userCase.Reason) ? "Undefined" : userCase.Reason)}\n" +
                                  $"*by {(mod == null ? "Undefined" : $"{Utility.GiveUsernameDiscrimComb(mod)}")}*";
                    });
                }
                await context.Channel.SendMessageAsync("", embed: eb);
            }
        }

        public async Task AddReason(SocketCommandContext context, int caseNr, string reason)
        {
            using (SoraContext soraContext = _services.GetService<SoraContext>())
            {
                var guildDb = Utility.GetOrCreateGuild(context.Guild.Id, soraContext);
                var foundCase = guildDb.Cases.FirstOrDefault(x => x.CaseNr == caseNr);
                //check if case nr is valid
                if (foundCase == null)
                {
                    await context.Channel.SendMessageAsync("", embed: Utility.ResultFeedback(Utility.RedFailiureEmbed,
                        Utility.SuccessLevelEmoji[2], "Couldn't find case"));
                    return;
                }
                //check if punishlogs channel exists
                var channel = context.Guild.GetTextChannel(guildDb.PunishLogsId);
                if (channel == null)
                {
                    await context.Channel.SendMessageAsync("", embed: Utility.ResultFeedback(Utility.RedFailiureEmbed,
                        Utility.SuccessLevelEmoji[2], "There is no punishlogs channel set up!"));
                    return;
                }
                
                //Check sora's perms
                if (await Utility.CheckReadWritePerms(context.Guild, channel) == false)
                {
                    await context.Channel.SendMessageAsync("", embed: Utility.ResultFeedback(Utility.RedFailiureEmbed,
                        Utility.SuccessLevelEmoji[2], "Sora laggs major permissions. Owner was notified"));
                    return;
                }

                //Get old message
                var msg = (IUserMessage)await channel.GetMessageAsync(foundCase.PunishMsgId);
                if (msg == null)
                {
                    await context.Channel.SendMessageAsync("", embed: Utility.ResultFeedback(Utility.RedFailiureEmbed,
                        Utility.SuccessLevelEmoji[2], "Log doesn't exist anymore!"));
                    return;
                }
                
                var mod = (SocketGuildUser)context.User;
                //if the case has a mod already check if its the same
                if (foundCase.ModId != 0)
                {
                    //his case he can edit
                    if (foundCase.ModId == mod.Id)
                    {
                        foundCase.Reason = reason;
                    }
                    else
                    {
                        await context.Channel.SendMessageAsync("", embed: Utility.ResultFeedback(Utility.RedFailiureEmbed,
                            Utility.SuccessLevelEmoji[2], "This is not your case! You can't update the reason!"));
                        return;
                    }
                }
                else
                {
                    //the log was auto generated and thus we must check if the user has the specific perms to claim this reason
                    //the only autologged cases are bans. so we can hardcode
                    if (!mod.GuildPermissions.Has(GuildPermission.BanMembers) && !Utility.IsSoraAdmin(mod))
                    {
                        await context.Channel.SendMessageAsync("", embed: Utility.ResultFeedback(
                            Utility.RedFailiureEmbed,
                            Utility.SuccessLevelEmoji[2],
                            $"You don't have ban permissions nor the {Utility.SORA_ADMIN_ROLE_NAME} role!"));
                        return;
                    }
                    foundCase.ModId = mod.Id;
                    foundCase.Reason = reason;
                }
                //reason was updated to change the post

                var eb = CreateLogReason(foundCase.CaseNr, foundCase.Type, foundCase.UserNameDisc, foundCase.UserId,
                    mod, reason, foundCase.WarnNr);
                
                await msg.ModifyAsync(x =>
                {
                    x.Embed = eb.Build();
                });
                
                await soraContext.SaveChangesAsync();
            }
            await context.Channel.SendMessageAsync("", embed: Utility.ResultFeedback(Utility.GreenSuccessEmbed,
                Utility.SuccessLevelEmoji[0], $"Successfully updated reason on case {caseNr}"));
        }

        public async Task WarnUser(SocketCommandContext context, SocketGuildUser user, string reason)
        {
            var sora = context.Guild.CurrentUser;
            if (await CheckPermissions(context, Case.Warning, sora, user) == false)
                return;
            //warn user
            bool logged = await Log(context.Guild, user, Case.Warning, context.User as SocketGuildUser, reason);
            if (!logged)
            {
                await context.Channel.SendMessageAsync("", embed: Utility.ResultFeedback(Utility.RedFailiureEmbed,
                    Utility.SuccessLevelEmoji[2], "Can't warn without a punish logs channel! Please set one up!"));
                return;
            }
            int warnNr = 0;
            using (SoraContext soraContext = _services.GetService<SoraContext>())
            {
                var guildDb = Utility.GetOrCreateGuild(context.Guild.Id, soraContext);
                //get warn nr.
                warnNr = guildDb.Cases.Count(x => x.UserId == user.Id && x.Type == Case.Warning);
            }
            await context.Channel.SendMessageAsync("", embed: Utility.ResultFeedback(Utility.GreenSuccessEmbed,
                    Utility.SuccessLevelEmoji[0], $"Successfully warned user {Utility.GiveUsernameDiscrimComb(user)}, this is his {warnNr} warning")
                .WithDescription($"{(string.IsNullOrWhiteSpace(reason) ? "*no reason added yet*" : reason)}"));
        }
        
        private EmbedBuilder CreateLogReason(int caseNr, Case modCase, string user, ulong userId,SocketGuildUser mod , string reason, int warnNr)
        {
            var eb = new EmbedBuilder()
            {
                Timestamp = DateTime.UtcNow,
            };

            switch (modCase)
            {
                case Case.Ban:
                    eb.Title = $"Case #{caseNr} | Ban 🔨";
                    eb.Color = _redBan;
                    break;
                case Case.Kick:
                    eb.Title = $"Case #{caseNr} | Kick 👢";
                    eb.Color = _orangeKick;
                    break;
                case Case.Warning:
                    eb.Title = $"Case #{caseNr} | Warning #{warnNr} ⚠";
                    eb.Color = _yellowWarning;
                    break;
            }
            eb.AddField(x =>
            {
                x.IsInline = false;
                x.Name = "User";
                x.Value = $"**{user}** ({userId})";
            });
            eb.AddField(x =>
            {
                x.IsInline = false;
                x.Name = "Moderator";
                x.Value = $"**{Utility.GiveUsernameDiscrimComb(mod)}** ({mod.Id})";
                eb.ThumbnailUrl = mod.GetAvatarUrl() ?? Utility.StandardDiscordAvatar;

            });
            eb.AddField(x =>
            {
                x.IsInline = false;
                x.Name = "Reason";
                x.Value = reason;
            });
            return eb;
        }

        private EmbedBuilder CreateLog(int caseNr, Case modCase, SocketUser user, SocketGuild guild ,SocketGuildUser mod = null, string reason = null, int warnNr = 0)
        {
            var eb = new EmbedBuilder()
            {
                Timestamp = DateTime.UtcNow,
            };

            switch (modCase)
            {
                case Case.Ban:
                    eb.Title = $"Case #{caseNr} | Ban 🔨";
                    eb.Color = _redBan;
                    break;
                case Case.Kick:
                    eb.Title = $"Case #{caseNr} | Kick 👢";
                    eb.Color = _orangeKick;
                    break;
                case Case.Warning:
                    eb.Title = $"Case #{caseNr} | Warning #{warnNr} ⚠";
                    eb.Color = _yellowWarning;
                    break;
            }
            eb.AddField(x =>
            {
                x.IsInline = false;
                x.Name = "User";
                x.Value = $"**{Utility.GiveUsernameDiscrimComb(user)}** ({user.Id})";
            });
            eb.AddField(x =>
            {
                x.IsInline = false;
                x.Name = "Moderator";
                if (mod == null)
                {
                    x.Value = "Unknown";
                }
                else
                {
                    x.Value = $"**{Utility.GiveUsernameDiscrimComb(mod)}** ({mod.Id})";
                    eb.ThumbnailUrl = mod.GetAvatarUrl() ?? Utility.StandardDiscordAvatar;
                }
            });
            eb.AddField(x =>
            {
                x.IsInline = false;
                x.Name = "Reason";
                if (string.IsNullOrWhiteSpace(reason))
                {
                    using (SoraContext soraCon = _services.GetService<SoraContext>())
                    {
                        x.Value =
                            $"Type `{Utility.GetGuildPrefix(guild, soraCon)}reason {caseNr} YourReason` to add it";
                    }
                    
                }
                else
                {
                    x.Value = reason;
                }
            });
            return eb;
        }

        private int GetCaseNumber(Guild guildDb)
        {
            return guildDb.Cases.OrderByDescending(x => x.CaseNr).FirstOrDefault().CaseNr + 1;
        }

        private async Task<bool> Log(SocketGuild guild, SocketUser user,Case modCase, SocketGuildUser mod = null,string reason= null)
        {
            using (SoraContext soraContext = _services.GetService<SoraContext>())
            {
                var guildDb = Utility.GetOrCreateGuild(guild.Id, soraContext);
                //check if punishlogs are initialized!
                if (guildDb.PunishLogsId == 0)
                    return false;
                var channel = guild.GetTextChannel(guildDb.PunishLogsId);
                //check if channel is null
                if (channel == null)
                {
                    guildDb.PunishLogsId = 0;
                    await soraContext.SaveChangesAsync();
                    return false;
                }
                //channel does exist so check permissions
                if (await Utility.CheckReadWritePerms(guild, channel) == false)
                    return false;
                //both is alright
                //if there where previous cases get new case number
                int caseNr = 1;
                if (guildDb.Cases.Count > 0)
                    caseNr = GetCaseNumber(guildDb);
                //if its a warning get warnNr
                int warnNr = 0;
                if (modCase == Case.Warning)
                {
                    warnNr = guildDb.Cases.Count(x => x.UserId == user.Id && x.Type == Case.Warning) + 1;
                }
                //log final result
                var eb =  CreateLog(caseNr, modCase, user,guild ,mod, reason, warnNr);
                var msg = await channel.SendMessageAsync("", embed: eb);
                guildDb.Cases.Add(new ModCase(){
                    CaseNr = caseNr,
                    GuildForeignId = guild.Id,
                    PunishMsgId = msg.Id,
                    Reason = reason,
                    Type = modCase,
                    UserId = user.Id,
                    ModId = mod?.Id ?? 0,
                    UserNameDisc = Utility.GiveUsernameDiscrimComb(user),
                    WarnNr = warnNr
                });
                await soraContext.SaveChangesAsync();
            }
            return true;
        }
    }
}