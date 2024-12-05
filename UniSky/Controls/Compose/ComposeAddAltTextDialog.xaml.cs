using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using UniSky.ViewModels.Compose;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace UniSky.Controls.Compose;

public sealed partial class ComposeAddAltTextDialog : ContentDialog
{
    public ComposeAddAltTextDialog(ComposeViewAttachmentViewModel viewModel)
    {
        this.InitializeComponent();
        this.DataContext = viewModel;
    }
}
