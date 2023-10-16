using Humanizer;
using LiteDB;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MonkeyCache;
using MonkeyCache.LiteDB;
using Sora.EventArgs.SoraEvent;
using Sora.Net.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using WhisperHime.Basics;
using WhisperHime.Dto.BGM;
using WhisperHime.Dto.Pixiv;
using WhisperHime.Dto.soutubot;
using WhisperHime.PixivApi;
using YukariToolBox.LightLog;
using Telegram.Bot;
using System.Threading;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;

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
            // 配置文件路径
            var configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json");

            // 读取配置文件
            var configRoot = new ConfigurationBuilder()
                .AddJsonFile(configPath)
                .Build();

            // 获取主配置节（所有配置都放在这个节里面）
            var config = configRoot.GetRequiredSection("WhisperHimeSettings");

            // 打开数据库
            var database = new LiteDatabase(new ConnectionString { Filename = config.GetValue<string>("Database"), InitialSize = 0x10000000 });
            // 支持实体名转复数形式，使用EFCore同款方案
            database.Mapper.ResolveCollectionName = type => type.Name.Pluralize();

            // 配置缓存
            BarrelUtils.SetBaseCachePath(AppDomain.CurrentDomain.BaseDirectory);
            Barrel.ApplicationId = "Cache";

            // 配置容器
            Ioc.Configure(services =>
            {
                // 配置文件
                services.AddSingleton<IConfiguration>(config);

                // 日志
                services.AddLogging(logging =>
                {
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
                ApiTimeOut = TimeSpan.FromSeconds(180),
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

            var botClient = new TelegramBotClient("5714174864:AAED6t1dMJ8x6vdZgNH5tM03M0BDlVPLg-0");

            //var me = await botClient.GetMeAsync();
            using CancellationTokenSource cts = new();

            // StartReceiving does not block the caller thread. Receiving is done on the ThreadPool.
            ReceiverOptions receiverOptions = new()
            {
                AllowedUpdates = Array.Empty<UpdateType>() // receive all update types except ChatMember related updates
            };

            botClient.StartReceiving(
                updateHandler: HandleUpdateAsync,
                pollingErrorHandler: HandlePollingErrorAsync,
                receiverOptions: receiverOptions,
                cancellationToken: cts.Token
            );

            var me = await botClient.GetMeAsync();

            Console.WriteLine($"Start listening for @{me.Username}");
            Console.ReadLine();

            // Send cancellation request to stop bot
            cts.Cancel();

            async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
            {
                // Only process Message updates: https://core.telegram.org/bots/api#message
                if (update.Message is not { } message)
                    return;
                // Only process text messages
                if (message.Text is not { } messageText)
                    return;

                var chatId = message.Chat.Id;

                Console.WriteLine($"Received a '{messageText}' message in chat {chatId}.");

                // Echo received message text
                Message sentMessage = await botClient.SendTextMessageAsync(
                    chatId: chatId,
                    text: "You said:\n" + messageText,
                    cancellationToken: cancellationToken);
            }

            Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
            {
                var ErrorMessage = exception switch
                {
                    ApiRequestException apiRequestException
                        => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                    _ => exception.ToString()
                };

                Console.WriteLine(ErrorMessage);
                return Task.CompletedTask;
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
