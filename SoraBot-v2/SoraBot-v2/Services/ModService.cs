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
        
        public async Task InitializeAsync(IServiceProvider services)
        {
            _services = services;
        }

        public enum Case
        {
            Warning = 0,
            Kick = 1,
            Ban = 2
        }

        private async Task<bool> CheckPermissions(SocketCommandContext context, Case modCase, SocketGuildUser sora
            ,SocketGuildUser user)
        {
            var mod = (SocketGuildUser)context.User;
            string punishment = "";
            switch (modCase)
            {
                case Case.Ban:
                    //Check sora's perms
                    if (!sora.GuildPermissions.Has(GuildPermission.BanMembers))
                    {
                        await context.Channel.SendMessageAsync("", embed: Utility.ResultFeedback(
                            Utility.RedFailiureEmbed,
                            Utility.SuccessLevelEmoji[2], "I don't have ban permissions!"));
                        return false;
                    }
                    //Check user perms
                    if (!mod.GuildPermissions.Has(GuildPermission.BanMembers) && !Utility.IsSoraAdmin(mod))
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
                    if (!sora.GuildPermissions.Has(GuildPermission.KickMembers))
                    {
                        await context.Channel.SendMessageAsync("", embed: Utility.ResultFeedback(
                            Utility.RedFailiureEmbed,
                            Utility.SuccessLevelEmoji[2], "I don't have kick permissions!"));
                        return false;
                    }
                    //Check user perms
                    if (!mod.GuildPermissions.Has(GuildPermission.KickMembers) && !Utility.IsSoraAdmin(mod))
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
                    if (!mod.GuildPermissions.Has(GuildPermission.KickMembers) && !mod.GuildPermissions.Has(GuildPermission.BanMembers) && !Utility.IsSoraAdmin(mod))
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
                var guildDb = Utility.GetOrCreateGuild(context.Guild, soraContext);
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
                var guildDb = Utility.GetOrCreateGuild(context.Guild, soraContext);
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
            await user.KickAsync(reason);
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
            await context.Guild.AddBanAsync(user, 7, reason);
            bool logged = await Log(context.Guild, user, Case.Ban, context.User as SocketGuildUser, reason);
            await context.Channel.SendMessageAsync("", embed: Utility.ResultFeedback(Utility.GreenSuccessEmbed,
                    Utility.SuccessLevelEmoji[0], $"Successfully banned user {Utility.GiveUsernameDiscrimComb(user)}")
                .WithDescription($"{(string.IsNullOrWhiteSpace(reason) ? "*no reason added yet*" : reason)}{(logged ? "":"\nThis action wasn't logged nor saved bcs the Punishlogs channel isn't set up!")}"));
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
            await context.Channel.SendMessageAsync("", embed: Utility.ResultFeedback(Utility.GreenSuccessEmbed,
                    Utility.SuccessLevelEmoji[0], $"Successfully warned user {Utility.GiveUsernameDiscrimComb(user)}")
                .WithDescription($"{(string.IsNullOrWhiteSpace(reason) ? "*no reason added yet*" : reason)}"));
        }

        private EmbedBuilder CreateLog(int caseNr, Case modCase, SocketGuildUser user, SocketGuildUser mod = null, string reason = null, int warnNr = 0)
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
                            $"Type `{Utility.GetGuildPrefix(user.Guild, soraCon)}reason {caseNr} YourReason` to add it";
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

        private async Task<bool> Log(SocketGuild guild, SocketGuildUser user,Case modCase, SocketGuildUser mod = null,string reason= null)
        {
            using (SoraContext soraContext = _services.GetService<SoraContext>())
            {
                var guildDb = Utility.GetOrCreateGuild(guild, soraContext);
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
                var eb =  CreateLog(caseNr, modCase, user, mod, reason, warnNr);
                var msg = await channel.SendMessageAsync("", embed: eb);
                guildDb.Cases.Add(new ModCase(){
                    CaseNr = caseNr,
                    GuildForeignId = guild.Id,
                    PunishMsgId = msg.Id,
                    Reason = reason,
                    Type = modCase,
                    UserId = user.Id,
                    ModId = mod?.Id ?? 0
                });
                await soraContext.SaveChangesAsync();
            }
            return true;
        }
    }
}