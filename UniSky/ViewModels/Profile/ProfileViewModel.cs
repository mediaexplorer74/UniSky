using CommunityToolkit.Mvvm.ComponentModel;
using FishyFlip.Models;

namespace UniSky.ViewModels.Profiles;

public partial class ProfileViewModel : ViewModelBase
{
    protected ATIdentifier id;

    [ObservableProperty]
    private string name;
    [ObservableProperty]
    private string handle;
    [ObservableProperty]
    private string avatarUrl;

    public ProfileViewModel()
    {
        this.id = null;
        this.AvatarUrl = "ms-appx:///Assets/Default/Avatar.png";
        this.Name = "Example User";
        this.Handle = "@example.com";
    }

    public ProfileViewModel(FeedProfile profile)
    {
        this.id = profile.Did;
        this.AvatarUrl = profile.Avatar;
        this.Name = profile.DisplayName;
        this.Handle = $"@{profile.Handle}";
    }

    public ProfileViewModel(ActorProfile profile)
    {
        this.id = profile.Did;
        this.AvatarUrl = profile.Avatar;
        this.Name = profile.DisplayName;
        this.Handle = $"@{profile.Handle}";
    }
}
