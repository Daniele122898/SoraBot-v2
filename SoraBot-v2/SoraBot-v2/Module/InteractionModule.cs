﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using SoraBot_v2.Data;
using SoraBot_v2.Services;
using Weeb.net;

namespace SoraBot_v2.Module
{
    [Name("Interaction")]
    public class InteractionModule : ModuleBase<SocketCommandContext>
    {
        
        private readonly InteractionsService _interactions;
        private readonly WeebService _weebService;

        public InteractionModule(InteractionsService interactionsService, WeebService weebService)
        {
            _interactions = interactionsService;
            _weebService = weebService;
        }

        [Command("types", RunMode = RunMode.Async), Alias("interactions"), Summary("Gets all interaction types")]
        public async Task Types()
        {
            await _weebService.GetTypes(Context);
        }
        
        [Command("pat",RunMode = RunMode.Async), Summary("Pats the specified person")]
        public async Task Pat([Summary("Mention the users you want to pat and maybe add a reason"), Remainder] string reason)
        {
            SocketUser[] usersT = Context.Message.MentionedUsers.ToArray();
            if (usersT.Length < 1)
            {
                await AtLeast1Param(Context);
                return;
            }
            var eb = new EmbedBuilder
            {
                Color = Utility.PurpleEmbed,
            };
            
            var users = usersT.Distinct().ToList();

            var sameAsInvoker = users.FirstOrDefault(x => x.Id == Context.User.Id);
            if(sameAsInvoker !=null)
            {
                users.Remove(sameAsInvoker);
                if (users.Count == 0)
                {
                    eb.Title =
                        $"{Utility.GiveUsernameDiscrimComb(Context.User)}, why are you patting yourself? Are you okay? ｡ﾟ･（>﹏<）･ﾟ｡";
                    eb.ImageUrl = "https://i.imgur.com/QFtH3Gl.gif";

                    await Context.Channel.SendMessageAsync("", embed: eb.Build());
                    return;
                }
            }

            eb.Footer = new EmbedFooterBuilder()
            {
                Text = "Powered by weeb.sh and the weeb.net wrapper"
            };
            var image = await _weebService.GetRandImage("pat", new string[] { }, FileType.Gif, NsfwSearch.False);
            eb.ImageUrl = image.Url;
            string patted ="";
            users.ForEach(x=>patted += Utility.GiveUsernameDiscrimComb(x)+", ");
            patted = (patted.Length > 150 ? $"{patted.Remove(150)}..." : patted);
            eb.Title =
                $"{Utility.GiveUsernameDiscrimComb(Context.User)} pats {patted.Remove(patted.Length-2)} ｡◕ ‿ ◕｡";

            using (var soraContext = new SoraContext())
            {
                await _interactions.InteractMultiple(InteractionType.Pat, users, Context, soraContext);
            }

            await Context.Channel.SendMessageAsync("", embed: eb.Build());
        }
        
