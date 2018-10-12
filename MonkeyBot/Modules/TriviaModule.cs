﻿using Discord;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using MonkeyBot.Common;
using MonkeyBot.Preconditions;
using MonkeyBot.Services;
using System;
using System.Threading.Tasks;

namespace MonkeyBot.Modules
{
    /// <summary>Module that provides support for trivia game</summary>
    [Group("Trivia")]
    [Name("Trivia")]
    [MinPermissions(AccessLevel.User)]
    [RequireContext(ContextType.Guild)]
    public class TriviaModule : ModuleBase
    {
        private readonly ITriviaService triviaService;
        private readonly CommandManager commandManager;
        private readonly DbService dbService;

        public TriviaModule(IServiceProvider provider)
        {
            triviaService = provider.GetService<ITriviaService>();
            commandManager = provider.GetService<CommandManager>();
            dbService = provider.GetService<DbService>();
        }

        [Command("Start", RunMode = RunMode.Async)]
        [Remarks("Starts a new trivia with the specified amount of questions.")]
        [Example("!trivia start 5")]
        public async Task StartTriviaAsync([Summary("The number of questions to play.")] int questionAmount = 10)
        {
            var success = await triviaService.StartTriviaAsync(questionAmount, Context as SocketCommandContext).ConfigureAwait(false);
            if (!success)
                await ReplyAsync("Trivia could not be started :(");
        }

        [Command("Stop")]
        [Remarks("Stops a running trivia")]
        public async Task StopTriviaAsync()
        {
            if (!(await triviaService?.StopTriviaAsync(new DiscordId(Context.Guild.Id, Context.Channel.Id, null))))
                await ReplyAsync($"No trivia is running! Use {commandManager.GetPrefixAsync(Context.Guild)}trivia start to create a new one.");
        }

        [Command("Skip")]
        [Remarks("Skips the current question")]
        public async Task SkipQuestionAsync()
        {
            if (!(await triviaService?.SkipQuestionAsync(new DiscordId(Context.Guild.Id, Context.Channel.Id, null))))
                await ReplyAsync($"No trivia is running! Use {commandManager.GetPrefixAsync(Context.Guild)}trivia start to create a new one.");
        }

        [Command("Scores")]
        [Remarks("Gets the global scores")]
        [Example("!trivia scores 10")]
        public async Task GetScoresAsync([Summary("The amount of scores to get.")] int amount = 5)
        {
            var globalScores = await triviaService.GetGlobalHighScoresAsync(amount, Context as SocketCommandContext);
            if (globalScores != null)
            {
                var embedBuilder = new EmbedBuilder()
                    .WithColor(new Color(46, 191, 84))
                    .WithTitle("Global scores")
                    .WithDescription(globalScores);
                await ReplyAsync("", embed: embedBuilder.Build());
            }
            else
                await ReplyAsync("No stored scores found!");
        }
    }
}