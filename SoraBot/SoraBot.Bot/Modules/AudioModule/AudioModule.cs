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

        [Command("leave")]
        [Summary("Make sore leave your voice channel")]
        public async Task LeaveVC()
        {
            if (!_node.TryGetPlayer(Context.Guild, out var player))
            {
                await ReplyFailureEmbed("I'm not connected to any VC in this guild");
                return;
            }

            var playerVC = player.VoiceChannel;
            var userVC = ((IGuildUser) Context.User).VoiceChannel;
            if (playerVC == null || userVC == null)
            {
                await ReplyFailureEmbed("You're not connected to a voice channel!");
                return;
            }
            if (playerVC.Id != userVC.Id)
            {
                await ReplyFailureEmbed("I'm not in the same VC as you. Only listening users can make me leave.");
                return;
            }

            try
            {
                await _node.LeaveAsync(playerVC);
            }
            catch (Exception)
            {
                await ReplyFailureEmbed("Something went wrong when i tried to leave :/");
            }
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