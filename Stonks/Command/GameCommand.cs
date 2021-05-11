using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using static Stonks.Module.GameModule;
using static Stonks.Module.ReactMessageModule;
using static Stonks.Program;

namespace Stonks.Command
{
    public class GameCommand : InteractiveBase<SocketCommandContext>
    {
        [Command("용돈", RunMode = RunMode.Async)]
        [Summary("용돈을 지급합니다.")]
        public async Task CashAsync()
        {
            Class.User user = new Class.User(Context.Guild.Id, Context.User.Id);

            if (stackCooldownTarget.Contains(Context.User as SocketGuildUser))
            {
                if (stackCooldownTimer[stackCooldownTarget.IndexOf(Context.Message.Author as SocketGuildUser)].AddSeconds(15) >= DateTimeOffset.Now)
                {
                    int secondsLeft = (int)(stackCooldownTimer[stackCooldownTarget.IndexOf(Context.Message.Author as SocketGuildUser)].AddSeconds(15) - DateTimeOffset.Now).TotalSeconds;
                    await Context.Channel.SendMessageAsync($"<@{Context.User.Id}>님, 용돈을 다시 받을려면 {secondsLeft}초 기다려야 해요!");
                }
                else
                {
                    Random rd = new Random();
                    int value = rd.Next(100, 1000);
                    user.addMoney((ulong)value);
                    await Context.Channel.SendMessageAsync($"{value} 코인을 받았습니다.");

                    stackCooldownTimer[stackCooldownTarget.IndexOf(Context.Message.Author as SocketGuildUser)] = DateTimeOffset.Now;
                }
            }
            else
            {
                Random rd = new Random();
                int value = rd.Next(100, 1000);
                user.addMoney((ulong)value);
                await Context.Channel.SendMessageAsync($"{value} 코인을 받았습니다.");

                stackCooldownTarget.Add(Context.User as SocketGuildUser);
                stackCooldownTimer.Add(DateTimeOffset.Now);
            }
        }

        [Command("내돈", RunMode = RunMode.Async)]
        [Summary("자신의 돈을 확인합니다.")]
        public async Task MoneyAsync()
        {
            Class.User user = new Class.User(Context.Guild.Id, Context.User.Id);

            EmbedBuilder builder = new EmbedBuilder();
            builder.WithTitle("💰 통장");
            builder.WithDescription($"당신의 통장에는 `{string.Format("{0:n0}", user.Money)}` 코인이 있습니다.");
            builder.WithColor(Color.Blue);
            builder.WithFooter(new EmbedFooterBuilder 
            { 
                IconUrl = Context.User.GetAvatarUrl(ImageFormat.Png, 128),
                Text = Context.User.Username 
            });
            builder.WithTimestamp(DateTimeOffset.Now);

            await Context.Channel.SendMessageAsync(string.Empty, false, builder.Build());
        }

        [Command("랭킹", RunMode = RunMode.Async)]
        [Summary("전체 랭킹을 확인합니다.")]
        public async Task RankingAsync()
        {
            var message = await Context.Channel.SendMessageAsync("🧮 계산중...");

            List<Class.User> users = getRanking(Context.Guild.Id, 20);

            EmbedBuilder builder = new EmbedBuilder();
            builder.WithTitle("🏆 랭킹");
            builder.WithColor(Color.LightOrange);
            builder.WithFooter(new EmbedFooterBuilder 
            { 
                IconUrl = Context.User.GetAvatarUrl(ImageFormat.Png, 128),
                Text = Context.User.Username 
            });
            builder.WithTimestamp(DateTimeOffset.Now);

            if (users.Count == 0)
            {
                builder.WithDescription("❌ 데이터가 존재하지 않습니다.");
            }
            else
            {
                for (int i = 0; i < users.Count; i++)
                {
                    builder.AddField($"{i + 1}등", $"{Context.Client.Rest.GetUserAsync(users[i].UserId).Result.Username}#{Context.Client.Rest.GetUserAsync(users[i].UserId).Result.Discriminator} - {string.Format("{0:n0}", users[i].Money)} 코인");
                }
            }

            await message.ModifyAsync(msg => { msg.Content = string.Empty; msg.Embed = builder.Build(); });
        }

