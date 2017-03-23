#region Directives
using System;
using System.Text;
using System.Threading;
using System.IO.Pipes;
#endregion


namespace Mp3Player
{
    /// <summary>
    /// EntryPoint class defines entry point for this application.
    /// </summary>
    class EntryPoint
    {

        #region Fields
        /// <summary>
        /// Mutex that used to check against instances of this application being
        /// run at an instance on machine.
        /// </summary>
        static Mutex mutex = new Mutex(true, "{406ea660-64cf-4c82-b6f0-42d48172a799}");
        
        /// <summary>
        /// Object of InstanceMessaging used to manage messaging between different
        /// instances of this application.
        /// </summary>
        public static InstanceMessaging instanceMessaging = new InstanceMessaging("{406ea660-64cf-4c82-b6f0-42d48172a799}");
        #endregion


        #region Main Method
        /// <summary>
        /// Custom entry point for the application.
        /// </summary>
        /// <param name="args">List of argument strings for the application</param>
        [STAThread]
        public static void Main(string[] args)
        {
            if(mutex.WaitOne(TimeSpan.Zero, true))
            {
                Mp3Player.App app = new Mp3Player.App();
                app.InitializeComponent();
                app.Run();

                mutex.ReleaseMutex();
            }
            else
            {
                instanceMessaging.SendMessage(args);
            }
        }
        #endregion

    }// end of class.
}
