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
        Progress<int> progress;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void LongTask()
        {
            for (int i = 1; i <= 100; i++)
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
            for (int i = 1; i <= 100; i++)
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

        private void button4_Click(object sender, EventArgs e)
        {
            if (bw != null)
            {
                bw.RunWorkerAsync(new MethodInvoker(AddProgress2));
            }
            else
            {
                bw = new BackgroundWorker();
                bw.WorkerReportsProgress = true;
                bw.WorkerSupportsCancellation = true;
                bw.DoWork += (s, eArgs) => ((MethodInvoker)eArgs.Argument).Invoke();
                bw.ProgressChanged += (s, eArgs) =>
                {
                    progressBar2.Style = ProgressBarStyle.Continuous;
                    progressBar2.Value = eArgs.ProgressPercentage;
                };
                bw.RunWorkerCompleted += (s, eArgs) =>
                {
                    if (progressBar2.Style == ProgressBarStyle.Marquee)
                    {
                        progressBar2.Visible = false;
                    }
                };
                bw.RunWorkerAsync(new MethodInvoker(AddProgress2));
            }
            button4.Enabled = false;
            button6.Enabled = true;
        }

        private void button6_Click(object sender, EventArgs e)
        {
            bw.CancelAsync();
            button4.Enabled = true;
            button6.Enabled = false;
            progressBar2.Value = progressBar2.Minimum;
        }

        
        private async Task LongTask2(IProgress<int> progress)
        {
            await Task.Run(() =>
             {
                 for (int i = 1; i <= 100; i++)
                 {
                     progress.Report(i);
                     Thread.Sleep(50);
                 }
             });
        }

        private async void button7_Click(object sender, EventArgs e)
        {
            button7.Enabled = false;
            button8.Enabled = true;
            button9.Enabled = true;
            progress = new Progress<int>(percent =>
            {
                progressBar3.Value = percent;
            });
            await LongTask2(progress);
            
        }

        private void button8_Click(object sender, EventArgs e)
        {
            //http://stackoverflow.com/questions/19613444/a-pattern-to-pause-resume-an-async-task/21712588#21712588
            button7.Enabled = true;
            button8.Enabled = false;
            button9.Enabled = true;
        }

        private void button9_Click(object sender, EventArgs e)
        {
            //
            button7.Enabled = true;
            button8.Enabled = true;
            button9.Enabled = false;
        }
    }
}
