#region Directives
using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

using IrrKlang;
#endregion


namespace Mp3Player
{
    /// <summary>
    /// PlayerEvents provide set of events for ISound instance properties, 
    /// and a timer tick event.
    /// </summary>
    class PlayerEvents
    {

        #region Constructors
        /// <summary>
        /// Default constructor to create a PlayerEvents object.
        /// </summary>
        public PlayerEvents()
        {
            timer.Interval = interval;
            timer.Tick += timer_Tick;
        }

        /// <summary>
        /// Overloaded constructor to create an object of PlayerEvents.
        /// </summary>
        /// <param name="sound_player">An object of class that implements ISound interface.</param>
        public PlayerEvents(ISound sound_player)
        {
            this.sound_player = sound_player;

            timer.Interval = interval;
            timer.Tick += timer_Tick;
            timer.Start();
        }

        /// <summary>
        /// Overloaded constructor to initialize an instance of PlayerEvents class.
        /// </summary>
        /// <param name="sound_palyer">Class instance that implement ISound interface.</param>
        /// <param name="interval">TimeSpan interval at which OnTimerTick event will be fired.</param>
        /// <param name="Enable">Boolean value specifies whether to start the timer or not.</param>
        public PlayerEvents(ISound sound_palyer, TimeSpan interval, bool Enable)
        {
            this.sound_player = sound_palyer;
            timer.Interval = interval;
            timer.Tick += timer_Tick;
            
            if (Enable) 
                timer.Start();
        }
        #endregion


        #region Fields
        /// <summary>
        /// An instane of ISound from which state for events are derived.
        /// </summary>
        ISound sound_player;
        
        /// <summary>
        /// Timer object used to check state for the player at a specific interval.
        /// </summary>
        DispatcherTimer timer = new DispatcherTimer();

        /// <summary>
        /// Defines time interval at which timer will be checking the state.
        /// </summary>
        TimeSpan interval = TimeSpan.FromSeconds(1);
        #endregion 


        #region Properties 
        /// <summary>
        /// Get or set ISound object for this class.
        /// </summary>
        public ISound SoundPlayer
        {
            get { return sound_player; }
            set { sound_player = value; }
        }

        /// <summary>
        /// Get or set TimeSpan object for this class.
        /// </summary>
        public TimeSpan TimerInterval 
        {
            get { return interval; }
            set { interval = value; }
        }

        /// <summary>
        /// Get or set whether to Timer is enable or not.
        /// </summary>
        public bool IsTimerEnable 
        {
            get { return timer.IsEnabled; }
            set { timer.IsEnabled = value; }
        }
        #endregion


        #region Delegates & Events
        /// <summary>
        /// Delegate used to define handler prototype for OnSoundFinished event.
        /// </summary>
        /// <param name="sender">Instance of this class.</param>
        public delegate void sound_ended(object sender);
        
        /// <summary>
        /// Delegate used to define handler prototype for OnSoundPaused event.
        /// </summary>
        /// <param name="sender">Instance this class.</param>
        public delegate void sound_paused(object sender);
        
        /// <summary>
        /// Delegate that defines handler prototype for OnTimerTick event.
        /// </summary>
        /// <param name="sender">Instance of this class.</param>
        public delegate void sound_tick(object sender);

        /// <summary>
        /// Event that will occur when the song is finished.
        /// </summary>
        public event sound_ended OnSoundFinished;

        /// <summary>
        /// An event that will occur when the song is paused.
        /// </summary>
        public event sound_ended OnSoundPaused;

        /// <summary>
        /// An event that will be fired at a spcified time interval.
        /// </summary>
        public event sound_tick OnTimerTick;
        #endregion 


        #region Event Handlers
        private void timer_Tick(object sender, EventArgs e)
        {
            // Trigger OnTimerTick event per timer interval provided...
            if (OnTimerTick != null) 
                OnTimerTick(this);

            if (sound_player != null)
            {
                // Trigger OnSoundPaused event if sound is paused...
                if (sound_player.Paused)
                    if (OnSoundPaused != null) 
                        OnSoundPaused(this);

                // Trigger OnSoundFinished event if song is ended...
                if (sound_player.Finished)
                    if (OnSoundFinished != null) 
                        OnSoundFinished(this);
            }
        }
        #endregion 


        #region Public Methods
        /// <summary>
        /// Start the DispatcherTimer.
        /// </summary>
        public void StartTimer()
        {
            timer.Start();
        }

        /// <summary>
        /// Stop the DispatcherTimer.
        /// </summary>
        public void StopTimer()
        {
            timer.Stop();
        }

        /// <summary>
        /// Reset the DispatcherTimer.
        /// </summary>
        public void ResetTimer()
        {
            timer.Stop();
            timer.Start();
        }
        #endregion

    }// end of class.
}
