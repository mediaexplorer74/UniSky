using System;
using System.Diagnostics;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using FishyFlip.Lexicon.App.Bsky.Embed;
using FishyFlip.Models;
using Microsoft.Toolkit.Uwp.UI.Controls;

namespace UniSky.ViewModels.Posts;

public partial class PostEmbedImagesViewModel : PostEmbedViewModel
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsOne), nameof(IsTwo), nameof(IsThree), nameof(IsFour))]
    private int count;
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Image1), nameof(Image2), nameof(Image3), nameof(Image4))]
    private PostEmbedImageViewModel[] images;

    [ObservableProperty]
    private AspectRatioConstraint aspectRatio;

    public PostEmbedImageViewModel Image1
        => Images.ElementAtOrDefault(0);
    public PostEmbedImageViewModel Image2
        => Images.ElementAtOrDefault(1);
    public PostEmbedImageViewModel Image3
        => Images.ElementAtOrDefault(2);
    public PostEmbedImageViewModel Image4
        => Images.ElementAtOrDefault(3);

    // i shouldn't need these *grumble grumble*
    public bool IsOne => Count == 1;
    public bool IsTwo => Count == 2;
    public bool IsThree => Count == 3;
    public bool IsFour => Count == 4;

    public PostEmbedImagesViewModel(ViewImages embed) : base(embed)
    {
        Count = embed.Images.Count;
        Images = embed.Images.Select(i => new PostEmbedImageViewModel(i)).ToArray();
        Debug.Assert(Images.Length > 0 && Images.Length <= 4);


        var firstRatio = embed.Images[0].AspectRatio;
        if (Images.Length == 1 && firstRatio == null)
        {
            AspectRatio = new();
        }
        else
        {
            AspectRatio = new AspectRatioConstraint(Images.Length switch
            {
                1 => Math.Max((double)firstRatio.Width.Value / firstRatio.Height.Value, 0.75),
                2 => 2.0,
                3 => 2.0,
                4 => 3.0 / 2.0,
                _ => throw new NotImplementedException()
            });
        }
    }
}
