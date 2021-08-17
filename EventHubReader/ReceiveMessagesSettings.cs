using System;
using System.ComponentModel;
using System.Linq;
using Azure.Messaging.EventHubs.Consumer;
using Spectre.Console.Cli;

namespace EventHubReader
{
    public class ReceiveMessagesSettings : CommandSettings
    {
        [Description("Azure Event Hub connection string as presented in the Azure portal")]
        [CommandArgument(0, "[connectionString]")]
        public string? ConnectionString { get; set; }

        [Description("Consumer group of Event Hub to read from. Default value '" + EventHubConsumerClient.DefaultConsumerGroupName + "'")]
        [CommandOption("-g|--consumer-group")]
        [DefaultValue(typeof(string), EventHubConsumerClient.DefaultConsumerGroupName)]
        public string ConsumerGroup { get; set; } = null!;

        [Description("Only messages which contain this string will be printed. Can be added multiple times")]
        [CommandOption("-c|--contains")]
        public string[] Contains { get; set; } = Array.Empty<string>();
        
        [Description("Messages containing this string will not be printed. Can be added multiple times")]
        [CommandOption("-n|--not-contains")]
        public string[] NotContains { get; set; } = Array.Empty<string>();
    }
}