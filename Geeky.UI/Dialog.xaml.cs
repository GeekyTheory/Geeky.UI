using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Geeky.UI.Annotations;

// The Content Dialog item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Geeky.UI
{
    public sealed partial class Dialog : ContentDialog, INotifyPropertyChanged
    {
        public Dialog()
        {
            this.InitializeComponent();

            Items = new ObservableCollection<string>();

            for (int i = 0; i < 25; i++)
            {
                Items.Add(string.Format("Hola que tal, esto es un texto largo para ver hasta donde puede llegar {0}", i));
            }
        }

        private ObservableCollection<string> items;
        public ObservableCollection<string> Items
        {
            get { return items; }
            set
            {
                if (items == value) return;
                items = value;
                OnPropertyChanged();
            }
        }

        private string item;

        public string Item
        {
            get { return item; }
            set
            {
                if (item == value) return;
                item = value;
                OnPropertyChanged();
            }
        }


        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
