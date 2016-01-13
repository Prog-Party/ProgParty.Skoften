using ProgParty.Skoften.Api.Parameter;
using ProgParty.Skoften.Api.Result;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Popups;
using ProgParty.Core;
using Windows.ApplicationModel.Store;
using System;
using System.Collections.Generic;
using ProgParty.Core.Pages;


// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace ProgParty.Skoften
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public SkoftenDataContext SkoftenDataContext { get; set; }
        public Pivot Pivot => searchPivot;
        
        CancellationTokenSource source = new CancellationTokenSource();
        SynchronizationContext context = SynchronizationContext.Current;

        public MainPage()
        {
            InitializeComponent();
            NavigationCacheMode = NavigationCacheMode.Required;

            Config.Instance = new Config(this)
            {
                Pivot = searchPivot,
                AppName = "Skoften",
                Ad = new ConfigAd()
                {
                    AdHolder = AdHolder,
                    AdApplicationId = "9ca95aaf-bd97-401d-9724-a9c75de6ed0e",
                    SmallAdUnitId = "11556545",
                    MediumAdUnitId = "11556546",
                    LargeAdUnitId = "11556547"
                }
            };
#if DEBUG
            Core.Config.Instance.LicenseInformation = CurrentAppSimulator.LicenseInformation;
#else
            Core.Config.Instance.LicenseInformation = CurrentApp.LicenseInformation;
#endif
            Core.License.LicenseInfo.SetLicenseInformation();

            Register.Execute();

            SkoftenDataContext = new SkoftenDataContext();
            DataContext = SkoftenDataContext;

            ShowPicGif();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Register.RegisterOnNavigatedTo(App.LicenseInformation);
        }

        private void LayoutRoot_Loaded(object sender, RoutedEventArgs e)
        {
            Register.RegisterOnLoaded();
        }

        private async void galleryList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedItem = e.AddedItems.FirstOrDefault() as OverviewResult;
            if (selectedItem == null)
                return;

            SkoftenDataContext.SelectedGallery = selectedItem.Index;
            
            if (SkoftenDataContext.NeedGalleryScrape())
                await Task.Factory.StartNew(() => Searcher.ExecuteGaleryScrape(this, context), source.Token);

            await Task.Factory.StartNew(() => Searcher.ExecuteSingleDumpScrape(this, context, selectedItem), source.Token);

            if (SkoftenDataContext.CurrentGalleryType == Api.Parameter.OverviewType.PicGif)
                Core.Track.Telemetry.Instance.Action("Shown pic/gif");
            else if (SkoftenDataContext.CurrentGalleryType == Api.Parameter.OverviewType.EroDump)
                Core.Track.Telemetry.Instance.Action("Shown erodump");
        }

        private void LoadMoreObjects_Click(object sender, RoutedEventArgs e)
        {
            SkoftenDataContext.ShowNextDumpItems();
        }

        private async void LoadMoreGalleries_Click(object sender, RoutedEventArgs e)
        {
            await Task.Factory.StartNew(() => Searcher.ExecuteGaleryScrape(this, context), source.Token);
        }

        private async void LoadPreviousGallery_Click(object sender, RoutedEventArgs e)
        {
            if (SkoftenDataContext.NeedGalleryScrape())
                await Task.Factory.StartNew(() => Searcher.ExecuteGaleryScrape(this, context), source.Token);

            var gallery = SkoftenDataContext.GetNextGallery();

            if(gallery == null)
            {
                //should not be possible, because gallery's are loaded async before the last gallery is reached.
                new MessageDialog("Geen gallerij aanwezig, laad er meer.").ShowAsync();
                Pivot.SelectedIndex = 0;
            }
            else
            {
                await Task.Factory.StartNew(() => Searcher.ExecuteSingleDumpScrape(this, context, gallery), source.Token);
            }
        }

        private void Grid_Holding(object sender, HoldingRoutedEventArgs e)
        {
            FrameworkElement senderElement = sender as FrameworkElement;
            FlyoutBase flyoutBase = FlyoutBase.GetAttachedFlyout(senderElement);
            flyoutBase.ShowAt(senderElement);
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            var save = new Core.Image.SaveImage();
            string imageUrl = ((sender as MenuFlyoutItem).DataContext as DumpResult).Url;
            save.RegisterForSave(imageUrl);
        }

        private void ShareButton_Click(object sender, RoutedEventArgs e)
        {
            var share = new Core.Image.ShareImage();
            string imageUrl = ((sender as MenuFlyoutItem).DataContext as DumpResult).Url;
            share.RegisterForShare(imageUrl);
        }

        private void ContactButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(Contact));
        }

        private void BuyBarButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(Shop));
        }

        private async void ComboBoxMenu_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = e.AddedItems.First() as ComboBoxItem;
            if (item.Name == "Ero")
            {
                if (!App.LicenseInformation.ProductLicenses[InAppPurchase.TokenPurchaseCustom].IsActive)
                {
                    Core.Track.Telemetry.Instance.Action("Erodump no license");
#if DEBUG
                    await CurrentAppSimulator.RequestProductPurchaseAsync(InAppPurchase.TokenPurchaseCustom);
#else
                    await CurrentApp.RequestProductPurchaseAsync(InAppPurchase.TokenPurchaseCustom);
#endif
                }

                TryShowErodump();
            } else if (item.Name == "PicGif")
            {
                if (DoNotLoadAfterChange)
                    DoNotLoadAfterChange = false;
                else
                    ShowPicGif();
            }
        }

        bool DoNotLoadAfterChange = true;

        private async void ShowPicGif()
        {
            Core.Track.Telemetry.Instance.Action("Filter", new Dictionary<string, string>() { { "filter", "Pic/gif" } });
            this.SkoftenDataContext.CurrentGalleryType = Api.Parameter.OverviewType.PicGif;
            await Task.Factory.StartNew(() => Searcher.ExecuteGaleryScrape(this, context, new Api.Parameter.OverviewParameter() { StartOver = true }), source.Token);
        }

        private async void ShowEro()
        {
            Core.Track.Telemetry.Instance.Action("Filter", new Dictionary<string, string>() { { "filter", "Ero" } } );
            this.SkoftenDataContext.CurrentGalleryType = Api.Parameter.OverviewType.EroDump;
            await Task.Factory.StartNew(() => Searcher.ExecuteGaleryScrape(this, context, new Api.Parameter.OverviewParameter() { StartOver = true, Type = Api.Parameter.OverviewType.EroDump }), source.Token);
        }

        private void TryShowErodump()
        {
            if (App.LicenseInformation.ProductLicenses[InAppPurchase.TokenPurchaseCustom].IsActive)
            {
                ShowEro();
            } else { 
                DoNotLoadAfterChange = true;
                ComboBoxMenu.SelectedIndex = 0;
            }
        }
    }
}
