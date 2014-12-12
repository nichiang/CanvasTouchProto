using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Devices.Input;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.ApplicationSettings;
using Windows.UI.Input;
using Windows.UI.Input.Inking;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;
using Windows.Storage;
using SimpleSettings;


namespace CanvasTouchProto
{
    public sealed partial class MainPage : Page
    {
        float prevZoom = 1;
        int bottomExpand = 0;
        Rect _windowBounds;
        double _settingsWidth = 346;
        Popup _settingsPopup;

        uint _penID;
        Point _previousContactPt;

        public enum sCanvasAlign { top, middle };
        public enum sZoomEnum { show100only, show50, showFine, showAll, off };
        public enum sSnapEnum { mandatory, optional, off };
        SnapPointsType zoomSnapType = SnapPointsType.MandatorySingle;
        SnapPointsType panSnapType = SnapPointsType.OptionalSingle;

        ApplicationDataContainer appSettings = ApplicationData.Current.LocalSettings;
        InkManager inkManager = new Windows.UI.Input.Inking.InkManager();
        
        ImageBrush bg = new ImageBrush();
        SnapCanvas mainCanvas = new SnapCanvas();

        public MainPage()
        {
            this.InitializeComponent();

            // Defaults
            appSettings.Values["sDarkFishbowl"] = true;
            appSettings.Values["sCanvasAlign"] = (int)sCanvasAlign.middle;
            appSettings.Values["sPanSnap"] = (int)sSnapEnum.optional;
            appSettings.Values["sPanSnapNon100"] = true;
            appSettings.Values["sEdgeSnap"] = false;
            appSettings.Values["sZoomSnap"] = (int)sSnapEnum.mandatory;
            appSettings.Values["sResetZoomButton"] = false;
            appSettings.Values["sResetPanButton"] = false;
            appSettings.Values["sZoomIndicator"] = (int)sZoomEnum.show100only;
            appSettings.Values["sInk"] = true;
            appSettings.Values["sDebugWindow"] = false;
            appSettings.Values["sZoomMin"] = 0.25f;
            appSettings.Values["sZoomMax"] = 4.0f;

            _windowBounds = Window.Current.Bounds;
            Window.Current.SizeChanged += OnWindowSizeChanged;
            SettingsPane.GetForCurrentView().CommandsRequested += MainPage_CommandsRequested;

            Uri imageBg = new Uri("ms-appx:/Assets/canvas.png");
            bg.ImageSource = new BitmapImage(imageBg);
            bg.Stretch = Stretch.None;
            bg.AlignmentY = AlignmentY.Top;
            bg.AlignmentX = AlignmentX.Left;

            mainCanvas.Width = 10622;
            mainCanvas.Height = Window.Current.Bounds.Height - 84;
            mainCanvas.MaxHeight = 5000;
            mainCanvas.Background = bg;
            mainCanvas.VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Center;

            mainCanvas.PointerPressed += new PointerEventHandler(InkCanvas_PointerPressed);
            mainCanvas.PointerMoved += new PointerEventHandler(InkCanvas_PointerMoved);
            mainCanvas.PointerReleased += new PointerEventHandler(InkCanvas_PointerReleased);
            mainCanvas.PointerExited += new PointerEventHandler(InkCanvas_PointerReleased);

            this.scrollViewer.Content = mainCanvas;

            applySettings();
        }

        void OnWindowSizeChanged(object sender, Windows.UI.Core.WindowSizeChangedEventArgs e)
        {
            _windowBounds = Window.Current.Bounds;
        }

        void MainPage_CommandsRequested(SettingsPane sender, SettingsPaneCommandsRequestedEventArgs args)
        {
            SettingsCommand cmd = new SettingsCommand("sample", "Options", (x) =>
            {
                _settingsPopup = new Popup();
                _settingsPopup.Closed += OnPopupClosed;
                Window.Current.Activated += OnWindowActivated;
                _settingsPopup.IsLightDismissEnabled = true;
                _settingsPopup.Width = _settingsWidth;
                _settingsPopup.Height = _windowBounds.Height;
                
                SimpleSettingsNarrow mypane = new SimpleSettingsNarrow();
                mypane.Width = _settingsWidth;
                mypane.Height = _windowBounds.Height;

                _settingsPopup.Child = mypane;
                _settingsPopup.SetValue(Canvas.LeftProperty, _windowBounds.Width - _settingsWidth);
                _settingsPopup.SetValue(Canvas.TopProperty, 0);
                _settingsPopup.IsOpen = true;
            });

            args.Request.ApplicationCommands.Add(cmd);
        }

        private void OnWindowActivated(object sender, Windows.UI.Core.WindowActivatedEventArgs e)
        {
            if (e.WindowActivationState == Windows.UI.Core.CoreWindowActivationState.Deactivated)
            {
                _settingsPopup.IsOpen = false;
            }
        }

