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
        public async Task Pat(SocketUser user)
        {
            var eb = new EmbedBuilder
            {
                Color = Utility.PurpleEmbed,
            };
            
            if(Context.User.Id == user.Id)
            {
                eb.Title =
                    $"{Utility.GiveUsernameDiscrimComb(Context.User)}, why are you patting yourself? Are you okay? ｡ﾟ･（>﹏<）･ﾟ｡";
                eb.ImageUrl = "https://media.giphy.com/media/wUArrd4mE3pyU/giphy.gif";

                await Context.Channel.SendMessageAsync("", embed: eb);
                return;
            }
            var r = new Random();
            
            eb.ImageUrl = $"{Utility.Pats[r.Next(0, Utility.Pats.Length - 1)]}";
            eb.Title =
                $"{Utility.GiveUsernameDiscrimComb(Context.User)} pats {Utility.GiveUsernameDiscrimComb(user)} ｡◕ ‿ ◕｡";
            
            await _interactions.Interact(InteractionType.Pat, user, Context, _soraContext);

            await Context.Channel.SendMessageAsync("", embed: eb);
        }
        
        [Command("hug"), Summary("Hugs the specified person")]
        public async Task Hug([Summary("Person to hug")]SocketUser user)
        {
            var eb = new EmbedBuilder
            {
                Color = Utility.PurpleEmbed,
            };
            
            if(Context.User.Id == user.Id)
            {
                eb.Title = $"{Utility.GiveUsernameDiscrimComb(Context.User)} don't hug yourself ;-; At least take this pillow (̂ ˃̥̥̥ ˑ̫ ˂̥̥̥ )̂ ";
                eb.ImageUrl = "http://i.imgur.com/CM0of.gif";
                await Context.Channel.SendMessageAsync("", embed: eb);
                return;
            }
            await _interactions.Interact(InteractionType.Hug, user, Context, _soraContext);
            var r = new Random();
            eb.ImageUrl = $"{Utility.Hugs[r.Next(0, Utility.Hugs.Length - 1)]}";
            eb.Title = $"{Utility.GiveUsernameDiscrimComb(Context.User)} hugged {Utility.GiveUsernameDiscrimComb(user)} °˖✧◝(⁰▿⁰)◜✧˖°";
            await Context.Channel.SendMessageAsync("", embed: eb);
        }

        [Command("reset"), Summary("Resets your own stats")]
        public async Task Reset()
        {
            //TODO AFFINITY RESET
            await ReplyAsync("Under construction");
        }

        [Command("poke"), Summary("Pokes the specified person")]
        public async Task Poke([Summary("Person to poke")]SocketUser User)
        {
            var r = new Random();
            if (Context.User.Id != User.Id)
            {
                await _interactions.Interact(InteractionType.Poke, User, Context, _soraContext);
            }
            var eb = new EmbedBuilder()
            {
                Color = Utility.PurpleEmbed,
                Title = $"{Utility.GiveUsernameDiscrimComb(Context.User)} poked {Utility.GiveUsernameDiscrimComb(User)} ( ≧Д≦)",
                ImageUrl = $"{Utility.Pokes[r.Next(0, Utility.Pokes.Length - 1)]}"
            };
            await Context.Channel.SendMessageAsync("", embed: eb);
        }

        [Command("kiss"), Summary("Kiss the specified person")]
        public async Task Kiss([Summary("Person to kiss")]SocketUser User)
        {
            var r = new Random();
            var eb = new EmbedBuilder()
            {
                Color = Utility.PurpleEmbed
            };
            if (Context.User.Id == User.Id)
            {
                eb.Color = Utility.YellowWarningEmbed;
                eb.Title =
                    $"{Utility.SuccessLevelEmoji[1]}️{Utility.GiveUsernameDiscrimComb(Context.User)} you may pat yourself or hug a pillow but kissing yourself is too much (๑•﹏•)";
                await ReplyAsync("", embed: eb);
                return;
            }
            await _interactions.Interact(InteractionType.Kiss, User, Context, _soraContext);
            eb.Title = $"{Utility.GiveUsernameDiscrimComb(Context.User)} kissed {Utility.GiveUsernameDiscrimComb(User)} (✿ ♥‿♥)♥";
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
        public async Task Slap([Summary("Person to slap")]SocketUser User)
        {
            var eb = new EmbedBuilder()
            {
                Color = Utility.PurpleEmbed
            };
            var r = new Random();
            if (Context.User.Id == User.Id)
            {
                eb.Title = $"{Utility.GiveUsernameDiscrimComb(Context.User)} why would you slap yourself... Are you okay? 〣( ºΔº )〣";
                eb.ImageUrl = $"https://media.giphy.com/media/Okk9cb1dvtMxq/giphy.gif";
                await ReplyAsync("", embed:eb);
                return;
            }
            await _interactions.Interact(InteractionType.Slap, User, Context, _soraContext);

            eb.Title = $"{Utility.GiveUsernameDiscrimComb(Context.User)} slapped {Utility.GiveUsernameDiscrimComb(User)} (ᗒᗩᗕ)՞ ";
            eb.ImageUrl = $"{Utility.Slaps[r.Next(0, Utility.Slaps.Length - 1)]}"; 
            await ReplyAsync("", embed: eb);
        }
        
        [Command("Punch"), Summary("Punches the specified person o.O")]
        public async Task Punch([Summary("Person to Punch")]SocketUser user)
        {
            var eb = new EmbedBuilder()
            {
                Color = Utility.PurpleEmbed
            };
            var r = new Random();
            if (Context.User.Id == user.Id)
            {
                eb.Color = Utility.YellowWarningEmbed;
                eb.Title = $"{Utility.SuccessLevelEmoji[1]} {Utility.GiveUsernameDiscrimComb(Context.User)} you may slap yourself but i wont allow you to punch yourself (̂ ˃̥̥̥ ˑ̫ ˂̥̥̥ )̂ ";
                await ReplyAsync("", embed:eb);
                return;
            }
            await _interactions.Interact(InteractionType.Punch, user, Context, _soraContext);

            eb.Title = $"{Utility.GiveUsernameDiscrimComb(Context.User)} punched {Utility.GiveUsernameDiscrimComb(user)} (ᗒᗩᗕ)՞";
            eb.ImageUrl= $"{Utility.Punches[r.Next(0, Utility.Punches.Length - 1)]}";
            await ReplyAsync("", embed: eb);
        }
    }
}