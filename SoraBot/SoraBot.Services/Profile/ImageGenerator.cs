using System;
using System.IO;
using System.Numerics;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;
using SixLabors.Shapes;
using SoraBot.Data.Dtos.Profile;
using Path = System.IO.Path;

namespace SoraBot.Services.Profile
{
    

    public class ImageGenerator : IDisposable
    {
        private readonly string _imageGenPath;

        private readonly Image<Rgba32> _templateOverlay;
        private readonly Font _heavyTitleFont;
        private readonly Font _statsLightFont;
        private readonly Font _statsTinyFont;

        public ImageGenerator()
        {
            _imageGenPath = Path.Combine(Directory.GetCurrentDirectory(), "ImageGenerationFiles");
            string pCreationPath = Path.Combine(_imageGenPath, "ProfileCreation");
            // Load template into memory so it's faster in the future :D
            _templateOverlay = Image.Load<Rgba32>(Path.Combine(pCreationPath, "profileTemplate.png"));
            _templateOverlay.Mutate(x => x.Resize(new Size(470, 265)));

            // Add and load fonts
            var fc = new FontCollection();
            var fontHeavy = fc.Install(Path.Combine(pCreationPath, "AvenirLTStd-Heavy.ttf"));
            _heavyTitleFont = new Font(fontHeavy, 20.31f, FontStyle.Bold);
            _statsLightFont = new Font(fontHeavy, 13.4f, FontStyle.Bold);
            _statsTinyFont = new Font(fontHeavy, 10.52f, FontStyle.Bold);
        }

        public void GenerateProfileImage(ProfileImageGenDto conf, string outputPath)
        {
            using var image = new Image<Rgba32>(470, 265);
            this.DrawProfileBackground(conf.BackgroundPath, image, new Size(470, 265));
            this.DrawProfileAvatar(conf.AvatarPath, image, new Rectangle(7, 6, 42, 42), 21);
            // Draw template
            image.Mutate(x => x.DrawImage(_templateOverlay, 1.0f));
            this.DrawStats(image, conf);
            image.Save(outputPath);
        }

        private void DrawStats(Image<Rgba32> image, ProfileImageGenDto c)
        {
            // Draw Username
            image.Mutate(x => x.DrawText(new TextGraphicsOptions(true)
            {
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Left
            }, c.Name, _heavyTitleFont, Rgba32.White, new Vector2(60, 27)));

            var blueHighlight = new Rgba32(47, 166, 222);

            // Draw global Rank
            var globalStatsText = $"GLOBAL RANK: {c.GlobalRank.ToString()}";
            var textSize = TextMeasurer.Measure(globalStatsText, new RendererOptions(_statsLightFont, 72));
            image.Mutate(x => x.DrawText(new TextGraphicsOptions(true)
            {
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Left
            }, "GLOBAL RANK: ", _statsLightFont, Rgba32.White, new Vector2(235 - (textSize.Width / 2), 221)));
            image.Mutate(x => x.DrawText(new TextGraphicsOptions(true)
            {
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Right
            }, c.GlobalRank.ToString(), _statsLightFont, blueHighlight, new Vector2(235 + (textSize.Width / 2), 221)));

            var centerOptions = new TextGraphicsOptions(true)
            {
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            // Draw global level
            image.Mutate(x =>
                x.DrawText(centerOptions, $"LEVEL: {c.GlobalLevel.ToString()}", _statsLightFont, Rgba32.White,
                    new Vector2(235, 236)));
            // Draw global EXP
            image.Mutate(x =>
                x.DrawText(centerOptions, $"{c.GlobalExp.ToString()} / {c.GlobalNextLevelExp.ToString()}",
                    _statsTinyFont, Rgba32.White, new Vector2(235, 250)));
        }

        public void DrawProfileAvatar(string avatarPath, Image<Rgba32> image, Rectangle pos, int cornerRadius)
        {
            using Image<Rgba32> avatar = Image.Load<Rgba32>(avatarPath);

            avatar.Mutate(x => x.Resize(new ResizeOptions()
            {
                Mode = ResizeMode.Crop,
                Size = pos.Size
            }).ApplyRoundedCorners(cornerRadius));


            // avatar.Mutate(x => x.Resize(pos.Size));
            // ReSharper disable once AccessToDisposedClosure
            image.Mutate(x => x.DrawImage(avatar, pos.Location, 1));
        }


        private void DrawProfileBackground(string backgroundPath, Image<Rgba32> image, Size size)
        {
            using Image bg = Image.Load(backgroundPath); // 960x540
            bg.Mutate(x => x.Resize(size));
            // ReSharper disable once AccessToDisposedClosure
            image.Mutate(x => x.DrawImage(bg, 1.0f));
        }

        public void Dispose()
        {
            _templateOverlay?.Dispose();
        }
    }

    internal static class ImageGenerationExtension
    {
        public static IImageProcessingContext ApplyRoundedCorners(this IImageProcessingContext ctx, float cornerRadius)
        {
            Size size = ctx.GetCurrentSize();
            IPathCollection corners = BuildCorners(size.Width, size.Height, cornerRadius);

            return ctx.Fill(new GraphicsOptions(true)
            {
                // enforces that any part of this shape that has color is punched out of the BG
                AlphaCompositionMode = PixelAlphaCompositionMode.DestOut,
            }, Rgba32.LimeGreen, corners); // User any color so that the corners will be clipped
        }

        private static IPathCollection BuildCorners(int imageWidth, int imageHeight, float cornerRadius)
        {
            // Create a square
            var rect = new RectangularPolygon(-0.5f, -0.5f, cornerRadius, cornerRadius);

            // then cut out of the square a circle so we are left with a corner
            IPath cornerTopLeft = rect.Clip(new EllipsePolygon(cornerRadius - 0.5f, cornerRadius - 0.5f, cornerRadius));

            // corner is now positioned in the top left
            // let's make 3 more positioned correctly, we do that by translating the top left one
            // around the center of the image
            // center = width / 2, height /2

            float rightPos = imageWidth - cornerTopLeft.Bounds.Width + 1;
            float bottomPos = imageHeight - cornerTopLeft.Bounds.Height + 1;

            // move it across the width of the image / shape
            IPath cornerTopRight = cornerTopLeft.RotateDegree(90).Translate(rightPos, 0);
            IPath cornerBottomLeft = cornerTopLeft.RotateDegree(-90).Translate(0, bottomPos);
            IPath cornerBottomRight = cornerTopLeft.RotateDegree(180).Translate(rightPos, bottomPos);

            return new PathCollection(cornerTopLeft, cornerBottomLeft, cornerTopRight, cornerBottomRight);
        }
    }
}