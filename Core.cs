using Konata.Core.Common;
using Konata.Core.Interfaces;
using Konata.Core.Interfaces.Api;
using Konata.Core.Message.Model;
using Konata.Core.Message;
using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;
using static Konata.Core.Events.Model.CaptchaEvent;

namespace QQBotKonata
{
    internal class Core
    {
        internal static bool IsLoginByInfo = false;

        internal static void Main()
        {
            Console.Title = "QQ机器人Konata启动器";
            Console.WriteLine("QQ机器人Konata启动器 -by SweelLong");
            if (!File.Exists(Path.Combine(Directory.GetCurrentDirectory(), "BotKeyStore.json")))
            {
                IsLoginByInfo = true;
            }
            Core core = new();
            if (IsLoginByInfo == true)
            {
                core.Login_ByInfo();
            }
            else
            {
                core.Login_ByLog();
            }
        }

        internal void Login_ByInfo()
        {
            Console.Write("输入你的 QQ: ");
            var uin = Console.ReadLine();// 输入QQ账号
            Console.Write("输入你的 QQ密码: ");
            var password = Console.ReadLine();// 输入QQ密码
            var botKeystore = new BotKeyStore(uin: uin, password: password);// 储存用户信息
            //string logsDir = Path.Combine(Directory.GetCurrentDirectory(), "Logs");
            //if (!Directory.Exists(logsDir))
            //{
            //    Directory.CreateDirectory(logsDir);
            //}
            LoadCore(botKeystore);
        }

