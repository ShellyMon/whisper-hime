// See https://aka.ms/new-console-template for more information
using Sisters.WudiLib.WebSocket;

var cqWebSocketEvent = new CqHttpWebSocketEvent("ws://127.0.0.1:6700/event"); // 创建 WebSocket 事件监听客户端。
var httpApiClient = new CqHttpWebSocketApiClient("ws://127.0.0.1:6700/api"); // 创建 HTTP 通信客户端。
cqWebSocketEvent.ApiClient = httpApiClient;