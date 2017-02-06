
namespace HID_PnP_Demo
{
    partial class Form1
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.ToggleLedButton = new System.Windows.Forms.Button();
            this.AnalogLabel = new System.Windows.Forms.Label();
            this.StatusText = new System.Windows.Forms.Label();
            this.AnalogBar = new System.Windows.Forms.ProgressBar();
            //this.ReadWriteThread = new System.ComponentModel.BackgroundWorker();
            this.FormUpdateTimer = new System.Windows.Forms.Timer(this.components);
            this.ANxVoltageToolTip = new System.Windows.Forms.ToolTip(this.components);
            this.ToggleLEDToolTip = new System.Windows.Forms.ToolTip(this.components);
            this.PushbuttonStateTooltip = new System.Windows.Forms.ToolTip(this.components);
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.toolTip2 = new System.Windows.Forms.ToolTip(this.components);
            this.LogTextBox = new System.Windows.Forms.TextBox();
            this.LogLabel = new System.Windows.Forms.Label();
            this.VidLabel = new System.Windows.Forms.Label();
            this.VidTextBox = new System.Windows.Forms.TextBox();
            this.PidLabel = new System.Windows.Forms.Label();
            this.PidTextBox = new System.Windows.Forms.TextBox();
            this.DeviceLabel = new System.Windows.Forms.Label();
            this.PushbuttonText = new System.Windows.Forms.Label();
            this.DevicesLabel = new System.Windows.Forms.Label();
            this.DevicesTextBox = new System.Windows.Forms.TextBox();
            this.HorizontalBar1 = new System.Windows.Forms.Label();
            this.HorizontalBar2 = new System.Windows.Forms.Label();
            this.HorizontalBar3 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.TxCountText = new System.Windows.Forms.Label();
            this.RxCountText = new System.Windows.Forms.Label();
            this.UptimeText = new System.Windows.Forms.Label();
            this.RxSpeedText = new System.Windows.Forms.Label();
            this.TxSpeedText = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // ToggleLedButton
            // 
            this.ToggleLedButton.Enabled = false;
            this.ToggleLedButton.Location = new System.Drawing.Point(15, 464);
            this.ToggleLedButton.Name = "ToggleLedButton";
            this.ToggleLedButton.Size = new System.Drawing.Size(96, 23);
            this.ToggleLedButton.TabIndex = 24;
            this.ToggleLedButton.Text = "Toggle LED";
            this.ToggleLedButton.UseVisualStyleBackColor = true;
            this.ToggleLedButton.Click += new System.EventHandler(this.ToggleLedButton_Click);
            // 
            // AnalogLabel
            // 
            this.AnalogLabel.AutoSize = true;
            this.AnalogLabel.Enabled = false;
            this.AnalogLabel.Location = new System.Drawing.Point(12, 506);
            this.AnalogLabel.Name = "AnalogLabel";
            this.AnalogLabel.Size = new System.Drawing.Size(107, 13);
            this.AnalogLabel.TabIndex = 23;
            this.AnalogLabel.Text = "Analog Measurement";
            // 
            // StatusText
            // 
            this.StatusText.AutoSize = true;
            this.StatusText.Location = new System.Drawing.Point(12, 337);
            this.StatusText.Name = "StatusText";
            this.StatusText.Size = new System.Drawing.Size(171, 13);
            this.StatusText.TabIndex = 22;
            this.StatusText.Text = "Connection Status: Not connected";
            // 
            // AnalogBar
            // 
            this.AnalogBar.BackColor = System.Drawing.Color.White;
            this.AnalogBar.ForeColor = System.Drawing.Color.Red;
            this.AnalogBar.Location = new System.Drawing.Point(12, 522);
            this.AnalogBar.Maximum = 1024;
            this.AnalogBar.Name = "AnalogBar";
            this.AnalogBar.Size = new System.Drawing.Size(298, 23);
            this.AnalogBar.Step = 1;
            this.AnalogBar.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.AnalogBar.TabIndex = 20;
            // 
            // ReadWriteThread
            // 
            //this.ReadWriteThread.WorkerReportsProgress = true;
            //this.ReadWriteThread.DoWork += new System.ComponentModel.DoWorkEventHandler(this.ReadWriteThread_DoWork);
            // 
            // FormUpdateTimer
            // 
            this.FormUpdateTimer.Enabled = true;
            this.FormUpdateTimer.Interval = 6;
            this.FormUpdateTimer.Tick += new System.EventHandler(this.FormUpdateTimer_Tick);
            // 
            // ANxVoltageToolTip
            // 
            this.ANxVoltageToolTip.AutomaticDelay = 20;
            this.ANxVoltageToolTip.AutoPopDelay = 20000;
            this.ANxVoltageToolTip.InitialDelay = 15;
            this.ANxVoltageToolTip.ReshowDelay = 15;
            // 
            // ToggleLEDToolTip
            // 
            this.ToggleLEDToolTip.AutomaticDelay = 2000;
            this.ToggleLEDToolTip.AutoPopDelay = 20000;
            this.ToggleLEDToolTip.InitialDelay = 15;
            this.ToggleLEDToolTip.ReshowDelay = 15;
            // 
            // PushbuttonStateTooltip
            // 
            this.PushbuttonStateTooltip.AutomaticDelay = 20;
            this.PushbuttonStateTooltip.AutoPopDelay = 20000;
            this.PushbuttonStateTooltip.InitialDelay = 15;
            this.PushbuttonStateTooltip.ReshowDelay = 15;
            // 
            // toolTip1
            // 
            this.toolTip1.AutomaticDelay = 2000;
            this.toolTip1.AutoPopDelay = 20000;
            this.toolTip1.InitialDelay = 15;
            this.toolTip1.ReshowDelay = 15;
            // 
            // toolTip2
            // 
            this.toolTip2.AutomaticDelay = 20;
            this.toolTip2.AutoPopDelay = 20000;
            this.toolTip2.InitialDelay = 15;
            this.toolTip2.ReshowDelay = 15;
            // 
            // LogTextBox
            // 
            this.LogTextBox.BackColor = System.Drawing.SystemColors.Window;
            this.LogTextBox.Location = new System.Drawing.Point(15, 598);
            this.LogTextBox.Multiline = true;
            this.LogTextBox.Name = "LogTextBox";
            this.LogTextBox.ReadOnly = true;
            this.LogTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.LogTextBox.Size = new System.Drawing.Size(710, 158);
            this.LogTextBox.TabIndex = 26;
            // 
            // LogLabel
            // 
            this.LogLabel.AutoSize = true;
            this.LogLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LogLabel.Location = new System.Drawing.Point(12, 579);
            this.LogLabel.Name = "LogLabel";
            this.LogLabel.Size = new System.Drawing.Size(70, 13);
            this.LogLabel.TabIndex = 27;
            this.LogLabel.Text = "ActivityLog";
            // 
            // VidLabel
            // 
            this.VidLabel.AutoSize = true;
            this.VidLabel.Location = new System.Drawing.Point(12, 29);
            this.VidLabel.Name = "VidLabel";
            this.VidLabel.Size = new System.Drawing.Size(55, 13);
            this.VidLabel.TabIndex = 29;
            this.VidLabel.Text = "Vendor ID";
            // 
            // VidTextBox
            // 
            this.VidTextBox.AcceptsReturn = true;
            this.VidTextBox.BackColor = System.Drawing.SystemColors.Window;
            this.VidTextBox.Location = new System.Drawing.Point(15, 45);
            this.VidTextBox.MaxLength = 6;
            this.VidTextBox.Name = "VidTextBox";
            this.VidTextBox.Size = new System.Drawing.Size(55, 20);
            this.VidTextBox.TabIndex = 28;
            this.VidTextBox.Text = "0x0000";
            this.VidTextBox.KeyUp += new System.Windows.Forms.KeyEventHandler(this.VidTextBox_KeyUp);
            this.VidTextBox.LostFocus += new System.EventHandler(this.VidTextBox_LostFocus);
            // 
            // PidLabel
            // 
            this.PidLabel.AutoSize = true;
            this.PidLabel.Location = new System.Drawing.Point(91, 29);
            this.PidLabel.Name = "PidLabel";
            this.PidLabel.Size = new System.Drawing.Size(55, 13);
            this.PidLabel.TabIndex = 31;
            this.PidLabel.Text = "Device ID";
            // 
            // PidTextBox
            // 
            this.PidTextBox.AcceptsReturn = true;
            this.PidTextBox.BackColor = System.Drawing.SystemColors.Window;
            this.PidTextBox.Location = new System.Drawing.Point(94, 45);
            this.PidTextBox.MaxLength = 6;
            this.PidTextBox.Name = "PidTextBox";
            this.PidTextBox.Size = new System.Drawing.Size(55, 20);
            this.PidTextBox.TabIndex = 30;
            this.PidTextBox.Text = "0x0000"; 
            this.PidTextBox.KeyUp += new System.Windows.Forms.KeyEventHandler(this.PidTextBox_KeyUp);
            this.PidTextBox.LostFocus += new System.EventHandler(this.PidTextBox_LostFocus);
            // 
            // DeviceLabel
            // 
            this.DeviceLabel.AutoSize = true;
            this.DeviceLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.DeviceLabel.Location = new System.Drawing.Point(12, 9);
            this.DeviceLabel.Name = "DeviceLabel";
            this.DeviceLabel.Size = new System.Drawing.Size(212, 13);
            this.DeviceLabel.TabIndex = 32;
            this.DeviceLabel.Text = "Vendor ID and Device ID to look for";
            // 
            // PushbuttonText
            // 
            this.PushbuttonText.AutoSize = true;
            this.PushbuttonText.Location = new System.Drawing.Point(12, 432);
            this.PushbuttonText.Name = "PushbuttonText";
            this.PushbuttonText.Size = new System.Drawing.Size(141, 13);
            this.PushbuttonText.TabIndex = 33;
            this.PushbuttonText.Text = "Pushbutton State: Unknown";
            // 
            // DevicesLabel
            // 
            this.DevicesLabel.AutoSize = true;
            this.DevicesLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.DevicesLabel.Location = new System.Drawing.Point(12, 94);
            this.DevicesLabel.Name = "DevicesLabel";
            this.DevicesLabel.Size = new System.Drawing.Size(108, 13);
            this.DevicesLabel.TabIndex = 34;
            this.DevicesLabel.Text = "Attached Devices";
            // 
            // DevicesTextBox
            // 
            this.DevicesTextBox.BackColor = System.Drawing.SystemColors.Window;
            this.DevicesTextBox.Location = new System.Drawing.Point(15, 116);
            this.DevicesTextBox.Multiline = true;
            this.DevicesTextBox.Name = "DevicesTextBox";
            this.DevicesTextBox.ReadOnly = true;
            this.DevicesTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.DevicesTextBox.Size = new System.Drawing.Size(710, 162);
            this.DevicesTextBox.TabIndex = 35;
            // 
            // HorizontalBar1
            // 
            this.HorizontalBar1.AutoSize = true;
            this.HorizontalBar1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.HorizontalBar1.Enabled = false;
            this.HorizontalBar1.Font = new System.Drawing.Font("Microsoft Sans Serif", 1F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.HorizontalBar1.Location = new System.Drawing.Point(15, 78);
            this.HorizontalBar1.Margin = new System.Windows.Forms.Padding(0);
            this.HorizontalBar1.MinimumSize = new System.Drawing.Size(710, 1);
            this.HorizontalBar1.Name = "HorizontalBar1";
            this.HorizontalBar1.Size = new System.Drawing.Size(710, 4);
            this.HorizontalBar1.TabIndex = 36;
            // 
            // HorizontalBar2
            // 
            this.HorizontalBar2.AutoSize = true;
            this.HorizontalBar2.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.HorizontalBar2.Enabled = false;
            this.HorizontalBar2.Font = new System.Drawing.Font("Microsoft Sans Serif", 1F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.HorizontalBar2.Location = new System.Drawing.Point(15, 294);
            this.HorizontalBar2.Margin = new System.Windows.Forms.Padding(0);
            this.HorizontalBar2.MinimumSize = new System.Drawing.Size(710, 1);
            this.HorizontalBar2.Name = "HorizontalBar2";
            this.HorizontalBar2.Size = new System.Drawing.Size(710, 4);
            this.HorizontalBar2.TabIndex = 37;
            // 
            // HorizontalBar3
            // 
            this.HorizontalBar3.AutoSize = true;
            this.HorizontalBar3.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.HorizontalBar3.Enabled = false;
            this.HorizontalBar3.Font = new System.Drawing.Font("Microsoft Sans Serif", 1F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.HorizontalBar3.Location = new System.Drawing.Point(15, 563);
            this.HorizontalBar3.Margin = new System.Windows.Forms.Padding(0);
            this.HorizontalBar3.MinimumSize = new System.Drawing.Size(710, 1);
            this.HorizontalBar3.Name = "HorizontalBar3";
            this.HorizontalBar3.Size = new System.Drawing.Size(710, 4);
            this.HorizontalBar3.TabIndex = 38;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(12, 312);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(92, 13);
            this.label3.TabIndex = 39;
            this.label3.Text = "Communication";
            // 
            // TxCountText
            // 
            this.TxCountText.AutoSize = true;
            this.TxCountText.Location = new System.Drawing.Point(12, 384);
            this.TxCountText.Name = "TxCountText";
            this.TxCountText.Size = new System.Drawing.Size(122, 13);
            this.TxCountText.TabIndex = 40;
            this.TxCountText.Text = "Packets sent/failed: 0/0";
            // 
            // RxCountText
            // 
            this.RxCountText.AutoSize = true;
            this.RxCountText.Location = new System.Drawing.Point(285, 384);
            this.RxCountText.Name = "RxCountText";
            this.RxCountText.Size = new System.Drawing.Size(143, 13);
            this.RxCountText.TabIndex = 41;
            this.RxCountText.Text = "Packets received/failed: 0/0";
            // 
            // UptimeText
            // 
            this.UptimeText.AutoSize = true;
            this.UptimeText.Location = new System.Drawing.Point(12, 360);
            this.UptimeText.Name = "UptimeText";
            this.UptimeText.Size = new System.Drawing.Size(52, 13);
            this.UptimeText.TabIndex = 42;
            this.UptimeText.Text = "Uptime: 0";
            // 
            // RxSpeedText
            // 
            this.RxSpeedText.AutoSize = true;
            this.RxSpeedText.Location = new System.Drawing.Point(285, 409);
            this.RxSpeedText.Name = "RxSpeedText";
            this.RxSpeedText.Size = new System.Drawing.Size(82, 13);
            this.RxSpeedText.TabIndex = 44;
            this.RxSpeedText.Text = "RX Speed: N/A";
            // 
            // TxSpeedText
            // 
            this.TxSpeedText.AutoSize = true;
            this.TxSpeedText.Location = new System.Drawing.Point(12, 409);
            this.TxSpeedText.Name = "TxSpeedText";
            this.TxSpeedText.Size = new System.Drawing.Size(81, 13);
            this.TxSpeedText.TabIndex = 43;
            this.TxSpeedText.Text = "TX Speed: N/A";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(734, 768);
            this.Controls.Add(this.RxSpeedText);
            this.Controls.Add(this.TxSpeedText);
            this.Controls.Add(this.UptimeText);
            this.Controls.Add(this.RxCountText);
            this.Controls.Add(this.TxCountText);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.HorizontalBar3);
            this.Controls.Add(this.HorizontalBar2);
            this.Controls.Add(this.HorizontalBar1);
            this.Controls.Add(this.DevicesTextBox);
            this.Controls.Add(this.DevicesLabel);
            this.Controls.Add(this.PushbuttonText);
            this.Controls.Add(this.DeviceLabel);
            this.Controls.Add(this.PidLabel);
            this.Controls.Add(this.PidTextBox);
            this.Controls.Add(this.VidLabel);
            this.Controls.Add(this.VidTextBox);
            this.Controls.Add(this.LogLabel);
            this.Controls.Add(this.LogTextBox);
            this.Controls.Add(this.ToggleLedButton);
            this.Controls.Add(this.AnalogLabel);
            this.Controls.Add(this.StatusText);
            this.Controls.Add(this.AnalogBar);
            this.Name = "Form1";
            this.Text = "HID PnP Demo";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button ToggleLedButton;
        private System.Windows.Forms.Label AnalogLabel;
        private System.Windows.Forms.Label StatusText;
        private System.Windows.Forms.ProgressBar AnalogBar;
        //private System.ComponentModel.BackgroundWorker ReadWriteThread;
        private System.Windows.Forms.Timer FormUpdateTimer;
        private System.Windows.Forms.ToolTip ANxVoltageToolTip;
        private System.Windows.Forms.ToolTip ToggleLEDToolTip;
        private System.Windows.Forms.ToolTip PushbuttonStateTooltip;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.ToolTip toolTip2;
        private System.Windows.Forms.TextBox LogTextBox;
        private System.Windows.Forms.Label LogLabel;
        private System.Windows.Forms.Label VidLabel;
        private System.Windows.Forms.TextBox VidTextBox;
        private System.Windows.Forms.Label PidLabel;
        private System.Windows.Forms.TextBox PidTextBox;
        private System.Windows.Forms.Label DeviceLabel;
        private System.Windows.Forms.Label PushbuttonText;
        private System.Windows.Forms.Label DevicesLabel;
        private System.Windows.Forms.TextBox DevicesTextBox;
        private System.Windows.Forms.Label HorizontalBar1;
        private System.Windows.Forms.Label HorizontalBar2;
        private System.Windows.Forms.Label HorizontalBar3;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label TxCountText;
        private System.Windows.Forms.Label RxCountText;
        private System.Windows.Forms.Label UptimeText;
        private System.Windows.Forms.Label RxSpeedText;
        private System.Windows.Forms.Label TxSpeedText;
    }
}

