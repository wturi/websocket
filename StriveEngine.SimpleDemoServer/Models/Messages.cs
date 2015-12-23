using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace StriveEngine.SimpleDemoServer.Models
{
    public class Messages
    {
        public string NickID { get; set; }
        public string NickName { get; set; }
        public string Msg { get; set; }
        public bool IsFirst { get; set; }
        public bool IsUser { get; set; }
        public string PairingServiceID { get; set; }
        public string PairingUserID { get; set; }
        public IPEndPoint PairingServiceIP { get; set; }
        public IPEndPoint PairingUserIP { get; set; }
        public string Headimgurl { get; set; }
    }
}
