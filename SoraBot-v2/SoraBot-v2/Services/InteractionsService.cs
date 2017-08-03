using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using SoraBot_v2.Data;

namespace SoraBot_v2.Services
{

    public enum InteractionType
    {
        Pat, Hug, Kiss, Poke, Slap, Punch
    }
    public class InteractionsService
    {
        public async Task Interact(InteractionType type, SocketUser user, SocketCommandContext context, SoraContext soraContext)
        {
            //FindUserMentioned
            var dbUser = Utility.GetOrCreateUser(user, soraContext);

            switch (type)
            {
                    case(InteractionType.Pat):
                        dbUser.Interactions.Pats++;
                        break;
                    case(InteractionType.Hug):
                        dbUser.Interactions.Hugs++;
                        break;
                    case(InteractionType.Kiss):
                        dbUser.Interactions.Kisses++;
                        break;
                    case(InteractionType.Poke):
                        dbUser.Interactions.Pokes++;
                        break;
                    case(InteractionType.Slap):
                        dbUser.Interactions.Slaps++;
                        break;
                    case(InteractionType.Punch):
                        dbUser.Interactions.Punches++;
                        break;
                        default:
                            await context.Channel.SendMessageAsync(":no_entry_sign: Something went horribly wrong :eyes:");
                            break;
            }

            soraContext.SaveChanges();
        }

        public async Task CheckAffinity(SocketUser user, SocketCommandContext context, SoraContext soraContext)
        {
            //FindUserMentioned
            var dbUser = Utility.GetOrCreateUser(user, soraContext);
            var interactions = dbUser.Interactions;
            var eb = new EmbedBuilder()
            {
                Color = Utility.PurpleEmbed,
                Footer = new EmbedFooterBuilder()
                {
                    Text = $"Requested by {context.User.Username}#{context.User.Discriminator}",
                    IconUrl =  (context.User.GetAvatarUrl()?? Utility.StandardDiscordAvatar)
                },
                Title = $"Affinity stats of {user.Username}#{user.Discriminator}",
                ThumbnailUrl =  (user.GetAvatarUrl()?? Utility.StandardDiscordAvatar),
                Description = $"" +
                              $"Pats:     {dbUser.Interactions.Pats}\n" +
                              $"Hugs:     {dbUser.Interactions.Hugs}\n" +
                              $"Kisses:   {dbUser.Interactions.Kisses}\n" +
                              $"Pokes:    {dbUser.Interactions.Pokes}\n" +
                              $"Slaps:    {dbUser.Interactions.Slaps}\n" +
                              $"Punches:    {dbUser.Interactions.Punches}\n" +
                              $"Affinity: {Utility.CalculateAffinity(dbUser.Interactions)}/100"
            };
            Console.WriteLine("WAIT");
        
            await context.Channel.SendMessageAsync("",false,eb);
        
        }
    }
}