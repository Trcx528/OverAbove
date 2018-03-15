using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace OverAbove
{
    class WindowsService : ServiceBase
    {
        ManagementEventWatcher owWatcher;
        EventLog log;
        private List<int> alreadySeenProcs = new List<int>();

        public WindowsService()
        {
            this.ServiceName = "Over and Above";
            this.EventLog.Log = "Application";

            this.CanHandlePowerEvent = false;
            this.CanHandleSessionChangeEvent = false;
            this.CanPauseAndContinue = false;
            this.CanShutdown = false;
            this.CanStop = true;
        }

        public static void Main()
        {
            ServiceBase.Run(new WindowsService());
        }

        protected override void OnStart(string[] args)
        {
            base.OnStart(args);
            log = new EventLog();
            if (!EventLog.SourceExists("OverAbove"))
                EventLog.CreateEventSource("OverAbove", "OverAbove");
            log.Source = "OverAbove";
            log.Log = "OverAbove";
            owWatcher = new ManagementEventWatcher(@"\\.\root\CIMV2", $"SELECT * FROM __InstanceOperationEvent WITHIN 30 WHERE TargetInstance ISA 'Win32_Process' AND TargetInstance.Name = 'Overwatch.exe'");
            owWatcher.EventArrived += new EventArrivedEventHandler(this.OnOverwatchStarted);
            owWatcher.Start();
            log.WriteEntry("OverAbove Started");
        }

        private void OnOverwatchStarted(object sender, EventArrivedEventArgs e)
        {
            var procs = Process.GetProcessesByName("overwatch");
            foreach (var proc in procs)
            {
                if (!alreadySeenProcs.Contains(proc.Id))
                {
                    log.WriteEntry($"Overwatch Running: changing pid: {proc.Id} to above normal");
                    proc.PriorityClass = ProcessPriorityClass.AboveNormal;
                    alreadySeenProcs.Add(proc.Id);
                }
            }
        }

        protected override void OnStop()
        {
            owWatcher.Stop();
            base.OnStop();
            log.WriteEntry("OverAbove Stopped");
        }
    }
}
