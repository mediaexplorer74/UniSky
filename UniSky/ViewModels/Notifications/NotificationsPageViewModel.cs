using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FishyFlip.Lexicon.App.Bsky.Actor;
using FishyFlip.Tools;
using Microsoft.Extensions.Logging;
using UniSky.Services;
using Windows.UI.Xaml.Data;

namespace UniSky.ViewModels.Notifications;

public partial class NotificationsPageViewModel : ViewModelBase
{
    private IProtocolService protocolService;


    public NotificationsCollection Notifications { get; }

    public NotificationsPageViewModel(IProtocolService protocolService)
    {
        this.protocolService = protocolService;
        this.Notifications = new NotificationsCollection(this, protocolService);
    }
}
