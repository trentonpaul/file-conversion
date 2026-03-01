using ImageMagick;

namespace FileConversation.Api.Services
{
    public class MagickImageConverter : IImageConverter
    {
        public async Task<Stream> ConvertAsync(Stream input, string to)
        {
            Console.WriteLine($"Converting image to {to} using Magick.NET");

            if (!input.CanSeek)
            {
                var copy = new MemoryStream();
                await input.CopyToAsync(copy);
                copy.Position = 0;
                input = copy;
            }

            using var image = new MagickImage(input);
            
            MagickFormat? targetFormat = null;
            if (Enum.TryParse<MagickFormat>(to, true, out var parsed))
            {
                targetFormat = parsed;
            }
            else
            {
                var ext = to.StartsWith('.') ? to : "." + to;
                var info = MagickFormatInfo.Create(ext.ToLower());
                if (info != null)
                    targetFormat = info.Format;
            }

            if (targetFormat == null)
                throw new InvalidOperationException($"Unsupported output format: {to}");

            image.Format = targetFormat.Value;

            var output = new MemoryStream();
            image.Write(output);
            output.Position = 0;
            return output;
        }
    }
}