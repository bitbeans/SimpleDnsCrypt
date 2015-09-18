using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Linq;
using System.Windows;
using Caliburn.Micro;
using SimpleDnsCrypt.ViewModels;

namespace SimpleDnsCrypt
{
    public class SimpleDnsCryptBootstraper : BootstrapperBase
    {
        private CompositionContainer _container;
        private IEventAggregator _events;

        public SimpleDnsCryptBootstraper()
        {
            AppDomain.CurrentDomain.AssemblyLoad += CurrentDomain_AssemblyLoad;
            Initialize();
        }

        protected override void OnStartup(object sender, StartupEventArgs e)
        {
            base.OnStartup(sender, e);
            DisplayRootViewFor<MainViewModel>();
        }

        private void CurrentDomain_AssemblyLoad(object sender, AssemblyLoadEventArgs args)
        {
            if (args.LoadedAssembly.FullName.StartsWith("ManagedInjector"))
                Application.Shutdown(0);
        }

        protected override void Configure()
        {
            try
            {
                _events = new EventAggregator();
                _container =
                    new CompositionContainer(
                        new AggregateCatalog(
                            AssemblySource.Instance.Select(x => new AssemblyCatalog(x)).OfType<ComposablePartCatalog>()));
                var batch = new CompositionBatch();
                batch.AddExportedValue<IWindowManager>(new AppWindowManager());
                batch.AddExportedValue(_events);
                batch.AddExportedValue(_container);
                _container.Compose(batch);
            }
            catch (Exception)
            {
            }
        }

        protected override object GetInstance(Type serviceType, string key)
        {
            try
            {
                var contract = string.IsNullOrEmpty(key) ? AttributedModelServices.GetContractName(serviceType) : key;
                var exports = _container.GetExportedValues<object>(contract);
                var enumerable = exports as IList<object> ?? exports.ToList();
                if (enumerable.Any())
                {
                    return enumerable.First();
                }
            }
            catch (Exception)
            {
            }
            return null;
        }
    }
}
