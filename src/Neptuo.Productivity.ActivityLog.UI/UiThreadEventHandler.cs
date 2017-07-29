﻿using Neptuo;
using Neptuo.Events;
using Neptuo.Events.Handlers;
using Neptuo.Productivity.ActivityLog.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neptuo.Productivity.ActivityLog
{
    public abstract class UiThreadEventHandler
    {
        public abstract void Add(IEventHandlerCollection handlers);
        public abstract void Remove(IEventHandlerCollection handlers);
    }

    public class UiThreadEventHandler<T> : UiThreadEventHandler, IEventHandler<T>
    {
        private readonly IEventHandler<T> handler;
        private readonly ISynchronizer synchronizer;

        public UiThreadEventHandler(IEventHandler<T> handler, ISynchronizer synchronizer)
        {
            Ensure.NotNull(handler, "handler");
            Ensure.NotNull(synchronizer, "synchronizer");
            this.handler = handler;
            this.synchronizer = synchronizer;
        }

        public Task HandleAsync(T payload)
        {
            TaskCompletionSource<object> source = new TaskCompletionSource<object>();
            synchronizer.Run(() =>
            {
                handler.HandleAsync(payload).ContinueWith(t =>
                {
                    if (t.IsFaulted)
                        source.SetException(t.Exception);
                    else if (t.IsCanceled)
                        source.SetCanceled();
                    else
                        source.SetResult(null);
                });
            });

            return source.Task;
        }

        public override void Add(IEventHandlerCollection handlers)
        {
            Ensure.NotNull(handlers, "handlers");
            handlers.Add(this);
        }

        public override void Remove(IEventHandlerCollection handlers)
        {
            Ensure.NotNull(handlers, "handlers");
            handlers.Remove(this);
        }
    }

    public static class UiThreadEventHandlerExtensions
    {
        public static UiThreadEventHandler<T> AddUiThread<T>(this IEventHandlerCollection handlers, IEventHandler<T> handler, ISynchronizer synchronizer)
        {
            Ensure.NotNull(handlers, "handlers");
            UiThreadEventHandler<T> threadHandler = new UiThreadEventHandler<T>(handler, synchronizer);
            handlers.Add<T>(threadHandler);
            return threadHandler;
        }
    }
}
