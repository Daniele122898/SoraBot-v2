using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using SoraBot.Bot.Models;
using SoraBot.Common.Utils;
using SoraBot.Services.Users;

namespace SoraBot.Bot.TypeReaders
{
    public class GuildUserTypeReader : UserTypeReader<IGuildUser>
    {
        public override async Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
        {
            var defaultRes = await base.ReadAsync(context, input, services);
            if (defaultRes.IsSuccess) return TypeReaderResult.FromSuccess(new DiscordGuildUser(defaultRes.BestMatch as IGuildUser));

            if (!MentionUtils.TryParseUser(input, out var uid) && !ulong.TryParse(input, out uid))
                return TypeReaderResult.FromError(CommandError.ParseFailed, "Could not find User");
            if (!SnowFlakeUtils.IsValidSnowflake(uid))
                return TypeReaderResult.FromError(CommandError.ParseFailed, "Could not find User");

            // We could at least get a uid. 
            // Use our user services
            var userService = services.GetRequiredService<IUserService>();
            var user = await userService.GetOrSetAndGet(uid, context.Guild.Id);
            if (!user.HasValue)
                return TypeReaderResult.FromError(CommandError.ParseFailed, "Could not find User");
            
            return TypeReaderResult.FromSuccess(new DiscordGuildUser(user.Value));
        }
    }
}