        [Command("끝말잇기 랭킹", RunMode = RunMode.Async)]
        [Summary("끝말잇기 랭킹을 확인합니다.")]
        public async Task RoundRankingAsync()
        {
            var message = await Context.Channel.SendMessageAsync("🧮 계산중...");

            List<Class.User> users = getRoundRanking(Context.Guild.Id, 20);

            EmbedBuilder builder = new EmbedBuilder();
            builder.WithTitle("🏆 끝말잇기 랭킹");
            builder.WithColor(Color.LightOrange);
            builder.WithFooter(new EmbedFooterBuilder 
            { 
                IconUrl = Context.User.GetAvatarUrl(ImageFormat.Png, 128),
                Text = Context.User.Username 
            });
            builder.WithTimestamp(DateTimeOffset.Now);

            if (users.Count == 0)
            {
                builder.WithDescription("❌ 데이터가 존재하지 않습니다.");
            }
            else
            {
                for (int i = 0; i < users.Count; i++)
                {
                    builder.AddField($"{i + 1}등", $"{Context.Client.Rest.GetUserAsync(users[i].UserId).Result.Username}#{Context.Client.Rest.GetUserAsync(users[i].UserId).Result.Discriminator} - {string.Format("{0:n0}", users[i].Round)} 라운드");
                }
            }

            await message.ModifyAsync(msg => { msg.Content = string.Empty; msg.Embed = builder.Build(); });
        }

