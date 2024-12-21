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
    }

    private Visual _headerGrid;
    private CompositionPropertySet _scrollerPropertySet;
    private void Page_Loaded(object sender, RoutedEventArgs e)
    {
        // Retrieve the ScrollViewer that the GridView is using internally
        var scrollViewer = RootList.FindDescendant<ScrollViewer>();

        // Update the ZIndex of the header container so that the header is above the items when scrolling
        var headerPresenter = (UIElement)VisualTreeHelper.GetParent((UIElement)RootList.Header);
        var headerContainer = (UIElement)VisualTreeHelper.GetParent(headerPresenter);
        Canvas.SetZIndex(headerContainer, 1);

        // Get the PropertySet that contains the scroll values from the ScrollViewer
        _scrollerPropertySet = ElementCompositionPreview.GetScrollViewerManipulationPropertySet(scrollViewer);
        _headerGrid = ElementCompositionPreview.GetElementVisual(HeaderContainer);

        var scrollingProperties = _scrollerPropertySet.GetSpecializedReference<ManipulationPropertySetReferenceNode>();
        var headerTranslationAnimation = -scrollingProperties.Translation.Y;
        _headerGrid.StartAnimation("Offset.Y", headerTranslationAnimation);
    }
}
