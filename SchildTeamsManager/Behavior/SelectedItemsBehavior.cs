using Microsoft.Xaml.Behaviors;
using System.Collections;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;

namespace SchildTeamsManager.Behavior
{
    public class SelectedItemsBehavior : Behavior<DataGrid>
    {

        private bool suppressBoundCollectionChangedEvent = false;

        public static DependencyProperty SelectedItemsProperty = DependencyProperty.Register("SelectedItems", typeof(IEnumerable), typeof(SelectedItemsBehavior), new PropertyMetadata(OnSelectedItemsChanged));

        private static void OnSelectedItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as SelectedItemsBehavior).OnSelectedItemsChanged(e.OldValue as IEnumerable, e.NewValue as IEnumerable);
        }

        private void OnSelectedItemsChanged(IEnumerable oldValue, IEnumerable newValue)
        {
            var oldCollection = oldValue as INotifyCollectionChanged;

            if (oldCollection != null)
            {
                oldCollection.CollectionChanged -= OnSelectedItemsCollectionChanged;
            }

            var newCollection = newValue as INotifyCollectionChanged;

            if (newCollection != null)
            {
                newCollection.CollectionChanged += OnSelectedItemsCollectionChanged;
            }
        }

        private void OnSelectedItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (AssociatedObject == null || suppressBoundCollectionChangedEvent)
            {
                return;
            }

            if (e.NewItems != null)
            {
                foreach (var addedItem in e.NewItems)
                {
                    if (!AssociatedObject.SelectedItems.Contains(addedItem))
                    {
                        AssociatedObject.SelectedItems.Add(addedItem);
                    }
                }
            }

            if (e.OldItems != null)
            {
                foreach (var removedItem in e.OldItems)
                {
                    AssociatedObject.SelectedItems.Remove(removedItem);
                }
            }

            var collection = sender as ICollection;

            if (e.Action == NotifyCollectionChangedAction.Reset && collection != null)
            {
                suppressBoundCollectionChangedEvent = true;

                foreach (var item in collection)
                {
                    AssociatedObject.SelectedItems.Remove(item);
                }

                suppressBoundCollectionChangedEvent = false;
            }
        }

        public IEnumerable SelectedItems
        {
            get { return (IEnumerable)GetValue(SelectedItemsProperty); }
            set { SetValue(SelectedItemsProperty, value); }
        }

        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.SelectionChanged += OnListViewSelectionChanged;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            AssociatedObject.SelectionChanged -= OnListViewSelectionChanged;
        }

        private void OnListViewSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (suppressBoundCollectionChangedEvent)
            {
                return;
            }

            var boundList = SelectedItems as IList;

            if (boundList == null)
            {
                // SelectedItems is not a list and thus cannot be motifed
                SelectedItems = AssociatedObject.SelectedItems;
                return;
            }

            suppressBoundCollectionChangedEvent = true;

            // Mofify the underlying collection
            foreach (var addedItem in e.AddedItems)
            {
                if (!boundList.Contains(addedItem))
                {
                    boundList.Add(addedItem);
                }
            }

            foreach (var removedItem in e.RemovedItems)
            {
                boundList.Remove(removedItem);
            }

            suppressBoundCollectionChangedEvent = false;
        }
    }
}