        [Command("슬롯머신", RunMode = RunMode.Async)]
        [Alias("도박")]
        [Summary("슬롯머신 게임을 시작합니다.")]
        public async Task SlotMachineAsync([Remainder] string money = "")
        {
            Class.User user = new Class.User(Context.Guild.Id, Context.User.Id);

            if (money == "올인")
            {
                money = Convert.ToString(user.Money);
            }

            if (string.IsNullOrWhiteSpace(money))
            {
                await Context.Channel.SendMessageAsync("❌ 배팅할 금액을 입력하여 주세요.");
            }
            else if (!money.All(char.IsDigit))
            {
                await Context.Channel.SendMessageAsync("❌ 배팅 금액은 반드시 소수가 아닌 양수이여야 합니다.");
            }
            else if (money == "0" || Convert.ToUInt64(money) < 0 || (Convert.ToDecimal(money) % 1) > 0)
            {
                await Context.Channel.SendMessageAsync("❌ 배팅 금액은 반드시 1 이상의 정수여야 합니다.");
            }
            else
            {
                if (user.Money < Convert.ToUInt64(money))
                {
                    await Context.Channel.SendMessageAsync("❌ 코인이 부족합니다.");
                }
                else
                {
                    List<string> SlotMachineItems = new List<string>();
                    Random rd = new Random();

                    for (int i = 0; i < 3; i++)
                    {
                        int value = rd.Next(1, 101);

                        if (value <= 30) //1 ~ 30
                        {
                            SlotMachineItems.Add("🍈");
                        }
                        else if (value > 30 && value <= 60) //31 ~ 60
                        {
                            SlotMachineItems.Add("🍒");
                        }
                        else if (value > 60 && value <= 90) //61 ~ 90
                        {
                            SlotMachineItems.Add("🍋");
                        }
                        else if (value > 90 && value <= 95) //91 ~ 95
                        {
                            SlotMachineItems.Add("⭐");
                        }
                        else if (value > 95 && value <= 99) //96 ~ 99
                        {
                            SlotMachineItems.Add("🔔");
                        }
                        else if (value == 100) //100
                        {
                            SlotMachineItems.Add(":seven:");
                        }
                    }

                    EmbedBuilder builder = new EmbedBuilder();
                    builder.WithTitle("🎲 슬롯머신");
                    builder.WithColor(Color.Orange);
                    builder.WithDescription(SlotMachineItems[0]);
                    builder.WithFooter(new EmbedFooterBuilder 
                    {
                        IconUrl = Context.User.GetAvatarUrl(ImageFormat.Png, 128), 
                        Text = Context.User.Username 
                    });
                    builder.WithTimestamp(DateTimeOffset.Now);

                    var message = await Context.Channel.SendMessageAsync(string.Empty, false, builder.Build());

                    string SlotString = SlotMachineItems[0];

                    for (int i = 1; i < 3; i++)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(1));

                        SlotString = SlotString + SlotMachineItems[i];

                        builder.WithTitle("🎲 슬롯머신");
                        builder.WithColor(Color.Orange);
                        builder.WithDescription(SlotString);
                        builder.WithFooter(new EmbedFooterBuilder 
                        { 
                            IconUrl = Context.User.GetAvatarUrl(ImageFormat.Png, 128),
                            Text = Context.User.Username
                        });
                        builder.WithTimestamp(DateTimeOffset.Now);
                        await message.ModifyAsync(msg => msg.Embed = builder.Build());
                    }

                    if (SlotMachineItems[0] == ":seven:" && SlotMachineItems[1] == ":seven:" && SlotMachineItems[2] == ":seven:")
                    {
                        builder.WithTitle(":seven::seven::seven: 10배!");
                        builder.WithColor(Color.Teal);
                        builder.WithDescription($"슬롯머신에서 잭팟이 나와 `{string.Format("{0:n0}", Convert.ToUInt64(money) * 10)}`코인을 얻었습니다!");

                        user.addMoney(Convert.ToUInt64(money) * 10);
                    }
                    else if (SlotMachineItems[0] == "⭐" && SlotMachineItems[1] == "⭐" && SlotMachineItems[2] == "⭐")
                    {
                        builder.WithTitle("⭐⭐⭐ 7배");
                        builder.WithColor(Color.Teal);
                        builder.WithDescription($"슬롯머신에서 `{string.Format("{0:n0}", Convert.ToUInt64(money) * 7)}`코인을 얻었습니다!");

                        user.addMoney(Convert.ToUInt64(money) * 7);
                    }
                    else if (SlotMachineItems[0] == "🔔" && SlotMachineItems[1] == "🔔" && SlotMachineItems[2] == "🔔")
                    {
                        builder.WithTitle("🔔🔔🔔 5배");
                        builder.WithColor(Color.Teal);
                        builder.WithDescription($"슬롯머신에서 `{string.Format("{0:n0}", Convert.ToUInt64(money) * 5)}`코인을 얻었습니다!");

                        user.addMoney(Convert.ToUInt64(money) * 5);
                    }
                    else if (SlotMachineItems[0] == SlotMachineItems[1] && SlotMachineItems[1] == SlotMachineItems[2])
                    {
                        builder.WithTitle($"{SlotMachineItems[0]}{SlotMachineItems[0]}{SlotMachineItems[0]} 3배");
                        builder.WithColor(Color.Teal);
                        builder.WithDescription($"슬롯머신에서 `{string.Format("{0:n0}", Convert.ToUInt64(money) * 3)}`코인을 얻었습니다!");

                        user.addMoney(Convert.ToUInt64(money) * 3);
                    }
                    else if (SlotMachineItems[0] == SlotMachineItems[2])
                    {
                        builder.WithTitle($"{SlotMachineItems[0]}{SlotMachineItems[1]}{SlotMachineItems[2]} 2배");
                        builder.WithColor(Color.Teal);
                        builder.WithDescription($"슬롯머신에서 `{string.Format("{0:n0}", Convert.ToUInt64(money) * 2)}`코인을 얻었습니다!");

                        user.addMoney(Convert.ToUInt64(money) * 2);
                    }
                    else
                    {
                        builder.WithTitle($"💸 꽝..");
                        builder.WithColor(Color.Teal);
                        builder.WithDescription($"슬롯머신에서 `{string.Format("{0:n0}", Convert.ToUInt64(money))}`코인을 잃었습니다...");

                        user.subMoney(Convert.ToUInt64(money));
                    }

                    builder.WithFooter(new EmbedFooterBuilder 
                    { 
                        IconUrl = Context.User.GetAvatarUrl(ImageFormat.Png, 128),
                        Text = Context.User.Username 
                    });
                    builder.WithTimestamp(DateTimeOffset.Now);

                    await message.ModifyAsync(msg => msg.Embed = builder.Build());
                }
            }
        }

        [Command("끝말잇기", RunMode = RunMode.Async)]
        [Summary("1대1 끝말잇기를 시작합니다.")]
        public async Task WordAsync()
        {
            //각종 필요한 변수를 정의함
            int round = 0;
            int wrongCount = 0;
            string word = getRandomWords();
            List<string> newWord = new List<string>();
            List<string> usedWords = new List<string>();
            Class.User user = new Class.User(Context.Guild.Id, Context.User.Id);

            await ReplyAsync("끝말잇기 시작!");
            await ReplyAsync($"{word}!"); //아무 단어나 가져와서 메시지를 보냄
            usedWords.Add(word); //그리고 사용한 단어 리스트에 집어넣음
            gamingUser.Add(Context.Message.Author.Id); //게임중인 유저를 봇이 무시하도록 리스트에 집어넣음

            Stopwatch sw = new Stopwatch(); //스탑워치를 정의함

            Start: //goto에 사용할 레이블
            wrongCount = 0;
            var response = await NextMessageAsync(true, true, TimeSpan.FromMilliseconds(10000 - sw.ElapsedMilliseconds)); //10초 이내에 답을 말하지 않으면 null을 저장하고 아니라면 값을 저장함
            sw.Start(); //스톱워치를 시작함

            if (response != null)
            {
                if (word[word.Length - 1] != response.Content[0])
                {
                    wrongCount++;

                    await ReplyAsync("❌ 앞 글자가 맞지 않습니다!");

                    if (!(sw.ElapsedMilliseconds >= 10000))
                    {
                        goto Start;
                    }
                    else
                    {
                        EmbedBuilder builder = new EmbedBuilder();
                        builder.WithTitle("📋 끝말잇기");
                        builder.WithColor(Color.Red);
                        builder.WithDescription($"게임에서 패배하셨습니다..");
                        builder.AddField("버틴 라운드 수", $"{round} 라운드");
                        builder.AddField("패배 이유", "답변 시간 초과");
                        builder.WithFooter(new EmbedFooterBuilder 
                        { 
                            IconUrl = Context.User.GetAvatarUrl(ImageFormat.Png, 128),
                            Text = Context.User.Username 
                        });
                        builder.WithTimestamp(DateTimeOffset.Now);

                        await ReplyAsync(string.Empty, false, builder.Build());

                        goto End;
                    }
                }

                if (!isWordExist(response.Content) || (response.Content.Length == 1))
                {
                    wrongCount++;

                    await ReplyAsync("❌ 존재하지 않는 단어입니다!");

                    if (!(sw.ElapsedMilliseconds >= 10000))
                    {
                        goto Start;
                    }
                    else
                    {
                        EmbedBuilder builder = new EmbedBuilder();
                        builder.WithTitle("📋 끝말잇기");
                        builder.WithColor(Color.Red);
                        builder.WithDescription($"게임에서 패배하셨습니다..");
                        builder.AddField("버틴 라운드 수", $"{round} 라운드");
                        builder.AddField("패배 이유", "답변 시간 초과");
                        builder.WithFooter(new EmbedFooterBuilder 
                        { 
                            IconUrl = Context.User.GetAvatarUrl(ImageFormat.Png, 128),
                            Text = Context.User.Username 
                        });
                        builder.WithTimestamp(DateTimeOffset.Now);

                        await ReplyAsync(string.Empty, false, builder.Build());

                        goto End;
                    }
                }

                if (usedWords.Contains(response.Content))
                {
                    wrongCount++;

                    await ReplyAsync("❌ 이미 사용한 단어입니다!");

                    if (!(sw.ElapsedMilliseconds >= 10000))
                    {
                        goto Start;
                    }
                    else
                    {
                        EmbedBuilder builder = new EmbedBuilder();
                        builder.WithTitle("📋 끝말잇기");
                        builder.WithColor(Color.Red);
                        builder.WithDescription($"게임에서 패배하셨습니다..");
                        builder.AddField("버틴 라운드 수", $"{round} 라운드");
                        builder.AddField("패배 이유", "답변 시간 초과");
                        builder.WithFooter(new EmbedFooterBuilder 
                        { 
                            IconUrl = Context.User.GetAvatarUrl(ImageFormat.Png, 128),
                            Text = Context.User.Username 
                        });
                        builder.WithTimestamp(DateTimeOffset.Now);

                        await ReplyAsync(string.Empty, false, builder.Build());

                        goto End;
                    }
                }

                if (wrongCount > 3)
                {
                    if (!(sw.ElapsedMilliseconds >= 10000))
                    {
                        goto Start;
                    }
                    else
                    {
                        EmbedBuilder builder = new EmbedBuilder();
                        builder.WithTitle("📋 끝말잇기");
                        builder.WithColor(Color.Red);
                        builder.WithDescription($"게임에서 패배하셨습니다..");
                        builder.AddField("버틴 라운드 수", $"{round} 라운드");
                        builder.AddField("패배 이유", "답변 가능 횟수 초과");
                        builder.WithFooter(new EmbedFooterBuilder 
                        { 
                            IconUrl = Context.User.GetAvatarUrl(ImageFormat.Png, 128),
                            Text = Context.User.Username 
                        });
                        builder.WithTimestamp(DateTimeOffset.Now);

                        await ReplyAsync(string.Empty, false, builder.Build());

                        goto End;
                    }
                }

                if ((word[word.Length - 1] == response.Content[0]) && (isWordExist(response.Content)) && (response.Content.Length != 1) && !usedWords.Contains(response.Content))
                {
                    newWord = getStartWords(response.Content[response.Content.Length - 1].ToString()); //특정 글자로 시작하는 단어 리스트를 가져옴

                    //그런다음 사용한 단어를 "특정 글자로 시작하는 단어 리스트" 에서 뺌
                    foreach (string item in usedWords)
                    {
                        try
                        {
                            newWord.Remove(item);
                        }
                        catch (Exception)
                        {
                            throw;
                        }
                    }

                    Random rnd = new Random();
                    int r = rnd.Next(newWord.Count);
                    newWord.Sort();

                    try
                    {
                        word = (string)newWord[r]; //랜덤한 단어를 가져옴
                    }
                    catch (Exception)
                    {
                        round++;

                        EmbedBuilder builder = new EmbedBuilder();
                        builder.WithTitle("📋 끝말잇기");
                        builder.WithColor(Color.Green);
                        builder.WithDescription($"게임에서 승리하셨습니다!");
                        builder.AddField("버틴 라운드 수", $"{round} 라운드");
                        builder.AddField("상금", $"{round * 3} 코인");
                        builder.WithFooter(new EmbedFooterBuilder 
                        { 
                            IconUrl = Context.User.GetAvatarUrl(ImageFormat.Png, 128),
                            Text = Context.User.Username 
                        });
                        builder.WithTimestamp(DateTimeOffset.Now);

                        user.addMoney(Convert.ToUInt64(round * 3));

                        await ReplyAsync(string.Empty, false, builder.Build());

                        goto End;
                    }

                    //랜덤한 단어를 가져올게 없는 경우 게임 승리
                    if (string.IsNullOrWhiteSpace(word))
                    {
                        round++;

                        EmbedBuilder builder = new EmbedBuilder();
                        builder.WithTitle("📋 끝말잇기");
                        builder.WithColor(Color.Green);
                        builder.WithDescription($"게임에서 승리하셨습니다!");
                        builder.AddField("버틴 라운드 수", $"{round} 라운드");
                        builder.AddField("상금", $"{round * 3} 코인");
                        builder.WithFooter(new EmbedFooterBuilder 
                        { 
                            IconUrl = Context.User.GetAvatarUrl(ImageFormat.Png, 128),
                            Text = Context.User.Username 
                        });
                        builder.WithTimestamp(DateTimeOffset.Now);

                        user.addMoney(Convert.ToUInt64(round * 3));

                        await ReplyAsync(string.Empty, false, builder.Build());

                        goto End;
                    }

                    //랜덤한 사용하지 않은 단어가 있다면
                    else
                    {
                        await ReplyAsync($"{word}!");
                        round++;

                        usedWords.Add(word);
                        usedWords.Add(response.Content);

                        sw.Stop();
                        sw.Reset();

                        goto Start;
                    }
                }
            }
            else
            {
                EmbedBuilder builder = new EmbedBuilder();
                builder.WithTitle("📋 끝말잇기");
                builder.WithColor(Color.Red);
                builder.WithDescription($"게임에서 패배하셨습니다..");
                builder.AddField("버틴 라운드 수", $"{round} 라운드");
                builder.AddField("패배 이유", "답변 시간 초과");
                builder.WithFooter(new EmbedFooterBuilder 
                { 
                    IconUrl = Context.User.GetAvatarUrl(ImageFormat.Png, 128),
                    Text = Context.User.Username 
                });
                builder.WithTimestamp(DateTimeOffset.Now);

                await ReplyAsync(string.Empty, false, builder.Build());

                goto End;
            }

            End:

            if (user.Round < round)
            {
                user.setScore(round);
            }

            gamingUser.Remove(Context.Message.Author.Id);
            sw.Stop();
            sw.Reset();
        }

        [Command("끝말잇기 검색", RunMode = RunMode.Async)]
        [Alias("검색")]
        [Summary("끝말잇기에 사용되는 단어를 검색합니다.")]
        public async Task SearchAsync([Remainder] string word = "")
        {
            if (string.IsNullOrWhiteSpace(word))
            {
                await Context.Channel.SendMessageAsync("❌ 검색어는 비어 있을 수 없습니다.");
            }
            else if (word.Contains(" "))
            {
                await Context.Channel.SendMessageAsync("❌ 검색어에는 공백이 포함될 수 없습니다.");
            }
            else
            {
                PaginatedMessage paginatedMessage = new PaginatedMessage();
                List<string> words = searchWord(word);
                List<EmbedFieldBuilder> embedFieldBuilders = new List<EmbedFieldBuilder>();

                if (words.Count == 0)
                {
                    paginatedMessage.AlternateDescription = "데이터가 존재하지 않습니다.";
                }
                else
                {
                    for (int i = 0; i < words.Count; i++)
                    {
                        embedFieldBuilders.Add(new EmbedFieldBuilder { Name = $"{i + 1}", Value = $"{words[i]}" });
                    }
                }

                paginatedMessage.Title = "🔍 검색";
                paginatedMessage.Color = Color.LightOrange;
                paginatedMessage.Options.FieldsPerPage = 10;
                paginatedMessage.Options.JumpDisplayOptions = JumpDisplayOptions.Never;
                paginatedMessage.Options.DisplayInformationIcon = false;
                paginatedMessage.Pages = embedFieldBuilders;

                await PagedReplyAsync(paginatedMessage);
            }
        }

        [Command("업다운", RunMode = RunMode.Async)]
        [Summary("업다운 게임을 합니다.")]
        public async Task UpDownGameAsync()
        {
            Random rd = new Random();
            int num = rd.Next(1, 100);

            EmbedBuilder builder = new EmbedBuilder();
            builder.WithTitle("↕️ 업다운 게임");
            builder.WithDescription("수의 범위는 1 ~ 99 입니다.");
            builder.WithFooter(new EmbedFooterBuilder 
            { 
                IconUrl = Context.User.GetAvatarUrl(ImageFormat.Png, 128),
                Text = Context.User.Username 
            });
            builder.WithTimestamp(DateTimeOffset.Now);

            await Context.Channel.SendMessageAsync(string.Empty, false, builder.Build());

            Start:
            var response = await NextMessageAsync(true, true, TimeSpan.FromSeconds(10));

            if (response.Content == null)
            {
                await Context.Channel.SendMessageAsync("❌ 시간 초과!");
            }
            else if (!response.Content.All(char.IsDigit))
            {
                await Context.Channel.SendMessageAsync("❌ 수는 반드시 소수가 아닌 양수이여야 합니다.");
            }
            else if (response.Content == "0" || Convert.ToUInt64(response.Content) < 0 || (Convert.ToDecimal(response.Content) % 1) > 0 || Convert.ToUInt64(response.Content) > 100)
            {
                await Context.Channel.SendMessageAsync("❌ 수는 반드시 1 이상 100 이하의 정수여야 합니다.");
            }
            else if (response.Content == num.ToString())
            {
                builder.WithTitle("↕️ 업다운 게임");
                builder.WithColor(Color.Green);
                builder.WithDescription($"🎉 게임에서 승리하셨습니다!");
                builder.WithFooter(new EmbedFooterBuilder 
                { 
                    IconUrl = Context.User.GetAvatarUrl(ImageFormat.Png, 128),
                    Text = Context.User.Username 
                });
                builder.WithTimestamp(DateTimeOffset.Now);

                await ReplyAsync(string.Empty, false, builder.Build());
            }
            else if (Convert.ToInt32(response.Content) > num)
            {
                await Context.Channel.SendMessageAsync("🔽 다운!");
                goto Start;
            }
            else if (Convert.ToInt32(response.Content) < num)
            {
                await Context.Channel.SendMessageAsync("🔼 업!");
                goto Start;
            }
        }

        [Command("야옹", RunMode = RunMode.Async)]
        [Summary("귀여운 야옹이 사진을 봅니다.")]
        public async Task AjeAsync()
        {
            EmbedBuilder builder = new EmbedBuilder();

            using (WebClient client = new WebClient())
            {
                builder.WithTitle("🐱 야옹이");
                builder.WithImageUrl(JObject.Parse(client.DownloadString("http://aws.random.cat/meow")).SelectToken("file").ToString());
                builder.WithColor(Color.LightOrange);
                builder.WithFooter(new EmbedFooterBuilder 
                { 
                    IconUrl = Context.User.GetAvatarUrl(ImageFormat.Png, 128),
                    Text = $"{Context.User.Username}" 
                });
                builder.WithTimestamp(DateTimeOffset.Now);
            }

            Discord.Rest.RestUserMessage message = await Context.Channel.SendMessageAsync(embed: builder.Build());

            Action action = async delegate ()
            {
                using (WebClient client = new WebClient())
                {
                    builder.WithTitle("🐱 야옹이");
                    builder.WithImageUrl(JObject.Parse(client.DownloadString("http://aws.random.cat/meow")).SelectToken("file").ToString());
                    builder.WithColor(Color.LightOrange);
                    builder.WithFooter(new EmbedFooterBuilder
                    {
                        IconUrl = Context.User.GetAvatarUrl(ImageFormat.Png, 128),
                        Text = $"{Context.User.Username}"
                    });
                    builder.WithTimestamp(DateTimeOffset.Now);
                }

                await message.ModifyAsync(msg => msg.Embed = builder.Build());
            };

            CreateReactMessage(
                msg: message,
                emoji: new List<IEmote> { new Emoji("➡️") },
                action: new List<Action> { action },
                timeSpan: TimeSpan.FromMinutes(1),
                userId: Context.Message.Author.Id
            );
        }
    }
}