using UnityEngine;
namespace SkyMeadow
{
    public class MeadowCamera : MonoBehaviour
    {
        public Transform target;public float distance=8;float yaw,pitch=22;
        void LateUpdate()
        {
            if(!target)return;
            if(!MeadowGame.Paused && (Cursor.lockState==CursorLockMode.Locked||Input.GetMouseButton(1)))
            {yaw+=Input.GetAxis("Mouse X")*3;pitch=Mathf.Clamp(pitch-Input.GetAxis("Mouse Y")*2, -20,70);}
            distance=Mathf.Clamp(distance-Input.mouseScrollDelta.y,3,15);
            Vector3 focus=target.position+Vector3.up*1.5f;
            Vector3 direction=Quaternion.Euler(pitch,yaw,0)*Vector3.back;
            float safeDistance=distance;
            if(Physics.SphereCast(focus,.22f,direction,out RaycastHit hit,distance,~(1<<2),QueryTriggerInteraction.Ignore))safeDistance=Mathf.Max(.5f,hit.distance-.2f);
            transform.position=focus+direction*safeDistance;transform.LookAt(focus);
        }
    }
}
