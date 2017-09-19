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
            cancellationToken.ThrowIfCancellationRequested();

            double bytesRead;

        }
    }
}
