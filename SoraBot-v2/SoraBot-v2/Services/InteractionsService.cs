using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using SoraBotv2.Migrations;
using SoraBot_v2.Data;
using SoraBot_v2.Data.Entities;

namespace SoraBot_v2.Services
{

    public enum InteractionType
    {
        Pat, Hug, Kiss, Poke, Slap, Punch, High5
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
                    case(InteractionType.High5):
                        dbUser.Interactions.High5++;
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
        
        public async Task InteractMultiple(InteractionType type, List<SocketUser> usersT, SocketCommandContext context, SoraContext soraContext)
        {
            //FindUserMentioned
            List<User> users = new List<User>();
            usersT.ForEach(x=> users.Add(Utility.GetOrCreateUser(x, soraContext)));
            //var dbUser = Utility.GetOrCreateUser(user, soraContext);

            switch (type)
            {
                case(InteractionType.Pat):
                    users.ForEach(x=> x.Interactions.Pats++);
                    break;
                case(InteractionType.Hug):
                    users.ForEach(x=> x.Interactions.Hugs++);
                    break;
                case(InteractionType.Kiss):
                    users.ForEach(x=> x.Interactions.Kisses++);
                    break;
                case(InteractionType.Poke):
                    users.ForEach(x=> x.Interactions.Pokes++);
                    break;
                case(InteractionType.Slap):
                    users.ForEach(x=> x.Interactions.Slaps++);
                    break;
                case(InteractionType.High5):
                    users.ForEach(x=> x.Interactions.High5++);
                    break;
                case(InteractionType.Punch):
                    users.ForEach(x=> x.Interactions.Punches++);
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
                /*Description = $"" +
                              $"Pats:     {dbUser.Interactions.Pats}\n" +
                              $"High5:     {dbUser.Interactions.High5}\n" +
                              $"Hugs:     {dbUser.Interactions.Hugs}\n" +
                              $"Kisses:   {dbUser.Interactions.Kisses}\n" +
                              $"Pokes:    {dbUser.Interactions.Pokes}\n" +
                              $"Slaps:    {dbUser.Interactions.Slaps}\n" +
                              $"Punches:    {dbUser.Interactions.Punches}\n" +
                              $"Affinity: {Utility.CalculateAffinity(dbUser.Interactions)}/100"*/
            };
            eb.AddField((x) =>
            {
                x.IsInline = true;
                x.Name = $"Pats";
                x.Value= $"{dbUser.Interactions.Pats}";
                
            });
            eb.AddField((x) =>
            {
                x.IsInline = true;
                x.Name = $"High5";
                x.Value= $"{dbUser.Interactions.High5}";
                
            });
            eb.AddField((x) =>
            {
                x.IsInline = true;
                x.Name = $"Hugs";
                x.Value= $"{dbUser.Interactions.Hugs}";
                
            });
            eb.AddField((x) =>
            {
                x.IsInline = true;
                x.Name = $"Kisses";
                x.Value= $"{dbUser.Interactions.Kisses}";
                
            });
            eb.AddField((x) =>
            {
                x.IsInline = true;
                x.Name = $"Pokes";
                x.Value= $"{dbUser.Interactions.Pokes}";
                
            });
            eb.AddField((x) =>
            {
                x.IsInline = true;
                x.Name = $"Slaps";
                x.Value= $"{dbUser.Interactions.Slaps}";
                
            });
            eb.AddField((x) =>
            {
                x.IsInline = true;
                x.Name = $"Punches";
                x.Value= $"{dbUser.Interactions.Punches}";
                
            });
            eb.AddField((x) =>
            {
                x.IsInline = true;
                x.Name = $"Affinity";
                x.Value= $"{Utility.CalculateAffinity(dbUser.Interactions)}/100 ⚜";
                
            });
            await context.Channel.SendMessageAsync("",false,eb);
        
        }
    }
}