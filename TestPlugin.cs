using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
using Resto.Front.Api.TestPlugin.Properties;
using Resto.Front.Api.Attributes;
using Resto.Front.Api.Attributes.JetBrains;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using Resto.Front.Api.TestPlugin.WpfHelpers;

namespace Resto.Front.Api.TestPlugin
{
    /// <summary>
    /// Тестовый плагин для демонстрации возможностей Api.
    /// Автоматически не публикуется, для использования скопировать Resto.Front.Api.TestPlugin.dll в Resto.Front.Main\bin\Debug\Plugins\Resto.Front.Api.TestPlugin\
    /// </summary>
    [UsedImplicitly]
    [PluginLicenseModuleId(21005918)]
    public sealed class TestPlugin : IFrontPlugin
    {
        private readonly Stack<IDisposable> subscriptions = new Stack<IDisposable>();
        public TestPlugin()
        {
            PluginContext.Log.Info("Initializing TestPlugin");

            if (Settings.Default.ExtendBillCheque)
                //subscriptions.Push(new NotificationHandlers.BillChequeExtender());
            subscriptions.Push(new ButtonsTester());
            OpenWPF(); //new
            //subscriptions.Push(new EditorTester());
            //ExternalOperationsTester.TestCalculator();
            //subscriptions.Push(new CookingPriority.CookingPriorityManager());
            //subscriptions.Push(new DiagnosticMessagesTester.MessagesTester());
            //subscriptions.Push(new Restaurant.RestaurantViewer());
            //subscriptions.Push(new Restaurant.MenuViewer());
            //subscriptions.Push(new DiagnosticMessagesTester.StopListChangeNotifier());
            //subscriptions.Push(new DiagnosticMessagesTester.FrontScreenReminder());
            //subscriptions.Push(new Restaurant.StreetViewer());
            //subscriptions.Push(new Restaurant.ReservesViewer());
            //subscriptions.Push(new Restaurant.ClientViewer());
            //subscriptions.Push(new DiagnosticMessagesTester.OrderItemReadyChangeNotifier());
            //subscriptions.Push(new LicensingTester());
            //subscriptions.Push(new LocalizationTester());
            //subscriptions.Push(new Kitchen.KitchenLoadMonitoringViewer());
            //subscriptions.Push(new Restaurant.BanquetAndReserveTester());
            //subscriptions.Push(new PreliminaryOrders.OrdersEditor());
            //subscriptions.Push(new Restaurant.SchemaViewer());
            //subscriptions.Push(new OrderStatusChangeNotifier());
            //subscriptions.Push(ChequeTaskProcessor.Register());
            //subscriptions.Push(new NotificationHandlers.BeforeServiceChequeHandler());
            //subscriptions.Push(new NotificationHandlers.BeforeDoChequeHandler());
            //subscriptions.Push(new NotificationHandlers.BeforeOrderBillHandler());
            //subscriptions.Push(new NotificationHandlers.NavigatingToPaymentScreenHandler());
            //subscriptions.Push(new NotificationHandlers.TableChangedHandler());
            //subscriptions.Push(new NotificationHandlers.TerminalsGroupChangedHandler());
            //subscriptions.Push(new NotificationHandlers.CashChequePrintingHandler());
            //subscriptions.Push(new NotificationHandlers.ConnectionToMainTerminalChangedHandler());
            //subscriptions.Push(new NotificationHandlers.PastOrderStornedHandler());
            //PersonalSessionsTester.Test();
            //subscriptions.Push(Screens.OrderEditScreen.AddProductByBarcode());
            //subscriptions.Push(Screens.OrderEditScreen.AddDiscountByCard());
            //subscriptions.Push(new BlockScreenPluginTester());
            //subscriptions.Push(new Modifiers.ModifiersViewer());
            // добавляйте сюда другие подписчики

            PluginContext.Log.Info("TestPlugin started");
        }

        public void Dispose()
        {
            while (subscriptions.Any())
            {
                var subscription = subscriptions.Pop();
                try
                {
                    subscription.Dispose();
                }
                catch (RemotingException)
                {
                    // nothing to do with the lost connection
                }
            }

            PluginContext.Log.Info("TestPlugin stopped");
        }
        public static void OpenWPF() //new
        {
            WpfWindowManager.Show<Order>(); //new
        }
    }

}