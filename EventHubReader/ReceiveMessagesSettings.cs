using System;
using System.ComponentModel;
using System.Linq;
using Azure.Messaging.EventHubs.Consumer;
using Spectre.Console.Cli;

namespace EventHubReader
{
    public class ReceiveMessagesSettings : CommandSettings
    {
        [Description("Azure Event Hub connection string as presented in the Azure portal. Must contain Event Hub name.")]
        [CommandArgument(0, "[consumerGroup]")]
        public string? ConnectionString { get; set; }

        [Description("Consumer group of Event hub to read from.")]
        [CommandOption("-g|--consumer-group")]
        [DefaultValue(typeof(string), EventHubConsumerClient.DefaultConsumerGroupName)]
        public string ConsumerGroup { get; set; } = null!;

        [Description("Only messages which contain all strings will be printed.")]
        [CommandOption("-c|--contains")]
        public string[] Contains { get; set; } = Array.Empty<string>();
        
        [Description("Messages containing any of these strings will NOT be printed.")]
        [CommandOption("-n|--not-contains")]
        public string[] NotContains { get; set; } = Array.Empty<string>();
    }
}