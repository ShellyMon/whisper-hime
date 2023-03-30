using Sora;
using Sora.EventArgs.SoraEvent;
using Sora.Interfaces;
using Sora.Net.Config;
using Sora.Util;
using SqlSugar.IOC;
using YukariToolBox.LightLog;

SugarIocServices.AddSqlSugar(new IocConfig()
{
    ConnectionString = "Server=localhost\\MSSQLSERVER01;Database=ST;Trusted_Connection=True;",//连接符字串
    DbType = IocDbType.SqlServer,
    IsAutoCloseConnection = true
});

SugarIocServices.ConfigurationSugar(db => {
    db.Aop.OnLogExecuted = (sql, p) => {
        Console.WriteLine(sql);
    };
});

//设置log等级
Log.LogConfiguration
   .EnableConsoleOutput()
   .SetLogLevel(LogLevel.Info);

//实例化Sora服务
//ISoraService service = SoraServiceFactory.CreateService(new ClientConfig({
//CommandExceptionHandle = CommandExceptionHandle,    
//}));

ISoraService service = SoraServiceFactory.CreateService(new ClientConfig
{
    CommandExceptionHandle = CommandExceptionHandle,
    ApiTimeOut = TimeSpan.FromSeconds(30),
});

//exception 为指令执行抛出的异常
//eventArgs 是本次消息的事件上下文
//log 为框架自动生成的错误日志
async void CommandExceptionHandle(Exception exception, BaseMessageEventArgs eventArgs, string log)
{
    string msg = $"死了啦都你害的啦\r\n{log}\r\n{exception.Message}";
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

//service.Event.OnGroupMessage += async (sender, eventArgs) => {
//    //await eventArgs.SourceGroup.SendGroupMessage(eventArgs.Message.MessageBody);
//    MessageBody messageBody = SoraSegment.At(eventArgs.SenderInfo.UserId);
//    await eventArgs.SourceGroup.SendGroupMessage(messageBody);

//};

//启动服务并捕捉错误
await service.StartService()
             .RunCatch(e => Log.Error("Sora Service", Log.ErrorLogBuilder(e)));
await Task.Delay(-1);
