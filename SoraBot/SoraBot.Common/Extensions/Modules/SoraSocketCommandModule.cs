using System.Threading.Tasks;
using ArgonautCore.Maybe;
using Discord;
using Discord.Commands;
using SoraBot.Common.Utils;
using SoraBot.Data.Models.SoraDb;

namespace SoraBot.Common.Extensions.Modules
{
    public class SoraSocketCommandModule : ModuleBase<SocketCommandContext>
    {
        public static readonly Color Purple = new Color(109, 41, 103);
        public static readonly Color Yellow = new Color(255, 204, 77);
        public static readonly Color Green = new Color(119, 178, 85);
        public static readonly Color Red = new Color(221, 46, 68);
        public static readonly Color Blue = new Color(59, 136, 195);

        public const string SuccessEmoji = "✅";
        public const string WarnEmoji = "⚠";
        public const string FailureEmoji = "❌";
        public const string InfoEmoji = "ℹ";
        public const string PartyEmoji = "🎉";
        public const string MusicalNote = "🎵";

        public async Task<IUserMessage> ReplyEmbed(string message,
            Color? embedColor = null, string emoji = null)
        {
            Color color = embedColor ?? Purple;
            return await ReplyAsync("", embed: SimpleEmbed(color, message, emoji).Build());
        }

        public async Task<IUserMessage> ReplyEmbed(EmbedBuilder eb)
            => await ReplyAsync("", embed: eb.Build()); 

        public async Task<IUserMessage> ReplySuccessEmbed(string message)
        {
            return await ReplyAsync("", embed: SimpleEmbed(Green, message, SuccessEmoji).Build());
        }
        
        public async Task<IUserMessage> ReplyMusicEmbed(string message, string url = null)
        {
            var eb = new EmbedBuilder()
            {
                Color = Blue,
                Title = $"{MusicalNote} {message}"
            };
            if (!string.IsNullOrWhiteSpace(url))
                eb.WithUrl(url);
            return await ReplyAsync("", embed: eb.Build());
        }
        
        public async Task<IUserMessage> ReplyMusicEmbedExtended(string songName, string authorName, string imageUrl, string songLength, string videoUrl, bool added = true)
        {
            var eb = new EmbedBuilder()
            {
                Color = Blue,
                Title = $"{MusicalNote} {(added ? "Enqueued" : "Playing")}: [{songLength}] - **{songName}**",
                Footer = new EmbedFooterBuilder()
                {
                    IconUrl = Context.User.GetAvatarUrl() ?? Context.User.GetDefaultAvatarUrl(),
                    Text = $"Requested by {Formatter.UsernameDiscrim(Context.User)} | Video by {authorName}"
                },
                Url = videoUrl,
            };
            if (!string.IsNullOrWhiteSpace(imageUrl))
                eb.WithThumbnailUrl(imageUrl);
            return await ReplyAsync("", embed: eb.Build());
        }
        
        public async Task<IUserMessage> ReplySuccessEmbedExtended(string title, string desc)
        {
            return await ReplyAsync("", embed: SimpleEmbed(Green, title, SuccessEmoji).WithDescription(desc).Build());
        }

        public async Task<IUserMessage> ReplyFailureEmbed(string message)
        {
            return await ReplyAsync("", embed: SimpleEmbed(Red, message, FailureEmoji).Build());
        }

        public async Task<IUserMessage> ReplyFailureEmbedExtended(string title, string desc)
        {
            return await ReplyAsync("", embed: SimpleEmbed(Red, title, FailureEmoji).WithDescription(desc).Build());
        }
        
        public async Task<IUserMessage> ReplyWarningEmbed(string message)
        {
            return await ReplyAsync("", embed: SimpleEmbed(Yellow, message, WarnEmoji).Build());
        }
        
        public async Task<IUserMessage> ReplyInfoEmbed(string message)
        {
            return await ReplyAsync("", embed: SimpleEmbed(Blue, message, InfoEmoji).Build());
        }
        
        public async Task<IUserMessage> ReplyDefaultEmbed(string message)
        {
            return await ReplyAsync("", embed: SimpleEmbed(Purple, message).Build());
        }

        public async Task<bool> FailedToGetUser(Maybe<User> userMaybe)
        {
            if (userMaybe.HasValue)
                return false;
            // otherwise send error
            await ReplyFailureEmbed("Failed to fetch or create your user data. Please try again.");
            return true;
        }

        public async Task<bool> FailedTryTransaction(bool transactionSucc, string message = "Failed to fetch or update data. Please try again")
        {
            if (transactionSucc) return false;
            // Send error message
            await ReplyFailureEmbed(message);
            return true;
        }
        
        public EmbedFooterBuilder RequestedByMe()
            => new EmbedFooterBuilder()
            {
                Text = $"Requested by {Formatter.UsernameDiscrim(Context.User)}",
                IconUrl = Context.User.GetAvatarUrl() ?? Context.User.GetDefaultAvatarUrl()
            };

        public static EmbedFooterBuilder RequestedByFooter(IUser user)
        {
            return new EmbedFooterBuilder()
            {
                Text = $"Requested by {Formatter.UsernameDiscrim(user)}",
                IconUrl = user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl()
            };
        }

        public static EmbedBuilder SimpleEmbed(Color color, string text)
        {
            var eb = new EmbedBuilder()
            {
                Color = color,
                Title = text
            };
            return eb;
        }
        
        public static EmbedBuilder SimpleEmbed(Color color, string text, string symbol)
        {
            var eb = new EmbedBuilder()
            {
                Color = color,
                Title = $"{symbol} {text}"
            };
            return eb;
        }

        protected async Task<bool> UserHasGuildPermission(GuildPermission guildPerm, string errorMessage = null)
        {
            var user = Context.User as IGuildUser;
            if (user == null) return false;
            if (!user.GuildPermissions.Has(guildPerm))
            {
                await ReplyFailureEmbed(errorMessage ??
                                        $"You require the Guild Permission `{guildPerm.ToString()}` for this command!");
                return false;
            }
            return true;
        }
    }
}