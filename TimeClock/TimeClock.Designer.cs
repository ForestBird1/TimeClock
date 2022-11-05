
namespace TimeClock
{
    partial class TimeClock
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TimeClock));
            this.btn_work_start = new System.Windows.Forms.Button();
            this.btn_work_end = new System.Windows.Forms.Button();
            this.btn_break_toggle = new System.Windows.Forms.Button();
            this.lb_message = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // btn_work_start
            // 
            this.btn_work_start.Enabled = false;
            this.btn_work_start.Font = new System.Drawing.Font("Maplestory", 35.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.btn_work_start.Location = new System.Drawing.Point(12, 12);
            this.btn_work_start.Name = "btn_work_start";
            this.btn_work_start.Size = new System.Drawing.Size(128, 100);
            this.btn_work_start.TabIndex = 0;
            this.btn_work_start.Text = "출근";
            this.btn_work_start.UseVisualStyleBackColor = true;
            this.btn_work_start.Click += new System.EventHandler(this.btn_work_start_Click);
            // 
            // btn_work_end
            // 
            this.btn_work_end.Enabled = false;
            this.btn_work_end.Font = new System.Drawing.Font("Maplestory", 35.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.btn_work_end.Location = new System.Drawing.Point(404, 12);
            this.btn_work_end.Name = "btn_work_end";
            this.btn_work_end.Size = new System.Drawing.Size(128, 100);
            this.btn_work_end.TabIndex = 1;
            this.btn_work_end.Text = "퇴근";
            this.btn_work_end.UseVisualStyleBackColor = true;
            this.btn_work_end.Click += new System.EventHandler(this.btn_work_end_Click);
            // 
            // btn_break_toggle
            // 
            this.btn_break_toggle.Enabled = false;
            this.btn_break_toggle.Font = new System.Drawing.Font("Maplestory", 24F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.btn_break_toggle.Location = new System.Drawing.Point(146, 30);
            this.btn_break_toggle.Name = "btn_break_toggle";
            this.btn_break_toggle.Size = new System.Drawing.Size(252, 73);
            this.btn_break_toggle.TabIndex = 2;
            this.btn_break_toggle.Text = "쉬는시간_시작";
            this.btn_break_toggle.UseVisualStyleBackColor = true;
            this.btn_break_toggle.Click += new System.EventHandler(this.btn_break_toggle_Click);
            // 
            // lb_message
            // 
            this.lb_message.AutoSize = true;
            this.lb_message.Font = new System.Drawing.Font("Maplestory", 20.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lb_message.Location = new System.Drawing.Point(12, 115);
            this.lb_message.Name = "lb_message";
            this.lb_message.Size = new System.Drawing.Size(87, 30);
            this.lb_message.TabIndex = 3;
            this.lb_message.Text = "label1";
            // 
            // TimeClock
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(545, 156);
            this.Controls.Add(this.lb_message);
            this.Controls.Add(this.btn_break_toggle);
            this.Controls.Add(this.btn_work_end);
            this.Controls.Add(this.btn_work_start);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "TimeClock";
            this.Text = "TimeClock";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btn_work_start;
        private System.Windows.Forms.Button btn_work_end;
        private System.Windows.Forms.Button btn_break_toggle;
        private System.Windows.Forms.Label lb_message;
    }
}

