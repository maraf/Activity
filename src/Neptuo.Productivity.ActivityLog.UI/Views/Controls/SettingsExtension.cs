﻿using Neptuo.Productivity.ActivityLog.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace Neptuo.Productivity.ActivityLog.Views.Controls
{
    /// <summary>
    /// Provides value from <see cref="SettingsBase"/> class.
    /// </summary>
    /// <remarks>
    /// To use it a <see cref="Settings"/> must be set before.
    /// </remarks>
    public class SettingsExtension : MarkupExtension
    {
        private DependencyProperty property;

        /// <summary>
        /// Gets or sets a collection of settings to use.
        /// </summary>
        public static SettingsBase Settings { get; set; }

        /// <summary>
        /// Gets a key to read from the <see cref="Settings"/>.
        /// </summary>
        public string Key { get; private set; }

        /// <summary>
        /// Gets or sets a converter to convert a value from the <see cref="Settings"/>.
        /// </summary>
        public IValueConverter Converter { get; set; }

        /// <summary>
        /// Gets or sets a parameter for the <see cref="Converter"/>.
        /// </summary>
        public object ConverterParameter { get; set; }

        /// <summary>
        /// Gets or sets if changed value should be automatically saved to settings.
        /// </summary>
        public bool IsTwoWayMode { get; set; }

        /// <summary>
        /// Creates a new instance that provides value from <see cref="Settings"/> associated with <paramref name="key"/>.
        /// </summary>
        /// <param name="key">A key to read from the <see cref="Settings"/>.</param>
        public SettingsExtension(string key)
        {
            Ensure.NotNullOrEmpty(key, "key");
            EnsureKey(key);
            Key = key;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            IProvideValueTarget provideValueTarget = (IProvideValueTarget)serviceProvider.GetService(typeof(IProvideValueTarget));
            property = provideValueTarget.TargetProperty as DependencyProperty;

            EnsureDesignTimeSettings(provideValueTarget);
            EnsureSettings();
            object value = Settings[Key];
            if (Converter != null)
                value = Converter.Convert(value, property?.PropertyType ?? typeof(object), ConverterParameter, Thread.CurrentThread.CurrentUICulture);

            if (IsTwoWayMode && property != null)
            {
                DependencyPropertyDescriptor
                    .FromProperty(property, provideValueTarget.TargetObject.GetType())
                    .AddValueChanged(provideValueTarget.TargetObject, OnPropertyChanged);
            }

            return value;
        }

        private void OnPropertyChanged(object sender, EventArgs e)
        {
            DependencyObject target = (DependencyObject)sender;
            object value = target.GetValue(property);

            if (Converter != null)
                value = Converter.ConvertBack(value, Settings[Key]?.GetType() ?? typeof(string), ConverterParameter, Thread.CurrentThread.CurrentUICulture);

            if (Settings[Key] != value)
            {
                Settings[Key] = value;
                Settings.Save();
            }
        }

        private static void EnsureSettings()
        {
            if (Settings == null)
                throw Ensure.Exception.InvalidOperation($"Missing '{nameof(SettingsExtension)}.{nameof(Settings)}' which is null.");
        }

        [Conditional("DEBUG")]
        private void EnsureDesignTimeSettings(IProvideValueTarget provideValueTarget)
        {
            DependencyObject target = provideValueTarget.TargetObject as DependencyObject;
            if (DesignerProperties.GetIsInDesignMode(target))
                Settings = new Settings();
        }

        [Conditional("DEBUG")]
        private static void EnsureKey(string key)
        {
            Debug.Assert(
                typeof(Settings).GetProperty(key) != null,
                $"Missing settings property '{key}'."
            );
        }
    }
}
