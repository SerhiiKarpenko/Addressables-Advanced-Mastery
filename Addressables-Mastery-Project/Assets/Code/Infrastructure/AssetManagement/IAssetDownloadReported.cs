using System;

namespace Code.Infrastructure.AssetManagement
{
    public interface IAssetDownloadReported : IProgress<float>
    {
        public float Progress { get; }
        public event Action ProgressUpdated;
        public void Report(float value);
        public void Reset();
    }
}