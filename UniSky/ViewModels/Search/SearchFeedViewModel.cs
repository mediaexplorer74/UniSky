using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Humanizer.Localisation;
using UniSky.Services;
using Windows.ApplicationModel.Resources;

namespace UniSky.ViewModels.Search;

public partial class SearchFeedViewModel : ViewModelBase
{
    private readonly SearchPageViewModel parent;
    private readonly string type;
    private readonly IProtocolService protocolService;

    [ObservableProperty]
    private string name;
    [ObservableProperty]
    private ICollection items;
    [ObservableProperty]
    private bool selected;

    public SearchFeedViewModel(SearchPageViewModel parent, string type, IProtocolService protocolService)
    {
        this.parent = parent;
        this.parent.PropertyChanged += OnParentPropertyChanged;
        this.type = type;
        this.protocolService = protocolService;

        var resources = ResourceLoader.GetForCurrentView();
        this.Name = type switch
        {
            "latest" => resources.GetString("SearchNew"),
            "top" => resources.GetString("SearchHot"),
            "people" => resources.GetString("SearchPeople"),
            //"posts_and_author_threads" => throw new NotImplementedException(),
            _ => throw new NotImplementedException(),
        };
    }

    private void OnParentPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(parent.SelectedFeed))
        {
            Selected = parent.SelectedFeed == this;
        }
    }

    partial void OnSelectedChanged(bool value)
    {
        if (value)
            parent.Select(this);
    }

    internal void DoQuery(string queryText)
    {
        if (type == "people")
        {
            Items = new SearchProfileCollection(queryText, this, this.protocolService);
        }
        else
        {
            Items = new SearchPostsCollection(queryText, this.type, this, this.protocolService);
        }
    }
}
