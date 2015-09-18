using System.Windows;
using System.Windows.Controls;

namespace SimpleDnsCrypt.Models
{
    /// <summary>
    ///     Custom ListView object.
    /// </summary>
    public class InterfaceListView : ListView
    {
        /// <summary>
        ///     Overwrite for GetContainerForItemOverride.
        /// </summary>
        /// <returns>A DependencyObject.</returns>
        protected override DependencyObject GetContainerForItemOverride()
        {
            return new InterfaceListViewItem();
        }
    }
}
