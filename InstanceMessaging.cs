#region Directives 
using System;
using System.Text;
using System.IO.Pipes;
using System.Threading;
using System.Collections.Generic;
using System.Runtime.InteropServices;
#endregion 


namespace Mp3Player
{
    /// <summary>
    /// InstanceMessaging provide interop services to send and recive messages from 
    /// instances of an application
    /// </summary>
    class InstanceMessaging
    {

        #region Ctor & Field
        /// <summary>
        /// Constructor to initialize instance of the InstanceMessaging class.
        /// </summary>
        /// <param name="mutexName">Name used with Mutex.</param>
        public InstanceMessaging(string mutexName)
        {
            ApplicationName = mutexName;
        }

        /// <summary>
        /// Name string for the application.
        /// </summary>
        private string ApplicationName = string.Empty;
        #endregion 


        #region Public Methods
        /// <summary>
        /// Method to send message to the running instance of this application.
        /// </summary>
        /// <param name="messages">Array of message strings</param>
        public void SendMessage(string[] messages)
        {
            // Encoding Formated string in to byte array ---------
            string cla = string.Empty;
            foreach (string arg in messages) cla += arg + "`";
            cla = cla.Trim('`');
            byte[] argsBuffer = Encoding.UTF8.GetBytes(cla);
            // ---------------------------------------------------

            NamedPipeServerStream claSvr = new NamedPipeServerStream(ApplicationName, PipeDirection.InOut,
                1, PipeTransmissionMode.Message, PipeOptions.WriteThrough, 8192, 8192);

            NativeMethods.PostMessage((IntPtr)NativeMethods.HWND_BROADCAST, NativeMethods.WM_SHOWME,
                IntPtr.Zero, IntPtr.Zero);

            claSvr.WaitForConnection(); 
            claSvr.Write(argsBuffer, 0, argsBuffer.Length);
            claSvr.WaitForPipeDrain();
            claSvr.Close();
        }


        /// <summary>
        /// Method to call from the HWnd event handler.
        /// </summary>
        /// <param name="messageHandle">Message handler integer value. </param>
        public void ReviceMessage(int messageHandle)
        {
            if (messageHandle == NativeMethods.WM_SHOWME)
            {
                NamedPipeClientStream argsClient = new NamedPipeClientStream(ApplicationName);
                List<byte> bytes = new List<byte>();
                byte[] buffer = new byte[8192];
                int count = 0;

                argsClient.Connect();

                do
                {
                    count = argsClient.Read(buffer, 0, buffer.Length); 

                    if (count == buffer.Length)
                    {
                        bytes.AddRange(buffer);
                    }
                    else
                    {
                        byte[] temp = new byte[count];
                        Array.Copy(buffer, 0, temp, 0, count);
                        bytes.AddRange(temp);
                        break;
                    }

                } while (true);

                argsClient.Close();

                string cla = Encoding.UTF8.GetString(bytes.ToArray());

                if (!string.IsNullOrEmpty(cla))
                {
                    string[] args = cla.Split('`');

                    if (args != null)
                    {
                        if (OnMessageRecived != null) OnMessageRecived(this, args);
                    }
                }
            }
        } // end of method.
        #endregion 


        #region Delegat & Event
        /// <summary>
        /// Delegate provided to OnMessageRecived event.
        /// </summary>
        /// <param name="sender">Instance of the class.</param>
        /// <param name="msg">String array of messages.</param>
        public delegate void getmessage(object sender, string[] msg);
        
        /// <summary>
        /// Event that is fired when application recives a message from another instance.
        /// </summary>
        public event getmessage OnMessageRecived;
        #endregion 
    
    }// end of class.


    /// <summary>
    /// NativeMethods contain InterOp methods & fields.
    /// </summary>
    class NativeMethods
    {

        #region Fields
        public const int HWND_BROADCAST = 0xffff;
        public static readonly int WM_SHOWME = RegisterWindowMessage("WM_SHOWME");
        #endregion 


        #region Native Methods
        [DllImport("user32")]
        public static extern bool PostMessage(IntPtr hwnd, int msg, IntPtr wparam, IntPtr lparam);

        [DllImport("user32")]
        public static extern int RegisterWindowMessage(string message);
        #endregion 

    }// end of class.

}
