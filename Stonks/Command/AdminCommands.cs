using Discord.Addons.Interactive;
using Discord.Commands;
using System;
using System.Threading.Tasks;
using static Stonks.Module.SettingModule;

namespace Stonks.Command
{
    public class AdminCommand : InteractiveBase<SocketCommandContext>
    {
        [Command("재시작", RunMode = RunMode.Async)]
        public async Task RestartAsync()
        {
            if (Context.User.Id.ToString() == GetSettingInfo().DeveloperID)
            {
                await Context.Channel.SendMessageAsync("✅ 봇이 재시작중입니다.. 잠시만 기다려 주십시오.");

                try
                {
                    System.Diagnostics.Process.Start(System.AppDomain.CurrentDomain.BaseDirectory + System.AppDomain.CurrentDomain.FriendlyName);
                    Environment.Exit(0);
                }
                catch (Exception)
                {
                    await Context.Channel.SendMessageAsync("❌ 재시작에 실패하였습니다.");
                }
            }
            else
            {
                await Context.Channel.SendMessageAsync("❌ 개발자만 사용할 수 있는 명령어입니다.");
            }
        }
    }
}