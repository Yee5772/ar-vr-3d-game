using System.Collections.Generic;
using UnityEngine;

namespace SkyMeadow
{
    public class MeadowWorld : MonoBehaviour
    {
        public Material grass, rock, sand, teal, orange, cream, skin, dark, gold, white;
        public GameObject playerPrefab, coinPrefab;
        public const int Seed = 7821;
        public static float Height(float x, float z)
        {
            float h = 9 + Mathf.Sin(x*.024f+.6f)*Mathf.Cos(z*.029f)*6;
            h += Mathf.Sin(x*.043f)*Mathf.Cos(z*.037f)*3;
            return h;
        }
        public static Vector3 Ground(float x,float z,float offset=0) => new Vector3(x,Height(x,z)+offset,z);
        public void Generate()
        {
            Random.InitState(Seed);
            grass=Mat("Meadow", new Color(.37f,.69f,.24f)); rock=Mat("Warm cliffs",new Color(.46f,.36f,.26f));
            sand=Mat("Trail",new Color(.83f,.72f,.43f)); teal=Mat("Explorer teal",new Color(.04f,.43f,.44f));
            orange=Mat("Explorer orange",new Color(1,.36f,.08f)); cream=Mat("Cream",new Color(1,.91f,.69f));
            skin=Mat("Skin",new Color(1,.68f,.43f)); dark=Mat("Ink",new Color(.09f,.14f,.22f));
            gold=Mat("Wind gold",new Color(1,.72f,.09f),.65f); white=Mat("Cloud",Color.white);
            var terrain=new GameObject("Grassland mesh — 240 x 240 m"); terrain.transform.parent=transform;
            var mesh=new Mesh {name="Grassland_Triangulated",indexFormat=UnityEngine.Rendering.IndexFormat.UInt32};
            var v=new List<Vector3>(); var t=new List<int>(); var colors=new List<Color>();
            for(int z=0;z<120;z++) for(int x=0;x<120;x++)
            {
                Vector3 a=Ground(x*2-120,z*2-120),b=Ground(x*2-118,z*2-120),c=Ground(x*2-120,z*2-118),d=Ground(x*2-118,z*2-118);
                AddTri(v,t,colors,a,c,b); AddTri(v,t,colors,b,c,d);
            }
            mesh.SetVertices(v);mesh.SetTriangles(t,0);mesh.SetColors(colors);mesh.RecalculateNormals();mesh.RecalculateBounds();
            terrain.AddComponent<MeshFilter>().sharedMesh=mesh;terrain.AddComponent<MeshRenderer>().sharedMaterial=grass;
            terrain.AddComponent<MeshCollider>().sharedMesh=mesh;
            // A broad meandering path and a gentle ground coin route.
            for(int i=0;i<70;i++)
            {
                float z=-76+i*2.15f,x=Mathf.Sin(i*.11f)*16;
                Shape("Trail stone",PrimitiveType.Cylinder,Ground(x,z,.015f),new Vector3(4.8f,.04f,3.1f),sand,transform);
                if(i%2==0) Coin(Ground(x,z,1.7f));
            }
            // Stairway to the sky: 3 m rises and 5 m gaps, comfortably within double jump range.
            float startY=Height(0,8);
            for(int i=0;i<20;i++)
            {
                float a=i*.40f;Vector3 p=new Vector3(Mathf.Sin(a)*17,startY+3+i*2.7f,10+i*4.1f);
                Island("Sky step "+(i+1),p,i==19?7:3.8f);
                Coin(p+Vector3.up*1.8f);
                if(i<19) Coin(p+new Vector3(0,3.4f,2));
            }
            // Ground scenery is generated directly, ready for further map editing.
            for(int i=0;i<130;i++)
            {
                float x=Random.Range(-112f,112f),z=Random.Range(-112f,112f);
                if(Mathf.Abs(x-Mathf.Sin((z+76)/2.15f*.11f)*16)<8 || (Mathf.Abs(x)<25 && z>0)) continue;
                Vector3 p=Ground(x,z);float s=Random.Range(.8f,1.9f);
                if(i%3==0)
                {
                    Shape("Meadow boulder",PrimitiveType.Sphere,p+Vector3.up*s*.4f,new Vector3(s*2,s,s*1.6f),rock,transform,true);
                }
                else
                {
                    Shape("Tree trunk",PrimitiveType.Cylinder,p+Vector3.up*1.8f*s,new Vector3(.5f,1.8f*s,.5f),rock,transform,true);
                    Shape("Round tree crown",PrimitiveType.Sphere,p+Vector3.up*4*s,new Vector3(4,4,4)*s,grass,transform,true);
                }
            }
            for(int i=0;i<28;i++)
            {
                Vector3 p=new Vector3(Random.Range(-150f,150f),Random.Range(72f,95f),Random.Range(-150f,150f));
                for(int j=0;j<3;j++)Shape("Cloud",PrimitiveType.Sphere,p+new Vector3(j*4,Mathf.Sin(j)*2,0),new Vector3(12,5,8),white,transform);
            }
            var sun=new GameObject("Warm afternoon sun").AddComponent<Light>();sun.transform.parent=transform;
            sun.type=LightType.Directional;sun.intensity=1.25f;sun.color=new Color(1,.94f,.81f);sun.shadows=LightShadows.Soft;sun.transform.rotation=Quaternion.Euler(48,-35,0);
            RenderSettings.ambientLight=new Color(.62f,.73f,.84f);RenderSettings.fog=true;RenderSettings.fogColor=new Color(.61f,.81f,.91f);RenderSettings.fogDensity=.0025f;
            var player=Instantiate(playerPrefab,Ground(0,-76,2),Quaternion.identity);player.name="Pip — Player";
            var cameraObject=new GameObject("Third person camera");cameraObject.tag="MainCamera";
            var cam=cameraObject.AddComponent<Camera>();cam.backgroundColor=RenderSettings.fogColor;cam.clearFlags=CameraClearFlags.SolidColor;cam.farClipPlane=500;cam.nearClipPlane=.12f;cam.fieldOfView=65;
            cameraObject.AddComponent<AudioListener>();cameraObject.AddComponent<MeadowCamera>().target=player.transform;
            player.GetComponent<MeadowPlayer>().view=cam.transform;
            var game=new GameObject("Game session").AddComponent<MeadowGame>(); game.player=player.GetComponent<MeadowPlayer>();
        }
        static void AddTri(List<Vector3> v,List<int> t,List<Color> c,Vector3 a,Vector3 b,Vector3 d)
        { int n=v.Count;v.Add(a);v.Add(b);v.Add(d);t.Add(n);t.Add(n+1);t.Add(n+2);Color col=Color.Lerp(new Color(.7f,.86f,.42f),Color.white,Random.value);c.Add(col);c.Add(col);c.Add(col); }
        public static Material Mat(string name,Color color,float metallic=0)
        {var m=new Material(Shader.Find("Standard"));m.name=name;m.color=color;m.SetFloat("_Metallic",metallic);m.SetFloat("_Glossiness",metallic>0?.65f:.12f);return m;}
        public static GameObject Shape(string name,PrimitiveType type,Vector3 pos,Vector3 scale,Material mat,Transform parent,bool solid=false)
        {
            var o=GameObject.CreatePrimitive(type);o.name=name;o.transform.SetParent(parent,false);o.transform.localPosition=pos;o.transform.localScale=scale;o.GetComponent<Renderer>().sharedMaterial=mat;
            // Unity's cylinder primitive uses a capsule collider, whose rounded ends
            // protrude above wide, shallow platforms and even narrow tree trunks.
            if(solid && type==PrimitiveType.Cylinder)
            {
                var original=o.GetComponent<Collider>();
                if(Application.isPlaying){original.enabled=false;Destroy(original);}else DestroyImmediate(original);
                o.AddComponent<MeshCollider>().sharedMesh=o.GetComponent<MeshFilter>().sharedMesh;
            }
            if(!solid) {var col=o.GetComponent<Collider>();if(Application.isPlaying)Destroy(col);else DestroyImmediate(col);}
            return o;
        }
        void Island(string name,Vector3 p,float radius)
        {
            Shape(name+" rock",PrimitiveType.Cylinder,p-Vector3.up*1.4f,new Vector3(radius*2,1.4f,radius*2),rock,transform,true);
            Shape(name+" turf",PrimitiveType.Cylinder,p-Vector3.up*.08f,new Vector3(radius*2.05f,.12f,radius*2.05f),grass,transform,true);
        }
        void Coin(Vector3 p){var o=Instantiate(coinPrefab,p,Quaternion.identity,transform);o.name="Wind coin";}
    }
}
