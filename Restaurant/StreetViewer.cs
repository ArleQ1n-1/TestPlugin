﻿using System;
using System.Reactive.Disposables;
using System.Threading;
using System.Windows;

namespace Resto.Front.Api.TestPlugin.Restaurant
{
    internal sealed class StreetViewer : IDisposable
    {
        private readonly object syncObject = new object();
        private readonly CompositeDisposable resources = new CompositeDisposable();
        private bool disposed;

        public StreetViewer()
        {
            var windowThread = new Thread(EntryPoint);
            windowThread.SetApartmentState(ApartmentState.STA);
            windowThread.Start();
            PluginContext.Log.Info("StreetViewer started");
        }

        private void EntryPoint()
        {
            Window window;
            lock (syncObject)
            {
                if (disposed)
                    return;

                window = new Window
                             {
                                 SizeToContent = SizeToContent.WidthAndHeight,
                                 ResizeMode = ResizeMode.NoResize,
                                 Content = new StreetView(),
                                 Title = GetType().Name,
                                 Topmost = true
                             };
                resources.Add(Disposable.Create(() =>
                {
                    window.Dispatcher.InvokeShutdown();
                    window.Dispatcher.Thread.Join();
                }));
            }

            PluginContext.Log.Info("Show StreetView dialog...");
            window.ShowDialog();
            PluginContext.Log.Info("Close StreetView dialog...");
        }

        public void Dispose()
        {
            if (disposed)
                return;
            lock (syncObject)
            {
                resources.Dispose();
                PluginContext.Log.Info("StreetViewer stopped");
                disposed = true;
            }
        }
    }
}
