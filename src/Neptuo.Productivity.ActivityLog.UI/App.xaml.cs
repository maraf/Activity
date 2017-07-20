﻿using Neptuo.Events;
using Neptuo.Formatters;
using Neptuo.Productivity.ActivityLog.Data;
using Neptuo.Productivity.ActivityLog.Events;
using Neptuo.Productivity.ActivityLog.Formatters;
using Neptuo.Productivity.ActivityLog.Services;
using Neptuo.Productivity.ActivityLog.ViewModels;
using Neptuo.Productivity.ActivityLog.Views;
using Neptuo.Windows.Threading;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;

namespace Neptuo.Productivity.ActivityLog
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application, ITimer, ISynchronizer
    {
        private DispatcherHelper dispatcher;
        private Timer timer;
        private DomainService service;

        public event Action Tick;

        public void Run(Action handler)
        {
            dispatcher.Run(handler);
        }

        private void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            if (Tick != null)
                Tick();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            DefaultEventManager eventManager = new DefaultEventManager();

            dispatcher = new DispatcherHelper(Dispatcher);
            timer = new Timer(1000);
            timer.Elapsed += OnTimerElapsed;
            timer.Start();

            OverviewViewModel viewModel = new OverviewViewModel(
                this,
                this,
                new DateTimeProvider(),
                new ApplicationNameProvider()
            );

            eventManager.AddAll(viewModel);

            service = new DomainService(eventManager);

            SimpleFormatter formatter = new SimpleFormatter();
            MainWindow wnd = new MainWindow();
            wnd.DataContext = viewModel;

            //string todayFile = GetFileName(DateTime.Today);
            //if (File.Exists(todayFile))
            //{
            //    using (Stream file = File.OpenRead(todayFile))
            //    {
            //        IDeserializerContext context = new DefaultDeserializerContext(typeof(IEvent));
            //        while (formatter.TryDeserialize(file, context))
            //        {
            //            if (context.Output is ActivityStarted started)
            //                eventManager.PublishAsync(started).Wait();
            //            else if (context.Output is ActivityEnded ended)
            //                eventManager.PublishAsync(ended).Wait();
            //        }
            //    }
            //}

            FileEventStore store = new FileEventStore(formatter, GetFileName);
            eventManager.AddAll(new EventStoreHandler(store));

            wnd.Show();
        }

        private static string GetFileName(DateTime dateTime)
        {
            return $"{dateTime.ToString("yyyy-MM-dd")}.alog";
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);

            timer.Dispose();
            service.Dispose();
        }
    }
}
