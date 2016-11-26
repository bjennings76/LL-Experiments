using System.IO;
using UnityEditor;

public static class CreateAssetBundles {
  [MenuItem("Assets/Build AssetBundles")]
  private static void BuildAssetBundles() {
    CheckDirectory("Assets/AssetBundles/Windows");
    CheckDirectory("Assets/AssetBundles/WebGL");

    BuildPipeline.BuildAssetBundles("Assets/AssetBundles/Windows", BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows64);
    BuildPipeline.BuildAssetBundles("Assets/AssetBundles/WebGL", BuildAssetBundleOptions.None, BuildTarget.WebGL);
  }

  private static void CheckDirectory(string dir) {
    if (!Directory.Exists(dir)) {
      Directory.CreateDirectory(dir);
    }
  }
}