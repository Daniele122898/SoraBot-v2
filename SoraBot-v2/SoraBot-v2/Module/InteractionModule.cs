using System;
using System.Threading.Tasks;
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
        public async Task Pat(SocketUser User)
        {
            if(Context.User.Id == User.Id)
            {
                await ReplyAsync($"{Context.User.Mention} don't hug yourself ;-; At least take this pillow (̂ ˃̥̥̥ ˑ̫ ˂̥̥̥ )̂ \n http://i.imgur.com/CM0of.gif");
                return;
            }
            await _interactions.Interact(InteractionType.Pat, User, Context, _soraContext);
            var r = new Random();
            await ReplyAsync($"{Context.User.Mention} pats {User.Mention} ｡◕ ‿ ◕｡ \n{_pats[r.Next(0,_pats.Length-1)]}");
        }
        
        [Command("hug"), Summary("Hugs the specified person")]
        public async Task Hug([Summary("Person to hug")]SocketUser User)
        {
            if(Context.User.Id == User.Id)
            {
                await ReplyAsync($"{Context.User.Mention} don't hug yourself ;-; At least take this pillow (̂ ˃̥̥̥ ˑ̫ ˂̥̥̥ )̂ \n http://i.imgur.com/CM0of.gif");
                return;
            }
            await _interactions.Interact(InteractionType.Hug, User, Context, _soraContext);
            var r = new Random();
            await ReplyAsync($"{Context.User.Mention} hugged {User.Mention} °˖✧◝(⁰▿⁰)◜✧˖°\n{_hugs[r.Next(0,_hugs.Length-1)]}");
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
            await ReplyAsync($"{Context.User.Mention} poked {User.Mention} ( ≧Д≦)\n{_pokes[r.Next(0, _pokes.Length - 1)]}");
        }

        [Command("kiss"), Summary("Kiss the specified person")]
        public async Task Kiss([Summary("Person to kiss")]SocketUser User)
        {
            var r = new Random();
            if (Context.User.Id == User.Id)
            {
                await ReplyAsync($"{Context.User.Mention} you may pat yourself or hug a pillow but kissing yourself is too much (๑•﹏•)");
                return;
            }
            await _interactions.Interact(InteractionType.Kiss, User, Context, _soraContext);
            await ReplyAsync($"{Context.User.Mention} kissed {User.Mention} (✿ ♥‿♥)♥\n{_kisses[r.Next(0, _kisses.Length - 1)]}");
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
            var r = new Random();
            if (Context.User.Id == User.Id)
            {
                await ReplyAsync($"{Context.User.Mention} why would you slap yourself... Are you okay? 〣( ºΔº )〣\n https://media.giphy.com/media/Okk9cb1dvtMxq/giphy.gif");
                return;
            }
            await _interactions.Interact(InteractionType.Slap, User, Context, _soraContext);
            await ReplyAsync($"{Context.User.Mention} slapped {User.Mention} (ᗒᗩᗕ)՞ \n{_slaps[r.Next(0, _slaps.Length - 1)]}");
        }
        
        [Command("Punch"), Summary("Punches the specified person o.O")]
        public async Task Punch([Summary("Person to Punch")]SocketUser user)
        {
            /*
            var r = new Random();
            if (Context.User.Id == user.Id)
            {
                await ReplyAsync($"{Context.User.Mention} why would you slap yourself... Are you okay? 〣( ºΔº )〣\n https://media.giphy.com/media/Okk9cb1dvtMxq/giphy.gif");
                return;
            }
            await _interactions.Interact(InteractionType.Punch, user, Context, _soraContext);
            await ReplyAsync($"{Context.User.Mention} slapped {user.Mention} (ᗒᗩᗕ)՞ \n{_slaps[r.Next(0, _slaps.Length - 1)]}");
            */
            //TODO PUNCH
            await ReplyAsync("UNDER CONSTRUCTION");
        }
    }
}