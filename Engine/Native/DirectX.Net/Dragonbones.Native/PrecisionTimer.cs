using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace Dragonbones.Native
{
    public class PrecisionTimer
    {
        Stopwatch _timer;
        static double frequency = (double)Stopwatch.Frequency;
        static float frequencyF = (float)Stopwatch.Frequency;

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
        public double ElapsedSeconds => _timer.ElapsedTicks / frequency;

        /// <summary>
        /// the amount of seconds elapsed in single precision floating point value
        /// </summary>
        public float ElapsedSecondsF => _timer.ElapsedTicks / frequencyF;

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
