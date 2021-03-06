﻿using Neptuo;
using Neptuo.Events.Handlers;
using Neptuo.Observables.Collections;
using Neptuo.Productivity.ActivityLog.Events;
using Neptuo.Productivity.ActivityLog.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neptuo.Productivity.ActivityLog.ViewModels
{
    public class TodayOverviewViewModel : IEventHandler<ActivityStarted>, IEventHandler<ActivityEnded>, IDisposable
    {
        private readonly ITimer timer;
        private readonly IDateTimeProvider dateTimeProvider;
        private readonly IApplicationNameProvider applicationNameProvider;
        private readonly ObservableCollection<ActivityOverviewViewModel> activities;

        public IReadOnlyList<ActivityOverviewViewModel> Activities
        {
            get { return activities; }
        }

        public TodayOverviewViewModel(ITimer timer, IDateTimeProvider dateTimeProvider, IApplicationNameProvider applicationNameProvider)
        {
            Ensure.NotNull(timer, "timer");
            Ensure.NotNull(dateTimeProvider, "dateTimeProvider");
            Ensure.NotNull(applicationNameProvider, "applicationNameProvider");
            this.timer = timer;
            this.timer.Tick += OnTimerTick;
            this.dateTimeProvider = dateTimeProvider;
            this.applicationNameProvider = applicationNameProvider;

            activities = new ObservableCollection<ActivityOverviewViewModel>();
        }

        private void OnTimerTick()
        {
            DateTime now = dateTimeProvider.Now();
            foreach (ActivityOverviewViewModel item in activities)
            {
                if (item.IsForeground)
                    item.Update(now);
            }
        }

        Task IEventHandler<ActivityStarted>.HandleAsync(ActivityStarted payload)
        {
            bool hasItem = false;

            foreach (ActivityOverviewViewModel item in activities)
            {
                if (payload.ApplicationPath == item.ApplicationPath)
                {
                    item.CurrentTitle = payload.WindowTitle;
                    item.StartAt(payload.StartedAt);
                    hasItem = true;
                }
            }

            if (!hasItem)
            {
                ActivityOverviewViewModel newItem = new ActivityOverviewViewModel(
                    applicationNameProvider.GetName(payload.ApplicationPath),
                    payload.ApplicationPath
                );
                newItem.CurrentTitle = payload.WindowTitle;
                newItem.StartAt(payload.StartedAt);
                activities.Add(newItem);
            }

            return Task.CompletedTask;
        }

        Task IEventHandler<ActivityEnded>.HandleAsync(ActivityEnded payload)
        {
            foreach (ActivityOverviewViewModel item in activities)
            {
                if (payload.ApplicationPath == item.ApplicationPath)
                    item.StopAt(payload.EndedAt);
            }

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            timer.Tick -= OnTimerTick;
        }
    }
}
