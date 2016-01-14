using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;

// The Templated Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234235

namespace Geeky.UI
{
    [TemplatePart(Name = ControlMainFrameName, Type = typeof(Frame))]
    [TemplatePart(Name = SidebarGridName, Type = typeof(ContentPresenter))]
    [TemplatePart(Name = EdgeGridName, Type = typeof(UIElement))]
    [TemplatePart(Name = SideTransformName, Type = typeof(CompositeTransform))]
    [TemplatePart(Name = FadeOutSidebarGridAnimationName, Type = typeof(DoubleAnimation))]
    [TemplatePart(Name = FadeInPropertyName, Type = typeof(Storyboard))]
    [TemplatePart(Name = FadeOutPropertyName, Type = typeof(Storyboard))]
    [TemplatePart(Name = ControlMainFrameThemeTransitionName, Type = typeof(EdgeUIThemeTransition))]
    public sealed class PaneView : Control
    {
        private const string ControlMainFrameName = "ControlMainFrame";
        private const string SidebarGridName = "SidebarGrid";
        private const string EdgeGridName = "EdgeGrid";
        private const string SideTransformName = "SideTransform";
        private const string FadeOutSidebarGridAnimationName = "FadeOutSidebarGridAnimation";
        private const string FadeInPropertyName = "FadeInProperty";
        private const string FadeOutPropertyName = "FadeOutProperty";
        private const string ControlMainFrameThemeTransitionName = "ControlMainFrameThemeTransition";

        private Frame controlMainFrame;
        private ContentPresenter sidebarGrid;
        private UIElement edgeGrid;
        private CompositeTransform sideTransform;
        private DoubleAnimation fadeOutSidebarGridAnimation;
        private Storyboard fadeInProperty;
        private Storyboard fadeOutProperty;
        private EdgeUIThemeTransition controlMainFrameThemeTransition;

        public bool IsSideBarVisible { get; private set; }

        /// <summary>
        /// 0 is sidebar visible
        /// 1 is sidebar collapsed
        /// 0.5 is sidebar at mid course
        /// </summary>
        public double Scrolling
        {
            get
            {
                try
                {
                    return (double)this.GetValue(ScrollingProperty);
                }
                catch
                {
                    return 1;
                }
            }
            set { SetValue(ScrollingProperty, value); }
        }

        public static readonly DependencyProperty ScrollingProperty = DependencyProperty.Register(
            "Scrolling", typeof(double), typeof(PaneView), new PropertyMetadata(default(double)));

        #region SideBarContent Property

        public DependencyObject SideBarContent
        {
            get { return (DependencyObject)GetValue(SideBarContentProperty); }
            set { SetValue(SideBarContentProperty, value); }
        }

        public static readonly DependencyProperty SideBarContentProperty = DependencyProperty.Register(
            "SideBarContent", typeof(DependencyObject), typeof(PaneView),
            new PropertyMetadata(default(DependencyObject), SideBarContentPropertyChangedCallback));

        private static void SideBarContentPropertyChangedCallback(DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var that = (PaneView)dependencyObject;
            if (that.sidebarGrid != null)
            {
                that.sidebarGrid.Content = dependencyPropertyChangedEventArgs.NewValue;
            }
        }

        #endregion

        #region VelocityThreshold Property

        public double VelocityThreshold
        {
            get { return (double)GetValue(VelocityThresholdProperty); }
            set { SetValue(VelocityThresholdProperty, value); }
        }

        public static readonly DependencyProperty VelocityThresholdProperty = DependencyProperty.Register(
            "VelocityThreshold", typeof(double), typeof(PaneView), new PropertyMetadata(default(double)));

        #endregion

        #region OpenRateThreshold Property

        public double OpenRateThreshold
        {
            get { return (double)GetValue(OpenRateThresholdProperty); }
            set { SetValue(OpenRateThresholdProperty, value); }
        }

        private double OpenThreshold
        {
            get { return sidebarGrid.ActualWidth * (OpenRateThreshold - 1d); }
        }

        public static readonly DependencyProperty OpenRateThresholdProperty = DependencyProperty.Register(
            "OpenRateThreshold", typeof(double), typeof(PaneView), new PropertyMetadata(default(double)));

        #endregion

        #region CloseRateThreshold Property

        public double CloseRateThreshold
        {
            get { return (double)GetValue(CloseRateThresholdProperty); }
            set { SetValue(CloseRateThresholdProperty, value); }
        }

