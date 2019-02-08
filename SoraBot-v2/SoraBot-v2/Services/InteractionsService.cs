using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using SoraBot_v2.Data;
using SoraBot_v2.Data.Entities;
using Weeb.net;
using Weeb.net.Data;

namespace SoraBot_v2.Services
{

    public enum InteractionType
    {
        Pat, Hug, Kiss, Poke, Slap, Punch, High5
    }
    public class InteractionsService
    {

        private WeebService _weebService;
        private static List<string> _taken = new List<string>(){"hug", "kiss", "pat", "poke", "slap", "highfive", "punch"};
        
        public InteractionsService(WeebService weebService)
        {
            _weebService = weebService;
        }
        
        public static  Dictionary<Func<int, bool>, string> MySwitch = new Dictionary<Func<int, bool>, string>
        {
            {x=>x <10,"☢"},
            {x=>x <20,"👹"},
            {x=>x <30,"🤢"},
            {x=>x <40,"👺"},
            {x=>x <50,"⚠"},
            {x=>x <60,"🤔"},
            {x=>x <70,"😒"},
            {x=>x <80,"😀"},
            {x=>x <90,"♥"},
            {x=>x <100,"💕"},
            {x=>x ==100,"💯"},
        };
        
        public async Task Interact(InteractionType type, SocketUser user, SocketCommandContext context, SoraContext soraContext)
        {
            //FindUserMentioned
            var dbUser = Utility.GetOrCreateUser(user.Id, soraContext);

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

            await soraContext.SaveChangesAsync();
        }

        private static bool RemoveInter(string s)
        {
            return _taken.Contains(s);
        }

        private string UserDiscrimCombo(IUser user)
        {
            return $"{user.Username}#{user.Discriminator}";
        }

        private string GetTitle(string type, ICommandContext context, string interacted)
        {
            switch (type)
            {
                    case "cuddle":
                        return $"{UserDiscrimCombo(context.User)} cuddled {interacted.Remove(interacted.Length - 2)} °˖✧◝(⁰▿⁰)◜✧˖°";
                    case "insult":
                        return $"{UserDiscrimCombo(context.User)} insulted {interacted.Remove(interacted.Length - 2)} (⌯˃̶᷄ ﹏ ˂̶᷄⌯)ﾟ";
                    case "lick":
                        return $"{UserDiscrimCombo(context.User)} licked {interacted.Remove(interacted.Length - 2)} ༼ つ ◕o◕ ༽つ";
                    case "nom":
                        return $"{UserDiscrimCombo(context.User)} nommed {interacted.Remove(interacted.Length - 2)} (*ﾟﾛﾟ)";
                    case "stare":
                        return $"{UserDiscrimCombo(context.User)} stares at {interacted.Remove(interacted.Length - 2)} ⁄(⁄ ⁄•⁄-⁄•⁄ ⁄)⁄";
                    case "tickle":
                        return $"{UserDiscrimCombo(context.User)} tickled {interacted.Remove(interacted.Length - 2)} o(*≧□≦)o";
                    case "bite":   
                        return $"{UserDiscrimCombo(context.User)} bit {interacted.Remove(interacted.Length - 2)} (˃̶᷄︿๏）";
                    case "greet":
                        return $"{UserDiscrimCombo(context.User)} bit {interacted.Remove(interacted.Length - 2)} (=ﾟωﾟ)ノ";
                    default:
                        return null;
            }
        }

