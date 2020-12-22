
namespace OpreatingSystemClassDesign
{
    partial class MainForm
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.InputText = new System.Windows.Forms.TextBox();
            this.RandomGenerate = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.label2 = new System.Windows.Forms.Label();
            this.PageFaultTime = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.PageFaultTrack = new System.Windows.Forms.TrackBar();
            this.MemoryTrack = new System.Windows.Forms.TrackBar();
            this.label4 = new System.Windows.Forms.Label();
            this.MemoryTime = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.TLBTrack = new System.Windows.Forms.TrackBar();
            this.label6 = new System.Windows.Forms.Label();
            this.TLBTime = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.GeneraterNumTarck = new System.Windows.Forms.TrackBar();
            this.label10 = new System.Windows.Forms.Label();
            this.GeneraterNum = new System.Windows.Forms.TextBox();
            this.label11 = new System.Windows.Forms.Label();
            this.MemoryBlockTrack = new System.Windows.Forms.TrackBar();
            this.label12 = new System.Windows.Forms.Label();
            this.MemroyBlockNum = new System.Windows.Forms.TextBox();
            this.label13 = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.PageFaultTrack)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.MemoryTrack)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.TLBTrack)).BeginInit();
            this.groupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.GeneraterNumTarck)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.MemoryBlockTrack)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.InputText);
            this.groupBox1.Controls.Add(this.RandomGenerate);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Location = new System.Drawing.Point(12, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(841, 51);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "内容输入";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(125, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "输入序列或内存地址：";
            // 
            // InputText
            // 
            this.InputText.Location = new System.Drawing.Point(126, 19);
            this.InputText.Name = "InputText";
            this.InputText.Size = new System.Drawing.Size(624, 21);
            this.InputText.TabIndex = 1;
            // 
            // RandomGenerate
            // 
            this.RandomGenerate.Location = new System.Drawing.Point(754, 17);
            this.RandomGenerate.Name = "RandomGenerate";
            this.RandomGenerate.Size = new System.Drawing.Size(75, 23);
            this.RandomGenerate.TabIndex = 2;
            this.RandomGenerate.Text = "随机生成";
            this.RandomGenerate.UseVisualStyleBackColor = true;
            this.RandomGenerate.Click += new System.EventHandler(this.RandomGenerate_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.TLBTrack);
            this.groupBox2.Controls.Add(this.label6);
            this.groupBox2.Controls.Add(this.TLBTime);
            this.groupBox2.Controls.Add(this.label7);
            this.groupBox2.Controls.Add(this.MemoryTrack);
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Controls.Add(this.MemoryTime);
            this.groupBox2.Controls.Add(this.label5);
            this.groupBox2.Controls.Add(this.PageFaultTrack);
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Controls.Add(this.PageFaultTime);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Location = new System.Drawing.Point(859, 3);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(370, 138);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "时间设置";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 22);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 12);
            this.label2.TabIndex = 0;
            this.label2.Text = "断页时间";
            // 
            // PageFaultTime
            // 
            this.PageFaultTime.Location = new System.Drawing.Point(98, 17);
            this.PageFaultTime.Name = "PageFaultTime";
            this.PageFaultTime.Size = new System.Drawing.Size(33, 21);
            this.PageFaultTime.TabIndex = 1;
            this.PageFaultTime.Text = "500";
            this.PageFaultTime.TextChanged += new System.EventHandler(this.PageFaultTime_TextChanged);
            this.PageFaultTime.Leave += new System.EventHandler(this.PageFaultTime_Leave);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(137, 22);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(17, 12);
            this.label3.TabIndex = 2;
            this.label3.Text = "ms";
            // 
            // PageFaultTrack
            // 
            this.PageFaultTrack.LargeChange = 100;
            this.PageFaultTrack.Location = new System.Drawing.Point(159, 17);
            this.PageFaultTrack.Maximum = 2000;
            this.PageFaultTrack.Minimum = 500;
            this.PageFaultTrack.Name = "PageFaultTrack";
            this.PageFaultTrack.Size = new System.Drawing.Size(205, 45);
            this.PageFaultTrack.TabIndex = 3;
            this.PageFaultTrack.TickFrequency = 50;
            this.PageFaultTrack.TickStyle = System.Windows.Forms.TickStyle.None;
            this.PageFaultTrack.Value = 500;
            this.PageFaultTrack.Scroll += new System.EventHandler(this.PageFaultTrack_Scroll);
            // 
            // MemoryTrack
            // 
            this.MemoryTrack.Location = new System.Drawing.Point(159, 50);
            this.MemoryTrack.Maximum = 500;
            this.MemoryTrack.Minimum = 5;
            this.MemoryTrack.Name = "MemoryTrack";
            this.MemoryTrack.Size = new System.Drawing.Size(205, 45);
            this.MemoryTrack.TabIndex = 7;
            this.MemoryTrack.TickFrequency = 50;
            this.MemoryTrack.TickStyle = System.Windows.Forms.TickStyle.None;
            this.MemoryTrack.Value = 5;
            this.MemoryTrack.Scroll += new System.EventHandler(this.PageFaultTrack_Scroll);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(137, 55);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(17, 12);
            this.label4.TabIndex = 6;
            this.label4.Text = "ms";
            // 
            // MemoryTime
            // 
            this.MemoryTime.Location = new System.Drawing.Point(98, 50);
            this.MemoryTime.Name = "MemoryTime";
            this.MemoryTime.Size = new System.Drawing.Size(33, 21);
            this.MemoryTime.TabIndex = 5;
            this.MemoryTime.Text = "5";
            this.MemoryTime.TextChanged += new System.EventHandler(this.PageFaultTime_TextChanged);
            this.MemoryTime.Leave += new System.EventHandler(this.PageFaultTime_Leave);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(6, 55);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(77, 12);
            this.label5.TabIndex = 4;
            this.label5.Text = "内存读取时间";
            // 
            // TLBTrack
            // 
            this.TLBTrack.Location = new System.Drawing.Point(159, 84);
            this.TLBTrack.Maximum = 500;
            this.TLBTrack.Minimum = 5;
            this.TLBTrack.Name = "TLBTrack";
            this.TLBTrack.Size = new System.Drawing.Size(205, 45);
            this.TLBTrack.TabIndex = 11;
            this.TLBTrack.TickFrequency = 50;
            this.TLBTrack.TickStyle = System.Windows.Forms.TickStyle.None;
            this.TLBTrack.Value = 5;
            this.TLBTrack.Scroll += new System.EventHandler(this.PageFaultTrack_Scroll);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(137, 89);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(17, 12);
            this.label6.TabIndex = 10;
            this.label6.Text = "ms";
            // 
            // TLBTime
            // 
            this.TLBTime.Location = new System.Drawing.Point(98, 84);
            this.TLBTime.Name = "TLBTime";
            this.TLBTime.Size = new System.Drawing.Size(33, 21);
            this.TLBTime.TabIndex = 9;
            this.TLBTime.Text = "5";
            this.TLBTime.TextChanged += new System.EventHandler(this.PageFaultTime_TextChanged);
            this.TLBTime.Leave += new System.EventHandler(this.PageFaultTime_Leave);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(6, 89);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(77, 12);
            this.label7.TabIndex = 8;
            this.label7.Text = "快表读取时间";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.GeneraterNumTarck);
            this.groupBox3.Controls.Add(this.label10);
            this.groupBox3.Controls.Add(this.GeneraterNum);
            this.groupBox3.Controls.Add(this.label11);
            this.groupBox3.Controls.Add(this.MemoryBlockTrack);
            this.groupBox3.Controls.Add(this.label12);
            this.groupBox3.Controls.Add(this.MemroyBlockNum);
            this.groupBox3.Controls.Add(this.label13);
            this.groupBox3.Location = new System.Drawing.Point(859, 147);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(370, 97);
            this.groupBox3.TabIndex = 12;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "杂项设定";
            // 
            // GeneraterNumTarck
            // 
            this.GeneraterNumTarck.Location = new System.Drawing.Point(159, 50);
            this.GeneraterNumTarck.Maximum = 50;
            this.GeneraterNumTarck.Minimum = 5;
            this.GeneraterNumTarck.Name = "GeneraterNumTarck";
            this.GeneraterNumTarck.Size = new System.Drawing.Size(205, 45);
            this.GeneraterNumTarck.TabIndex = 7;
            this.GeneraterNumTarck.TickFrequency = 50;
            this.GeneraterNumTarck.TickStyle = System.Windows.Forms.TickStyle.None;
            this.GeneraterNumTarck.Value = 5;
            this.GeneraterNumTarck.Scroll += new System.EventHandler(this.PageFaultTrack_Scroll);
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(137, 55);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(17, 12);
            this.label10.TabIndex = 6;
            this.label10.Text = "个";
            // 
            // GeneraterNum
            // 
            this.GeneraterNum.Location = new System.Drawing.Point(98, 50);
            this.GeneraterNum.Name = "GeneraterNum";
            this.GeneraterNum.Size = new System.Drawing.Size(33, 21);
            this.GeneraterNum.TabIndex = 5;
            this.GeneraterNum.Text = "5";
            this.GeneraterNum.TextChanged += new System.EventHandler(this.PageFaultTime_TextChanged);
            this.GeneraterNum.Leave += new System.EventHandler(this.PageFaultTime_Leave);
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(6, 55);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(77, 12);
            this.label11.TabIndex = 4;
            this.label11.Text = "生成序列个数";
            // 
            // MemoryBlockTrack
            // 
            this.MemoryBlockTrack.LargeChange = 100;
            this.MemoryBlockTrack.Location = new System.Drawing.Point(159, 17);
            this.MemoryBlockTrack.Minimum = 3;
            this.MemoryBlockTrack.Name = "MemoryBlockTrack";
            this.MemoryBlockTrack.Size = new System.Drawing.Size(205, 45);
            this.MemoryBlockTrack.TabIndex = 3;
            this.MemoryBlockTrack.TickFrequency = 50;
            this.MemoryBlockTrack.TickStyle = System.Windows.Forms.TickStyle.None;
            this.MemoryBlockTrack.Value = 4;
            this.MemoryBlockTrack.Scroll += new System.EventHandler(this.PageFaultTrack_Scroll);
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(137, 22);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(17, 12);
            this.label12.TabIndex = 2;
            this.label12.Text = "个";
            // 
            // MemroyBlockNum
            // 
            this.MemroyBlockNum.Location = new System.Drawing.Point(98, 17);
            this.MemroyBlockNum.Name = "MemroyBlockNum";
            this.MemroyBlockNum.Size = new System.Drawing.Size(33, 21);
            this.MemroyBlockNum.TabIndex = 1;
            this.MemroyBlockNum.Text = "4";
            this.MemroyBlockNum.TextChanged += new System.EventHandler(this.PageFaultTime_TextChanged);
            this.MemroyBlockNum.Leave += new System.EventHandler(this.PageFaultTime_Leave);
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(6, 22);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(89, 12);
            this.label13.TabIndex = 0;
            this.label13.Text = "驻留内存页块数";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1241, 610);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "MainForm";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.PageFaultTrack)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.MemoryTrack)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.TLBTrack)).EndInit();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.GeneraterNumTarck)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.MemoryBlockTrack)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TextBox InputText;
        private System.Windows.Forms.Button RandomGenerate;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.TrackBar TLBTrack;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox TLBTime;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TrackBar MemoryTrack;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox MemoryTime;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TrackBar PageFaultTrack;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox PageFaultTime;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.TrackBar GeneraterNumTarck;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.TextBox GeneraterNum;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.TrackBar MemoryBlockTrack;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.TextBox MemroyBlockNum;
        private System.Windows.Forms.Label label13;
    }
}