using System;
using System.Globalization;
using System.Net.NetworkInformation;
using System.Windows.Data;
using SimpleDnsCrypt.Models;

namespace SimpleDnsCrypt.Converter
{
    /// <summary>
    ///     LocalNetworkInterface to color converter.
    /// </summary>
    public class InUseToBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var localNetworkInterface = (LocalNetworkInterface) value;

            if (localNetworkInterface != null && localNetworkInterface.OperationalStatus != OperationalStatus.Up)
            {
                // red
                return "#CCC1170F";
            }

            if (localNetworkInterface != null && localNetworkInterface.UseDnsCrypt)
            {
                // green
                return "#FF8ab329";
            }

	        if (localNetworkInterface != null && localNetworkInterface.UseInsecureFallbackDns)
	        {
		        // yellow
		        return "#FFe0ca26";
	        }
			
			// gray
			return "#FFA0A0A0";
        }

        public object ConvertBack(object value, Type targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}