using Cysharp.Threading.Tasks;

namespace Code.Infrastructure.AssetManagement
{
    public interface IAssetDownloadService
    {
        public UniTask InitializeDownloadDataAsync();
        public float GetDownloadSizeMb();
        public UniTask UpdateContentAsync();
    }
}