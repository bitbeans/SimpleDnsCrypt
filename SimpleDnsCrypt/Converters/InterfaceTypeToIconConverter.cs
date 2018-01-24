using System;
using System.Globalization;
using System.Net.NetworkInformation;
using System.Windows.Data;
using SimpleDnsCrypt.Models;

namespace SimpleDnsCrypt.Converters
{
	/// <summary>
	///     Interface to icon converter.
	/// </summary>
	public class InterfaceTypeToIconConverter : IValueConverter
	{
		public object EthernetIconOffline { get; set; }
		public object EthernetIcon { get; set; }
		public object WifiIcon { get; set; }
		public object WifiIconOffline { get; set; }


		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			try
			{
				var localNetworkInterface = (LocalNetworkInterface)value;
				bool isCable;
				switch (localNetworkInterface.Type)
				{
					case NetworkInterfaceType.Ethernet:
					case NetworkInterfaceType.Ethernet3Megabit:
					case NetworkInterfaceType.FastEthernetFx:
					case NetworkInterfaceType.FastEthernetT:
					case NetworkInterfaceType.GigabitEthernet:
						isCable = true;
						break;
					case NetworkInterfaceType.Wireless80211:
						isCable = false;
						break;
					default:
						isCable = true;
						break;
				}

				if (isCable)
				{
					return localNetworkInterface.OperationalStatus != OperationalStatus.Up ? EthernetIconOffline : EthernetIcon;
				}
				return localNetworkInterface.OperationalStatus != OperationalStatus.Up ? WifiIconOffline : WifiIcon;
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
