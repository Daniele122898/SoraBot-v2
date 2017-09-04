using System.Threading.Tasks;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;

namespace SoraBot_v2.Extensions
{
    public class EnsureReactionFromSourceUserCriterionMod: ICriterion<SocketReaction>
    {

        public Task<bool> JudgeAsync(SocketCommandContext sourceContext, SocketReaction parameter)
        {
            return Task.FromResult<bool>((long) parameter.UserId == (long) sourceContext.User.Id);
        }

    }
}