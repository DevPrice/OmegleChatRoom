using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OmegleChatRoom.Event
{
    public class MessageEventArgs : EventArgs
    {
        public string Message;

        public MessageEventArgs(string message)
        {
            this.Message = message;
        }
    }

    public delegate void MessageReceivedEvent(object sender, MessageEventArgs e);
    public delegate void MessageSentEvent(object sender, MessageEventArgs e);
}