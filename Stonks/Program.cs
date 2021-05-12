using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Stonks.Class;
using System;
using System.Collections.Generic;
using System.Runtime.Caching;
using System.Threading;
using System.Threading.Tasks;
using static Stonks.Module.SettingModule;

namespace Stonks
{
    internal class Program
    {
        public static List<ulong> gamingUser = new List<ulong>();
        public static List<DateTimeOffset> stackCooldownTimer = new List<DateTimeOffset>();
        public static List<SocketGuildUser> stackCooldownTarget = new List<SocketGuildUser>();
        public static DiscordSocketClient client;

        private static void Main(string[] args)
            => new Program().MainAsync().GetAwaiter().GetResult();

        public async Task MainAsync()
        {
            using (var services = ConfigureServices())
            {
                client = services.GetRequiredService<DiscordSocketClient>();
                client.Log += LogAsync;
                client.ReactionAdded += ReactionAddedAsync;
                services.GetRequiredService<CommandService>().Log += LogAsync;

                await client.LoginAsync(TokenType.Bot, GetSettingInfo().Token);
                await client.StartAsync();
                await client.SetGameAsync("/도움말 | https://teamwv.ml", null, ActivityType.Playing);
                await services.GetRequiredService<CommandHandling>().InitializeAsync();
                await Task.Delay(Timeout.Infinite);
            }
        }

        private async Task ReactionAddedAsync(Cacheable<IUserMessage, ulong> cachedMessage, ISocketMessageChannel originChannel, SocketReaction reaction)
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

        private Task LogAsync(LogMessage log)
        {
            Console.WriteLine(log.ToString());
            return Task.CompletedTask;
        }

        private ServiceProvider ConfigureServices()
        {
            return new ServiceCollection()
                .AddSingleton<DiscordSocketClient>()
                .AddSingleton<CommandService>()
                .AddSingleton<CommandHandling>()
                .AddSingleton<InteractiveService>()
                .BuildServiceProvider();
        }
    }
}