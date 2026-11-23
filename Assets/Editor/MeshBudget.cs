using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityMeshSimplifier;
using Object=UnityEngine.Object;

namespace SkyMeadow.Editor
{
    public static class MeshBudget
    {
        public static Mesh Reduce(Mesh source,int triangles,string key,string sourceFile)
        {
            Directory.CreateDirectory("Assets/Art/Optimized");
            string path="Assets/Art/Optimized/"+key+".asset";
            string stamp=path+".stamp.txt";
            string version="2:"+triangles+":"+new FileInfo(sourceFile).LastWriteTimeUtc.Ticks;
            var cached=AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if(cached && File.Exists(stamp) && File.ReadAllText(stamp)==version)return cached;
            int before=(int)source.GetIndexCount(0)/3;
            Debug.Log($"SIMPLIFY_START {key}: {before} -> target {triangles}");
            var simplifier=new MeshSimplifier();
            var options=SimplificationOptions.Default;
            // Tripo exports contain extensive UV seams. Locking every seam prevents
            // useful reduction; the simplifier interpolates the UV attributes instead.
            options.PreserveUVSeamEdges=false;options.PreserveUVFoldoverEdges=false;
            simplifier.SimplificationOptions=options;
            simplifier.Initialize(source);
            simplifier.SimplifyMesh(Mathf.Min(1,triangles/(float)before));
            var mesh=simplifier.ToMesh();mesh.name=key;
            if(cached){EditorUtility.CopySerialized(mesh,cached);Object.DestroyImmediate(mesh);mesh=cached;EditorUtility.SetDirty(mesh);}
            else AssetDatabase.CreateAsset(mesh,path);
            File.WriteAllText(stamp,version);
            string report=$"{key}: {before:N0} -> {(int)mesh.GetIndexCount(0)/3:N0} triangles; {source.vertexCount:N0} -> {mesh.vertexCount:N0} vertices\n";
            File.AppendAllText("Design/ModelOptimizationReport.txt",report);Debug.Log(report);
            return mesh;
        }
        public static void Coin(GameObject instance,string sourceFile)
        {
            PrefabUtility.UnpackPrefabInstance(instance,PrefabUnpackMode.Completely,InteractionMode.AutomatedAction);
            int i=0;
            foreach(var filter in instance.GetComponentsInChildren<MeshFilter>())
                filter.sharedMesh=Reduce(filter.sharedMesh,3000,"WindCoin_Game_"+i++,sourceFile);
        }
    }
}
