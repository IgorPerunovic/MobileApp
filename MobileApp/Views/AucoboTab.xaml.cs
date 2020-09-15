using MobileApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MobileApp.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AucoboTab : ContentPage
    {
        public TabModel tabModel;
        public AucoboTab()
        {
            InitializeComponent();
        }
        
        public new string Id => tabModel.Id;
        
        public AucoboTab(TabModel model) 
        {
            this.tabModel = model;
            this.Title = tabModel.Title;
            InitializeComponent();
        }
    }
}