using System.Windows.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

// The Templated Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234235

namespace Geeky.UI
{
    [TemplatePart(Name = PART_ROOT_BORDER, Type = typeof(Border))]
    [TemplatePart(Name = PART_ROOT_GRID, Type = typeof(Grid))]
    [TemplatePart(Name = PART_BACK_BUTTON, Type = typeof(Button))]
    [TemplatePart(Name = PART_CONTENT, Type = typeof(ContentPresenter))]
    [TemplatePart(Name = PART_DIALOGGRID, Type = typeof(Grid))]
    public sealed class Dialog : ContentControl
    {
        private const string PART_ROOT_BORDER = "PART_RootBorder";
        private const string PART_ROOT_GRID = "PART_RootGrid";
        private const string PART_BACK_BUTTON = "PART_BackButton";
        private const string PART_CONTENT = "PART_Content";
        private const string PART_DIALOGGRID = "PART_DialogGrid";

        private Grid rootGrid;
        private Border rootBorder;
        private Button backButton;
        private Grid dialogGrid;
        private ContentPresenter contentPresenter;

        public event RoutedEventHandler BackButtonClicked;

        public Dialog()
        {
            this.DefaultStyleKey = typeof(Dialog);
        }

        protected override void OnApplyTemplate()
        {
            rootBorder = (Border) GetTemplateChild(PART_ROOT_BORDER);
            rootGrid = (Grid) GetTemplateChild(PART_ROOT_GRID);
            backButton = (Button) GetTemplateChild(PART_BACK_BUTTON);
            dialogGrid = (Grid) GetTemplateChild(PART_DIALOGGRID);
            contentPresenter = (ContentPresenter) GetTemplateChild(PART_CONTENT);

            BackButtonVisibility = Visibility.Visible;

            ResizeContainers();

            if (backButton != null)
                backButton.Click += BackButton_Click;

            Window.Current.SizeChanged += OnWindowSizeChanged;
            Unloaded += OnUnloaded;

            base.OnApplyTemplate();
        }

        private void ResizeContainers()
        {
            if (rootGrid != null)
            {
                rootGrid.Width = Window.Current.Bounds.Width;
                rootGrid.Height = Window.Current.Bounds.Height;
                dialogGrid.Margin = new Thickness(0,80,0,80);
                contentPresenter.Width = Window.Current.Bounds.Width - 160;
            }

            if (rootBorder != null)
            {
                rootBorder.Width = Window.Current.Bounds.Width;
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (BackButtonClicked != null)
                BackButtonClicked(sender, e);
            else
                IsOpen = false;
        }

        private void OnWindowSizeChanged(object sender, Windows.UI.Core.WindowSizeChangedEventArgs e)
        {
            ResizeContainers();
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            Unloaded -= OnUnloaded;
            Window.Current.SizeChanged -= OnWindowSizeChanged;
        }

        public static readonly DependencyProperty BackButtonVisibilityProperty = DependencyProperty.Register(
            "BackButtonVisibility", typeof (Visibility), typeof (Dialog), new PropertyMetadata(Visibility.Visible));

        public Visibility BackButtonVisibility
        {
            get { return (Visibility) GetValue(BackButtonVisibilityProperty); }
            set { SetValue(BackButtonVisibilityProperty, value); }
        }

        public static readonly DependencyProperty IsOpenProperty = DependencyProperty.Register(
            "IsOpen", typeof (bool), typeof (Dialog), new PropertyMetadata(false, OnIsOpenPropertyChanged));

        public bool IsOpen
        {
            get { return (bool) GetValue(IsOpenProperty); }
            set { SetValue(IsOpenProperty, value); }
        }

        private static void OnIsOpenPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(bool) e.NewValue) return;
            var dlg = d as Dialog;
            dlg?.ApplyTemplate();
        }


        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
            "Title", typeof (string), typeof (Dialog), null);

        public string Title
        {
            get { return (string) GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }


        public static readonly DependencyProperty BackButtonCommandProperty = DependencyProperty.Register(
            "BackButtonCommand", typeof (ICommand), typeof (Dialog), new PropertyMetadata(DependencyProperty.UnsetValue));

        public ICommand BackButtonCommand
        {
            get { return (ICommand) GetValue(BackButtonCommandProperty); }
            set { SetValue(BackButtonCommandProperty, value); }
        }


        public static readonly DependencyProperty BackButtonCommandParameterProperty = DependencyProperty.Register(
            "BackButtonCommandParameter", typeof (object), typeof (Dialog), new PropertyMetadata(DependencyProperty.UnsetValue));

        public object BackButtonCommandParameter
        {
            get { return (object) GetValue(BackButtonCommandParameterProperty); }
            set { SetValue(BackButtonCommandParameterProperty, value); }
        }
    }
}
