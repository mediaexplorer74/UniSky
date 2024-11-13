using System.Numerics;
using CommunityToolkit.Mvvm.DependencyInjection;
using FishyFlip.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Graphics.Canvas.Effects;
using Microsoft.Toolkit.Uwp.UI.Animations.Expressions;
using Microsoft.Toolkit.Uwp.UI.Extensions;
using UniSky.Services;
using UniSky.ViewModels.Profile;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using EF = Microsoft.Toolkit.Uwp.UI.Animations.Expressions.ExpressionFunctions;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace UniSky.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ProfilePage : Page
    {
        private Compositor _compositor;
        private CompositionPropertySet _props;
        private CompositionPropertySet _scrollerPropertySet;
        private Visual _headerGrid;
        private Visual _profileImage;
        private Visual _textContainer;
        private SpriteVisual _blurredBackgroundImageVisual;

        public ProfilePage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is not FeedProfile profile)
                return;

            this.DataContext = ActivatorUtilities.CreateInstance<ProfilePageViewModel>(Ioc.Default, profile);

            var safeAreaService = Ioc.Default.GetRequiredService<ISafeAreaService>();
            safeAreaService.SafeAreaUpdated += OnSafeAreaUpdated;
        }

        private void OnSafeAreaUpdated(object sender, SafeAreaUpdatedEventArgs e)
        {
            UpdateSizeDependentProperties();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            // Retrieve the ScrollViewer that the GridView is using internally
            var scrollViewer = RootList.FindDescendant<ScrollViewer>();

            // Update the ZIndex of the header container so that the header is above the items when scrolling
            var headerPresenter = (UIElement)VisualTreeHelper.GetParent((UIElement)RootList.Header);
            var headerContainer = (UIElement)VisualTreeHelper.GetParent(headerPresenter);
            Canvas.SetZIndex((UIElement)headerContainer, 1);

            // Get the PropertySet that contains the scroll values from the ScrollViewer
            _scrollerPropertySet = ElementCompositionPreview.GetScrollViewerManipulationPropertySet(scrollViewer);
            _compositor = _scrollerPropertySet.Compositor;

            _headerGrid = ElementCompositionPreview.GetElementVisual(HeaderGrid);
            _profileImage = ElementCompositionPreview.GetElementVisual(ProfileImage);
            _textContainer = ElementCompositionPreview.GetElementVisual(TextContainer);
            _blurredBackgroundImageVisual = _compositor.CreateSpriteVisual();

            _props = _compositor.CreatePropertySet();
            _props.InsertScalar("progress", 0);
            _props.InsertScalar("scaleFactor", 0.375f);

            UpdateSizeDependentProperties();

            // Get references to our property sets for use with ExpressionNodes
            var scrollingProperties = _scrollerPropertySet.GetSpecializedReference<ManipulationPropertySetReferenceNode>();
            var props = _props.GetReference();
            var progressNode = props.GetScalarProperty("progress");
            var clampSizeNode = props.GetScalarProperty("clampSize");
            var scaleFactorNode = props.GetScalarProperty("scaleFactor");
            var sizeNode = props.GetScalarProperty("size");
            var headerSizeNode = props.GetScalarProperty("headerSize");

            // Create a blur effect to be animated based on scroll position
            var blurEffect = new GaussianBlurEffect()
            {
                Name = "blur",
                BlurAmount = 0.0f,
                BorderMode = EffectBorderMode.Hard,
                Optimization = EffectOptimization.Balanced,
                Source = new CompositionEffectSourceParameter("source")
            };

            var blurBrush = _compositor.CreateEffectFactory(blurEffect, ["blur.BlurAmount"])
                .CreateBrush();

            blurBrush.SetSourceParameter("source", _compositor.CreateBackdropBrush());

            _blurredBackgroundImageVisual.Brush = blurBrush;
            ElementCompositionPreview.SetElementChildVisual(ProfileBanner, _blurredBackgroundImageVisual);

            ExpressionNode progressAnimation = EF.Clamp(-scrollingProperties.Translation.Y / clampSizeNode, 0, 1);
            _props.StartAnimation("progress", progressAnimation);

            ExpressionNode blurAnimation = EF.Lerp(0, 20, progressNode);
            _blurredBackgroundImageVisual.Brush.Properties.StartAnimation("blur.BlurAmount", blurAnimation);

            Visual profileContainer = ElementCompositionPreview.GetElementVisual(ProfileContainer);
            ScalarNode headerTranslationAnimation = EF.Conditional(progressNode < 1, 0, -scrollingProperties.Translation.Y - clampSizeNode);
            profileContainer.StartAnimation("Offset.Y", headerTranslationAnimation);

            ScalarNode headerGridTranslationAnimation = EF.Min(clampSizeNode, sizeNode - headerSizeNode) * progressNode;
            ScalarNode headerScaleAnimation = EF.Lerp(1, 1.25f, EF.Clamp(scrollingProperties.Translation.Y / 50, 0, 1));

            _headerGrid.StartAnimation("Offset.Y", headerGridTranslationAnimation);
            _headerGrid.StartAnimation("Scale.X", headerScaleAnimation);
            _headerGrid.StartAnimation("Scale.Y", headerScaleAnimation);

            ScalarNode scaleAnimation = EF.Lerp(1, scaleFactorNode, progressNode);
            ScalarNode translateAnimation = EF.Lerp(0, clampSizeNode, progressNode);
            _profileImage.StartAnimation("Scale.X", scaleAnimation);
            _profileImage.StartAnimation("Scale.Y", scaleAnimation);
            _profileImage.StartAnimation("Offset.Y", translateAnimation);

            ElementCompositionPreview.SetIsTranslationEnabled(TextContainer, true);

            ScalarNode textOpacityAnimation = EF.Clamp(1 - (progressNode * 1.5f), 0, 1);
            ScalarNode textTranslateAnimation = EF.Lerp(0, 64, progressNode);
            _textContainer.StartAnimation("Opacity", textOpacityAnimation);
            _textContainer.StartAnimation("Translation", EF.Vector3(-textTranslateAnimation, 0, 0));

            Visual scrolledDisplayNameContainer = ElementCompositionPreview.GetElementVisual(ScrolledDisplayNameContainer);
            ScalarNode scrolledTextOpacityAnimation = EF.Clamp((progressNode - 0.5f) * 2f, 0, 1);
            scrolledDisplayNameContainer.StartAnimation("Offset.Y", translateAnimation);
            scrolledDisplayNameContainer.StartAnimation("Opacity", scrolledTextOpacityAnimation);
        }

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (_props == null)
                return;

            UpdateSizeDependentProperties();
        }

        private void UpdateSizeDependentProperties()
        {
            var safeAreaService = Ioc.Default.GetRequiredService<ISafeAreaService>();

            var titleBarHeight = (float)safeAreaService.State.Bounds.Top;
            var size = (float)ProfileContainer.ActualHeight;
            var clampHeight = (float)(52 + titleBarHeight);
            var pixelsToMove = (float)(size - clampHeight);

            if (_props != null)
            {
                _props.InsertScalar("size", size);
                _props.InsertScalar("clampSize", pixelsToMove);
                _props.InsertScalar("headerOffset", clampHeight);
                _props.InsertScalar("headerSize", (float)HeaderGrid.ActualHeight);
            }

            if (_blurredBackgroundImageVisual != null)
                _blurredBackgroundImageVisual.Size = new Vector2((float)ProfileBanner.ActualWidth, (float)ProfileBanner.ActualHeight);

            if (_headerGrid != null)
                _headerGrid.CenterPoint = new Vector3((float)(HeaderGrid.ActualWidth / 2), (float)HeaderGrid.ActualHeight, 0);
            if (_textContainer != null)
                _textContainer.CenterPoint = new Vector3((float)TextContainer.ActualWidth / 2, (float)TextContainer.ActualHeight / 2, 0);
            if (_profileImage != null)
                _profileImage.CenterPoint = new Vector3(4, (float)ProfileImage.ActualHeight + 4, 0);
        }

        private void ProfileContainer_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var scrollView = RootList.FindDescendant<ScrollViewer>();
            if (scrollView is null)
                return;

            scrollView.ChangeView(0, 0, null);
        }
    }
}
