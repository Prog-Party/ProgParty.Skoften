using ProgParty.Skoften.Api.Execute;
using ProgParty.Skoften.Api.Result;
using System.Linq;
using System.Threading;
using Windows.Networking;

namespace ProgParty.Skoften
{
    public class Searcher
    {
        public static void ExecuteSingleDumpScrape(MainPage mainpage, SynchronizationContext theContext, OverviewResult search)
        {
            theContext.Post((_) =>
            {
                mainpage.Pivot.SelectedIndex = 1;
                mainpage.SkoftenDataContext.DumpItemsLoading = true;
                mainpage.SkoftenDataContext.DumpItems.Clear();
            }, null);

            DumpExecute execute = new DumpExecute();
            execute.Parameters.Url = search.Url;
            execute.Parameters.Type = mainpage.SkoftenDataContext.CurrentGalleryType;
            execute.Execute();
            var dumpItems = execute.Result;

            theContext.Post((_) =>
            {
                mainpage.SkoftenDataContext.InitializeNewDumpList(dumpItems);
            }, null);
        }

        public static void ExecuteGaleryScrape(MainPage mainpage, SynchronizationContext context, Api.Parameter.OverviewParameter parameters = null)
        {
            if (parameters == null)
                parameters = new Api.Parameter.OverviewParameter();

            context.Post((_) =>
            {
                mainpage.SkoftenDataContext.GalleryItemsLoading = true;
            }, null);

            parameters.Type = mainpage.SkoftenDataContext.CurrentGalleryType;

            if (parameters.StartOver)
            {
                mainpage.SkoftenDataContext.GalleryPaging = 0;
                mainpage.SkoftenDataContext.GalleryItemIndex = 0;
                mainpage.SkoftenDataContext.SelectedGallery = 0;
            }


            parameters.Paging = mainpage.SkoftenDataContext.GalleryPaging;

            mainpage.SkoftenDataContext.GalleryPaging += 12;

            OverviewExecute oe = new OverviewExecute();
            oe.Parameters = parameters;
            oe.Execute();
            var result = oe.Result;


            context.Post((_) =>
            {
                if(parameters.StartOver)
                {
                    mainpage.SkoftenDataContext.Gallery.Clear();
                }

                foreach (var item in result)
                {
                    if(parameters.Type == Api.Parameter.OverviewType.EroDump)
                    {
                        if (!item.Url.Contains("babes.skoften.net"))
                        {
                            mainpage.SkoftenDataContext.GalleryItemIndex++;
                            continue;
                        }
                    }

                    item.Index = mainpage.SkoftenDataContext.GalleryItemIndex;
                    mainpage.SkoftenDataContext.Gallery.Add(item);
                    mainpage.SkoftenDataContext.GalleryItemIndex++;
                }

                mainpage.SkoftenDataContext.GalleryItemsLoading = false;
            }, null);
        }
    }
}
