using System;
using System.IO;
using System.Linq;
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
        private bool printMessages = true;
        private StatusContext? statusContext;

        public override async Task<int> ExecuteAsync(CommandContext context, ReceiveMessagesSettings settings)
        {
            InitSettings(settings);

            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("Press [green]SPACE[/] to pause / resume printing of messages or [orange3]ESC[/] to exit.");
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
            table.AddRow("consumer group", settings.ConsumerGroup);

            if (settings.Filename != null)
            {
                table.AddRow("Write to file", settings.Filename);
            }

            foreach (var contains in settings.Contains)
            {
                table.AddRow("Message must contain", contains);
            }

            foreach (var notContains in settings.NotContains)
            {
                table.AddRow("Message must NOT contain", notContains);
            }


            table.Border(TableBorder.Rounded);

            AnsiConsole.Render(table);
        }

        private async Task MonitorUserInput(CancellationTokenSource cts)
        {
            while (true)
            {
                while (Console.KeyAvailable)
                {
                    var keyInfo = Console.ReadKey(true);
                    if (keyInfo.Key is ConsoleKey.Escape || keyInfo.Modifiers is ConsoleModifiers.Control && keyInfo.Key is ConsoleKey.C)
                    {
                        cts.Cancel();
                        return;
                    }

                    if (keyInfo.Key is ConsoleKey.Spacebar)
                        ToggleMessagePrint();
                }

                await Task.Delay(200);
            }
        }

        private async Task ReceiveMessages(ReceiveMessagesSettings settings, CancellationToken cancellationToken)
        {
            FileStream? fileStream = null;
            StreamWriter? streamWriter = null;
            try
            {
                if (settings.Filename != null)
                {
                    fileStream = File.Open(settings.Filename, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read);
                    streamWriter = new StreamWriter(fileStream);
                }
            
                await using var consumer = new EventHubConsumerClient(settings.ConsumerGroup, settings.ConnectionString);
                await foreach (var receivedEvent in consumer.ReadEventsAsync(false, null, cancellationToken))
                    try
                    {
                        if (!printMessages)
                            continue;

                        var message = Encoding.UTF8.GetString(receivedEvent.Data.EventBody);
                        if (settings.NotContains
                            .Select(x => message.Contains(x))
                            .Any(x=> x == true))
                        {
                            continue;
                        }

                        if (settings.Contains
                            .Select(x => message.Contains(x))
                            .Any(x=> x == false))
                        {
                            continue;
                        }


                        var timestamp = DateTime.Now;
                        var consoleTimestamp = $"[gray]{timestamp:O}:[/] ";
                        AnsiConsole.MarkupLine(consoleTimestamp);
                        AnsiConsole.WriteLine(message);
                    
                        
                        streamWriter?.WriteLine($"{timestamp:O}: {message}");
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
            }
            finally
            {
                streamWriter?.Dispose();
                fileStream?.Dispose();
            }
        }

        private async Task RunCommand(ReceiveMessagesSettings settings)
        {
            using var cts = new CancellationTokenSource();


            var sendMessagesTask =
                AnsiConsole.Status()
                    .Spinner(Spinner.Known.BouncingBar)
                    .StartAsync("Running...", context =>
                    {
                        statusContext = context;
                        return ReceiveMessages(settings, cts.Token);
                    });


            try
            {
                await MonitorUserInput(cts);
                await sendMessagesTask;
            }
            catch (OperationCanceledException)
            {
                // Ignore
            }
        }

        private void ToggleMessagePrint()
        {
            if (printMessages)
            {
                printMessages = false;
                if (statusContext != null) statusContext.Status = "Paused";
            }
            else
            {
                printMessages = true;
                if (statusContext != null) statusContext.Status = "Running...";
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