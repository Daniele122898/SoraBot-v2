using System.Numerics;
using System.Threading.Tasks;
using Humanizer;
using ImageSharp;
using SixLabors.Fonts;
using SixLabors.Primitives;

namespace SoraBot_v2.Services
{
    public static class ProfileImageProcessing
    {
        private static FontCollection _fontCollection;
        private static FontFamily _titleFont;

        private static Image<Rgba32> _bgMaskImage;
        private static Image<Rgba32> _noBgMask;
        private static Image<Rgba32> _noBgMaskOverlay;
        private static Image<Rgba32> _shippingMask;

        public static void Initialize()
        {
            _fontCollection = new FontCollection();
            _titleFont = _fontCollection.Install("ProfileCreation/Lato-Bold.ttf");
            _bgMaskImage = Image.Load("ProfileCreation/newBGProfile.png");
            _noBgMask = Image.Load("ProfileCreation/profilecardtemplate.png");
            _noBgMaskOverlay = Image.Load("ProfileCreation/ProfileMASK.png");
            _shippingMask = Image.Load("Shipping/shippingMask.png");
        }

        public static async Task GenerateShipping(string avatarUrl1, string avatarUrl2, string outPath)
        {
            using (var output = new Image<Rgba32>(384, 128))
            {
                DrawMask(_shippingMask, output, new Size(384,128));
                DrawAvatar(avatarUrl1, output, new Rectangle(5,5,118,118));
                DrawAvatar(avatarUrl2, output, new Rectangle(261,5,118,118));
                output.Save(outPath);
            }
        }

        public static async Task GenerateProfileWithBg(string avatarUrl, string backgroundUrl, string name, int rank,
            int level, int exp, string outputPath)
        {
            using (var output = new Image<Rgba32>(960,540))
            {
                DrawBackground(backgroundUrl, output, new Size(960,540));
                
                DrawMask(_bgMaskImage, output, new Size(960,540));
                
                DrawStats(rank, level, exp, output, new Vector2(360,485), new Vector2(920, 485), Rgba32.Gray);

                DrawTitle(name, output, new Vector2(420, 350), Rgba32.White);
                
                DrawAvatar(avatarUrl, output, new Rectangle(204,348,140,140));

                output.Save(outputPath);
            }//dispose of output to help save memory
        }

        public static async Task GenerateProfile(string avatarUrl, string name, int rank, int level, int ep,
            string outputPath)
        {
            using (var output = new Image<Rgba32>(890, 150))
            {
                DrawMask(_noBgMask, output, new Size(1000, 150));

                DrawAvatar(avatarUrl, output, new Rectangle(26, 15, 121, 121));

                DrawMask(_noBgMaskOverlay, output, new Size(1000, 150));

                DrawStats(rank, level, ep, output, new System.Numerics.Vector2(200, 92), new System.Numerics.Vector2(830, 92), Rgba32.Black);

                DrawTitle(name, output, new System.Numerics.Vector2(200, -5), Rgba32.FromHex("#6D2967"));//#2398e1  

                output.Save(outputPath);
            }//dispose of output to help save memory
        }
       


        private static void DrawMask(Image<Rgba32> mask, Image<Rgba32>output, Size size)
        {
            output.DrawImage(mask, 1, size, new Point(0, 0));
        }

        private static void DrawBackground(string backgroundUrl, Image<Rgba32> output, Size size)
        {
            using (Image<Rgba32> background = Image.Load(backgroundUrl)) //960x540
            {
                output.DrawImage(background, 1, size, new Point(0, 0));
            }//can be disposed since no longer needed in memory. 
        }

        private static void DrawAvatar(string avatarUrl, Image<Rgba32> output, Rectangle rec)
        {
            var avatarPosition = rec;

            using (var avatar = Image.Load(avatarUrl)) //57x57
            {
                avatar.Resize(new ImageSharp.Processing.ResizeOptions
                {
                    Mode = ImageSharp.Processing.ResizeMode.Crop,
                    Size = avatarPosition.Size
                });

                output.DrawImage(avatar, 1, avatarPosition.Size, avatarPosition.Location);
            }
        }

        private static void DrawTitle(string name, Image<Rgba32> output, Vector2 pos, Rgba32 color)
        {
            output.DrawText(name, new Font(_titleFont, 60, FontStyle.Bold), color, pos);
        }

        private static void DrawStats(int rank, int level, int exp, Image<Rgba32> output, Vector2 posRank,
            Vector2 posExp, Rgba32 color)
        {
            //measure each string and split the margin between them
            var font = new Font(_titleFont,37,FontStyle.Bold);

            var rankText = $"Rank: {rank}";
            var levelText = $"Level: {level}";
            var epText = $"EXP: {exp}";

            var rankSize = TextMeasurer.Measure(rankText, font, 72);//find the width of the rank text
            var epSize = TextMeasurer.Measure(epText, font, 72);//find the width fo the ep text

            var left = posRank.X + rankSize.Width;//find the point the ranktext stops
            var right = posExp.X - epSize.Width; // fint he point the eptext starts
            
            var posLevel = new Vector2(left+(right-left)/2, posRank.Y);//find the point halfway between the 2 other bits of text

            output.DrawText(rankText, font, color, posRank, new ImageSharp.Drawing.TextGraphicsOptions
            {
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Left
            });
            output.DrawText(levelText, font, color, posLevel, new ImageSharp.Drawing.TextGraphicsOptions
            {
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            });
            output.DrawText(epText, font, color, posExp, new ImageSharp.Drawing.TextGraphicsOptions
            {
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Right
            });
        }
    }
}