using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using SoraBot_v2.Data;
using SoraBot_v2.Services;

namespace SoraBot_v2.Module
{
    public class InteractionModule : ModuleBase<SocketCommandContext>
    {
        
        private SoraContext _soraContext;
        private InteractionsService _interactions;

        public InteractionModule(SoraContext soracontext, InteractionsService interactionsService)
        {
            _soraContext = soracontext;
            _interactions = interactionsService;
        }
        
        [Command("pat"), Summary("Pats the specified person")]
        public async Task Pat(params SocketUser[] usersT)
        {
            if (usersT.Length < 1)
            {
                await AtLeast1Param(Context);
                return;
            }
            var eb = new EmbedBuilder
            {
                Color = Utility.PurpleEmbed,
            };
            
            var users = usersT.ToList();

            var sameAsInvoker = users.FirstOrDefault(x => x.Id == Context.User.Id);
            if(sameAsInvoker !=null)
            {
                users.Remove(sameAsInvoker);
                if (users.Count == 0)
                {
                    eb.Title =
                        $"{Utility.GiveUsernameDiscrimComb(Context.User)}, why are you patting yourself? Are you okay? ｡ﾟ･（>﹏<）･ﾟ｡";
                    eb.ImageUrl = "https://media.giphy.com/media/wUArrd4mE3pyU/giphy.gif";

                    await Context.Channel.SendMessageAsync("", embed: eb);
                    return;
                }
            }
            var r = new Random();
            
            eb.ImageUrl = $"{Utility.Pats[r.Next(0, Utility.Pats.Length - 1)]}";
            string patted ="";
            users.ForEach(x=>patted += Utility.GiveUsernameDiscrimComb(x)+", ");
            eb.Title =
                $"{Utility.GiveUsernameDiscrimComb(Context.User)} pats {patted.Remove(patted.Length-2)} ｡◕ ‿ ◕｡";
            
            await _interactions.InteractMultiple(InteractionType.Pat, users, Context, _soraContext);

            await Context.Channel.SendMessageAsync("", embed: eb);
        }
        
        [Command("hug"), Summary("Hugs the specified person")]
        public async Task Hug([Summary("Person to hug")]params SocketUser[] usersT)
        {
            if (usersT.Length < 1)
            {
                await AtLeast1Param(Context);
                return;
            }
            
            var eb = new EmbedBuilder
            {
                Color = Utility.PurpleEmbed,
            };

            var users = usersT.ToList();

            var sameAsInvoker = users.FirstOrDefault(x => x.Id == Context.User.Id);
            if(sameAsInvoker != null)
            {
                users.Remove(sameAsInvoker);
                if (users.Count == 0)
                {
                    eb.Title =
                        $"{Utility.GiveUsernameDiscrimComb(Context.User)} don't hug yourself ;-; At least take this pillow (̂ ˃̥̥̥ ˑ̫ ˂̥̥̥ )̂ ";
                    eb.ImageUrl = "http://i.imgur.com/CM0of.gif";
                    await Context.Channel.SendMessageAsync("", embed: eb);
                    return;
                }
            }
            await _interactions.InteractMultiple(InteractionType.Hug, users, Context, _soraContext);
            var r = new Random();
            eb.ImageUrl = $"{Utility.Hugs[r.Next(0, Utility.Hugs.Length - 1)]}";
            string hugged = "";
            users.ForEach(x=> hugged+= Utility.GiveUsernameDiscrimComb(x)+", ");
            eb.Title = $"{Utility.GiveUsernameDiscrimComb(Context.User)} hugged {hugged.Remove(hugged.Length-2)} °˖✧◝(⁰▿⁰)◜✧˖°";
            await Context.Channel.SendMessageAsync("", embed: eb);
        }
        
        [Command("high5"), Alias("h5"), Summary("High5 the specified person")]
        public async Task High5([Summary("Person to High5")]params SocketUser[] usersT)
        {
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

            var users = usersT.ToList();
            var sameAsInvoker = users.FirstOrDefault(x => x.Id == Context.User.Id);
            if(sameAsInvoker!= null)
            {
                users.Remove(sameAsInvoker);
                if (users.Count == 0)
                {
                    eb.Title =
                        $"{Utility.GiveUsernameDiscrimComb(Context.User)} no friends to high five? (̂ ˃̥̥̥ ˑ̫ ˂̥̥̥ )̂ ";
                    eb.ImageUrl = $"{Utility.Self5[r.Next(0, Utility.Self5.Length - 1)]}";
                    await Context.Channel.SendMessageAsync("", embed: eb);
                    return;
                }
            }
            await _interactions.InteractMultiple(InteractionType.High5, users, Context, _soraContext);
            var high5ed = "";
            users.ForEach(x=>high5ed+= Utility.GiveUsernameDiscrimComb(x)+", ");
            eb.ImageUrl = $"{Utility.High5[r.Next(0, Utility.High5.Length - 1)]}";
            eb.Title = $"{Utility.GiveUsernameDiscrimComb(Context.User)} high fived {high5ed.Remove(high5ed.Length -2)} °˖✧◝(⁰▿⁰)◜✧˖°";
            await Context.Channel.SendMessageAsync("", embed: eb);
        }

        [Command("reset"), Summary("Resets your own stats")]
        public async Task Reset()
        {
            //TODO AFFINITY RESET
            await ReplyAsync("Under construction");
        }

