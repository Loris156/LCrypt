using CommandLine;
using LCrypt.Core.Crypto;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;

namespace LCrypt.CLI.Commands
{
    [Verb("decrypt", HelpText = "Decrypt files")]
    public class DecryptOptions
        : OptionsBase
    {
        [Value(0, Required = true, HelpText = "List of input files")]
        public IEnumerable<string> Files { get; set; }

        [Option('o', "out-dir", HelpText = "Set decrypted files output directory")]
        public string OutDir { get; set; }
    }

    public class Decrypt
        : CommandBase<DecryptOptions>
    {
        private const int FileBufferSize = 131072;

        public Decrypt(DecryptOptions options) 
            : base(options)
        {
        }

        public override async Task<int> Exec()
        {
            var password = ReadLine.ReadPassword("Enter password: ");
            if (string.IsNullOrWhiteSpace(password))
                return 1;

            foreach (var file in Options.Files)
            {
                var fileInfo = new FileInfo(file);

                using var sourceStream = new FileStream(fileInfo.FullName, FileMode.Open,
                    FileAccess.Read, FileShare.Read, FileBufferSize, useAsync: true);
                var outDir = Options.OutDir ?? fileInfo.Directory.FullName;
                string outName = GetDecryptedFileName(fileInfo.Name);

                using var destinationStream = new FileStream(Path.Combine(outDir, outName), FileMode.CreateNew,
                    FileAccess.Write, FileShare.None, FileBufferSize, useAsync: true);
                using var decryptionService = new DecryptionService(sourceStream, destinationStream, password, null);
                await decryptionService.DecryptAsync().ConfigureAwait(false);
            }

            return 0;
        }

        private string GetDecryptedFileName(string fileName)
        {
            if (fileName == null)
                throw new ArgumentNullException(nameof(fileName));

            // Remove .lcrypt extension
            var lcryptIndex = fileName.LastIndexOf(".lcrypt", StringComparison.OrdinalIgnoreCase);
            if (lcryptIndex >= 0)
                fileName = fileName.Substring(0, lcryptIndex);

            return fileName.Replace("-encrypted", "-decrypted", StringComparison.OrdinalIgnoreCase);
        }
    }
}
