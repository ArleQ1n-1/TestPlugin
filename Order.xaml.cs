using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Resto.Front.Api;
using Resto.Front.Api.Data;
using Resto.Front.Api.Editors;
using Resto.Front.Api.Data.Organization.Sections;
using System.Windows.Controls.Primitives;
using System.Security.AccessControl;
using System.Windows.Controls;
using Resto.Front.Api.Data.Assortment;
using Resto.Front.Api.Attributes.JetBrains;
using Resto.Front.Api.TestPlugin.Restaurant;
using Resto.Front.Api.Data.PreliminaryOrders;
using Resto.Front.Api.Exceptions;
using Resto.Front.Api.TestPlugin.PreliminaryOrders;
using static Resto.Front.Api.TestPlugin.Order;
using System.Windows.Input;
using System.Windows.Media;
using Resto.Front.Api.Editors.Stubs;
using Resto.Front.Api.Data.Orders;
using Resto.Front.Api.Data.Payments;
using System.Threading;
using Resto.Front.Api.Extensions;

namespace Resto.Front.Api.TestPlugin
{
    public sealed partial class Order : UserControl
    {
        Random random = new Random();
        int gCount = 1;
        int tCount = 1;
        int oCount = 1;
        bool gC = false;
        bool tC = false;
        bool oC = false;

        public class Product
        {
            public string Article { get; set; }
            public string Name { get; set; }
        }

        private List<Product> Products = new List<Product>();
        private List<IProduct> ProductForOrder = new List<IProduct>();

        public Order()
        {
            InitializeComponent();
            DishSearchBox.Loaded += (s, e) =>
            {
                var textBox = (TextBox)DishSearchBox.Template.FindName("PART_EditableTextBox", DishSearchBox);
                if (textBox != null)
                {
                    textBox.TextChanged += DishSearchBox_TextChanged;
                }
            };
            DishSearchBox.ItemsSource = Products.Select(p => $"{p.Name} | {p.Article}").ToList();
            GetAll();
        }

        private void FilterProductsByOrderList()
        {
            var selectedArticles = OrderItemsListBox.Items.Cast<string>().Select(item =>
            {
                var parts = item.Split('|');
                return parts.Length == 2 ? parts[1].Trim() : null;
            }).Where(article => article != null).ToHashSet();

            ProductForOrder = ProductForOrder.Where(product => selectedArticles.Contains(product.Number)).ToList();
        }

        private void GetAll()
        {
            var paymentsType = PluginContext.Operations.GetPaymentTypes();
            foreach (var paymentType in paymentsType)
            {
                PaymentTypeBox.Items.Add(paymentType.Name);
            }

            var menu = PluginContext.Operations.GetHierarchicalMenu();
            var rootProducts = menu.Products;
            var nestedProducts = GetAllProductsRecursively(menu.ProductGroups);

            Products = GetSimpleProductList();
            ProductForOrder = nestedProducts;

            var allProducts = rootProducts.Concat(nestedProducts).Distinct().ToList();
            DishSearchBox.ItemsSource = Products.Select(p => $"{p.Name} | {p.Article}").ToList();
        }

        private List<Product> GetSimpleProductList()
        {
            var menu = PluginContext.Operations.GetHierarchicalMenu();
            var products = GetAllProductsRecursively(menu.ProductGroups);

            return products.OrderBy(p => p.Name).Select(p => new Product
            {
                Article = p.Number,
                Name = p.Name
            }).ToList();
        }

        public static List<IProduct> GetAllProductsRecursively(IEnumerable<IProductGroup> rootGroups)
        {
            var result = new List<IProduct>();
            var stack = new Stack<IProductGroup>(rootGroups);

            while (stack.Count > 0)
            {
                var group = stack.Pop();
                var products = PluginContext.Operations.GetChildProductsByProductGroup(group)
                    .Where(product => product.Price != 0 && product.Type == ProductType.Dish && product.Scale == null);
                result.AddRange(products);

                var childGroups = PluginContext.Operations.GetChildGroupsByProductGroup(group);
                foreach (var child in childGroups)
                    stack.Push(child);
            }

            return result;
        }

