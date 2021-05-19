using System;
using System.Threading.Tasks;
using System.Collections.Generic;

using Discord;
using Discord.Rest;
using Discord.Commands;
using Discord.Addons.Interactive;

using static Stonks.Program;
using static Stonks.Module.SettingModule;
using static Stonks.Module.ReactMessageModule;

namespace Stonks.Command
{
    public class AdminCommand : InteractiveBase<SocketCommandContext>
    {
        [Command("재시작", RunMode = RunMode.Async)]
        public async Task RestartAsync()
        {
            if (Context.User.Id.ToString() == GetSettingInfo().DeveloperID)
            {
                EmbedBuilder builder = new EmbedBuilder();
                builder.WithTitle("🔄 재시작");
                builder.WithDescription("정말 재시작 하시겠습니까?");
                builder.WithColor(Color.Red);
                builder.WithFooter(new EmbedFooterBuilder
                {
                    IconUrl = Context.User.GetAvatarUrl(ImageFormat.Png, 128),
                    Text = $"{Context.User.Username}"
                });
                builder.WithTimestamp(DateTimeOffset.Now);

                RestUserMessage message = await Context.Channel.SendMessageAsync(embed: builder.Build());

                Action OkAction = async delegate ()
                {
                    builder.WithTitle("🔄 재시작");
                    builder.WithDescription("봇이 재시작 중입니다. 이 작업은 시간이 걸릴 수 있습니다...");
                    builder.WithColor(Color.Green);
                    builder.WithFooter(new EmbedFooterBuilder
                    {
                        IconUrl = Context.User.GetAvatarUrl(ImageFormat.Png, 128),
                        Text = $"{Context.User.Username}"
                    });
                    builder.WithTimestamp(DateTimeOffset.Now);

                    await message.ModifyAsync(msg => msg.Embed = builder.Build());

                    System.Diagnostics.Process.Start(System.AppDomain.CurrentDomain.BaseDirectory + System.AppDomain.CurrentDomain.FriendlyName);
                    Environment.Exit(0);
                };

                Action CancelAction = async delegate ()
                {
                    RemoveReactMessage(message.Id);

                    await message.ModifyAsync(msg => { msg.Content = "❌ 작업이 취소되었습니다."; msg.Embed = null; });
                    await message.Channel.SendMessageAsync("❌ 작업이 취소되었습니다.");
                };

                CreateReactMessage(
                    msg: message,
                    emoji: new List<IEmote> { new Emoji("✅"), new Emoji("❎") },
                    action: new List<Action> { OkAction, CancelAction },
                    timeSpan: TimeSpan.FromMinutes(1),
                    userId: Context.Message.Author.Id
                );
            }
            else
            {
                await Context.Channel.SendMessageAsync("❌ 개발자만 사용할 수 있는 명령어입니다.");
            }
        }

        [Command("업타임", RunMode = RunMode.Async)]
        public async Task UptimeAsync()
        {
            EmbedBuilder builder = new EmbedBuilder();
            TimeSpan uptime = TimeSpan.FromMilliseconds(uptimeStopwatch.ElapsedMilliseconds);

            builder.WithTitle("🕒 업타임");
            builder.WithDescription($"{uptime.Days} 일 {uptime.Hours} 시간 {uptime.Minutes} 분 {uptime.Seconds} 초");
            builder.WithColor(Color.Teal);
            builder.WithFooter(new EmbedFooterBuilder
            {
                IconUrl = Context.User.GetAvatarUrl(ImageFormat.Png, 128),
                Text = $"{Context.User.Username}"
            });
            builder.WithTimestamp(DateTimeOffset.Now);

            await Context.Channel.SendMessageAsync(embed: builder.Build());
        }
    }
}