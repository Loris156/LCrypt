using CommandLine;
using LCrypt.CLI.Verbs;
using System;
using System.Threading.Tasks;

namespace LCrypt.CLI
{
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            return await Parser.Default
                .ParseArguments<EncryptOptions>(args)
                .MapResult(
                (EncryptOptions o) => new Encrypt(o).Exec(),
                (err) => { return Task.FromResult(1); })
                .ConfigureAwait(false);
        }
    }
}
