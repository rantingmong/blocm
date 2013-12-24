using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using viewm.Renderer;

namespace viewm
{
    public partial class FormMain : Form
    {
        public FormMain()
        {
            InitializeComponent();
        }

        private void buttonOpen_Click(object sender, EventArgs e)
        {
            contextMenuOpenWorld.Show(buttonOpen, new Point(0, buttonOpen.Height), ToolStripDropDownDirection.BelowRight);
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            string appdataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

            if (Directory.Exists(Path.Combine(appdataPath, ".minecraft")))
            {
                int i = 0;
                string[] directories = Directory.GetDirectories(Path.Combine(appdataPath, ".minecraft", "saves"));

                contextMenuOpenWorld.Items.Clear();
                contextMenuOpenWorld.Items.Add(menuItemHeader);
                contextMenuOpenWorld.Items.Add(new ToolStripSeparator());

                foreach (ToolStripMenuItem item in directories.Select(directory => new ToolStripMenuItem(Path.GetFileName(directory))))
                {
                    int index = i++;

                    item.Click += (o, args) =>
                        {
                            var thread = new Thread(() =>
                                {
                                    DateTime sTime = DateTime.Now;

                                    var chunkProcessor = new WorldProcessor(directories[index]);

                                    chunkProcessor.ProcessFailed += s => { BeginInvoke(new Action(() => { MessageBox.Show(this, s, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); })); };

                                    chunkProcessor.ProcessStarted += () =>
                                        {
                                            sTime = DateTime.Now;

                                            BeginInvoke(new Action(() =>
                                                {
                                                    progressBar.Value = 0;
                                                    labelStatus.Text = "Processing world...";

                                                    buttonOpen.Enabled = false;
                                                }));
                                        };
                                    chunkProcessor.ProcessComplete += () =>
                                        {
                                            DateTime eTime = DateTime.Now;

                                            BeginInvoke(new Action(() =>
                                                {
                                                    labelStatus.Text = "Process completed in " + (eTime - sTime).TotalSeconds + " seconds.";

                                                    mapViewer.InputBitmap   = null;
                                                    buttonOpen.Enabled      = true;
                                                }));
                                        };

                                    chunkProcessor.ProgressChanged +=
                                        (p) => BeginInvoke(new Action(() => { progressBar.Value = (int)(100 * p); }));

                                    chunkProcessor.Start();
                                });

                            thread.Start();
                        };

                    contextMenuOpenWorld.Items.Add(item);
                }
            }
            else
            {
                menuItemHeader.Text = "Minecraft folder cannot be found!";
            }
        }
    }
}
