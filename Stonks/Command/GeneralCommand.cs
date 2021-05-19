using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

using Discord;
using Discord.Rest;
using Discord.Commands;
using Discord.Addons.Interactive;

using static Stonks.CommandHandling;
using static Stonks.Module.ReactMessageModule;

namespace Stonks.Command
{
    public class GeneralCommand : InteractiveBase<SocketCommandContext>
    {
        [Command("핑", RunMode = RunMode.Async)]
        [Summary("서버와의 연결 지연시간을 확인합니다.")]
        public async Task PingAsync()
        {
            await Context.Channel.SendMessageAsync($"🏓 Pong! {Context.Client.Latency}ms");
        }

        [Command("도움", RunMode = RunMode.Async)]
        [Alias("도움말")]
        [Summary("이 메시지를 표시합니다.")]
        public async Task NewHelpAsync()
        {
            //변수 설정
            int page = 0;
            EmbedBuilder[] builders = new EmbedBuilder[2];
            List<CommandInfo> commands = _commands.Commands.ToList();

            //builders 변수 초기화
            for (int i = 0; i < 2; i++)
            {
                builders[i] = new EmbedBuilder();
            }

            //게임 명령어 임베드
            builders[0].WithTitle("🎮 게임 명령어");
            builders[0].WithColor(Color.Red);
            builders[0].WithFooter(new EmbedFooterBuilder
            {
                IconUrl = Context.User.GetAvatarUrl(ImageFormat.Png, 128),
                Text = $"{Context.User.Username}"
            });
            builders[0].WithTimestamp(DateTimeOffset.Now);

            foreach (CommandInfo command in commands)
            {
                if (command.Module.Name == "GameCommand")
                {
                    builders[0].AddField($"/{command.Name}", command.Summary);
                }
            }

            //기본 명령어 임베드
            builders[1].WithTitle("📄 기본 명령어");
            builders[1].WithColor(Color.Orange);
            builders[1].WithFooter(new EmbedFooterBuilder
            {
                IconUrl = Context.User.GetAvatarUrl(ImageFormat.Png, 128),
                Text = $"{Context.User.Username}"
            });
            builders[1].WithTimestamp(DateTimeOffset.Now);

            foreach (CommandInfo command in commands)
            {
                if (command.Module.Name == "GeneralCommand")
                {
                    builders[1].AddField($"/{command.Name}", command.Summary);
                }
            }

            //전송
            RestUserMessage message = await Context.Channel.SendMessageAsync(embed: builders[0].Build());

            //델리게이트
            Action BackAction = async delegate ()
            {
                page--;

                if (page == -1)
                    page = 0;

                await message.ModifyAsync(msg => msg.Embed = builders[page].Build());
            };

            Action ForwardAction = async delegate ()
            {
                page++;

                if (page == 2)
                    page = 1;

                await message.ModifyAsync(msg => msg.Embed = builders[page].Build());
            };

            CreateReactMessage(
                msg: message,
                emoji: new List<IEmote> { new Emoji("⬅️"), new Emoji("➡️") },
                action: new List<Action> { BackAction, ForwardAction },
                timeSpan: TimeSpan.FromMinutes(1),
                userId: Context.Message.Author.Id
            );
        }
    }
}