        private double CloseThreshold
        {
            get { return sidebarGrid.ActualWidth * (-CloseRateThreshold); }
        }

        public static readonly DependencyProperty CloseRateThresholdProperty = DependencyProperty.Register(
            "CloseRateThreshold", typeof(double), typeof(PaneView), new PropertyMetadata(default(double)));

        #endregion

        #region SideBareWidth Property

        public double SideBareWidth
        {
            get { return (double)GetValue(SideBareWidthProperty); }
            set { SetValue(SideBareWidthProperty, value); }
        }

        public static readonly DependencyProperty SideBareWidthProperty = DependencyProperty.Register(
            "SideBareWidth", typeof(double), typeof(PaneView), new PropertyMetadata(default(double)));

        #endregion

        public static readonly DependencyProperty IsPanOpenProperty = DependencyProperty.Register(
            "IsPanOpen", typeof(bool), typeof(PaneView), new PropertyMetadata(default(bool), new PropertyChangedCallback(IsPanOpenPropertyChanged)));

        private static void IsPanOpenPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var that = (PaneView)d;
            if (that.IsPanOpen)
                that.ShowSidebar();
            else
                that.HideSidebar();
        }

        public bool IsPanOpen
        {
            get { return (bool)GetValue(IsPanOpenProperty); }
            set { SetValue(IsPanOpenProperty, value); }
        }

        public Frame MainFrame
        {
            get { return controlMainFrame; }
        }

        public EdgeUIThemeTransition MainFrameThemeTransition
        {
            get { return controlMainFrameThemeTransition; }
        }

        public PaneView()
        {
            DefaultStyleKey = typeof(PaneView);

            if (!DesignMode.DesignModeEnabled)
            {
                SystemNavigationManager.GetForCurrentView().BackRequested += SystemNavigationManager_BackRequested;
                this.Loaded += OnLoaded;
            }
        }

        protected override void OnApplyTemplate()
        {
            UnregisterManipulationEvents();

            base.OnApplyTemplate();

            sideTransform = GetTemplateChild(SideTransformName) as CompositeTransform;
            sidebarGrid = GetTemplateChild(SidebarGridName) as ContentPresenter;
            fadeOutSidebarGridAnimation = GetTemplateChild(FadeOutSidebarGridAnimationName) as DoubleAnimation;
            fadeInProperty = GetTemplateChild(FadeInPropertyName) as Storyboard;
            fadeOutProperty = GetTemplateChild(FadeOutPropertyName) as Storyboard;
            controlMainFrame = GetTemplateChild(ControlMainFrameName) as Frame;
            edgeGrid = GetTemplateChild(EdgeGridName) as Grid;
            controlMainFrameThemeTransition =
                GetTemplateChild(ControlMainFrameThemeTransitionName) as EdgeUIThemeTransition;

            if (sidebarGrid != null && SideBarContent != null)
            {
                sidebarGrid.Content = SideBarContent;
            }

            RegisterManipulationEvents();
            sidebarGrid.Loaded += (sender, args) => DisableTextBox();
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            this.Unloaded += OnUnloaded;
            this.SizeChanged += OnSizeChanged;
            Responsive();
        }

        private void OnUnloaded(object sender, RoutedEventArgs routedEventArgs)
        {
            this.SizeChanged -= OnSizeChanged;
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs sizeChangedEventArgs)
        {
            Responsive();
        }

        private void Responsive()
        {
            if (sideTransform != null)
            {
                sideTransform.TranslateX = -sidebarGrid.ActualWidth;
            }

            if (fadeOutSidebarGridAnimation != null)
            {
                fadeOutSidebarGridAnimation.To = -sidebarGrid.ActualWidth;
            }
        }

        private async void SystemNavigationManager_BackRequested(object sender, BackRequestedEventArgs e)
        {
            if (Scrolling > 0)
            {
                e.Handled = true;
                await Task.Delay(200);
                HideSidebar();
            }
        }

        public void ShowSidebar()
        {
            OnSideBarVisible();
        }

        public void HideSidebar()
        {
            OnSideBarCollapsed();
        }

        private void OnSideBarVisible()
        {
            fadeInProperty.Begin();

            Page page = controlMainFrame.Content as Page;
            if (page != null && page.BottomAppBar != null)
            {
                page.BottomAppBar.ClosedDisplayMode = AppBarClosedDisplayMode.Minimal;
            }
            Scrolling = 1;
            //IsPanOpen = true;
            controlMainFrame.IsEnabled = true;
            IsSideBarVisible = true;
            EnabledTextBox();
        }

