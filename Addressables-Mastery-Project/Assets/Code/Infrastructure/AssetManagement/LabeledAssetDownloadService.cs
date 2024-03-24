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
    /// <summary>
    /// This class is the another version of Asset Download Service.
    /// NOTICE: WE DONT NEED 2 OF THEM, IT'S JUST AN ANOTHER VERSION
    /// 
    /// It's a rare solution for this service, because it's a solution for bad situation in our bundles.
    /// When there a LOT of assets and a LOT of bundles it can be hard to download them because of their dependencies.
    /// Downloading process will take a lot of time even when we have fast internet and problem is not internet, but dependencies resolving.
    ///
    ///  
    /// </summary>
    public class LabeledAssetDownloadService : IAssetDownloadService
    {
        public const string RemoteLabel = "remote";
        
        private long _downloadSize;
        
        private readonly IAssetDownloadReported _assetDownloadReported;

        [Inject]
        public LabeledAssetDownloadService(IAssetDownloadReported assetDownloadReported)
        {
            _assetDownloadReported = assetDownloadReported;
        }
        
        public async UniTask InitializeDownloadDataAsync()
        {
            await Addressables.InitializeAsync().ToUniTask();
            await UpdateCatalogsAsync();
            await UpdateDownloadSizeAsync();
        }

        public float GetDownloadSizeMb() =>
            SizeToMb(_downloadSize);

        public async UniTask UpdateContentAsync()
        {
            try
            {
                AsyncOperationHandle downloadHandle = Addressables.DownloadDependenciesAsync(RemoteLabel);
            
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

                if (downloadHandle.IsValid())
                {
                    Addressables.Release(downloadHandle);
                }
            
                _assetDownloadReported.Reset();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }

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
                return;
            }

            await Addressables.UpdateCatalogs(catalogsToUpdate).ToUniTask();
        }

        private async UniTask UpdateDownloadSizeAsync()
        {
            _downloadSize = await Addressables
                .GetDownloadSizeAsync(RemoteLabel)
                .ToUniTask();
        }

        private float SizeToMb(long size) => 
            size * 1f / 1048576;
    }
}