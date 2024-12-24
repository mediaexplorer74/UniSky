using System;

namespace UniSky.Moderation;

public enum ModerationBehaviorContext
{
    ProfileList,
    ProfileView,
    Avatar,
    Banner,
    DisplayName,
    ContentList,
    ContentView,
    ContentMedia
}

public struct ModerationBehavior
{
    public static readonly ModerationBehavior BlockBehaviour = new()
    {
        ProfileList = ModerationBehaviorType.Blur,
        ProfileView = ModerationBehaviorType.Alert,
        Avatar = ModerationBehaviorType.Blur,
        Banner = ModerationBehaviorType.Blur,
        ContentList = ModerationBehaviorType.Blur,
        ContentView = ModerationBehaviorType.Blur
    };

    public static readonly ModerationBehavior MuteBehaviour = new()
    {
        ProfileList = ModerationBehaviorType.Inform,
        ProfileView = ModerationBehaviorType.Alert,
        ContentList = ModerationBehaviorType.Blur,
        ContentView = ModerationBehaviorType.Inform
    };

    public static readonly ModerationBehavior MuteWordBehavour = new()
    {
        ContentList = ModerationBehaviorType.Blur,
        ContentView = ModerationBehaviorType.Blur
    };

    public static readonly ModerationBehavior HideBehaviour
        = MuteWordBehavour;

    public readonly ModerationBehaviorType this[ModerationBehaviorContext target]
        => target switch
        {
            ModerationBehaviorContext.ProfileList => ProfileList,
            ModerationBehaviorContext.ProfileView => ProfileView,
            ModerationBehaviorContext.Avatar => Avatar,
            ModerationBehaviorContext.Banner => Banner,
            ModerationBehaviorContext.DisplayName => DisplayName,
            ModerationBehaviorContext.ContentList => ContentList,
            ModerationBehaviorContext.ContentView => ContentView,
            ModerationBehaviorContext.ContentMedia => ContentMedia,
            _ => throw new InvalidOperationException()
        };

    public ModerationBehaviorType ProfileList;
    public ModerationBehaviorType ProfileView;
    public ModerationBehaviorType Avatar;
    public ModerationBehaviorType Banner;
    public ModerationBehaviorType DisplayName;
    public ModerationBehaviorType ContentList;
    public ModerationBehaviorType ContentView;
    public ModerationBehaviorType ContentMedia;
}
