using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OmegleChatRoom.Event
{
    public class CaptchaRequiredArgs : EventArgs
    {
        public string Id;
        public string Url;

        public CaptchaRequiredArgs(string id)
        {
            this.Id = id;
            this.Url = "http://www.google.com/recaptcha/api/challenge?k=" + id + "&ajax=1&cachestop=0.7569315146943529";
        }
    }

    public delegate void CaptchaRequiredEvent(object sender, CaptchaRequiredArgs e);
}
