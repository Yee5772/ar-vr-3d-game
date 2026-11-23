using UnityEngine;

namespace SkyMeadow
{
    // A lightweight procedural skeleton for the supplied unrigged A-pose model.
    // Rotations drive skin weights rather than moving a rigid character as one piece.
    public class MeadowCharacterAnimation : MonoBehaviour
    {
        public Transform[] bones;
        MeadowPlayer player;
        float phase, movement, landing;
        bool wasGrounded;
        public string State { get; private set; } = "Idle";
        void Awake(){player=GetComponent<MeadowPlayer>();}
        void LateUpdate()
        {
            if(MeadowGame.Paused || bones==null || bones.Length<16)return;
            bool ground=player.Grounded;
            if(ground && !wasGrounded)landing=1;
            wasGrounded=ground;landing=Mathf.MoveTowards(landing,0,Time.deltaTime*5);
            movement=Mathf.MoveTowards(movement,player.MoveAmount,Time.deltaTime*7);
            phase+=Time.deltaTime*(player.Sprinting?13:9)*movement;
            Pose(movement,phase,ground,player.VerticalSpeed,player.Dashing,player.Sprinting,landing,Time.deltaTime);
        }

        public void Pose(float move,float cycle,bool grounded,float vertical,bool dash,bool sprint,float land,float dt)
        {
            State=dash?"Dash":!grounded?(vertical>0?"Jump":"Fall"):land>.1f?"Land":move>.1f?(sprint?"Run":"Walk"):"Idle";
            float stride=Mathf.Sin(cycle)*move,opposite=-stride;
            float amplitude=sprint?38:27;
            float leftThigh=stride*amplitude,rightThigh=opposite*amplitude;
            float leftKnee=Mathf.Max(0,-stride)*(sprint?58:42),rightKnee=Mathf.Max(0,stride)*(sprint?58:42);
            float leftArm=-stride*(sprint?36:24),rightArm=stride*(sprint?36:24);
            float armsOut=0,lean=move*(sprint?10:5),crouch=land*.09f;
            if(!grounded)
            {
                leftThigh=vertical>0?-28:-12;rightThigh=vertical>0?-18:10;
                leftKnee=vertical>0?55:24;rightKnee=vertical>0?40:18;
                leftArm=rightArm=vertical>0?-45:-15;armsOut=vertical>0?12:28;
            }
            if(dash){lean=48;leftThigh=15;rightThigh=-30;leftKnee=65;rightKnee=25;leftArm=rightArm=35;armsOut=-8;}
            Rotate(0,new Vector3(lean,0,-stride*2),dt);
            Rotate(1,new Vector3(-lean*.25f,stride*4,0),dt);
            Rotate(2,new Vector3(-lean*.4f,-stride*2,0),dt);
            Rotate(3,new Vector3(leftArm,0,armsOut),dt);
            Rotate(4,new Vector3(-10-move*15,0,0),dt);Rotate(5,Vector3.zero,dt);
            Rotate(6,new Vector3(rightArm,0,-armsOut),dt);
            Rotate(7,new Vector3(-10-move*15,0,0),dt);Rotate(8,Vector3.zero,dt);
            Rotate(9,new Vector3(leftThigh,0,0),dt);Rotate(10,new Vector3(leftKnee,0,0),dt);
            Rotate(11,new Vector3(-leftKnee*.3f,0,0),dt);
            Rotate(12,new Vector3(rightThigh,0,0),dt);Rotate(13,new Vector3(rightKnee,0,0),dt);
            Rotate(14,new Vector3(-rightKnee*.3f,0,0),dt);Rotate(15,Vector3.zero,dt);
            var visual=player?player.visual:bones[0].parent;
            if(visual)visual.localPosition=Vector3.up*((grounded?Mathf.Abs(Mathf.Cos(cycle))*move*.025f:0)-crouch+Mathf.Sin(Time.time*2)*.004f*(1-move));
        }
        void Rotate(int index,Vector3 angles,float dt)
        {bones[index].localRotation=Quaternion.Slerp(bones[index].localRotation,Quaternion.Euler(angles),1-Mathf.Exp(-14*dt));}
    }
}
