using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
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
        private bool _isConnected;
        private bool _isConnecting;

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

        #endregion



        public MainViewModel()
        {
            _session.ConnectionStateChanged += SessionOnConnectionStateChanged;
            ConfigData = ConfigManager.Load<ConfigData>(ConfigData.DisplayName) ?? ConfigData;
        }

        #region Private Methods

        private void SessionOnConnectionStateChanged(object sender, ConnectionStateChangedEventArgs e)
        {
            try
            {
                if (_session.ConnectionState == ConnectionState.Up)
                    Context.Send(s => ConfigManager.Save(ConfigData), null);

                IsConnected = _session.ConnectionState == ConnectionState.Up;
                IsConnecting = _session.ConnectionState == ConnectionState.Attempting;
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

                _session.ConnectAsync(new SessionSettings(), new HostSettings(new HostEndpoint(ConfigData.Server)),
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

        #endregion
    }
}
