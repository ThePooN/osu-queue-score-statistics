// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using McMaster.Extensions.CommandLineUtils;
using osu.Server.Queues.ScoreStatisticsProcessor;

namespace osu.Server.Queues.ScorePump.Performance.Totals
{
    [Command("all", Description = "Updates the total PP of all users.")]
    public class AllCommand : PerformanceCommand
    {
        [Option(CommandOptionType.SingleValue, Template = "-r|--ruleset", Description = "The ruleset to process score for.")]
        public int RulesetId { get; set; }

        protected override async Task<int> ExecuteAsync(CommandLineApplication app)
        {
            LegacyDatabaseHelper.RulesetDatabaseInfo databaseInfo = LegacyDatabaseHelper.GetRulesetSpecifics(RulesetId);

            int[] userIds;

            using (var db = Queue.GetDatabaseConnection())
                userIds = (await db.QueryAsync<int>($"SELECT `user_id` FROM {databaseInfo.UserStatsTable}")).ToArray();

            Console.WriteLine($"Processed 0 of {userIds.Length}");

            int processedCount = 0;
            await Task.WhenAll(Partitioner
                               .Create(userIds)
                               .GetPartitions(Threads)
                               .AsParallel()
                               .Select(processPartition));

            return 0;

            async Task processPartition(IEnumerator<int> partition)
            {
                using (partition)
                {
                    while (partition.MoveNext())
                    {
                        await Task.Yield();

                        await Processor.UpdateTotalsAsync(partition.Current, RulesetId);

                        Console.WriteLine($"Processed {Interlocked.Increment(ref processedCount)} of {userIds.Length}");
                    }
                }
            }
        }
    }
}
