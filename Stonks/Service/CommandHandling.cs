using System;
using System.Reflection;
using System.Threading.Tasks;

using Discord;
using Discord.Commands;
using Discord.WebSocket;

using Microsoft.Extensions.DependencyInjection;

using static Stonks.Program;
using static Stonks.Module.SettingModule;

namespace Stonks
{
    internal class CommandHandling
    {
        public static CommandService _commands;
        private readonly DiscordSocketClient _discord;
        private readonly IServiceProvider _services;

        public CommandHandling(IServiceProvider services)
        {
            _commands = services.GetRequiredService<CommandService>();
            _discord = services.GetRequiredService<DiscordSocketClient>();
            _services = services;
            _commands.CommandExecuted += CommandExecutedAsync;
            _discord.MessageReceived += MessageReceivedAsync;
        }

        public async Task InitializeAsync()
        {
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
        }

        public async Task MessageReceivedAsync(SocketMessage rawMessage)
        {
            if (!(rawMessage is SocketUserMessage message)) return;
            if (message.Source != MessageSource.User) return;
            if (rawMessage.Channel is IPrivateChannel) return;
            if (GamingUserList.Contains(rawMessage.Author.Id)) return;

            var argPos = 0;
            if (!message.HasStringPrefix("/", ref argPos)) return;

            var context = new SocketCommandContext(_discord, message);
            await _commands.ExecuteAsync(context, argPos, _services);
        }

        public async Task CommandExecutedAsync(Optional<CommandInfo> command, ICommandContext context, IResult result)
        {
            if (!command.IsSpecified)
                return;
            if (result.IsSuccess)
                return;

            EmbedBuilder builder = new EmbedBuilder();

            builder.WithTitle("Stonks 오류 리포트");
            builder.WithDescription("Stonks 봇에서 심각한 오류가 발생하였습니다.");
            builder.WithColor(Color.Red);
            builder.AddField("발생 시각", DateTime.Now.ToString("G"), false);
            builder.AddField("오류 내용", result, false);
            builder.AddField("오류 이유", result.ErrorReason, false);
            builder.WithFooter(new EmbedFooterBuilder
            {
                IconUrl = client.CurrentUser.GetAvatarUrl(),
                Text = client.CurrentUser.Username
            });
            builder.WithTimestamp(DateTimeOffset.Now);

            await context.Client.GetUserAsync(GetSettingInfo().DeveloperID).Result.SendMessageAsync(embed: builder.Build());
        }
    }
}