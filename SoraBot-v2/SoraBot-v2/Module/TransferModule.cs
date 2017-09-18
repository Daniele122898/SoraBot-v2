using System.Threading.Tasks;
using Discord.Commands;
using SoraBot_v2.Services;

namespace SoraBot_v2.Module
{
    public class TransferModule //: ModuleBase<SocketCommandContext>
    {/*
        private readonly TransferData _transfer;

        public TransferModule(TransferData transferData)
        {
            _transfer = transferData;
        }*/
        /*
        [Command("syncguild", RunMode = RunMode.Async)]
        [RequireOwner]
        public async Task SyncGuild()
        {
            await ReplyAsync("Started to Sync all Old guild data");
            
            Task.Run(async () =>
            {
                await _transfer.SyncAllGuilds(Context);
            });
        }*/
        /*
        [Command("owners", RunMode = RunMode.Async)]
        [RequireOwner]
        public async Task TextOwners()
        {
            await _transfer.MessageAllGuildOwners(Context);
        }*/
        /*
        [Command("syncusers", RunMode = RunMode.Async)]
        [RequireOwner]
        public async Task SyncUsers()
        {
            Task.Run(async () =>
            {
                await _transfer.SyncAllUsers(Context);
            });

        }*/
    }
}