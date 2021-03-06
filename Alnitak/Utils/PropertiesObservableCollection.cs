﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alnitak.Utils
{
    public class PropertiesObservableCollection<T> : ObservableCollection<T>
        where T : INotifyPropertyChanged
    {

        public PropertiesObservableCollection() : base()
        {
            foreach (INotifyPropertyChanged element in this)
            {
                element.PropertyChanged += Element_PropertyChanged;
            }
            this.CollectionChanged += PropertiesObservableCollection_CollectionChanged;
        }

        public PropertiesObservableCollection(List<T> list) : base(list)
        {
            foreach (INotifyPropertyChanged element in this)
            {
                element.PropertyChanged += Element_PropertyChanged;
            }
            this.CollectionChanged += PropertiesObservableCollection_CollectionChanged;
        }

        public PropertiesObservableCollection(IEnumerable<T> collection) : base(collection)
        {
            foreach (INotifyPropertyChanged element in this)
            {
                element.PropertyChanged += Element_PropertyChanged;
            }
            this.CollectionChanged += PropertiesObservableCollection_CollectionChanged;
        }

        private void PropertiesObservableCollection_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (T element in e.OldItems)
                {
                    element.PropertyChanged -= Element_PropertyChanged;
                }
            }
            if (e.NewItems != null)
            {
                foreach (T element in e.NewItems)
                {
                    element.PropertyChanged += Element_PropertyChanged;
                }
            }
        }

        private void Element_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            NotifyCollectionChangedEventArgs args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, sender, sender, IndexOf((T)sender));
            OnCollectionChanged(args);
        }
    }
}
