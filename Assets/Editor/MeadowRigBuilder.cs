using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using Object=UnityEngine.Object;

namespace SkyMeadow.Editor
{
    public static class MeadowRigBuilder
    {
        public static void Build(GameObject player,Transform visual,GameObject imported,string sourceFile)
        {
            var names=new[]{"Hips","Spine","Head","LeftUpperArm","LeftForearm","LeftHand","RightUpperArm","RightForearm","RightHand","LeftThigh","LeftShin","LeftFoot","RightThigh","RightShin","RightFoot","Neck"};
            var positions=new[]{new Vector3(0,.85f,0),new Vector3(0,1.2f,0),new Vector3(0,1.53f,0),new Vector3(.28f,1.38f,0),new Vector3(.43f,1.13f,0),new Vector3(.53f,.94f,.01f),new Vector3(-.28f,1.38f,0),new Vector3(-.43f,1.13f,0),new Vector3(-.53f,.94f,.01f),new Vector3(.17f,.8f,0),new Vector3(.17f,.43f,0),new Vector3(.17f,.15f,.03f),new Vector3(-.17f,.8f,0),new Vector3(-.17f,.43f,0),new Vector3(-.17f,.15f,.03f),new Vector3(0,1.48f,0)};
            var parents=new[]{-1,0,15,1,3,4,1,6,7,0,9,10,0,12,13,1};
            var bones=new Transform[names.Length];
            for(int i=0;i<bones.Length;i++){bones[i]=new GameObject(names[i]).transform;bones[i].SetParent(visual,false);bones[i].localPosition=positions[i];}
            for(int i=0;i<bones.Length;i++)if(parents[i]>=0)bones[i].SetParent(bones[parents[i]],true);
            var filters=imported.GetComponentsInChildren<MeshFilter>();
            int meshIndex=0;
            foreach(var filter in filters)
            {
                var source=filter.sharedMesh;
                var renderer=filter.GetComponent<MeshRenderer>();
                if(!source || !renderer)continue;
                // Bake all FBX unit/axis transforms before binding the normalized skeleton.
                var combines=Enumerable.Range(0,source.subMeshCount).Select(sub=>new CombineInstance{mesh=source,subMeshIndex=sub,transform=visual.worldToLocalMatrix*filter.transform.localToWorldMatrix}).ToArray();
                var mesh=new Mesh{name="Pip skinned "+meshIndex,indexFormat=IndexFormat.UInt32};
                mesh.CombineMeshes(combines,false,true);
                var highDetail=mesh;
                mesh=Object.Instantiate(MeshBudget.Reduce(highDetail,60000,"Pip_Game_"+meshIndex,sourceFile));
                Object.DestroyImmediate(highDetail);
                var vertices=mesh.vertices;
                var weights=new BoneWeight[vertices.Length];
                for(int i=0;i<vertices.Length;i++)weights[i]=Weight(vertices[i]);
                mesh.boneWeights=weights;
                mesh.bindposes=bones.Select(b=>b.worldToLocalMatrix*visual.localToWorldMatrix).ToArray();
                string path="Assets/Art/RefinedModels/Pip/PipSkinned"+meshIndex+".asset";
                var existing=AssetDatabase.LoadAssetAtPath<Mesh>(path);
                if(existing){EditorUtility.CopySerialized(mesh,existing);existing.boneWeights=weights;existing.bindposes=mesh.bindposes;Object.DestroyImmediate(mesh);mesh=existing;EditorUtility.SetDirty(mesh);}
                else AssetDatabase.CreateAsset(mesh,path);
                var obj=new GameObject("Pip animated skin "+meshIndex++);obj.transform.SetParent(visual,false);
                var skin=obj.AddComponent<SkinnedMeshRenderer>();skin.sharedMesh=mesh;skin.bones=bones;skin.rootBone=bones[0];
                skin.sharedMaterials=renderer.sharedMaterials;skin.quality=SkinQuality.Bone4;
                skin.localBounds=new Bounds(new Vector3(0,1.1f,0),new Vector3(3,3,3));
            }
            if(meshIndex==0)throw new Exception("No character meshes to rig");
            Object.DestroyImmediate(imported);
            var animation=player.GetComponent<MeadowCharacterAnimation>();
            if(!animation)animation=player.AddComponent<MeadowCharacterAnimation>();
            animation.bones=bones;
        }

