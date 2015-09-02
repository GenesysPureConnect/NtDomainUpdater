using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using ININ.IceLib.Configuration;
using ININ.IceLib.Connection;
using NtDomainUpdater.model;
using WpfConfiguratorLib;

namespace NtDomainUpdater.viewModel
{
    public class MainViewModel : ViewModelBase
    {
        #region Private Members

        private ConfigData _configData = new ConfigData();
        private readonly Session _session = new Session();
        private bool _isConnected = false;
        private bool _isConnecting = false;
        private UserConfigurationList _userConfigurationList;
        private readonly BackgroundWorker _dataFetcher = new BackgroundWorker();
        private readonly BackgroundWorker _processor = new BackgroundWorker();
        private readonly BackgroundWorker _setSingleUserDataWorker = new BackgroundWorker();
        private ObservableCollection<UserViewModel> _users = new ObservableCollection<UserViewModel>();
        private int _totalUsers;
        private int _matchingUsers;
        private int _fetchProgress = -1;
        private int _processProgress = -1;
        private string _statusText= "";
        private string _connectionStateMessage = "Not connected";
        private string _singleCicUser = "";
        private string _singleNtUser = "";

        #endregion



        #region Public Members

        public ConfigData ConfigData
        {
            get { return _configData; }
            set
            {
                _configData = value; 
                OnPropertyChanged();
            }
        }

        public bool IsConnected
        {
            get { return _isConnected; }
            set
            {
                _isConnected = value;
                OnPropertyChanged();
            }
        }

        public bool IsConnecting
        {
            get { return _isConnecting; }
            set
            {
                _isConnecting = value; 
                OnPropertyChanged();
            }
        }

        public string ConnectionStateMessage
        {
            get { return _connectionStateMessage; }
            set
            {
                _connectionStateMessage = value; 
                OnPropertyChanged();
            }
        }

        public ObservableCollection<UserViewModel> Users
        {
            get { return _users; }
            set
            {
                _users = value; 
                OnPropertyChanged();
            }
        }

        public int TotalUsers
        {
            get { return _totalUsers; }
            set
            {
                _totalUsers = value; 
                OnPropertyChanged();
            }
        }

        public int MatchingUsers
        {
            get { return _matchingUsers; }
            set
            {
                _matchingUsers = value; 
                OnPropertyChanged();
            }
        }

        public int FetchProgress
        {
            get { return _fetchProgress; }
            set
            {
                _fetchProgress = value;
                OnPropertyChanged();
                OnPropertyChanged("IsNotBusy");
            }
        }

        public int ProcessProgress
        {
            get { return _processProgress; }
            set
            {
                _processProgress = value;
                OnPropertyChanged();
                OnPropertyChanged("IsNotBusy");
            }
        }

        public bool IsNotBusy { get { return FetchProgress == -1 && ProcessProgress == -1 && !IsSingleUserBusy; } }

        public string StatusText
        {
            get { return _statusText; }
            set
            {
                _statusText = value; 
                OnPropertyChanged();
            }
        }

        public string SingleCicUser
        {
            get { return _singleCicUser; }
            set
            {
                _singleCicUser = value;
                OnPropertyChanged();
                OnPropertyChanged("HasSingleUser");
            }
        }

        public string SingleNtUser
        {
            get { return _singleNtUser; }
            set
            {
                _singleNtUser = value;
                OnPropertyChanged();
                OnPropertyChanged("HasSingleUser");
            }
        }

        public bool HasSingleUser
        {
            get { return !string.IsNullOrEmpty(SingleCicUser); }
        }

        public bool IsSingleUserBusy { get { return _setSingleUserDataWorker.IsBusy; } }

        #endregion



