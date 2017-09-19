using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace LCrypt.Utility.Extensions
{
    public static class StreamExtensions
    {
        public static async Task CopyToAsync(this Stream stream, Stream destination, IProgress<long> progress,
            CancellationToken cancellationToken)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));
            if (destination == null)
                throw new ArgumentNullException(nameof(destination));
            if (progress == null)
                throw new ArgumentNullException(nameof(progress));

            if (!destination.CanWrite)
                throw new NotSupportedException("Destination stream does not support writing.");

            cancellationToken.ThrowIfCancellationRequested();
            progress.Report(0);

            var buffer = new byte[81920]; // Default copy buffer size
            long totalBytesRead = 0;
            int bytesRead;
            while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken).ConfigureAwait(false)) != 0)
            {
                await destination.WriteAsync(buffer, 0, bytesRead, cancellationToken).ConfigureAwait(false);
                totalBytesRead += bytesRead;
                progress.Report(totalBytesRead);
            }
        }
    }
}
