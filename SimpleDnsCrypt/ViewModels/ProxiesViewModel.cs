using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;

namespace SimpleDnsCrypt.ViewModels
{
	[Export(typeof(ListenAddressesViewModel))]
	public class ProxiesViewModel : Screen
    {
	    private readonly IWindowManager _windowManager;
	    private readonly IEventAggregator _events;
	    private string _windowTitle;
	    private string _httpProxyInput;
	    private string _socksProxyInput;

		public ProxiesViewModel()
	    {
	    }

	    [ImportingConstructor]
	    public ProxiesViewModel(IWindowManager windowManager, IEventAggregator events)
	    {
		    _windowManager = windowManager;
		    _events = events;
		    _events.Subscribe(this);
	    }

	    /// <summary>
	    ///     The title of the window.
	    /// </summary>
	    public string WindowTitle
	    {
		    get => _windowTitle;
		    set
		    {
			    _windowTitle = value;
			    NotifyOfPropertyChange(() => WindowTitle);
		    }
	    }

	    public string HttpProxyInput
		{
		    get => _httpProxyInput;
		    set
		    {
			    _httpProxyInput = value;
			    NotifyOfPropertyChange(() => HttpProxyInput);
		    }
	    }

	    public string SocksProxyInput
		{
		    get => _socksProxyInput;
		    set
		    {
			    _socksProxyInput = value;
			    NotifyOfPropertyChange(() => SocksProxyInput);
		    }
	    }
	}
}