        [Command("hug",RunMode = RunMode.Async), Summary("Hugs the specified person")]
        public async Task Hug([Summary("Mention the users you want to hug and maybe add a reason"), Remainder]string reason)
        {
            SocketUser[] usersT = Context.Message.MentionedUsers.ToArray();
            if (usersT.Length < 1)
            {
                await AtLeast1Param(Context);
                return;
            }
            
            var eb = new EmbedBuilder
            {
                Color = Utility.PurpleEmbed,
            };

            var users = usersT.Distinct().ToList();

            var sameAsInvoker = users.FirstOrDefault(x => x.Id == Context.User.Id);
            if(sameAsInvoker != null)
            {
                users.Remove(sameAsInvoker);
                if (users.Count == 0)
                {
                    eb.Title =
                        $"{Utility.GiveUsernameDiscrimComb(Context.User)} don't hug yourself ;-; At least take this pillow (̂ ˃̥̥̥ ˑ̫ ˂̥̥̥ )̂ ";
                    eb.ImageUrl = "http://i.imgur.com/CM0of.gif";
                    await Context.Channel.SendMessageAsync("", embed: eb.Build());
                    return;
                }
            }
            using (var soraContext = new SoraContext())
            {
                await _interactions.InteractMultiple(InteractionType.Hug, users, Context, soraContext);
            }
            
            eb.Footer = new EmbedFooterBuilder()
            {
                Text = "Powered by weeb.sh and the weeb.net wrapper"
            };
            var image = await _weebService.GetRandImage("hug", new string[] { }, FileType.Gif, NsfwSearch.False);
            eb.ImageUrl = image.Url;
            
            string hugged = "";
            users.ForEach(x=> hugged+= Utility.GiveUsernameDiscrimComb(x)+", ");
            hugged = (hugged.Length > 150 ? $"{hugged.Remove(150)}..." : hugged);

            eb.Title = $"{Utility.GiveUsernameDiscrimComb(Context.User)} hugged {hugged.Remove(hugged.Length-2)} °˖✧◝(⁰▿⁰)◜✧˖°";
            await Context.Channel.SendMessageAsync("", embed: eb.Build());
        }
        
        [Command("high5"), Alias("h5"), Summary("High5 the specified person")]
        public async Task High5([Summary("Mention the users you want to high5 and maybe add a reason"), Remainder]string reason)
        {
            SocketUser[] usersT = Context.Message.MentionedUsers.ToArray();
            if (usersT.Length < 1)
            {
                await AtLeast1Param(Context);
                return;
            }
            var eb = new EmbedBuilder
            {
                Color = Utility.PurpleEmbed,
            };
            var r = new Random();

            var users = usersT.Distinct().ToList();
            var sameAsInvoker = users.FirstOrDefault(x => x.Id == Context.User.Id);
            if(sameAsInvoker!= null)
            {
                users.Remove(sameAsInvoker);
                if (users.Count == 0)
                {
                    eb.Title =
                        $"{Utility.GiveUsernameDiscrimComb(Context.User)} no friends to high five? (̂ ˃̥̥̥ ˑ̫ ˂̥̥̥ )̂ ";
                    eb.ImageUrl = $"{Utility.Self5[r.Next(0, Utility.Self5.Length)]}";
                    await Context.Channel.SendMessageAsync("", embed: eb.Build());
                    return;
                }
            }
            using (var soraContext = new SoraContext())
            {
                await _interactions.InteractMultiple(InteractionType.High5, users, Context, soraContext);
            }
            var high5ed = "";
            users.ForEach(x=>high5ed+= Utility.GiveUsernameDiscrimComb(x)+", ");
            high5ed = (high5ed.Length > 150 ? $"{high5ed.Remove(150)}..." : high5ed);

            eb.ImageUrl = $"{Utility.High5[r.Next(0, Utility.High5.Length)]}";
            eb.Title = $"{Utility.GiveUsernameDiscrimComb(Context.User)} high fived {high5ed.Remove(high5ed.Length -2)} °˖✧◝(⁰▿⁰)◜✧˖°";
            await Context.Channel.SendMessageAsync("", embed: eb.Build());
        }

        [Command("poke",RunMode = RunMode.Async), Summary("Pokes the specified person")]
        public async Task Poke([Summary("Mention the users you want to poke and maybe add a reason"), Remainder]string reason)
        {
            SocketUser[] usersT = Context.Message.MentionedUsers.ToArray();
            if (usersT.Length < 1)
            {
                await AtLeast1Param(Context);
                return;
            }
            var users = usersT.Distinct().ToList();

            var sameAsInvoker = users.FirstOrDefault(x => x.Id == Context.User.Id);
            string poked = "";
            users.ForEach(x=> poked+=Utility.GiveUsernameDiscrimComb(x)+", ");
            poked = (poked.Length > 150 ? $"{poked.Remove(150)}..." : poked);
            
            var image = await _weebService.GetRandImage("poke", new string[] { }, FileType.Gif, NsfwSearch.False);

            var eb = new EmbedBuilder()
            {
                Color = Utility.PurpleEmbed,
                Title = $"{Utility.GiveUsernameDiscrimComb(Context.User)} poked {poked.Remove(poked.Length-2)} ( ≧Д≦)",
                ImageUrl = image.Url,//$"{Utility.Pokes[r.Next(0, Utility.Pokes.Length )]}"
                Footer = new EmbedFooterBuilder()
                {
                    Text = "Powered by weeb.sh and the weeb.net wrapper"
                }
            };
            if(sameAsInvoker!= null)
                users.Remove(sameAsInvoker);
            using (var soraContext = new SoraContext())
            {
                await _interactions.InteractMultiple(InteractionType.Poke, users, Context, soraContext);
            }

            await Context.Channel.SendMessageAsync("", embed: eb.Build());
        }

