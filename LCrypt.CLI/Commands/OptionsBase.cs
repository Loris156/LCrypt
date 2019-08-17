using CommandLine;
using System;
using System.Collections.Generic;
using System.Text;

namespace LCrypt.CLI.Commands
{
    public class OptionsBase
    {
        [Option('v', "verbose", Default = false, HelpText = "Enables verbose output")]
        public bool Verbose { get; set; }
    }
}
