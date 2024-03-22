using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Code.Extensions;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace Code.Infrastructure.AssetManagement
{
    public class AssetDownloadService 
    {
        private List<IResourceLocator> _catalogLocators;
        private long _downloadSize;

        public async UniTask InitializeDownloadDataAsync()
        {
            await Addressables.InitializeAsync();
            await UpdateCatalogsAsync();
            await UpdateDownloadSizeAsync();
        }

        public float GetDownloadSizeMb() =>
            SizeToMb(_downloadSize);

        public async UniTask UpdateContentAsync()
        {
            if (_catalogLocators == null)
            {
                await UpdateCatalogsAsync();
            }
            
            IList<IResourceLocation> locations = await RefreshResourceLocations(_catalogLocators);

            if (locations.IsNullOrEmpty())
            {
                return;
            }

            await DownloadContent(locations);
        }

        private async UniTask DownloadContent(IList<IResourceLocation> locations)
        {
            Addressables.DownloadDependenciesAsync(locations).ToUniTask();
        }

        /// <summary>
        /// Updated catalogs, if there is something to updated cache it and update. 
        /// </summary>
        private async UniTask UpdateCatalogsAsync()
        {
            List<string> catalogsToUpdate =  await Addressables.CheckForCatalogUpdates().ToUniTask();

            if (catalogsToUpdate.IsNullOrEmpty())
            {
                _catalogLocators = Addressables.ResourceLocators.ToList();
                return;
            }
            
            _catalogLocators = await Addressables.UpdateCatalogs(catalogsToUpdate).ToUniTask();

        }

        private async UniTask UpdateDownloadSizeAsync()
        {
            IList<IResourceLocation> locations = await RefreshResourceLocations(_catalogLocators);

            if (locations.IsNullOrEmpty())
            {
                return;
            }

            _downloadSize = await Addressables
                .GetDownloadSizeAsync(locations)
                .ToUniTask();
        }

        private async UniTask<IList<IResourceLocation>> RefreshResourceLocations(IEnumerable<IResourceLocator> locators)
        {
            IEnumerable<object> keysToCheck = locators.SelectMany(x => x.Keys);
            
            return await Addressables
                .LoadResourceLocationsAsync(keysToCheck, Addressables.MergeMode.Union)
                .ToUniTask();
        }

        private float SizeToMb(long size) => 
            size * 1f / 1048576;
    }
}