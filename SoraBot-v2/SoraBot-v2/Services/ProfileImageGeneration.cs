using System;
using System.Numerics;
using SixLabors.ImageSharp;
using SixLabors.Fonts;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;
using SixLabors.Shapes;

namespace SoraBot_v2.Services
{
    public class ProfileImageGeneration
    {
        private static FontCollection _fontCollection;
        private static FontFamily _fontheavy;
        private static FontFamily _fontLight;

        private static Image<Rgba32> _profileTemplate;
        private static Image<Rgba32> _shippingMask;

        public static void Initialize()
        {
            _fontCollection = new FontCollection();
            //_fontLight = _fontCollection.Install("ProfileCreation/AvenirLTStd-LightOblique.ttf");
            _fontLight = _fontCollection.Install("ProfileCreation/AvenirLTStd-Light.ttf");
            _fontheavy = _fontCollection.Install("ProfileCreation/AvenirLTStd-Heavy.ttf");
            _profileTemplate = Image.Load("ProfileCreation/profileTemplate1.png");
            _shippingMask = Image.Load("Shipping/shippingMask.png");
        }

        public static void GenerateShipping(string avatarUrl1, string avatarUrl2, string outPath)
        {
            using (var output = new Image<Rgba32>(384, 128))
            {
                DrawMask(_shippingMask, output, new Size(384,128));
                DrawShipAv(avatarUrl1, output, new Rectangle(5,5,118,118));
                DrawShipAv(avatarUrl2, output, new Rectangle(261,5,118,118));
                output.Save(outPath);
            }
        }
        
        private static void DrawMask(Image<Rgba32> mask, Image<Rgba32>output, Size size)
        {
            output.Mutate(x=>  x.DrawImage(mask, 1, size, new Point(0, 0)));
        }
        
        private static void DrawShipAv(string avatarUrl, Image<Rgba32> output, Rectangle rec)
        {
            var avatarPosition = rec;

            using (var avatar = Image.Load(avatarUrl)) //57x57
            {
                avatar.Mutate(x=> x.Resize(new ResizeOptions()
                {
                    Mode = ResizeMode.Crop,
                    Size = avatarPosition.Size
                }));
                output.Mutate(x=> x.DrawImage(avatar,1, avatarPosition.Size, avatarPosition.Location));
            }
        }

        public static void GenerateProfile(string avatarUrl, string backgroundUrl, string name,string clanName, int globalRank, int globalLevel, int globalExp, int globalExpNeeded, int localRank, int localLevel, int localExp, int localExpNeeded, string outputPath)
        {
            int avatarSize = 42;
            using (var output = new Image<Rgba32>(470, 265))
            {
                DrawBackground(backgroundUrl, output, new Size(470, 265));
                
                DrawAvatar(avatarUrl, output, new Rectangle(7,6,avatarSize,avatarSize), 21);

                DrawTemplate(_profileTemplate, output, new Size(470, 265));
                
                DrawStats(output, name, clanName, globalRank, globalLevel, globalExp, globalExpNeeded, localRank, localLevel, localExp, localExpNeeded);
                
                output.Save(outputPath);
            }//dispose of output to help save memory
        }

