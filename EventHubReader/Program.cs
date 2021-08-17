using System;
using System.Text;
using EventHubReader;
using Spectre.Console.Cli;

Console.OutputEncoding = Encoding.Unicode;
Console.InputEncoding = Encoding.Unicode;
var app = new CommandApp<ReceiveMessagesCommand>();
await app.RunAsync(args);