        [Command("kiss",RunMode = RunMode.Async), Summary("Kiss the specified person")]
        public async Task Kiss([Summary("Mention the users you want to kiss and maybe add a reason"), Remainder]string reason)
        {
            SocketUser[] usersT = Context.Message.MentionedUsers.ToArray();
            if (usersT.Length < 1)
            {
                await AtLeast1Param(Context);
                return;
            }
            var users = usersT.Distinct().ToList();

            var sameAsInvoker = users.FirstOrDefault(x => x.Id == Context.User.Id);
            
            var eb = new EmbedBuilder()
            {
                Color = Utility.PurpleEmbed
            };
            if (sameAsInvoker != null)
            {
                users.Remove(sameAsInvoker);
                if (users.Count == 0)
                {
                    eb.Color = Utility.YellowWarningEmbed;
                    eb.Title =
                        $"{Utility.SuccessLevelEmoji[1]}️{Utility.GiveUsernameDiscrimComb(Context.User)} you may pat yourself or hug a pillow but kissing yourself is too much (๑•﹏•)";
                    await ReplyAsync("", embed: eb.Build());
                    return;
                }
            }
            using (var soraContext = new SoraContext())
            {
                await _interactions.InteractMultiple(InteractionType.Kiss, users, Context, soraContext);
            }
            string kissed = "";
            users.ForEach(x=> kissed+= Utility.GiveUsernameDiscrimComb(x)+", ");
            kissed = (kissed.Length > 150 ? $"{kissed.Remove(150)}..." : kissed);

            eb.Title = $"{Utility.GiveUsernameDiscrimComb(Context.User)} kissed {kissed.Remove(kissed.Length-2)} (✿ ♥‿♥)♥";
            
            eb.Footer = new EmbedFooterBuilder()
            {
                Text = "Powered by weeb.sh and the weeb.net wrapper"
            };
            var image = await _weebService.GetRandImage("kiss", new string[] { }, FileType.Gif, NsfwSearch.False);
            eb.ImageUrl = image.Url;
            
            //eb.ImageUrl = $"{Utility.Kisses[r.Next(0, Utility.Kisses.Length)]}";
            await ReplyAsync("", embed: eb.Build());
        }

        [Command("affinity"), Alias("aff", "stats"), Summary("Shows the Affinity of the specified user or if none is specified your own.")]
        public async Task GetAffinity([Summary("Person to check")]SocketUser UserT = null)
        {
            var user = UserT ?? Context.User;
            using (var soraContext = new SoraContext())
            {
                await _interactions.CheckAffinity(user, Context, soraContext);
            }
        }

