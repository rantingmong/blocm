namespace viewm
{
	partial class FormMain
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
            this.contextMenuOpenWorld = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.menuItemHeader = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
            this.panel2 = new System.Windows.Forms.Panel();
            this.labelStatus = new System.Windows.Forms.Label();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.buttonOpen = new System.Windows.Forms.Button();
            this.contextMenuOpenWorld.SuspendLayout();
            this.panel2.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // contextMenuOpenWorld
            // 
            this.contextMenuOpenWorld.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuItemHeader,
            this.toolStripMenuItem2});
            this.contextMenuOpenWorld.Name = "contextMenuOpenWorld";
            this.contextMenuOpenWorld.Size = new System.Drawing.Size(139, 32);
            // 
            // menuItemHeader
            // 
            this.menuItemHeader.Enabled = false;
            this.menuItemHeader.Name = "menuItemHeader";
            this.menuItemHeader.Size = new System.Drawing.Size(138, 22);
            this.menuItemHeader.Text = "Select world";
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(135, 6);
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.Color.SteelBlue;
            this.panel2.Controls.Add(this.labelStatus);
            this.panel2.Controls.Add(this.progressBar);
            this.panel2.Location = new System.Drawing.Point(57, 132);
            this.panel2.Margin = new System.Windows.Forms.Padding(0);
            this.panel2.Name = "panel2";
            this.panel2.Padding = new System.Windows.Forms.Padding(5);
            this.panel2.Size = new System.Drawing.Size(352, 192);
            this.panel2.TabIndex = 1;
            // 
            // labelStatus
            // 
            this.labelStatus.Dock = System.Windows.Forms.DockStyle.Left;
            this.labelStatus.ForeColor = System.Drawing.Color.White;
            this.labelStatus.Location = new System.Drawing.Point(5, 5);
            this.labelStatus.Name = "labelStatus";
            this.labelStatus.Size = new System.Drawing.Size(182, 182);
            this.labelStatus.TabIndex = 0;
            this.labelStatus.Text = "Open a Minecraft world to start!";
            this.labelStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // progressBar
            // 
            this.progressBar.Dock = System.Windows.Forms.DockStyle.Right;
            this.progressBar.Location = new System.Drawing.Point(147, 5);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(200, 182);
            this.progressBar.TabIndex = 2;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Controls.Add(this.buttonOpen);
            this.flowLayoutPanel1.Location = new System.Drawing.Point(57, 14);
            this.flowLayoutPanel1.Margin = new System.Windows.Forms.Padding(5);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(437, 76);
            this.flowLayoutPanel1.TabIndex = 4;
            // 
            // buttonOpen
            // 
            this.buttonOpen.Location = new System.Drawing.Point(0, 0);
            this.buttonOpen.Margin = new System.Windows.Forms.Padding(0, 0, 5, 0);
            this.buttonOpen.Name = "buttonOpen";
            this.buttonOpen.Size = new System.Drawing.Size(110, 30);
            this.buttonOpen.TabIndex = 0;
            this.buttonOpen.Text = "Open world";
            this.buttonOpen.UseVisualStyleBackColor = true;
            this.buttonOpen.Click += new System.EventHandler(this.buttonOpen_Click);
            // 
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(677, 351);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.flowLayoutPanel1);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MinimumSize = new System.Drawing.Size(320, 260);
            this.Name = "FormMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "View\'m";
            this.Load += new System.EventHandler(this.FormMain_Load);
            this.contextMenuOpenWorld.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.flowLayoutPanel1.ResumeLayout(false);
            this.ResumeLayout(false);

		}

		#endregion

        private System.Windows.Forms.ContextMenuStrip contextMenuOpenWorld;
		private System.Windows.Forms.ToolStripMenuItem menuItemHeader;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Label labelStatus;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Button buttonOpen;
	}
}

