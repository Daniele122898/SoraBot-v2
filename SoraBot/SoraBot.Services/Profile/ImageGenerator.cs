using System;
using System.IO;
using System.Numerics;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SoraBot.Data.Dtos.Profile;
using Path = System.IO.Path;

namespace SoraBot.Services.Profile
{
    public class ImageGenerator : IDisposable
    {
        public const string AVATAR_CACHE = "AvatarCache";
        public const string PROFILE_BG = "ProfileBackgrounds";
        public const string PROFILE_CARDS = "ProfileCards";
        public const string PROFILE_CREATION = "ProfileCreation";

        public readonly string ImageGenPath;

        private readonly Image<Rgba32> _templateOverlay;
        private readonly Font _heavyTitleFont;
        private readonly Font _statsLightFont;
        private readonly Font _statsTinyFont;

        public ImageGenerator()
        {
            ImageGenPath = Path.Combine(Directory.GetCurrentDirectory(), "ImageGenerationFiles");
            string pCreationPath = Path.Combine(ImageGenPath, PROFILE_CREATION);
            // Load template into memory so it's faster in the future :D
            _templateOverlay = Image.Load<Rgba32>(Path.Combine(pCreationPath, "profileTemplate.png"));
            _templateOverlay.Mutate(x => x.Resize(new Size(470, 265)));

            // Add and load fonts
            var fc = new FontCollection();
            var fontHeavy = fc.Install(Path.Combine(pCreationPath, "AvenirLTStd-Heavy.ttf"));
            _heavyTitleFont = new Font(fontHeavy, 20.31f, FontStyle.Bold);
            _statsLightFont = new Font(fontHeavy, 13.4f, FontStyle.Bold);
            _statsTinyFont = new Font(fontHeavy, 10.52f, FontStyle.Bold);

            // Create accessory directories
            this.CreateImageGenDirectoryIfItDoesntExist(AVATAR_CACHE);
            this.CreateImageGenDirectoryIfItDoesntExist(PROFILE_BG);
            this.CreateImageGenDirectoryIfItDoesntExist(PROFILE_CARDS);
        }

