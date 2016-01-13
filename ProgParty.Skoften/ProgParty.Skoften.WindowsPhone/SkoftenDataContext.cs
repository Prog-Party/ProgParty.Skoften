using ProgParty.Skoften.Api.Result;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Windows.UI.Xaml;

namespace ProgParty.Skoften
{
    public class SkoftenDataContext : INotifyPropertyChanged
    {
        public ObservableCollection<OverviewResult> Gallery { get; set; } = new ObservableCollection<OverviewResult>();

        private bool _galleryItemsLoading = false;
        public bool GalleryItemsLoading
        {
            get { return _galleryItemsLoading; }
            set
            {
                _galleryItemsLoading = value;

                OnPropertyChanged("GalleryItemsLoadingVisibility");
            }
        }
        public Visibility GalleryItemsLoadingVisibility => GalleryItemsLoading ? Visibility.Visible : Visibility.Collapsed;

        public int GalleryItemIndex = 0;
        public int SelectedGallery = 0;

        public Api.Parameter.OverviewType CurrentGalleryType { get; set; } = Api.Parameter.OverviewType.PicGif;

        private bool _dumpItemsLoading = false;
        public bool DumpItemsLoading
        {
            get { return _dumpItemsLoading; }
            set
            {
                _dumpItemsLoading = value;

                OnPropertyChanged("DumpItemsLoadingVisibility");
                OnPropertyChanged("GifListResultsVisibility");
            }
        }

        public Visibility DumpItemsLoadingVisibility
        {
            get
            {
                if (CurrentGalleryIsGif)
                    return Visibility.Collapsed;
                return DumpItemsLoading ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public Visibility GifListResultsVisibility
        {
            get
            {
                if (!CurrentGalleryIsGif)
                    return Visibility.Collapsed;
                return DumpItemsLoading ? Visibility.Visible : Visibility.Collapsed;
            }
        }
        
        public Visibility MoreDumpItemsAvailableVisibility
        {
            get
            {
                if (DumpItemIndex == AllDumpItems.Count)
                    return Visibility.Collapsed;
                if (!CurrentGalleryIsGif && DumpItemIndex > DumpItems.Count)
                    return Visibility.Collapsed;

                return Visibility.Visible;
            }
        }

        public ObservableCollection<DumpResult> DumpItems { get; internal set; } = new ObservableCollection<DumpResult>();
        public ObservableCollection<DumpResult> DumpGifItems { get; internal set; } = new ObservableCollection<DumpResult>();

        public List<DumpResult> AllDumpItems { get; set; } = new List<DumpResult>();

        public int DumpItemIndex = 0;
        public int GalleryPaging = 0;
        public bool CurrentGalleryIsGif = false;

        public SkoftenDataContext()
        {
        }

        public void ShowNextDumpItems()
        {
            int count = 10;
            if (CurrentGalleryIsGif)
            { 
                count = 1;
                DumpGifItems.Clear();
            }
            var items = AllDumpItems.Skip(DumpItemIndex).Take(count);

            foreach (var dumpItem in items)
            {
                if (CurrentGalleryIsGif)
                    DumpGifItems.Add(dumpItem);
                else
                    DumpItems.Add(dumpItem);
                DumpItemIndex ++;
            }

            OnPropertyChanged("MoreDumpItemsAvailableVisibility");
        }

        internal OverviewResult GetNextGallery(int getNextGalleryCounter = 12)
        {
            SelectedGallery++;
            var galleryIndex = SelectedGallery;

            var galleryItem = Gallery.FirstOrDefault(g => g.Index == galleryIndex);
            if(galleryItem == null)
            {
                if (getNextGalleryCounter != 0)
                    return GetNextGallery(getNextGalleryCounter - 1);
            }

            return galleryItem;
        }

        internal bool NeedGalleryScrape()
        {
            var galleryIndex = SelectedGallery;
            if (galleryIndex > (GalleryItemIndex - 4))
            {
                return true;
            }

            return false;
        }

        internal void InitializeNewDumpList(List<DumpResult> dumpItems)
        {
            CurrentGalleryIsGif = false;
            if (dumpItems.Any() && dumpItems.First().IsGif)
                CurrentGalleryIsGif = true;

            DumpItemsLoading = false;
            AllDumpItems = dumpItems;
            DumpItemIndex = 0;
            DumpItems.Clear();
            DumpGifItems.Clear();
            ShowNextDumpItems();

        }

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        public event PropertyChangedEventHandler PropertyChanged;
    }
}