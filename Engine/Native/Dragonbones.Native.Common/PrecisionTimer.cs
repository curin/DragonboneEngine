using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace Dragonbones.Native
{
    /// <summary>
    /// A precision timer used to profile functions
    /// </summary>
    public class PrecisionTimer
    {
        private readonly Stopwatch _timer;
        /// <summary>
        /// The frequency which the timer runs on
        /// </summary>
        public static double Frequency => (double)Stopwatch.Frequency;
        /// <summary>
        /// The frequency which the timer runs on
        /// </summary>
        public static float FrequencyF => (float)Stopwatch.Frequency;

        /// <summary>
        /// Creates a <see cref="PrecisionTimer"/>
        /// </summary>
        public PrecisionTimer()
        {
            _timer = new Stopwatch();
        }

        /// <summary>
        /// Is the timer currently runnning
        /// </summary>
        public bool IsRunning => _timer.IsRunning;

        /// <summary>
        /// The total time elapsed as a timespan
        /// </summary>
        public TimeSpan Elapsed => _timer.Elapsed;

        /// <summary>
        /// The amount of seconds elapsed
        /// </summary>
        public double ElapsedSeconds => _timer.ElapsedTicks / Frequency;

        /// <summary>
        /// the amount of seconds elapsed in single precision floating point value
        /// </summary>
        public float ElapsedSecondsF => _timer.ElapsedTicks / FrequencyF;

        /// <summary>
        /// Starts Timer
        /// </summary>
        public void Start()
        {
            _timer.Start();
        }

        /// <summary>
        /// Stops timer
        /// </summary>
        public void Stop()
        {
            _timer.Stop();
        }

        /// <summary>
        /// Stops timer and resets elapsed to zero
        /// </summary>
        public void Reset()
        {
            _timer.Reset();
        }

        /// <summary>
        /// Stops timer, resets elapsed to zero, and starts timer anew
        /// </summary>
        public void Restart()
        {
            _timer.Restart();
        }
    }
}
