// See https://aka.ms/new-console-template for more information
using Microsoft.Extensions.Caching.Memory;
using Sisters.WudiLib.WebSocket;
using whisper_hime;
using Message = Sisters.WudiLib.SendingMessage;

var cqWebSocketEvent = new CqHttpWebSocketEvent("ws://127.0.0.1:6700/event"); // 创建 WebSocket 事件监听客户端。
var httpApiClient = new CqHttpWebSocketApiClient("ws://127.0.0.1:6700/api"); // 创建 HTTP 通信客户端。
cqWebSocketEvent.ApiClient = httpApiClient;
Pub pub = new Pub();
Sauce sauce = new Sauce(pub);
IMemoryCache cache = new MemoryCache(new MemoryCacheOptions());
//消息事件
cqWebSocketEvent.MessageEvent += async(api, e) =>
{
    var Stext = e.Content.Text;
    Console.WriteLine(e.Content.Text);
    await pub.SauceAsync(api, e,0); //识图
    if (Stext.IndexOf("#测试")==0)
    {
        MultiDownload Down = new MultiDownload();
        List<Task> taskList = new List<Task>();
            Down.DownloadFile("https://i.pximg.net/img-original/img/2022/02/26/00/03/55/96519763_p0.png", @"D:\ng\cs\96519763_p0.png", 5);
        //taskList.Add(Task.Factory.StartNew(() =>
        //{
        //    Down.DownloadFile("https://i.pximg.net/img-original/img/2022/03/05/00/00/23/96680243_p0.jpg", @"D:\ng\cs\96680243_p0.jpg", 5);
        //}));
        //taskList.Add(Task.Factory.StartNew(() =>
        //{
        //    Down.DownloadFile("https://i.pximg.net/img-original/img/2022/02/06/00/00/14/96029615_p0.png", @"D:\ng\cs\96029615_p0.png", 5);
        //}));
    }
};
//识图
pub.SauceEvent += async (api, e) => {
    bool group = false;
    if (e.MessageType is "group")
    {
        group = true;
    }
    try
    {
        if (e.Content.Text.Trim().IndexOf("识图") == 0)
        {

            string[] eee = e.RawMessage.ToString().Split(new char[2] { '[', ']' });
            if (e.Content.Sections[0].ToString().IndexOf("CQ:reply,id=") >= 0)
            {
                var id = e.Content.Sections[0].Data["id"];
                var d = await api.getMsg(id);

            }
            else
            {
                bool acg = false;
                for (int i = 0; i < eee.Length; i++)
                {
                    if (eee[i].IndexOf("CQ:image,file=") >= 0)
                    {
                        acg = true;
                    }
                }
                if (acg)
                {
                    await pub.SauceAsync(api, e, 1);
                }
                else
                {
                    if (group)
                        await api.SendMessageAsync(e.Endpoint, Message.At(e.UserId) + "发送一张图片喵！");
                    CacheHelper.CacatDate(e.UserId.ToString(), "test", api, e, cache);
                }
            }
        }

        string[] ccc = e.RawMessage.ToString().Split(new char[2] { '[', ']' });
        for (int i = 0; i < ccc.Length; i++)
        {
            if (ccc[i].IndexOf("CQ:image,file=") >= 0)
            {
                if (CacheHelper.boolDate(e.UserId.ToString(), cache))
                {
                    await pub.SauceAsync(api, e, 1);
                    CacheHelper.RemoveCache(e.UserId.ToString(), cache);
                }

            }
        }
    }
    catch
    {

    }
};

// 连接前等待 3 秒观察状态。
Task.Delay(TimeSpan.FromSeconds(3)).Wait();
// 连接（开始监听上报）。
var cancellationTokenSource = new CancellationTokenSource();
cqWebSocketEvent.StartListen(cancellationTokenSource.Token);
while (true)
{
    if (cqWebSocketEvent.IsAvailable == false && cqWebSocketEvent.IsListening == false)
    {
        cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(1));
        Task.Delay(TimeSpan.FromSeconds(1)).Wait();
        cancellationTokenSource.Dispose();
        cancellationTokenSource = new CancellationTokenSource();
        cqWebSocketEvent.StartListen(cancellationTokenSource.Token);
    }
    else
    {
        Task.Delay(-1).Wait();
    }
}
// 按下回车会在 2 秒后断开，再过 3 秒使用新的 CancellationTokenSource 重连。
// 您可以先断开网络，观察自动重连，再继续执行后面的代码。
Console.ReadLine();