        private void CreateImageGenDirectoryIfItDoesntExist(string dir)
        {
            string path = Path.Combine(ImageGenPath, dir);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        /// <summary>
        /// Uses stream to save file to path. This will NOT DISPOSE of the stream
        /// </summary>
        public void ResizeAndSaveImage(Stream stream, string path, Size size, ResizeMode resizeMode = ResizeMode.Crop)
        {
            using var img = Image.Load<Rgba32>(stream);
            img.Mutate(x => x.Resize(new ResizeOptions()
            {
                Size = size,
                Mode = resizeMode
            }));
            img.Save(path);
        }

        public void ResizeAndSaveImage(string pathToImage, string path, Size size,
            ResizeMode resizeMode = ResizeMode.Crop)
        {
            using var img = Image.Load<Rgba32>(pathToImage);
            img.Mutate(x => x.Resize(new ResizeOptions()
            {
                Size = size,
                Mode = resizeMode
            }));
            img.Save(path);
        }

        public void GenerateProfileImage(ProfileImageGenDto conf, string outputPath)
        {
            string backgroundPath = conf.HasCustomBg
                ? Path.Combine(ImageGenPath, PROFILE_BG, $"{conf.UserId.ToString()}.png")
                : Path.Combine(ImageGenPath, PROFILE_CREATION, "defaultBG.png");
            string avatarPath = Path.Combine(ImageGenPath, AVATAR_CACHE, $"{conf.UserId.ToString()}.png");

            using var image = new Image<Rgba32>(470, 265);
            this.DrawProfileBackground(backgroundPath, image, new Size(470, 265));
            this.DrawProfileAvatar(avatarPath, image, new Rectangle(7, 6, 42, 42), 21);
            // Draw template
            image.Mutate(x => x.DrawImage(_templateOverlay, 1.0f));
            this.DrawStats(image, conf);
            image.Save(outputPath);
        }

        private void DrawStats(Image<Rgba32> image, ProfileImageGenDto c)
        {
            var textGraphicOptionsCenterLeft = new TextGraphicsOptions(
                new GraphicsOptions()
                {
                    Antialias = true
                },
                new TextOptions()
                {
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Left
                });

            var textGraphicOptionsCenterRight = new TextGraphicsOptions(
                new GraphicsOptions()
                {
                    Antialias = true
                },
                new TextOptions()
                {
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Right
                });

            if (string.IsNullOrWhiteSpace(c.ClanName))
            {
                // Draw Username
                image.Mutate(x => x.DrawText(
                    textGraphicOptionsCenterLeft,
                    c.Name,
                    _heavyTitleFont,
                    Color.White,
                    new Vector2(60, 27)));
            }
            else
            {
                // Draw Username
                image.Mutate(x => x.DrawText(
                    textGraphicOptionsCenterLeft,
                    c.Name,
                    _heavyTitleFont,
                    Color.White,
                    new Vector2(60, 16)));
                
                // Draw Clan name
                image.Mutate(x => x.DrawText(
                    textGraphicOptionsCenterLeft,
                    c.ClanName,
                    _statsLightFont,
                    Color.White,
                    new Vector2(60, 36)));
            }


            var blueHighlight = new Rgba32(47, 166, 222);

            // Draw global Rank
            var globalStatsText = $"GLOBAL RANK: {c.GlobalRank.ToString()}";
            var renderOps = new RendererOptions(_statsLightFont, 72);
            var textSize = TextMeasurer.Measure(globalStatsText, renderOps);
            image.Mutate(x => x.DrawText(
                textGraphicOptionsCenterLeft,
                "GLOBAL RANK: ",
                _statsLightFont,
                Color.White,
                new Vector2(235 - (textSize.Width / 2), 221)));

            image.Mutate(x => x.DrawText(
                textGraphicOptionsCenterRight,
                c.GlobalRank.ToString(),
                _statsLightFont,
                blueHighlight,
                new Vector2(235 + (textSize.Width / 2), 221)));

            var centerOptions = new TextGraphicsOptions(
                new GraphicsOptions()
                {
                    Antialias = true
                },
                new TextOptions()
                {
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center
                });
            // Draw global level
            image.Mutate(x =>
                x.DrawText(centerOptions, $"LEVEL: {c.GlobalLevel.ToString()}", _statsLightFont, Color.White,
                    new Vector2(235, 236)));
            // Draw global EXP
            image.Mutate(x =>
                x.DrawText(centerOptions, $"{c.GlobalExp.ToString()} / {c.GlobalNextLevelExp.ToString()}",
                    _statsTinyFont, Color.White, new Vector2(235, 250)));

            // Draw local STats
            var localStatsText = $"LOCAL RANK: {c.LocalRank.ToString()}";
            var localTextSize = TextMeasurer.Measure(localStatsText, renderOps);

            image.Mutate(x =>
                x.DrawText(textGraphicOptionsCenterLeft, "LOCAL RANK: ", _statsLightFont, Color.White,
                    new Vector2(386 - (localTextSize.Width / 2), 221)));
            image.Mutate(x =>
                x.DrawText(textGraphicOptionsCenterRight, c.LocalRank.ToString(), _statsLightFont,
                    blueHighlight, new Vector2(386 + (localTextSize.Width / 2), 221)));
            image.Mutate(x =>
                x.DrawText(centerOptions, $"LEVEL: {c.LocalLevel.ToString()}", _statsLightFont, Color.White,
                    new Vector2(386, 236)));
            image.Mutate(x =>
                x.DrawText(centerOptions, $"{c.LocalExp.ToString()} / {c.LocalNextLevelExp.ToString()}",
                    _statsTinyFont, Color.White, new Vector2(386, 250)));
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

            ctx.SetGraphicsOptions(new GraphicsOptions()
            {
                Antialias = true,
                AlphaCompositionMode = PixelAlphaCompositionMode.DestOut
            });

            return ctx.Fill(Color.Red, corners);
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