        void OnPopupClosed(object sender, object e)
        {
            Window.Current.Activated -= OnWindowActivated;
            applySettings();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
        }

        public void InkCanvas_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            PointerPoint pt = e.GetCurrentPoint((Canvas)sender);
            _previousContactPt = pt.Position;

            PointerDeviceType pointerDevType = e.Pointer.PointerDeviceType;
            if (pointerDevType == PointerDeviceType.Pen || pointerDevType == PointerDeviceType.Mouse && pt.Properties.IsLeftButtonPressed)
            {
                inkManager.ProcessPointerDown(pt);
                _penID = pt.PointerId;

                e.Handled = true;
            }
        }

        public void InkCanvas_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (e.Pointer.PointerId == _penID)
            {
                PointerPoint pt = e.GetCurrentPoint((Canvas)sender);

                Point currentContactPt = pt.Position;
                if (Math.Sqrt(Math.Pow(currentContactPt.X - _previousContactPt.X, 2) + Math.Pow(currentContactPt.Y - _previousContactPt.Y, 2)) > 2)
                {
                    Line line = new Line()
                    {
                        X1 = _previousContactPt.X,
                        Y1 = _previousContactPt.Y,
                        X2 = currentContactPt.X,
                        Y2 = currentContactPt.Y,
                        StrokeThickness = 2,
                        Stroke = new SolidColorBrush(Windows.UI.Colors.Black)
                    };

                    _previousContactPt = currentContactPt;

                    ((Canvas)sender).Children.Add(line);

                    inkManager.ProcessPointerUpdate(pt);
                }
            }

