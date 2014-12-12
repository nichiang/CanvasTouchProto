using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.ApplicationSettings;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Storage;
using CanvasTouchProto;

namespace SimpleSettings
{
    public sealed partial class SimpleSettingsNarrow : UserControl
    {
        ApplicationDataContainer appSettings = ApplicationData.Current.LocalSettings;
        
        public SimpleSettingsNarrow()
        {
            this.InitializeComponent();

            this.settingsDarkFishbowl.IsOn = (bool)appSettings.Values["sDarkFishbowl"];
            this.settingsPanSnapNon100.IsOn = (bool)appSettings.Values["sPanSnapNon100"];
            this.settingsEdgeSnap.IsOn = (bool)appSettings.Values["sEdgeSnap"];
            this.settingsResetZoom.IsOn = (bool)appSettings.Values["sResetZoomButton"];
            this.settingsResetPos.IsOn = (bool)appSettings.Values["sResetPanButton"];
            this.settingsInk.IsOn = (bool)appSettings.Values["sInk"];
            this.settingsDebug.IsOn = (bool)appSettings.Values["sDebugWindow"];
            
            this.settingsZoomMin.Value = (float)appSettings.Values["sZoomMin"];
            this.settingsZoomMax.Value = (float)appSettings.Values["sZoomMax"];

            this.settingsCanvasTop.IsChecked = false;
            this.settingsCanvasMiddle.IsChecked = false;

            switch ((MainPage.sCanvasAlign)appSettings.Values["sCanvasAlign"])
            {
                case MainPage.sCanvasAlign.top:
                    this.settingsCanvasTop.IsChecked = true;
                    break;
                case MainPage.sCanvasAlign.middle:
                    this.settingsCanvasMiddle.IsChecked = true;
                    break;
            }

            this.settingsPanSnapMandatory.IsChecked = false;
            this.settingsPanSnapOptional.IsChecked = false;
            this.settingsPanSnapOff.IsChecked = false;

            switch ((MainPage.sSnapEnum)appSettings.Values["sPanSnap"])
            {
                case MainPage.sSnapEnum.mandatory:
                    this.settingsPanSnapMandatory.IsChecked = true;
                    break;
                case MainPage.sSnapEnum.optional:
                    this.settingsPanSnapOptional.IsChecked = true;
                    break;
                case MainPage.sSnapEnum.off:
                    this.settingsPanSnapOff.IsChecked = true;
                    break;
            }

            this.settingsZoomSnapMandatory.IsChecked = false;
            this.settingsZoomSnapOptional.IsChecked = false;
            this.settingsZoomSnapOff.IsChecked = false;

            switch ((MainPage.sSnapEnum)appSettings.Values["sZoomSnap"])
            {
                case MainPage.sSnapEnum.mandatory:
                    this.settingsZoomSnapMandatory.IsChecked = true;
                    break;
                case MainPage.sSnapEnum.optional:
                    this.settingsZoomSnapOptional.IsChecked = true;
                    break;
                case MainPage.sSnapEnum.off:
                    this.settingsZoomSnapOff.IsChecked = true;
                    break;
            }

            this.settingsZoom100Only.IsChecked = false;
            this.settingsZoom50.IsChecked = false;
            this.settingsZoomFine.IsChecked = false;
            this.settingsZoomAll.IsChecked = false;
            this.settingsZoomOff.IsChecked = false;
            
            switch ((MainPage.sZoomEnum)appSettings.Values["sZoomIndicator"])
            {
                case MainPage.sZoomEnum.show100only:
                    this.settingsZoom100Only.IsChecked = true;
                    break;
                case MainPage.sZoomEnum.show50:
                    this.settingsZoom50.IsChecked = true;
                    break;
                case MainPage.sZoomEnum.showFine:
                    this.settingsZoomFine.IsChecked = true;
                    break;
                
                case MainPage.sZoomEnum.showAll:
                    this.settingsZoomAll.IsChecked = true;
                    break;
                case MainPage.sZoomEnum.off:
                    this.settingsZoomOff.IsChecked = true;
                    break;
            }
        }

        private void MySettingsBackClicked(object sender, RoutedEventArgs e)
        {
            if (this.Parent.GetType() == typeof(Popup))
            {
                ((Popup)this.Parent).IsOpen = false;
            }
            SettingsPane.Show();
        }

        private void UserControl_Unloaded_1(object sender, RoutedEventArgs e)
        {
            appSettings.Values["sDarkFishbowl"] = this.settingsDarkFishbowl.IsOn;
            appSettings.Values["sPanSnapNon100"] = this.settingsPanSnapNon100.IsOn;
            appSettings.Values["sEdgeSnap"] = this.settingsEdgeSnap.IsOn;
            appSettings.Values["sResetZoomButton"] = this.settingsResetZoom.IsOn;
            appSettings.Values["sResetPanButton"] = this.settingsResetPos.IsOn;
            appSettings.Values["sInk"] = this.settingsInk.IsOn;
            appSettings.Values["sDebugWindow"] = this.settingsDebug.IsOn;

            appSettings.Values["sZoomMin"] = (float)this.settingsZoomMin.Value;
            appSettings.Values["sZoomMax"] = (float)this.settingsZoomMax.Value;

            if (this.settingsCanvasTop.IsChecked.Value)
                appSettings.Values["sCanvasAlign"] = (int)MainPage.sCanvasAlign.top;
            else if (this.settingsCanvasMiddle.IsChecked.Value)
                appSettings.Values["sCanvasAlign"] = (int)MainPage.sCanvasAlign.middle;

            if (this.settingsPanSnapMandatory.IsChecked.Value)
                appSettings.Values["sPanSnap"] = (int)MainPage.sSnapEnum.mandatory;
            else if (this.settingsPanSnapOptional.IsChecked.Value)
                appSettings.Values["sPanSnap"] = (int)MainPage.sSnapEnum.optional;
            else if (this.settingsPanSnapOff.IsChecked.Value)
                appSettings.Values["sPanSnap"] = (int)MainPage.sSnapEnum.off;

            if (this.settingsZoomSnapMandatory.IsChecked.Value)
                appSettings.Values["sZoomSnap"] = (int)MainPage.sSnapEnum.mandatory;
            else if (this.settingsZoomSnapOptional.IsChecked.Value)
                appSettings.Values["sZoomSnap"] = (int)MainPage.sSnapEnum.optional;
            else if (this.settingsZoomSnapOff.IsChecked.Value)
                appSettings.Values["sZoomSnap"] = (int)MainPage.sSnapEnum.off;

            if (this.settingsZoom100Only.IsChecked.Value)
                appSettings.Values["sZoomIndicator"] = (int)MainPage.sZoomEnum.show100only;
            else if (this.settingsZoom50.IsChecked.Value)
                appSettings.Values["sZoomIndicator"] = (int)MainPage.sZoomEnum.show50;
            else if (this.settingsZoomFine.IsChecked.Value)
                appSettings.Values["sZoomIndicator"] = (int)MainPage.sZoomEnum.showFine;
            else if (this.settingsZoomAll.IsChecked.Value)
                appSettings.Values["sZoomIndicator"] = (int)MainPage.sZoomEnum.showAll;
            else if (this.settingsZoomOff.IsChecked.Value)
                appSettings.Values["sZoomIndicator"] = (int)MainPage.sZoomEnum.off;
        }
    }
}