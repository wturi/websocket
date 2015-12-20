using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using System.IO;
using StriveEngine.SimpleDemoServer.Models;
using System.Net;
using Newtonsoft.Json.Linq;

namespace StriveEngine.SimpleDemoServer
{
    public class UCTools
    {


        // 从一个对象信息生成Json串
        public static string ObjectToJson(object obj)
        {


            return JsonConvert.SerializeObject(obj);
        }
        // 从一个Json串生成对象信息
        public static Messages JsonToObject(string jsonString)
        {
            var jsons = jsonString.Replace("\"", "").Replace("{", "").Replace("}", "").Split(',');

            string serviceip = jsons[7].Split(':')[1];
            string userip = jsons[8].Split(':')[1];
            if (serviceip == "" && userip == "")
            {
                return JsonConvert.DeserializeObject<Messages>(jsonString);
            }
            else
            {
                Messages m = new Messages();
                m.NickID = jsons[0].Split(':')[1];
                m.NickName = jsons[1].Split(':')[1];
                m.Msg = jsons[2].Split(':')[1];
                m.IsFirst = jsons[3].Split(':')[1] == "true" ? true : false;
                m.IsUser = jsons[4].Split(':')[1] == "true" ? true : false;
                m.PairingServiceID = jsons[5].Split(':')[1];
                m.PairingUserID = jsons[6].Split(':')[1];
                m.PairingServiceIP = null;
                m.PairingUserIP = null;
                if (serviceip != "")
                {
                    serviceip = jsons[7].Split(':')[1] + ":" + jsons[7].Split(':')[2];
                    IPAddress ipadr = IPAddress.Parse(serviceip.Split(':')[0]);
                    m.PairingServiceIP = new IPEndPoint(ipadr, int.Parse(serviceip.Split(':')[1]));
                }
                if (userip != "")
                {
                    userip = jsons[8].Split(':')[1] + ":" + jsons[8].Split(':')[2];
                    IPAddress ipadr = IPAddress.Parse(userip.Split(':')[0]);
                    m.PairingUserIP = new IPEndPoint(ipadr, int.Parse(userip.Split(':')[1]));
                }

                return m;
            }

        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="Json"></param>
        /// <returns></returns>
        public static Messages Deal(string Json)
        {
            Messages mes = new Messages();

            var jsons = Json.Replace("\"", "");

            return mes;
        }


    }





}
