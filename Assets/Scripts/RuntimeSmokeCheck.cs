using System;
using System.Collections;
using System.IO;
using UnityEngine;
namespace SkyMeadow
{
    public class RuntimeSmokeCheck : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Init(){if(Array.IndexOf(Environment.GetCommandLineArgs(),"-smokeTest")>=0)new GameObject("Runtime verification").AddComponent<RuntimeSmokeCheck>();}
        IEnumerator Start()
        {
            // Large imported meshes can stall the first rendered frames during GPU upload.
            // Wait for the actual landing rather than assuming a fixed startup duration.
            yield return null;
            var game=MeadowGame.Instance;var player=game.player;var motor=player.GetComponent<CharacterController>();
            float deadline=Time.realtimeSinceStartup+15;
            while(!player.Grounded && Time.realtimeSinceStartup<deadline)yield return null;
            bool ground=player.Grounded;int before=game.collected;
            var coin=FindAnyObjectByType<WindCoin>();
            player.enabled=false;motor.enabled=false;player.transform.position=coin.transform.position-Vector3.up;
            deadline=Time.realtimeSinceStartup+5;
            while(coin.gameObject.activeSelf && Time.realtimeSinceStartup<deadline)yield return null;
            bool pickup=!coin.gameObject.activeSelf && game.collected>before;
            motor.enabled=true;player.enabled=true;
            player.Respawn();
            for(int frame=0;frame<10;frame++)yield return null;
            bool respawn=Vector3.Distance(player.transform.position,player.Checkpoint)<1;
            bool refinedPlayer=player.transform.Find("Refined Visual")!=null;
            bool refinedCoin=coin.transform.Find("Refined Visual")!=null;
            bool platforms=true;int platformCount=0;
            foreach(var filter in FindObjectsByType<MeshFilter>())
            {
                if(!filter.name.StartsWith("Sky step ") || !filter.name.EndsWith(" turf"))continue;
                platformCount++;
                var collider=filter.GetComponent<MeshCollider>();
                platforms &= collider && !filter.GetComponent<CapsuleCollider>();
                if(!collider)continue;
                var bounds=collider.bounds;
                foreach(float fraction in new[]{0f,.65f})
                {
                    var point=bounds.center+Vector3.right*bounds.extents.x*fraction;
                    point.y=bounds.max.y+2;
                    platforms &= collider.Raycast(new Ray(point,Vector3.down),out RaycastHit hit,4) && Mathf.Abs(hit.point.y-bounds.max.y)<.015f;
                }
            }
            platforms &= platformCount==20;
            // Drop the controller onto a platform near its edge, not just a ray probe.
            var platform=GameObject.Find("Sky step 1 turf").GetComponent<MeshCollider>();
            player.enabled=false;motor.enabled=false;
            player.transform.position=new Vector3(platform.bounds.center.x+2,platform.bounds.max.y+2,platform.bounds.center.z);
            motor.enabled=true;
            for(int frame=0;frame<120 && !motor.isGrounded;frame++){motor.Move(Vector3.down*.06f);yield return null;}
            bool platformLanding=motor.isGrounded && Mathf.Abs(player.transform.position.y-platform.bounds.max.y)<.13f;
            player.Respawn();player.enabled=true;
            var animation=player.GetComponent<MeadowCharacterAnimation>();
            var skin=player.GetComponentInChildren<SkinnedMeshRenderer>();
            bool animated=false;
            if(animation && skin)
            {
                animation.enabled=false;
                foreach(var bone in animation.bones)bone.localRotation=Quaternion.identity;
                var baked=new Mesh();skin.BakeMesh(baked);var rest=baked.vertices;
                animation.Pose(1,1.2f,true,0,false,true,0,1);
                skin.BakeMesh(baked);var posed=baked.vertices;float displacement=0;
                for(int i=0;i<rest.Length;i+=31)displacement=Mathf.Max(displacement,Vector3.Distance(rest[i],posed[i]));
                animated=displacement>.08f;
                Destroy(baked);
                foreach(var bone in animation.bones)bone.localRotation=Quaternion.identity;
                animation.enabled=true;
            }
            game.SetPaused(true);var pausedPosition=player.transform.position;
            yield return new WaitForSecondsRealtime(.15f);
            bool pause=Time.timeScale==0 && Cursor.lockState==CursorLockMode.None && Vector3.Distance(pausedPosition,player.transform.position)<.001f;
            game.ResumeGame();pause &= !MeadowGame.Paused && Time.timeScale==1;
            string path=Path.Combine(Application.dataPath,"../RuntimeValidation.txt");
            File.WriteAllText(path,$"Ground collision: {ground}\nCoin pickup: {pickup}\nRespawn: {respawn}\nCoins: {game.total}\nRefined player: {refinedPlayer}\nRefined coin: {refinedCoin}\nFlat platform surfaces (20): {platforms}\nPlatform edge landing: {platformLanding}\nSkinned limb movement: {animated}\nPause / resume: {pause}\n");
            var camera=Camera.main;var rt=new RenderTexture(1440,900,24);camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;
            var tex=new Texture2D(1440,900,TextureFormat.RGB24,false);tex.ReadPixels(new Rect(0,0,1440,900),0,0);tex.Apply();
            File.WriteAllBytes(Path.Combine(Application.dataPath,"../Gameplay.png"),tex.EncodeToPNG());
            camera.targetTexture=null;RenderTexture.active=null;Destroy(rt);Destroy(tex);
            yield return new WaitForSeconds(.5f);Application.Quit(ground&&pickup&&respawn&&platforms&&platformLanding&&animated&&pause?0:1);
        }
    }
}
