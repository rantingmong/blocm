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
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.panel1 = new System.Windows.Forms.Panel();
			this.labelStatus = new System.Windows.Forms.Label();
			this.progressBar = new System.Windows.Forms.ProgressBar();
			this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
			this.buttonOpen = new System.Windows.Forms.Button();
			this.buttonSaveImage = new System.Windows.Forms.Button();
			this.contextMenuOpenWorld = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.menuItemHeader = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
			this.mapViewer = new viewm.MapViewer(this.components);
			this.tableLayoutPanel1.SuspendLayout();
			this.panel1.SuspendLayout();
			this.flowLayoutPanel1.SuspendLayout();
			this.contextMenuOpenWorld.SuspendLayout();
			this.SuspendLayout();
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.ColumnCount = 1;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
			this.tableLayoutPanel1.Controls.Add(this.panel1, 0, 2);
			this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel1, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.mapViewer, 0, 1);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 3;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(784, 441);
			this.tableLayoutPanel1.TabIndex = 0;
			// 
			// panel1
			// 
			this.panel1.BackColor = System.Drawing.Color.SteelBlue;
			this.panel1.Controls.Add(this.labelStatus);
			this.panel1.Controls.Add(this.progressBar);
			this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panel1.Location = new System.Drawing.Point(0, 411);
			this.panel1.Margin = new System.Windows.Forms.Padding(0);
			this.panel1.Name = "panel1";
			this.panel1.Padding = new System.Windows.Forms.Padding(5);
			this.panel1.Size = new System.Drawing.Size(784, 30);
			this.panel1.TabIndex = 0;
			// 
			// labelStatus
			// 
			this.labelStatus.Dock = System.Windows.Forms.DockStyle.Left;
			this.labelStatus.ForeColor = System.Drawing.Color.White;
			this.labelStatus.Location = new System.Drawing.Point(5, 5);
			this.labelStatus.Name = "labelStatus";
			this.labelStatus.Size = new System.Drawing.Size(400, 20);
			this.labelStatus.TabIndex = 0;
			this.labelStatus.Text = "Open a Minecraft world to start!";
			this.labelStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// progressBar
			// 
			this.progressBar.Dock = System.Windows.Forms.DockStyle.Right;
			this.progressBar.Location = new System.Drawing.Point(579, 5);
			this.progressBar.Name = "progressBar";
			this.progressBar.Size = new System.Drawing.Size(200, 20);
			this.progressBar.TabIndex = 2;
			// 
			// flowLayoutPanel1
			// 
			this.flowLayoutPanel1.Controls.Add(this.buttonOpen);
			this.flowLayoutPanel1.Controls.Add(this.buttonSaveImage);
			this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.flowLayoutPanel1.Location = new System.Drawing.Point(5, 5);
			this.flowLayoutPanel1.Margin = new System.Windows.Forms.Padding(5);
			this.flowLayoutPanel1.Name = "flowLayoutPanel1";
			this.flowLayoutPanel1.Size = new System.Drawing.Size(774, 30);
			this.flowLayoutPanel1.TabIndex = 1;
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
			// buttonSaveImage
			// 
			this.buttonSaveImage.Enabled = false;
			this.buttonSaveImage.Location = new System.Drawing.Point(115, 0);
			this.buttonSaveImage.Margin = new System.Windows.Forms.Padding(0, 0, 5, 0);
			this.buttonSaveImage.Name = "buttonSaveImage";
			this.buttonSaveImage.Size = new System.Drawing.Size(75, 30);
			this.buttonSaveImage.TabIndex = 1;
			this.buttonSaveImage.Text = "Save image";
			this.buttonSaveImage.UseVisualStyleBackColor = true;
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
			// mapViewer
			// 
			this.mapViewer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.mapViewer.InputBitmap = null;
			this.mapViewer.Location = new System.Drawing.Point(0, 40);
			this.mapViewer.Margin = new System.Windows.Forms.Padding(0);
			this.mapViewer.Name = "mapViewer";
			this.mapViewer.Size = new System.Drawing.Size(784, 371);
			this.mapViewer.TabIndex = 2;
			// 
			// FormMain
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(784, 441);
			this.Controls.Add(this.tableLayoutPanel1);
			this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.MinimumSize = new System.Drawing.Size(800, 480);
			this.Name = "FormMain";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "View\'m";
			this.Load += new System.EventHandler(this.FormMain_Load);
			this.tableLayoutPanel1.ResumeLayout(false);
			this.panel1.ResumeLayout(false);
			this.flowLayoutPanel1.ResumeLayout(false);
			this.contextMenuOpenWorld.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
		private System.Windows.Forms.Button buttonOpen;
		private System.Windows.Forms.Button buttonSaveImage;
		private System.Windows.Forms.Label labelStatus;
		private System.Windows.Forms.ContextMenuStrip contextMenuOpenWorld;
		private System.Windows.Forms.ToolStripMenuItem menuItemHeader;
		private System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
		private System.Windows.Forms.ProgressBar progressBar;
		private MapViewer mapViewer;
	}
}

