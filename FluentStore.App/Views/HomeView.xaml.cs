﻿using FluentStore.ViewModels;
using FluentStore.ViewModels.Messages;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace FluentStore.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class HomeView : Page
    {
        public HomeView()
        {
            InitializeComponent();
            ViewModel = new HomeViewModel();
        }

        public HomeViewModel ViewModel
        {
            get => (HomeViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(nameof(ViewModel),
            typeof(HomeViewModel), typeof(HomeView), new PropertyMetadata(null));

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            WeakReferenceMessenger.Default.Send(new SetPageHeaderMessage("Home"));
        }
    }
}
