using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using SoraBot.Common.Extensions.Modules;
using Victoria;

namespace SoraBot.Bot.Modules.AudioModule
{
    
    [Name("Music")]
    [Summary("All commands around music playing :>")]
    public class AudioModule : SoraSocketCommandModule
    {
        private readonly LavaNode _node;

        public AudioModule(LavaNode node)
        {
            _node = node;
        }

        [Command("join")]
        [Summary("Make Sora join your voice channel")]
        public async Task Join()
        {
            if (_node.HasPlayer(Context.Guild))
            {
                await ReplyFailureEmbed("I'm already in another Voice Channel. Dont try to steal me >.<");
                return;
            }

            var voiceChannel = ((IGuildUser) Context.User).VoiceChannel;
            if (voiceChannel == null)
            {
                await ReplyFailureEmbed("You are currently not in a Voice Channel!");
                return;
            }

            try
            {
                await _node.JoinAsync(voiceChannel, (ITextChannel) Context.Channel);
            }
            catch (Exception)
            {
                await ReplyFailureEmbed("Failed to join your channel for some reason :/ Maybe i lack permission.");
            }
        }
    }
}