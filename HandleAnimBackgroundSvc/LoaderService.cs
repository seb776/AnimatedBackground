using System;
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
            var path = "\"" + folder + "\\\\" + "DrawBehindDesktopIcons.exe\\" + "\"";
            try
            {

            ApplicationLoader.StartProcessAndBypassUAC(path, out procInfo);
            }
            catch (Exception e)
            {

            File.WriteAllText("test.log", e.ToString());
            }
        }

        protected override void OnStop()
        {
        }
    }
}