        [Command("slap",RunMode = RunMode.Async), Summary("Slaps the specified person <.<")]
        public async Task Slap([Summary("Mention the users you want to slap and maybe add a reason"), Remainder]string reason)
        {
            SocketUser[] usersT = Context.Message.MentionedUsers.ToArray();
            if (usersT.Length < 1)
            {
                await AtLeast1Param(Context);
                return;
            }
            var users = usersT.Distinct().ToList();

            var sameAsInvoker = users.FirstOrDefault(x => x.Id == Context.User.Id);
            
            var eb = new EmbedBuilder()
            {
                Color = Utility.PurpleEmbed
            };
            if (sameAsInvoker != null)
            {
                users.Remove(sameAsInvoker);
                if (users.Count == 0)
                {
                    eb.Title =
                        $"{Utility.GiveUsernameDiscrimComb(Context.User)} why would you slap yourself... Are you okay? 〣( ºΔº )〣";
                    eb.ImageUrl = $"https://media.giphy.com/media/Okk9cb1dvtMxq/giphy.gif";
                    await ReplyAsync("", embed: eb.Build());
                    return;
                }
            }
            using (var soraContext = new SoraContext())
            {
                await _interactions.InteractMultiple(InteractionType.Slap, users, Context, soraContext);
            }

            string slapped = "";
            users.ForEach(x=> slapped+= Utility.GiveUsernameDiscrimComb(x)+ ", ");
            slapped = (slapped.Length > 150 ? $"{slapped.Remove(150)}..." : slapped);
            eb.Title = $"{Utility.GiveUsernameDiscrimComb(Context.User)} slapped {slapped.Remove(slapped.Length-2)} (ᗒᗩᗕ)՞ ";
            
            eb.Footer = new EmbedFooterBuilder()
            {
                Text = "Powered by weeb.sh and the weeb.net wrapper"
            };
            var image = await _weebService.GetRandImage("slap", new string[] { }, FileType.Gif, NsfwSearch.False);
            eb.ImageUrl = image.Url;
            
            //eb.ImageUrl = $"{Utility.Slaps[r.Next(0, Utility.Slaps.Length)]}"; 
            await ReplyAsync("", embed: eb.Build());
        }
        
        [Command("Punch"), Summary("Punches the specified person o.O")]
        public async Task Punch([Summary("Mention the users you want to punch and maybe add a reason"), Remainder]string reason)
        {
            SocketUser[] usersT = Context.Message.MentionedUsers.ToArray();
            if (usersT.Length < 1)
            {
                await AtLeast1Param(Context);
                return;
            }
            var users = usersT.Distinct().ToList();

            var sameAsInvoker = users.FirstOrDefault(x => x.Id == Context.User.Id);
            var eb = new EmbedBuilder()
            {
                Color = Utility.PurpleEmbed
            };
            var r = new Random();
            
            if (sameAsInvoker!= null)
            {
                users.Remove(sameAsInvoker);
                if (users.Count == 0)
                {
                    eb.Color = Utility.YellowWarningEmbed;
                    eb.Title =
                        $"{Utility.SuccessLevelEmoji[1]} {Utility.GiveUsernameDiscrimComb(Context.User)} you may slap yourself but i wont allow you to punch yourself (̂ ˃̥̥̥ ˑ̫ ˂̥̥̥ )̂ ";
                    await ReplyAsync("", embed: eb.Build());
                    return;
                }
            }
            using (var soraContext = new SoraContext())
            {
                await _interactions.InteractMultiple(InteractionType.Punch, users, Context, soraContext);
            }
            string punched = "";
            users.ForEach(x=> punched+= Utility.GiveUsernameDiscrimComb(x)+", ");
            punched = (punched.Length > 150 ? $"{punched.Remove(150)}..." : punched);

            eb.Title = $"{Utility.GiveUsernameDiscrimComb(Context.User)} punched {punched} (ᗒᗩᗕ)՞";
            eb.ImageUrl= $"{Utility.Punches[r.Next(0, Utility.Punches.Length)]}";
            await ReplyAsync("", embed: eb.Build());
        }

        private async Task AtLeast1Param(SocketCommandContext context)
        {
            var eb = new EmbedBuilder()
            {
                Color = Utility.RedFailiureEmbed,
                Title= $"{Utility.SuccessLevelEmoji[2]} You need to specify at least 1 person to be interacted with! (@Mention them)"
            };
            await context.Channel.SendMessageAsync("", embed: eb.Build());
        }
    }
}