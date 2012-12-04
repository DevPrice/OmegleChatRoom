using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OmegleChatRoom
{
    public class UnhandledResponseEventArgs : EventArgs
    {
        public string Response;

        public UnhandledResponseEventArgs(string response)
        {
            this.Response = response;
        }
    }

    public delegate void UnhandledResponseEvent(object sender, UnhandledResponseEventArgs e);
}