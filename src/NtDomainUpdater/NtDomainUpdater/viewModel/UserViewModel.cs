using ININ.IceLib.Configuration;

namespace NtDomainUpdater.viewModel
{
    public class UserViewModel : ViewModelBase
    {
        private UserConfiguration _userConfiguration;
        private string _newDomainId;
        private ProcessingState _processingState;
        private string _tooltip;


        public UserConfiguration UserConfiguration
        {
            get { return _userConfiguration; }
            private set
            {
                _userConfiguration = value; 
                OnPropertyChanged();
            }
        }

        public string Username
        {
            get { return UserConfiguration.ConfigurationId.Id; }
        }

        public string OldDomainId
        {
            get { return UserConfiguration.NtDomainUser.Value; }
        }

        public string NewDomainId
        {
            get { return _newDomainId; }
            set
            {
                _newDomainId = value;
                OnPropertyChanged();
            }
        }

        public ProcessingState ProcessingState
        {
            get { return _processingState; }
            set
            {
                _processingState = value; 
                OnPropertyChanged();
            }
        }

        public string Tooltip
        {
            get { return _tooltip; }
            set
            {
                _tooltip = value; 
                OnPropertyChanged();
            }
        }


        public UserViewModel(UserConfiguration userConfiguration)
        {
            UserConfiguration = userConfiguration;
        }
    }

    public enum ProcessingState
    {
        None,
        InProcess,
        Error,
        Warning,
        Complete
    }
}
