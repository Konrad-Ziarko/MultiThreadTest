using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MultiThreadTest
{
    public partial class MainWindow : Form
    {
        Thread thread;
        BackgroundWorker bw;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void LongTask()
        {
            for (int i = 0; i <= 100; i++)
            {
                AddProgress(i);
                Thread.Sleep(50);
            }
        }

        private void AddProgress(int i)
        {
            if (InvokeRequired)
            {
                this.BeginInvoke(new Action<int>(AddProgress), new object[] { i });
                return;
            }
            progressBar1.Value = i;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (thread != null)
                thread.Resume();
            else
            {
                thread = new Thread(LongTask);
                thread.Start();
            }
            button1.Enabled = false;
            button2.Enabled = true;
            button3.Enabled = true;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                thread.Suspend();
            }
            catch (ThreadStateException)
            {
                thread.Abort();
                thread = null;
                progressBar1.Value = progressBar1.Minimum;
            }
            button1.Enabled = true;
            button2.Enabled = false;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                thread.Abort();
            }
            catch (ThreadStateException)
            {
                thread.Resume();
                thread.Abort();
            }
            thread = null;
            progressBar1.Value = progressBar1.Minimum;
            button1.Enabled = true;
            button2.Enabled = false;
            button3.Enabled = false;
        }

        private void AddProgress2()
        {
            for (int i = 0; i <= 100; i++)
            {
                Thread.Sleep(50);
                bw.ReportProgress(i);
                if (bw.CancellationPending)
                {
                    progressBar2.BeginInvoke(new MethodInvoker(() => progressBar2.Value = progressBar2.Minimum));
                    break;
                }
            }
        }
        
    }
}
