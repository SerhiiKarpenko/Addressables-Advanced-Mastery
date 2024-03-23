using System;
using System.Collections.Generic;
using System.Linq;
using Code.Extensions;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using Zenject;

namespace Code.Infrastructure.AssetManagement
{
    public class AssetDownloadService : IAssetDownloadService
    {
        private List<IResourceLocator> _catalogLocators;
        private long _downloadSize;
        
        private readonly IAssetDownloadReported _assetDownloadReported;

        [Inject]
        public AssetDownloadService(IAssetDownloadReported assetDownloadReported)
        {
            _assetDownloadReported = assetDownloadReported;
        }
        
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

            await DownloadContentWithPreciseProgress(locations);
        }

        /// <summary>
        /// First version of downloading content and reporting download progress
        /// 
        /// Notice: problem here is our progress and unitask, because Addressables gives us PercentComplete
        /// But percent Complete is ALL PROGRESS: loading to memory etc. It's not necessary for us.  
        /// </summary>
        /// <param name="locations"></param>
        private async UniTask DownloadContent(IList<IResourceLocation> locations)
        {
            UniTask downloadTask = Addressables
                .DownloadDependenciesAsync(locations)
                .ToUniTask(_assetDownloadReported);

            await downloadTask;
            
            if (downloadTask.Status.IsFaulted())
            {
                Debug.LogError("Error while downloading catalog dependencies");
            }

            _assetDownloadReported.Reset();
        }
        
        /// <summary>
        /// First version of downloading content and reporting download progress,
        /// but now with precise progress.
        /// </summary>
        /// <param name="locations"></param>
        private async UniTask DownloadContentWithPreciseProgress(IList<IResourceLocation> locations)
        {
            AsyncOperationHandle downloadHandle = Addressables.DownloadDependenciesAsync(locations);
            
            while (!downloadHandle.IsDone && downloadHandle.IsValid())
            {
                await UniTask.Delay(100);
                _assetDownloadReported.Report(downloadHandle.GetDownloadStatus().Percent);
            }
            
            _assetDownloadReported.Report(1);
            
            if (downloadHandle.Status == AsyncOperationStatus.Failed)
            {
                Debug.LogError("Error while downloading catalog dependencies");
            }
            
            _assetDownloadReported.Reset();
        }

        /// <summary>
        /// Updated catalogs, if there is something to updated cache it and update.
        /// Find catalogs that needs to be updated and update them, then cache all updated catalogs. 
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