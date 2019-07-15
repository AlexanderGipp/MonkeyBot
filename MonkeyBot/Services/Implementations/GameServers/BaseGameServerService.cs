﻿using Discord.WebSocket;
using FluentScheduler;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MonkeyBot.Database;
using MonkeyBot.Models;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace MonkeyBot.Services
{
    public abstract class BaseGameServerService : IGameServerService
    {
        private readonly GameServerType gameServerType;
        private readonly MonkeyDBContext dbContext;
        private readonly DiscordSocketClient discordClient;
        private readonly ILogger<IGameServerService> logger;

        protected BaseGameServerService(GameServerType gameServerType, MonkeyDBContext dbContext, DiscordSocketClient discordClient, ILogger<IGameServerService> logger)
        {
            this.gameServerType = gameServerType;
            this.dbContext = dbContext;
            this.discordClient = discordClient;
            this.logger = logger;
        }

        public void Initialize()
        {
            JobManager.AddJob(async () => await PostAllServerInfoAsync().ConfigureAwait(false), (x) => x.ToRunNow().AndEvery(1).Minutes());
        }

        public async Task<bool> AddServerAsync(IPEndPoint endpoint, ulong guildID, ulong channelID)
        {
            var server = new GameServer { GameServerType = gameServerType, ServerIP = endpoint, GuildID = guildID, ChannelID = channelID };
            bool success = await PostServerInfoAsync(server).ConfigureAwait(false);
            if (success)
            {
                dbContext.Add(server);
                await dbContext.SaveChangesAsync().ConfigureAwait(false);
            }
            return success;
        }

        protected abstract Task<bool> PostServerInfoAsync(GameServer discordGameServer);

        private async Task PostAllServerInfoAsync()
        {
            var servers = await dbContext.GameServers.Where(x => x.GameServerType == gameServerType).ToListAsync().ConfigureAwait(false);
            foreach (var server in servers)
            {
                try
                {
                    await PostServerInfoAsync(server).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Error posting server infos");
                }
            }
        }

        public async Task RemoveServerAsync(IPEndPoint endPoint, ulong guildID)
        {
            var serverToRemove = await dbContext.GameServers.FirstOrDefaultAsync(x => x.ServerIP.Address.ToString() == endPoint.Address.ToString() && x.ServerIP.Port == endPoint.Port && x.GuildID == guildID).ConfigureAwait(false);
            if (serverToRemove == null)
                throw new ArgumentException("The specified server does not exist");
            if (serverToRemove.MessageID != null)
            {
                try
                {
                    var guild = discordClient.GetGuild(serverToRemove.GuildID);
                    var channel = guild?.GetTextChannel(serverToRemove.ChannelID);
                    if (await (channel?.GetMessageAsync(serverToRemove.MessageID.Value)).ConfigureAwait(false) is Discord.Rest.RestUserMessage msg)
                    {
                        await msg.DeleteAsync().ConfigureAwait(false);
                    }
                }
                catch (Exception e)
                {
                    logger.LogError(e, $"Error trying to remove message for game server {endPoint.Address}");
                }
            }
            dbContext.GameServers.Remove(serverToRemove);
            await dbContext.SaveChangesAsync().ConfigureAwait(false);
        }
    }
}