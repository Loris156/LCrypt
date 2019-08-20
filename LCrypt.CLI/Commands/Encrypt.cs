using CommandLine;
using LCrypt.Core.Crypto;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace LCrypt.CLI.Commands
{
    [Verb("encrypt", HelpText = "Encrypt files")]
    public class EncryptOptions
        : OptionsBase
    {
        [Value(0, Required = true, HelpText = "List of input files")]
        public IEnumerable<string> Files { get; set; }

        [Option('a', "algorithm", Default = "aes")]
        public string Algorithm { get; set; }

        [Option('o', "out-dir")]
        public string OutDir { get; set; }
    }

    public class Encrypt
        : CommandBase<EncryptOptions>
    {
        private const int FileBufferSize = 131072;

        public Encrypt(EncryptOptions options)
            : base(options)
        {
        }

        public override async Task<int> Exec()
        {
            var algorithm = Algorithm.GetByName(Options.Algorithm);

            foreach (var file in Options.Files)
            {
                var fileInfo = new FileInfo(file);

                using(var sourceStream = new FileStream(fileInfo.FullName, FileMode.Open,
                    FileAccess.Read, FileShare.Read, FileBufferSize, useAsync: true))
                {
                    var outDir = Options.OutDir ?? fileInfo.Directory.FullName;
                    var outName = Path.GetFileNameWithoutExtension(fileInfo.Name) + "-encrypted" + fileInfo.Extension + ".lcrypt";

                    Directory.CreateDirectory(outDir);

                    using(var destinationStream = new FileStream(Path.Combine(outDir, outName), 
                        FileMode.Create, FileAccess.Write, FileShare.None, FileBufferSize, useAsync: true))
                    {
                        using (var encryptionService = new EncryptionService(algorithm, sourceStream, destinationStream, "123", null))
                        {
                            await encryptionService.EncryptAsync().ConfigureAwait(false);
                        }
                    }
                }
            }

            return 0;
        }
    }
}
