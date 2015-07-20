using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Net.Sockets;
using StriveEngine.Core;
using StriveEngine.Tcp.Passive;

namespace StriveEngine.SimpleDemoClient
{
    /*
    * 更多实用组件请访问 www.oraycn.com 或 QQ：168757008。
    * 
    * ESFramework 强悍的通信框架、P2P框架、群集平台。OMCS 简单易用的网络语音视频框架。MFile 语音视频录制组件。StriveEngine 轻量级的通信引擎。
    */
    public partial class Form1 : Form
    {
        private ITcpPassiveEngine tcpPassiveEngine;
        public Form1()
        {
            InitializeComponent();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                //初始化并启动客户端引擎（TCP、文本协议）
                this.tcpPassiveEngine = NetworkEngineFactory.CreateTextTcpPassiveEngine(this.textBox_IP.Text, int.Parse(this.textBox_port.Text), new DefaultTextContractHelper("\0"));
                this.tcpPassiveEngine.MessageReceived += new CbDelegate<System.Net.IPEndPoint, byte[]>(tcpPassiveEngine_MessageReceived);
                this.tcpPassiveEngine.AutoReconnect = true;//启动掉线自动重连                
                this.tcpPassiveEngine.ConnectionInterrupted += new CbDelegate(tcpPassiveEngine_ConnectionInterrupted);
                this.tcpPassiveEngine.ConnectionRebuildSucceed += new CbDelegate(tcpPassiveEngine_ConnectionRebuildSucceed);
                this.tcpPassiveEngine.Initialize();

                this.button2.Enabled = true;
                this.button3.Enabled = false;
                MessageBox.Show("连接成功！");
            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.Message);
            }
        }

        void tcpPassiveEngine_ConnectionRebuildSucceed()
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new CbDelegate(this.tcpPassiveEngine_ConnectionInterrupted));
            }
            else
            {
                this.button2.Enabled = true;
                MessageBox.Show("重连成功。");
            }
        }

        void tcpPassiveEngine_ConnectionInterrupted()
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new CbDelegate(this.tcpPassiveEngine_ConnectionInterrupted));
            }
            else
            {
                this.button2.Enabled = false;
                MessageBox.Show("您已经掉线。");
            }
        }

        void tcpPassiveEngine_MessageReceived(System.Net.IPEndPoint serverIPE, byte[] bMsg)
        {
            string msg = System.Text.Encoding.UTF8.GetString(bMsg); //消息使用UTF-8编码
            msg = msg.Substring(0, msg.Length - 1); //将结束标记"\0"剔除
            this.ShowMessage(msg);
        }       

        private void ShowMessage(string msg)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new CbDelegate<string>(this.ShowMessage), msg);
            }
            else
            {
                ListViewItem item = new ListViewItem(new string[] { DateTime.Now.ToString(), msg });
                this.listView1.Items.Insert(0, item);                
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string msg = this.textBox_msg.Text + "\0";// "\0" 表示一个消息的结尾
            byte[] bMsg = System.Text.Encoding.UTF8.GetBytes(msg);//消息使用UTF-8编码
            this.tcpPassiveEngine.SendMessageToServer(bMsg);
        }
    }   
}
