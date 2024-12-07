using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using FishyFlip.Lexicon;
using FishyFlip.Lexicon.App.Bsky.Embed;
using FishyFlip.Lexicon.App.Bsky.Feed;
using FishyFlip.Lexicon.Tools.Ozone.Team;
using Humanizer;
using UniSky.ViewModels.Profile;
using UniSky.ViewModels.Text;

namespace UniSky.ViewModels.Posts;

public partial class PostEmbedPostViewModel : PostEmbedViewModel
{
    private readonly ViewRecord view;
    private readonly Post post;

    [ObservableProperty]
    private string text;
    [ObservableProperty]
    private RichTextViewModel richText;
    [ObservableProperty]
    private ProfileViewModel author;
    [ObservableProperty]
    private string date;
    [ObservableProperty]
    private PostEmbedViewModel embed;

    public PostEmbedPostViewModel(ViewRecord view, Post post) : base(view)
    {
        this.view = view;
        this.post = post;

        Text = post.Text;
        RichText = new RichTextViewModel(post.Text, post.Facets ?? []);
        Author = new ProfileViewModel(view.Author);

        Embed = PostViewModel.CreateEmbedViewModel(view.Embeds?.FirstOrDefault(), true);

        var timeSinceIndex = DateTime.Now - (view.IndexedAt.Value.ToLocalTime());
        var date = timeSinceIndex.Humanize(1, minUnit: Humanizer.Localisation.TimeUnit.Second);
        Date = date;
    }
}
