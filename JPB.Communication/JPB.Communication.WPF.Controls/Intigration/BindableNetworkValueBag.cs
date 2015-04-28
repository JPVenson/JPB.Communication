using System.Collections.Specialized;
using JPB.Communication.Shared;
using JPB.WPFBase.MVVM.ViewModel;
using System;

namespace JPB.Communication.WPF.Controls.Intigration
{
    /// <summary>
    /// This class holds and Updates values that will be Synced over the Network
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class BindableNetworkValueBag<T> : NetworkValueBag<T>
    {
        public BindableNetworkValueBag(ushort port, string guid)
            : base(port, guid)
        {
            WpfSyncHelper = new ThreadSaveViewModelBase();
            LocalValues = new ThreadSaveObservableCollection<T>();
        }

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            WpfSyncHelper.ThreadSaveAction(() =>
            {
                base.OnCollectionChanged(e);
            });
        }

        public ThreadSaveViewModelActor WpfSyncHelper { get; private set; }
    }
}
