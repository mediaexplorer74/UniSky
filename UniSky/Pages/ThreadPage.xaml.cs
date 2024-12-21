using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using FishyFlip.Lexicon.App.Bsky.Actor;
using FishyFlip.Lexicon;
using Microsoft.Extensions.DependencyInjection;
using UniSky.Services;
using UniSky.ViewModels.Profile;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using FishyFlip.Models;
using UniSky.ViewModels.Thread;
using Microsoft.Toolkit.Uwp.UI.Animations.Expressions;
using Windows.UI.Composition;
using Windows.UI.Xaml.Hosting;
using Microsoft.Toolkit.Uwp.UI.Extensions;
using EF = Microsoft.Toolkit.Uwp.UI.Animations.Expressions.ExpressionFunctions;
using System.Numerics;
using Windows.Networking.NetworkOperators;

namespace UniSky.Pages;

public sealed partial class ThreadPage : Page
{
    public ThreadViewModel ViewModel
    {
        get => (ThreadViewModel)GetValue(ViewModelProperty);
        set => SetValue(ViewModelProperty, value);
    }

    public static readonly DependencyProperty ViewModelProperty =
        DependencyProperty.Register("ViewModel", typeof(ThreadViewModel), typeof(ThreadPage), new PropertyMetadata(null));

    public ThreadPage()
    {
        this.InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        var safeAreaService = ServiceContainer.Scoped.GetRequiredService<ISafeAreaService>();
        safeAreaService.SafeAreaUpdated += OnSafeAreaUpdated;

        if (e.Parameter is not (ATUri))
            return;

        if (e.Parameter is ATUri uri)
            this.DataContext = ViewModel = ActivatorUtilities.CreateInstance<ThreadViewModel>(ServiceContainer.Default, uri);
    }

    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        var safeAreaService = ServiceContainer.Scoped.GetRequiredService<ISafeAreaService>();
        safeAreaService.SafeAreaUpdated -= OnSafeAreaUpdated;
    }

    private void OnSafeAreaUpdated(object sender, SafeAreaUpdatedEventArgs e)
    {
        HeaderGrid.Padding = e.SafeArea.Bounds with { Bottom = 0, Left = 0, Right = 0 };
        HandleScrolling();
    }

    private void Page_Loaded(object sender, RoutedEventArgs e)
    {
        HandleScrolling();
    }

    private void HandleScrolling()
    {
        var stackPanel = RootList.FindDescendant<ItemsStackPanel>();
        if (stackPanel == null)
            return;

        stackPanel.Margin = new Thickness(0, HeaderContainer.ActualHeight, 0, ActualHeight - HeaderContainer.ActualHeight);
    }
}
