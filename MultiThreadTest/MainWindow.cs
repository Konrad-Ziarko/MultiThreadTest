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
        private class MyBackgroundWorker : BackgroundWorker
        {
            public bool isWorking { get; set; }
            public MyBackgroundWorker()
            {
                isWorking = true;
            }
        }
        MyBackgroundWorker bw;
        private object _threadLock = new object();
        Progress<int> progress;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void LongTask()
        {
            bool work = true;
            for (int i = 1; i <= 100 && work; i++)
            {
                try
                {
                    AddProgress(i);
                    Thread.Sleep(50);
                }
                catch (ThreadInterruptedException)
                {
                    lock (_threadLock)
                    {
                        Monitor.Wait(_threadLock);
                    }
                }
                catch (ThreadAbortException)
                {
                    work = false;
                }
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
                lock (_threadLock)
                    Monitor.Pulse(_threadLock);
            //thread.Resume();
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
                thread.Interrupt();
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
            thread.Abort();
            thread = null;
            button1.Enabled = true;
            button2.Enabled = false;
            button3.Enabled = false;
            Thread.Sleep(10);
            progressBar1.Value = progressBar1.Minimum;
        }

        private void AddProgress2()
        {
            bool work = true;
            try
            {
                for (int i = 1; i <= 100 && work; i++)
                {
                    Thread.Sleep(50);
                    bw.ReportProgress(i);
                    while (!bw.isWorking)
                    {
                        Thread.Sleep(100);
                    }
                    if (bw.CancellationPending)
                    {
                        work = false;
                    }
                }
            }
            catch (NullReferenceException) { }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (bw != null)
            {
                //bw.RunWorkerAsync(new MethodInvoker(AddProgress2));
                bw.isWorking = true;
            }
            else
            {
                bw = new MyBackgroundWorker();
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
            button5.Enabled = true;
            button6.Enabled = true;
        }
        private void button5_Click(object sender, EventArgs e)
        {
            bw.isWorking = false;
            button4.Enabled = true;
            button5.Enabled = false;
        }
        private void button6_Click(object sender, EventArgs e)
        {
            bw.CancelAsync();
            button4.Enabled = true;
            button5.Enabled = false;
            button6.Enabled = false;
            Thread.Sleep(50);
            bw = null;
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
            if (progress == null)
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
            
            button7.Enabled = true;
            button8.Enabled = true;
            button9.Enabled = false;
        }
    }
}
