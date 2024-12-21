using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using FishyFlip.Lexicon.App.Bsky.Feed;
using UniSky.ViewModels.Posts;

namespace UniSky.ViewModels.Thread;

public partial class ThreadPostViewModel : PostViewModel
{
    [ObservableProperty]
    private bool isSelected;

    public ThreadPostViewModel(ThreadViewPost threadPost, bool isSelected = false) : base(threadPost.Post, false)
    {
        this.IsSelected = isSelected;
        this.HasParent = threadPost.Parent != null;
    }
}
