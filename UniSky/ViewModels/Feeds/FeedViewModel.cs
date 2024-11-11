using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FishyFlip.Models;
using UniSky.Helpers;
using UniSky.Services;
using Windows.Foundation;

namespace UniSky.ViewModels.Feeds;

#nullable enable

public enum FeedType
{
    Following,
    Custom,
    Author
}

public partial class FeedViewModel : ViewModelBase
{
    private readonly FeedType type;
    private readonly ATUri? id;
    private readonly FeedRecord? generator;
    private readonly IProtocolService protocolService;

    [ObservableProperty]
    private string name = null!;
    [ObservableProperty]
    private FeedItemCollection items = null!;

    protected FeedViewModel(FeedType type)
    {
        this.type = type;
    }

    public FeedViewModel(FeedType type, ATUri? id, FeedRecord? record, IProtocolService protocolService)
    {
        this.type = type;
        this.id = id;
        this.generator = record;
        this.protocolService = protocolService;

        this.Name = record?.DisplayName ?? "Following";
        this.Items = new FeedItemCollection(this, type, id, protocolService);
    }

    public async Task RefreshAsync(Deferral? deferral = null)
    {
        await this.Items.RefreshAsync();
        deferral?.Complete();
    }

    internal void OnFeedLoadError(Exception ex)
    {
        base.SetErrored(ex);
    }
}
