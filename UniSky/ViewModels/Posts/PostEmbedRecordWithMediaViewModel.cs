using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using FishyFlip.Lexicon;
using FishyFlip.Lexicon.App.Bsky.Embed;

namespace UniSky.ViewModels.Posts;

public partial class PostEmbedRecordWithMediaViewModel : PostEmbedViewModel
{
    [ObservableProperty]
    private PostEmbedViewModel record;
    [ObservableProperty]
    private PostEmbedViewModel media;

    public PostEmbedRecordWithMediaViewModel(ViewRecordWithMedia embed) : base(embed)
    {
        Record = PostViewModel.CreateEmbedViewModel(embed.Record, true);
        Media = PostViewModel.CreateEmbedViewModel(embed.Media, false);
    }
}
