using UnityEngine;
namespace SkyMeadow
{
    [RequireComponent(typeof(CharacterController))]
    public class MeadowPlayer : MonoBehaviour
    {
        public Transform view, visual;
        public float speed=8, sprintSpeed=13, jumpSpeed=14, gravity=30;
        CharacterController motor; Vector3 velocity,dashDirection; float coyote,buffer,dashTime; int jumps;bool dashed;
        public Vector3 Checkpoint { get; private set; }
        public bool Grounded => motor && motor.isGrounded;
        public float MoveAmount { get; private set; }
        public bool Sprinting { get; private set; }
        public bool Dashing => dashTime>0;
        public float VerticalSpeed => velocity.y;
        public int JumpCount => jumps;
        void Start(){motor=GetComponent<CharacterController>();Checkpoint=transform.position;}
        void Update()
        {
            if(MeadowGame.Paused)return;
            if(Input.GetKeyDown(KeyCode.Escape)){Cursor.lockState=CursorLockMode.None;Cursor.visible=true;}
            if(Input.GetMouseButtonDown(0)){Cursor.lockState=CursorLockMode.Locked;Cursor.visible=false;}
            if(Input.GetKeyDown(KeyCode.R))Respawn();
            float dt=Time.deltaTime;
            if(motor.isGrounded)
            {
                coyote=.13f;jumps=0;dashed=false;if(velocity.y<0)velocity.y=-3;
                if(transform.position.y<MeadowWorld.Height(transform.position.x,transform.position.z)+3)Checkpoint=transform.position+Vector3.up*.1f;
            }
            else coyote-=dt;
            if(Input.GetKeyDown(KeyCode.Space))buffer=.14f;else buffer-=dt;
            if(buffer>0 && (jumps<2))
            {if(jumps==0 && coyote<=0)jumps=1;velocity.y=jumpSpeed;jumps++;buffer=0;}
            if(Input.GetKeyUp(KeyCode.Space) && velocity.y>5)velocity.y*=.65f;
            Vector2 input=new Vector2((Input.GetKey(KeyCode.D)||Input.GetKey(KeyCode.RightArrow)?1:0)-(Input.GetKey(KeyCode.A)||Input.GetKey(KeyCode.LeftArrow)?1:0),(Input.GetKey(KeyCode.W)||Input.GetKey(KeyCode.UpArrow)?1:0)-(Input.GetKey(KeyCode.S)||Input.GetKey(KeyCode.DownArrow)?1:0));
            input=Vector2.ClampMagnitude(input,1);Vector3 forward=view?Vector3.ProjectOnPlane(view.forward,Vector3.up).normalized:Vector3.forward;
            MoveAmount=input.magnitude;Sprinting=Input.GetKey(KeyCode.LeftShift);
            Vector3 right=Vector3.Cross(Vector3.up,forward),move=forward*input.y+right*input.x;
            if(move.sqrMagnitude>.01f)transform.rotation=Quaternion.Slerp(transform.rotation,Quaternion.LookRotation(move),dt*14);
            if(Input.GetKeyDown(KeyCode.E) && !dashed){dashed=true;dashTime=.22f;dashDirection=move.sqrMagnitude>.01f?move:transform.forward;}
            velocity.y-=gravity*dt;
            Vector3 horizontal=move*(Input.GetKey(KeyCode.LeftShift)?sprintSpeed:speed);
            if(dashTime>0){dashTime-=dt;horizontal=dashDirection*24;velocity.y=0;}
            var flags=motor.Move((horizontal+Vector3.up*velocity.y)*dt);if((flags&CollisionFlags.Above)!=0 && velocity.y>0)velocity.y=0;
            if(visual && !GetComponent<MeadowCharacterAnimation>()){float bob=motor.isGrounded?Mathf.Sin(Time.time*18)*input.magnitude*.055f:0;visual.localPosition=Vector3.up*bob;visual.localRotation=Quaternion.Euler(input.y*5,0,-input.x*7);}
            if(transform.position.y < -20 || Mathf.Abs(transform.position.x)>122 || Mathf.Abs(transform.position.z)>122)Respawn();
        }
        public void Respawn(){motor.enabled=false;transform.position=Checkpoint;motor.enabled=true;velocity=Vector3.zero;dashTime=0;jumps=0;dashed=false;}
    }
}
