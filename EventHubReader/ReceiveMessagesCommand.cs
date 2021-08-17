using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Consumer;
using Spectre.Console;
using Spectre.Console.Cli;

namespace EventHubReader
{
    public class ReceiveMessagesCommand : AsyncCommand<ReceiveMessagesSettings>
    {
        public override async Task<int> ExecuteAsync(CommandContext context, ReceiveMessagesSettings settings)
        {
            AnsiConsole.Clear();

            InitSettings(settings);

            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("Press [green]SPACE[/] to trigger a message or [orange3]ESC[/] to exit.");
            AnsiConsole.WriteLine();

            await RunCommand(settings);

            Goodbye();

            return 0;
        }

        private static void Goodbye()
        {
            AnsiConsole.WriteLine();
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("   " + Emoji.Known.GrowingHeart + "  [orange3]THANK YOU FOR PARTICIPATING IN THIS ENRICHMENT CENTER ACTIVITY[/]  " +
                                   Emoji.Known.BirthdayCake);
            AnsiConsole.WriteLine();
            AnsiConsole.WriteLine();
        }

        private static void InitSettings(ReceiveMessagesSettings settings)
        {
            AnsiConsole.WriteLine();
            if (string.IsNullOrWhiteSpace(settings.ConnectionString))
                settings.ConnectionString =
                    AnsiConsole.Prompt(
                        new TextPrompt<string>("Enter [green]Event Hub connection string[/]")
                            .PromptStyle("deepskyblue1")
                            .Secret());

            var connectionStringProperties = TryParseConnectionString(settings.ConnectionString);

            var table = new Table();

            table.AddColumn("Parameter");
            table.AddColumn(new TableColumn("Value"));

            table.AddRow("Event Hub namespace", connectionStringProperties.FullyQualifiedNamespace);
            table.AddRow("Event Hub", connectionStringProperties.EventHubName);
            table.AddRow("Endpoint", connectionStringProperties.Endpoint.ToString());

            table.Border(TableBorder.Rounded);

            AnsiConsole.Render(table);
        }

        private async Task MonitorUserInput(CancellationTokenSource cts, IObserver<Unit> manualTrigger)
        {
            while (true)
            {
                while (Console.KeyAvailable)
                {
                    var keyInfo = Console.ReadKey(true);
                    if (keyInfo.Key is ConsoleKey.Escape)
                    {
                        cts.Cancel();
                        manualTrigger.OnCompleted();
                        return;
                    }

                    if (keyInfo.Key is ConsoleKey.Spacebar)
                        manualTrigger.OnNext(Unit.Default);
                }

                await Task.Delay(200);
            }
        }

        private static async Task ReceiveMessages(ReceiveMessagesSettings settings, CancellationToken cancellationToken)
        {
            await using var consumer = new EventHubConsumerClient(settings.ConsumerGroup, settings.ConnectionString);
            await foreach (var receivedEvent in consumer.ReadEventsAsync(cancellationToken))
            {
                var message = Encoding.UTF8.GetString(receivedEvent.Data.EventBody);
                AnsiConsole.WriteLine(message);
            }
        }

        private async Task RunCommand(ReceiveMessagesSettings settings)
        {
            using var cts = new CancellationTokenSource();
            using var manualTriggerSubject = new Subject<Unit>();

            var sendMessagesTask =
                AnsiConsole.Status()
                    .Spinner(Spinner.Known.BouncingBar)
                    .StartAsync("Running...", _ => ReceiveMessages(settings, cts.Token));


            try
            {
                await MonitorUserInput(cts, manualTriggerSubject);
                await sendMessagesTask;
            }
            catch (OperationCanceledException)
            {
                // Ignore
            }
        }

        private static EventHubsConnectionStringProperties TryParseConnectionString(string connectionString)
        {
            try
            {
                return EventHubsConnectionStringProperties.Parse(connectionString);
            }
            catch (Exception e)
            {
                throw new Exception("Failed to parse provided connection string: " + e.Message, e);
            }
        }
    }
}