        public static void Diagnose(GameObject root)
        {
            var skin=root.GetComponentInChildren<SkinnedMeshRenderer>();var baked=new Mesh();skin.BakeMesh(baked);
            var posed=baked.vertices;var rest=skin.sharedMesh.vertices;var indices=skin.sharedMesh.triangles;var weights=skin.sharedMesh.boneWeights;
            float worst=0;int index=0;
            for(int i=0;i<indices.Length;i+=3)
            {
                int a=indices[i],b=indices[i+1],c=indices[i+2];
                float diff=(posed[a]-posed[b]).sqrMagnitude+(posed[a]-posed[c]).sqrMagnitude-(rest[a]-rest[b]).sqrMagnitude-(rest[a]-rest[c]).sqrMagnitude;
                if(diff>worst){worst=diff;index=i;}
            }
            string report="Worst triangle stretch: "+worst+"\n";
            for(int j=0;j<3;j++){int i=indices[index+j];var w=weights[i];report+=$"Vertex {rest[i]} -> {posed[i]}: {skin.bones[w.boneIndex0].name} {w.weight0}, {skin.bones[w.boneIndex1].name} {w.weight1}, {skin.bones[w.boneIndex2].name} {w.weight2}, {skin.bones[w.boneIndex3].name} {w.weight3}\n";}
            System.IO.File.WriteAllText("Design/ModelPreviews/SkinDiagnostics.txt",report);Object.DestroyImmediate(baked);
        }

        static BoneWeight Blend(int lower,int upper,float factor)
        {
            factor=Mathf.SmoothStep(0,1,Mathf.Clamp01(factor));
            if(factor>.5f)return new BoneWeight{boneIndex0=upper,weight0=factor,boneIndex1=lower,weight1=1-factor};
            return new BoneWeight{boneIndex0=lower,weight0=1-factor,boneIndex1=upper,weight1=factor};
        }
        static BoneWeight Weight(Vector3 p)
        {
            float x=Mathf.Abs(p.x),y=p.y;
            var torso=y>1.48f?Blend(1,2,(y-1.48f)/.1f):y<.84f
                ? Mix(Leg(12,y),Leg(9,y),Mathf.SmoothStep(0,1,(p.x+.065f)/.13f))
                : Blend(0,1,(y-.95f)/.23f);
            // Blend continuously across the sleeve/body seam; a hard region split
            // tears triangles which cross that seam when the arm starts swinging.
            float armBoundary=Mathf.Lerp(.34f,.26f,Mathf.InverseLerp(.95f,1.35f,y));
            float seam=y<1.1f?.035f:.06f;
            float armMix=Mathf.SmoothStep(0,1,(x-armBoundary+seam)/(2*seam))
                *Mathf.SmoothStep(0,1,(y-.48f)/.17f)*(1-Mathf.SmoothStep(0,1,(y-1.42f)/.13f));
            if(armMix<=0)return torso;
            int shoulder=p.x>0?3:6;
            var arm=y<1.02f?Blend(shoulder+2,shoulder+1,(y-.9f)/.12f):Blend(shoulder+1,shoulder,(y-1.06f)/.17f);
            return Mix(torso,arm,armMix);
        }
        static BoneWeight Leg(int thigh,float y)=>y>.74f?Blend(thigh,0,(y-.74f)/.1f):y<.24f?Blend(thigh+2,thigh+1,(y-.14f)/.1f):Blend(thigh+1,thigh,(y-.36f)/.15f);
        static BoneWeight Mix(BoneWeight a,BoneWeight b,float t)
        {
            if(t<=0)return a;if(t>=1)return b;
            Span<float> weights=stackalloc float[16];weights.Clear();
            weights[a.boneIndex0]+=a.weight0*(1-t);weights[a.boneIndex1]+=a.weight1*(1-t);
            weights[a.boneIndex2]+=a.weight2*(1-t);weights[a.boneIndex3]+=a.weight3*(1-t);
            weights[b.boneIndex0]+=b.weight0*t;weights[b.boneIndex1]+=b.weight1*t;
            weights[b.boneIndex2]+=b.weight2*t;weights[b.boneIndex3]+=b.weight3*t;
            Span<int> indices=stackalloc int[4];Span<float> amounts=stackalloc float[4];float sum=0;
            for(int i=0;i<4;i++){int best=0;for(int j=1;j<16;j++)if(weights[j]>weights[best])best=j;indices[i]=best;amounts[i]=weights[best];sum+=amounts[i];weights[best]=0;}
            return new BoneWeight{boneIndex0=indices[0],weight0=amounts[0]/sum,boneIndex1=indices[1],weight1=amounts[1]/sum,boneIndex2=indices[2],weight2=amounts[2]/sum,boneIndex3=indices[3],weight3=amounts[3]/sum};
        }
    }
}
