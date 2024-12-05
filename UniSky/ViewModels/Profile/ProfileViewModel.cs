using CommunityToolkit.Mvvm.ComponentModel;
using FishyFlip.Lexicon;
using FishyFlip.Lexicon.App.Bsky.Actor;
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

    public ProfileViewModel(ATObject obj)
    {
        if (obj is ProfileViewBasic profile)
        {
            this.id = profile.Did;
            this.AvatarUrl = profile.Avatar;
            this.Name = string.IsNullOrWhiteSpace(profile.DisplayName) ? 
                profile.Handle.ToString() :
                profile.DisplayName;

            this.Handle = $"@{profile.Handle}";
        }

        if (obj is ProfileViewDetailed profileDetailed)
        {
            this.id = profileDetailed.Did;
            this.AvatarUrl = profileDetailed.Avatar;
            this.Name = string.IsNullOrWhiteSpace(profileDetailed.DisplayName) ? 
                profileDetailed.Handle.ToString() :
                profileDetailed.DisplayName;

            this.Handle = $"@{profileDetailed.Handle}";
        }
    }
}
