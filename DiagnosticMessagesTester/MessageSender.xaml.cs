using System;
using System.Windows;

namespace Resto.Front.Api.TestPlugin.DiagnosticMessagesTester
{
    /// <summary>
    /// Interaction logic for MessageSender.xaml
    /// </summary>
    public sealed partial class MessageSender
    {
        public MessageSender()
        {
            InitializeComponent();

            Severity.Items.Add("Notification");
            Severity.Items.Add("Warning");
            Severity.Items.Add("Error");

            Severity.SelectedIndex = 0;
        }

        private void Send_Click(object sender, RoutedEventArgs e)
        {
            var expireTime = TimeSpan.FromSeconds(int.Parse(expire.Text));

            switch ((string)Severity.SelectedItem)
            {
                case "Notification":
                    if (expireTime == TimeSpan.Zero)
                        PluginContext.Operations.AddNotificationMessage(text.Text, "TestPlugin");
                    else
                        PluginContext.Operations.AddNotificationMessage(text.Text, "TestPlugin", expireTime);

                    break;

                case "Warning":
                    if (expireTime == TimeSpan.Zero)
                        PluginContext.Operations.AddWarningMessage(text.Text, "TestPlugin");
                    else
                        PluginContext.Operations.AddWarningMessage(text.Text, "TestPlugin", expireTime);

                    break;

                case "Error":
                    if (expireTime == TimeSpan.Zero)
                        PluginContext.Operations.AddErrorMessage(text.Text, "TestPlugin");
                    else
                        PluginContext.Operations.AddErrorMessage(text.Text, "TestPlugin", expireTime);

                    break;
            }
        }
    }
}
