using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using StriveEngine;
using StriveEngine.Core;
using StriveEngine.Tcp.Server;
using System.Net;
using Newtonsoft.Json;
using StriveEngine.SimpleDemoServer.Models;

namespace StriveEngine.SimpleDemoServer
{
    /*
     * 
     * ESFramework 强悍的通信框架、P2P框架、群集平台。OMCS 简单易用的网络语音视频框架。MFile 语音视频录制组件。StriveEngine 轻量级的通信引擎。
     */
    public partial class Form1 : Form
    {
        private ITcpServerEngine tcpServerEngine;
        private Messages msgobj = new Models.Messages();
        private IPEndPoint ServiceIP;
        private string jsonstr = "{\"NickID\":\"{0}\",\"NickName\":\"{1}\",\"Msg\":\"{2}\",\"IsFirst\":{3},\"IsUser\":{4},\"PairingServiceID\":\"{5}\",\"PairingUserID\": \"{6}\",\"PairingServiceIP\":\"{7}\",\"PairingUserIP\":\"{8}\"}";
        public Form1()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 启动程序
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                //初始化并启动服务端引擎（TCP、文本协议）
                this.tcpServerEngine = NetworkEngineFactory.CreateTextTcpServerEngine(int.Parse(this.textBox_port.Text), new DefaultTextContractHelper("\0"));//DefaultTextContractHelper是StriveEngine内置的ITextContractHelper实现。使用UTF-8对EndToken进行编码。 
                this.tcpServerEngine.ClientCountChanged += new CbDelegate<int>(tcpServerEngine_ClientCountChanged);
                this.tcpServerEngine.ClientConnected += new CbDelegate<System.Net.IPEndPoint>(tcpServerEngine_ClientConnected);
                this.tcpServerEngine.ClientDisconnected += new CbDelegate<System.Net.IPEndPoint>(tcpServerEngine_ClientDisconnected);
                this.tcpServerEngine.MessageReceived += new CbDelegate<IPEndPoint, byte[]>(tcpServerEngine_MessageReceived);
                this.tcpServerEngine.Initialize();

