using System;
using System.Globalization;
using System.Net.NetworkInformation;
using System.Windows.Data;
using SimpleDnsCrypt.Models;

namespace SimpleDnsCrypt.Converter
{
    /// <summary>
    ///     Interface to icon converter.
    /// </summary>
    public class InterfaceTypeToIconConverter : IValueConverter
    {
        public object EthernetIcon { get; set; }
        public object WifiIcon { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                var localNetworkInterface = (LocalNetworkInterface) value;

                switch (localNetworkInterface.Type)
                {
                    case NetworkInterfaceType.Ethernet:
                    case NetworkInterfaceType.Ethernet3Megabit:
                    case NetworkInterfaceType.FastEthernetFx:
                    case NetworkInterfaceType.FastEthernetT:
                    case NetworkInterfaceType.GigabitEthernet:
                        return EthernetIcon;
                    case NetworkInterfaceType.Wireless80211:
                        return WifiIcon;
                    default:
                        return EthernetIcon;
                }
            }
            catch (Exception)
            {
                return EthernetIcon;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}