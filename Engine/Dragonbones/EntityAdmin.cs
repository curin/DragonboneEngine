using Dragonbones.Components;
using Dragonbones.Entities;
using Dragonbones.Native;
using Dragonbones.Systems;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Dragonbones
{
    /// <summary>
    /// A Default implementation for <see cref="IEntityAdmin"/>
    /// This implementation focuses on running the systems using a load balance system to allow as much rendering power when available, 
    /// but when not available attempting to give enough power so that updates can smoothly run
    /// </summary>
    public class EntityAdmin : IEntityAdmin
    {
        private Thread[] _threads;
        private AutoResetEvent[] _events;
        private PrecisionTimer[] _laneTimers;
        private int _logicLaneCount;
        private int _renderLaneCount;
        private readonly int _totalLaneCount;

        /// <summary>
        /// An array of all the threads used for computing
        /// index 0 is the main thread
        /// </summary>
        protected Thread[] GetThreads()
        {
            return _threads;
        }

        /// <summary>
        /// An array of all the threads used for computing
        /// index 0 is the main thread
        /// </summary>
        protected void SetThreads(Thread[] value)
        {
            _threads = value;
        }

        /// <summary>
        /// Event handlers used to wait on individual threads for all threads
        /// Main use is to wait when systemSchedule is conflicting
        /// </summary>
        protected AutoResetEvent[] GetEvents()
        {
            return _events;
        }

        /// <summary>
        /// Event handlers used to wait on individual threads for all threads
        /// Main use is to wait when systemSchedule is conflicting
        /// </summary>
        /// <param name="value">the new value for the events array</param>
        protected void SetEvents(AutoResetEvent[] value)
        {
            _events = value;
        }

        /// <summary>
        /// The barrier used to syncronize logic threads
        /// </summary>
        protected Barrier LogicBarrier { get; set; }
        /// <summary>
        /// The barrier used to syncronize render threads
        /// </summary>
        protected Barrier RenderBarrier { get; set; }
        /// <summary>
        /// The barrier used to syncronize all threads
        /// </summary>
        protected Barrier SystemBarrier { get; set; }
        /// <summary>
        /// is the game still running
        /// used to stop the game when finished and then cleanup after finished running
        /// </summary>
        protected bool Running { get; set; }
        /// <summary>
        /// The current schedule being used for logic systems
        /// </summary>
        protected ISystemSchedule CurrentLogicSchedule { get; set; }
        /// <summary>
        /// The next schedule to be used or the old schedule if schedule was just replaced
        /// </summary>
        protected ISystemSchedule NewLogicSchedule { get; set; }
        /// <summary>
        /// Is a new schedule available for logic systems
        /// </summary>
        protected bool LogicScheduleAvailable { get; set; }
        /// <summary>
        /// The current schedule being used for render systems
        /// </summary>
        protected ISystemSchedule CurrentRenderSchedule { get; set; }
        /// <summary>
        /// The next schedule to be used or the old schedule if the schedule was just replaced
        /// </summary>
        protected ISystemSchedule NewRenderSchedule { get; set; }
        /// <summary>
        /// Is a new schedule available for render systems
        /// </summary>
        protected bool RenderScheduleAvailable { get; set; }
        /// <summary>
        /// The amount of time in seconds between logic runs
        /// </summary>
        protected float LogicDeltaTime { get; set; }
        /// <summary>
        /// The amount of time taken for all logic system to run
        /// </summary>
        protected float LogicTime { get; set; }
        /// <summary>
        /// The desired time interval in seconds between logic runs
        /// </summary>
        protected float LogicInterval { get; set; }
        /// <summary>
        /// The minimum time interval in seconds between render runs
        /// </summary>
        protected float MaxRenderInterval { get; set; }
        /// <summary>
        /// The amount of time in seconds between render runs
        /// </summary>
        protected float RenderDeltaTime { get; set; }
        /// <summary>
        /// The timer used to time logic threads
        /// </summary>
        protected PrecisionTimer LogicTimer { get; set; }
        /// <summary>
        /// The timer used to time render threads
        /// </summary>
        protected PrecisionTimer RenderTimer { get; set; }

        /// <inheritdoc/>
        public IComponentTypeRegistry Components { get; set; }
        /// <inheritdoc/>
        public ISystemRegistry Systems { get; set; }
        /// <inheritdoc/>
        public IEntityBuffer Entities { get; set; }

        ///<inheritdoc/>
        public ISystemSchedule LogicSchedule
        {
            get => NewLogicSchedule; 
            set
            {
                if (CurrentLogicSchedule == null)
                {
                    CurrentLogicSchedule = value;
                    return;
                }

                NewLogicSchedule = value;
                LogicScheduleAvailable = true;
            }
        }
        ///<inheritdoc/>
        public ISystemSchedule RenderSchedule 
        { 
            get => NewRenderSchedule; 
            set
            {
                if (CurrentRenderSchedule == null)
                {
                    CurrentRenderSchedule = value;
                    return;
                }

                NewRenderSchedule = value;
                RenderScheduleAvailable = true;
            }
        }

        ///<inheritdoc/>
        public int LogicLaneCount => _logicLaneCount;

        ///<inheritdoc/>
        public int RenderLaneCount => _renderLaneCount;

        ///<inheritdoc/>
        public int TotalLaneCount => _totalLaneCount;

        /// <summary>
        /// A constructor for the EntityAdmin
        /// </summary>
        /// <param name="logicUpdateInterval">the minimum interval of time for it to run updates in seconds</param>
        /// <param name="maxRenderInterval">the maximum desired interval for it to take for a render frame</param>
        /// <param name="components">the <see cref="IComponentTypeRegistry"/> to use to store component buffers</param>
        /// <param name="systems">the <see cref="ISystemRegistry"/> to use to store systems</param>
        /// <param name="entities">the <see cref="IEntityBuffer"/> to use to store entities</param>
        /// <param name="links">the <see cref="ILinkBuffer"/> to use to store links between entities and components</param>
        public EntityAdmin(float logicUpdateInterval, float maxRenderInterval, IComponentTypeRegistry components, ISystemRegistry systems, IEntityBuffer entities)
        {
            MaxRenderInterval = maxRenderInterval;
            LogicInterval = logicUpdateInterval;
            Components = components;
            Systems = systems;
            Entities = entities;
#pragma warning disable CA1062 // Validate arguments of public methods
            Systems.SetAdmin(this);
#pragma warning restore CA1062 // Validate arguments of public methods
            _logicLaneCount = Environment.ProcessorCount;
            _logicLaneCount >>= 1;
            if (_logicLaneCount == 0)
                _logicLaneCount = 1;
            _renderLaneCount = _logicLaneCount;
            _totalLaneCount = _renderLaneCount + _logicLaneCount;
        }

        ///<inheritdoc/>
        public void Run()
        {
            LogicTimer = new PrecisionTimer();
            RenderTimer = new PrecisionTimer();
            RenderTimer.Start();

            _threads = new Thread[_totalLaneCount];
            _laneTimers = new PrecisionTimer[_totalLaneCount];
            _events = new AutoResetEvent[_totalLaneCount];

            _threads[0] = Thread.CurrentThread;
            _threads[_totalLaneCount - 1] = new Thread(MainRenderMethod);


            for (int i = 0; i < _totalLaneCount; i++)
            {
                _events[i] = new AutoResetEvent(false);
                _laneTimers[i] = new PrecisionTimer();

                if (i > 0 && i < _totalLaneCount - 1)
                    _threads[i] = new Thread(SecondaryMethod);
            }
            
            LogicBarrier = new Barrier(_logicLaneCount);
            RenderBarrier = new Barrier(_renderLaneCount);
            SystemBarrier = new Barrier(_logicLaneCount + _renderLaneCount);

            if (CurrentLogicSchedule == null)
                CurrentLogicSchedule = Systems.CreateSchedule(SystemType.Logic, _totalLaneCount);

            if (CurrentRenderSchedule == null)
                CurrentRenderSchedule = Systems.CreateSchedule(SystemType.Logic, _totalLaneCount);

            LogicTime = LogicInterval;
            RenderDeltaTime = MaxRenderInterval;

            Running = true;

            for (int i = 1; i < _totalLaneCount; i++)
            {
                _threads[i].Start(i);
            }

            MainLogicMethod();
        }

        /// <summary>
        /// The method run on the main logic thread
        /// This should be used to handle anything that should be done only once and used to control the other threads
        /// Logic threads should handle the logic systems
        /// Main priority is running logic systems and swapping write buffers
        /// 
        /// </summary>
        protected virtual void MainLogicMethod()
        {
            int laneID = 0;
            SystemInitialize(laneID, CurrentLogicSchedule);

            CurrentLogicSchedule.Reset();

            Components.SwapWriteBuffer();
            Entities.SwapWriteBuffer();
            SystemBarrier.SignalAndWait();

            do
            {
                LogicTimer.Stop();
                LogicDeltaTime = LogicTimer.ElapsedSecondsF;
                LogicTimer.Reset();
                LogicTimer.Start();

                float renderTime = RenderDeltaTime;
                bool condition = LogicTime < LogicInterval;
                if (condition || renderTime < MaxRenderInterval)
                {
                    float percent = condition ? LogicTime / LogicInterval : renderTime / MaxRenderInterval;
                    int count = condition ? _logicLaneCount - 1 : _totalLaneCount - 2;
                    _logicLaneCount = (int)(condition ? MathF.Ceiling(percent * count) : MathF.Truncate(percent * count)) + 1;
                    _renderLaneCount = _totalLaneCount - _logicLaneCount;
                }

                LogicBarrier.SignalAndWait();

                if (_logicLaneCount < LogicBarrier.ParticipantCount)
                    LogicBarrier.RemoveParticipants(LogicBarrier.ParticipantCount - _logicLaneCount);

                if (_logicLaneCount > LogicBarrier.ParticipantCount)
                    LogicBarrier.AddParticipants(_logicLaneCount - LogicBarrier.ParticipantCount);

                SystemRun(laneID, CurrentLogicSchedule, LogicTimer, LogicInterval, LogicDeltaTime);

                LogicBarrier.SignalAndWait();
                if (LogicScheduleAvailable)
                {
                    ISystemSchedule temp = CurrentLogicSchedule;
                    CurrentLogicSchedule = NewLogicSchedule;
                    NewLogicSchedule = temp;
                    LogicScheduleAvailable = false;
                }
                CurrentLogicSchedule.Reset();

                Components.SwapWriteBuffer();
                Entities.SwapWriteBuffer();
                LogicTime = LogicTimer.ElapsedSecondsF;
                SpinWait.SpinUntil(() => { return LogicTimer.ElapsedSecondsF >= LogicInterval; });
            } while (Running);

            LogicBarrier.SignalAndWait();
            SystemBarrier.SignalAndWait();
            SystemDispose(laneID, CurrentLogicSchedule);
        }

        /// <summary>
        /// The method run on secondary threads
        /// Use this to supplement the computing power of the main threads
        /// Do not count on having any threads run this method
        /// </summary>
        /// <param name="laneObject">the index of this thread (index 0 is reserved for the main thread)</param>
        protected virtual void SecondaryMethod(object laneObject)
        {
            int lane = (int)laneObject;
            
            if (lane < _logicLaneCount)
            {
                SystemInitialize(lane, CurrentLogicSchedule);
                SystemBarrier.SignalAndWait();
                LogicBarrier.SignalAndWait();
            }
            else
            {
                SystemInitialize(lane, CurrentRenderSchedule);
                SystemBarrier.SignalAndWait();
                RenderBarrier.SignalAndWait();
            }

            do
            {
                if (lane < _logicLaneCount)
                {
                    SystemRun(lane, CurrentLogicSchedule, LogicTimer, LogicInterval, LogicDeltaTime);
                    LogicBarrier.SignalAndWait();
                    LogicBarrier.SignalAndWait();
                }
                else
                {
                    SystemRun(lane, CurrentRenderSchedule, RenderTimer, MaxRenderInterval, RenderDeltaTime);
                    RenderBarrier.SignalAndWait();
                    RenderBarrier.SignalAndWait();
                }
                
            } while (Running);

            SystemBarrier.SignalAndWait();
            if (lane < _logicLaneCount)
                SystemDispose(lane, CurrentLogicSchedule);
            else
                SystemDispose(lane, CurrentRenderSchedule);
        }

        /// <summary>
        /// The main thread for rendering
        /// This should be used to handle anything that should be done only once and used to control the other threads
        /// </summary>
        protected virtual void MainRenderMethod(object lane)
        {
            int laneID = (int)lane;
            SystemInitialize(laneID, CurrentRenderSchedule);

            SystemBarrier.SignalAndWait();
            do
            {
                CurrentRenderSchedule.Reset();

                Components.SwapReadBuffer();
                Entities.SwapReadBuffer();

                RenderTimer.Stop();
                RenderDeltaTime = RenderTimer.ElapsedSecondsF;
                RenderTimer.Reset();

                RenderBarrier.SignalAndWait();
                RenderTimer.Start();

                if (_renderLaneCount < RenderBarrier.ParticipantCount)
                    RenderBarrier.RemoveParticipants(RenderBarrier.ParticipantCount - _renderLaneCount);

                if (_renderLaneCount > RenderBarrier.ParticipantCount)
                    RenderBarrier.AddParticipants(_renderLaneCount - RenderBarrier.ParticipantCount);

                SystemRun(laneID, CurrentRenderSchedule, RenderTimer, MaxRenderInterval, RenderDeltaTime);

                RenderBarrier.SignalAndWait();

                if (RenderScheduleAvailable)
                {
                    ISystemSchedule temp = CurrentRenderSchedule;
                    CurrentRenderSchedule = NewRenderSchedule;
                    NewRenderSchedule = temp;
                    RenderScheduleAvailable = false;
                }

            } while (Running);

            RenderBarrier.SignalAndWait();
            SystemBarrier.SignalAndWait();
            SystemDispose(laneID, CurrentRenderSchedule);
        }

        /// <summary>
        /// The general method for how systems are run.
        /// </summary>
        /// <param name="laneID">what lane is calling</param>
        /// <param name="schedule">the schedule of systems to run</param>
        /// <param name="time">The current time in the frame</param>
        /// <param name="interval">the maximum amount of time to take on a frame</param>
        /// <param name="deltaTime">the time since last run</param>
        private void SystemRun(int laneID, ISystemSchedule schedule, PrecisionTimer time, float interval, float deltaTime)
        {
            ScheduleResult result = schedule.NextSystem(laneID, out SystemInfo sysInf);

            while (result == ScheduleResult.Conflict)
            {
                _events[laneID].WaitOne();
                result = time.ElapsedSecondsF < interval ? schedule.NextSystem(laneID, out sysInf) : ScheduleResult.Finished;
            }

            while (result == ScheduleResult.Supplied)
            {
                ISystem system = Systems.GetSystem(sysInf.ID);

                _laneTimers[laneID].Start();
                system.Run(deltaTime);
                _laneTimers[laneID].Stop();

                sysInf.Update(_laneTimers[laneID].ElapsedSeconds);
                _laneTimers[laneID].Reset();

                result = time.ElapsedSecondsF < interval ? schedule.NextSystem(laneID, out sysInf) : ScheduleResult.Finished;

                int count = result == ScheduleResult.Conflict ? 0 : laneID < _logicLaneCount ? _logicLaneCount : _events.Length;

                for(int i = laneID < _logicLaneCount ? 0 : _logicLaneCount; i < count; i++)
                    _events[i].Set();

                while (result == ScheduleResult.Conflict)
                {
                    _events[laneID].WaitOne();
                    result = time.ElapsedSecondsF < interval ? schedule.NextSystem(laneID, out sysInf) : ScheduleResult.Finished;
                }
            }
        }

        /// <summary>
        /// The general method for how systems are initialized.
        /// </summary>
        /// <param name="laneID">what lane is calling</param>
        /// <param name="schedule">the schedule of systems to run</param>
        private void SystemInitialize(int laneID, ISystemSchedule schedule)
        {
            ScheduleResult result = schedule.NextSystem(laneID, out SystemInfo sysInf);

            while (result == ScheduleResult.Conflict)
            {
                _events[laneID].WaitOne();
                result = schedule.NextSystem(laneID, out sysInf);
            }

            while (result == ScheduleResult.Supplied)
            {
                ISystem system = Systems.GetSystem(sysInf.ID);
                system.Initialize();

                result = schedule.NextSystem(laneID, out sysInf);

                int count = result == ScheduleResult.Conflict ? 0 : laneID < _logicLaneCount ? _logicLaneCount : _events.Length;
                for (int i = laneID < _logicLaneCount ? 0 : _logicLaneCount; i < count; i++)
                    _events[i].Set();

                while (result == ScheduleResult.Conflict)
                {
                    _events[laneID].WaitOne();
                    result = schedule.NextSystem(laneID, out sysInf);
                }
            }
        }

        /// <summary>
        /// The general method for how systems are initialized.
        /// </summary>
        /// <param name="laneID">what lane is calling</param>
        /// <param name="schedule">the schedule of systems to run</param>
        private void SystemDispose(int laneID, ISystemSchedule schedule)
        {
            ScheduleResult result = schedule.NextSystem(laneID, out SystemInfo sysInf);

            while (result == ScheduleResult.Conflict)
            {
                _events[laneID].WaitOne();
                result = schedule.NextSystem(laneID, out sysInf);
            }

            while (result == ScheduleResult.Supplied)
            {
                ISystem system = Systems.GetSystem(sysInf.ID);
                system.Dispose();

                result = schedule.NextSystem(laneID, out sysInf);

                int count = result == ScheduleResult.Conflict ? 0 : laneID < _logicLaneCount ? _logicLaneCount : _events.Length;

                for (int i = laneID < _logicLaneCount ? 0 : _logicLaneCount; i < count; i++)
                    _events[i].Set();

                while (result == ScheduleResult.Conflict)
                {
                    _events[laneID].WaitOne();
                    result = schedule.NextSystem(laneID, out sysInf);
                }
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        /// <summary>
        /// Disposes this object
        /// </summary>
        /// <param name="disposing">should the managed objects also be disposed</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        ///<inheritdoc/>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
