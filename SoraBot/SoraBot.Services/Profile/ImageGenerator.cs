using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;
using SixLabors.Shapes;

namespace SoraBot.Services.Profile
{
    public class ProfileImageGenConfig
    {
        public string AvatarPath { get; set; }
        public string BackgroundPath { get; set; }
        public string Name { get; set; }
        public int GlobalRank { get; set; }
        public uint GlobalExp { get; set; }
        public int GlobalLevel { get; set; }
        public uint GlobalNextLevelExp { get; set; }
        public int LocalRank { get; set; }
        public uint LocalExp { get; set; }
        public int LocalLevel { get; set; }
        public uint LocalNextLevelExp { get; set; }
    }

    public class ImageGenerator
    {
        public void GenerateProfileImage(ProfileImageGenConfig conf, string outputPath)
        {
            using var image = new Image<Rgba32>(470, 265);
            this.DrawProfileBackground(conf.BackgroundPath, image, new Size(470, 265));
            this.DrawProfileAvatar(conf.AvatarPath, image, new Rectangle(7, 6, 42, 42), 21);
            image.Save(outputPath);
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