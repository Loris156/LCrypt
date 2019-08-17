using CommandLine;
using LCrypt.CLI.Commands;
using System;
using System.Threading.Tasks;

namespace LCrypt.CLI
{
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            return await Parser.Default
                .ParseArguments<EncryptOptions, DecryptOptions>(args)
                .MapResult(
                (EncryptOptions o) => new Encrypt(o).Exec(),
                (DecryptOptions o) => new Decrypt(o).Exec(),
                (err) => { return Task.FromResult(1); })
                .ConfigureAwait(false);
        }
    }
}
