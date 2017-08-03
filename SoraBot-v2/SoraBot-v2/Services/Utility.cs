using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using SoraBot_v2.Data;
using SoraBot_v2.Data.Entities;
using SoraBot_v2.Data.Entities.SubEntities;

namespace SoraBot_v2.Services
{
    public static class Utility
    {

        public static Discord.Color PurpleEmbed = new Discord.Color(109, 41, 103);
        public static string StandardDiscordAvatar = "http://i.imgur.com/tcpgezi.jpg";
        
        public static User GetOrCreateUser(SocketUser user, SoraContext soraContext)
        {
            var result = soraContext.Users.FirstOrDefault(x => x.UserId == user.Id);
            if (result == null)
            {
                //User Not found => CREATE
                var addedUser = soraContext.Users.Add(new User() {UserId = user.Id, Interactions = new Interactions()});
                return addedUser.Entity;
            }
            result.Interactions = soraContext.Interactions.FirstOrDefault(x => x.UserForeignId == user.Id);
            return result;
        }

        public static double CalculateAffinity(Interactions interactions)
        {
            double total = interactions.Pats + interactions.Hugs * 2 + interactions.Kisses* 3 + interactions.Slaps + interactions.Punches*2;
            double good = interactions.Pats + interactions.Hugs * 2 + interactions.Kisses * 3;
            if (total == 0)
                return 0;
            if (good == 0)
                return 0;
            return Math.Round((100.0 / total * good), 2);
        }
    }
}