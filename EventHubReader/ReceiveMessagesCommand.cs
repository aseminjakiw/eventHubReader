using System.Threading.Tasks;
using Spectre.Console.Cli;

namespace EventHubReader
{
    public class ReceiveMessagesCommand : AsyncCommand<ReceiveMessagesSettings>
    {
        public override Task<int> ExecuteAsync(CommandContext context, ReceiveMessagesSettings settings)
        {
            throw new System.NotImplementedException();
        }
    }
}