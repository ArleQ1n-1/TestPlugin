using Resto.Front.Api.TestPlugin.WpfHelpers;

namespace Resto.Front.Api.TestPlugin.Modifiers
{
    internal abstract class OrderItemViewModel : ViewModelBase
    {
        public virtual string DisplayText { get; set; }
        public virtual decimal Price { get; set; }
        public virtual decimal Amount { get; set; }
    }
}