        internal void Login_ByLog()
        {
            string text = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "BotKeyStore.json"));
            var botKeystore = JsonSerializer.Deserialize<BotKeyStore>(text, new JsonSerializerOptions { WriteIndented = true });
            LoadCore(botKeystore);
        }

        internal async void LoadCore(BotKeyStore botKeystore)
        {
            //string logsDir = Path.Combine(Directory.GetCurrentDirectory(), "Logs");
            //if (!Directory.Exists(logsDir))
            //{
            //    Directory.CreateDirectory(logsDir);
            //}
            var bot = BotFather.Create(BotConfig.Default(), BotDevice.Default(), botKeystore);// 创建机器人实例
            {
                //bot.OnLog += (_, e) =>// 日志
                //{
                //    string logFilePath = Path.Combine(logsDir, DateTime.Now.ToString("yyyy-MM-dd HH"));
                //    File.AppendAllLines(logFilePath, new string[] { DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff"), e.EventMessage }, System.Text.Encoding.UTF8);
                //};
                if (IsLoginByInfo == true)
                {
                    bot.OnCaptcha += (bot, e) =>// 验证码
                    {
                        bool isSuccess = false;
                        Console.WriteLine();
                        if (e.Type == CaptchaType.Slider)
                        {
                            Console.WriteLine("☆★☆★ 人机验证");
                            Console.WriteLine("请复制下方链接在浏览器打开, 完成验证后，\n浏览器打开开发者工具（按f12键），再选择Network（网络），找到cap_union_new_verify文件点Preview（预览），找到ticket，复制ticket冒号内的字符串（即登录密钥）\n然后在下方输入");
                            Console.WriteLine();
                            Console.WriteLine(e.SliderUrl);
                            Console.WriteLine();
                            Console.WriteLine("☆★☆★ 粘贴 ticket:");
                            isSuccess = bot.SubmitSliderTicket(Console.ReadLine()?.Trim() ?? "");
                        }
                        else if (e.Type == CaptchaType.Sms)
                        {
                            Console.WriteLine("☆★☆★ 短信验证");
                            Console.WriteLine();
                            Console.WriteLine(e.Phone);
                            Console.WriteLine();
                            Console.WriteLine("☆★☆★ 输入接收到的验证码:");
                            isSuccess = bot.SubmitSmsCode(Console.ReadLine()?.Trim() ?? "");
                        }
                        Console.WriteLine();
                        Console.WriteLine($"验证 {(isSuccess ? "通过" : "失败, 请重新验证")}");
                        Console.WriteLine();
                    };
                }
                bot.OnGroupMessage += (_, e) =>
                {
                    RandomImage.Group(_, e.Message.Chain.ToString(), e.GroupUin, e.Message.Sender.Uin);// 随机图片
                    Console.WriteLine($"群{e.Message.Sender}消息: {e.Message.Chain?.FirstOrDefault()?.ToString() ?? ""}");
                    string str = "";
                    foreach (BaseChain c in e.Chain)// 通过数据链转字符串的方式保留图片等代码
                    {
                        str += c.ToString();
                    }
                    AntiRecall.HistoryCall.Add(e.Message.Sequence, str);
                    AntiRecall.Write();
                };
                bot.OnFriendMessage += (_, e) =>
                {
                    RandomImage.Friend(_, e.Message.Chain.ToString(), e.Message.Sender.Uin);// 随机图片
                    Console.WriteLine($"好友{e.Message.Sender}消息: {e.Message.Chain?.FirstOrDefault()?.ToString() ?? ""}");
                };
                bot.OnFriendMessageRecall += (_, e) =>
                {

                };
                bot.OnGroupMessageRecall += (_, e) =>
                {
                    MessageBuilder mb = new();
                    mb.At(e.AffectedUin);
                    mb.Text("试图撤回一条消息：\n");
                    try
                    {
                        string coreText = AntiRecall.HistoryCall[e.Sequence];
                        GetCore(); 
                        void GetCore()
                        {
                            char[] str = coreText.ToCharArray();// 获取消息
                            for (int i = 0; i < str.Length; i++)
                            {
                                try
                                {
                                    if (str[i] == '[')
                                    {
                                        // 试图使用反射获取图片等信息，可能会影响性能
                                        if (new string(str[(i + 1)..(i + 9)]) == "KQ:image")
                                        {
                                            // 正则匹配提取关键代码
                                            string temp = new Regex("\\[KQ:image,(.+?)]").Match(coreText).Groups[0].Value;
                                            coreText = coreText.Replace(temp, "");
                                            mb.Add(typeof(ImageChain).GetMethod("Parse", BindingFlags.NonPublic | BindingFlags.Static).Invoke(this, new object[] { temp }) as ImageChain);
                                            GetCore(); break;
                                        }
                                        if (new string(str[(i + 1)..(i + 8)]) == "KQ:face")
                                        {
                                            string temp = new Regex("\\[KQ:face,(.+?)]").Match(coreText).Groups[0].Value;
                                            coreText = coreText.Replace(temp, "");
                                            mb.Add(typeof(QFaceChain).GetMethod("Parse", BindingFlags.NonPublic | BindingFlags.Static).Invoke(this, new object[] { temp }) as QFaceChain);
                                            GetCore(); break;
                                        }
                                        if (new string(str[(i + 1)..(i + 6)]) == "KQ:at")
                                        {
                                            string temp = new Regex("\\[KQ:at,(.+?)]").Match(coreText).Groups[0].Value;
                                            coreText = coreText.Replace(temp, "");
                                            mb.Add(typeof(AtChain).GetMethod("Parse", BindingFlags.NonPublic | BindingFlags.Static).Invoke(this, new object[] { temp }) as AtChain);
                                            GetCore(); break;
                                        }
                                        if (new string(str[(i + 1)..(i + 9)]) == "KQ:flash")
                                        {
                                            string temp = new Regex("\\[KQ:flash,(.+?)]").Match(coreText).Groups[0].Value;
                                            coreText = coreText.Replace(temp, "");
                                            mb.Add(typeof(FlashImageChain).GetMethod("Parse", BindingFlags.NonPublic | BindingFlags.Static).Invoke(this, new object[] { temp }) as FlashImageChain);
                                            GetCore(); break;
                                        }
                                        if (new string(str[(i + 1)..(i + 10)]) == "KQ:record")
                                        {
                                            string temp = new Regex("\\[KQ:record,(.+?)]").Match(coreText).Groups[0].Value;
                                            coreText = coreText.Replace(temp, "");
                                            mb.Add(typeof(RecordChain).GetMethod("Parse", BindingFlags.NonPublic | BindingFlags.Static).Invoke(this, new object[] { temp }) as RecordChain);
                                            GetCore(); break;
                                        }
                                    }
                                }
                                catch
                                {
                                    continue;
                                }
                            }
                        }
                        mb.Text(coreText);
                    }
                    catch
                    {
                        mb.Text("该部分已隐藏");
                    }
                    _.SendGroupMessage(e.GroupUin, mb);
                };
            }
            while (!await bot.Login())
            {
                Console.WriteLine("登录失败, 自动尝试重新登录并验证, 若为 QQ或密码错误, 请重新运行软件 以便 重新输入QQ及密码");
            }
            Console.WriteLine("登录成功");
            if (IsLoginByInfo == true)
            {
                Console.WriteLine("同步登录文件中...\n下方是该账号的 BotKeyStore.json文件, 请将它备份好, 用于登录使用");
                Console.WriteLine("不要复制 start 与 end 两行");
                string jsonStr = JsonSerializer.Serialize(bot.KeyStore,
                               new JsonSerializerOptions { WriteIndented = true });
                Console.WriteLine(jsonStr);
                File.WriteAllText(Path.Combine(Directory.GetCurrentDirectory(), "BotKeyStore.json"), jsonStr, System.Text.Encoding.UTF8);
            }
            AntiRecall.Read();// 读取历史消息
        }
    }
}