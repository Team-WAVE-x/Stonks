using System;
using System.Threading;
using System.Reflection;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Runtime.Caching;
using System.Collections.Generic;

using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Discord.Addons.Interactive;

using Microsoft.Extensions.DependencyInjection;

using Stonks.Class;
using static Stonks.Module.SettingModule;

namespace Stonks
{
    internal class Program
    {
        public static CommandService commands;
        public static IServiceProvider services;
        public static DiscordSocketClient client;

        public static Stopwatch uptimeStopwatch = new Stopwatch();

        public static bool ClassicMode = false;
        public static List<string> AssemblyList = new List<string>();
        public static List<string> NamespaceList = new List<string>();

        public static List<ulong> GamingUserList = new List<ulong>();

        public static List<DateTimeOffset> stackCooldownTimer = new List<DateTimeOffset>();
        public static List<SocketGuildUser> stackCooldownTarget = new List<SocketGuildUser>();

        private static void Main(string[] args)
            => new Program().MainAsync().GetAwaiter().GetResult();

        public async Task MainAsync()
        {
            client = new DiscordSocketClient(new DiscordSocketConfig()
            {
                LogLevel = LogSeverity.Verbose
            });

            commands = new CommandService(new CommandServiceConfig()
            {
                LogLevel = LogSeverity.Verbose
            });

            client.Log += OnClientLogReceived;
            commands.Log += OnClientLogReceived;
            client.ReactionAdded += OnReactionAdded;
            client.MessageReceived += OnClientMessage;
            commands.CommandExecuted += OnCommandExecuted;

            uptimeStopwatch.Start();

            await client.LoginAsync(TokenType.Bot, GetSettingInfo().Token);
            await client.StartAsync();

            services = new ServiceCollection()
                .AddSingleton(client)
                .AddSingleton<InteractiveService>()
                .BuildServiceProvider();

            await client.SetGameAsync("/도움말 | https://teamwv.ml", null, ActivityType.Playing);
            await commands.AddModulesAsync(Assembly.GetEntryAssembly(), services);
            await Task.Delay(-1);
        }

        private async Task OnReactionAdded(Cacheable<IUserMessage, ulong> cachedMessage, ISocketMessageChannel originChannel, SocketReaction reaction)
        {
            var message = await cachedMessage.GetOrDownloadAsync();
            var cache = MemoryCache.Default;

            if (message != null && reaction.User.IsSpecified && reaction.UserId != client.CurrentUser.Id)
            {
                foreach (KeyValuePair<string, object> items in cache)
                {
                    var reactMessage = items.Value as ReactMessage;

                    if (message.Id.ToString() == items.Key && reactMessage.messageEmote.Contains(reaction.Emote) && reactMessage.messageUserId == reaction.UserId) //반응이 달린 메시지의 아이디가 캐싱되어 있다면
                    {
                        Console.WriteLine("{0} React {1,19} Added React At {2}", DateTime.Now.ToString("HH:mm:ss"), reaction.User.Value, message.Id);

                        for (int i = 0; i < reactMessage.messageEmote.Count; i++)
                        {
                            if (reactMessage.messageEmote[i].Name == reaction.Emote.Name)
                            {
                                reactMessage.messageAction[i]();
                            }
                        }

                        await message.RemoveReactionAsync(reaction.Emote, reaction.UserId);
                    }
                }
            }
        }

        public async Task OnCommandExecuted(Optional<CommandInfo> command, ICommandContext context, IResult result)
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

        private async Task OnClientMessage(SocketMessage rawMessage)
        {
            if (!(rawMessage is SocketUserMessage message)) return;
            if (message.Source != MessageSource.User) return;
            if (rawMessage.Channel is IPrivateChannel) return;
            if (GamingUserList.Contains(rawMessage.Author.Id)) return;

            var argPos = 0;
            if (!message.HasStringPrefix("/", ref argPos)) return;

            var context = new SocketCommandContext(client, message);
            await commands.ExecuteAsync(context, argPos, services);
        }

        private Task OnClientLogReceived(LogMessage message)
        {
            Console.WriteLine(message.ToString());
            return Task.CompletedTask;
        }
    }
}