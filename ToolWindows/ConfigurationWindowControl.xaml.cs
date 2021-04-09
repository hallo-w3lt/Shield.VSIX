﻿using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
//using Shield.Client;
using ShieldVSExtension.Configuration;
using ShieldVSExtension.UI_Extensions;

namespace ShieldVSExtension.ToolWindows
{
    public partial class ConfigurationWindowControl : Window
    {
        private readonly ConfigurationViewModel _viewModel;
        private const string ExtensionConfigurationFile = "ExtensionConfiguration";

        //public SecureLocalStorage.SecureLocalStorage LocalStorage { get; set; }

        private ShieldExtensionConfiguration ExtensionConfiguration { get; }

        public ConfigurationWindowControl(ConfigurationViewModel viewModel)
        {
            InitializeComponent();

            _viewModel = viewModel;
            DataContext = viewModel;

            //LocalStorage = new SecureLocalStorage.SecureLocalStorage(
            //    new SecureLocalStorage.CustomLocalStorageConfig(null, "DotnetsaferShieldForVisualStudio").WithDefaultKeyBuilder()
            //);

            //ExtensionConfiguration = LocalStorage.Exists(ExtensionConfigurationFile) ?
            //    LocalStorage.Get<ShieldExtensionConfiguration>(ExtensionConfigurationFile) :
            //    new ShieldExtensionConfiguration();

            ExtensionConfiguration = new ShieldExtensionConfiguration() {ApiToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjE4ODgzMmEyLTUxODktNDMwZS05NGFmLTc3MTJkZTBiM2FmZCIsInVuaXF1ZV9uYW1lIjoiOTE4ZDgxNmYtZDI4Zi00YThjLWE3MWItMzZiM2VkYTdlNjY4IiwidmVyc2lvbiI6IjEuMC4wIiwic2VydmljZSI6ImRvdG5ldHNhZmVyIiwiZWRpdGlvbiI6ImNvbW11bml0eSIsImp0aSI6IjY5YTlkNjdiLWM4ZTgtNGNhYS05MWM3LTk5NDIwZGE2ZDU5YyIsImV4cCI6MTYxNzQwMjg1Mn0.Ohr4WeJaU5w_2CP1QhzAepis_xKmDheLYxz4BN2rLEo" };

            if (!string.IsNullOrEmpty(ExtensionConfiguration.ApiToken))
                try
                {
                    //_ = ShieldClient.CreateInstance(ExtensionConfiguration.ApiToken);
                    _viewModel.IsValidClient = true;
                    ApiKeyBox.Password = ExtensionConfiguration.ApiToken;
                    ConnectButton.IsEnabled = false;
                }
                catch (Exception)
                {
                    _viewModel.IsValidClient = false;
                }
            else _viewModel.IsValidClient = false;

            if (!_viewModel.IsValidClient)
                ShieldControl.SelectedIndex = 1;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //_ = ShieldClient.CreateInstance(ApiKeyBox.Password);
                _viewModel.IsValidClient = true;
                ExtensionConfiguration.ApiToken = ApiKeyBox.Password;
                ShieldControl.SelectedIndex = 0;
                SaveExtensionConfiguration();
            }
            catch (Exception)
            {
                _viewModel.IsValidClient = false;
                MessageBox.Show("The api key is not valid, check that it has not been revoked and the associated scopes.","Invalid Shield API Key",MessageBoxButton.OK,MessageBoxImage.Error);
            }
        }

        private void SaveExtensionConfiguration()
        {}//=> LocalStorage.Set(ExtensionConfigurationFile, ExtensionConfiguration);
        

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var removedItems = e.RemovedItems.OfType<ConfigurationViewModel.ProjectViewModel>();
            foreach (var item in removedItems)
                _viewModel.SelectedProjects.Remove(item);

            var addedItems = e.AddedItems.OfType<ConfigurationViewModel.ProjectViewModel>().Except(_viewModel.SelectedProjects);
            foreach (var item in addedItems)
                _viewModel.SelectedProjects.Add(item);
        }
    

        private void ListBox_Loaded(object sender, RoutedEventArgs e)
        {
            ((ListBox)sender).ScrollIntoView(_viewModel.SelectedProject);
        }

        private void EnableMenuItem_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.Enable(true);
        }

        private void DisableMenuItem_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.Enable(false);
        }

        private void AddCustomProtectionConfigMenuItem_Click(object sender, RoutedEventArgs e)
        {
            foreach (var viewModelSelectedProject in _viewModel.SelectedProjects)
            {
                viewModelSelectedProject.InheritFromProject = false;
                viewModelSelectedProject.ApplicationPreset = _viewModel.ProjectPresets.First(preset =>
                    preset.Name.ToLower().Equals(((MenuItem) sender).Header.ToString().ToLower()));
            }
        }

        private void OutputFilesComboBox_Loaded(object sender, RoutedEventArgs e)
        {
            var comboBox = (ComboBox)sender;
            comboBox.Focus();

            var projectViewModel = _viewModel.SelectedProject;
            if (projectViewModel == null)
                return;

            var path = projectViewModel.OutputFullPath;

            if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path))
            {
                comboBox.ItemsSource = null;
                return;
            }

            var files = Directory.GetFiles(path, "*", SearchOption.TopDirectoryOnly)
                .Select(Path.GetFileName)
                .OrderByDescending(p => p.StartsWith(projectViewModel.Name))
                .ThenBy(Path.GetFileNameWithoutExtension)
                .ToArray();

            comboBox.ItemsSource = files;
        }

        private void ApiKeyBox_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            ConnectButton.IsEnabled = ExtensionConfiguration.ApiToken != ApiKeyBox.Password;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.Save();
            DialogResult = true;
            Close();
        }

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void InheritConfigFromGlobal_Copy_Checked(object sender, RoutedEventArgs e)
        {

        }
    }
}