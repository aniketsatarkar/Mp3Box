#region Directives
using System;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Windows.Media.Imaging;

using System.IO;
using System.IO.Pipes;
using Microsoft.Win32;
using System.Windows.Interop;
using System.Windows.Threading;
using System.Windows.Media.Animation;

using IrrKlang;
using GlobalHotKey;
#endregion


namespace Mp3Player
{
    public partial class MainWindow : Window
    {

        #region Constructor
        public MainWindow()
        {
            InitializeComponent();
            
            // handling Window events >> 
            this.Loaded += MainWindow_Loaded;
            this.Closing += MainWindow_Closing;

            // Registring OnMessageRecives Event >>>
            EntryPoint.instanceMessaging.OnMessageRecived += instanceMessaging_OnMessageRecived;

            // DispaterTimer instance initialization >>>
            ArgsTimer.Interval = TimeSpan.FromSeconds(0.5);
            ArgsTimer.Tick += ArgsTimer_Tick;
        }
        #endregion


        #region Fields
        ISoundEngine soundEngine;
        ISound sound_player;
        List<string> playlist_files = new List<string>();
        PlayerEvents player_events;
        HoverCountdown HoverVisibility;
        Iterator.RepeatModeEnum RepeatMode = Iterator.RepeatModeEnum.RepeatAll;
        DispatcherTimer ArgsTimer = new DispatcherTimer();

        // using class that inherits Iterator >>>
        ///////////////////////////////////////////
        FileManager fileManager = new FileManager();
        ///////////////////////////////////////////

        #endregion 
        

        #region Event Handlers
        // Loaded event for this MainWindow [E]
        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {

            #region HotKeys Registrtion 
            // Associate Hot Keys >>>
            HotKeyHost KeyHost = new HotKeyHost((HwndSource)HwndSource.FromVisual(this));
            HotKey[] hotKeys = new HotKey[11];

            hotKeys[0] = new HotKey(Key.Home, ModifierKeys.Control | ModifierKeys.Alt, true);
            hotKeys[1] = new HotKey(Key.PageDown, ModifierKeys.Control | ModifierKeys.Alt, true);
            hotKeys[2] = new HotKey(Key.PageUp, ModifierKeys.Control | ModifierKeys.Alt, true);
            hotKeys[3] = new HotKey(Key.Right, ModifierKeys.Control | ModifierKeys.Alt, true);
            hotKeys[4] = new HotKey(Key.Left, ModifierKeys.Control | ModifierKeys.Alt, true);
            hotKeys[5] = new HotKey(Key.Up, ModifierKeys.Control | ModifierKeys.Alt, true);
            hotKeys[6] = new HotKey(Key.Down, ModifierKeys.Control | ModifierKeys.Alt, true);
            hotKeys[7] = new HotKey(Key.MediaPlayPause, ModifierKeys.None, true);
            hotKeys[8] = new HotKey(Key.MediaNextTrack, ModifierKeys.None, true);
            hotKeys[9] = new HotKey(Key.MediaPreviousTrack, ModifierKeys.None, true);
            hotKeys[10] = new HotKey(Key.End, ModifierKeys.Alt | ModifierKeys.Control, true); // <---

            hotKeys[0].HotKeyPressed += (o, s) => { PlayPause(); };
            hotKeys[1].HotKeyPressed += (o, s) => { PlayNext(); };
            hotKeys[2].HotKeyPressed += (o, s) => { PlayPrevious(); };
            hotKeys[3].HotKeyPressed += (o, s) => { seekForward(2000); };
            hotKeys[4].HotKeyPressed += (o, s) => { seekBackword(2000); };
            hotKeys[5].HotKeyPressed += (o, s) => { VolumeUp(); };
            hotKeys[6].HotKeyPressed += (o, s) => { VolumeDown(); };
            hotKeys[7].HotKeyPressed += (o, s) => { PlayPause(); };
            hotKeys[8].HotKeyPressed += (o, s) => { PlayNext(); };
            hotKeys[9].HotKeyPressed += (o, s) => { PlayPrevious(); };
            hotKeys[10].HotKeyPressed += (o, s) => { StopPlay(); };

            foreach (HotKey hKey in hotKeys)
            {
                try  { KeyHost.AddHotKey(hKey); }
                catch{ continue; }
            }
            #endregion

            // Reading User Preferences from Registry >>>
            ReadRegistrys();

            // Window DragMove Logic >>>
            this.MouseLeftButtonDown += (o, s) => { if(s.LeftButton == MouseButtonState.Pressed) this.DragMove(); };

            // initialize ISoundEngine instance...!
            soundEngine = new ISoundEngine();

            // initialize PlayerEvents class instance with event handlers...!
            player_events = new PlayerEvents();
            player_events.TimerInterval = TimeSpan.FromSeconds(0.8);
            player_events.IsTimerEnable = false;
            player_events.OnTimerTick += player_events_OnTimerTick;
            player_events.OnSoundFinished += player_events_OnSoundFinished;

            // initialize HoverCountdown instance, to manage visibility for elements...!
            HoverVisibility = new HoverCountdown(this, TimeSpan.FromSeconds(5));
            HoverVisibility.OnMouseMove += HoverVisibility_OnMouseMove;
            HoverVisibility.OnTimeElapsed += HoverVisibility_OnTimeElapsed;

            // Adding command arguments to the play list >>
            string[] cmdArg = Environment.GetCommandLineArgs();
            if(cmdArg.Length > 1)
            {
                //////////////////////////////////////////////
                fileManager.AddFiles(cmdArg, false, true);
                //////////////////////////////////////////////
            }
        }