        [Command("poke"), Summary("Pokes the specified person")]
        public async Task Poke([Summary("Person to poke")]params SocketUser[] usersT)
        {
            if (usersT.Length < 1)
            {
                await AtLeast1Param(Context);
                return;
            }
            var users = usersT.ToList();

            var sameAsInvoker = users.FirstOrDefault(x => x.Id == Context.User.Id);
            var r = new Random();
            string poked = "";
            users.ForEach(x=> poked+=Utility.GiveUsernameDiscrimComb(x)+", ");
            var eb = new EmbedBuilder()
            {
                Color = Utility.PurpleEmbed,
                Title = $"{Utility.GiveUsernameDiscrimComb(Context.User)} poked {poked.Remove(poked.Length-2)} ( ≧Д≦)",
                ImageUrl = $"{Utility.Pokes[r.Next(0, Utility.Pokes.Length - 1)]}"
            };
            if(sameAsInvoker!= null)
                users.Remove(sameAsInvoker);
            
            await _interactions.InteractMultiple(InteractionType.Poke, users, Context, _soraContext);
            
            await Context.Channel.SendMessageAsync("", embed: eb);
        }

        [Command("kiss"), Summary("Kiss the specified person")]
        public async Task Kiss([Summary("Person to kiss")]params SocketUser[] usersT)
        {
            if (usersT.Length < 1)
            {
                await AtLeast1Param(Context);
                return;
            }
            var users = usersT.ToList();

            var sameAsInvoker = users.FirstOrDefault(x => x.Id == Context.User.Id);
            
            var r = new Random();
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
                    await ReplyAsync("", embed: eb);
                    return;
                }
            }
            await _interactions.InteractMultiple(InteractionType.Kiss, users, Context, _soraContext);
            string kissed = "";
            users.ForEach(x=> kissed+= Utility.GiveUsernameDiscrimComb(x)+", ");
            eb.Title = $"{Utility.GiveUsernameDiscrimComb(Context.User)} kissed {kissed.Remove(kissed.Length-2)} (✿ ♥‿♥)♥";
            eb.ImageUrl = $"{Utility.Kisses[r.Next(0, Utility.Kisses.Length - 1)]}";
            await ReplyAsync("", embed: eb);
        }

        [Command("affinity"), Alias("aff", "stats"), Summary("Shows the Affinity of the specified user or if none is specified your own.")]
        public async Task GetAffinity([Summary("Person to check")]SocketUser UserT = null)
        {
            var user = UserT ?? Context.User;
            await _interactions.CheckAffinity(user, Context, _soraContext);
        }

        [Command("slap"), Summary("Slaps the specified person <.<")]
        public async Task Slap([Summary("Person to slap")]params SocketUser[] usersT)
        {
            if (usersT.Length < 1)
            {
                await AtLeast1Param(Context);
                return;
            }
            var users = usersT.ToList();

            var sameAsInvoker = users.FirstOrDefault(x => x.Id == Context.User.Id);
            
            var eb = new EmbedBuilder()
            {
                Color = Utility.PurpleEmbed
            };
            var r = new Random();
            if (sameAsInvoker != null)
            {
                users.Remove(sameAsInvoker);
                if (users.Count == 0)
                {
                    eb.Title =
                        $"{Utility.GiveUsernameDiscrimComb(Context.User)} why would you slap yourself... Are you okay? 〣( ºΔº )〣";
                    eb.ImageUrl = $"https://media.giphy.com/media/Okk9cb1dvtMxq/giphy.gif";
                    await ReplyAsync("", embed: eb);
                    return;
                }
            }
            await _interactions.InteractMultiple(InteractionType.Slap, users, Context, _soraContext);

            string slapped = "";
            users.ForEach(x=> slapped+= Utility.GiveUsernameDiscrimComb(x)+ ", ");            
            eb.Title = $"{Utility.GiveUsernameDiscrimComb(Context.User)} slapped {slapped.Remove(slapped.Length-2)} (ᗒᗩᗕ)՞ ";
            eb.ImageUrl = $"{Utility.Slaps[r.Next(0, Utility.Slaps.Length - 1)]}"; 
            await ReplyAsync("", embed: eb);
        }
        
        [Command("Punch"), Summary("Punches the specified person o.O")]
        public async Task Punch([Summary("Person to Punch")]params SocketUser[] usersT)
        {
            if (usersT.Length < 1)
            {
                await AtLeast1Param(Context);
                return;
            }
            var users = usersT.ToList();

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
                    await ReplyAsync("", embed: eb);
                    return;
                }
            }
            await _interactions.InteractMultiple(InteractionType.Punch, users, Context, _soraContext);
            string punched = "";
            users.ForEach(x=> punched+= Utility.GiveUsernameDiscrimComb(x)+", ");
            eb.Title = $"{Utility.GiveUsernameDiscrimComb(Context.User)} punched {punched} (ᗒᗩᗕ)՞";
            eb.ImageUrl= $"{Utility.Punches[r.Next(0, Utility.Punches.Length - 1)]}";
            await ReplyAsync("", embed: eb);
        }

        private async Task AtLeast1Param(SocketCommandContext context)
        {
            var eb = new EmbedBuilder()
            {
                Color = Utility.YellowWarningEmbed,
                Title= $"{Utility.SuccessLevelEmoji[1]} You need to specify at least 1 person to be interacted with!"
            };
            await context.Channel.SendMessageAsync("", embed: eb);
        }
    }
}