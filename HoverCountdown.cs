#region Directives
using System;
using System.Windows;
using System.Windows.Threading;
#endregion


namespace Mp3Player
{
    /// <summary>
    /// HoverCountdown Class privide some mouse event functionality 
    /// using time interval.
    /// </summary>
    class HoverCountdown
    {

        #region Constructors 
        /// <summary>
        /// Default constructor to initialize HoverCountdown class.
        /// </summary>
        public HoverCountdown()
        {
            // Initialize DispatcherTimer with 3 sec TimeSpan.
            Timer = new DispatcherTimer();
            Timer.Interval = Interval;
            Timer.Tick += Timer_Tick;
        }

        /// <summary>
        /// Overloaded constructor to initialize HoverCountdown class.
        /// </summary>
        /// <param name="contextElement">Element that is used as a context for MouseMover Event.</param>
        /// <param name="interval">A TimeSpan object that set the time for countdown</param>
        public HoverCountdown(UIElement contextElement, TimeSpan interval)
        {
            // Initialize DispatcherTimer with user defined TimeSpan.
            Timer = new DispatcherTimer();
            Interval = interval;
            Timer.Tick += Timer_Tick;

            ContextElement = contextElement;
        }
        #endregion


        #region Fields
        /// <summary>
        /// A DispatcherTimer object to fire event at a specific time interval.
        /// </summary>
        private DispatcherTimer Timer;

        /// <summary>
        /// An interval at which Timer will fire an event.
        /// </summary>
        private TimeSpan timeSpan = TimeSpan.FromSeconds(3);

        /// <summary>
        /// UIElement which is used as an context to decide whether to fire an event. 
        /// </summary>
        private UIElement context = new UIElement();
        #endregion


        #region Properties
        /// <summary>
        /// Get or set an acquaintance object for passing through the event. 
        /// </summary>
        public Object Data { get; set; }
        
        /// <summary>
        /// Get or set time interval for timer.
        /// </summary>
        public TimeSpan Interval 
        {
            get { return timeSpan;  }
            set { timeSpan = value; Timer.Interval = timeSpan; }
        }

        /// <summary>
        /// Get or set context element for this object.
        /// </summary>
        public UIElement ContextElement 
        {
            get { return context;  }
            set { context = value; context.MouseMove += ContextElement_MouseMove; } 
        }
        #endregion


        #region Delegates & Events 
        public delegate void MouseMove(object Data);
        public delegate void TimeElapsed(object Data);

        /// <summary>
        /// Event that will be fired if the Mouse Moves over the surface of context element provided.
        /// </summary>
        public event MouseMove OnMouseMove;

        /// <summary>
        /// Event which will be fired if mouse do not move for provided interval of time.
        /// for the interval.
        /// </summary>
        public event TimeElapsed OnTimeElapsed;
        #endregion


        #region Event Handlers
        /// <summary>
        /// Event handler for MouseMove event of context element.
        /// </summary>
        private void ContextElement_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Timer.Stop(); // Stop Previous timer...

            /////////////////////////////////////////
            if(OnMouseMove != null) OnMouseMove(Data);
            /////////////////////////////////////////

            Timer.Start(); // Start New timer...
        }

        /// <summary>
        /// Tick event handler for dispatcher timer.
        /// </summary>
        private void Timer_Tick(object sender, EventArgs e)
        {
            Timer.Stop(); // Stop timer, as time is elapsed...

            //////////////////////////////////////////////
            if(OnTimeElapsed != null) OnTimeElapsed(Data);
            //////////////////////////////////////////////
        }
        #endregion


    }// end of class.
}