                this.button1.Enabled = false;
                this.textBox_port.ReadOnly = true;
                this.button2.Enabled = true;
                this.comboBox1.Enabled = true;
                this.button3.Enabled = true;
            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.Message);
            }
        }


        /// <summary>
        /// 接受消息
        /// </summary>
        /// <param name="client"></param>
        /// <param name="bMsg"></param>
        void tcpServerEngine_MessageReceived(IPEndPoint client, byte[] bMsg)
        {
            string msg = System.Text.Encoding.UTF8.GetString(bMsg); //消息使用UTF-8编码
            if (msg.Length <= 0)
            {
                return;
            }
            msg = msg.Substring(0, msg.Length - 1); //将结束标记"\0"剔除
            this.ShowClientMsg(client, msg);
        }

        /// <summary>
        /// 下线提醒
        /// </summary>
        /// <param name="ipe"></param>
        void tcpServerEngine_ClientDisconnected(System.Net.IPEndPoint ipe)
        {
            string msg = string.Format("{0} 下线", ipe);
            this.ShowEvent(msg);
        }

        /// <summary>
        /// 上线提醒
        /// </summary>
        /// <param name="ipe"></param>
        void tcpServerEngine_ClientConnected(System.Net.IPEndPoint ipe)
        {
            string msg = string.Format("{0} 上线", ipe);
            this.ShowEvent(msg);
        }


        void tcpServerEngine_ClientCountChanged(int count)
        {
            this.ShowConnectionCount(count);
        }

        private void ShowEvent(string msg)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new CbDelegate<string>(this.ShowEvent), msg);
            }
            else
            {
                this.toolStripLabel_event.Text = msg;
            }
        }

        /// <summary>
        /// 处理接受到的消息并分发对应人员
        /// </summary>
        /// <param name="client"></param>
        /// <param name="msg"></param>
        private void ShowClientMsg(IPEndPoint client, string msg)
        {
            if (this.InvokeRequired)
            {
                this.msgobj = UCTools.JsonToObject(msg);


                #region 客服
                if (this.msgobj.IsUser != true)
                {
                    if (this.msgobj.IsFirst == true)
                    {
                        this.ServiceIP = client;
                        this.msgobj.IsFirst = false;
                        this.BeginInvoke(new CbDelegate<IPEndPoint, string>(this.ShowClientMsg), client, "客服：" + this.msgobj.NickName + " 登录客服系统！");
                    }
                    else
                    {
                        this.BeginInvoke(new CbDelegate<IPEndPoint, string>(this.ShowClientMsg), client, msg);
                        string json = "";
                        this.msgobj.PairingServiceIP = client;
                        //发送对应人员
                        //private string jsonstr = "{\"NickID\":\"{0}\",\"NickName\":\"{1}\",\"Msg\":\"{2}\",\"IsFirst\":{3},\"IsUser\":{4},\"PairingServiceID\":\"{5}\",\"PairingUserID\": \"{6}\",\"PairingServiceIP\":\"{7}\",\"PairingUserIP\":\"{8}\"}";
                        json = "{" + string.Format("\"NickID\":\"{0}\",\"NickName\":\"{1}\",\"Msg\":\"{2}\",\"IsFirst\":\"{3}\",\"IsUser\":\"{4}\",\"PairingServiceID\":\"{5}\",\"PairingUserID\": \"{6}\",\"PairingServiceIP\":\"{7}\",\"PairingUserIP\":\"{8}\"",
                                                    this.msgobj.NickID,
                                                    this.msgobj.NickName,
                                                    this.msgobj.Msg,
                                                    this.msgobj.IsFirst,
                                                    this.msgobj.IsUser,
                                                    this.msgobj.PairingServiceID,
                                                    this.msgobj.PairingUserID,
                                                    this.msgobj.PairingServiceIP.ToString(),
                                                    this.msgobj.PairingUserIP.ToString()
                                                   ) + "}";

                        byte[] bMsg = Encoding.UTF8.GetBytes(json);//消息使用UTF-8编码
                        this.tcpServerEngine.SendMessageToClient(this.msgobj.PairingUserIP, bMsg);
                    }
                }
                #endregion

                #region 用户
                else if (this.ServiceIP != null)
                {
                    this.msgobj.PairingServiceIP = this.ServiceIP;
                    this.msgobj.PairingUserIP = client;


                    if (this.msgobj.IsFirst == false)
                    {
                        this.msgobj.PairingUserIP = client;
                        this.BeginInvoke(new CbDelegate<IPEndPoint, string>(this.ShowClientMsg), client, msg);
                        //发送对应人员
                        //private string jsonstr = "{\"NickID\":\"{0}\",\"NickName\":\"{1}\",\"Msg\":\"{2}\",\"IsFirst\":{3},\"IsUser\":{4},\"PairingServiceID\":\"{5}\",\"PairingUserID\": \"{6}\",\"PairingServiceIP\":\"{7}\",\"PairingUserIP\":\"{8}\"}";
                        string json = "{" + string.Format("\"NickID\":\"{0}\",\"NickName\":\"{1}\",\"Msg\":\"{2}\",\"IsFirst\":\"{3}\",\"IsUser\":\"{4}\",\"PairingServiceID\":\"{5}\",\"PairingUserID\": \"{6}\",\"PairingServiceIP\":\"{7}\",\"PairingUserIP\":\"{8}\"",
                                                    this.msgobj.NickID,
                                                    this.msgobj.NickName,
                                                    this.msgobj.Msg,
                                                    this.msgobj.IsFirst,
                                                    this.msgobj.IsUser,
                                                    this.msgobj.PairingServiceID,
                                                    this.msgobj.PairingUserID,
                                                    this.msgobj.PairingServiceIP.ToString(),
                                                    this.msgobj.PairingUserIP.ToString()
                                                   ) + "}";

                        byte[] bMsg = Encoding.UTF8.GetBytes(json);//消息使用UTF-8编码
                        this.tcpServerEngine.SendMessageToClient(this.msgobj.PairingServiceIP, bMsg);
                    }
                    else
                    {
                        this.BeginInvoke(new CbDelegate<IPEndPoint, string>(this.ShowClientMsg), client, "用户：" + this.msgobj.NickName + " 使用客服系统！");
                    }
                }
                #endregion

            }
            else
            {
                ListViewItem item = new ListViewItem(new string[] { DateTime.Now.ToString(), client.ToString(), msg });
                this.listView1.Items.Insert(0, item);
            }
        }


        private void ShowConnectionCount(int clientCount)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new CbDelegate<int>(this.ShowConnectionCount), clientCount);
            }
            else
            {
                this.toolStripLabel_clientCount.Text = "在线数量： " + clientCount.ToString();
            }
        }

        private void comboBox1_DropDown(object sender, EventArgs e)
        {

            List<IPEndPoint> list = this.tcpServerEngine.GetClientList();
            this.comboBox1.DataSource = list;
            //this.comboBox1.DataSource = userip.Values;
        }

        /// <summary>
        /// 选择指定ip发送消息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                IPEndPoint client = (IPEndPoint)this.comboBox1.SelectedItem;
                if (client == null)
                {
                    MessageBox.Show("没有选中任何在线客户端！");
                    return;
                }

                if (!this.tcpServerEngine.IsClientOnline(client))
                {
                    MessageBox.Show("目标客户端不在线！");
                    return;
                }

                string msg = this.textBox_msg.Text + "";// "\0" 表示一个消息的结尾
                byte[] bMsg = Encoding.UTF8.GetBytes(msg);//消息使用UTF-8编码
                this.tcpServerEngine.SendMessageToClient(client, bMsg);
            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.Message);
            }
        }

        /// <summary>
        /// 全体发送
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                if (this.tcpServerEngine.ClientCount > 0)
                {
                    List<IPEndPoint> lists = this.tcpServerEngine.GetClientList();
                    string msg = this.textBox_msg.Text + "";
                    byte[] bMsg = System.Text.Encoding.UTF8.GetBytes(msg);
                    foreach (IPEndPoint list in lists)
                    {
                        this.tcpServerEngine.PostMessageToClient(list, bMsg);
                    }
                }
            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.Message);
            }
        }






    }
}
