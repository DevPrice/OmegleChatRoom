using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmegleChatRoom.Event
{
    public class StatusChangedEventArgs : EventArgs
    {

    }

    public delegate void StatusChanged(object sender, StatusChangedEventArgs e);
}