        private static void DrawStats(Image<Rgba32> output, string username, string clanName, int globalRank, int globalLevel, int globalExp, int globalExpNeeded, int localRank, int localLevel, int localExp, int localExpNeeded)
        {
            var heavy = new Font(_fontheavy, 20.31f, FontStyle.Bold);
            var light = new Font(_fontLight, 15.4f, FontStyle.Regular);
            
            //Draw name and clan
            output.Mutate(x=> x.DrawText(username, heavy, Rgba32.White, new Vector2(60,14), new TextGraphicsOptions(true)
            {
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Left
            }));
            
            output.Mutate(x=> x.DrawText(clanName, light, Rgba32.White, new Vector2(60,34), new TextGraphicsOptions(true)
            {
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Left
            }));

            //Draw global stats
            var oStats = new Font(_fontheavy, 13.4f, FontStyle.Bold);
            var expStats = new Font(_fontheavy, 10.52f, FontStyle.Bold);
            
            var globalStatsText = $"GLOBAL RANK: {globalRank}";
            var textSize = TextMeasurer.Measure(globalStatsText, new RendererOptions(oStats, 72));
            
            output.Mutate(x=> x.DrawText($"GLOBAL RANK: ", oStats, Rgba32.White, new Vector2(235-(textSize.Width/2), 221), new TextGraphicsOptions(true)
            {
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Left
            }));
            output.Mutate(x=> x.DrawText($"{globalRank}", oStats, new Rgba32(47,166,222), new Vector2(235+(textSize.Width/2), 221), new TextGraphicsOptions(true)
            {
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Right
            }));
            output.Mutate(x=> x.DrawText($"LEVEL: {globalLevel}", oStats, Rgba32.White, new Vector2(235, 236), new TextGraphicsOptions(true)
            {
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            }));
            output.Mutate(x=> x.DrawText($"{globalExp} / {globalExpNeeded}", expStats, Rgba32.White, new Vector2(235, 250), new TextGraphicsOptions(true)
            {
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            }));
            
            //draw local stats
            var localStatsText = $"LOCAL RANK: {localRank}";
            var localTextSize = TextMeasurer.Measure(localStatsText, new RendererOptions(oStats, 72));
            
            output.Mutate(x=> x.DrawText($"LOCAL RANK: ", oStats, Rgba32.White, new Vector2(386-(localTextSize.Width/2), 221), new TextGraphicsOptions(true)
            {
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Left
            }));
            output.Mutate(x=> x.DrawText($"{localRank}", oStats, new Rgba32(47,166,222), new Vector2(386+(localTextSize.Width/2), 221), new TextGraphicsOptions(true)
            {
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Right
            }));
            output.Mutate(x=> x.DrawText($"LEVEL: {localLevel}", oStats, Rgba32.White, new Vector2(386, 236), new TextGraphicsOptions(true)
            {
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            }));
            output.Mutate(x=> x.DrawText($"{localExp} / {localExpNeeded}", expStats, Rgba32.White, new Vector2(386, 250), new TextGraphicsOptions(true)
            {
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            }));
        }

        private static void DrawTemplate(Image<Rgba32> template, Image<Rgba32> output, Size size)
        {
            output.Mutate(x=> x.DrawImage(template, 1, size, new Point(0, 0)));
        }
        
        private static void DrawAvatar(string avatarUrl, Image<Rgba32> output, Rectangle rec, float cornerRadius)
        {
            var avatarPosition = rec;

            using (var avatar = Image.Load(avatarUrl)) //57x57
            {
                avatar.Mutate(x=> x.Resize(new ResizeOptions()
                {
                    Mode = ResizeMode.Crop,
                    Size = avatarPosition.Size
                }).Apply(i=> ApplyRoundedCorners(i, cornerRadius)));
                output.Mutate(x=> x.DrawImage(avatar,1, avatarPosition.Size, avatarPosition.Location));
            }
        }

        private static void ApplyRoundedCorners(Image<Rgba32> img, float cornerRadius)
        {
            IPathCollection corner = BuildCorners(img.Width, img.Height, cornerRadius);
            
            img.Mutate(x=> x.Fill(Rgba32.Transparent, corner, new GraphicsOptions(true)
            {
                BlenderMode = PixelBlenderMode.Src // enforces that any part of this shape that has color is punched out of the background
            }));
        }
        
        private static IPathCollection BuildCorners(int imageWidth, int imageHeight, float cornerRadius)
        {
            // first create a square
            var rect = new RectangularePolygon(-0.5f, -0.5f, cornerRadius, cornerRadius);

            // then cut out of the square a circle so we are left with a corner
            IPath cornerToptLeft = rect.Clip(new EllipsePolygon(cornerRadius - 0.5f, cornerRadius - 0.5f, cornerRadius));

            // corner is now a corner shape positions top left
            //lets make 3 more positioned correctly, we can do that by translating the orgional artound the center of the image
            var center = new Vector2(imageWidth / 2F, imageHeight / 2F);

            float rightPos = imageWidth - cornerToptLeft.Bounds.Width + 1;
            float bottomPos = imageHeight - cornerToptLeft.Bounds.Height + 1;

            // move it across the widthof the image - the width of the shape
            IPath cornerTopRight = cornerToptLeft.RotateDegree(90).Translate(rightPos, 0);
            IPath cornerBottomLeft = cornerToptLeft.RotateDegree(-90).Translate(0, bottomPos);
            IPath cornerBottomRight = cornerToptLeft.RotateDegree(180).Translate(rightPos, bottomPos);

            return new PathCollection(cornerToptLeft, cornerBottomLeft, cornerTopRight, cornerBottomRight);
        }

        private static void DrawBackground(string backgroundUrl, Image<Rgba32> output, Size size)
        {
            using (Image<Rgba32> background = Image.Load(backgroundUrl)) //960x540
            {
                output.Mutate(x=> x.DrawImage(background, 1, size, new Point(0, 0)));
            }//can be disposed since no longer needed in memory. 
        }
    }
}