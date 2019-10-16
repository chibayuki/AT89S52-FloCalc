namespace WinFormApp
{
    partial class Form_Main
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form_Main));
            this.Panel_Main = new System.Windows.Forms.Panel();
            this.Panel_Map = new System.Windows.Forms.Panel();
            this.Timer_AutoRepaint = new System.Windows.Forms.Timer(this.components);
            this.Panel_Main.SuspendLayout();
            this.SuspendLayout();
            // 
            // Panel_Main
            // 
            this.Panel_Main.BackColor = System.Drawing.Color.Transparent;
            this.Panel_Main.Controls.Add(this.Panel_Map);
            this.Panel_Main.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Panel_Main.Location = new System.Drawing.Point(0, 0);
            this.Panel_Main.Name = "Panel_Main";
            this.Panel_Main.Size = new System.Drawing.Size(385, 580);
            this.Panel_Main.TabIndex = 0;
            // 
            // Panel_Map
            // 
            this.Panel_Map.BackColor = System.Drawing.Color.Black;
            this.Panel_Map.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Panel_Map.Location = new System.Drawing.Point(0, 0);
            this.Panel_Map.Name = "Panel_Map";
            this.Panel_Map.Size = new System.Drawing.Size(385, 580);
            this.Panel_Map.TabIndex = 0;
            this.Panel_Map.Paint += new System.Windows.Forms.PaintEventHandler(this.Panel_Map_Paint);
            this.Panel_Map.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Panel_Map_MouseDown);
            this.Panel_Map.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Panel_Map_MouseUp);
            // 
            // Timer_AutoRepaint
            // 
            this.Timer_AutoRepaint.Interval = 10;
            this.Timer_AutoRepaint.Tick += new System.EventHandler(this.Timer_AutoRepaint_Tick);
            // 
            // Form_Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.ClientSize = new System.Drawing.Size(385, 580);
            this.Controls.Add(this.Panel_Main);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Form_Main";
            this.Opacity = 0D;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Panel_Main.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel Panel_Main;
        private System.Windows.Forms.Panel Panel_Map;
        private System.Windows.Forms.Timer Timer_AutoRepaint;
    }
}