using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Net.Sockets;

namespace StriveEngine.SimpleDemo
{
    /*
     * 本客户端Demo直接基于.NET的Socket进行开发，其目的是为了演示：在客户端不使用StriveEngine的情况下，如何与基于StriveEngine的服务端进行通信。
     * 本客户端Demo只是粗糙地实现了基本目的，很多细节问题都被忽略，像粘包问题、消息重组、掉线检测等等。而这些问题在实际的应用中，是必需要处理的。（当然，StriveEngine中的客户端和服务端引擎都内置解决了这些问题）
     * 
     */
    public partial class Form1 : Form
    {
        private NetworkStream stream;
        private byte[] buffer = new byte[10240];
        public Form1()
        {
            InitializeComponent();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                TcpClient client = new TcpClient();
                client.Connect(this.textBox_IP.Text, int.Parse(this.textBox_port.Text));
                this.stream = client.GetStream();
                this.stream.BeginRead(this.buffer, 0, this.buffer.Length, new AsyncCallback(this.ReceiveCallback), null);
                this.button2.Enabled = true;
                this.button3.Enabled = false;
                MessageBox.Show("连接成功！");
            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.Message);
            }
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                int bytesRead = this.stream.EndRead(ar);
                if (bytesRead == 0)
                {
                    return; //连接断开
                }

                string msg = System.Text.Encoding.UTF8.GetString(this.buffer, 0, bytesRead); //消息使用UTF-8编码
                msg = msg.Substring(0, msg.Length - 1); //将结束标记"\0"剔除
                this.ShowMessage(msg);

                this.stream.BeginRead(this.buffer, 0, this.buffer.Length, new AsyncCallback(this.ReceiveCallback), null);
            }
            catch (Exception ee)
            {
                //网路读取异常，又可能 连接断开
            }
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
            try
            {
                string msg = this.textBox_msg.Text + "\0";// "\0" 表示一个消息的结尾
                byte[] bMsg = System.Text.Encoding.UTF8.GetBytes(msg);//消息使用UTF-8编码
                this.stream.Write(bMsg, 0, bMsg.Length);
            }
            catch (Exception ee)
            {
                //网路读取异常，又可能 连接断开
                MessageBox.Show(ee.Message);
            }
        }
    }

    public delegate void CbDelegate<T>(T obj);
}
