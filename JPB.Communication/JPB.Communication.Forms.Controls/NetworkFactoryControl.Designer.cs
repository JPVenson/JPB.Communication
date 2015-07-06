namespace JPB.Communication.Forms.Controls
{
    partial class NetworkFactoryControl
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.Sender = new System.Windows.Forms.DataGridView();
            this.Port = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Timeout = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Serilizer = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.SharedConnectionState = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ExternalIpUsageState = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Receiver = new System.Windows.Forms.DataGridView();
            this.PortC = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.AutoRespond = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.IncommingMessageState = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.LMSupportState = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.SerilizerC = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.DisposeState = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.Sender)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.Receiver)).BeginInit();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.Sender);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.Receiver);
            this.splitContainer1.Size = new System.Drawing.Size(649, 420);
            this.splitContainer1.SplitterDistance = 216;
            this.splitContainer1.TabIndex = 0;
            // 
            // Sender
            // 
            this.Sender.AllowUserToAddRows = false;
            this.Sender.AllowUserToDeleteRows = false;
            this.Sender.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.Sender.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Port,
            this.Timeout,
            this.Serilizer,
            this.SharedConnectionState,
            this.ExternalIpUsageState});
            this.Sender.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Sender.Location = new System.Drawing.Point(0, 0);
            this.Sender.MultiSelect = false;
            this.Sender.Name = "Sender";
            this.Sender.ReadOnly = true;
            this.Sender.Size = new System.Drawing.Size(649, 216);
            this.Sender.TabIndex = 0;
            // 
            // Port
            // 
            this.Port.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.ColumnHeader;
            this.Port.HeaderText = "Port";
            this.Port.Name = "Port";
            this.Port.ReadOnly = true;
            this.Port.Width = 51;
            // 
            // Timeout
            // 
            this.Timeout.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.ColumnHeader;
            this.Timeout.HeaderText = "Timeout";
            this.Timeout.Name = "Timeout";
            this.Timeout.ReadOnly = true;
            this.Timeout.Width = 70;
            // 
            // Serilizer
            // 
            this.Serilizer.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.ColumnHeader;
            this.Serilizer.HeaderText = "Serilizer";
            this.Serilizer.Name = "Serilizer";
            this.Serilizer.ReadOnly = true;
            this.Serilizer.Width = 68;
            // 
            // SharedConnectionState
            // 
            this.SharedConnectionState.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.SharedConnectionState.HeaderText = "Is Shared Connection";
            this.SharedConnectionState.Name = "SharedConnectionState";
            this.SharedConnectionState.ReadOnly = true;
            // 
            // ExternalIpUsageState
            // 
            this.ExternalIpUsageState.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.ExternalIpUsageState.HeaderText = "Use External Ip as Sender";
            this.ExternalIpUsageState.Name = "ExternalIpUsageState";
            this.ExternalIpUsageState.ReadOnly = true;
            // 
            // Receiver
            // 
            this.Receiver.AllowUserToAddRows = false;
            this.Receiver.AllowUserToDeleteRows = false;
            this.Receiver.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.Receiver.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.PortC,
            this.AutoRespond,
            this.IncommingMessageState,
            this.LMSupportState,
            this.SerilizerC,
            this.DisposeState});
            this.Receiver.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Receiver.Location = new System.Drawing.Point(0, 0);
            this.Receiver.Name = "Receiver";
            this.Receiver.ReadOnly = true;
            this.Receiver.Size = new System.Drawing.Size(649, 200);
            this.Receiver.TabIndex = 0;
            // 
            // PortC
            // 
            this.PortC.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.ColumnHeader;
            this.PortC.HeaderText = "Port";
            this.PortC.Name = "PortC";
            this.PortC.ReadOnly = true;
            this.PortC.Width = 51;
            // 
            // AutoRespond
            // 
            this.AutoRespond.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.ColumnHeader;
            this.AutoRespond.HeaderText = "Auto Respond";
            this.AutoRespond.Name = "AutoRespond";
            this.AutoRespond.ReadOnly = true;
            // 
            // IncommingMessageState
            // 
            this.IncommingMessageState.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.ColumnHeader;
            this.IncommingMessageState.HeaderText = "Incomming Message";
            this.IncommingMessageState.Name = "IncommingMessageState";
            this.IncommingMessageState.ReadOnly = true;
            this.IncommingMessageState.Width = 118;
            // 
            // LMSupportState
            // 
            this.LMSupportState.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.ColumnHeader;
            this.LMSupportState.HeaderText = "Large Message Support";
            this.LMSupportState.Name = "LMSupportState";
            this.LMSupportState.ReadOnly = true;
            this.LMSupportState.Width = 132;
            // 
            // SerilizerC
            // 
            this.SerilizerC.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.SerilizerC.HeaderText = "Serilizer";
            this.SerilizerC.Name = "SerilizerC";
            this.SerilizerC.ReadOnly = true;
            // 
            // DisposeState
            // 
            this.DisposeState.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.ColumnHeader;
            this.DisposeState.HeaderText = "Is Disposing";
            this.DisposeState.Name = "DisposeState";
            this.DisposeState.ReadOnly = true;
            this.DisposeState.Width = 82;
            // 
            // NetworkFactoryControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainer1);
            this.Name = "NetworkFactoryControl";
            this.Size = new System.Drawing.Size(649, 420);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.Sender)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.Receiver)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.DataGridView Sender;
        private System.Windows.Forms.DataGridViewTextBoxColumn Port;
        private System.Windows.Forms.DataGridViewTextBoxColumn Timeout;
        private System.Windows.Forms.DataGridViewTextBoxColumn Serilizer;
        private System.Windows.Forms.DataGridViewTextBoxColumn SharedConnectionState;
        private System.Windows.Forms.DataGridViewTextBoxColumn ExternalIpUsageState;
        private System.Windows.Forms.DataGridView Receiver;
        private System.Windows.Forms.DataGridViewTextBoxColumn PortC;
        private System.Windows.Forms.DataGridViewTextBoxColumn AutoRespond;
        private System.Windows.Forms.DataGridViewTextBoxColumn IncommingMessageState;
        private System.Windows.Forms.DataGridViewTextBoxColumn LMSupportState;
        private System.Windows.Forms.DataGridViewTextBoxColumn SerilizerC;
        private System.Windows.Forms.DataGridViewTextBoxColumn DisposeState;
    }
}
