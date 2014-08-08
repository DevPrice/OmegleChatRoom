using OmegleChatRoom.Event;
using System;
using System.Threading.Tasks;

namespace OmegleChatRoom.User
{
    public class RemoteUser : IUser
    {
        public string Name { get; set; }
        public bool IsMuted { get; set; }
        public bool IsTyping { get; set; }
        public PriviledgeLevel PriviledgeLevel { get; set; }

        public OmegleSession Session { get; private set; }

        private static int CurrentUserNumber = 1;

        /// <summary>
        /// Raised when a message from a stranger is received.
        /// </summary>
        public event MessageReceivedEvent MessageReceived;

        /// <summary>
        /// Raised when the Stranger disconnects.
        /// </summary>
        public event EventHandler StrangerDisconnected;

        /// <summary>
        /// Raised when the stranger is typing a message.
        /// </summary>
        public event EventHandler StrangerTyping;

        /// <summary>
        /// Raised a stranger is connected.
        /// </summary>
        public event EventHandler Connected;

        /// <summary>
        /// Raised when the stranger stops typing a message.
        /// </summary>
        public event EventHandler StrangerStoppedTyping;

        /// <summary>
        /// Raised when a user count message is received.
        /// </summary>
        public event EventHandler Count;

        /// <summary>
        /// Raised when an unknown or unhandled event is received.
        /// </summary>
        public event UnhandledResponseEvent UnhandledResponse;

        /// <summary>
        /// Raised when the server asks for a captcha response.
        /// </summary>
        public event CaptchaRequiredEvent CaptchaRequired;

        /// <summary>
        /// Raised when the server refuses a captcha.
        /// </summary>
        public event EventHandler CaptchaRefused;

        /// <summary>
        /// Raised when the application is still looking for a partner to connect to.
        /// </summary>
        public event EventHandler WaitingForPartner;

        public RemoteUser()
        {
            Name = "User" + (CurrentUserNumber++).ToString();
            PriviledgeLevel = PriviledgeLevel.Standard;

            Session = new OmegleSession();

            Session.CaptchaRefused += Omegle_CaptchaRefused;
            Session.CaptchaRequired += Omegle_CaptchaRequired;
            Session.Connected += Omegle_Connected;
            Session.Count += Omegle_Count;
            Session.MessageReceived += Omegle_MessageReceived;
            Session.StrangerDisconnected += Omegle_StrangerDisconnected;
            Session.StrangerStoppedTyping += Omegle_StrangerStoppedTyping;
            Session.StrangerTyping += Omegle_StrangerTyping;
            Session.WaitingForPartner += Omegle_WaitingForPartner;
            Session.UnhandledResponse += Omegle_UnhandledResponse;
        }

        /// <summary>
        /// Connects to the Omegle network and starts processing events.
        /// </summary>
        public void Connect()
        {
            new Task(delegate { Session.Connect(); }).Start();
        }

        /// <summary>
        /// Disconnects from the server.
        /// </summary>
        public void Disconnect()
        {
            new Task(delegate { Session.Disconnect(); }).Start();
        }

        /// <summary>
        /// Sends in another thread, unlike OmegleSession.SendMessage()
        /// </summary>
        /// <param name="message"></param>
        public void SendMessage(string message)
        {
            new Task(delegate { Session.SendMessage(message); }).Start();
        }

        void Omegle_UnhandledResponse(object sender, UnhandledResponseEventArgs e)
        {
            if (this.UnhandledResponse != null)
                this.UnhandledResponse(this, e);
        }

        void Omegle_WaitingForPartner(object sender, EventArgs e)
        {
            if (this.WaitingForPartner != null)
                this.WaitingForPartner(this, e);
        }

        void Omegle_StrangerTyping(object sender, EventArgs e)
        {
            IsTyping = true;

            if (this.StrangerTyping != null)
                this.StrangerTyping(this, e);
        }

        void Omegle_StrangerStoppedTyping(object sender, EventArgs e)
        {
            IsTyping = false;

            if (this.StrangerStoppedTyping != null)
                this.StrangerStoppedTyping(this, e);
        }

        void Omegle_StrangerDisconnected(object sender, EventArgs e)
        {
            IsTyping = false;

            if (this.StrangerDisconnected != null)
                this.StrangerDisconnected(this, e);
        }

        void Omegle_MessageReceived(object sender, MessageEventArgs e)
        {
            IsTyping = false;

            if (this.MessageReceived != null)
                this.MessageReceived(this, e);
        }

        void Omegle_Count(object sender, EventArgs e)
        {
            if (this.Count != null)
                this.Count(sender, e);
        }

        void Omegle_Connected(object sender, EventArgs e)
        {
            if (this.Connected != null)
                this.Connected(this, e);
        }

        void Omegle_CaptchaRequired(object sender, CaptchaRequiredEventArgs e)
        {
            if (this.CaptchaRequired != null)
                this.CaptchaRequired(this, e);
        }

        void Omegle_CaptchaRefused(object sender, EventArgs e)
        {
            if (this.CaptchaRefused != null)
                this.CaptchaRefused(this, e);
        }

        public override string ToString()
        {
            return String.Format("{0}{1}", Name, IsMuted ? " (muted)" : (IsTyping ? " (typing)" : ""));
        }
    }

    
}
