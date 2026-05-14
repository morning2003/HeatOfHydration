using VolPro.Core.Enums;
using VolPro.Core.Extensions;
using VolPro.Core.Language;

namespace VolPro.Core.Utilities
{
    public class WebResponseContent
    {
        public WebResponseContent()
        {
        }
        public WebResponseContent(bool status)
        {
            this.Status = status;
        }
        public bool Status { get; set; }
        public string Code { get; set; }
        public string Message { get; set; }
        //public string Message { get; set; }
        public object Data { get; set; }

        public WebResponseContent OK()
        {
            this.Status = true;
            return this;
        }
        public WebResponseContent OKData(object data)
        {
            this.Status = true;
            this.Data = data;
            return this;
        }
        public WebResponseContent OKData(string message, object data, bool ts = false)
        {
            this.Message = ts ? message?.Translator() : message;
            this.Status = true;
            this.Data = data;
            return this;
        }
        public static WebResponseContent Instance
        {
            get { return new WebResponseContent(); }
        }
        public WebResponseContent OK(string message = null, object data = null, bool ts = true)
        {
            this.Status = true;
            this.Message = ts ? message?.Translator() : message;
            this.Data = data;
            return this;
        }
        public WebResponseContent OKDataToString(object data = null)
        {
            this.Status = true;
            this.Data = data.Serialize();
            return this;
        }
        public WebResponseContent OK(ResponseType responseType, bool ts = true)
        {
            return Set(responseType, true, true);
        }
        public WebResponseContent Error(string message = null, bool ts = false)
        {
            this.Status = false;
            this.Message = ts ? message?.Translator() : message;
            return this;
        }
        public WebResponseContent Error(ResponseType responseType, bool ts = false)
        {
            return Set(responseType, false, ts);
        }
        public WebResponseContent Set(ResponseType responseType, bool ts = false)
        {
            bool? b = null;
            return this.Set(responseType, b, ts);
        }
        public WebResponseContent Set(ResponseType responseType, bool? status, bool ts = false)
        {
            return this.Set(responseType, null, status, ts);
        }
        public WebResponseContent Set(ResponseType responseType, string msg, bool ts = false)
        {
            bool? b = null;
            return this.Set(responseType, msg, b);
        }
        public WebResponseContent Set(ResponseType responseType, string msg, bool? status, bool ts = false)
        {
            if (status != null)
            {
                this.Status = (bool)status;
            }
            this.Code = ((int)responseType).ToString();
            if (!string.IsNullOrEmpty(msg))
            {
                Message = ts ? msg.Translator() : msg;
                return this;
            }

            Message = responseType.GetMsg();
            if (ts)
            {
                Message = Message.Translator();
            }
            return this;
        }

    }
}
