using System;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FishyFlip.Lexicon.App.Bsky.Embed;
using FishyFlip.Lexicon.App.Bsky.Feed;
using Humanizer;
using Microsoft.Extensions.DependencyInjection;
using UniSky.Pages;
using UniSky.Services;
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
    [ObservableProperty]
    private ContentWarningViewModel warning;

    public PostEmbedPostViewModel(ViewRecord view, Post post) : base(view)
    {
        this.view = view;
        this.post = post;

        Text = post.Text;
        RichText = new RichTextViewModel(post.Text, post.Facets ?? []);
        Author = new ProfileViewModel(view.Author);

        Embed = PostViewModel.CreateEmbedViewModel(view.Embeds?.FirstOrDefault(), true);

        if (this.view.Labels?.Count is not (null or 0))
        {
            Warning = new ContentWarningViewModel(this.view.Labels);
        }

        var timeSinceIndex = DateTime.Now - (view.IndexedAt.Value.ToLocalTime());
        var date = timeSinceIndex.Humanize(1, minUnit: Humanizer.Localisation.TimeUnit.Second);
        Date = date;
    }

    [RelayCommand]
    private void OpenThread()
    {
        var navigationService = ServiceContainer.Scoped.GetRequiredService<INavigationServiceLocator>()
            .GetNavigationService("Home");
        navigationService.Navigate<ThreadPage>(this.view.Uri);
    }
}
