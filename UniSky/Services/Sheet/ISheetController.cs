using System.Threading.Tasks;
using UniSky.Controls.Sheet;
using Windows.UI.Xaml;

namespace UniSky.Services;

public interface ISheetController
{
    UIElement Root { get; } 
    bool IsFullWindow { get; }
    ISafeAreaService SafeAreaService { get; }
    Task<bool> TryHideSheetAsync();
}
