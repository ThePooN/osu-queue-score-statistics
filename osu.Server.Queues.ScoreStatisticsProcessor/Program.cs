﻿using System;
using System.Threading;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using osu.Server.Queues.ScoreStatisticsProcessor.Commands;

namespace osu.Server.Queues.ScoreStatisticsProcessor
{
    [Command]
    [Subcommand(typeof(QueueCommands))]
    [Subcommand(typeof(PerformanceCommands))]
    public class Program
    {
        private static readonly CancellationTokenSource cts = new CancellationTokenSource();

        public static async Task Main(string[] args)
        {
            Console.CancelKeyPress += (_, e) =>
            {
                Console.WriteLine("Cancellation requested!");
                cts.Cancel();

                e.Cancel = true;
            };

            await CommandLineApplication.ExecuteAsync<Program>(args, cts.Token);
        }

        public Task<int> OnExecuteAsync(CommandLineApplication app, CancellationToken cancellationToken)
        {
            app.ShowHelp(false);
            return Task.FromResult(1);
        }
    }
}
