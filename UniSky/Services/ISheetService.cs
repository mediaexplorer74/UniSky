using System.Threading.Tasks;
using UniSky.Controls.Sheet;

namespace UniSky.Services
{
    public interface ISheetService
    {
        Task<ISheetController> ShowAsync<T>(object parameter = null) where T : SheetControl, new();
    }
}