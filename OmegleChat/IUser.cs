using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmegleChatRoom
{
    public interface IUser
    {
        string Name { get; set; }
        bool IsMuted { get; set; }
        bool IsTyping { get; set; }
        PriviledgeLevel PriviledgeLevel { get; set; }

        void SendMessage(string message);
    }

    public enum PriviledgeLevel { None, Standard, Mod }
}
