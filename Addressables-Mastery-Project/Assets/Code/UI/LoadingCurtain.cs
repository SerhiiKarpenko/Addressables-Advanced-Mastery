using System;
using Code.Infrastructure.AssetManagement;
using UnityEngine;
using Zenject;

namespace Code.UI
{
  public class LoadingCurtain : MonoBehaviour
  {
    public Canvas Canvas;
    public DownloadBar DownloadBar;
    private IAssetDownloadReported _downloadReported;

    [Inject]
    private void Construct(IAssetDownloadReported downloadReported)
    {
      _downloadReported = downloadReported;
    }

    private void Awake()
    {
      _downloadReported.ProgressUpdated += DisplayDownloadProgress;
    }

    public void Show()
    {
      Canvas.enabled = true;
    }

    public void Hide()
    {
      Canvas.enabled = false;
      DownloadBar.gameObject.SetActive(false);
    }

    private void DisplayDownloadProgress()
    {
      DownloadBar.gameObject.SetActive(true);
      DownloadBar.SetProgress(_downloadReported.Progress);
    }

    private void OnDestroy()
    {
      _downloadReported.ProgressUpdated -= DisplayDownloadProgress;
    }
  }
}