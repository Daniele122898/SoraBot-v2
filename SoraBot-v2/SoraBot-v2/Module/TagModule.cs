using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using SoraBot_v2.Services;

namespace SoraBot_v2.Module
{
    public class TagModule : InteractiveBase<SocketCommandContext>
    {


        public TagModule()
        {

        }
        
        [Command("taglist"), Alias("tl"), Summary("Shows all the tags that exist in the guild")]
        public async Task SearchTag()
        {
            try
            {
                var pages = new List<string>
                {
                    "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Phasellus semper felis sapien, in gravida neque pharetra nec. Aliquam eu feugiat lectus. Vestibulum sollicitudin, neque vitae lacinia viverra, arcu quam accumsan turpis, et sollicitudin nisi erat ut nibh. Cras venenatis vulputate ipsum sed laoreet. Nulla ut ligula elit. Cras ornare justo id risus venenatis, sed porttitor purus consectetur. Fusce quam arcu, egestas nec felis eu, finibus auctor felis. Donec imperdiet, risus at laoreet efficitur, lacus enim ultricies sapien, a rutrum nibh enim quis nunc. Interdum et malesuada fames ac ante ipsum primis in faucibus. Donec ac orci elementum, luctus leo scelerisque, egestas lorem. Praesent vitae diam nec tortor mollis tristique sed sit amet ligula. Phasellus quis felis nisi. Cras non ultricies turpis.",
                    "Fusce blandit augue nec dignissim tempor. Ut mollis ligula neque, eget eleifend sem fringilla eget. Cras id massa imperdiet, egestas nulla vitae, egestas tellus. Nam dapibus nibh libero, non lacinia enim accumsan at. Donec dapibus felis nunc, sed tincidunt metus bibendum a. Quisque eget convallis mauris. Integer erat dui, tincidunt aliquet fringilla nec, gravida in ligula. Maecenas imperdiet porttitor est sed pretium.",
                    "Nam dignissim dolor quis dolor ornare lobortis. Aliquam iaculis neque id auctor facilisis. Fusce vehicula enim vitae quam semper suscipit. Cras pharetra risus turpis, at faucibus enim dictum nec. ",
                    "Phasellus ut sapien luctus, dapibus ligula a, pulvinar lectus. Cras elementum lorem sodales ullamcorper tincidunt. Nam eleifend fermentum varius. Etiam maximus tempor pharetra",
                    "Praesent in rutrum lectus, eu ultrices diam. Aenean nec aliquam justo, at fermentum lorem. Sed mattis varius vehicula. Cras non volutpat nisi. Suspendisse potenti. Nullam tincidunt velit id maximus gravida. Etiam id tellus ac sem elementum tristique quis non ante."
                };
                var pmsg = new PaginatedMessage()
                {
                    Author = new EmbedAuthorBuilder()
                    {
                        IconUrl = Context.User.GetAvatarUrl() ?? Utility.StandardDiscordAvatar,
                        Name = Context.User.Username
                    },
                    Color = Utility.PurpleEmbed,
                    Title = "Lorem Impsum",
                    Options = new PaginatedAppearanceOptions()
                    {
                        DisplayInformationIcon = true,
                        InformationText = "React with the respective icons to change page",
                        
                    },
                    Content = "Only the invoker may switch pages, ⏹ to stop the pagination",
                    Pages = pages
                };

                await PagedReplyAsync(pmsg);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

        }
    }
}