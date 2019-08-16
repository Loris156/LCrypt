using System.Threading.Tasks;

namespace LCrypt.CLI.Verbs
{
    public abstract class CommandBase<TOptions>
        where TOptions : OptionsBase
    {
        public CommandBase(TOptions options)
        {
            Options = options;
        }

        protected TOptions Options { get; }

        public abstract Task<int> Exec();
    }
}
