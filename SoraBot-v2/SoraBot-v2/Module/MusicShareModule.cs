using System;
using System.Threading.Tasks;
using Discord.Commands;
using SoraBot_v2.Services;

namespace SoraBot_v2.Module
{
    public class MusicShareModule : ModuleBase<SocketCommandContext>
    {
        private MusicShareService _service;

        public MusicShareModule(MusicShareService service)
        {
            _service = service;
        }

        [Command("privateplaylist", RunMode = RunMode.Async), Alias("private", "unlisted"), 
         Summary("Create a private playlist. Attention ppl can still import it using the link. It simply doesn't show up in searches")]
        public async Task PrivatePlaylist([Summary("Url to Hastebin")] string url,
            [Summary("title of playlist | trap;edm;chill"), Remainder] string titleAndTags)
        {
            await SharePlaylist(Context, url, titleAndTags, true);   
        }
        
        [Command("setprivate"), Alias("private"), Summary("Change the share state to Private for specified playlist URL")]
        public async Task SetPrivate([Summary("Hastebin URL")] string url)
        {
            await _service.SetPrivate(Context, url.Trim());
        }

        [Command("setpublic"), Alias("public"), Summary("Change the share state to Public for specified playlist URL")]
        public async Task SetPublic([Summary("Hastebin URL")] string url)
        {
            await _service.SetPublic(Context, url.Trim());
        }

        private async Task SharePlaylist(SocketCommandContext context, string url, string titleAndTags, bool isPrivate)
        {
            int index = titleAndTags.IndexOf('|');
            if (index < 1)
            {
                //FAILED TO SEPERATE
                await context.Channel.SendMessageAsync("", embed:
                    Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], "Failed to share!")
                        .WithDescription($"Make sure the format is `{(isPrivate ? "private" : "share")} hastebinURL playlist title | trap;edm;chill`").Build());
                return;
            }
            string title = titleAndTags.Remove(index);
            string tags = titleAndTags.Substring(index + 1).ToLower();

            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(tags))
            {
                await context.Channel.SendMessageAsync("", embed:
                    Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], "Title or Tags are empty! Add them!").Build());
                return;
            }
            
            if (title.Length > 100)
            {
                await Context.Channel.SendMessageAsync("", embed:
                    Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], "Title too long! Please don't exceed 100 chars for the title!").Build());
                return;
            }
            await _service.SharePlaylist(context, url.Trim(), title.Trim(), tags.Trim(), isPrivate);
        }

        [Command("shareplaylist", RunMode = RunMode.Async), Alias("share"),
         Summary("Share a Playlist with the rest of the world!")]
        public async Task SharePlaylist([Summary("Url to Hastebin")]string url, [Summary("title of playlist | trap;edm;chill"), Remainder] string titleAndTags)
        {
            await SharePlaylist(Context, url, titleAndTags, false);
        }

        [Command("downvote"), Alias("dv","votedown", "down"), Summary("Downvote a playlist")]
        public async Task DownVotePlaylist([Summary("hastebin url to playlist")] string url)
        {
            await _service.VotePlaylist(Context, url.Trim(), false);
        }
        
        [Command("upvote"), Alias("uv","voteup","up"), Summary("Upvote a playlist")]
        public async Task UpVotePlaylist([Summary("hastebin url to playlist")] string url)
        {
            await _service.VotePlaylist(Context, url.Trim(), true);
        }

        [Command("playlists", RunMode = RunMode.Async),
         Alias("allplaylists", "bestplaylists", "allshared", "bestshared"), Summary("Shows all shared playlists")]
        public async Task AllPlaylists()
        {
            await _service.GetAllPlaylists(Context);
        }
        
        [Command("search name", RunMode = RunMode.Async), Alias("searchname", "sn"),
         Summary("Search shared playlists by Name")]
        public async Task SearchByName([Summary("Name to search: Some Name"), Remainder] string name)
        {
            await _service.SearchPlaylistsByName(Context, name.Trim());
        }

        [Command("search tag", RunMode = RunMode.Async), Alias("searchtag", "st", "search tags", "searchtags"),
         Summary("Search shared playlists by Tags")]
        public async Task SearchByTag([Summary("tags to search: `tag;tag`"), Remainder] string tags)
        {
            await _service.SearchPlaylistByTags(Context, tags.Trim());
        }

        [Command("myshared", RunMode = RunMode.Async), Alias("myplaylist", "myplaylists", "myshare"),
         Summary("Shows all your shared playlists")]
        public async Task MySharedPlaylists()
        {
            await _service.ShowAllMySharedPlaylists(Context);
        }

        [Command("removepshared"), Alias("removeplaylist", "rs", "rp"), Summary("Removes specified playlist")]
        public async Task RemoveMyPlaylist([Summary("Hastebin URL to remove")] string url)
        {
            await _service.RemovePlaylist(Context, url.Trim());
        }

        [Command("editshare", RunMode = RunMode.Async), Alias("editplaylist", "es", "ep"),
         Summary("Edit ur own playlist entries")]
        public async Task EditPlaylist([Summary("Url to Hastebin")]string url, [Summary("title of playlist | trap;edm;chill"), Remainder] string titleAndTags)
        {
            int index = titleAndTags.IndexOf('|');
            if (index < 1)
            {
                //FAILED TO SEPERATE
                await Context.Channel.SendMessageAsync("", embed:
                    Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], "Failed to Edit!")
                        .WithDescription($"Make sure the format is `share hastebinURL playlist title | trap;edm;chill`").Build());
                return;
            }
            string title = titleAndTags.Remove(index);
            string tags = titleAndTags.Substring(index + 1).ToLower();

            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(tags))
            {
                await Context.Channel.SendMessageAsync("", embed:
                    Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], "Title or Tags are empty! Add them!").Build());
                return;
            }

            if (title.Length > 100)
            {
                await Context.Channel.SendMessageAsync("", embed:
                    Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], "Title too long! Please don't exceed 100 chars for the title!").Build());
                return;
            }
            await _service.UpdateEntry(Context, url.Trim(), title.Trim(), tags.Trim());
        }
       
        //TODO REPORT
        
    }
}