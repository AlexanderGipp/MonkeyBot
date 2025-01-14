﻿using DSharpPlus;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MonkeyBot.Common;
using MonkeyBot.Database;
using MonkeyBot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MonkeyBot.Services
{
    /// <summary>
    /// A service that handles announcements
    /// </summary>
    public class AnnouncementService : IAnnouncementService
    {
        private readonly MonkeyDBContext _dbContext;
        private readonly DiscordClient _discordClient;
        private readonly ISchedulingService _schedulingService;
        private readonly ILogger<AnnouncementService> _logger;

        public AnnouncementService(MonkeyDBContext dbContext, DiscordClient discordClient, ISchedulingService schedulingService, ILogger<AnnouncementService> logger)
        {
            _dbContext = dbContext;
            _discordClient = discordClient;
            _schedulingService = schedulingService;
            _logger = logger;
        }

        public async Task InitializeAsync()
        {
            await RemovePastJobsAsync();
            await BuildJobsAsync();
        }

        /// <summary>
        /// Add an announcement that should be broadcasted regularly based on the interval defined by the Cron Expression
        /// </summary>
        /// <param name="name">The unique ID of the announcement</param>
        /// <param name="cronExpression">The cron expression that defines the broadcast intervall. See https://github.com/atifaziz/NCrontab/wiki/Crontab-Expression for details</param>
        /// <param name="message">The message to be broadcasted</param>
        /// <param name="guildID">The ID of the Guild where the message will be broadcasted</param>
        /// <param name="channelID">The ID of the Channel where the message will be broadcasted</param>
        public Task AddRecurringAnnouncementAsync(string name, string cronExpression, string message, ulong guildID, ulong channelID)
        {
            if (name.IsEmpty())
            {
                throw new ArgumentException("Please provide an ID");
            }
            // Create the announcement, add it to the list and persist it
            var announcement = new Announcement { Type = AnnouncementType.Recurring, GuildID = guildID, ChannelID = channelID, CronExpression = cronExpression, Name = name, Message = message };
            AddRecurringJob(announcement);
            _dbContext.Announcements.Add(announcement);
            return _dbContext.SaveChangesAsync();
        }

        private void AddRecurringJob(Announcement announcement)
        {
            string id = GetUniqueId(announcement);
            _schedulingService.ScheduleJobRecurring(id, announcement.CronExpression, async () => await AnnounceAsync(announcement.Message, announcement.GuildID, announcement.ChannelID));
        }

        /// <summary>
        /// Add an announcement that should be broadcasted once on the provided Execution Time
        /// </summary>
        /// <param name="name">The unique ID of the announcement</param>
        /// <param name="excecutionTime">The date and time at which the message should be broadcasted. Must be in the future</param>
        /// <param name="message">The message to be broadcasted</param>
        /// <param name="guildID">The ID of the Guild where the message will be broadcasted</param>
        /// <param name="channelID">The ID of the Channel where the message will be broadcasted</param>
        public Task AddSingleAnnouncementAsync(string name, DateTime excecutionTime, string message, ulong guildID, ulong channelID)
        {
            if (name.IsEmpty())
            {
                throw new ArgumentException("Please provide an ID");
            }
            if (excecutionTime < DateTime.Now)
            {
                throw new ArgumentException("The time you provided is in the past!");
            }
            // Create the announcement, add it to the list and persist it
            var announcement = new Announcement { Type = AnnouncementType.Once, GuildID = guildID, ChannelID = channelID, ExecutionTime = excecutionTime, Name = name, Message = message };
            AddSingleJob(announcement);
            _dbContext.Announcements.Add(announcement);
            return _dbContext.SaveChangesAsync();
        }

        private void AddSingleJob(Announcement announcement)
        {
            // The announcment's name must be unique on a per guild basis
            string uniqueName = GetUniqueId(announcement);
            // Add a new RunOnce job with the provided ID to the Scheduling Service
            _schedulingService.ScheduleJobOnce(uniqueName, announcement.ExecutionTime.Value, async () => await Task.WhenAll(
                    AnnounceAsync(announcement.Message, announcement.GuildID, announcement.ChannelID),
                    RemovePastJobsAsync()
                ));
        }

        /// <summary>
        /// Removes the announcement with the provided ID from the list of announcements if it exists
        /// </summary>
        /// <param name="announcementName">The unique ID of the announcement to remove</param>
        /// <param name="guildID">The ID of the guild where the announcement should be removed</param>
        public async Task RemoveAsync(string announcementName, ulong guildID)
        {
            // Try to retrieve the announcement with the provided ID
            Announcement announcement = await GetSpecificAnnouncementAsync(guildID, announcementName);
            if (announcement == null)
            {
                throw new ArgumentException("The announcement with the specified ID does not exist");
            }
            _schedulingService.RemoveJob(GetUniqueId(announcement));
            try
            {
                _dbContext.Announcements.Remove(announcement);
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing Announcement");
            }

        }

        /// <summary>
        /// Returns the next execution time of the announcement with the provided ID
        /// </summary>
        /// <param name="announcementName">The unique ID of the announcement</param>
        /// <param name="guildID">ID of the guild where the announcement is posted</param>
        /// <returns>Next execution time</returns>
        public async Task<DateTime> GetNextOccurenceAsync(string announcementName, ulong guildID)
        {
            // Try to retrieve the announcement with the provided ID
            Announcement announcement = await GetSpecificAnnouncementAsync(guildID, announcementName);
            if (announcement == null)
            {
                throw new ArgumentException("The announcement with the specified ID does not exist");
            }
            return _schedulingService.GetNextRun(GetUniqueId(announcement));
        }

        /// <summary>Cleanup method to remove single announcements that are in the past</summary>
        private async Task RemovePastJobsAsync()
        {
            List<Announcement> announcements = await _dbContext.Announcements
                .AsQueryable()
                .Where(x => x.Type == AnnouncementType.Once && x.ExecutionTime < DateTime.Now)
                .ToListAsync()
                ;
            _dbContext.RemoveRange(announcements);
            await _dbContext.SaveChangesAsync();

        }

        /// <summary>Creates actual jobs from the announcements in the Announcements List to activate them</summary>
        private async Task BuildJobsAsync()
        {
            List<Announcement> announcements = await GetAllAnnouncementsAsync();
            foreach (Announcement announcement in announcements)
            {
                if (announcement.Type == AnnouncementType.Recurring)
                {
                    AddRecurringJob(announcement);
                }
                else if (announcement.Type == AnnouncementType.Once)
                {
                    AddSingleJob(announcement);
                }
            }
        }

        private Task AnnounceAsync(string message, ulong guildID, ulong channelID)
            => MonkeyHelpers.SendChannelMessageAsync(_discordClient, guildID, channelID, message);

        private Task<List<Announcement>> GetAllAnnouncementsAsync()
            => _dbContext.Announcements.AsQueryable().ToListAsync();

        public Task<List<Announcement>> GetAnnouncementsForGuildAsync(ulong guildID)
            => _dbContext.Announcements.AsQueryable().Where(x => x.GuildID == guildID).ToListAsync();

        private Task<Announcement> GetSpecificAnnouncementAsync(ulong guildID, string announcementName)
            => _dbContext.Announcements.AsQueryable().SingleOrDefaultAsync(x => x.GuildID == guildID && x.Name == announcementName);

        // The announcment's name must be unique on a per guild basis
        private static string GetUniqueId(Announcement announcement) => $"{announcement.Name}-{announcement.GuildID}";
    }
}