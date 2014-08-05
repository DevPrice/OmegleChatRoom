using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OmegleChatRoom.Event
{
    public class CaptchaRequiredEventArgs : EventArgs
    {
        public string Id;
        public string Url;

        public CaptchaRequiredEventArgs(string id)
        {
            this.Id = id;
            this.Url = "http://www.google.com/recaptcha/api/challenge?k=" + id + "&ajax=1&cachestop=0.7569315146943529";
        }
    }

    public delegate void CaptchaRequiredEvent(object sender, CaptchaRequiredEventArgs e);
}
