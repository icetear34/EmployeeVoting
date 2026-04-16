using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace EmployeeVoting.Api.Infrastructure.Services
{
    /// <summary>
    /// 驗證碼圖片生成器介面
    /// </summary>
    public interface ICaptchaImageGenerator
    {
        /// <summary>
        /// 產生驗證碼圖片
        /// </summary>
        /// <param name="code">驗證碼文字</param>
        /// <returns>Base64 編碼的圖片</returns>
        string Generate(string code);
    }

    /// <summary>
    /// 驗證碼圖片生成器實作
    /// </summary>
    public class CaptchaImageGenerator : ICaptchaImageGenerator
    {
        private static readonly Random _random = new();

        public string Generate(string code)
        {
            const int width = 120;
            const int height = 40;

            using var image = new Image<Rgba32>(width, height);

            // 背景色
            image.Mutate(ctx =>
            {
                ctx.Fill(Color.White);

                // 加入干擾線
                for (int i = 0; i < 5; i++)
                {
                    var pen = Pens.Solid(GetRandomLightColor(), 1);
                    var points = new PointF[]
                    {
                        new PointF(_random.Next(width), _random.Next(height)),
                        new PointF(_random.Next(width), _random.Next(height))
                    };
                    ctx.DrawLine(pen, points);
                }

                // 加入干擾點
                for (int i = 0; i < 50; i++)
                {
                    var x = _random.Next(width);
                    var y = _random.Next(height);
                    image[x, y] = GetRandomLightColor();
                }
            });

            // 繪製文字
            FontFamily fontFamily;
            try
            {
                fontFamily = SystemFonts.Get("Arial");
            }
            catch
            {
                fontFamily = SystemFonts.Families.First();
            }

            var font = fontFamily.CreateFont(24, FontStyle.Bold);

            for (int i = 0; i < code.Length; i++)
            {
                using var charImage = new Image<Rgba32>(30, height);
                charImage.Mutate(ctx =>
                {
                    ctx.Fill(Color.Transparent);

                    var textOptions = new RichTextOptions(font)
                    {
                        Origin = new PointF(5, 8),
                        HorizontalAlignment = HorizontalAlignment.Left
                    };

                    ctx.DrawText(textOptions, code[i].ToString(), GetRandomDarkColor());

                    // 輕微旋轉
                    var angle = _random.Next(-15, 15);
                    ctx.Rotate(angle);
                });

                image.Mutate(ctx =>
                {
                    ctx.DrawImage(charImage, new Point(10 + i * 25, 0), 1f);
                });
            }

            // 輸出為 Base64
            using var ms = new MemoryStream();
            image.Save(ms, new PngEncoder());
            return $"data:image/png;base64,{Convert.ToBase64String(ms.ToArray())}";
        }

        private static Color GetRandomLightColor()
        {
            return Color.FromRgb(
                (byte)_random.Next(150, 256),
                (byte)_random.Next(150, 256),
                (byte)_random.Next(150, 256));
        }

        private static Color GetRandomDarkColor()
        {
            return Color.FromRgb(
                (byte)_random.Next(0, 100),
                (byte)_random.Next(0, 100),
                (byte)_random.Next(0, 100));
        }
    }
}
