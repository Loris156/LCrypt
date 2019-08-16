using CommandLine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LCrypt.CLI.Verbs
{
    [Verb("encrypt", HelpText = "Encrypt files")]
    public class EncryptOptions
        : OptionsBase
    {
        [Value(0, Required = true, HelpText = "List of input files")]
        public IEnumerable<string> Files { get; set; }

        [Option('a', "algorithm")]
        public string Algorithm { get; set; }

        [Option('o', "out-dir")]
        public string OutDir { get; set; }

    }

    public class Encrypt
        : CommandBase<EncryptOptions>
    {
        public Encrypt(EncryptOptions options)
            : base(options)
        {
        }

        public override Task<int> Exec()
        {
            throw new NotImplementedException();
        }
    }
}
