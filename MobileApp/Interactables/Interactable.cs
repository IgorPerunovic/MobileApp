using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using Xamarin.Forms;

namespace MobileApp.Interactables
{
    public abstract class Interactable : ContentView
    {
        private readonly int id;

        // todo: add parameters

        // TODO: add next steps

        public Interactable()
        {
            this.id = -1;
        }

        public Interactable(int id)
        {
            this.id = id;
        }

    }
}
