using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using NtDomainUpdater.viewModel;
using WpfConfiguratorLib;

namespace NtDomainUpdater
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainViewModel ViewModel
        {
            get { return DataContext as MainViewModel; }
        }

        public MainWindow()
        {
            this.Closing += OnClosing;

            this.DataContext = new MainViewModel();
            InitializeComponent();
            this.PasswordBox.Password = SecureStringSerializer.ConvertToUnsecureString(ViewModel.ConfigData.Password);
        }

        private void OnClosing(object sender, CancelEventArgs cancelEventArgs)
        {
            try
            {
                ViewModel.Disconnect();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void PasswordBox_OnPasswordChanged(object sender, RoutedEventArgs e)
        {
            try
            {
                var pw = (sender as PasswordBox);
                if (pw == null) return;
                ViewModel.ConfigData.Password = pw.SecurePassword;
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
                if (ViewModel.IsConnected) 
                    ViewModel.Disconnect();
                else 
                    ViewModel.Connect();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FetchData_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ViewModel.FetchData();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ProcessChanges_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ViewModel.ProcessChanges();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ConnectionElement_OnKeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key != Key.Enter && e.Key != Key.Return) return;

                if (ViewModel.IsConnected)
                    ViewModel.Disconnect();
                else
                    ViewModel.Connect();
            }
            catch (Exception ex)
            {
                // Supress error, don't care
            }
        }

        private void ConfigurationElement_OnKeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key != Key.Enter && e.Key != Key.Return) return;

                ViewModel.FetchData();
            }
            catch (Exception ex)
            {
                // Supress error, don't care
            }
        }
    }
}
