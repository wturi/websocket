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

namespace StriveEngine.SimpleDemoServer
{
    /*
     * 更多实用组件请访问 www.oraycn.com 或 QQ：168757008。
     * 
     * ESFramework 强悍的通信框架、P2P框架、群集平台。OMCS 简单易用的网络语音视频框架。MFile 语音视频录制组件。StriveEngine 轻量级的通信引擎。
     */
    public partial class Form1 : Form
    {
        private ITcpServerEngine tcpServerEngine;
        public Form1()
        {
            InitializeComponent();
        }

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

        void tcpServerEngine_MessageReceived(IPEndPoint client, byte[] bMsg)
        {
            string msg = System.Text.Encoding.UTF8.GetString(bMsg); //消息使用UTF-8编码
            msg = msg.Substring(0, msg.Length - 1); //将结束标记"\0"剔除
            this.ShowClientMsg(client, msg);
        }

        void tcpServerEngine_ClientDisconnected(System.Net.IPEndPoint ipe)
        {
            string msg = string.Format("{0} 下线", ipe);
            this.ShowEvent(msg);
        }

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

        private void ShowClientMsg(IPEndPoint client, string msg)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new CbDelegate<IPEndPoint, string>(this.ShowClientMsg), client, msg);
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
        }

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

                string msg = this.textBox_msg.Text + "\0";// "\0" 表示一个消息的结尾
                byte[] bMsg = System.Text.Encoding.UTF8.GetBytes(msg);//消息使用UTF-8编码
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
                    string msg = this.textBox_msg.Text + "\0";
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