        private void DishSearchBox_Loaded(object sender, RoutedEventArgs e)
        {
            var combo = sender as ComboBox;
            var textBox = (TextBox)combo.Template.FindName("PART_EditableTextBox", combo);
            if (textBox != null)
            {
                textBox.TextChanged += DishSearchBox_TextChanged;
            }
        }

        private void DishSearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var input = DishSearchBox.Text.ToLower();

            List<string> results = string.IsNullOrWhiteSpace(input)
                ? Products.Select(p => $"{p.Name} | {p.Article}").ToList()
                : Products.Where(p => p.Name.ToLower().Contains(input) || p.Article.Contains(input))
                    .Select(p => $"{p.Name} | {p.Article}").ToList();

            DishSearchBox.ItemsSource = results;
            DishSearchBox.IsDropDownOpen = results.Any();
        }

        private void DishSearchBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DishSearchBox.SelectedItem != null)
            {
                var selectedItem = DishSearchBox.SelectedItem.ToString();
                if (!OrderItemsListBox.Items.Contains(selectedItem))
                {
                    OrderItemsListBox.Items.Add(selectedItem);
                }
                DishSearchBox.Text = "";
                DishSearchBox.ItemsSource = null;
                DishSearchBox.IsDropDownOpen = false;
                DishSearchBox.SelectedItem = null;
            }
        }

        private void CreateOrder()
        {
            try
            {
                FilterProductsByOrderList();
                var tables = GetAvailableTables();
                var editSession = PluginContext.Operations.CreateEditSession();
                var selectedTables = GetSelectedTables(tables);
                var newOrder = editSession.CreateOrder(selectedTables);
                editSession.ChangeOrderOriginName("Test Plugin", newOrder);
                IOrderGuestItemStub lastGuest = AddGuestsToOrder(editSession, newOrder);
                AddProductsToOrder(editSession, newOrder, lastGuest);
                var paymentType = GetSelectedPaymentType();
                if (paymentType == null) return;
                PluginContext.Operations.SubmitChanges(editSession);
                PayOrder(paymentType);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Общая ошибка создания заказа. Проверьте поля\r\n" + ex.Message);
            }
        }

        private List<ITable> GetAvailableTables()
        {
            return PluginContext.Operations.GetRestaurantSections()
                .Where(s => s.Tables?.Any() == true)
                .SelectMany(s => s.Tables)
                .OrderBy(t => t.Number).ToList();
        }

        private List<ITable> GetSelectedTables(List<ITable> tables)
        {
            int tableNumber = tC ? tCount : Convert.ToInt32(TableNumberInput.Text);
            return tables.Where(t => t.Number == tableNumber).ToList();
        }

        private IOrderGuestItemStub AddGuestsToOrder(IEditSession editSession, INewOrderStub order)
        {
            int guestCount = gC ? gCount : Convert.ToInt32(GuestCountInput.Text);
            IOrderGuestItemStub lastGuest = editSession.AddOrderGuest("Гость номер: 1", order);

            for (int i = 2; i <= guestCount; i++)
            {
                lastGuest = editSession.AddOrderGuest($"Гость номер: {i}", order);
            }

            return lastGuest;
        }

        private void AddProductsToOrder(IEditSession editSession, INewOrderStub order, IOrderGuestItemStub lastGuest)
        {
            foreach (var product in ProductForOrder)
            {
                editSession.AddOrderProductItem(random.Next(1, 10), product, order, lastGuest, null);
            }
        }

        private IPaymentType GetSelectedPaymentType()
        {
            if (PaymentTypeBox.SelectedItem == null)
            {
                MessageBox.Show("Не выбран тип оплаты");
                return null;
            }

            string selectedText = PaymentTypeBox.SelectedItem.ToString();
            try
            {
                PaymentTypeKind paymentTypeKind;
                switch (selectedText)
                {
                    case "Наличные":
                        paymentTypeKind = PaymentTypeKind.Cash;
                        return PluginContext.Operations.GetPaymentTypes().FirstOrDefault(x => x.Kind == paymentTypeKind);

                    case "Банковские карты":
                        paymentTypeKind = PaymentTypeKind.Card;
                        return PluginContext.Operations.GetPaymentTypesToPayOutOnUser().FirstOrDefault(x => x.Kind == paymentTypeKind);

                    default:
                        MessageBox.Show("Неизвестный способ оплаты: " + selectedText);
                        return null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при определении типа оплаты:\r\n" + ex.Message);
                return null;
            }
        }

        private void PayOrder(IPaymentType paymentType)
        {
            try
            {
                var credentials = PluginContext.Operations.AuthenticateByPin("1111");
                var order = PluginContext.Operations.GetOrders().Last(o => o.Status == OrderStatus.New);
                string selectedText = PaymentTypeBox.SelectedItem.ToString();

                if (selectedText == "Наличные")
                {
                    PluginContext.Operations.AddPaymentItem(order.FullSum, null, paymentType, order, PluginContext.Operations.GetDefaultCredentials());
                    PluginContext.Operations.PayOrder(order, true, credentials);
                }
                else if (selectedText == "Банковские карты")
                {
                    PluginContext.Operations.PayOrderAndPayOutOnUser(order, true, paymentType, order.ResultSum, credentials);
                }
            }
            catch (PaymentActionFailedException ex)
            {
                PluginContext.Log.Error($"Во время оплаты произошла ошибка: {ex.Details}");
            }
            catch (Exception ex)
            {
                PluginContext.Log.Error(ex.ToString());
            }
        }

        private void BtnRandomTable_Click(object sender, RoutedEventArgs e)
        {
            tCount = random.Next(1, 30);
            TableNumberInput.Text = tCount.ToString();
        }

        private void BtnRandomGuests_Click(object sender, RoutedEventArgs e)
        {
            gCount = random.Next(1, 999);
            GuestCountInput.Text = gCount.ToString();
        }

        private void BtnRandomOrderQty_Click(object sender, RoutedEventArgs e)
        {
            oCount = random.Next(1, 10);
            OrderQuantityBox.Text = oCount.ToString();
        }

        private void BtnCreateOrders_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (oC)
                {
                    for (int i = 0; i < oCount; i++)
                    {
                        CreateOrder();
                        Thread.Sleep(1000);
                    }
                }
                else
                {
                    for (int i = 0; i < Convert.ToInt32(OrderQuantityBox.Text); i++)
                    {
                        CreateOrder();
                        Thread.Sleep(1000);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Общая ошибка создания заказа. Проверьте поля\r" + ex.Message);
            }
        }

        private void NumericOnly(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !e.Text.All(char.IsDigit);
        }

        private void LimitTo999(object sender, TextChangedEventArgs e)
        {
            var tb = sender as TextBox;
            if (int.TryParse(tb.Text, out int value))
            {
                if (value > 999)
                {
                    tb.Text = "999";
                    tb.CaretIndex = tb.Text.Length;
                }
            }
        }

        private void LimitTo30(object sender, TextChangedEventArgs e)
        {
            var tb = sender as TextBox;
            if (int.TryParse(tb.Text, out int value))
            {
                if (value > 30)
                {
                    tb.Text = "30";
                    tb.CaretIndex = tb.Text.Length;
                }
            }
        }

        private void LimitTo10(object sender, TextChangedEventArgs e)
        {
            var tb = sender as TextBox;
            if (int.TryParse(tb.Text, out int value))
            {
                if (value > 10)
                {
                    tb.Text = "10";
                    tb.CaretIndex = tb.Text.Length;
                }
            }
        }
    }
}