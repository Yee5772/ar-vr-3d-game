using UnityEngine;
namespace SkyMeadow
{
    public class WindCoin : MonoBehaviour
    {
        Vector3 origin;bool collected;
        void Start(){origin=transform.position;}
        void Update()
        {
            transform.Rotate(0,100*Time.deltaTime,0,Space.World);transform.position=origin+Vector3.up*Mathf.Sin(Time.time*2.6f+origin.x)*.18f;
            if(!collected && MeadowGame.Instance && MeadowGame.Instance.player && Vector3.Distance(transform.position,MeadowGame.Instance.player.transform.position+Vector3.up)<1.45f)
            {collected=true;MeadowGame.Instance.Collect();gameObject.SetActive(false);}
        }
    }
}
