﻿using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;
using MonkeyBot.Common;
using MonkeyBot.Models;
using MonkeyBot.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MonkeyBot.Modules
{
    /// <summary>Module that provides support for announcements</summary>
    [Group("Announcements")]
    [Description("Announcements")]
    [MinPermissions(AccessLevel.ServerAdmin)]
    [RequireGuild]
    public class AnnouncementsModule : BaseCommandModule
    {
        private readonly IAnnouncementService _announcementService;
        private readonly ILogger<AnnouncementsModule> _logger;

        public AnnouncementsModule(IAnnouncementService announcementService, ILogger<AnnouncementsModule> logger)
        {
            _announcementService = announcementService;
            _logger = logger;
        }

        [Command("AddRecurring")]
        [Description("Adds the specified recurring announcement to the specified channel")]
        [Example("announcements addrecurring \"weeklyMsg1\" \"0 19 * * 5\" \"It is Friday 19:00\" \"general\"")]
        public async Task AddRecurringAsync(CommandContext ctx, [Description("The id of the announcement.")] string announcementId, [Description("The cron expression to use.")] string cronExpression, [Description("The message to announce.")] string announcement, [Description("Optional: The channel where the announcement should be posted")] DiscordChannel channel = null)
        {            
            if (announcementId.IsEmpty())
            {
                await ctx.ErrorAsync("You need to specify an ID for the Announcement!");
                return;
            }

            if (cronExpression.IsEmpty())
            {
                await ctx.ErrorAsync("You need to specify a Cron expression that sets the interval for the Announcement!");
                return;
            }

            if (announcement.IsEmpty())
            {
                await ctx.ErrorAsync("You need to specify a message to announce!");
                return;
            }

            // ID must be unique per guild -> check if it already exists
            List<Announcement> announcements = await _announcementService.GetAnnouncementsForGuildAsync(ctx.Guild.Id);
            if (announcements != null && announcements.Any(x => x.Name == announcementId))
            {
                await ctx.ErrorAsync("The ID is already in use");
                return;
            }

            channel ??= ctx.Channel ?? ctx.Guild.GetDefaultChannel();

            try
            {
                // Add the announcement to the Service to activate it
                await _announcementService.AddRecurringAnnouncementAsync(announcementId, cronExpression, announcement, ctx.Guild.Id, channel.Id);
                DateTime nextRun = await _announcementService.GetNextOccurenceAsync(announcementId, ctx.Guild.Id);
                await ctx.OkAsync($"The announcement has been added. The next run is on {nextRun}");
            }
            catch (ArgumentException ex)
            {
                await ctx.ErrorAsync(ex.Message);
                _logger.LogWarning(ex, "Wrong argument while adding a recurring announcement");
            }
        }

        [Command("AddSingle")]
        [Description("Adds the specified single announcement at the given time to the specified channel")]
        [Example("announcements addsingle \"reminder1\" \"19:00\" \"It is 19:00\" \"general\"")]
        public async Task AddSingleAsync(CommandContext ctx, [Description("The id of the announcement.")] string announcementId, [Description("The time when the message should be announced.")] string time, [Description("The message to announce.")] string announcement, [Description("Optional: The channel where the announcement should be posted")] DiscordChannel channel = null)
        {            
            // Do parameter checks
            if (announcementId.IsEmpty())
            {
                await ctx.ErrorAsync("You need to specify an ID for the Announcement!");
                return;
            }
            if (time.IsEmpty() || !DateTime.TryParse(time, out DateTime parsedTime) || parsedTime < DateTime.Now)
            {
                await ctx.ErrorAsync("You need to specify a date and time for the Announcement that lies in the future!");
                return;
            }
            if (announcement.IsEmpty())
            {
                await ctx.ErrorAsync("You need to specify a message to announce!");
                return;
            }
            // ID must be unique per guild -> check if it already exists
            List<Announcement> announcements = await _announcementService.GetAnnouncementsForGuildAsync(ctx.Guild.Id);
            if (announcements.Any(x => x.Name == announcementId))
            {
                await ctx.ErrorAsync("The ID is already in use");
                return;
            }
            channel ??= ctx.Channel ?? ctx.Guild.GetDefaultChannel();
            try
            {
                // Add the announcement to the Service to activate it
                await _announcementService.AddSingleAnnouncementAsync(announcementId, parsedTime, announcement, ctx.Guild.Id, channel.Id);
                DateTime nextRun = await _announcementService.GetNextOccurenceAsync(announcementId, ctx.Guild.Id);
                await ctx.OkAsync($"The announcement has been added. It will be broadcasted on {nextRun}");
            }
            catch (ArgumentException ex)
            {
                await ctx.ErrorAsync(ex.Message);
                _logger.LogWarning(ex, "Wrong argument while adding a single announcement");
            }
        }

        [Command("List")]
        [Description("Lists all upcoming announcements")]
        public async Task ListAsync(CommandContext ctx)
        {
            List<Announcement> announcements = await _announcementService.GetAnnouncementsForGuildAsync(ctx.Guild.Id);
            string message = announcements.Count == 0 ? "No upcoming announcements" : "The following upcoming announcements exist:";
            var builder = new System.Text.StringBuilder();
            builder.Append(message);
            foreach (Announcement announcement in announcements)
            {
                DateTime nextRun = await _announcementService.GetNextOccurenceAsync(announcement.Name, ctx.Guild.Id);
                DiscordChannel channel = ctx.Guild.GetChannel(announcement.ChannelID);
                if (announcement.Type == AnnouncementType.Recurring)
                {
                    builder.AppendLine($"Recurring announcement with ID: \"{announcement.Name}\" will run next at {nextRun} in channel {channel?.Name} with message: \"{announcement.Message}\"");
                }
                else if (announcement.Type == AnnouncementType.Once)
                {
                    builder.AppendLine($"Single announcement with ID: \"{announcement.Name}\" will run once at {nextRun} in channel {channel?.Name} with message: \"{announcement.Message}\"");
                }
            }
            message = builder.ToString();
            await ctx.RespondDeletableAsync(message);            
        }

        [Command("Remove")]
        [Description("Removes the announcement with the specified ID")]
        [Example("announcements remove announcement1")]
        public async Task RemoveAsync(CommandContext ctx, [Description("The id of the announcement.")] [RemainingText] string id)
        {
            string cleanID = id.Trim('\"'); // Because the id is flagged with remainder we need to strip leading and trailing " if entered by the user
            if (cleanID.IsEmpty())
            {
                await ctx.ErrorAsync("You need to specify the ID of the Announcement you wish to remove!");
                return;
            }
            try
            {
                await _announcementService.RemoveAsync(cleanID, ctx.Guild.Id);
                await ctx.OkAsync("The announcement has been removed!");
            }
            catch (Exception ex)
            {
                await ctx.ErrorAsync(ex.Message);
            }
        }

        [Command("NextRun")]
        [Description("Gets the next execution time of the announcement with the specified ID.")]
        [Example("announcements nextrun announcement1")]
        public async Task NextRunAsync(CommandContext ctx, [RemainingText, Description("The id of the announcement.")] string id)
        {
            string cleanID = id.Trim('\"'); // Because the id is flagged with remainder we need to strip leading and trailing " if entered by the user
            if (cleanID.IsEmpty())
            {
                await ctx.ErrorAsync("You need to specify an ID for the Announcement!");
                return;
            }
            try
            {
                List<Announcement> announcements = await _announcementService.GetAnnouncementsForGuildAsync(ctx.Guild.Id);
                Announcement announcement = announcements?.SingleOrDefault(announcement => announcement.Name == cleanID);

                if (announcement == null)
                {
                    await ctx.ErrorAsync("The specified announcement does not exist");
                    return;
                }

                DateTime nextRun = await _announcementService.GetNextOccurenceAsync(cleanID, ctx.Guild.Id);
                await ctx.RespondAsync(nextRun.ToString());

                DiscordChannel channel = ctx.Guild.GetChannel(announcement.ChannelID);

                if (announcement.Type == AnnouncementType.Recurring)
                {
                    await ctx.RespondAsync($"Recurring announcement with ID: \"{announcement.Name}\" will run next at {nextRun} in channel {channel?.Name} with message: \"{announcement.Message}\"");
                }
                else if (announcement.Type == AnnouncementType.Once)
                {
                    await ctx.RespondAsync($"Single announcement with ID: \"{announcement.Name}\" will run once at {nextRun} in channel {channel?.Name} with message: \"{announcement.Message}\"");
                }
            }
            catch (Exception ex)
            {
                await ctx.ErrorAsync(ex.Message);
            }
        }
    }
}