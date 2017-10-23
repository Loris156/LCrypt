using System.Collections;
using System.Windows;
using System.Windows.Controls;

namespace LCrypt.Utility
{
    public class ListBoxHelper
    {
        public static readonly DependencyProperty SelectedItemsProperty;

        static ListBoxHelper()
        {
            SelectedItemsProperty = DependencyProperty.RegisterAttached("SelectedItems", typeof(IList),
                typeof(ListBoxHelper),
                new FrameworkPropertyMetadata(null, OnSelectedItemsChanged));
        }

        public static void SetSelectedItems(DependencyObject d, IList value)
        {
            d.SetValue(SelectedItemsProperty, value);
        }

        public static IList GetSelectedItems(DependencyObject d)
        {
            return (IList) d.GetValue(SelectedItemsProperty);
        }

        private static void OnSelectedItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var listBox = (ListBox) d;
            ResetSelectedItems(listBox);
            listBox.SelectionChanged += (o, ev) => ResetSelectedItems(listBox);
        }

        private static void ResetSelectedItems(ListBox listBox)
        {
            var selectedItems = GetSelectedItems(listBox);
            selectedItems.Clear();
            foreach (var item in listBox.SelectedItems)
                selectedItems.Add(item);
        }
    }
}
