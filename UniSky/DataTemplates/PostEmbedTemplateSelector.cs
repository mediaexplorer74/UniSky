using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniSky.ViewModels.Posts;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace UniSky.DataTemplates;

internal class PostEmbedTemplateSelector : DataTemplateSelector
{
    public DataTemplate VideoEmbedTemplate { get; set; }
    public DataTemplate ImagesEmbedTemplate { get; set; }
    public DataTemplate PostEmbedTemplate { get; set; }
    public DataTemplate ExternalEmbedTemplate { get; set; }

    protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
    {
        return item switch
        {
            PostEmbedImagesViewModel => ImagesEmbedTemplate,
            PostEmbedVideoViewModel => VideoEmbedTemplate,
            PostEmbedPostViewModel => PostEmbedTemplate,
            PostEmbedExternalViewModel => ExternalEmbedTemplate,
            _ => null,
        };
    }

    protected override DataTemplate SelectTemplateCore(object item)
    {
        return SelectTemplateCore(item, null);
    }
}
