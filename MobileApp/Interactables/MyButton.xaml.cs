using System;
using System.Diagnostics;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MobileApp.Interactables
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MyButton : Interactable
    {
        public MyButton()
        {
            InitializeComponent();
        }
        
        public static readonly BindableProperty TappedCommandProperty = BindableProperty.Create(
            "TappedCommandProperty",
            typeof(ICommand),
            typeof(MyButton),
            propertyChanged: (b,o,n)=> {
                ((MyButton)b).rootStackLayout.GestureRecognizers.Clear();
               
                var tapGestureRecognizer = new TapGestureRecognizer();
                tapGestureRecognizer.Command = ((Command)n);
                ((MyButton)b).rootStackLayout.GestureRecognizers.Add(tapGestureRecognizer);

            });

        public ICommand TappedCommand
        {
            get => (ICommand)GetValue(TappedCommandProperty);
            set => SetValue(TappedCommandProperty, value);
        }
    }
}