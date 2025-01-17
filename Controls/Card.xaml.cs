using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Resolved.Controls
{
    public sealed partial class Card : UserControl
    {
        public Card()
        {
            this.InitializeComponent();
        }

        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
            "Title", typeof(string), typeof(Card), new PropertyMetadata("Title")
        );
        public string Title {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty , value);
        }

        public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register(
            "Description" , typeof(string) , typeof(Card) , new PropertyMetadata("Description")
        );
        public string Description {
            get => (string)GetValue(DescriptionProperty);
            set => SetValue(DescriptionProperty , value);
        }

        public static new readonly DependencyProperty ContentProperty = DependencyProperty.Register(
            "Content" , typeof(object) , typeof(Card) , new PropertyMetadata(null)
        );
        public new object Content {
            get => GetValue(ContentProperty);
            set => SetValue(ContentProperty , value);
        }
    }
}
