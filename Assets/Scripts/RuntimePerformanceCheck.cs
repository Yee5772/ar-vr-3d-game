using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace SkyMeadow
{
    public class RuntimePerformanceCheck:MonoBehaviour
    {
        int renderedFrames;
        void OnEnable(){Camera.onPostRender+=CountRender;}
        void OnDisable(){Camera.onPostRender-=CountRender;}
        void CountRender(Camera camera){if(camera==Camera.main)renderedFrames++;}
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Init(){if(Environment.GetCommandLineArgs().Contains("-benchmark"))new GameObject("Performance measurement").AddComponent<RuntimePerformanceCheck>();}
        IEnumerator Start()
        {
            QualitySettings.vSyncCount=0;Application.targetFrameRate=-1;
            yield return new WaitForSecondsRealtime(4);
            var report=new List<string>{DateTime.Now.ToString("s"),SystemInfo.graphicsDeviceName,$"Resolution: {Screen.width} x {Screen.height}","VSync disabled for uncapped comparison"};
            var skins=FindObjectsByType<SkinnedMeshRenderer>();
            var coins=FindObjectsByType<WindCoin>();
            bool valid=true;
            foreach(string stage in new[]{"Idle","WithoutCharacter","WithoutCoins"})
            {
                foreach(var skin in skins)skin.enabled=stage!="WithoutCharacter";
                foreach(var coin in coins)foreach(var r in coin.GetComponentsInChildren<Renderer>())r.forceRenderingOff=stage=="WithoutCoins";
                yield return new WaitForSecondsRealtime(1);
                var frames=new List<float>();float until=Time.realtimeSinceStartup+6;int renderStart=renderedFrames;
                while(Time.realtimeSinceStartup<until){yield return null;frames.Add(Time.unscaledDeltaTime*1000);}
                frames.Sort();
                bool rendering=renderedFrames-renderStart>=frames.Count*.9f;valid &= rendering;
                report.Add($"{stage}: {1000/frames.Average():F1} FPS average, median {frames[frames.Count/2]:F2} ms, p95 {frames[Math.Min(frames.Count-1,(int)(frames.Count*.95f))]:F2} ms ({frames.Count} frames; rendered={renderedFrames-renderStart}; valid={rendering})");
            }
            string label=Environment.GetCommandLineArgs().FirstOrDefault(s=>s.StartsWith("-benchmarkLabel="))?.Split('=')[1]??"Current";
            File.WriteAllLines(Path.Combine(Application.dataPath,"../Performance-"+label+".txt"),report);
            Application.Quit(valid?0:1);
        }
    }
}
