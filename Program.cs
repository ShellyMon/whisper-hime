using Humanizer;
using LiteDB;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MonkeyCache;
using MonkeyCache.LiteDB;
using Sora.EventArgs.SoraEvent;
using Sora.Net.Config;
using WhisperHime.Basics;
using WhisperHime.PixivApi;
using System;
using System.Threading.Tasks;
using YukariToolBox.LightLog;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using System.Net.Http;
using System.IO;
using SixLabors.ImageSharp.PixelFormats;
using System.Net.Http.Headers;
using System.Reflection.Metadata;
using System.Security.Policy;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Net;
using System.Text;
using System.Linq;
using JiebaNet.Segmenter;

namespace Sora
{
    class Program
    {
        static void Main(string[] args)
        {
            RunAsync().Wait();
        }

        static async Task RunAsync()
        {
            // 打开数据库
            var database = new LiteDatabase(new ConnectionString { Filename = "database.dat", InitialSize = 0x10000000 });
            // 支持实体名转复数形式，使用EFCore同款方案
            database.Mapper.ResolveCollectionName = type => type.Name.Pluralize();

            // 配置缓存
            BarrelUtils.SetBaseCachePath(AppDomain.CurrentDomain.BaseDirectory);
            Barrel.ApplicationId = "Cache";

            // 配置容器
            Ioc.Configure(services => {
                // 日志
                services.AddLogging(logging => {
                    logging.AddConsole();
                });

                // 数据库
                services.AddSingleton<ILiteDatabase>(database);

                // Pixiv
                services.AddSingleton<PixivClient>();
            });

            // 配置Bot日志
            Log.LogConfiguration
               .SetLogLevel(YukariToolBox.LightLog.LogLevel.Info)
               .AddLogService(new LogService());

            // 获取logger
            var logger = Ioc.Require<ILogger<Program>>();

            // 登录Pixiv
            try
            {
                await PixivLogin();
            }
            catch (Exception e)
            {
                logger.LogError(e, "Pixiv 登录错误");
            }

            // 处理Bot命令异常
            async void HandleBotCommandException(Exception exception, BaseMessageEventArgs eventArgs, string message)
            {
                logger.LogError(exception, "{message}", message);

                var msg = $"报错\r\n{message}\r\n{exception.Message}";

                switch (eventArgs)
                {
                    case GroupMessageEventArgs g:
                        await g.Reply(msg);
                        break;
                    case PrivateMessageEventArgs p:
                        await p.Reply(msg);
                        break;
                }
            }

            // 配置Bot
            var bot = SoraServiceFactory.CreateService(new ClientConfig
            {
                CommandExceptionHandle = HandleBotCommandException,
                SendCommandErrMsg = false,
                ApiTimeOut = TimeSpan.FromSeconds(30),
            });

            //bot.Event.CommandManager.RegisterPrivateDynamicCommand(
            //    new[] { "插入" },
            //async eventArgs =>
            //{
            //    var a =  SoraBot.Tools.Util.GetFileName(@"clp\img");
            //    string path = @"clp\img\";
            //    var db = Ioc.Require<ILiteDatabase>();
            //    var col = db.GetCollection<Wife>();
            //    foreach (var item in a)
            //    {
            //        string[] b = item.Split(new char[] { '-', '.' });
            //        Wife wife = new Wife()
            //        {
            //            Name = b[0],
            //            Sentence = b[1],
            //            ImageUrl = path+item
            //        };
            //        if (!col.Exists(x => x.Name.Contains(wife.Name)))
            //        {
            //            col.Insert(wife);
            //        }

            //    }
            //    eventArgs.IsContinueEventChain = false;
            //    await eventArgs.Reply("坏耶");
            //});

            bot.Event.CommandManager.RegisterPrivateDynamicCommand(new[] { "测试" },
                async eventArge =>
                {
                    var CHROME_UA = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/112.0.0.0 Safari/537.36";

                    var Kind1970 =new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

                    var datenow= DateTime.Now.ToUniversalTime().Subtract(Kind1970).Ticks/ 1000;

                    var  api_key = Convert.ToBase64String(Encoding.UTF8.GetBytes((Math.Pow(datenow, 2) + Math.Pow(CHROME_UA.Length, 2)).ToString())).Trim().Replace("=", "");
                    Console.Write(api_key);
                    HttpClient client = new HttpClient();
                    var filePath = @"D:\1122.jpg";

                    var content = new MultipartFormDataContent();

                    //content.Headers.Add("content-type", "multipart/form-data");
                    content.Headers.Add("x-api-key",api_key);
                    //content.Headers.Add("referer", "ttps://soutubot.moe/");
                    content.Headers.Add("x-requested-with","XMLHttpRequest");

                    content.Add(new StringContent("factor"), "1.2");
                    content.Add(new ByteArrayContent(System.IO.File.ReadAllBytes(filePath)));

                    var requestUri = "https://soutubot.moe/api/search";
                    var result = client.PostAsync(requestUri, content).Result.Content.ReadAsStringAsync().Result;

                    //Console.WriteLine(result);

                    await eventArge.Reply("好耶");

                });

            // 启动Bot
            logger.LogInformation("Startup");

            try
            {
                await bot.StartService();
            }
            catch (Exception e)
            {
                logger.LogError(e, "BotException");
            }

            // TODO: 堵塞
            await Task.Delay(-1);
        }

        static async Task PixivLogin()
        {
            var logger = Ioc.Require<ILogger<Program>>();
            var client = Ioc.Require<PixivClient>();

            logger.LogInformation("读取 Pixiv token");

            var path = "pixiv_token.json";

            try
            {
                await client.LoadTokenAsync(path);
            }
            catch (Exception)
            {
                logger.LogWarning("Pixiv token 读取失败");
            }

            if (client.IsTokenValid())
            {
                logger.LogInformation("Pixiv token 读取成功");
                return;
            }

            logger.LogInformation("刷新 Pixiv token");

            await client.RefreshTokenAsync();

            if (client.IsTokenValid())
            {
                logger.LogInformation("Pixiv token 刷新成功");

                await client.SaveTokenAsync(path);
                return;
            }

            logger.LogInformation("登录 Pixiv");

            await client.LoginAsync();

            if (client.IsTokenValid())
            {
                logger.LogInformation("Pixiv 登录成功");

                await client.SaveTokenAsync(path);
                return;
            }

            logger.LogWarning("Pixiv 登录失败");
        }
    }
}
