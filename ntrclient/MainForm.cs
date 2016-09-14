using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ntrbase
{
    public partial class MainForm : Form
    {

        public static class Delay
        {

            static System.Windows.Forms.Timer runDelegates;
            static Dictionary<MethodInvoker, DateTime> delayedDelegates = new Dictionary<MethodInvoker, DateTime>();

            static Delay()
            {

                runDelegates = new System.Windows.Forms.Timer();
                runDelegates.Interval = 250;
                runDelegates.Tick += RunDelegates;
                runDelegates.Enabled = true;

            }

            public static void Add(MethodInvoker method, int delay)
            {

                delayedDelegates.Add(method, DateTime.Now + TimeSpan.FromSeconds(delay));

            }

            static void RunDelegates(object sender, EventArgs e)
            {

                List<MethodInvoker> removeDelegates = new List<MethodInvoker>();

                foreach (MethodInvoker method in delayedDelegates.Keys)
                {

                    if (DateTime.Now >= delayedDelegates[method])
                    {
                        method();
                        removeDelegates.Add(method);
                    }

                }

                foreach (MethodInvoker method in removeDelegates)
                {

                    delayedDelegates.Remove(method);

                }


            }

        }


        public delegate void LogDelegate(string l);
		public LogDelegate delAddLog;

        public MainForm()
        {
			delAddLog = new LogDelegate(Addlog);
            InitializeComponent();
        }


        public void Addlog(string l)
        {
			if (!l.Contains("\r\n"))
            {
				l = l.Replace("\n", "\r\n");
			}
            if (!l.EndsWith("\n"))
            {
                l += "\r\n";
            }
            txtLog.AppendText(l);
        }

		void runCmd(String cmd)
        {
			try
            {
				object ret = Program.pyEngine.CreateScriptSourceFromString(cmd).Execute(Program.globalScope);
			}
			catch (Exception ex)
            {
				Addlog(ex.Message);
				Addlog(ex.StackTrace);
			}
		}

		private void timer1_Tick(object sender, EventArgs e)
        {
			try
            {
				Program.ntrClient.sendHeartbeatPacket();
				
			}
            catch (Exception)
            {
			}
		}

		private void CmdWindow_Load(object sender, EventArgs e)
        {
			runCmd("import sys;sys.path.append('.\\python\\Lib')");
			runCmd("for n in [n for n in dir(nc) if not n.startswith('_')]: globals()[n] = getattr(nc,n)    ");
			runCmd("repr([n for n in dir(nc) if not n.startswith('_')])");
		}

		private void CmdWindow_FormClosed(object sender, FormClosedEventArgs e)
        {
			Program.ntrClient.disconnect();
		}


        public void startAutoDisconnect()
        {
            disconnectTimer.Enabled = true;

        }


        private void disconnectTimer_Tick(object sender, EventArgs e)
        {
            disconnectTimer.Enabled = false;
            runCmd("disconnect()");
        }

        public void connectCheck()
        {
            if (txtLog.Text.Contains("Server connected"))
            {
                buttonConnect.Text = "Connected";
            }
        }

        private void buttonConnect_Click(object sender, EventArgs e)
        {
            txtLog.Clear();
            runCmd("connect('" + host.Text + "',8000)");
            Delay.Add(connectCheck, 1);

        }


        private void buttonDisconnect_Click(object sender, EventArgs e)
        {
            runCmd("disconnect()");
            buttonConnect.Text = "Connect";
        }
    }
}
