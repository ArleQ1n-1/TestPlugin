using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Windows;
using System.Windows.Controls;

namespace Resto.Front.Api.TestPlugin.WpfHelpers
{
    internal class WpfWindowManager //new
    {
        private static Thread _uiThread;
        private static Window _windowInstance;

        public static void Show<T>() where T : UserControl, new()
        {
            if (_uiThread != null && _uiThread.IsAlive)
            {
                var dispatcher = Dispatcher.FromThread(_uiThread);
                if (dispatcher != null)
                {
                    dispatcher.BeginInvoke(new Action(() =>
                    {
                        if (_windowInstance != null && _windowInstance.IsLoaded)
                        {
                            if (_windowInstance.WindowState == WindowState.Minimized)
                                _windowInstance.WindowState = WindowState.Normal;

                            _windowInstance.Activate();
                            _windowInstance.Topmost = true;
                            _windowInstance.Topmost = false;
                            return;
                        }

                        CreateWindow<T>();
                    }));
                    return;
                }
            }
            _uiThread = new Thread(() =>
            {
                var app = new Application
                {
                    ShutdownMode = ShutdownMode.OnExplicitShutdown
                };

                CreateWindow<T>();

                Dispatcher.Run();
            });

            _uiThread.SetApartmentState(ApartmentState.STA);
            _uiThread.IsBackground = true;
            _uiThread.Start();
        }

        private static void CreateWindow<T>() where T : UserControl, new()
        {
            _windowInstance = new Window
            {
                Title = "Создать заказ",
                Content = new T(),
                Width = 800,
                Height = 600
            };
            _windowInstance.Closed += (s, e) => _windowInstance = null;
            _windowInstance.Show();
        }
    }
}
