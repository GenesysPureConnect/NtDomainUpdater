using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;
using NtDomainUpdater.Annotations;

namespace NtDomainUpdater.viewModel
{
    public class ViewModelBase : INotifyPropertyChanged
    {
        #region Private Members

        protected SynchronizationContext Context { get; private set; }

        #endregion



        #region Public Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion



        public ViewModelBase()
        {
            Context = SynchronizationContext.Current;
            if (Context == null)
                MessageBox.Show(this.GetType() + " was created outside of the UI thread!", "Threading error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
        }



        #region Private Methods

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion



        #region Public Methods



        #endregion
    }
}
