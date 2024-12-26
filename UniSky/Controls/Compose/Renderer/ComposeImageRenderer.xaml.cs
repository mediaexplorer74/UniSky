using System;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace UniSky.Controls.Compose.Renderer
{
    public sealed partial class ComposeImageRenderer : UserControl
    {
        public ComposeImageRenderer()
        {
            this.InitializeComponent();
        }

        public async Task LoadImage(RandomAccessStreamReference reference)
        {
            using var bmp = await reference.OpenReadAsync();
            var decoder = await BitmapDecoder.CreateAsync(bmp);
            using var softwareBmp = await decoder.GetSoftwareBitmapAsync(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);

            var softwreBitmapSource = new SoftwareBitmapSource();
            await softwreBitmapSource.SetBitmapAsync(softwareBmp);

            Image.Source = softwreBitmapSource;
        }
    }
} 