        public MainViewModel()
        {
            _session.ConnectionStateChanged += SessionOnConnectionStateChanged;

            ConfigData = ConfigManager.Load<ConfigData>(ConfigData.DisplayName) ?? ConfigData;

            _dataFetcher.DoWork += DataFetcherOnDoWork;
            _dataFetcher.RunWorkerCompleted += DataFetcherOnRunWorkerCompleted;

            _processor.DoWork += ProcessorOnDoWork;
            _processor.RunWorkerCompleted += ProcessorOnRunWorkerCompleted;

            _setSingleUserDataWorker.DoWork += SetSingleUserDataWorkerOnDoWork;
            _setSingleUserDataWorker.RunWorkerCompleted += SetSingleUserDataWorkerOnRunWorkerCompleted;

            try
            {
                var name = Environment.UserName;
                name = name.Replace('.', ' ');
                StatusText = "Welcome, " + CultureInfo.CurrentCulture.TextInfo.ToTitleCase(name);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #region Private Methods

        private void SessionOnConnectionStateChanged(object sender, ConnectionStateChangedEventArgs e)
        {
            try
            {
                // Initialize things on UP
                if (_session.ConnectionState == ConnectionState.Up)
                {
                    Context.Send(s =>
                    {
                        ConfigManager.Save(ConfigData);
                        StatusText = "Connected to " + _session.ICServer;
                    }, null);
                    _userConfigurationList = new UserConfigurationList(ConfigurationManager.GetInstance(_session));
                }
                else
                {
                    Context.Send(s => StatusText = _session.ConnectionStateMessage, null);
                }

                // Save states
                Context.Send(s =>
                {
                    IsConnected = _session.ConnectionState == ConnectionState.Up;
                    IsConnecting = _session.ConnectionState == ConnectionState.Attempting;
                    ConnectionStateMessage = _session.ConnectionStateMessage;
                }, null);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DataFetcherOnDoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                Context.Send(s =>
                {
                    // Save current settings
                    ConfigManager.Save(ConfigData);

                    // Get everything ready
                    StatusText = "Fetching users...";
                    Users.Clear();
                    TotalUsers = 0;
                    MatchingUsers = 0;
                    FetchProgress = 0;
                }, null);

                // Stop caching
                if (_userConfigurationList.IsCaching) _userConfigurationList.StopCaching();
                
                // Build query
                var query = _userConfigurationList.CreateQuerySettings();
                query.SetRightsFilterToAdmin();
                query.SetPropertiesToRetrieve(UserConfiguration.Property.Id, UserConfiguration.Property.NtDomainUser);

                // Get results
                _userConfigurationList.StartCaching(query);
                var users = _userConfigurationList.GetConfigurationList();

                // Update count
                Context.Send(s =>
                {
                    TotalUsers = users.Count;
                    StatusText = "Evaluating " + users.Count + " users...";
                }, null);

                // Make UI elements
                for (int i = 0; i < users.Count; i++)
                {
                    var user = users[i];

                    if (user.NtDomainUser.Value != null &&
                        user.NtDomainUser.Value.StartsWith(ConfigData.ExistingDomain + "\\",
                            StringComparison.InvariantCultureIgnoreCase))
                    {
                        Context.Send(s =>
                        {
                            try
                            {
                                var newUser = new UserViewModel(user);
                                newUser.NewDomainId = ConfigData.NewDomain +
                                                      newUser.OldDomainId.Substring(ConfigData.ExistingDomain.Length);
                                Users.Add(newUser);

                                // Update count
                                MatchingUsers = Users.Count;
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(ex.Message + Environment.NewLine + ex.StackTrace, "Error",
                                    MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }, null);
                    }

                    // Update progress
                    Context.Send(s => FetchProgress = (int) (((double) i/(double) users.Count)*100), null);
                }

                Context.Send(s => StatusText = "Fetch complete.", null);

                // Run processor?
                if (e.Argument != null &&
                    e.Argument.ToString().Equals("process", StringComparison.InvariantCultureIgnoreCase))
                {
                    if (!_processor.IsBusy) _processor.RunWorkerAsync();

                    return;
                }
                else
                {
                    _userConfigurationList.StopCaching();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DataFetcherOnRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                // Update counts
                Context.Send(s =>
                {
                    FetchProgress = -1;
                }, null);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ProcessorOnDoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                Context.Send(s => StatusText = "Processing " + Users.Count + " users...", null);

                for (int i = 0; i < Users.Count; i++)
                {
                    var user = Users[i];
                    try
                    {
                        Context.Send(s => user.ProcessingState = ProcessingState.InProcess, null);

                        user.UserConfiguration.PrepareForEdit();
                        user.UserConfiguration.NtDomainUser.Value = user.NewDomainId;
                        user.UserConfiguration.Commit();

                        Context.Send(s => user.ProcessingState = ProcessingState.Complete, null);
                    }
                    catch (Exception ex)
                    {
                        Context.Send(s =>
                        {
                            user.ProcessingState = ProcessingState.Error;
                            user.Tooltip = ex.Message;
                        },null);
                    }

                    Context.Send(s =>
                    {
                        ProcessProgress = (int) (((double) i/(double) Users.Count)*100);
                    }, null);
                }

                Context.Send(s => StatusText = "Processing complete.", null);

                _userConfigurationList.StopCaching();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ProcessorOnRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                // Update counts
                Context.Send(s =>
                {
                    FetchProgress = -1;
                    ProcessProgress = -1;
                }, null);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SetSingleUserDataWorkerOnDoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                Context.Send(s =>
                {
                    StatusText = "Fetching single user...";
                    OnPropertyChanged("IsSingleUserBusy");
                }, null);

                // Stop caching
                if (_userConfigurationList.IsCaching) _userConfigurationList.StopCaching();

                // Create query
                var query = _userConfigurationList.CreateQuerySettings();
                query.SetRightsFilterToAdmin();
                query.SetPropertiesToRetrieve(UserConfiguration.Property.Id, UserConfiguration.Property.NtDomainUser);
                query.SetFilterDefinition(
                    new BasicFilterDefinition<UserConfiguration, UserConfiguration.Property>(
                        UserConfiguration.Property.Id, SingleCicUser, FilterMatchType.Exact));

                // Get results
                _userConfigurationList.StartCaching(query);
                var users = _userConfigurationList.GetConfigurationList();

                // Check for results
                if (users.Count == 0)
                {
                    MessageBox.Show("Unable to find user " + SingleCicUser + ". Check the username and try again.",
                        "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                Context.Send(s =>
                {
                    StatusText = "Updating single user...";
                    OnPropertyChanged("IsSingleUserBusy");
                }, null);

                // Set value
                var user = users[0];
                user.PrepareForEdit();
                user.NtDomainUser.Value = SingleNtUser;
                user.Commit();

                e.Result = true;

                _userConfigurationList.StopCaching();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SetSingleUserDataWorkerOnRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                Context.Send(s =>
                {
                    if (e.Result is bool && (bool) e.Result)
                    {
                        if (string.IsNullOrEmpty(SingleNtUser))
                            StatusText = SingleCicUser + "'s NT user setting has been cleared";
                        else
                            StatusText = SingleCicUser + "'s NT user has been updated to " + SingleNtUser;
                    }
                    else
                        StatusText = "Failed to update " + SingleCicUser;

                    OnPropertyChanged("IsSingleUserBusy");
                }, null);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion



        #region Public Methods

        public void Connect()
        {
            try
            {
                var authSettings = ConfigData.UseWindowsAuth
                    ? new WindowsAuthSettings() as AuthSettings
                    : new ICAuthSettings(ConfigData.Username, ConfigData.Password);

                var customSettings = new Dictionary<string, string>();
                var identitydetailRaw =
                    App.Args.FirstOrDefault(
                        arg => arg.StartsWith("/identitydetail=", StringComparison.InvariantCultureIgnoreCase));
                if (!string.IsNullOrEmpty(identitydetailRaw))
                {
                    customSettings.Add("identitydetail", identitydetailRaw.Substring("/identitydetail=".Length));
                }

                _session.ConnectAsync(new SessionSettings(customSettings), new HostSettings(new HostEndpoint(ConfigData.Server)),
                    authSettings, new StationlessSettings(), null, null);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void Disconnect()
        {
            try
            {
                _session.Disconnect();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void FetchData()
        {
            try
            {
                if (!_dataFetcher.IsBusy)
                {
                    // Update counts
                    Context.Send(s =>
                    {
                        FetchProgress = 0;
                    }, null);

                    // Run worker
                    _dataFetcher.RunWorkerAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void ProcessChanges()
        {
            try
            {
                if (!_dataFetcher.IsBusy)
                {
                    // Update counts
                    Context.Send(s =>
                    {
                        FetchProgress = 0;
                        ProcessProgress = 0;
                    }, null);

                    // Run worker and tell it to process after fetching
                    _dataFetcher.RunWorkerAsync("process");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void SetSingleUserData()
        {
            try
            {
                if (!_setSingleUserDataWorker.IsBusy)
                {
                    // Run worker
                    _setSingleUserDataWorker.RunWorkerAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion
    }
}
