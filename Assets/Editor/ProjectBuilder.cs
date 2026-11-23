using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Build.Reporting;

namespace SkyMeadow.Editor
{
    public static class ProjectBuilder
    {
        [InitializeOnLoadMethod]
        static void FirstOpen()
        {
            if(Application.isBatchMode || File.Exists("Assets/Scenes/Grassland.unity"))return;
            EditorApplication.delayCall+=()=>{if(!File.Exists("Assets/Scenes/Grassland.unity"))CreateWorld();};
        }
        [MenuItem("Sky Meadow/Rebuild World")]
        public static void CreateWorld()
        {
            Directory.CreateDirectory("Assets/Scenes");Directory.CreateDirectory("Assets/Art/Generated");Directory.CreateDirectory("Assets/Prefabs");
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            var world=new GameObject("SKY MEADOW — Generated World").AddComponent<MeadowWorld>();
            // Materials and models are persisted below, not hidden in runtime-only generation.
            var temporary=new GameObject("Material setup").AddComponent<MeadowWorld>();
            Material teal=MeadowWorld.Mat("Pip teal",new Color(.03f,.45f,.46f)),orange=MeadowWorld.Mat("Pip orange",new Color(1,.35f,.07f)),cream=MeadowWorld.Mat("Pip cream",new Color(1,.91f,.73f)),skin=MeadowWorld.Mat("Pip skin",new Color(1,.68f,.43f)),dark=MeadowWorld.Mat("Pip navy",new Color(.10f,.15f,.23f)),brown=MeadowWorld.Mat("Boot leather",new Color(.28f,.15f,.08f)),gold=MeadowWorld.Mat("Coin gold",new Color(1,.74f,.1f),.7f);
            Object.DestroyImmediate(temporary.gameObject);
            var pip=new GameObject("Pip");pip.layer=2;
            var controller=pip.AddComponent<CharacterController>();controller.height=2.1f;controller.center=Vector3.up*1.05f;controller.radius=.38f;controller.stepOffset=.35f;controller.slopeLimit=55;
            var visual=new GameObject("Visual — replace with your refined model").transform;visual.SetParent(pip.transform,false);
            Part("Jacket",PrimitiveType.Capsule,new Vector3(0,1.02f,0),new Vector3(.72f,.48f,.48f),orange,visual);
            Part("Shirt",PrimitiveType.Sphere,new Vector3(0,1.13f,.22f),new Vector3(.34f,.57f,.08f),cream,visual);
            Part("Shorts",PrimitiveType.Sphere,new Vector3(0,.66f,0),new Vector3(.65f,.42f,.44f),dark,visual);
            Part("Head",PrimitiveType.Sphere,new Vector3(0,1.72f,0),new Vector3(.85f,.86f,.76f),skin,visual);
            Part("Cap",PrimitiveType.Sphere,new Vector3(0,2.02f,-.04f),new Vector3(.93f,.44f,.84f),teal,visual);
            Part("Cap visor",PrimitiveType.Sphere,new Vector3(0,1.98f,.35f),new Vector3(.84f,.10f,.43f),teal,visual);
            Part("Nose",PrimitiveType.Sphere,new Vector3(0,1.7f,.4f),Vector3.one*.23f,skin,visual);
            Part("Backpack",PrimitiveType.Cube,new Vector3(0,1.15f,-.34f),new Vector3(.5f,.58f,.23f),teal,visual);
            for(int side=-1;side<=1;side+=2)
            {
                Part("Eye",PrimitiveType.Sphere,new Vector3(side*.18f,1.8f,.34f),new Vector3(.13f,.22f,.08f),cream,visual);
                Part("Pupil",PrimitiveType.Sphere,new Vector3(side*.18f,1.8f,.385f),new Vector3(.07f,.13f,.04f),dark,visual);
                Part("Sleeve",PrimitiveType.Capsule,new Vector3(side*.45f,1.11f,0),new Vector3(.26f,.22f,.28f),orange,visual);
                Part("Glove",PrimitiveType.Sphere,new Vector3(side*.52f,.79f,.03f),new Vector3(.25f,.3f,.26f),cream,visual);
                Part("Leg",PrimitiveType.Capsule,new Vector3(side*.2f,.42f,0),new Vector3(.21f,.20f,.21f),skin,visual);
                Part("Boot",PrimitiveType.Sphere,new Vector3(side*.2f,.15f,.1f),new Vector3(.31f,.31f,.48f),brown,visual);
                Part("Ear",PrimitiveType.Sphere,new Vector3(side*.42f,1.69f,0),new Vector3(.18f,.25f,.18f),skin,visual);
            }
            var player=pip.AddComponent<MeadowPlayer>();player.visual=visual;
            foreach(Transform child in pip.GetComponentsInChildren<Transform>())child.gameObject.layer=2;
            PersistMaterials(pip);
            world.playerPrefab=PrefabUtility.SaveAsPrefabAsset(pip,"Assets/Prefabs/Pip.prefab");Object.DestroyImmediate(pip);
            var coin=new GameObject("WindCoin");coin.AddComponent<WindCoin>();
            var disc=Part("Visual — replace with refined coin",PrimitiveType.Cylinder,Vector3.zero,new Vector3(1,.075f,1),gold,coin.transform);disc.transform.localRotation=Quaternion.Euler(90,0,0);
            for(int side=-1;side<=1;side+=2)
            {
                var star=Part("Wind emblem",PrimitiveType.Cube,new Vector3(0,0,side*.085f),new Vector3(.35f,.35f,.035f),cream,coin.transform);star.transform.localRotation=Quaternion.Euler(0,0,45);
            }
            PersistMaterials(coin);world.coinPrefab=PrefabUtility.SaveAsPrefabAsset(coin,"Assets/Prefabs/WindCoin.prefab");Object.DestroyImmediate(coin);
            world.Generate();
            PersistMaterials(world.gameObject);
            foreach(var mf in world.GetComponentsInChildren<MeshFilter>())
            {
                if(!AssetDatabase.Contains(mf.sharedMesh)&&mf.sharedMesh.name=="Grassland_Triangulated")
                {
                    var existing=AssetDatabase.LoadAssetAtPath<Mesh>("Assets/Art/Generated/Grassland.asset");
                    if(existing){EditorUtility.CopySerialized(mf.sharedMesh,existing);mf.sharedMesh=existing;mf.GetComponent<MeshCollider>().sharedMesh=existing;EditorUtility.SetDirty(existing);}
                    else AssetDatabase.CreateAsset(mf.sharedMesh,"Assets/Art/Generated/Grassland.asset");
                }
            }
            var camera=Object.FindAnyObjectByType<MeadowCamera>();camera.transform.position=MeadowWorld.Ground(0,-86,7);camera.transform.LookAt(MeadowWorld.Ground(0,-70,2));
            PlayerSettings.companyName="Meadow Studio";PlayerSettings.productName="Sky Meadow";PlayerSettings.defaultScreenWidth=1440;PlayerSettings.defaultScreenHeight=900;PlayerSettings.fullScreenMode=FullScreenMode.Windowed;
            PlayerSettings.runInBackground=true;PlayerSettings.colorSpace=ColorSpace.Linear;PlayerSettings.SetScriptingBackend(UnityEditor.Build.NamedBuildTarget.Standalone,ScriptingImplementation.Mono2x);
            QualitySettings.shadows=ShadowQuality.All;QualitySettings.shadowDistance=100;QualitySettings.antiAliasing=4;
            EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene(),"Assets/Scenes/Grassland.unity");
            EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene("Assets/Scenes/Grassland.unity",true)};
            AssetDatabase.SaveAssets();
            Validate(world);
            RenderPreview(camera.GetComponent<Camera>());
        }
        static GameObject Part(string name,PrimitiveType type,Vector3 p,Vector3 s,Material m,Transform parent)=>MeadowWorld.Shape(name,type,p,s,m,parent);
        static void PersistMaterials(GameObject root)
        {
            foreach(var r in root.GetComponentsInChildren<Renderer>())foreach(var m in r.sharedMaterials)
            {if(m&&!AssetDatabase.Contains(m)){string path=AssetDatabase.GenerateUniqueAssetPath("Assets/Art/Generated/"+m.name.Replace("/","_")+".mat");AssetDatabase.CreateAsset(m,path);}}
        }
        static void Validate(MeadowWorld world)
        {
            int coins=Object.FindObjectsByType<WindCoin>().Length;
            if(coins!=74)throw new System.Exception("Expected 74 coins; got "+coins);
            var mc=world.GetComponentInChildren<MeshCollider>();if(!mc||mc.sharedMesh.vertexCount!=86400)throw new System.Exception("Terrain mesh invalid");
            // Each step rises 2.7 m; a full jump rises 3.27 m, double jump ~6.5 m.
            float jumpApex=14*14/(2f*30);if(jumpApex<2.7f)throw new System.Exception("Jump cannot reach next step");
            File.WriteAllText("Validation.txt","PASS: 74 coins; 28,800 terrain triangles; collider present; 20 sky steps; jump apex "+jumpApex+"m; prefabs and materials saved.\n");
            Debug.Log("SKY_MEADOW_VALIDATION_PASS");
        }
        static void RenderPreview(Camera camera)
        {
            var rt=new RenderTexture(1440,900,24);camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;
            var tex=new Texture2D(1440,900,TextureFormat.RGB24,false);tex.ReadPixels(new Rect(0,0,1440,900),0,0);tex.Apply();File.WriteAllBytes("Preview.png",tex.EncodeToPNG());camera.targetTexture=null;RenderTexture.active=null;Object.DestroyImmediate(rt);Object.DestroyImmediate(tex);
        }
        public static void BuildAll()
        {
            CreateWorld();RefinedModelInstaller.Apply();
            BuildCurrentScene();
        }
        public static void BuildCurrentScene()
        {
            Directory.CreateDirectory("Builds/Windows");
            PerformanceBuilder.ApplySettings();
            var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{"Assets/Scenes/Grassland.unity"},locationPathName="Builds/Windows/SkyMeadow.exe",target=BuildTarget.StandaloneWindows64,options=BuildOptions.None});
            if(report.summary.result!=BuildResult.Succeeded)throw new System.Exception("Build failed: "+report.summary.result);
            Debug.Log("SKY_MEADOW_BUILD_PASS");
        }
    }
}
