﻿using System;
using System.Diagnostics;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;

namespace UniSky.ViewModels;

public partial class MenuItemViewModel : ViewModelBase
{
    private readonly HomeViewModel parent;
    private Frame _contentCache; // i dont like this either

    [ObservableProperty]
    private HomePages _page;
    [ObservableProperty]
    private string name;
    [ObservableProperty]
    private string iconGlyph;
    [ObservableProperty]
    private ImageSource avatarUrl; // i dont like this
    [ObservableProperty]
    private int notificationCount;
    [ObservableProperty]
    private bool isSelected;

    public Type FrameType { get; }
    // TODO: unsure if i actually want this or not
    public virtual object NavigationParameter { get; }

    public Frame Content
    {
        get
        {
            if (_contentCache != null) return _contentCache;

            var frame = new Frame();
            frame.ContentTransitions = [new NavigationThemeTransition()];
            frame.SourcePageType = FrameType;
            return _contentCache = frame;
        }
    }

    public MenuItemViewModel(HomeViewModel parent, HomePages page, string iconGlyph, Type frameType)
    {
        Debug.Assert(typeof(Page).IsAssignableFrom(frameType));
        this.parent = parent;

        var resources = ResourceLoader.GetForCurrentView();

        Page = page;
        Name = resources.GetString($"HomePage_{page}");
        IconGlyph = iconGlyph;
        FrameType = frameType;
    }

    public virtual Task LoadAsync() { return Task.CompletedTask; }

    [RelayCommand]
    public void GoBack()
    {
        _contentCache?.GoBack();
    }

    partial void OnIsSelectedChanged(bool value)
    {
        if (value == true)
        {
            parent.SelectedMenuItem = this;
        }
    }
}