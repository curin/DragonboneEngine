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
    public class EntityAdmin : IEntityAdmin
    {
        private Thread[] _renderThreads;
        private Thread[] _logicThreads;
        private AutoResetEvent[] _logicEvents;
        private AutoResetEvent[] _renderEvents;

        /// <summary>
        /// An array of all threads used for rendering
        /// index 0 is the main thread
        /// </summary>
        protected Thread[] GetRenderThreads()
        {
            return _renderThreads;
        }

        /// <summary>
        /// An array of all threads used for rendering
        /// index 0 is the main thread
        /// </summary>
        protected void SetRenderThreads(Thread[] value)
        {
            _renderThreads = value;
        }

        /// <summary>
        /// An array of all the threads used for logic
        /// index 0 is the main thread
        /// </summary>
        protected Thread[] GetLogicThreads()
        {
            return _logicThreads;
        }

        /// <summary>
        /// An array of all the threads used for logic
        /// index 0 is the main thread
        /// </summary>
        protected void SetLogicThreads(Thread[] value)
        {
            _logicThreads = value;
        }

        /// <summary>
        /// Event handlers used to wait on individual threads for logic threads
        /// Main use is to wait when systemSchedule is conflicting
        /// </summary>
        protected AutoResetEvent[] GetLogicEvents()
        {
            return _logicEvents;
        }

        /// <summary>
        /// Event handlers used to wait on individual threads for logic threads
        /// Main use is to wait when systemSchedule is conflicting
        /// </summary>
        /// <param name="value">the new value for the events array</param>
        protected void SetLogicEvents(AutoResetEvent[] value)
        {
            _logicEvents = value;
        }

        /// <summary>
        /// Event handlers used to wait on individual threads for render threads
        /// Main use is to wait when systemSchedule is conflicting
        /// </summary>
        protected AutoResetEvent[] GetRenderEvents()
        {
            return _renderEvents;
        }

        /// <summary>
        /// Event handlers used to wait on individual threads for render threads
        /// Main use is to wait when systemSchedule is conflicting
        /// </summary>
        /// <param name="value">the new value for the events array</param>
        protected void SetRenderEvents(AutoResetEvent[] value)
        {
            _renderEvents = value;
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
        protected ISystemSchedule LogicSchedule { get; set; }
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
        protected ISystemSchedule RenderSchedule { get; set; }
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
        /// The desired time interval in seconds between logic runs
        /// </summary>
        protected float LogicInterval { get; set; }
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
        /// <inheritdoc/>
        public ILinkBuffer Links { get; set; }


        public EntityAdmin(IComponentTypeRegistry components, ISystemRegistry systems, IEntityBuffer entities, ILinkBuffer links)
        {
            Components = components;
            Systems = systems;
            Entities = entities;
            Links = links;
        }

        ///<inheritdoc/>
        public void Run()
        {
            LogicTimer = new PrecisionTimer();
            RenderTimer = new PrecisionTimer();

            int threadCount = Environment.ProcessorCount;
            threadCount >>= 1;
            if (threadCount == 0)
                threadCount = 1;

            _logicThreads = new Thread[threadCount];
            _renderThreads = new Thread[threadCount];
            _logicEvents = new AutoResetEvent[threadCount];
            _renderEvents = new AutoResetEvent[threadCount];

            _logicThreads[0] = Thread.CurrentThread;
            _renderThreads[0] = new Thread(MainRenderMethod);

            for (int i = 1; i < threadCount; i++)
            {
                _logicThreads[i] = new Thread(SecondaryLogicMethod);
                _renderThreads[i] = new Thread(SecondaryRenderMethod);
                _logicEvents[i] = new AutoResetEvent(false);
                _renderEvents[i] = new AutoResetEvent(false);
            }

            LogicBarrier = new Barrier(threadCount);
            RenderBarrier = new Barrier(threadCount);
            SystemBarrier = new Barrier(threadCount << 1);

            
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
            SystemInitialize(0, LogicSchedule, _logicEvents);

            LogicSchedule.Reset();

            Components.SwapWriteBuffer();
            Entities.SwapWriteBuffer();
            Links.SwapWriteBuffer();
            SystemBarrier.SignalAndWait();

            while (Running)
            {
                LogicTimer.Stop();
                LogicDeltaTime = LogicTimer.ElapsedSecondsF;
                LogicTimer.Reset();

                LogicBarrier.SignalAndWait();
                LogicTimer.Start();

                SystemRun(0, LogicSchedule, _logicEvents, LogicDeltaTime);

                LogicBarrier.SignalAndWait();
                if (LogicScheduleAvailable)
                {
                    ISystemSchedule temp = LogicSchedule;
                    LogicSchedule = NewLogicSchedule;
                    NewLogicSchedule = temp;
                    LogicScheduleAvailable = false;
                }
                LogicSchedule.Reset();

                Components.SwapWriteBuffer();
                Entities.SwapWriteBuffer();
                Links.SwapWriteBuffer();
                SpinWait.SpinUntil(() => { return LogicTimer.ElapsedSecondsF >= LogicInterval; });
            }

            SystemBarrier.SignalAndWait();
            SystemDispose(0, LogicSchedule, _logicEvents);
        }

        /// <summary>
        /// The method run on secondary logic threads
        /// Use this to supplement the computing power of the main thread
        /// Do not count on having any threads run this method
        /// </summary>
        /// <param name="laneObject">the index of this thread (index 0 is reserved for the main thread)</param>
        protected virtual void SecondaryLogicMethod(object laneObject)
        {
            int lane = (int)laneObject;
            SystemInitialize(lane, LogicSchedule, _logicEvents);

            SystemBarrier.SignalAndWait();
            LogicBarrier.SignalAndWait();

            while (Running)
            {
                SystemRun(lane, LogicSchedule, _logicEvents, LogicDeltaTime);

                LogicBarrier.SignalAndWait();
                LogicBarrier.SignalAndWait();
            }

            SystemBarrier.SignalAndWait();
            SystemDispose(lane, LogicSchedule, _logicEvents);
        }

        /// <summary>
        /// The main thread for rendering
        /// This should be used to handle anything that should be done only once and used to control the other threads
        /// </summary>
        protected virtual void MainRenderMethod()
        {
            SystemInitialize(0, RenderSchedule, _logicEvents);

            SystemBarrier.SignalAndWait();
            while (Running)
            {
                RenderSchedule.Reset();

                Components.SwapReadBuffer();
                Entities.SwapReadBuffer();
                Links.SwapReadBuffer();

                RenderTimer.Stop();
                RenderDeltaTime = LogicTimer.ElapsedSecondsF;
                RenderTimer.Reset();

                RenderBarrier.SignalAndWait();
                RenderTimer.Start();

                SystemRun(0, RenderSchedule, _renderEvents, RenderDeltaTime);

                RenderBarrier.SignalAndWait();
                if (RenderScheduleAvailable)
                {
                    ISystemSchedule temp = RenderSchedule;
                    RenderSchedule = NewRenderSchedule;
                    NewRenderSchedule = temp;
                    RenderScheduleAvailable = false;
                }
            }

            SystemBarrier.SignalAndWait();
            SystemDispose(0, RenderSchedule, _logicEvents);
        }

        /// <summary>
        /// The method run on secondary render threads
        /// Use this to supplement the computing power of the main thread
        /// Do not count on having any threads run this method
        /// </summary>
        /// <param name="laneObject">the index of this thread (index 0 is reserved for the main thread)</param>
        protected virtual void SecondaryRenderMethod(object laneObject)
        {
            int lane = (int)laneObject;
            SystemInitialize(lane, RenderSchedule, _renderEvents);

            SystemBarrier.SignalAndWait();
            RenderBarrier.SignalAndWait();

            while (Running)
            {
                SystemRun(lane, RenderSchedule, _renderEvents, RenderDeltaTime);

                RenderBarrier.SignalAndWait();
                RenderBarrier.SignalAndWait();
            }

            SystemBarrier.SignalAndWait();
            SystemDispose(lane, RenderSchedule, _renderEvents);
        }

        /// <summary>
        /// The general method for how systems are run.
        /// </summary>
        /// <param name="laneID">what lane is calling</param>
        /// <param name="schedule">the schedule of systems to run</param>
        /// <param name="events">the reset events used when a system is stopped due to conflict</param>
        /// <param name="deltaTime">the time since last run</param>
        private void SystemRun(int laneID, ISystemSchedule schedule, AutoResetEvent[] events, float deltaTime)
        {
            ScheduleResult result = schedule.NextSystem(laneID, out SystemInfo sysInf);

            while (result == ScheduleResult.Conflict)
            {
                events[laneID].WaitOne();
                result = schedule.NextSystem(laneID, out sysInf);
            }

            while (result == ScheduleResult.Supplied)
            {
                ISystem system = Systems.GetSystem(sysInf.ID);
                system.Run(deltaTime);

                result = schedule.NextSystem(laneID, out sysInf);
                if (result == ScheduleResult.Supplied)
                    for (int i = 0; i < events.Length; i++)
                        events[i].Set();

                while (result == ScheduleResult.Conflict)
                {
                    events[laneID].WaitOne();
                    result = schedule.NextSystem(laneID, out sysInf);
                }
            }
        }

        /// <summary>
        /// The general method for how systems are initialized.
        /// </summary>
        /// <param name="laneID">what lane is calling</param>
        /// <param name="schedule">the schedule of systems to run</param>
        /// <param name="events">the reset events used when a system is stopped due to conflict</param>
        private void SystemInitialize(int laneID, ISystemSchedule schedule, AutoResetEvent[] events)
        {
            ScheduleResult result = schedule.NextSystem(laneID, out SystemInfo sysInf);

            while (result == ScheduleResult.Conflict)
            {
                events[laneID].WaitOne();
                result = schedule.NextSystem(laneID, out sysInf);
            }

            while (result == ScheduleResult.Supplied)
            {
                ISystem system = Systems.GetSystem(sysInf.ID);
                system.Initialize();

                result = schedule.NextSystem(laneID, out sysInf);
                if (result == ScheduleResult.Supplied)
                    for (int i = 0; i < events.Length; i++)
                        events[i].Set();

                while (result == ScheduleResult.Conflict)
                {
                    events[laneID].WaitOne();
                    result = schedule.NextSystem(laneID, out sysInf);
                }
            }
        }

        /// <summary>
        /// The general method for how systems are initialized.
        /// </summary>
        /// <param name="laneID">what lane is calling</param>
        /// <param name="schedule">the schedule of systems to run</param>
        /// <param name="events">the reset events used when a system is stopped due to conflict</param>
        private void SystemDispose(int laneID, ISystemSchedule schedule, AutoResetEvent[] events)
        {
            ScheduleResult result = schedule.NextSystem(laneID, out SystemInfo sysInf);

            while (result == ScheduleResult.Conflict)
            {
                events[laneID].WaitOne();
                result = schedule.NextSystem(laneID, out sysInf);
            }

            while (result == ScheduleResult.Supplied)
            {
                ISystem system = Systems.GetSystem(sysInf.ID);
                system.Dispose();

                result = schedule.NextSystem(laneID, out sysInf);
                if (result == ScheduleResult.Supplied)
                    for (int i = 0; i < events.Length; i++)
                        events[i].Set();

                while (result == ScheduleResult.Conflict)
                {
                    events[laneID].WaitOne();
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
