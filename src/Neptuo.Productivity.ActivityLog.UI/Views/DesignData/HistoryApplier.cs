﻿using Neptuo.Productivity.ActivityLog.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neptuo.Productivity.ActivityLog.Views.DesignData
{
    public class HistoryApplier : IHistoryApplier
    {
        public Task Apply(object handler, DateTime date)
        {
            return Task.CompletedTask;
        }

        public Task Apply(object handler, DateTime dateFrom, DateTime dateTo)
        {
            return Task.CompletedTask;
        }
    }
}
