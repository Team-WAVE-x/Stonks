using System;
using System.Threading.Tasks;
using System.Collections.Generic;

using Discord;
using Discord.Rest;
using Discord.Commands;
using Discord.Addons.Interactive;

using Westwind.Scripting;

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
            if (Context.User.Id == GetSettingInfo().DeveloperID)
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

                    await message.RemoveAllReactionsAsync();
                    await message.ModifyAsync(msg => { msg.Content = "❌ 작업이 취소되었습니다."; msg.Embed = null; });
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

        [Command("실행", RunMode = RunMode.Async)]
        public async Task EvalAsync([Remainder] string code)
        {
            if (code.StartsWith("```cs") && code.EndsWith("```"))
            {
                code = code.Remove(0, 5);
                code = code.Remove(code.Length - 3, 3);

                var script = new CSharpScriptExecution()
                {
                    SaveGeneratedCode = true,
                    CompilerMode = ClassicMode ? ScriptCompilerModes.Classic : ScriptCompilerModes.Roslyn,
                };

                script.AddDefaultReferencesAndNamespaces();

                foreach (var item in AssemblyList)
                {
                    script.AddAssembly(item);
                }

                foreach (var item in Program.NamespaceList)
                {
                    script.AddNamespace(item);
                }

                string result = script.ExecuteCode(code) as string;

                EmbedBuilder builder = new EmbedBuilder();
                builder.WithTitle("🏃 실행");
                builder.AddField("결과", $"```{result ?? "-"}```");
                builder.AddField("오류 메시지", $"```{script.ErrorMessage ?? "-"}```");
                builder.AddField("생성된 코드", $"```{script.GeneratedClassCodeWithLineNumbers}```");
                builder.WithColor(script.Error ? Color.Red : Color.Teal);
                builder.WithFooter(new EmbedFooterBuilder
                {
                    IconUrl = Context.User.GetAvatarUrl(ImageFormat.Png, 128),
                    Text = $"{Context.User.Username}"
                });
                builder.WithTimestamp(DateTimeOffset.Now);

                await Context.Channel.SendMessageAsync(embed: builder.Build());
            }
            else
            {
                await Context.Channel.SendMessageAsync("❌ 실행할 코드는 반드시 코드 블럭으로 감싸야 합니다.");
            }
        }

        [Command("설정", RunMode = RunMode.Async)]
        public async Task EvalSettingAsync()
        {
            EmbedBuilder builder;

            void BuildEmbed()
            {
                builder = new EmbedBuilder();
                builder.WithTitle("⚙️ 실행 설정");
                builder.AddField("1", string.Format("어셈블리 추가 - [{0}]", AssemblyList.Count == 0 ? "비어 있음" : String.Join(", ", AssemblyList)));
                builder.AddField("2", string.Format("네임스페이스 추가 - [{0}]", Program.NamespaceList.Count == 0 ? "비어 있음" : String.Join(", ", Program.NamespaceList)));
                builder.AddField("3", string.Format("Classic 모드 - [{0}]", ClassicMode ? "On" : "Off"));
                builder.AddField("4", $"설정 초기화");
                builder.WithColor(Color.Teal);
                builder.WithFooter(new EmbedFooterBuilder
                {
                    IconUrl = Context.User.GetAvatarUrl(ImageFormat.Png, 128),
                    Text = $"{Context.User.Username}"
                });
                builder.WithTimestamp(DateTimeOffset.Now);
            }

            BuildEmbed();

            var message = await Context.Channel.SendMessageAsync(embed: builder.Build());

            Action assemblyAddAction = async delegate ()
            {
                await Context.Channel.SendMessageAsync("추가할 어셈블리의 이름을 입력해 주세요. (예: System.dll)");

                var response = await NextMessageAsync(true, true, TimeSpan.FromMinutes(1));

                if (response != null)
                {
                    AssemblyList.Add(response.Content);
                    BuildEmbed();

                    await message.ModifyAsync(msg => msg.Embed = builder.Build());
                    await Context.Channel.SendMessageAsync("✅ 어셈블리가 성공적으로 추가되었습니다!");
                }
                else
                {
                    await Context.Channel.SendMessageAsync("❌ 입력 시간을 초과하셨습니다.");
                }
            };

            Action namespaceAddAction = async delegate ()
            {
                await Context.Channel.SendMessageAsync("추가할 네임스페이스의 이름을 입력해 주세요. (예: System)");

                var response = await NextMessageAsync(true, true, TimeSpan.FromMinutes(1));

                if (response != null)
                {
                    Program.NamespaceList.Add(response.Content);
                    BuildEmbed();

                    await message.ModifyAsync(msg => msg.Embed = builder.Build());
                    await Context.Channel.SendMessageAsync("✅ 네임스페이스가 성공적으로 추가되었습니다!");
                }
                else
                {
                    await Context.Channel.SendMessageAsync("❌ 입력 시간을 초과하셨습니다.");
                }
            };

            Action classicModeEnableAction = async delegate ()
            {
                if (ClassicMode)
                {
                    ClassicMode = false;
                    BuildEmbed();

                    await message.ModifyAsync(msg => msg.Embed = builder.Build());
                }
                else
                {
                    ClassicMode = true;
                    BuildEmbed();

                    await message.ModifyAsync(msg => msg.Embed = builder.Build());
                }
            };

            Action resetSettingAction = async delegate ()
            {
                AssemblyList.Clear();
                Program.NamespaceList.Clear();
                ClassicMode = false;
                BuildEmbed();

                await message.ModifyAsync(msg => msg.Embed = builder.Build());
                await Context.Channel.SendMessageAsync("✅ 설정이 초기화되었습니다.");
            };

            CreateReactMessage(
                msg: message,
                emoji: new List<IEmote> { new Emoji("1️⃣"), new Emoji("2️⃣"), new Emoji("3️⃣"), new Emoji("4️⃣") },
                action: new List<Action> { assemblyAddAction, namespaceAddAction, classicModeEnableAction, resetSettingAction },
                timeSpan: TimeSpan.FromMinutes(1),
                userId: Context.Message.Author.Id
            );
        }
    }
}