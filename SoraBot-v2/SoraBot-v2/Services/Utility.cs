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
        public static Discord.Color YellowWarningEmbed = new Discord.Color(255,204,77);
        public static string StandardDiscordAvatar = "http://i.imgur.com/tcpgezi.jpg";

        public static string[] SuccessLevelEmoji = new string[]
        {
            "✅","⚠","❌"
        };
        
        #region Gifs
        public static string[] Pats = new string[]
        {
            "https://media.giphy.com/media/3ohzdLjvu2Q8rQLspq/source.gif",
            "http://i.imgur.com/bDMMk0L.gif",
            "http://i.imgur.com/LxbKriA.gif",
            "http://i.imgur.com/gQ5r1li.gif",
            "http://i.imgur.com/yHsXnMg.gif",
            "http://i.imgur.com/M5kqhq9.gif",
            "http://i.imgur.com/ulbteUq.gif",
            "http://i.imgur.com/DwojHLE.gif",
            "http://i.imgur.com/uyvFoxz.gif",
            "http://i.imgur.com/arv0y4f.gif",
            "https://m.popkey.co/a5cfaf/1x6lW.gif",
            "http://i.imgur.com/otTgjpn.gif",
            "https://media.giphy.com/media/ye7OTQgwmVuVy/giphy.gif",
            "http://i.imgur.com/cPS1JlS.gif",
            "https://media.giphy.com/media/KZQlfylo73AMU/giphy.gif",
            "https://media.giphy.com/media/xgTs8CcCMbqb6/giphy.gif"
        };
        public static string[] Hugs = new string[]
        {
            "https://media.giphy.com/media/od5H3PmEG5EVq/giphy.gif",
            "http://i.imgur.com/t4hw0by.gif",
            "https://m.popkey.co/fca5d5/bXDgV.gif",
            "https://media.giphy.com/media/143v0Z4767T15e/giphy.gif",
            "http://i.imgur.com/vbiLwKl.gif",
            "http://i.imgur.com/vbiLwKl.gif",
            "http://i.imgur.com/xDjTItB.gif",
            "http://i.imgur.com/wmU5rg1.gif",
            "https://media.giphy.com/media/du8yT5dStTeMg/giphy.gif",
            "https://media.giphy.com/media/kvKFM3UWg2P04/giphy.gif",
            "https://media.giphy.com/media/wnsgren9NtITS/giphy.gif",
            "http://i.imgur.com/nbWWuYJ.gif",
            "http://i.imgur.com/ffsADGT.gif"
        };

        public static string[] Pokes = new string[]
        {
            "https://media.giphy.com/media/ovbDDmY4Kphtu/giphy.gif",
            "http://i.imgur.com/TtV7VRg.gif",
            "https://media.giphy.com/media/pWd3gD577gOqs/giphy.gif",
            "https://media.giphy.com/media/WvVzZ9mCyMjsc/giphy.gif",
            "https://media.giphy.com/media/LXTQN2kRbaqAw/giphy.gif",
            "http://i.imgur.com/1NzLne8.gif",
            "http://i.imgur.com/VtWJ8ak.gif",
            "http://i.imgur.com/rasGw2Z.gif",
            "http://i.imgur.com/g8k3KkH.gif"
        };

        public static string[] Slaps= new string[]
        {
            "https://i.imgur.com/oY3UC4g.gif",
            "http://i.imgur.com/8Q45tO7.gif",
            "http://i.imgur.com/BpTaDPy.gif",
            "http://i.imgur.com/AB07ibk.gif",
            "http://i.imgur.com/MBX2kMu.gif",
            "http://i.imgur.com/CqhzJ72.gif",
            "http://i.imgur.com/Pxom6ma.gif",
            "http://i.imgur.com/6AnlHwX.gif",
            "http://i.imgur.com/yEL7bpC.gif",
            "http://i.imgur.com/3rHE4Ee.gif",
            "http://i.imgur.com/ihkVAis.gif",
            ""
        };

        public static string[] Kisses= new string[]
        {
            "http://i.imgur.com/I9CROFT.gif",
            "http://i.imgur.com/iK5fmug.gif",
            "http://i.imgur.com/dvRHPBL.gif",
            "http://i.imgur.com/brclvvu.gif",
            "http://i.imgur.com/jC0LGI1.gif",
            "http://i.imgur.com/nQ2jGRe.gif",
            "http://i.imgur.com/znX38JU.gif",
            "http://i.imgur.com/kRz9dq0.gif",
            "http://i.imgur.com/3cXlM6i.gif",
            "http://i.imgur.com/AzB99oj.gif",
            "http://i.imgur.com/zkuYWxW.gif",
            "http://i.imgur.com/4ttao29.gif",
            "http://i.imgur.com/USSPwRM.gif",
            "http://i.imgur.com/tCO461O.gif",
            "http://i.imgur.com/GW1BXj8.gif"
        };

        public static string[] Punches = new string[]
        {
            "http://i.imgur.com/wH4S2CX.gif",
            "http://i.imgur.com/G09HFZs.gif",
            "http://i.imgur.com/GbRgS8h.gif",
            "http://i.imgur.com/tiB026d.gif",
            "http://i.imgur.com/VXlBPm4.gif",
            "http://i.imgur.com/6w2SNY2.gif",
            "http://i.imgur.com/3XQr4pm.gif",
            "http://i.imgur.com/tlbnCVX.gif",
            "http://i.imgur.com/FThVNEf.gif",
            "http://i.imgur.com/KP230Rp.gif"
        };
        #endregion

        
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

        public static string GiveUsernameDiscrimComb(SocketUser user)
        {
            return $"{user.Username}#{user.Discriminator}";
        }
    }
}