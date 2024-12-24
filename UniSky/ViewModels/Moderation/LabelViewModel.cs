using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using UniSky.Moderation;
using UniSky.Services;

namespace UniSky.ViewModels.Moderation;

public partial class LabelViewModel : ViewModelBase
{
    [ObservableProperty]
    private string text;

    public LabelViewModel(LabelModerationCause label)
    {
        var moderationService = ServiceContainer.Scoped.GetRequiredService<IModerationService>();
        if (moderationService.TryGetLocalisedStringsForLabel(label.LabelDef, out var strings))
        {
            Text = strings.Name;
        }
        else
        {
            Text = label.Label.Val;
        }
    }
}