        public async Task AddOtherCommands(CommandService service)
        {
            //  get all tags
            TypesData types = await _weebService.GetTypesRaw();
            // if cant get types just quit
            if (types == null) return;    
            
            types.Types.RemoveAll(RemoveInter);
            var module = await service.CreateModuleAsync("", build =>
            {
                foreach (var type in types.Types)
                {
                    build.AddCommand(type, async (context, objects, serviceProvider, commandInfo) =>
                    {
                        var image = await _weebService.GetRandImage(type, new string[] { }, FileType.Any, NsfwSearch.False);
                        
                        if (image == null)
                        {
                            await context.Channel.SendMessageAsync("",
                                embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2],
                                    "Failed to get image. Try another one.").Build());
                            return;
                        }

                        ulong[] usersT = context.Message.MentionedUserIds.ToArray();
                        string title = null;
                        if (usersT.Length > 0)
                        {
                            var users = usersT.Distinct().ToList();
                            
                            // remove self
                            users.Remove(context.User.Id);
                            // remove sora
                            users.Remove(context.Client.CurrentUser.Id);
                            
                            if (users.Count > 0)
                            {
                                string interacted ="";
                                users.ForEach(x=>interacted += UserDiscrimCombo(context.Guild.GetUserAsync(x).Result)+", ");
                                interacted = (interacted.Length > 150 ? $"{interacted.Remove(150)}..." : interacted);
                                title = GetTitle(type, context, interacted);
                            }
                        }
                        
                        var eb = new EmbedBuilder()
                        {
                            Color = Utility.PurpleEmbed,
                            Footer = new EmbedFooterBuilder()
                            {
                                Text = $"Powered by weeb.sh and the weeb.net wrapper"
                            },
                            ImageUrl = image.Url
                        };

                        if (!string.IsNullOrWhiteSpace(title))
                        {
                            eb.Title = title;
                        }
                        
                        await context.Channel.SendMessageAsync("", embed: eb.Build());
                    }, builder =>
                        {
                            builder.AddParameter("remainder", typeof(string),
                                parameterBuilder =>
                                {
                                    parameterBuilder.IsRemainder = true;
                                    parameterBuilder.IsOptional = true;
                                });
                        });    
                }
                
            });
        }
        
        public async Task InteractMultiple(InteractionType type, List<SocketUser> usersT, SocketCommandContext context, SoraContext soraContext)
        {
            //FindUserMentioned
            List<User> users = new List<User>();
            usersT.ForEach(x=> users.Add(Utility.GetOrCreateUser(x.Id, soraContext)));

            User giver = Utility.GetOrCreateUser(context.User.Id, soraContext);
            //var dbUser = Utility.GetOrCreateUser(user, soraContext);

            switch (type)
            {
                case(InteractionType.Pat):
                    foreach (var user in users)
                    {
                        user.Interactions.Pats++;
                        giver.Interactions.PatsGiven++;
                    }
                    break;
                case(InteractionType.Hug):
                    foreach (var user in users)
                    {
                        user.Interactions.Hugs++;
                        giver.Interactions.HugsGiven++;
                    }
                    break;
                case(InteractionType.Kiss):
                    foreach (var user in users)
                    {
                        user.Interactions.Kisses++;
                        giver.Interactions.KissesGiven++;
                    }
                    break;
                case(InteractionType.Poke):
                    foreach (var user in users)
                    {
                        user.Interactions.Pokes++;
                        giver.Interactions.PokesGiven++;
                    }
                    break;
                case(InteractionType.Slap):
                    foreach (var user in users)
                    {
                        user.Interactions.Slaps++;
                        giver.Interactions.SlapsGiven++;
                    }
                    break;
                case(InteractionType.High5):
                    foreach (var user in users)
                    {
                        user.Interactions.High5++;
                        giver.Interactions.High5Given++;
                    }
                    break;
                case(InteractionType.Punch):
                    foreach (var user in users)
                    {
                        user.Interactions.Punches++;
                        giver.Interactions.PunchesGiven++;
                    }
                    break;
                default:
                    await context.Channel.SendMessageAsync(":no_entry_sign: Something went horribly wrong :eyes:");
                    break;
            }

            await soraContext.SaveChangesAsync();
            soraContext.Dispose();
        }

        public async Task CheckAffinity(SocketUser user, SocketCommandContext context, SoraContext soraContext)
        {
            //FindUserMentioned
            var dbUser = Utility.OnlyGetUser(user.Id, soraContext);
            if (dbUser == null)
            {
                await context.Channel.SendMessageAsync("",
                    embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], $"{Utility.GiveUsernameDiscrimComb(user)} has no Interactions yet!").Build());
                return;
            }
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
                Description = "Received Interactions / Given Interactions"
            };
            eb.AddField((x) =>
            {
                x.IsInline = true;
                x.Name = $"Pats";
                x.Value= $"{dbUser.Interactions.Pats}/{dbUser.Interactions.PatsGiven}";
                
            });
            eb.AddField((x) =>
            {
                x.IsInline = true;
                x.Name = $"High5";
                x.Value= $"{dbUser.Interactions.High5}/{dbUser.Interactions.High5Given}";
                
            });
            eb.AddField((x) =>
            {
                x.IsInline = true;
                x.Name = $"Hugs";
                x.Value= $"{dbUser.Interactions.Hugs}/{dbUser.Interactions.HugsGiven}";
                
            });
            eb.AddField((x) =>
            {
                x.IsInline = true;
                x.Name = $"Kisses";
                x.Value= $"{dbUser.Interactions.Kisses}/{dbUser.Interactions.KissesGiven}";
                
            });
            eb.AddField((x) =>
            {
                x.IsInline = true;
                x.Name = $"Pokes";
                x.Value= $"{dbUser.Interactions.Pokes}/{dbUser.Interactions.PokesGiven}";
                
            });
            eb.AddField((x) =>
            {
                x.IsInline = true;
                x.Name = $"Slaps";
                x.Value= $"{dbUser.Interactions.Slaps}/{dbUser.Interactions.SlapsGiven}";
                
            });
            eb.AddField((x) =>
            {
                x.IsInline = true;
                x.Name = $"Punches";
                x.Value= $"{dbUser.Interactions.Punches}/{dbUser.Interactions.PunchesGiven}";
                
            });
            eb.AddField((x) =>
            {
                x.IsInline = true;
                x.Name = $"Affinity";
                double aff = Utility.CalculateAffinity(dbUser.Interactions);
                string icon = MySwitch.First(sw => sw.Key((int) Math.Round(aff))).Value;
                x.Value= $"{aff}/100 {icon}";
                
            });
            await context.Channel.SendMessageAsync("",false,eb.Build());
        
        }
    }
}