            e.Handled = true;
        }

        public void InkCanvas_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            if (e.Pointer.PointerId == _penID)
            {
                PointerPoint pt = e.GetCurrentPoint((Canvas)sender);

                inkManager.ProcessPointerUp(pt);
            }

            _penID = 0;

            e.Handled = true;
        }

        private void bResetZoom_Click(object sender, RoutedEventArgs e)
        {
            double hOffset = (this.scrollViewer.HorizontalOffset + (this.ActualWidth / 2)) / this.scrollViewer.ZoomFactor;
            double vOffset = (this.scrollViewer.VerticalOffset + (this.ActualHeight / 2)) / this.scrollViewer.ZoomFactor;
            
            this.scrollViewer.ZoomToFactor(1);

            this.scrollViewer.ScrollToHorizontalOffset(hOffset - (this.ActualWidth / 2));
            this.scrollViewer.ScrollToVerticalOffset(vOffset - (this.ActualHeight / 2));
        }

        private void bResetPos_Click(object sender, RoutedEventArgs e)
        {
            this.scrollViewer.ZoomToFactor(1);

            this.scrollViewer.ScrollToHorizontalOffset(0);
            this.scrollViewer.ScrollToVerticalOffset(0);
        }

        private void scrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            updateZoomVisuals();

            if (this.scrollViewer.ZoomFactor != 1 && !(bool)appSettings.Values["sPanSnapNon100"])
            {
                this.scrollViewer.HorizontalSnapPointsType = SnapPointsType.None;
                this.scrollViewer.VerticalSnapPointsType = SnapPointsType.None;
            }
            else
            {
                this.scrollViewer.HorizontalSnapPointsType = panSnapType;
                this.scrollViewer.VerticalSnapPointsType = panSnapType;
            }
            
            if (!e.IsIntermediate)
            {
                this.tZoom.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                
                if (this.scrollViewer.ZoomFactor != prevZoom)
                    this.mainCanvas.Margin = new Thickness(0);

                setPanSnaps();

                prevZoom = this.scrollViewer.ZoomFactor;
            }
        }

        private void setPanSnaps()
        {
            mainCanvas.updateSnapPoints(this.scrollViewer.ZoomFactor);
        }

        private void setZoomSnaps()
        {
            this.scrollViewer.ZoomSnapPoints.Clear();
            
            if ((sZoomEnum)appSettings.Values["sZoomIndicator"] == sZoomEnum.show100only)
            {
                for (float i = (float)appSettings.Values["sZoomMin"]; i < 0.6f; i += 0.05f)
                    this.scrollViewer.ZoomSnapPoints.Add(i);
                
                this.scrollViewer.ZoomSnapPoints.Add(1.0f);

                for (float i = 1.5f; i <= (float)appSettings.Values["sZoomMax"]; i += 0.05f)
                    this.scrollViewer.ZoomSnapPoints.Add(i);
            }
            else if ((sZoomEnum)appSettings.Values["sZoomIndicator"] == sZoomEnum.show50)
            {
                for (float i = (float)appSettings.Values["sZoomMin"]; i <= (float)appSettings.Values["sZoomMax"]; i += 0.5f)
                    this.scrollViewer.ZoomSnapPoints.Add(i);
            }
            else if ((sZoomEnum)appSettings.Values["sZoomIndicator"] == sZoomEnum.showFine)
            {
                for (float i = (float)appSettings.Values["sZoomMin"]; i <= (float)appSettings.Values["sZoomMax"]; i += 0.25f)
                    this.scrollViewer.ZoomSnapPoints.Add(i);
            }
        }

        private void updateZoomVisuals()
        {
            if (this.scrollViewer.ZoomFactor != 1 && (bool)appSettings.Values["sResetZoomButton"])
                this.bZoom.Visibility = Windows.UI.Xaml.Visibility.Visible;
            else
                this.bZoom.Visibility = Windows.UI.Xaml.Visibility.Collapsed;

            if (this.scrollViewer.ZoomFactor != 1 && (bool)appSettings.Values["sResetPanButton"])
                this.bResetPos.Visibility = Windows.UI.Xaml.Visibility.Visible;
            else
                this.bResetPos.Visibility = Windows.UI.Xaml.Visibility.Collapsed;

            if (this.scrollViewer.ZoomFactor != prevZoom && (sZoomEnum)appSettings.Values["sZoomIndicator"] != sZoomEnum.off)
                this.tZoom.Visibility = Windows.UI.Xaml.Visibility.Visible;
        }

        private void applySettings()
        {
            this.scrollViewer.MinZoomFactor = (float)appSettings.Values["sZoomMin"];
            this.scrollViewer.MaxZoomFactor = (float)appSettings.Values["sZoomMax"];
            
            if ((bool)appSettings.Values["sDarkFishbowl"])
                this.scrollViewer.Background = new SolidColorBrush(Windows.UI.Colors.DarkGray);
            else
                this.scrollViewer.Background = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 213, 213, 213));
            
            if ((sCanvasAlign)appSettings.Values["sCanvasAlign"] == sCanvasAlign.top)
                this.mainCanvas.VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Top;
            else if ((sCanvasAlign)appSettings.Values["sCanvasAlign"] == sCanvasAlign.middle)
                this.mainCanvas.VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Center;
            
            if ((sSnapEnum)appSettings.Values["sPanSnap"] == sSnapEnum.mandatory)
                panSnapType = SnapPointsType.MandatorySingle;
            else if ((sSnapEnum)appSettings.Values["sPanSnap"] == sSnapEnum.optional)
                panSnapType = SnapPointsType.OptionalSingle;
            else
                panSnapType = SnapPointsType.None;

            this.scrollViewer.HorizontalSnapPointsType = panSnapType;
            this.scrollViewer.VerticalSnapPointsType = panSnapType;

            if ((sSnapEnum)appSettings.Values["sZoomSnap"] == sSnapEnum.mandatory)
                zoomSnapType = SnapPointsType.MandatorySingle;
            else if ((sSnapEnum)appSettings.Values["sZoomSnap"] == sSnapEnum.optional)
                zoomSnapType = SnapPointsType.OptionalSingle;
            else
                zoomSnapType = SnapPointsType.None;

            this.scrollViewer.ZoomSnapPointsType = zoomSnapType;
            
            if ((bool)appSettings.Values["sDebugWindow"])
                this.tDebug.Visibility = Windows.UI.Xaml.Visibility.Visible;
            else
                this.tDebug.Visibility = Windows.UI.Xaml.Visibility.Collapsed;

            // Edge snap disabling hack --------------------------------------------------
            
            if (!(bool)appSettings.Values["sEdgeSnap"])
            {
                this.bg.AlignmentX = AlignmentX.Left;
                this.bg.AlignmentY = AlignmentY.Top;

                this.mainCanvas.Width = 10622;
                this.mainCanvas.Height = 996;
            }
            else
            {
                this.bg.AlignmentX = AlignmentX.Right;
                this.bg.AlignmentY = AlignmentY.Bottom;

                this.mainCanvas.Width = 10622 + Window.Current.Bounds.Width * 1.9;
                this.mainCanvas.Height = 9004;

                this.scrollViewer.ZoomToFactor(1);
                this.scrollViewer.ScrollToHorizontalOffset(Window.Current.Bounds.Width * 1.9);
                this.scrollViewer.ScrollToVerticalOffset(Window.Current.Bounds.Height * 1.9);
            }
            
            setZoomSnaps();
            updateZoomVisuals();
            setPanSnaps();
        }

        private void bAddMoreSpace_Click(object sender, RoutedEventArgs e)
        {
            int expansion = (int)((Window.Current.Bounds.Height - 84) / 4);
            bottomExpand += expansion;

            // expanded canvas smaller than scrollViewer height
            if ((this.mainCanvas.Height + expansion * 2) * this.scrollViewer.ZoomFactor < this.scrollViewer.Height)
            {
                this.mainCanvas.Margin = new Thickness(0, bottomExpand, 0, 0);
            }
            // expanded canvas taller than scrollViewer height, existing canvas smaller than scrollViewer height
            else if ((this.mainCanvas.Height + expansion * 2) * this.scrollViewer.ZoomFactor > this.scrollViewer.Height
                && this.mainCanvas.Height * this.scrollViewer.ZoomFactor < this.scrollViewer.Height)
            {
                this.mainCanvas.Margin = new Thickness(0, (this.scrollViewer.Height - this.mainCanvas.Height) / 2, 0, 0);
            }
            // existing canvas taller than scrollViewer height
            else
            {

            }

            this.mainCanvas.Height = Window.Current.Bounds.Height - 84 + bottomExpand;
        }

        private void bResetSpace_Click(object sender, RoutedEventArgs e)
        {
            this.mainCanvas.Children.Clear();
            this.mainCanvas.Height = Window.Current.Bounds.Height - 84;
            this.mainCanvas.Margin = new Thickness(0);
            bottomExpand = 0;
        }
    }

    public class SnapCanvas : Canvas, IScrollSnapPointsInfo 
    {
        public event EventHandler<object> HorizontalSnapPointsChanged;
        public event EventHandler<object> VerticalSnapPointsChanged;

        float horSnapPoint = (float)(Window.Current.Bounds.Width * 0.95);
        float verSnapPoint = (float)((Window.Current.Bounds.Height - 84) * 0.95);
        
        public SnapCanvas()
        {
        }

        public bool AreHorizontalSnapPointsRegular 
        { 
            get { return true; } 
        }

        public bool AreVerticalSnapPointsRegular 
        {
            get { return true; }
        }

        public IReadOnlyList<float> GetIrregularSnapPoints(Orientation orientation, SnapPointsAlignment alignment)
        {
            return null;
        }

        public float GetRegularSnapPoints(Orientation orientation, SnapPointsAlignment alignment, out float offset)
        {
            offset = 0;
            
            if (orientation == Orientation.Horizontal)
                return horSnapPoint;
            else
                return verSnapPoint;
        }

        protected virtual void OnHorizontalSnapPointsChanged(EventArgs e)
        {
            if (HorizontalSnapPointsChanged != null)
                HorizontalSnapPointsChanged(this, e);
        }

        protected virtual void OnVerticalSnapPointsChanged(EventArgs e)
        {
            if (VerticalSnapPointsChanged != null)
                VerticalSnapPointsChanged(this, e);
        }

        public void updateSnapPoints(float zoomFactor)
        {
            horSnapPoint = (float)(Window.Current.Bounds.Width * 0.95 / zoomFactor);
            verSnapPoint = (float)((Window.Current.Bounds.Height - 84) * 0.95 / zoomFactor);

            OnHorizontalSnapPointsChanged(EventArgs.Empty);
            OnVerticalSnapPointsChanged(EventArgs.Empty);
        }
    }

    public class PercentConverter : IValueConverter
    {
        public object Convert(object value, System.Type type, object parameter, string language)
        {
            float _value = (float)value;
            string _zoom = string.Empty;
            float zoomFactor = 1;
            
            ApplicationDataContainer appSettings = ApplicationData.Current.LocalSettings;

            if ((MainPage.sZoomEnum)appSettings.Values["sZoomIndicator"] == MainPage.sZoomEnum.show100only)
            {
                zoomFactor = (float)Math.Round(_value * 10) / 10;

                if (zoomFactor > 0.8 && zoomFactor < 1.2)
                    return "100%";
                else if (zoomFactor <= (float)appSettings.Values["sZoomMin"])
                    return "Min Zoom";
                else if (zoomFactor >= (float)appSettings.Values["sZoomMax"])
                    return "Max Zoom";
                else
                    return "";
            }
            else if ((MainPage.sZoomEnum)appSettings.Values["sZoomIndicator"] == MainPage.sZoomEnum.show50)
                zoomFactor = (float)Math.Round(_value * 2) / 2;
            else if ((MainPage.sZoomEnum)appSettings.Values["sZoomIndicator"] == MainPage.sZoomEnum.showFine)
                zoomFactor = (float)Math.Round(_value * 4) / 4;
            else
                zoomFactor = _value;

            if (zoomFactor == 1)
                _zoom = ">>> 100% <<<";
            else
                _zoom = string.Format("{0}%", Math.Round(zoomFactor * 100));

            return _zoom;
        }

        public object ConvertBack(object value, System.Type type, object parameter, string language)
        {
            throw new NotImplementedException(); //doing one-way binding so this is not required.
        }
    }
}
