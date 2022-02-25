// See https://aka.ms/new-console-template for more information
using Sisters.WudiLib.WebSocket;

var cqWebSocketEvent = new CqHttpWebSocketEvent("ws://127.0.0.1:6700/event"); // 创建 WebSocket 事件监听客户端。
var httpApiClient = new CqHttpWebSocketApiClient("ws://127.0.0.1:6700/api"); // 创建 HTTP 通信客户端。
cqWebSocketEvent.ApiClient = httpApiClient;

cqWebSocketEvent.MessageEvent += async(api, e) =>
{

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