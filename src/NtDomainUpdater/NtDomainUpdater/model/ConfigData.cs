using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using NtDomainUpdater.Annotations;
using NtDomainUpdater.viewModel;
using WpfConfiguratorLib.attributes;
using WpfConfiguratorLib.entities;

namespace NtDomainUpdater.model
{
    public class ConfigData : ConfigGroup, INotifyPropertyChanged
    {
        private string _server;
        private bool _useWindowsAuth;
        private string _username;
        private SecureString _password;
        private string _existingDomain;
        private string _newDomain;


        public event PropertyChangedEventHandler PropertyChanged;

        public override string DisplayName
        {
            get { return "NT Domain Updater Configuration"; }
        }

        public override string Description
        {
            get { return "config data"; }
        }


        [ConfigProperty("Server", Description = "CIC Server", DefaultValue = "")]
        public string Server
        {
            get { return _server; }
            set
            {
                _server = value;
                OnPropertyChanged();
            }
        }

        [ConfigProperty("UseWindowsAuth", Description = "Use Windows auth", DefaultValue = false)]
        public bool UseWindowsAuth
        {
            get { return _useWindowsAuth; }
            set
            {
                _useWindowsAuth = value;
                OnPropertyChanged();
            }
        }

        [ConfigProperty("Username", Description = "CIC Username", DefaultValue = "")]
        public string Username
        {
            get { return _username; }
            set
            {
                _username = value;
                OnPropertyChanged();
            }
        }

        [ConfigProperty("Password", Description = "CIC Password", DefaultValue = "")]
        public SecureString Password
        {
            get { return _password; }
            set
            {
                _password = value;
                OnPropertyChanged();
            }
        }

        [ConfigProperty("ExistingDomain", Description = "Existing Domain", DefaultValue = "")]
        public string ExistingDomain
        {
            get { return _existingDomain.Trim(); }
            set
            {
                _existingDomain = value;
                OnPropertyChanged();
            }
        }

        [ConfigProperty("NewDomain", Description = "New Domain", DefaultValue = "")]
        public string NewDomain
        {
            get { return _newDomain.Trim(); }
            set
            {
                _newDomain = value;
                OnPropertyChanged();
            }
        }


        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
