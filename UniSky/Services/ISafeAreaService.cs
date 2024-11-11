using System;

namespace UniSky.Services
{
    internal interface ISafeAreaService
    {
        SafeAreaInfo State { get; }
        event EventHandler<SafeAreaUpdatedEventArgs> SafeAreaUpdated;
    }
}