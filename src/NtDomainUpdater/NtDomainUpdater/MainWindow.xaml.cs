using System;
using System.Windows;
using System.Windows.Controls;
using NtDomainUpdater.model;
using NtDomainUpdater.viewModel;
using WpfConfiguratorLib;
using WpfConfiguratorLib.view;

namespace NtDomainUpdater
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainViewModel _viewModel
        {
            get { return DataContext as MainViewModel; }
        }

        public MainWindow()
        {
            this.DataContext = new MainViewModel();
            InitializeComponent();
            this.PasswordBox.Password = SecureStringSerializer.ConvertToUnsecureString(_viewModel.ConfigData.Password);
        }

        private void PasswordBox_OnPasswordChanged(object sender, RoutedEventArgs e)
        {
            try
            {
                var pw = (sender as PasswordBox);
                if (pw == null) return;
                _viewModel.ConfigData.Password = pw.SecurePassword;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Connect_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_viewModel.IsConnected) 
                    _viewModel.Disconnect();
                else 
                    _viewModel.Connect();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
