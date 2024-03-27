using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Build.AnalyzeRules;
using UnityEditor.AddressableAssets.Settings;

namespace Editor.AddressablesTools.CustomAnalyzeRules
{
  // Custom Rule For Analyzer
  public class SceneToAssetNoUnityBuildInAnalyzeRule : CheckSceneDupeDependencies
  {
    public override string ruleName => "Scene To Addressable No Unity Build In";

    public override List<AnalyzeResult> RefreshAnalysis(AddressableAssetSettings settings)
    {
      // result name is not a name, its a big string with a lot of information's
      List<AnalyzeResult> results = base.RefreshAnalysis(settings)
        .Where(x=>!x.resultName.Contains("unity_builtin_extra"))
        .ToList();
      
      return results;
    }
  }

  [InitializeOnLoad]
  class RegisterSceneToAssetNoUnityBuildInAnalyzeRule
  {
    static RegisterSceneToAssetNoUnityBuildInAnalyzeRule()
    {
      AnalyzeSystem.RegisterNewRule<SceneToAssetNoUnityBuildInAnalyzeRule>();
    }
  }
}