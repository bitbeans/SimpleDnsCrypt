using Caliburn.Micro;
using DnsCrypt.Models;
using GongSolutions.Wpf.DragDrop;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows;

namespace SimpleDnsCrypt.ViewModels
{
	[Export(typeof(RouteViewModel))]
	public class RouteViewModel : Screen, IDropTarget
	{
		private string _resolver;

		[ImportingConstructor]
		public RouteViewModel()
		{
		}

		public string Resolver
		{
			get => _resolver;
			set
			{
				_resolver = value;
				NotifyOfPropertyChange(() => Resolver);
			}
		}

		private ObservableCollection<StampFileEntry> _route;

		public ObservableCollection<StampFileEntry> Route
		{
			get => _route;
			set
			{
				_route = value;
				NotifyOfPropertyChange(() => Route);
			}
		}

		public BindableCollection<StampFileEntry> Relays { get; internal set; }

		public void Remove(StampFileEntry stampFileEntry)
		{
			if (stampFileEntry != null)
			{
				Route.Remove(stampFileEntry);
			}
		}

		void IDropTarget.DragOver(IDropInfo dropInfo)
		{
			if (dropInfo.Data is StampFileEntry && dropInfo.TargetItem is StampFileEntry || dropInfo.TargetItem is null)
			{
				dropInfo.DropTargetAdorner = DropTargetAdorners.Highlight;
				dropInfo.Effects = DragDropEffects.Move;
			}
		}

		void IDropTarget.Drop(IDropInfo dropInfo)
		{
			var stampFileEntry = (StampFileEntry)dropInfo.Data;
			if (Route.Where(s => s.Name.Equals(stampFileEntry.Name)).FirstOrDefault() != null) return;
			Route.Add(stampFileEntry);
		}
	}
}
