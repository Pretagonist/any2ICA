using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Threading;
using System.Xml.Serialization;

namespace anyICA
{
    public static class Extensions
    {
        public static string ToXml(this object obj)
        {
            XmlSerializer s = new XmlSerializer(obj.GetType());
            using (StringWriter writer = new StringWriter())
            {
                s.Serialize(writer, obj);
                return writer.ToString();
            }
        }

        public static T FromXml<T>(this string data)
        {
            XmlSerializer s = new XmlSerializer(typeof(T));
            using (StringReader reader = new StringReader(data))
            {
                object obj = s.Deserialize(reader);
                return (T)obj;
            }
        }
    }

    //public static class DataGridExtensions
    //{
    //    public static void SelectCellByIndex(DataGrid dataGrid, int rowIndex, int columnIndex)
    //    {
    //        if (!dataGrid.SelectionUnit.Equals(DataGridSelectionUnit.Cell))
    //            throw new ArgumentException("The SelectionUnit of the DataGrid must be set to Cell.");

    //        if (rowIndex < 0 || rowIndex > (dataGrid.Items.Count - 1))
    //            throw new ArgumentException(string.Format("{0} is an invalid row index.", rowIndex));

    //        if (columnIndex < 0 || columnIndex > (dataGrid.Columns.Count - 1))
    //            throw new ArgumentException(string.Format("{0} is an invalid column index.", columnIndex));

    //        dataGrid.SelectedCells.Clear();

    //        object item = dataGrid.Items[rowIndex]; //=Product X
    //        DataGridRow row = dataGrid.ItemContainerGenerator.ContainerFromIndex(rowIndex) as DataGridRow;
    //        if (row == null)
    //        {
    //            dataGrid.ScrollIntoView(item);
    //            row = dataGrid.ItemContainerGenerator.ContainerFromIndex(rowIndex) as DataGridRow;
    //        }
    //        if (row != null)
    //        {
    //            DataGridCell cell = GetCell(dataGrid, row, columnIndex);
    //            if (cell != null)
    //            {
    //                DataGridCellInfo dataGridCellInfo = new DataGridCellInfo(cell);
    //                dataGrid.SelectedCells.Add(dataGridCellInfo);
    //                cell.Focus();
    //            }
    //        }
    //    }
    //    public static DataGridCell GetCell(this DataGrid grid, DataGridRow row, int columnIndex = 0)
    //    {
    //        if (row == null) return null;
    //        row.
    //        var presenter = row.FindVisualChild<DataGridCellsPresenter>();
    //        if (presenter == null) return null;

    //        var cell = (DataGridCell)presenter.ItemContainerGenerator.ContainerFromIndex(columnIndex);
    //        if (cell != null) return cell;

    //        // now try to bring into view and retreive the cell
    //        grid.ScrollIntoView(row, grid.Columns[columnIndex]);
    //        cell = (DataGridCell)presenter.ItemContainerGenerator.ContainerFromIndex(columnIndex);

    //        return cell;
    //    }
    //    public static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
    //    {
    //        if (depObj != null)
    //        {
    //            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
    //            {
    //                DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
    //                if (child != null && child is T)
    //                {
    //                    yield return (T)child;
    //                }

    //                foreach (T childOfChild in FindVisualChildren<T>(child))
    //                {
    //                    yield return childOfChild;
    //                }
    //            }
    //        }
    //    }

    //    public static childItem FindVisualChild<childItem>(DependencyObject obj)
    //        where childItem : DependencyObject
    //    {
    //        foreach (childItem child in FindVisualChildren<childItem>(obj))
    //        {
    //            return child;
    //        }

    //        return null;
    //    }
    //}

    public static class Tools
    {
        public static string RemoveSpecialCharacters(string str)
        {
            //return Regex.Replace(str, "[^a-zA-Z0-9_.]+", "", RegexOptions.Compiled);
            return System.Net.WebUtility.HtmlDecode(str.Replace("\r", "").Replace("  ", ""));
        }

        

    }

    //extension found at https://stackoverflow.com/questions/2946954/make-listview-scrollintoview-scroll-the-item-into-the-center-of-the-listview-c
    public static class ItemsControlExtensions
    {
        public static void ScrollToCenterOfView(this ItemsControl itemsControl, object item)
        {
            // Scroll immediately if possible
            if (!itemsControl.TryScrollToCenterOfView(item))
            {
                // Otherwise wait until everything is loaded, then scroll
                if (itemsControl is ListBox) ((ListBox)itemsControl).ScrollIntoView(item);
                itemsControl.Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(() =>
                {
                    itemsControl.TryScrollToCenterOfView(item);
                }));
            }
        }

        private static bool TryScrollToCenterOfView(this ItemsControl itemsControl, object item)
        {
            // Find the container
            var container = itemsControl.ItemContainerGenerator.ContainerFromItem(item) as UIElement;
            if (container == null) return false;

            // Find the ScrollContentPresenter
            ScrollContentPresenter presenter = null;
            for (Visual vis = container; vis != null && vis != itemsControl; vis = VisualTreeHelper.GetParent(vis) as Visual)
                if ((presenter = vis as ScrollContentPresenter) != null)
                    break;
            if (presenter == null) return false;

            // Find the IScrollInfo
            var scrollInfo =
                !presenter.CanContentScroll ? presenter :
                presenter.Content as IScrollInfo ??
                FirstVisualChild(presenter.Content as ItemsPresenter) as IScrollInfo ??
                presenter;

            // Compute the center point of the container relative to the scrollInfo
            Size size = container.RenderSize;
            Point center = container.TransformToAncestor((Visual)scrollInfo).Transform(new Point(size.Width / 2, size.Height / 2));
            center.Y += scrollInfo.VerticalOffset;
            center.X += scrollInfo.HorizontalOffset;

            // Adjust for logical scrolling
            if (scrollInfo is StackPanel || scrollInfo is VirtualizingStackPanel)
            {
                double logicalCenter = itemsControl.ItemContainerGenerator.IndexFromContainer(container) + 0.5;
                Orientation orientation = scrollInfo is StackPanel ? ((StackPanel)scrollInfo).Orientation : ((VirtualizingStackPanel)scrollInfo).Orientation;
                if (orientation == Orientation.Horizontal)
                    center.X = logicalCenter;
                else
                    center.Y = logicalCenter;
            }

            // Scroll the center of the container to the center of the viewport
            if (scrollInfo.CanVerticallyScroll) scrollInfo.SetVerticalOffset(CenteringOffset(center.Y, scrollInfo.ViewportHeight, scrollInfo.ExtentHeight));
            if (scrollInfo.CanHorizontallyScroll) scrollInfo.SetHorizontalOffset(CenteringOffset(center.X, scrollInfo.ViewportWidth, scrollInfo.ExtentWidth));
            return true;
        }

        private static double CenteringOffset(double center, double viewport, double extent)
        {
            return Math.Min(extent - viewport, Math.Max(0, center - viewport / 2));
        }
        private static DependencyObject FirstVisualChild(Visual visual)
        {
            if (visual == null) return null;
            if (VisualTreeHelper.GetChildrenCount(visual) == 0) return null;
            return VisualTreeHelper.GetChild(visual, 0);
        }
    }

}
