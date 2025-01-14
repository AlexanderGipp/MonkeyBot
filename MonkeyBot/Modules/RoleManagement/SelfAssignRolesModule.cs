﻿using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using MonkeyBot.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MonkeyBot.Modules
{
    //TODO: Consolidate with role buttons and make assignable roles configurable

    /// <summary>Module that handles role assignments</summary>    
    [Description("Self role management")]
    [MinPermissions(AccessLevel.User)]
    [RequireGuild]
    public class SelfAssignRolesModule : BaseCommandModule
    {
        [Command("GiveRole")]
        [Aliases(new[] { "GrantRole", "AddRole" })]
        [Description("Adds the specified role to your own roles.")]
        [Example("giverole @bf")]
        public async Task AddRoleAsync(CommandContext ctx, [Description("The role you want to have")] DiscordRole role)
        {
            if (role == null)
            {
                await ctx.ErrorAsync("Invalid role");
                return;
            }
            DiscordRole botRole = await GetBotRoleAsync(ctx.Client.CurrentUser, ctx.Guild);

            // The bot's role must be higher than the role to be able to assign it
            if (botRole == null || botRole?.Position <= role.Position)
            {
                await ctx.ErrorAsync("Sorry, I don't have sufficient permissions to give you this role!");
                return;
            }

            if (ctx.Member.Roles.Contains(role))
            {
                await ctx.ErrorAsync("You already have that role");
                return;
            }
            await ctx.Member.GrantRoleAsync(role);
            await ctx.OkAsync($"Role {role.Name} has been added");
        }

        [Command("RemoveRole")]
        [Aliases(new[] { "RevokeRole" })]
        [Description("Removes the specified role from your roles.")]
        [Example("RemoveRole @bf")]
        public async Task RemoveRoleAsync(CommandContext ctx, [Description("The role you want to get rid of")] DiscordRole role)
        {
            if (!ctx.Member.Roles.Contains(role))
            {
                await ctx.ErrorAsync("You don't have that role");
            }
            DiscordRole botRole = await GetBotRoleAsync(ctx.Client.CurrentUser, ctx.Guild);
            // The bot's role must be higher than the role to be able to remove it
            if (botRole == null || botRole?.Position <= role.Position)
            {
                await ctx.ErrorAsync("Sorry, I don't have sufficient permissions to take this role from you!");
            }
            await ctx.Member.RevokeRoleAsync(role);
            await ctx.OkAsync($"Role {role.Name} has been revoked");
        }

        [Command("ListRoles")]
        [Description("Lists all roles that can be mentioned and assigned.")]
        public async Task ListRolesAsync(CommandContext ctx)
        {
            DiscordRole botRole = await GetBotRoleAsync(ctx.Client.CurrentUser, ctx.Guild);
            IEnumerable<DiscordRole> assignableRoles = GetAssignableRoles(botRole, ctx.Guild);
            if (assignableRoles.Any())
            {
                string roles = string.Join(", ", assignableRoles.OrderBy(r => r.Name).Select(r => r.Name));
                await ctx.OkAsync(roles, "The following assignable roles exist");
            }
            else
            {
                await ctx.ErrorAsync("No assignable roles exist!");
            }
        }

        [Command("ListRolesWithMembers")]
        [Description("Lists all roles and the users who have these roles")]
        public async Task ListMembersAsync(CommandContext ctx)
        {
            var builder = new DiscordEmbedBuilder()
                .WithColor(new DiscordColor(114, 137, 218))
                .WithDescription("These are the are all the assignable roles and the users assigned to them:");

            DiscordRole botRole = await GetBotRoleAsync(ctx.Client.CurrentUser, ctx.Guild);
            IEnumerable<DiscordRole> assignableRoles = GetAssignableRoles(botRole, ctx.Guild);
            IReadOnlyCollection<DiscordMember> guildMembers = await ctx.Guild.GetAllMembersAsync();
            foreach (DiscordRole role in assignableRoles)
            {
                IOrderedEnumerable<string> roleUsers = guildMembers?.Where(m => m.Roles.Contains(role))
                                                                    .Select(x => x.Username)
                                                                    .OrderBy(x => x);
                _ = roleUsers != null && roleUsers.Any()
                    ? builder.AddField(role.Name, string.Join(", ", roleUsers), false)
                    : builder.AddField(role.Name, "-", false);
            }
            await ctx.RespondDeletableAsync(builder.Build());
        }

        [Command("ListRoleMembers")]
        [Description("Lists all the members of the specified role")]
        [Example("ListRoleMembers @bf")]
        public async Task ListMembersAsync(CommandContext ctx, [Description("The role to display members for")] DiscordRole role)
        {
            IReadOnlyCollection<DiscordMember> guildMembers = await ctx.Guild.GetAllMembersAsync();
            IOrderedEnumerable<string> roleUsers = guildMembers?.Where(x => x.Roles.Contains(role))
                                                              .Select(x => x.Username)
                                                              .OrderBy(x => x);
            if (roleUsers == null || !roleUsers.Any())
            {
                await ctx.ErrorAsync("This role does not have any members!");
                return;
            }
            var builder = new DiscordEmbedBuilder()
                .WithColor(new DiscordColor(114, 137, 218))
                .WithTitle($"These are the users assigned to the {role.Name} role:")
                .WithDescription(string.Join(", ", roleUsers));

            await ctx.RespondDeletableAsync(builder.Build());
        }

        private static async Task<DiscordRole> GetBotRoleAsync(DiscordUser botUser, DiscordGuild guild)
        {
            DiscordMember bot = await guild.GetMemberAsync(botUser.Id);
            return bot.Roles.FirstOrDefault(x => x.Permissions.HasPermission(Permissions.ManageRoles));
        }

        private static IEnumerable<DiscordRole> GetAssignableRoles(DiscordRole botRole, DiscordGuild guild)
        {
            // Get all roles that are lower than the bot's role (roles the bot can assign)
            return guild.Roles.Values
                .Where(role => role.IsMentionable
                               && role != guild.EveryoneRole
                               && !role.Permissions.HasFlag(Permissions.Administrator)
                               && role.Position < botRole.Position);

        }
    }
}