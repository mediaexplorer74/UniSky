using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Microsoft.Toolkit.Uwp.UI.Media;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

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
