using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using Resto.Front.Api;
using Resto.Front.Api.Data;
using Resto.Front.Api.Editors;
using Resto.Front.Api.Data.Organization.Sections;
using Resto.Front.Api.Attributes.JetBrains;
using Resto.Front.Api.Data.Assortment;
using Resto.Front.Api.TestPlugin.Restaurant;
using Resto.Front.Api.Data.Organization;
using System.Windows.Media;
using Resto.Front.Api.Data.Orders;
using System.Windows.Documents;

namespace Resto.Front.Api.TestPlugin
{
    public sealed partial class SectionsPage
    {
        public SectionsPage()
        {
            InitializeComponent();
            GetSections();
        }
        public void GetSections()
        {
            var sections = PluginContext.Operations.GetRestaurantSections()
                .Where(s => s.Tables != null && s.Tables.Count != 0);
            foreach (var section in sections)
            {
                var sectionButton = new Button
                {
                    Content = section.Name,
                    Margin = new Thickness(0, 5, 0, 5),
                    FontWeight = FontWeights.Bold
                };
                SectionsPanel.Children.Add(sectionButton);
                var tablesPanel = new WrapPanel
                {
                    Margin = new Thickness(10, 0, 0, 10)
                };
                foreach (var table in section.Tables.OrderBy(t => t.Number))
                {
                    var busyTables = PluginContext.Operations.GetOrders().Where(o => o.Status == OrderStatus.New).SelectMany(o => o.Tables).Distinct().ToList();
                    Brush brush;
                    if (busyTables.Contains(table))
                    {
                        brush = new SolidColorBrush(Color.FromRgb(255, 0, 0));
                    }
                    else { brush = new SolidColorBrush(Color.FromRgb(0, 255, 0));  }
                    var tableButton = new Button
                    {
                        Content = "Стол " + table.Number,
                        Background = brush,
                        Width = 80,
                        Height = 30,                        
                        Margin = new Thickness(5)
                        
                    };
                    tableButton.Click += (s, e) =>
                    {
                        MessageBox.Show($"Вы выбрали стол: {table.Name}");
                    };
                    tablesPanel.Children.Add(tableButton);
                }
                SectionsPanel.Children.Add(tablesPanel);
            }
        }
    }
}
