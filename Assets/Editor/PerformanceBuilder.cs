using UnityEditor.SceneManagement;
namespace SkyMeadow.Editor
{
    public static class PerformanceBuilder
    {
        public static void BuildOptimized()
        {
            EditorSceneManager.OpenScene("Assets/Scenes/Grassland.unity");
            RefinedModelInstaller.Apply();
            ApplySettings();
            EditorSceneManager.SaveOpenScenes();
            UnityEditor.AssetDatabase.SaveAssets();
            ProjectBuilder.BuildCurrentScene();
        }
        public static void ApplySettings()
        {
            UnityEditor.PlayerSettings.gpuSkinning=true;
            UnityEngine.QualitySettings.shadowDistance=65;
            UnityEngine.QualitySettings.shadowCascades=2;
            UnityEngine.QualitySettings.antiAliasing=2;
            UnityEngine.QualitySettings.lodBias=1;
            UnityEngine.QualitySettings.vSyncCount=1;
            UnityEngine.QualitySettings.asyncUploadBufferSize=32;
        }
        public static void BuildBaseline()
        {
            EditorSceneManager.OpenScene("Assets/Scenes/Grassland.unity");
            ProjectBuilder.BuildCurrentScene();
        }
    }
}