        private void OnSideBarCollapsed()
        {
            fadeOutProperty.Begin();

            Page page = controlMainFrame.Content as Page;
            if (page != null && page.BottomAppBar != null)
            {
                page.BottomAppBar.ClosedDisplayMode = AppBarClosedDisplayMode.Compact;
            }
            Scrolling = 0;
            //IsPanOpen = false;
            controlMainFrame.IsEnabled = true;
            IsSideBarVisible = false;
            DisableTextBox();
        }

        /// <summary>
        /// Enabled textboxes can make keybord appear even if textbox is out of the screen/not visible
        /// Disabling them prevent this bug
        /// </summary>
        private void DisableTextBox()
        {
            List<TextBox> textBoxs = new List<TextBox>();
            Helpers.VisualTreeHelper.FindChildren(textBoxs, sidebarGrid);
            foreach (TextBox textBox in textBoxs)
            {
                textBox.IsEnabled = false;
            }
        }

        private void EnabledTextBox()
        {
            List<TextBox> textBoxs = new List<TextBox>();
            VisualTreeHelper.FindChildren(textBoxs, sidebarGrid);
            foreach (TextBox textBox in textBoxs)
            {
                textBox.IsEnabled = true;
            }
        }

        #region Manipulations Event Handlers

        private void RegisterManipulationEvents()
        {
            if (edgeGrid != null)
            {
                edgeGrid.ManipulationStarted += EdgeGrid_OnManipulationStarted;
                edgeGrid.ManipulationDelta += EdgeGrid_OnManipulationDelta;
                edgeGrid.ManipulationCompleted += EdgeGrid_OnManipulationCompleted;
            }

            if (sidebarGrid != null)
            {
                sidebarGrid.ManipulationStarted += Sidebar_OnManipulationStarted;
                sidebarGrid.ManipulationDelta += Sidebar_OnManipulationDelta;
                sidebarGrid.ManipulationCompleted += Sidebar_OnManipulationCompleted;
            }
        }

        private void UnregisterManipulationEvents()
        {
            if (edgeGrid != null)
            {
                edgeGrid.ManipulationStarted -= EdgeGrid_OnManipulationStarted;
                edgeGrid.ManipulationDelta -= EdgeGrid_OnManipulationDelta;
                edgeGrid.ManipulationCompleted -= EdgeGrid_OnManipulationCompleted;
            }

            if (sidebarGrid != null)
            {
                sidebarGrid.ManipulationStarted -= Sidebar_OnManipulationStarted;
                sidebarGrid.ManipulationDelta -= Sidebar_OnManipulationDelta;
                sidebarGrid.ManipulationCompleted -= Sidebar_OnManipulationCompleted;
            }
        }

        private void EdgeGrid_OnManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            if (sideTransform.TranslateX > OpenThreshold || e.Velocities.Linear.X > VelocityThreshold)
            {
                OnSideBarVisible();
            }
            else
            {
                OnSideBarCollapsed();
            }
        }

        private void EdgeGrid_OnManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
        }

        private void EdgeGrid_OnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            if (e.Cumulative.Translation.X < SideBareWidth - 10)
            {
                sideTransform.TranslateX += e.Delta.Translation.X;
                //MainFramePlaneProjection.GlobalOffsetZ -= e.Delta.Translation.X / 4;
            }
            Scrolling = (sidebarGrid.ActualWidth + sideTransform.TranslateX) / sidebarGrid.ActualWidth;
        }

        private void Sidebar_OnManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
        }

        private void Sidebar_OnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            if (e.Cumulative.Translation.X < 0)
            {
                //MainFramePlaneProjection.GlobalOffsetZ -= e.Delta.Translation.X / 4;
                sideTransform.TranslateX += e.Delta.Translation.X;
            }
            Scrolling = (sidebarGrid.ActualWidth + sideTransform.TranslateX) / sidebarGrid.ActualWidth;
        }

        private void Sidebar_OnManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            if (sideTransform.TranslateX < CloseThreshold || e.Velocities.Linear.X < -VelocityThreshold)
            {
                OnSideBarCollapsed();
            }
            else
            {
                OnSideBarVisible();
            }
        }

        #endregion
    }
}
