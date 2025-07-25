using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Resto.Front.Api.TestPlugin.WpfHelpers
{
    internal class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public void RaisePropertyChanged([CallerMemberName] string propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
