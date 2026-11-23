using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace SkyMeadow.Editor
{
    public static class RefinedModelInstaller
    {
        const string Root = "Assets/Art/RefinedModels/";

        [MenuItem("Sky Meadow/Apply Refined Models")]
        public static void Apply()
        {
            AssetDatabase.Refresh();
            Install("Pip", 2.2f, true);
            Install("WindCoin", 1f, false);
            UpdateSceneInstances();
            FixCylinderColliders();
            AssetDatabase.SaveAssets();
            EditorSceneManager.SaveOpenScenes();
        }

        public static void FixCylinderColliders()
        {
            int repaired=0;
            foreach(var capsule in Object.FindObjectsByType<CapsuleCollider>())
            {
                var filter=capsule.GetComponent<MeshFilter>();
                if(!filter || !filter.sharedMesh || filter.sharedMesh.name!="Cylinder")continue;
                var obj=capsule.gameObject;Object.DestroyImmediate(capsule);
                var collider=obj.GetComponent<MeshCollider>();
                if(!collider)collider=obj.AddComponent<MeshCollider>();
                collider.sharedMesh=filter.sharedMesh;collider.convex=false;repaired++;
            }
            Debug.Log("CYLINDER_COLLIDERS_REPAIRED: "+repaired);
        }

        static void UpdateSceneInstances()
        {
            // The original generator saved ordinary clones, so update those explicitly.
            var player = Object.FindAnyObjectByType<MeadowPlayer>();
            if (player)
            {
                var replacement = Replace(player.gameObject, "Pip");
                var updated = replacement.GetComponent<MeadowPlayer>();
                updated.view = player.view;
                foreach (var camera in Object.FindObjectsByType<MeadowCamera>())
                    if (camera.target == player.transform) camera.target = replacement.transform;
                foreach (var game in Object.FindObjectsByType<MeadowGame>())
                    if (game.player == player) game.player = updated;
                Object.DestroyImmediate(player.gameObject);
            }
            foreach (var coin in Object.FindObjectsByType<WindCoin>())
            {
                Replace(coin.gameObject, "WindCoin");
                Object.DestroyImmediate(coin.gameObject);
            }
            EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        }

        static GameObject Replace(GameObject previous, string name)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/" + name + ".prefab");
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, previous.transform.parent);
            instance.name = previous.name;
            instance.transform.localPosition = previous.transform.localPosition;
            instance.transform.localRotation = previous.transform.localRotation;
            instance.transform.localScale = previous.transform.localScale;
            return instance;
        }

        static void Install(string name, float size, bool character)
        {
            string folder = Root + name;
            if (!Directory.Exists(folder)) return;
            string path = Directory.GetFiles(folder, "*.fbx", SearchOption.AllDirectories).FirstOrDefault();
            if (path == null) return;
            path = path.Replace('\\', '/');
            var importer = (ModelImporter)AssetImporter.GetAtPath(path);
            importer.importAnimation = false;
            importer.isReadable = true;
            importer.addCollider = false;
            importer.materialImportMode = ModelImporterMaterialImportMode.None;
            importer.SaveAndReimport();
            var model = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            var material = MakeMaterial(folder, name, character);
            string prefabPath = "Assets/Prefabs/" + name + ".prefab";
            var prefab = PrefabUtility.LoadPrefabContents(prefabPath);
            try
            {
                // Preserve the gameplay root, controller and existing far-distance coin geometry.
                var previous = prefab.transform.Find("Refined Visual");
                if (previous) Object.DestroyImmediate(previous.gameObject);
                var lod = prefab.GetComponent<LODGroup>();
                if (lod) Object.DestroyImmediate(lod);
                if (character)
                    foreach (Transform child in prefab.transform.Cast<Transform>().ToArray())
                        Object.DestroyImmediate(child.gameObject);
                var visual = new GameObject("Refined Visual").transform;
                visual.SetParent(prefab.transform, false);
                var instance = (GameObject)PrefabUtility.InstantiatePrefab(model);
                instance.transform.SetParent(visual, false);
                var bounds = BoundsOf(instance);
                float extent = character ? bounds.size.y : Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);
                if (extent < .00001f) throw new Exception("Empty model: " + path);
                float scale = size / extent;
                instance.transform.localScale *= scale;
                bounds = BoundsOf(instance);
                instance.transform.localPosition -= character
                    ? new Vector3(bounds.center.x, bounds.min.y, bounds.center.z)
                    : bounds.center;
                if(!character)MeshBudget.Coin(instance,path);
                foreach (var renderer in instance.GetComponentsInChildren<Renderer>())
                {
                    renderer.sharedMaterials = Enumerable.Repeat(material, renderer.sharedMaterials.Length).ToArray();
                    if (!character) renderer.shadowCastingMode = ShadowCastingMode.Off;
                }
                foreach (var collider in instance.GetComponentsInChildren<Collider>()) Object.DestroyImmediate(collider);
                if (character)
                {
                    MeadowRigBuilder.Build(prefab,visual,instance,path);
                    prefab.GetComponent<MeadowPlayer>().visual = visual;
                    foreach (var t in prefab.GetComponentsInChildren<Transform>()) t.gameObject.layer = 2;
                }
                else
                {
                    var near = visual.GetComponentsInChildren<Renderer>();
                    var far = prefab.GetComponentsInChildren<Renderer>().Except(near).ToArray();
                    if (far.Length > 0)
                    {
                        lod = prefab.AddComponent<LODGroup>();
                        lod.SetLODs(new[] {new LOD(.07f, near), new LOD(.003f, far)});
                        lod.RecalculateBounds();
                    }
                }
                PrefabUtility.SaveAsPrefabAsset(prefab, prefabPath);
                Directory.CreateDirectory("Design/ModelPreviews");
                Preview(visual.gameObject, name, 0);
                Preview(visual.gameObject, name, 180);
                if(character)
                {
                    var animation=prefab.GetComponent<MeadowCharacterAnimation>();
                    foreach(string pose in new[]{"Walk","Run","Jump","Dash"})
                    {
                        animation.Pose(1,1.2f,pose=="Walk"||pose=="Run",8,pose=="Dash",pose=="Run",0,1);
                        if(pose=="Run")MeadowRigBuilder.Diagnose(visual.gameObject);
                        Preview(visual.gameObject,"Pip_"+pose,90);
                    }
                    foreach(var bone in animation.bones)bone.localRotation=Quaternion.identity;
                    visual.localPosition=Vector3.zero;
                }
                int triangles = visual.GetComponentsInChildren<MeshFilter>().Sum(m => (int)m.sharedMesh.GetIndexCount(0) / 3)+visual.GetComponentsInChildren<SkinnedMeshRenderer>().Sum(m=>(int)m.sharedMesh.GetIndexCount(0)/3);
                File.WriteAllText(folder + "/ImportReport.txt", $"Source: {path}\nSize: {BoundsOf(visual.gameObject).size}\nTriangles (first submeshes): {triangles}\nSkinned renderers: {visual.GetComponentsInChildren<SkinnedMeshRenderer>().Length}\nAnimation: {(character?"16-bone procedural Idle/Walk/Run/Jump/Fall/Land/Dash":"coin rotation")}\n");
                Debug.Log("REFINED_MODEL_INSTALLED: " + name + " triangles=" + triangles);
            }
            finally { PrefabUtility.UnloadPrefabContents(prefab); }
        }

        static Bounds BoundsOf(GameObject obj)
        {
            var renderers = obj.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0) throw new Exception("Model has no renderers");
            var bounds = renderers[0].bounds;
            foreach (var renderer in renderers.Skip(1)) bounds.Encapsulate(renderer.bounds);
            return bounds;
        }

        static Material MakeMaterial(string folder, string name, bool character)
        {
            string Find(string suffix) => Directory.GetFiles(folder, "*" + suffix + ".jpg", SearchOption.AllDirectories).FirstOrDefault()?.Replace('\\', '/');
            Texture2D Texture(string suffix, bool normal = false)
            {
                string path = Find(suffix);
                if (path == null) return null;
                var importer = (TextureImporter)AssetImporter.GetAtPath(path);
                importer.textureType = normal ? TextureImporterType.NormalMap : TextureImporterType.Default;
                importer.sRGBTexture = !normal && suffix == "basecolor";
                importer.maxTextureSize = character ? 2048 : 1024;
                importer.SaveAndReimport();
                return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            }
            string materialPath = folder + "/" + name + ".mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if (!material) { material = new Material(Shader.Find("Standard")); AssetDatabase.CreateAsset(material, materialPath); }
            material.mainTexture = Texture("basecolor");
            material.color = Color.white;
            material.SetTexture("_BumpMap", Texture("normal", true));
            material.EnableKeyword("_NORMALMAP");
            material.SetFloat("_Metallic", character ? 0 : .65f);
            material.SetFloat("_Glossiness", character ? .25f : .55f);
            EditorUtility.SetDirty(material);
            return material;
        }

        static void Preview(GameObject source, string name, int angle)
        {
            var preview = new PreviewRenderUtility();
            try
            {
                var clone = Object.Instantiate(source);
                clone.transform.position = Vector3.zero;
                preview.AddSingleGO(clone);
                var bounds = BoundsOf(clone);
                if(clone.GetComponentInChildren<SkinnedMeshRenderer>())bounds=new Bounds(new Vector3(0,1.1f,0),new Vector3(1.4f,2.2f,1.1f));
                preview.camera.backgroundColor = new Color(.18f,.23f,.3f);
                preview.camera.clearFlags = CameraClearFlags.SolidColor;
                preview.camera.fieldOfView = 32;
                preview.camera.nearClipPlane = .01f;
                preview.camera.farClipPlane = 100;
                preview.camera.transform.position = bounds.center + Quaternion.Euler(5, angle, 0) * Vector3.forward * (bounds.size.magnitude * 2);
                preview.camera.transform.LookAt(bounds.center);
                preview.lights[0].intensity = 1.3f;
                preview.lights[0].transform.rotation = Quaternion.Euler(35, -35, 0);
                preview.lights[1].intensity = .8f;
                preview.BeginStaticPreview(new Rect(0, 0, 700, 700));
                preview.camera.Render();
                var image = preview.EndStaticPreview();
                File.WriteAllBytes($"Design/ModelPreviews/{name}_{angle}.png", image.EncodeToPNG());
                Object.DestroyImmediate(image);
            }
            finally { preview.Cleanup(); }
        }

        public static void BuildInstalled()
        {
            EditorSceneManager.OpenScene("Assets/Scenes/Grassland.unity");
            Apply();
            EditorSceneManager.SaveOpenScenes();
            ProjectBuilder.BuildCurrentScene();
        }
    }
}
