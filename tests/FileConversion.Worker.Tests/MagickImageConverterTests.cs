using System;
using System.IO;
using System.Threading.Tasks;
using FileConversion.Worker.Services;
using ImageMagick;
using Xunit;

namespace FileConversion.Worker.Tests
{
    public class MagickImageConverterTests
    {
        [Theory]
        [InlineData("jpg", MagickFormat.Jpeg)]
        [InlineData("jpeg", MagickFormat.Jpeg)]
        [InlineData("gif", MagickFormat.Gif)]
        [InlineData("png", MagickFormat.Png)]
        public async Task ConvertAsync_ValidFormat_ReturnsConvertedStream(string format, MagickFormat expected)
        {
            // create a tiny red PNG image in memory (source format doesn't matter)
            using var sourceImage = new MagickImage(MagickColors.Red, 1, 1);
            sourceImage.Format = MagickFormat.Png;

            using var input = new MemoryStream();
            sourceImage.Write(input);
            input.Position = 0;

            var converter = new MagickImageConverter();
            using var result = await converter.ConvertAsync(input, format);

            using var outImage = new MagickImage(result);
            Assert.Equal(expected, outImage.Format);
        }

        [Theory]
        [InlineData("gif", MagickFormat.Gif)]
        [InlineData("jpg", MagickFormat.Jpeg)]
        public async Task ConvertAsync_NonSeekableStream_CopiesAndConverts(string format, MagickFormat expected)
        {
            // prepare a valid PNG stream but wrap it to make it non-seekable
            using var sourceImage = new MagickImage(MagickColors.Blue, 1, 1);
            sourceImage.Format = MagickFormat.Png;

            using var memory = new MemoryStream();
            sourceImage.Write(memory);
            memory.Position = 0;

            using var nonSeekable = new NonSeekableStream(memory);
            var converter = new MagickImageConverter();

            using var result = await converter.ConvertAsync(nonSeekable, format);
            using var outImage = new MagickImage(result);
            Assert.Equal(expected, outImage.Format);
        }

        [Fact]
        public async Task ConvertAsync_UnsupportedFormat_ThrowsInvalidOperationException()
        {
            // supply a valid image stream so the converter advances past the read
            using var sourceImage = new MagickImage(MagickColors.Green, 1, 1);
            sourceImage.Format = MagickFormat.Png;

            using var input = new MemoryStream();
            sourceImage.Write(input);
            input.Position = 0;

            var converter = new MagickImageConverter();
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => converter.ConvertAsync(input, "not-a-format"));
        }

        private class NonSeekableStream : Stream
        {
            private readonly Stream _inner;
            public NonSeekableStream(Stream inner) => _inner = inner;
            public override bool CanRead => _inner.CanRead;
            public override bool CanSeek => false;
            public override bool CanWrite => _inner.CanWrite;
            public override long Length => throw new NotSupportedException();
            public override long Position
            {
                get => throw new NotSupportedException();
                set => throw new NotSupportedException();
            }

            public override void Flush() => _inner.Flush();
            public override int Read(byte[] buffer, int offset, int count) => _inner.Read(buffer, offset, count);
            public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
            public override void SetLength(long value) => _inner.SetLength(value);
            public override void Write(byte[] buffer, int offset, int count) => _inner.Write(buffer, offset, count);
            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    _inner.Dispose();
                }
                base.Dispose(disposing);
            }
        }
    }
}
