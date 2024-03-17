using Code.UI;
using UnityEngine;
using Zenject;

namespace Code.Infrastructure.Loading
{
  public class GameLoader : MonoBehaviour
  {
    private ISceneLoader _sceneLoader;
    private LoadingCurtain _loadingCurtain;

    [Inject]
    private void Construct(ISceneLoader sceneLoader, LoadingCurtain loadingCurtain)
    {
      _loadingCurtain = loadingCurtain;
      _sceneLoader = sceneLoader;
    }
    
    private void Start()
    {
      Initialize();
    }

    private void Initialize()
    {
      _loadingCurtain.Show();
      
      _sceneLoader.LoadScene(Scenes.Menu, () => _loadingCurtain.Hide());
    }
  }
}