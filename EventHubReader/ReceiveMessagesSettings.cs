using System;
using System.ComponentModel;
using System.Linq;
using Azure.Messaging.EventHubs.Consumer;
using Spectre.Console.Cli;

namespace EventHubReader
{
    public class ReceiveMessagesSettings : CommandSettings
    {
        [Description("Azure Event Hub connection string as presented in the Azure portal. Must contain Event Hub name")]
        [CommandArgument(0, "[consumerGroup]")]
        public string? ConnectionString { get; set; }

        [Description("Consumer group of Event hub to read from.")]
        [CommandOption("-c|--consumer-group")]
        [DefaultValue(typeof(string), EventHubConsumerClient.DefaultConsumerGroupName)]
        public string ConsumerGroup { get; set; }
    }
}