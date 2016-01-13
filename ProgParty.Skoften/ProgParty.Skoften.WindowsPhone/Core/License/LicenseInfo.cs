using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Store;
using Windows.Storage;

namespace ProgParty.Core.License
{
    public class LicenseInfo
    {
        public static LicenseInfo Instance = new LicenseInfo();

        public static void SetLicenseInformation()
        {
#if DEBUG
            Task.Run(() => LoadInAppPurchaseProxyFileAsync());
#endif
        }

        private static async Task LoadInAppPurchaseProxyFileAsync()
        {
            try
            {
                StorageFolder coreFolder = await Package.Current.InstalledLocation.GetFolderAsync("Core");
                StorageFolder licenseFolder = await coreFolder.GetFolderAsync("License");
                StorageFile proxyFile = await licenseFolder.GetFileAsync("WindowsStoreProxy.xml");
                await CurrentAppSimulator.ReloadSimulatorAsync(proxyFile);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message, e.InnerException);
            }
        }
    }
}