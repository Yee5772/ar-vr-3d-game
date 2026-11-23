using UnityEngine;
namespace SkyMeadow
{
    public class MeadowGame : MonoBehaviour
    {
        public static MeadowGame Instance;public static bool Paused;public MeadowPlayer player;public int collected,total;
        float flash;AudioSource audioSource;AudioClip coinSound;GUIStyle title,label,small;
        void Awake(){Instance=this;Paused=false;Time.timeScale=1;}
        void Start()
        {
            total=FindObjectsByType<WindCoin>().Length;
            audioSource=gameObject.AddComponent<AudioSource>();
            int count=8820;float[] samples=new float[count];for(int i=0;i<count;i++){float t=i/44100f;samples[i]=Mathf.Sin(t*2*Mathf.PI*(t<.07f?880:1320))*.15f*(1-i/(float)count);}
            coinSound=AudioClip.Create("Coin chime",count,1,44100,false);coinSound.SetData(samples,0);
        }
        public void Collect(){collected++;flash=1;audioSource.PlayOneShot(coinSound);PlayerPrefs.SetInt("SkyMeadowBest",Mathf.Max(collected,PlayerPrefs.GetInt("SkyMeadowBest")));}
        public void SetPaused(bool paused)
        {
            Paused=paused;Time.timeScale=paused?0:1;
            // Keep the pointer available after Resume; the next gameplay click captures it.
            Cursor.lockState=CursorLockMode.None;Cursor.visible=true;
        }
        public void ResumeGame()=>SetPaused(false);
        public void QuitGame(){Application.Quit();}
        void Update(){flash=Mathf.Max(0,flash-Time.unscaledDeltaTime*2);if(Input.GetKeyDown(KeyCode.Tab))SetPaused(!Paused);}
        void OnDestroy(){Paused=false;Time.timeScale=1;if(coinSound)Destroy(coinSound);}
        void OnGUI()
        {
            if(title==null){title=new GUIStyle(GUI.skin.label){fontSize=30,fontStyle=FontStyle.Bold};title.normal.textColor=Color.white;label=new GUIStyle(title){fontSize=21};small=new GUIStyle(title){fontSize=15,fontStyle=FontStyle.Normal};}
            float scale=Screen.height/900f;GUI.matrix=Matrix4x4.TRS(Vector3.zero,Quaternion.identity,Vector3.one*scale);float width=Screen.width/scale;
            GUI.color=new Color(.05f,.13f,.17f,.88f);GUI.DrawTexture(new Rect(24,24,330,136),Texture2D.whiteTexture);GUI.color=Color.white;
            GUI.Label(new Rect(44,37,310,40),"SKY MEADOW",title);GUI.color=Color.Lerp(new Color(1,.8f,.25f),Color.white,flash);
            GUI.Label(new Rect(44,80,300,32),"WIND COINS    "+collected.ToString("000")+" / "+total,label);GUI.color=Color.white;
            GUI.Label(new Rect(44,120,300,30),"BEST "+PlayerPrefs.GetInt("SkyMeadowBest")+"   •   EXPLORE THE SKY",small);
            GUI.color=new Color(.05f,.13f,.17f,.85f);GUI.DrawTexture(new Rect(24,782,600,94),Texture2D.whiteTexture);GUI.color=Color.white;
            GUI.Label(new Rect(42,796,580,28),"WASD Move    SPACE Double jump    SHIFT Sprint    E Air dash",small);
            GUI.Label(new Rect(42,827,580,28),"Click / RMB Look    Wheel Zoom    R Respawn    TAB Pause",small);
            GUI.Label(new Rect(width-250,28,230,30),"ALTITUDE  "+Mathf.RoundToInt(player.transform.position.y)+" m",label);
            if(collected==total && total>0)GUI.Label(new Rect(width/2-240,180,500,50),"ALL WIND COINS FOUND!",title);
            if(Paused)
            {
                GUI.Box(new Rect(width/2-210,300,420,235),"");GUI.Label(new Rect(width/2-100,320,250,45),"PAUSED",title);
                if(GUI.Button(new Rect(width/2-140,385,280,44),"Resume"))ResumeGame();
                if(GUI.Button(new Rect(width/2-140,450,280,44),"Quit game"))QuitGame();
            }
        }
    }
}
