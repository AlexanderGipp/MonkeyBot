﻿using DSharpPlus;
using MonkeyBot.Database;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace MonkeyBot.Services
{
    public class TriviaService : ITriviaService
    {
        private readonly DiscordClient _discordClient;
        private readonly MonkeyDBContext _dbContext;
        private readonly IHttpClientFactory _clientFactory;

        // holds all trivia instances on a per guild and channel basis
        private readonly ConcurrentDictionary<(ulong guildId, ulong channelId), OTDBTriviaInstance> trivias;

        public TriviaService(DiscordClient discordClient, MonkeyDBContext dbContext, IHttpClientFactory clientFactory)
        {
            _discordClient = discordClient;
            _dbContext = dbContext;
            _clientFactory = clientFactory;
            trivias = new ConcurrentDictionary<(ulong guildId, ulong channelId), OTDBTriviaInstance>();
        }

        public async Task<bool> StartTriviaAsync(ulong guildId, ulong channelId, int questionsToPlay)
        {
            if (!trivias.ContainsKey((guildId, channelId)))
            {
                trivias.TryAdd((guildId, channelId), new OTDBTriviaInstance(guildId, channelId, _discordClient, _dbContext, _clientFactory));
            }
            return trivias.TryGetValue((guildId, channelId), out OTDBTriviaInstance instance)
                   && await instance.StartTriviaAsync(questionsToPlay);
        }

        public async Task<bool> StopTriviaAsync(ulong guildId, ulong channelId)
        {
            if (trivias.TryGetValue((guildId, channelId), out OTDBTriviaInstance triviaInstance))
            {
                await triviaInstance.EndTriviaAsync();
                trivias.TryRemove((guildId, channelId), out OTDBTriviaInstance trivia);
                trivia.Dispose();
                return true;
            }
            return false;
        }

        public Task<IEnumerable<(ulong userId, int score)>> GetGlobalHighScoresAsync(ulong guildId, int amount)
        {
            throw new NotImplementedException();
            //TODO: Implement
        }
    }
}
