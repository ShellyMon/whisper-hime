using Sora.Entities;
using Sora.EventArgs.SoraEvent;
using System;
using System.Threading.Tasks;

namespace WhisperHime.Tools
{
    internal static class BotExtensions
    {
        /// <summary>
        /// 快速发送回复，只支持<c>GroupMessageEventArgs</c>和<c>PrivateMessageEventArgs</c>这两种类型的事件。
        /// </summary>
        /// <param name="e">参数</param>
        /// <param name="message">消息</param>
        /// <param name="timeout">超时</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        internal static ValueTask<(ApiStatus apiStatus, int messageId)> Reply(this BaseSoraEventArgs e, MessageBody message, TimeSpan? timeout = null)
        {
            if (e is PrivateMessageEventArgs p)
                return p.Reply(message, timeout);

            if (e is GroupMessageEventArgs g)
                return g.Reply(message, timeout);

            throw new NotImplementedException();
        }
    }
}
