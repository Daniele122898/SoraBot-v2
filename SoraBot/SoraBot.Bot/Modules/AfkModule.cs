using System.Threading.Tasks;
using Discord.Commands;
using SoraBot.Common.Extensions.Modules;
using SoraBot.Data.Repositories.Interfaces;

namespace SoraBot.Bot.Modules
{
    public class AfkModule : SoraSocketCommandModule
    {
        private readonly IAfkRepository _afkRepo;

        public AfkModule(IAfkRepository afkRepo)
        {
            _afkRepo = afkRepo;
        }
        
        [Command("afk")]
        [Summary("Toggle your AFK status. If you just use 'afk' it will toggle your status. " +
                 "If however you add a custom message after the afk command it will always set that " +
                 "custom message and not toggle your afk on or off (it'll remain on).")]
        public async Task SetOrToggleAfk([Remainder, Summary("Custom status.")] string status = null)
        {
            if (status == null)
            {
                // We now toggle the status
                var curr = await _afkRepo.GetUserAfk(Context.User.Id);
                if (!curr)
                {
                    // No current afk status set so we enable it
                    await _afkRepo.SetUserAfk(Context.User.Id, null);
                    await ReplySuccessEmbed("Successfully set AFK status");
                    return;
                }
                // Otherwise we have a status and we should remove it
                await _afkRepo.RemoveUserAfk(Context.User.Id);
                await ReplySuccessEmbed("Successfully removed AFK status");
                return;
            }
            // Check if status is too long
            if (status.Length > 256)
            {
                await ReplyFailureEmbed("Make sure your custom AFK status is shorter than 256 characters!");
                return;
            }
            // Set the new custom status
            await _afkRepo.SetUserAfk(Context.User.Id, status);
            await ReplySuccessEmbed("Successfully set custom AFK status");
        }
    }
}