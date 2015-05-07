using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using JPB.Communication.ComBase;
using JPB.Communication.ComBase.Messages;
using JPB.Communication.ComBase.Messages.Wrapper;
using JPB.Communication.ComBase.TCP;
using JPB.Communication.Forms.Controls.Model;

namespace JPB.Communication.Forms.Controls
{
    public partial class NetworkLogControl : UserControl
    {
        public NetworkLogControl()
        {
            InitializeComponent();

            Networkbase.OnIncommingMessage += Networkbase_OnIncommingMessage;
            Networkbase.OnMessageSend += Networkbase_OnMessageSend;
            Networkbase.OnNewItemLoadedFail += Networkbase_OnNewItemLoadedFail;
            Networkbase.OnNewItemLoadedSuccess += Networkbase_OnNewItemLoadedSuccess;
            Networkbase.OnNewLargeItemLoadedSuccess += Networkbase_OnNewLargeItemLoadedSuccess;

            NetworkFactory.Instance.OnReceiverCreate += Instance_OnReceiverCreate;
            NetworkFactory.Instance.OnSenderCreate += InstanceOnOnSenderCreate;

            ConnectionPool.Instance.OnConnectionCreated += Instance_OnConnectionCreated;
            ConnectionPool.Instance.OnConnectionClosed += InstanceOnOnConnectionClosed;
        }


        private void InstanceOnOnConnectionClosed(object sender, ConnectionWrapper connectionWrapper)
        {
            this.BeginInvoke(new Action(() =>
            {
                TcpNetworkActionLog.Rows.Add(new TcpNetworkAction(TcpNetworkActionType.ConnectionClosed, new { Sender = sender, Source = connectionWrapper }));
            }));
        }

        void Instance_OnConnectionCreated(object sender, ConnectionWrapper connectionWrapper)
        {
            this.BeginInvoke(new Action(() =>
            {
                TcpNetworkActionLog.Rows.Add(new TcpNetworkAction(TcpNetworkActionType.ConnectionOpen, new { Sender = sender, Source = connectionWrapper }));
            }));
        }

        private void InstanceOnOnSenderCreate(object sender, GenericNetworkSender tcpNetworkSender)
        {
            this.BeginInvoke(new Action(() =>
            {
                TcpNetworkActionLog.Rows.Add(new TcpNetworkAction(TcpNetworkActionType.InitSender, new { Sender = sender, Source = tcpNetworkSender }));
            }));
        }

        void Instance_OnReceiverCreate(object sender, GenericNetworkReceiver tcpNetworkReceiver)
        {
            this.BeginInvoke(new Action(() =>
            {
                TcpNetworkActionLog.Rows.Add(new TcpNetworkAction(TcpNetworkActionType.InitReceiver, new { Sender = sender, Source = tcpNetworkReceiver }));
            }));
        }

        void Networkbase_OnNewLargeItemLoadedSuccess(LargeMessage mess, ushort port)
        {
            this.BeginInvoke(new Action(() =>
            {
                TcpNetworkActionLog.Rows.Add(new TcpNetworkAction(TcpNetworkActionType.LoadLargeSuccess, new { Port = port, Message = mess }));
            }));
        }

        void Networkbase_OnNewItemLoadedSuccess(MessageBase mess, ushort port)
        {
            this.BeginInvoke(new Action(() =>
            {
                TcpNetworkActionLog.Rows.Add(new TcpNetworkAction(TcpNetworkActionType.LoadSuccess, new { Port = port, Message = mess }));
            }));
        }

        void Networkbase_OnNewItemLoadedFail(object sender, string e)
        {
            this.BeginInvoke(new Action(() =>
            {
                TcpNetworkActionLog.Rows.Add(new TcpNetworkAction(TcpNetworkActionType.LoadFail, new { (sender as Networkbase).Port, Message = e }));
            }));
        }

        void Networkbase_OnMessageSend(MessageBase mess, ushort port)
        {
            this.BeginInvoke(new Action(() =>
            {
                TcpNetworkActionLog.Rows.Add(new TcpNetworkAction(TcpNetworkActionType.Send, new { Port = port, Message = mess }));
            }));
        }

        void Networkbase_OnIncommingMessage(object sender, MessageBase e)
        {
            this.BeginInvoke(new Action(() =>
            {
                TcpNetworkActionLog.Rows.Add(new TcpNetworkAction(TcpNetworkActionType.Incomming, new { (sender as Networkbase).Port, Message = e }));
            }));
        }
        
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            splitContainer2.Panel2Collapsed = !splitContainer2.Panel2Collapsed;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            TcpNetworkActionLog.Rows.Clear();
        }

        private void TcpNetworkActionLog_SelectionChanged(object sender, EventArgs e)
        {
            var tcpNetworkAction = this.TcpNetworkActionLog.SelectedRows.Cast<TcpNetworkAction>().FirstOrDefault<TcpNetworkAction>();
            if (tcpNetworkAction != null)
            {
                this.typeLabel.Text = tcpNetworkAction.TcpNetworkActionType.ToString();
            }
        }
    }
}
