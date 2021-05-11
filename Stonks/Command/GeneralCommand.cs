using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Stonks.CommandHandling;

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
        public async Task HelpAsync()
        {
            List<CommandInfo> commands = _commands.Commands.ToList();
            List<EmbedFieldBuilder> embedFieldBuilders = new List<EmbedFieldBuilder>();

            foreach (CommandInfo command in commands)
            {
                if (!(command.Module.Name == "AdminCommand"))
                {
                    embedFieldBuilders.Add(new EmbedFieldBuilder { Name = $"/{command.Name}", Value = command.Summary ?? "설명이 존재하지 않습니다.\n" });
                }
            }

            PaginatedMessage paginatedMessage = new PaginatedMessage();
            paginatedMessage.Title = "도움말";
            paginatedMessage.Color = Color.Green;
            paginatedMessage.Options.FieldsPerPage = 5;
            paginatedMessage.Options.JumpDisplayOptions = JumpDisplayOptions.Never;
            paginatedMessage.Options.DisplayInformationIcon = false;
            paginatedMessage.Pages = embedFieldBuilders;

            await PagedReplyAsync(paginatedMessage);
        }
    }
}