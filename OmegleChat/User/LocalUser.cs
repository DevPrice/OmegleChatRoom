using OmegleChatRoom.Event;

namespace OmegleChatRoom.User
{
    public class LocalUser : IUser
    {
        public string Name { get; set; }
        public bool IsMuted { get; set; }
        public bool IsTyping { get; set; }
        public PriviledgeLevel PriviledgeLevel { get; set; }

        public event MessageSentEvent MessageSent;

        public LocalUser()
        {
            Name = "Admin";
            PriviledgeLevel = PriviledgeLevel.Mod;
        }

        public void SendMessage(string message)
        {
            if (MessageSent != null)
                MessageSent(this, new MessageEventArgs(message));
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
