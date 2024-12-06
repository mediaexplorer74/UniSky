using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FishyFlip.Lexicon;
using FishyFlip.Lexicon.App.Bsky.Actor;
using FishyFlip.Models;
using UniSky.Pages;
using UniSky.Services;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml;
using Microsoft.Extensions.DependencyInjection;
using Windows.ApplicationModel.Resources;

namespace UniSky.ViewModels.Profile;

public partial class ProfileViewModel : ViewModelBase
{
    protected ATIdentifier id;
    protected ATObject source;

    [ObservableProperty]
    private string name;
    [ObservableProperty]
    private string handle;
    [ObservableProperty]
    private string avatarUrl;
    [ObservableProperty]
    private string bio;

    [ObservableProperty]
    private bool isFollowing;
    [ObservableProperty]
    private bool followsYou;

    public string FollowButtonText
    {
        get
        {
            var strings = ResourceLoader.GetForCurrentView();
            return strings.GetString(IsFollowing ? "ProfileFollowing" : "ProfileFollow");
        }
    }

    public ProfileViewModel()
    {
        this.id = null;
        this.AvatarUrl = "ms-appx:///Assets/Default/Avatar.png";
        this.Name = "Example User";
        this.Handle = "@example.com";
    }

    public ProfileViewModel(ATObject obj)
    {
        this.source = obj;

        Populate(obj);
    }

    protected virtual void Populate(ATObject obj)
    {
        if (obj is ProfileView view)
        {
            this.id = view.Did;
            this.AvatarUrl = view.Avatar;
            this.Name = string.IsNullOrWhiteSpace(view.DisplayName) ?
                view.Handle.ToString() :
                view.DisplayName;

            this.Handle = $"@{view.Handle}";
            this.Bio = view.Description;

            if (view.Viewer is ViewerState viewer)
            {
                this.IsFollowing = viewer.Following != null;
            }
        }

        if (obj is ProfileViewBasic profile)
        {
            this.id = profile.Did;
            this.AvatarUrl = profile.Avatar;
            this.Name = string.IsNullOrWhiteSpace(profile.DisplayName) ?
                profile.Handle.ToString() :
                profile.DisplayName;

            this.Handle = $"@{profile.Handle}";

            if (profile.Viewer is ViewerState viewer)
            {
                this.IsFollowing = viewer.Following != null;
            }
        }

        if (obj is ProfileViewDetailed profileDetailed)
        {
            this.id = profileDetailed.Did;
            this.AvatarUrl = profileDetailed.Avatar;
            this.Name = string.IsNullOrWhiteSpace(profileDetailed.DisplayName) ?
                profileDetailed.Handle.ToString() :
                profileDetailed.DisplayName;

            this.Handle = $"@{profileDetailed.Handle}";

            if (profileDetailed.Viewer is ViewerState viewer)
            {
                this.IsFollowing = viewer.Following != null;
            }
        }
    }

    [RelayCommand]
    private void OpenProfile(UIElement element)
    {
        var service = ServiceContainer.Scoped.GetRequiredService<INavigationServiceLocator>()
            .GetNavigationService("Home");

        service.Navigate<ProfilePage>(this.source, new ContinuumNavigationTransitionInfo() { ExitElement = element });
    }
}