        // Window Drop Event Handler [E]
        private void Window_Drop(object sender, DragEventArgs e)
        {
            if(e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] dropFiles = (string[])e.Data.GetData(DataFormats.FileDrop);
                if(fileManager.AddFiles(dropFiles, false, true))
                {
                    StartPlay(fileManager.currentFile);
                }
            }
        }


        // Click event handler for Previous/Play-Pause/Next Buttons [E]
        private void PlayerCommandButton_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            string tag = btn.Tag.ToString();

            if(tag == "Previous")  PlayPrevious();
            else if(tag == "Play") PlayPause();
            else if(tag == "Next") PlayNext();
        }


        // Mouse-Down event handler for Rectangle Element in MainGrid [E]
        private void albumArt_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                // If Iterator instnace is not null & playlist is not empty -> Open Explorer Window of Parent Directory of the file currently Playing >>
                if (fileManager.Count > 0)
                {
                    #region -- Bug -- 
                    // there is a Bug in Iterator class, that return NEGATIVE VALUE from CurrentIndex Property >>
                    #endregion

                    fileManager.ShowExplorer();
                    
                    //if(!String.IsNullOrEmpty(fileManager.currentFile))
                    //{
                    //    FileInfo info = new FileInfo(fileManager.currentFile);
                    //    Process.Start(info.DirectoryName);
                    //}
                    //else
                    //{
                    //    MessageBox.Show("Current file is null...!!! in albumArt_MouseDown()\n");
                    //}
                
                }
                else
                {
                    if(fileManager.AddFiles(null, true, true))
                    {
                        StartPlay(fileManager.currentFile);
                    }
                }
            }
        }


        // Hover CountDown Event Handlers ---------------
        // HoverCountdown.OnTimeElapsed event hanlder [E]
        void HoverVisibility_OnTimeElapsed(object Data)
        {
            if (MainGrid.Opacity == 1) MainGrid.BeginAnimation(Grid.OpacityProperty, new DoubleAnimation(1, 0, new Duration(TimeSpan.FromSeconds(0.5))));
        }

        // HoverCountdoen.OnMouseMover event handler [E]
        void HoverVisibility_OnMouseMove(object Data)
        {
            if (MainGrid.Opacity == 0) MainGrid.BeginAnimation(Grid.OpacityProperty, new DoubleAnimation(0, 1, new Duration(TimeSpan.FromSeconds(0.5))));
        }
        //-----------------------------------------------


        // PlayerEvents Event Handlers ------------------
        // PlayerEvent.OnSoundFinished Event handler [E]
        void player_events_OnSoundFinished(object sender)
        {
            // play next file after finisheing the first if there is any in array,
            // else, will play the only one represtedly, as Iterater intance iterates
            // in continuous loop fashion...
            PlayNext();
        }

        // PlayerEvent.OnTimerTick event Handler [E]
        void player_events_OnTimerTick(object sender)
        {
            SeekSlider.Value = sound_player.PlayPosition;
        }
        // ----------------------------------------------


        // ValueChanged Evnet handler for VolumeSlider Element [E]
        private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (sound_player != null) sound_player.Volume = (float)VolumeSlider.Value;
        }

        // ValueChanged event handler for SeekSlider Element [E]
        private void SeekSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (sound_player != null && SeekSlider.IsMouseOver && SeekSlider.IsMouseCaptureWithin) sound_player.PlayPosition = (uint)SeekSlider.Value;
        }

        // MouseRightButtonDown event Handler for Master Grid. [E]
        // Animate Clode and Minimize Buttons into view..
        private void MasterGrid_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            // condition check for if we are at firstgrid or not...
            if (MasterGridTT.X == 0)
            {
                DoubleAnimation da = new DoubleAnimation();
                da.Duration = new Duration(TimeSpan.FromSeconds(0.8));
                da.EasingFunction = new CircleEase() { EasingMode = EasingMode.EaseIn };

                if (ContextGridTT.Y == -25)
                {
                    da.From = -25;
                    da.To = 0;
                    ContextGridTT.BeginAnimation(TranslateTransform.YProperty, da);
                }
                if (ContextGridTT.Y == 0)
                {
                    da.From = 0;
                    da.To = -25;
                    ContextGridTT.BeginAnimation(TranslateTransform.YProperty, da);
                }
            }
        }


        // KeyDown Event Handler for this MainWindow. [E]
        // used to perform actions according to key pressed, while application is under focus.
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Up) VolumeUp();
            if (e.Key == Key.Down) VolumeDown();
        }


        // KeyUp event handler for MainWindow [e]
        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Right) seekForward(2000);
            if (e.Key == Key.Left) seekBackword(2000);
        }


        // Click Event handler for slider button [E]
        // used to slide between first and second grid elements.
        private void SliderButton_Click(object sender, RoutedEventArgs e)
        {
            DoubleAnimation da = new DoubleAnimation(0, -100, new Duration(TimeSpan.FromSeconds(0.8)));
            da.EasingFunction = new CircleEase() { EasingMode = EasingMode.EaseIn };

            // animating AlbumArt Image Element to slide in opposite direction simultaniously as grid do.
            DoubleAnimation da1 = new DoubleAnimation(0, 100, new Duration(TimeSpan.FromSeconds(0.8)));
            da1.EasingFunction = da.EasingFunction;

            if (MasterGridTT.X == 0)
            {
                MasterGridTT.BeginAnimation(TranslateTransform.XProperty, da);
                AlbumArtTT.BeginAnimation(TranslateTransform.XProperty, da1);
            }
            if (MasterGridTT.X == -100)
            {
                da.From = -100; da.To = 0;
                MasterGridTT.BeginAnimation(TranslateTransform.XProperty, da);
                da1.From = 100; da1.To = 0;
                AlbumArtTT.BeginAnimation(TranslateTransform.XProperty, da1);
            }
        }


        // Checked event handler for RepeatModeBtn RadioButton Group. [E]
        private void RepeatModeRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton btn = sender as RadioButton;

            if (btn.Tag.ToString() == "RepeatOnce")
            {
                fileManager.RepeatMode = RepeatMode = Iterator.RepeatModeEnum.RepeatOnce;
            }
            if (btn.Tag.ToString() == "RepeatAll")
            {
                fileManager.RepeatMode = RepeatMode = Iterator.RepeatModeEnum.RepeatAll;
            }
            if (btn.Tag.ToString() == "Shuffle")
            {
                fileManager.RepeatMode = RepeatMode = Iterator.RepeatModeEnum.Shuffle;
            }
        }


        // button Click Event handler for Add Files Buttons [E]
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button Btn = sender as Button;

            if (Btn.Tag.ToString() == "Add") fileManager.AddFiles(null, true);
            if (Btn.Tag.ToString() == "NewFiles")
            {
                if(fileManager.AddFiles(null, true, true))
                {
                    StartPlay(fileManager.currentFile);
                }
            }
        }


        // Drop Event handler for Add button in Second Grid. [E]
        // which will add files to Playlist...!
        private void AddButton_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] dropedFiles = (string[])e.Data.GetData(DataFormats.FileDrop);
                fileManager.AddFiles(dropedFiles, false, false);
            }
        }


        // Click Event handler for Close & Minimize buttons [E]
        private void CloseMinimizeBtn_click(object sender, RoutedEventArgs e)
        {
            Button btn = new Button();
            btn = sender as Button;
            if (btn.Tag.ToString() == "close") this.Close();
            if (btn.Tag.ToString() == "minimize") this.WindowState = System.Windows.WindowState.Minimized;
        }


        // MouseWheel Event handler used to Increase/Decrease Player Volume. [E]
        private void MasterGrid_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            // if Mouse Scrolled & Delta Value resolved negative, Volume is Decresed else incresed !!!
            if (e.Delta < 0) VolumeDown();
            else VolumeUp();
        }


        // Click Event handler for TaskBarInfo-ThumbButtonInfo element of this window [E]
        private void ThumbButtonInfo_Click(object sender, EventArgs e)
        {
            System.Windows.Shell.ThumbButtonInfo ThumbBtn = sender as System.Windows.Shell.ThumbButtonInfo;
            
            // play & pause buttons first additionly check if the ISound Intance is initialized and if Sound is paused
            // befre calling PlayPause() method.
            if (ThumbBtn.Description == "Play" && sound_player != null && sound_player.Paused)   PlayPause();
            if (ThumbBtn.Description == "Pause" && sound_player != null && !sound_player.Paused) PlayPause();
            if (ThumbBtn.Description == "Next Track") PlayNext();
            if (ThumbBtn.Description == "Previous Track") PlayPrevious();
        }


        // ValueChanged Event Handler for HiddenSlider [E]
        private void HiddenSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            // simply change the value >>>
            if (sound_player != null) sound_player.Volume = (float)e.NewValue;
        }


        // Window.Closing event handler for this application
        // Writing back to registr & stoping all sound...
        void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            WriteRegistrys();
            if (sound_player != null) soundEngine.StopAllSounds();
        }

        #endregion --------------------------------------------------------------------------------------


        #region Private Methods

        #region Player Command Methods
        private void PlayPause()
        {
            #region -- Info --
            /* Following code is organised to animate value property of HiddenSlider element, as name implies 
             * it's Visibility state is Hidden, using ValueChanged event handler of this slider, we are 
             * changing the value for ISound.Volume instance property, which result in interpolation of 
             * output sound volume increment/decrement. Also we called PlayPauseBtnUpdate() method 
             * which is responsible for updating the icon of a button. 
             */
            #endregion
            if (sound_player != null)
            {
                if(sound_player.Paused)
                {
                    sound_player.Paused = !sound_player.Paused;
                    PlayPauseBtnUpdate(this.PlayPauseBtn);

                    DoubleAnimation da = new DoubleAnimation(0, VolumeSlider.Value, new Duration(TimeSpan.FromSeconds(0.5)));
                    HiddenSlider.BeginAnimation(Slider.ValueProperty, da);
                }
                else
                {
                    DoubleAnimation da = new DoubleAnimation(VolumeSlider.Value, 0, new Duration(TimeSpan.FromSeconds(0.5)));
                    da.Completed += (o, s) => { sound_player.Paused = !sound_player.Paused; PlayPauseBtnUpdate(this.PlayPauseBtn); };
                    HiddenSlider.BeginAnimation(Slider.ValueProperty, da);
                }
            }
        }


        // method pause ISound instance & set PlayPosition to 0...
        private void StopPlay()
        {
            if (sound_player != null)
            {
                sound_player.Paused = true;
                sound_player.PlayPosition = (uint)0;
            }
        }

        private void PlayNext()
        {
            if (sound_player != null) StartPlay(fileManager.NextFile());
        }

        private void PlayPrevious()
        {
            if(sound_player != null) StartPlay(fileManager.PreviousFile());
        }

        private void VolumeUp()
        {
            if (sound_player != null && sound_player.Volume + (float)0.01 <= 1.01)
            {
                sound_player.Volume += (float)0.01;
                VolumeSlider.Value = sound_player.Volume;
            }
        }

        private void VolumeDown()
        {
            if (sound_player != null && sound_player.Volume - (float)0.01 >= -0.01)
            {
                sound_player.Volume -= (float)0.01;
                VolumeSlider.Value = sound_player.Volume;
            }
        }

        private void seekForward(uint value)
        {
            if (sound_player != null)
            {
                sound_player.PlayPosition += value;
                SeekSlider.Value = sound_player.PlayPosition;
            }
        }

        private void seekBackword(uint value)
        {
            if (sound_player != null)
            {
                sound_player.PlayPosition -= value;
                SeekSlider.Value = sound_player.PlayPosition;
            }
        }
        #endregion


        // method to start playing sound and initialize the sound_player object [M]
        private void StartPlay(string file)
        {
            if (sound_player != null) sound_player.Stop();

            albumArt.Source = AlbumArt.getAlbumart(file);          // Update Album Art ImageSource.
            TaskBarInfo.Description = AlbumArt.getSongTital(file); // Update TaskBar Description for this application.
            sound_player = soundEngine.Play2D(file);               // Initialize Isound instance.


            /////////////////////////////////////////
            if (player_events.IsTimerEnable) player_events.ResetTimer();
            player_events.SoundPlayer = sound_player;
            player_events.StartTimer();                         // strting player_event timer.
            sound_player.Volume = (float)VolumeSlider.Value;    // put current volueme levet to sound player volume.
            SeekSlider.Maximum = sound_player.PlayLength;       // Setting maximum size of seek slider to file play length.
            PlayPauseBtnUpdate(this.PlayPauseBtn);              // <---- method to update icon in Play-Pause button, 
            /////////////////////////////////////////
        }


        #region Comment-Out Code [\\]
        //// Method to check suppourted file formats before adding to playlist [M]
        //private bool IsValidFile(string filePath)
        //{
        //    try
        //    {
        //        FileInfo info = new FileInfo(filePath);
        //        switch (info.Extension.ToString().ToLower())
        //        {
        //            case ".mp3":
        //            case ".ogg":
        //            case ".mod":
        //            case "xm":
        //            case "s3m":
        //            case "wav":
        //                return true;
        //            default:
        //                return false;
        //        }
        //    }
        //    catch { return false; }
        //}


        //// Method to add file to a playlist [M]
        //private bool AddToPlaylist(string[] list, ref List<string> playList, bool clearList = false)
        //{
        //    List<string> tempList = new List<string>();
        //    bool isSuccess = false;

        //    foreach (string filePaht in list)
        //    {
        //        if (clearList) playList.Clear();
        //        if (IsValidFile(filePaht)) tempList.Add(filePaht);
        //    }

        //    if (tempList.Count > 0)
        //    {
        //        if (clearList) playList.Clear();
        //        playList.AddRange(tempList);
        //        isSuccess = true;
        //    }
        //    return isSuccess;
        //}


        //// Method to show Open-File-Dialog and add files to playslist [M]
        //private void ShowAddFilesDialog(bool appendToList = true)
        //{
        //    OpenFileDialog dialog = new OpenFileDialog();
        //    dialog.Multiselect = true;
        //    dialog.Filter = "Audio Files | *.mp3; *.ogg; *.mod; *.xm; *.s3m; *.wav";

        //    if (appendToList)
        //    {
        //        dialog.ShowDialog();

        //        if (dialog.FileNames.Length > 0)
        //        {
        //            if (AddToPlaylist(dialog.FileNames, ref playlist_files))
        //            {
        //                loop_iterate = new Iterator(playlist_files.Count); // <-- just update loop reange.
        //            }
        //        }
        //    }
        //    else
        //    {
        //        dialog.ShowDialog();

        //        if (dialog.FileNames.Length > 0)
        //        {
        //            // note that the last true argument will clear the list.
        //            if (AddToPlaylist(dialog.FileNames, ref playlist_files, true))
        //            {
        //                loop_iterate = new Iterator(playlist_files.Count);     // <-- update loop range &
        //                StartPlay(loop_iterate.InitialIndex);  // <-- Start To Play.
        //            }
        //        }
        //    }
        //}
        #endregion


        // Private method to Write User Preferences to registry [M]
        // Conventions Used :
        // # VL = Volume Level
        // # RM = Repeat Mode for Iterator instance.
        // # SP = Screen Position.
        private void WriteRegistrys()
        {
            try
            {
                RegistryKey rKey = Registry.CurrentUser.OpenSubKey(@"Software\Mp3Box", true);

                if (rKey != null)
                {
                    Point currPos = new Point(this.Top, this.Left);

                    rKey.SetValue("VL", VolumeSlider.Value);
                    rKey.SetValue("RM", RepeatMode);
                    rKey.SetValue("SP", currPos.ToString());
                }
                rKey.Close();
            }
            catch
            {
                MessageBox.Show("Writing Registry Failed", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        // Private method to Read user preferences from registry [M]
        private void ReadRegistrys()
        {
            RegistryKey rKey = Registry.CurrentUser.OpenSubKey(@"Software\Mp3Box");

            if (rKey == null)
            {
                // if registry subkey is resolve to null then create a sub-key >>>
                rKey = Registry.CurrentUser.CreateSubKey(@"Software\Mp3Box");
            }
            else
            {
                try
                {
                    // Get and Update volume value of Volume-Slider >>
                    VolumeSlider.Value = double.Parse((string)rKey.GetValue("VL"));

                    // Get and Update Repeat Mode >>>
                    //RepeatMode = Iterator.ParseRepeatMode((string)rKey.GetValue("RM"));
                    RepeatMode = FileManager.ParseRepeatMode((string)rKey.GetValue("RM"));
                    UpdateRepratModeRadioBtn(RepeatMode);

                    // Get and Update Position of the window over screen  >>>
                    Point Pos = Point.Parse((string)rKey.GetValue("SP"));
                    this.Top = Pos.X; this.Left = Pos.Y;
                }
                catch (Exception Ex)
                {
                    //MessageBox.Show(string.Format("Reading Registry Failed :\n{0}", Ex.Message), "Error", MessageBoxButton.OKCancel, MessageBoxImage.Error);
                }
            }
            rKey.Close();
        }


        // Private method used to update icon of PlayPause button element [M]
        private void PlayPauseBtnUpdate(Button btn)
        {
            System.Windows.Shapes.Path pt = (System.Windows.Shapes.Path)btn.Template.FindName("PlayPausePath", btn);
            
            if (sound_player != null && !sound_player.Paused)
            {
                Geometry geo = Geometry.Parse("F1 M 15.3845,0L 11.5384,0C 10.6886,0 9.99985,0.688843 9.99985,1.53845L 9.99985,18.4616C 9.99985,19.3112 10.6886,20 11.5384,20L 15.3845,20C 16.2342,20 16.9229,19.3112 16.9229,18.4616L 16.9229,1.53845C 16.9231,0.688843 16.2342,0 15.3845,0 Z M 5.38458,0L 1.53842,0C 0.688721,0 0,0.688843 0,1.53845L 0,18.4616C 0,19.3112 0.688721,20 1.53842,20L 5.38458,20C 6.23425,20 6.92297,19.3112 6.92297,18.4616L 6.92297,1.53845C 6.92291,0.688843 6.23413,0 5.38458,0 Z");
                pt.Data = geo;
            }

            if (sound_player != null && sound_player.Paused)
            {
                Geometry geo = Geometry.Parse("F1 M 16.0292,8.58054L 2.7204,0.195221C 2.3269,-0.039032 1.83838,0 1.46942,0C -0.00653076,0 0,1.1395 0,1.42819L 0,18.5669C 0,18.8109 -0.00640869,19.9951 1.46942,19.9951C 1.83838,19.9951 2.32703,20.034 2.7204,19.7998L 16.0291,11.4146C 17.1215,10.7646 16.9327,9.99753 16.9327,9.99753C 16.9327,9.99753 17.1216,9.23044 16.0292,8.58054 Z");
                pt.Data = geo;
            }
        }


        // private method to Update RepeatMode-Radio-Button state. [M]
        private void UpdateRepratModeRadioBtn(Iterator.RepeatModeEnum Mode)
        {
            if(Iterator.RepeatModeEnum.RepeatAll == Mode)   repeatAllRb.IsChecked = true;
            if (Iterator.RepeatModeEnum.RepeatOnce == Mode) repeatOnceRb.IsChecked = true;
            if (Iterator.RepeatModeEnum.Shuffle == Mode)    shuffleRb.IsChecked = true;
        }
        #endregion --------------------------------------------------------------------------------------


        #region -- Application's Message Handling Mechanism -- 
        // equivalent of HwndProc() method in Form [O]
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            HwndSource hwndsource = (HwndSource)HwndSource.FromVisual(this);
            hwndsource.AddHook(HwndSourceHook);
        }


        // an Event handler that get called when this application revices ant message [E]
        private IntPtr HwndSourceHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            // calling static instance of InstanceMessaging & its instance method ReciveMessage(mesageIntegerHandler) >>> 
            EntryPoint.instanceMessaging.ReviceMessage(msg);
            return IntPtr.Zero;
        }


        // Tick event handler for Argument DispatcherTimer [E]
        void ArgsTimer_Tick(object sender, EventArgs e)
        {
            ArgsTimer.Stop(); // <----

            //loop_iterate = new Iterator(playlist_files.Count, RepeatMode);
            StartPlay(fileManager.currentFile);
        }


        // Event fired when application recives a message [E]
        void instanceMessaging_OnMessageRecived(object sender, string[] msg)
        {
            if (!ArgsTimer.IsEnabled)
            {
                ArgsTimer.Start(); // <----
                playlist_files.Clear();
            }

            if (ArgsTimer.IsEnabled)
            {
                fileManager.AddFiles(msg, false, false);
                //AddToPlaylist(msg, ref playlist_files);
            }
        }
        #endregion --------------------------------------------------------------------------------------


    }// end of MainWindow.
}