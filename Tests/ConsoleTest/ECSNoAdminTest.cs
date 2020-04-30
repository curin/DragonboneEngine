using System;
using System.Collections.Generic;
using System.Text;
using Dragonbones.Systems;
using Dragonbones.Components;
using Dragonbones.Entities;
using Dragonbones.Native;
using System.Diagnostics.CodeAnalysis;

namespace ConsoleTest
{
    public static class ECSNoAdminTest
    {
        public static void Run()
        {
            PrecisionTimer timer = new PrecisionTimer();
            SystemRegistry systems = new SystemRegistry(64);
            ComponentTypeRegistry components = new ComponentTypeRegistry(64);
            EntityBuffer entities = new EntityBuffer();
            LinkBuffer links = new LinkBuffer(64, 64);

            timer.Start();
            int c = components.Register(new ComponentBuffer<TestComponent>("TestComponent", 64));
            IComponentBuffer<TestComponent> tests = components.Get<TestComponent>("TestComponent");
            int b = entities.Add(SystemType.SystemWriteRead, "Ben");
            int t = entities.Add(SystemType.SystemWriteRead, "Tom");
            int bc = tests.Add(SystemType.SystemWriteRead, "Ben.Test", new TestComponent() { Test = "Hello Tom!" });
            int tc = tests.Add(SystemType.SystemWriteRead, "Tom.Test", new TestComponent() { Test = "Hello Ben!" });
            links.Add(SystemType.SystemWriteRead, c, b, bc);
            links.Add(SystemType.SystemWriteRead, c, t, tc);
            
            entities.SwapWriteBuffer();
            tests.SwapWriteBuffer();
            links.SwapWriteBuffer();
            entities.SwapReadBuffer();
            tests.SwapReadBuffer();
            links.SwapReadBuffer();
            

            systems.Register(new TestSystem(links, components, entities));
            systems.Register(new TestLogic(links, components, entities, tests));

            timer.Stop();
            float setupTime = timer.ElapsedSecondsF;

            timer.Reset();
            timer.Start();
            SafeSystemSchedule logicSchedule = new SafeSystemSchedule(SystemType.Logic, 1, 64);
            logicSchedule.AddFromRegistry(systems);

            SystemSchedule renderSchedule = new SystemSchedule(SystemType.Render, 1, 64);
            renderSchedule.AddFromRegistry(systems);

            while (renderSchedule.NextSystem(0, out SystemInfo sysInf) == ScheduleResult.Supplied)
            {
                ISystem sys = systems.GetSystem(sysInf.ID);
                sys.Run(1f / 60f);
            }

            timer.Reset();
            timer.Start();
            while (logicSchedule.NextSystem(0, out SystemInfo sysInf) == ScheduleResult.Supplied)
            {
                
                
                ISystem sys = systems.GetSystem(sysInf.ID);
                sys.Run(1f / 60f);
                
            }
            timer.Stop();
            float runTime = timer.ElapsedSecondsF;

            entities.SwapWriteBuffer();
            tests.SwapWriteBuffer();
            links.SwapWriteBuffer();
            entities.SwapReadBuffer();
            tests.SwapReadBuffer();
            links.SwapReadBuffer();
            logicSchedule.Reset();
            renderSchedule.Reset();

            

            Console.ReadLine();

            while (renderSchedule.NextSystem(0, out SystemInfo sysInf) == ScheduleResult.Supplied)
            {
                ISystem sys = systems.GetSystem(sysInf.ID);
                sys.Run(1f / 60f);
            }

            while (logicSchedule.NextSystem(0, out SystemInfo sysInf) == ScheduleResult.Supplied)
            {
                ISystem sys = systems.GetSystem(sysInf.ID);
                sys.Run(1f / 60f);
            }

            Console.ReadLine();

            Console.WriteLine("SetupTime: " + setupTime);
            Console.WriteLine("RunTime: " + runTime);
            Console.ReadLine();
        }
    }

    public struct TestComponent : IEquatable<TestComponent>
    {
        public string Test { get; set; }

        public bool Equals([AllowNull] TestComponent other)
        {
            return other.Test == Test;
        }
    }

    public class TestSystem : ISystem
    {
        public TestSystem(ILinkBuffer links, IComponentTypeRegistry components, IEntityBuffer entities)
        {
            _links = links;
            _components = components;
            _entities = entities;

            string[] used = _sysInf.GetComponentsUsed();
            int[] ids = new int[used.Length];
            for (int i = 0; i < used.Length; i++)
                ids[i] = _components.GetID(used[i]);
            _sysInf.SetComponentIDs(ids);
        }

        ILinkBuffer _links;
        IComponentTypeRegistry _components;
        IEntityBuffer _entities;
        private SystemInfo _sysInf = new SystemInfo("TestSystem", 0, 0, SystemType.Render, true, "TestComponent");

        public SystemInfo SystemInfo => _sysInf;

        public bool Equals([AllowNull] ISystem other)
        {
            return other.SystemInfo.Name == _sysInf.Name;
        }

        public void Run(float deltaTime)
        {
            List<EntityComponentLink[]> entityLinks = _links.GetLinks(_sysInf.Type, _sysInf.GetComponentsUsedIDs());
            IComponentBuffer<TestComponent> tests = _components.Get<TestComponent>(_sysInf.GetComponentsUsedIDs()[0]);
            foreach (EntityComponentLink[] links in entityLinks)
            {
                Console.WriteLine(_entities.GetName(_sysInf.Type, links[0].EntityID) + ": " + tests.Get(_sysInf.Type, links[0].ComponentID).Test);
            }
        }
    }

    public class TestLogic : ISystem
    {
        public TestLogic(ILinkBuffer links, IComponentTypeRegistry components, IEntityBuffer entities, IComponentBuffer<TestComponent> tests)
        {
            _links = links;
            _components = tests;
            _entities = entities;

            string[] used = _sysInf.GetComponentsUsed();
            int[] ids = new int[used.Length];
            for (int i = 0; i < used.Length; i++)
                ids[i] = components.GetID(used[i]);
            _sysInf.SetComponentIDs(ids);
        }

        PrecisionTimer _timer = new PrecisionTimer();
        ILinkBuffer _links;
        IComponentBuffer<TestComponent> _components;
        IEntityBuffer _entities;
        private SystemInfo _sysInf = new SystemInfo("TestLogic", 0, 0, SystemType.Logic, true, "TestComponent");

        public SystemInfo SystemInfo => _sysInf;

        public bool Equals([AllowNull] ISystem other)
        {
            return other.SystemInfo.Name == _sysInf.Name;
        }

        public void Run(float deltaTime)
        {
            _timer.Start();
            int e = _entities.Add(_sysInf.Type, "Lewis");
            _components.Set(_sysInf.Type, 0, new TestComponent() { Test = "Hello Lewis!" });
            _components.Set(_sysInf.Type, 1, new TestComponent() { Test = "Hello Lewis!" });
            int c = _components.Add(_sysInf.Type, "Lewis.Test", new TestComponent() { Test = "Sharky and Palp!" });
            _links.Add(_sysInf.Type, _sysInf.GetComponentsUsedIDs()[0], e, c);
            _timer.Stop();
            Console.WriteLine(_timer.ElapsedSecondsF);
            _timer.Reset();
        }
    }
}
