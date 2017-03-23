#region Directives
using System;
#endregion


namespace Mp3Player
{
    /// <summary>
    /// Iterator provide iteration over an array, using the index
    /// of an array with different looping fashions.
    /// </summary>
    abstract class Iterator
    {
        #region Constructors 
        /// <summary>
        /// Default constructor to initialize Iterator class.
        /// </summary>
        public Iterator() { }

        /// <summary>
        /// Constructor to initialize Iterator instance with array lenght.
        /// </summary>
        /// <param name="ArrayLength">Length Of the array on which Iteraor is working.</param>
        public Iterator(int ArrayLength)
        {
           Length = ArrayLength;
        }

        /// <summary>
        /// Constructor to initialize Iteator instance with array lenght & repeat mode.
        /// </summary>
        /// <param name="ArrayLength">Lenght of an array on which Iterator will perform</param>
        /// <param name="mode">Mode of iteration over an array.</param>
        public Iterator(int ArrayLength, RepeatModeEnum mode)
           :this(ArrayLength)
        {
           Mode = mode;
        }
        #endregion


        #region Enum
        /// <summary>
        /// Enum that provide element repeat modes.
        /// </summary>
        public enum RepeatModeEnum
        {
            /// <summary>
            /// Repeat all indexes in continuous loop fashion.
            /// </summary>
            RepeatAll,
            /// <summary>
            /// Randomly generate index, will effect Next() & Previous() methods. 
            /// </summary>
            Shuffle,
            /// <summary>
            /// Perform iteration without loop back to start index. 
            /// </summary>
            RepeatOnce
        }

        /// <summary>
        /// Enum provide values regarding the start and end of an array.
        /// </summary>
        public enum EndPoint
        {
            /// <summary>
            /// Represent the first index.
            /// </summary>
            First,
            /// <summary>
            /// Represent the last index.
            /// </summary>
            Last
        }
        #endregion


        #region Fields
        /// <summary>
        /// Length of the array.
        /// </summary>
        private int Length = 0;

        /// <summary>
        /// Count holds current index for an array.
        /// </summary>
        private int Count = 0;

        /// <summary>
        /// Initial index of array in iteration.
        /// </summary>
        private int initialIndex = 0;

        /// <summary>
        /// Repeat mode with which iteration progresses, default is reapeat-all.
        /// </summary>
        private RepeatModeEnum Mode = RepeatModeEnum.RepeatAll;
        #endregion


        #region Delegate & Events
        public delegate void EndofItems(object sender, EndPoint point);
        /// <summary>
        /// OnReachEnd event is fired when call made to Next() or Previous() methods
        /// when last index is returend previously or first index returned previously.
        /// </summary>
        public event EndofItems OnReachEnd;
        #endregion


        #region Properties
        /// <summary>
        /// get or set Length of the array.
        /// </summary>
        protected int ArraySize
        {
            get { return Length; }
            set { Length = value; } 
        }

        /// <summary>
        /// get current index of iteration.
        /// </summary>
        protected int CurrentInde 
        {
            get { return Count; }
        }

        /// <summary>
        /// get or set Initial index for starting the iteration over an array. Default is 0.
        /// </summary>
        protected int InitialIndex 
        {
            get { return initialIndex; }
            set { initialIndex = value; Count = initialIndex; }
        }

        /// <summary>
        /// Get or set repeat mode for this instance.
        /// </summary>
        public RepeatModeEnum RepeatMode
        {
            get { return Mode; }
            set { Mode = value; }
        }
        #endregion


        #region Public Methods
        /// <summary>
        /// Returns Next subsequent index in iteration process over an array.
        /// </summary>
        /// <returns>Next subsequent integer index.</returns>
        protected int Next()
        {
            // return Shuffle() when Shuffle mode is set.
            if(Mode == RepeatModeEnum.Shuffle) return Shuffle();

            if (Count < Length - 1) { Count++; }
            else 
            { 
               if(Mode != RepeatModeEnum.RepeatOnce) Count = 0; // start at the begining if RepeatOnce mode is not set.
               else
               {
                   // If RepeatOnce mode is set and last index was returned already, fire OnReachEnd event.
                   if (OnReachEnd != null) OnReachEnd(this, EndPoint.Last);
               }
            }
            return Count;
        }


        /// <summary>
        /// returns Previous subsequent index in iteration process over an array.
        /// </summary>
        /// <returns>Subsequent Previous index to the current one.</returns>
        protected int Previous()
        {
            if (Mode == RepeatModeEnum.Shuffle) return Shuffle();

            if (Count - 1 >= 0) { Count--; }
            else 
            { 
                if(Mode != RepeatModeEnum.RepeatOnce) Count = Length-1; 
                else
                {
                    // If RepeatOnce Mode is set and first index was returned already, fire OnReachEnd event.
                    if (OnReachEnd != null) OnReachEnd(this, EndPoint.First);
                }
            }            
            return Count;
        }

        
        /// <summary>
        /// returns random index between the lenght of an array.
        /// </summary>
        /// <returns>Integer representing a random index.</returns>
        protected int Shuffle()
        {
            // Generate a random interger index and put it to the current index,
            // and then return the curernt index or Count varialbe, so this way
            // if user call Next() or Previous() method result will be concluded 
            // according to the current number, so there will be no conflict...
            Random R = new Random();
            Count = R.Next(0, Length-1);
            return Count;
        }

        
        /// <summary>
        /// Static method to parse RepeatMode enum from string.
        /// </summary>
        /// <param name="str">String to parse.</param>
        /// <returns>Instance of PrepeatModeEnum. If string is not valid, return RepeatAll Value.</returns>
        public static RepeatModeEnum ParseRepeatMode(string str)
        {
            if (str == "RepeatAll")  return RepeatModeEnum.RepeatAll;
            if (str == "Shuffle")    return RepeatModeEnum.Shuffle;
            if (str == "RepeatOnce") return RepeatModeEnum.RepeatOnce;
            else return RepeatModeEnum.RepeatAll;
        }
        #endregion

    }// end of Class.
}