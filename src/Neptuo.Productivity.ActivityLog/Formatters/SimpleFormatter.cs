﻿using Neptuo.Formatters;
using Neptuo.Productivity.ActivityLog.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Globalization;
using Neptuo.Events;

namespace Neptuo.Productivity.ActivityLog.Formatters
{
    public class SimpleFormatter : IFormatter
    {
        public const string DateTimeFormat = "yyyy-MM-dd_HH:mm:ss";

        public bool TryDeserialize(Stream input, IDeserializerContext context)
        {
            using (StreamReader reader = new StreamReader(input, Encoding.UTF8, false, 128, true))
            {
                List<IEvent> result = new List<IEvent>();
                string line;
                while (!string.IsNullOrEmpty((line = reader.ReadLine())))
                {
                    string[] parts = line.Split(';');

                    if (parts.Length != 5)
                        return false;

                    string rawVersion = parts[0];
                    string type = parts[1];
                    string applicationPath = parts[2];
                    string windowTitle = parts[3];
                    string rawDateTime = parts[4];

                    if (Int32.TryParse(rawVersion, out int version) && DateTime.TryParseExact(rawDateTime, DateTimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dateTime))
                    {
                        if (version == 1)
                        {
                            if (type == nameof(ActivityStarted))
                                result.Add(new ActivityStarted(applicationPath, windowTitle, dateTime));
                            else if (type == nameof(ActivityEnded))
                                result.Add(new ActivityEnded(applicationPath, windowTitle, dateTime));
                        }
                    }
                }

                context.Output = result;
                return true;
            }
        }

        public Task<bool> TryDeserializeAsync(Stream input, IDeserializerContext context)
        {
            return Task.FromResult(TryDeserialize(input, context));
        }

        public bool TrySerialize(object input, ISerializerContext context)
        {
            if (input is ActivityStarted started)
            {
                using (StreamWriter writer = new StreamWriter(context.Output, Encoding.UTF8, 128, true))
                    writer.WriteLine($"1;{nameof(ActivityStarted)};{started.ApplicationPath};{started.WindowTitle};{started.StartedAt.ToString(DateTimeFormat)}");

                return true;
            }
            else if (input is ActivityEnded ended)
            {
                using (StreamWriter writer = new StreamWriter(context.Output, Encoding.UTF8, 128, true))
                    writer.WriteLine($"1;{nameof(ActivityEnded)};{ended.ApplicationPath};{ended.WindowTitle};{ended.EndedAt.ToString(DateTimeFormat)}");

                return true;
            }

            return false;
        }

        public Task<bool> TrySerializeAsync(object input, ISerializerContext context)
        {
            return Task.FromResult(TrySerialize(input, context));
        }
    }
}
