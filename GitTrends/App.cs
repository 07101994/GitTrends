﻿using System;
using System.Diagnostics;
using AsyncAwaitBestPractices;
using Autofac;
using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;

namespace GitTrends
{
    public class App : Xamarin.Forms.Application
    {
        readonly WeakEventManager<Theme> _themeChangedEventManager = new WeakEventManager<Theme>();

        public App()
        {
            FFImageLoading.ImageService.Instance.Initialize(new FFImageLoading.Config.Configuration
            {
                HttpHeadersTimeout = 60
            });

            using var scope = ContainerService.Container.BeginLifetimeScope();
            MainPage = scope.Resolve<SplashScreenPage>();

            On<iOS>().SetHandleControlUpdatesOnMainThread(true);
        }

        public event EventHandler<Theme> ThemeChanged
        {
            add => _themeChangedEventManager.AddEventHandler(value);
            remove => _themeChangedEventManager.RemoveEventHandler(value);
        }

        protected override void OnStart()
        {   
            base.OnStart();

            SetTheme();
        }

        protected override void OnResume()
        {
            base.OnResume();

            SetTheme();
        }

        void SetTheme()
        {
            var operatingSystemTheme = DependencyService.Get<IEnvironment>().GetOperatingSystemTheme();

            BaseTheme preferedTheme = operatingSystemTheme switch
            {
                Theme.Light => new LightTheme(),
                Theme.Dark => new DarkTheme(),
                _ => throw new NotSupportedException()
            };

            if (Resources.GetType() != preferedTheme.GetType())
            {
                Resources = preferedTheme;

                EnableDebugRainbows(false);

                OnThemeChanged(operatingSystemTheme);
            }
        }

        [Conditional("DEBUG")]
        void EnableDebugRainbows(bool shouldUseDebugRainbows)
        {
            Resources.Add(new Style(typeof(ContentPage))
            {
                ApplyToDerivedTypes = true,
                Setters = {
                    new Setter
                    {
                        Property = Xamarin.Forms.DebugRainbows.DebugRainbow.ShowColorsProperty,
                        Value = shouldUseDebugRainbows
                    }
                }
            });
        }

        void OnThemeChanged(Theme newTheme) => _themeChangedEventManager.HandleEvent(this, newTheme, nameof(ThemeChanged));
    }
}
