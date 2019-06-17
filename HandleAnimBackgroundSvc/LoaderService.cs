using System;
using System.Diagnostics;
using System.IO;
using System.ServiceProcess;

namespace Toolkit
{
    public partial class LoaderService : ServiceBase
    {
        public LoaderService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            // the name of the application to launch;
            // to launch an application using the full command path simply escape
            // the path with quotes, for example to launch firefox.exe:
            //      String applicationName = "\"C:\\Program Files (x86)\\Mozilla Firefox\\firefox.exe\"";
            String applicationName = "cmd.exe";

            // launch the application
            ApplicationLoader.PROCESS_INFORMATION procInfo;

            var folder = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            folder = folder.Replace("\\", "\\\\");
            var path = "\"" + folder + "\\\\" + "DrawBehindDesktopIcons.exe" + "\"";


            try
            {
                var curExecDir = Directory.GetCurrentDirectory();
                var dirp = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                File.WriteAllText("test.log", "time:" + DateTime.Now + "\n");
                File.AppendAllText("test.log", "exec:" + curExecDir + "\n");
                File.AppendAllText("test.log", "path:" + path + "\n");
                File.AppendAllText("test.log", "dirp:" + dirp + "\n");

                Directory.SetCurrentDirectory(dirp);
                _pid = ApplicationLoader.StartProcessAndBypassUAC(dirp, path, out procInfo);
                File.AppendAllText("test.log", "_pid:" + _pid + "\n");

            }
            catch (Exception e)
            {
                File.AppendAllText("test.log", e.ToString() + "\n");
            }
        }
        private uint _pid;
        protected override void OnStop()
        {
            File.AppendAllText("test.log", "kill:" + (int)_pid + "\n");
            Process.GetProcessById((int)_pid).Kill();
        }
    }
}
