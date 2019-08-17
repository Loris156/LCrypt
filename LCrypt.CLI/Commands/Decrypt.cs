using CommandLine;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;

namespace LCrypt.CLI.Commands
{
    public class DecryptOptions
        : OptionsBase
    {
        [Value(0, Required = true, HelpText = "List of input files")]
        public IEnumerable<string> Files { get; set; }

        [Option('o', "out-dir")]
        public string OutDir { get; set; }
    }

    public class Decrypt
        : CommandBase<DecryptOptions>
    {
        public Decrypt(DecryptOptions options) 
            : base(options)
        {
        }

        public override async Task<int> Exec()
        {
            return 0;
        }
    }
}
