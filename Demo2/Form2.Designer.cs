namespace Demo
{
    partial class Form2
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
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.hint = new System.Windows.Forms.Label();
            this.job_title_lb = new System.Windows.Forms.Label();
            this.department_lb = new System.Windows.Forms.Label();
            this.name_lb = new System.Windows.Forms.Label();
            this.label_time2 = new System.Windows.Forms.Label();
            this.label_time = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tableLayoutPanel1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.panel1, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1012, 592);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // panel1
            // 
            this.panel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.panel1.Controls.Add(this.textBox1);
            this.panel1.Controls.Add(this.hint);
            this.panel1.Controls.Add(this.job_title_lb);
            this.panel1.Controls.Add(this.department_lb);
            this.panel1.Controls.Add(this.name_lb);
            this.panel1.Controls.Add(this.label_time2);
            this.panel1.Controls.Add(this.label_time);
            this.panel1.Controls.Add(this.label5);
            this.panel1.Controls.Add(this.label4);
            this.panel1.Controls.Add(this.label3);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(3, 3);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1006, 586);
            this.panel1.TabIndex = 0;
            // 
            // textBox1
            // 
            this.textBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox1.Location = new System.Drawing.Point(882, 556);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(115, 21);
            this.textBox1.TabIndex = 11;
            this.textBox1.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBox1_KeyDown);
            // 
            // hint
            // 
            this.hint.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.hint.AutoSize = true;
            this.hint.Font = new System.Drawing.Font("黑体", 25F);
            this.hint.Location = new System.Drawing.Point(173, 482);
            this.hint.Name = "hint";
            this.hint.Size = new System.Drawing.Size(0, 34);
            this.hint.TabIndex = 10;
            // 
            // job_title_lb
            // 
            this.job_title_lb.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.job_title_lb.AutoSize = true;
            this.job_title_lb.Font = new System.Drawing.Font("黑体", 40F);
            this.job_title_lb.Location = new System.Drawing.Point(170, 403);
            this.job_title_lb.Name = "job_title_lb";
            this.job_title_lb.Size = new System.Drawing.Size(0, 54);
            this.job_title_lb.TabIndex = 9;
            // 
            // department_lb
            // 
            this.department_lb.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.department_lb.AutoSize = true;
            this.department_lb.Font = new System.Drawing.Font("黑体", 40F);
            this.department_lb.Location = new System.Drawing.Point(170, 334);
            this.department_lb.Name = "department_lb";
            this.department_lb.Size = new System.Drawing.Size(0, 54);
            this.department_lb.TabIndex = 8;
            // 
            // name_lb
            // 
            this.name_lb.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.name_lb.AutoSize = true;
            this.name_lb.Font = new System.Drawing.Font("黑体", 40F);
            this.name_lb.Location = new System.Drawing.Point(170, 265);
            this.name_lb.Name = "name_lb";
            this.name_lb.Size = new System.Drawing.Size(0, 54);
            this.name_lb.TabIndex = 7;
            // 
            // label_time2
            // 
            this.label_time2.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label_time2.AutoSize = true;
            this.label_time2.Font = new System.Drawing.Font("黑体", 40F);
            this.label_time2.Location = new System.Drawing.Point(9, 196);
            this.label_time2.Name = "label_time2";
            this.label_time2.Size = new System.Drawing.Size(0, 54);
            this.label_time2.TabIndex = 6;
            // 
            // label_time
            // 
            this.label_time.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label_time.AutoSize = true;
            this.label_time.Font = new System.Drawing.Font("黑体", 40F);
            this.label_time.Location = new System.Drawing.Point(9, 127);
            this.label_time.Name = "label_time";
            this.label_time.Size = new System.Drawing.Size(0, 54);
            this.label_time.TabIndex = 5;
            // 
            // label5
            // 
            this.label5.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("黑体", 40F);
            this.label5.Location = new System.Drawing.Point(9, 472);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(158, 54);
            this.label5.TabIndex = 4;
            this.label5.Text = "提示:";
            // 
            // label4
            // 
            this.label4.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("黑体", 40F);
            this.label4.Location = new System.Drawing.Point(9, 334);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(158, 54);
            this.label4.TabIndex = 3;
            this.label4.Text = "部门:";
            // 
            // label3
            // 
            this.label3.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("黑体", 40F);
            this.label3.Location = new System.Drawing.Point(9, 403);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(158, 54);
            this.label3.TabIndex = 2;
            this.label3.Text = "职称:";
            // 
            // label2
            // 
            this.label2.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("黑体", 40F);
            this.label2.Location = new System.Drawing.Point(9, 265);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(158, 54);
            this.label2.TabIndex = 1;
            this.label2.Text = "姓名:";
            // 
            // label1
            // 
            this.label1.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("黑体", 40F);
            this.label1.Location = new System.Drawing.Point(9, 58);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(752, 54);
            this.label1.TabIndex = 0;
            this.label1.Text = "富英华工贸有限公司 报餐系统";
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Interval = 1000;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(61, 4);
            // 
            // Form2
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(128)))));
            this.ClientSize = new System.Drawing.Size(1012, 592);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "Form2";
            this.Text = "Form2";
            this.Load += new System.EventHandler(this.Form2_Load);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label_time;
        private System.Windows.Forms.Label label_time2;
        private System.Windows.Forms.Label hint;
        private System.Windows.Forms.Label job_title_lb;
        private System.Windows.Forms.Label department_lb;
        private System.Windows.Forms.Label name_lb;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
    }
}