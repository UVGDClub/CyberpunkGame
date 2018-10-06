using UnityEngine.UI;
using System;
using UnityEngine;

using UnityEngine.Networking;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif
/*
 * AUTHOR'S NOTES:
 * 
 * Naming Shorthand:
 * If a variable ends with: A, it is short for:B
 * 		A	|	B
 * --------------------------
 * 		T	| 	Threshold 	(Value at which something changes)
 * 		M	| 	Multiplier 	(Used to scale things. Usually a value between 0 and 1)
 * 		H	|	Horizontal 	(Axis)
 * 		V	|	Vertical	(Axis)
 * 		G	|	Ground		(Direction)
 * 		C	|	Ceiling		(Direction)
 * 		L	|	Left		(Direction)
 * 		R	|	Right		(Direction)
*/

[System.Serializable]
public class ProtoPlayer : FighterChar
{        	

	//############################################################################################################################################################################################################
	// OBJECT REFERENCES
	//###########################################################################################################################################################################
	#region OBJECT REFERENCES
	[Header("Player Components:")]
	[SerializeField][ReadOnlyAttribute] private Text o_Speedometer;      			// Reference to the speed indicator (dev tool).
	[SerializeField][ReadOnlyAttribute] private Camera o_MainCamera;				// Reference to the main camera.
	[SerializeField][ReadOnlyAttribute] private Transform o_MainCameraTransform;	// Reference to the main camera's parent's transform, used to move it.
	#endregion
	//##########################################################################################################################################################################
	// PLAYER INPUT VARIABLES
	//###########################################################################################################################################################################
	#region PLAYERINPUT
	[Header("Player Input:")]
	[SerializeField] public float i_DoubleTapDelayTime; // How short the time between presses must be to count as a doubletap.
	public int inputBufferSize = 2;
	[SerializeField] public Queue<FighterState> inputBuffer;
	#endregion
	//############################################################################################################################################################################################################
	// DEBUGGING VARIABLES
	//##########################################################################################################################################################################
	#region DEBUGGING
	[SerializeField]private float d_LastFrameSpeed;
	[SerializeField]private float d_DeltaV;
	#endregion
	//############################################################################################################################################################################################################
	// VISUAL&SOUND VARIABLES
	//###########################################################################################################################################################################
	#region VISUALS&SOUND
	[SerializeField] private Vector3 v_CamWhiplashAmount;
	[SerializeField] private Vector3 v_CamWhiplashRecovery;
	[SerializeField] private float v_CamWhiplashM = 1;
	[SerializeField] public float v_CameraZoomLevel = 12;
	[SerializeField][ReadOnlyAttribute] public float v_CameraFinalSize;
	[SerializeField] private float v_CameraScrollZoom = 12;
	[SerializeField] private float v_CameraZoomLerp = 12;	
	#endregion 
	//############################################################################################################################################################################################################
	// GAMEPLAY VARIABLES
	//###########################################################################################################################################################################
	#region GAMEPLAY VARIABLES
	#endregion 	
	//############################################################################################################################################################################################################
	// NETWORKING VARIABLES
	//###########################################################################################################################################################################
	#region NETWORKING VARIABLES
	[SerializeField]private bool sceneIsReady = false;
	#endregion 
	//########################################################################################################################################
	// CORE FUNCTIONS
	//########################################################################################################################################
	#region CORE FUNCTIONS
	protected override void Awake()
	{
		inputBuffer = new Queue<FighterState>();
		isAPlayer = true;
		//FighterState.DevMode = true;
		FighterAwake();
	}

	//void OnDestroy()
	//{
	//	SceneManager.sceneLoaded -= SceneLoadPlayer;
	//}

 //   void OnEnable()
 //   {
	//	SceneManager.sceneLoaded += SceneLoadPlayer;
 //   }

 //   void OnDisable()
 //   {
	//	SceneManager.sceneLoaded -= SceneLoadPlayer;
 //   }

	protected void SceneLoadPlayer(Scene scene, LoadSceneMode mode)
	{
		sceneIsReady = true;
		//if(!isLocalPlayer||!isClient){return;}
		print("Executing post-scenelaunch player code!");
		o_MainCamera = GameObject.Find("Main Camera").GetComponent<Camera>();
		v.defaultCameraMode = 0;
		o_MainCameraTransform = o_MainCamera.transform.parent.transform;
		o_MainCameraTransform.SetParent(this.transform);
		o_MainCameraTransform.localPosition = new Vector3(0, 0, -10f);
    
	}

	protected void Start()
	{
		this.FighterState.FinalPos = this.transform.position;
        //if(SceneManager.GetActiveScene().isLoaded)
        //{
        //	SceneLoadPlayer(SceneManager.GetActiveScene(), LoadSceneMode.Single);
        //}
        SceneLoadPlayer(SceneManager.GetActiveScene(), LoadSceneMode.Single);

    }

    protected override void FixedUpdate()
	{
		if(!sceneIsReady){return;}

		d.tickCounter++; // Used for debug purposes, to determine what happened on what frame with print statements.
		d.tickCounter = (d.tickCounter > 60) ? 0 : d.tickCounter; // Rolls back to zero when hitting 60

		FixedUpdateProcessInput();

		if(k_IsKinematic)
		{
			FixedUpdateKinematic();	//If the player is in kinematic mode, physics are disabled while animations are played.
		}
		else
		{
			FixedUpdatePhysics(); // This is where all movement, collision, etc happen.
		}

		FixedUpdateLogic();			// Deals with variables such as health
		//FixedUpdateAnimation();		// Animates the character based on movement and input.
		FixedUpdateAudio();
    }

	protected override void Update()
	{
		//
		// Any player code.
		//
		UpdateAnimation();
		//
		// Local player code.
		//
		if(!sceneIsReady){return;}
		//if(!isLocalPlayer){return;}

		UpdatePlayerInput();
		UpdatePlayerAnimation();
	}

	protected override void LateUpdate()
	{
		if(!sceneIsReady){return;}
	}

	#endregion
	//###################################################################################################################################
	// CUSTOM FUNCTIONS
	//###################################################################################################################################
	#region CUSTOM FUNCTIONS


	//[Command(channel=1)]protected void CmdSendInput(FighterState[] theInput)
	//{
	//	RpcSendInput(theInput);
	//}

	//[ClientRpc(channel=1)]protected void RpcSendInput(FighterState[]  theInput)
	//{
	//	if(!isLocalPlayer)
	//	{
	//		foreach (FighterState i in theInput)
	//		{
	//			this.inputBuffer.Enqueue(i);
	//		}
	//	}
	//}

    protected override void FixedUpdateProcessInput() // FUPI
    {
        phys.worldImpact = false; //Placeholder??
        FighterState.Stance = 0;
        phys.kneeling = false;

        if (FighterState.LeftClickHold && (FighterState.DevMode || d.clickToKnockFighter))
        {
            FighterState.Vel += FighterState.PlayerMouseVector * 20 * Time.fixedDeltaTime;
            //print("Knocking the fighter.");
        }

        // Automatic input options.
        if (d.autoJump)
        {
            FighterState.JumpKeyPress = true;
        }
        if (d.autoLeftClick)
        {
            FighterState.LeftClickPress = true;
            FighterState.LeftClickHold = true;
        }
        if (FighterState.DevkeyTilde)
        {
            FighterState.DevkeyTilde = false;
        }
        if (FighterState.DevKey1)
        {
            FighterState.DevKey1 = false;
        }


        if (FighterState.DevKey2)
        {
            FighterState.DevKey2 = false;
        }

        if (FighterState.DevKey3)
        {
            FighterState.DevKey3 = false;
        }

        if (FighterState.DevKey4)
        {
            FighterState.DevKey4 = false;
        }
        if (FighterState.DevKey5)
        {
            v.defaultCameraMode++;
            string[] cameraNames = { "A - LockedClose", "B - AimGuided", "C - AimGuided SuperJump", "D - AimGuidedWhiplash", "E - Stationary", "F - SlowMo" };
            if (v.defaultCameraMode == 2)
            {
                v.defaultCameraMode++;
            }
            if (v.defaultCameraMode > 5)
            {
                v.defaultCameraMode = 0;
            }
            print("Camera mode " + cameraNames[v.defaultCameraMode]);
            FighterState.DevKey5 = false;
        }
        if (FighterState.DevKey6)
        {
            FighterState.DevKey6 = false;
        }
        if (FighterState.DevKey7)
        {
            FighterState.DevKey7 = false;
        }
        if (FighterState.DevKey8)
        {
            FighterState.DevKey8 = false;
        }
        if (FighterState.DevKey9)
        {
			FighterState.DevKey9 = false;
		}
		if(FighterState.DevKey10)
		{
			FighterState.DevKey10 = false;
		}
		if(FighterState.DevKey11)
		{
			FighterState.DevKey11 = false;
		}
		if(FighterState.DevKey12)
		{
			FighterState.DevKey12 = false;
			#if UNITY_EDITOR
			EditorApplication.isPaused = true;
			#endif
		}

		if(IsDisabled())
		{
			FighterState.RightClickPress = false;
			FighterState.RightClickRelease = false;
			FighterState.RightClickHold = false;
			FighterState.LeftClickPress = false;
			FighterState.LeftClickRelease = false;
			FighterState.LeftClickHold = false;
			FighterState.ShiftKeyPress = false;
			FighterState.UpKeyHold = false;
			FighterState.LeftKeyHold = false;
			FighterState.DownKeyHold = false;
			FighterState.RightKeyHold = false;
			FighterState.JumpKeyPress = false;
			FighterState.QKeyPress = false;
			FighterState.DisperseKeyPress = false;
		}

		//#################################################################################
		//### ALL INPUT AFTER THIS POINT IS DISABLED WHEN THE FIGHTER IS INCAPACITATED. ###
		//#################################################################################

		//Horizontal button pressing
		FighterState.PlayerMouseVector = FighterState.MouseWorldPos-Vec2(this.transform.position);
		if((FighterState.LeftKeyHold && FighterState.RightKeyHold) || !(FighterState.LeftKeyHold||FighterState.RightKeyHold))
		{
			//print("BOTH OR NEITHER");
			if(!(d.autoPressLeft||d.autoPressRight)|| IsDisabled())
			{
				CtrlH = 0;
			}
			else if(d.autoPressLeft)
			{
				CtrlH = -1;
			}
			else
			{
				CtrlH = 1;
			}
		}
		else if(FighterState.LeftKeyHold)
		{
			//print("LEFT");
			CtrlH = -1;
		}
		else
		{
			//print("RIGHT");
			CtrlH = 1;
		}

		if (CtrlH < 0) 
		{
			v.facingDirection = false; //true means right (the direction), false means left.
		} 
		else if (CtrlH > 0)
		{
			v.facingDirection = true; //true means right (the direction), false means left.
		}

		if(phys.airborne)
		{
			if(FighterState.Vel.x>0)
			{
				v.facingDirection = true;
			}
			else if(FighterState.Vel.x<0)
			{
				v.facingDirection = false;
			}
		}

		//Vertical button pressing
		if((FighterState.DownKeyHold && FighterState.UpKeyHold) || !(FighterState.UpKeyHold||FighterState.DownKeyHold))
		{
			//print("BOTH OR NEITHER");
			if(!(d.autoPressDown||d.autoPressUp)||IsDisabled())
			{
				CtrlV = 0;
			}
			else if(d.autoPressDown)
			{
				CtrlV = -1;
			}
			else
			{
				CtrlV = 1;
			}
		}
		else if(FighterState.DownKeyHold)
		{
			//print("LEFT");
			CtrlV = -1;
		}
		else
		{
			//print("RIGHT");
			CtrlV = 1;
		}

		if(CtrlV<0)
		{
			v.facingDirectionV = -1; //true means up (the direction), false means down.
		}
		else if(CtrlV>0)
		{
			v.facingDirectionV = 1; //true means up (the direction), false means down.
		}
		else
		{
			v.facingDirectionV = 0;	
		}
			
		//if(FighterState.JumpKeyPress&&(phys.grounded||phys.ceilinged||phys.leftWalled||phys.rightWalled))
		if(FighterState.JumpKeyPress)
		{
			FighterState.JumpKeyPress = false;
			if(m.jumpBufferG>0 || m.jumpBufferC>0 || m.jumpBufferL>0 || m.jumpBufferR>0)
			{
				Jump(CtrlH);
			}
		}

		//
		// Strand Jump key double-taps
		//

		if(FighterState.LeftKeyDoubleTapReady){FighterState.LeftKeyDoubleTapDelay += Time.unscaledDeltaTime;} 	// If player pressed key, time how long since it was pressed.
		if(FighterState.RightKeyDoubleTapReady){FighterState.RightKeyDoubleTapDelay += Time.unscaledDeltaTime;} // If player pressed key, time how long since it was pressed.
		if(FighterState.UpKeyDoubleTapReady){FighterState.UpKeyDoubleTapDelay += Time.unscaledDeltaTime;} 		// If player pressed key, time how long since it was pressed.
		if(FighterState.DownKeyDoubleTapReady){FighterState.DownKeyDoubleTapDelay += Time.unscaledDeltaTime;}	// If player pressed key, time how long since it was pressed.
	

		if(FighterState.LeftKeyDoubleTapDelay>i_DoubleTapDelayTime) // If over the time limit, next keypress won't count as a doubletap.
		{
			FighterState.LeftKeyDoubleTapReady = false;
		}
		if(FighterState.RightKeyDoubleTapDelay>i_DoubleTapDelayTime) // If over the time limit, next keypress won't count as a doubletap.
		{
			FighterState.RightKeyDoubleTapReady = false;
		}
		if(FighterState.UpKeyDoubleTapDelay>i_DoubleTapDelayTime) // If over the time limit, next keypress won't count as a doubletap.
		{
			FighterState.UpKeyDoubleTapReady = false;
		}
		if(FighterState.DownKeyDoubleTapDelay>i_DoubleTapDelayTime) // If over the time limit, next keypress won't count as a doubletap.
		{
			FighterState.DownKeyDoubleTapReady = false;
		}

		if(FighterState.LeftKeyPress) 
		{
			FighterState.RightKeyDoubleTapReady = false; // Other keys interrupt double taps.
			FighterState.UpKeyDoubleTapReady = false; // Other keys interrupt double taps.
			FighterState.DownKeyDoubleTapReady = false; // Other keys interrupt double taps.
			if(FighterState.LeftKeyDoubleTapReady) // If double tap, strand jump. If not, prime double tap.
			{
				FighterState.LeftKeyDoubleTapReady = false;
				//print("LeftKeyDoubleTapDelay "+FighterState.LeftKeyDoubleTapDelay);
			}
			else
			{
				FighterState.LeftKeyDoubleTapReady = true;
				FighterState.RightKeyDoubleTapReady = false;
			}
		}

		if(FighterState.RightKeyPress)
		{
			FighterState.LeftKeyDoubleTapReady = false; // Other keys interrupt double taps.
			FighterState.UpKeyDoubleTapReady = false; // Other keys interrupt double taps.
			FighterState.DownKeyDoubleTapReady = false; // Other keys interrupt double taps.
			if(FighterState.RightKeyDoubleTapReady) // If double tap, strand jump. If not, prime double tap.
			{
				FighterState.RightKeyDoubleTapReady = false;
				//print("RightKeyDoubleTapDelay "+FighterState.RightKeyDoubleTapDelay);
			}
			else
			{
				FighterState.RightKeyDoubleTapReady = true;
				FighterState.LeftKeyDoubleTapReady = false;
			}
		}

		if(FighterState.UpKeyPress)
		{
			FighterState.LeftKeyDoubleTapReady = false; // Other keys interrupt double taps.
			FighterState.RightKeyDoubleTapReady = false; // Other keys interrupt double taps.
			FighterState.DownKeyDoubleTapReady = false; // Other keys interrupt double taps.
			if(FighterState.UpKeyDoubleTapReady) // If double tap, strand jump. If not, prime double tap.
			{
				FighterState.UpKeyDoubleTapReady = false;
				//print("UpKeyDoubleTapDelay "+FighterState.UpKeyDoubleTapDelay);
			}
			else
			{
				FighterState.UpKeyDoubleTapReady = true;
				FighterState.DownKeyDoubleTapReady = false;
			}
		}
		if(FighterState.DownKeyPress)
		{
			FighterState.LeftKeyDoubleTapReady = false; // Other keys interrupt double taps.
			FighterState.RightKeyDoubleTapReady = false; // Other keys interrupt double taps.
			FighterState.UpKeyDoubleTapReady = false; // Other keys interrupt double taps.
			if(FighterState.DownKeyDoubleTapReady) // If double tap, strand jump. If not, prime double tap.
			{
				FighterState.DownKeyDoubleTapReady = false;
				//print("DownKeyDoubleTapDelay "+FighterState.DownKeyDoubleTapDelay);
			}
			else
			{
				FighterState.DownKeyDoubleTapReady = true;
				FighterState.UpKeyDoubleTapReady = false;
			}
		}

		if(FighterState.LeftKeyDoubleTapDelay>i_DoubleTapDelayTime) // If over the time limit, next keypress won't count as a doubletap.
		{
			FighterState.LeftKeyDoubleTapReady = false;
		}
		if(FighterState.RightKeyDoubleTapDelay>i_DoubleTapDelayTime) // If over the time limit, next keypress won't count as a doubletap.
		{
			FighterState.RightKeyDoubleTapReady = false;
		}
		if(FighterState.UpKeyDoubleTapDelay>i_DoubleTapDelayTime) // If over the time limit, next keypress won't count as a doubletap.
		{
			FighterState.UpKeyDoubleTapReady = false;
		}
		if(FighterState.DownKeyDoubleTapDelay>i_DoubleTapDelayTime) // If over the time limit, next keypress won't count as a doubletap.
		{
			FighterState.DownKeyDoubleTapReady = false;
		}

		// Once the input has been processed, set the press inputs to false so they don't run several times before being changed by update() again. 
	// FixedUpdate can run multiple times before Update refreshes, so a keydown input can be registered as true multiple times before update changes it back to false, instead of just the intended one time.
		FighterState.LeftClickPress = false; 	
		FighterState.RightClickPress = false;
		FighterState.QKeyPress = false;				
		FighterState.DisperseKeyPress = false;				
		FighterState.JumpKeyPress = false;				
		FighterState.LeftKeyPress = false;
		FighterState.ShiftKeyPress = false;
		FighterState.RightKeyPress = false;
		FighterState.UpKeyPress = false;
		FighterState.DownKeyPress = false;

		FighterState.LeftClickRelease = false; 	
		FighterState.RightClickRelease = false;			
		FighterState.LeftKeyRelease = false;
		FighterState.ShiftKeyRelease = false;
		FighterState.RightKeyRelease = false;
		FighterState.UpKeyRelease = false;
		FighterState.DownKeyRelease = false;

		FighterState.DevkeyTilde = false;				
		FighterState.DevKey1 = false;				
		FighterState.DevKey2  = false;				
		FighterState.DevKey3  = false;				
		FighterState.DevKey4  = false;				
		FighterState.DevKey5  = false;				
		FighterState.DevKey6  = false;				
		FighterState.DevKey7  = false;				
		FighterState.DevKey8  = false;				
		FighterState.DevKey9  = false;
		FighterState.DevKey10  = false;				
		FighterState.DevKey11  = false;				
		FighterState.DevKey12 = false;				
	}

	protected override void UpdatePlayerInput() //UPI
	{
		//if(!isLocalPlayer){return;}

		//
		// Individual keydown presses
		//

		FighterState.ScrollWheel = Input.GetAxis("Mouse ScrollWheel");

		if(Input.GetMouseButtonDown(0))
		{
			FighterState.LeftClickPress = true;
		}
		if(Input.GetMouseButtonDown(1))
		{
			FighterState.RightClickPress = true;
		}
		if(Input.GetButtonDown("Q"))
		{
			FighterState.QKeyPress = true;				
		}
		if(Input.GetButtonDown("Shift"))
		{
			FighterState.ShiftKeyPress = true;				
		}
		if(Input.GetButtonDown("Jump"))
		{
			FighterState.JumpKeyPress = true;				
		}
		if(Input.GetButtonDown("Left"))
		{
			FighterState.LeftKeyPress = true;
			FighterState.LeftKeyDoubleTapDelay = 0;
		}
		if(Input.GetButtonDown("Right"))
		{
			FighterState.RightKeyPress = true;
			FighterState.RightKeyDoubleTapDelay = 0;
		}
		if(Input.GetButtonDown("Up"))
		{
			FighterState.UpKeyPress = true;
			FighterState.UpKeyDoubleTapDelay = 0;
		}
		if(Input.GetButtonDown("Down"))
		{
			FighterState.DownKeyPress = true;
			FighterState.DownKeyDoubleTapDelay = 0;
		}
	
		//
		// Dev Keys
		//
		if(Input.GetButtonDown("Tilde"))
		{
			FighterState.DevkeyTilde = true;				
		}
		if(Input.GetButtonDown("F1"))
		{
			FighterState.DevKey1 = true;				
		}
		if(Input.GetButtonDown("F2"))
		{
			FighterState.DevKey2  = true;				
		}
		if(Input.GetButtonDown("F3"))
		{
			FighterState.DevKey3  = true;				
		}
		if(Input.GetButtonDown("F4"))
		{
			FighterState.DevKey4  = true;				
		}
		if(Input.GetButtonDown("F5"))
		{
			FighterState.DevKey5  = true;				
		}
		if(Input.GetButtonDown("F6"))
		{
			FighterState.DevKey6  = true;				
		}
		if(Input.GetButtonDown("F7"))
		{
			FighterState.DevKey7  = true;				
		}
		if(Input.GetButtonDown("F8"))
		{
			FighterState.DevKey8  = true;				
		}
		if(Input.GetButtonDown("F9"))
		{
			FighterState.DevKey9  = true;				
		}
		if(Input.GetButtonDown("F10"))
		{
			FighterState.DevKey10  = true;				
		}
		if(Input.GetButtonDown("F11"))
		{
			FighterState.DevKey11  = true;				
		}
		if(Input.GetButtonDown("F12"))
		{
			FighterState.DevKey12 = true;				
		}

		//
		// Key-Up releases
		//
		if(Input.GetMouseButtonUp(0))
			FighterState.LeftClickRelease = true;
		
		if(Input.GetMouseButtonUp(1))
			FighterState.RightClickRelease = true;
		
		if(Input.GetButtonUp("Left"))
			FighterState.LeftKeyRelease = true;
		
		if(Input.GetButtonUp("Right"))
			FighterState.RightKeyRelease = true;
		
		if(Input.GetButtonUp("Up"))
			FighterState.UpKeyRelease = true;
		
		if(Input.GetButtonUp("Down"))
			FighterState.DownKeyRelease = true;

		if(Input.GetButtonUp("Shift"))
			FighterState.ShiftKeyRelease = true;				

		//
		// Key Hold-Downs
		//
		FighterState.LeftKeyHold = Input.GetButton("Left");
		FighterState.RightKeyHold = Input.GetButton("Right");
		FighterState.UpKeyHold = Input.GetButton("Up");
		FighterState.DownKeyHold = Input.GetButton("Down");
		FighterState.LeftClickHold = Input.GetMouseButton(0);
		FighterState.RightClickHold = Input.GetMouseButton(1);

		//
		// Mouse position in world space
		//
		Vector3 mousePoint = o_MainCamera.ScreenToWorldPoint(Input.mousePosition);
		FighterState.MouseWorldPos = Vec2(mousePoint);

	}

	protected void UpdatePlayerAnimation() // UPA
	{
		v_CameraScrollZoom -= FighterState.ScrollWheel*1.5f;
		v_CameraScrollZoom = (v_CameraScrollZoom>15) ? 15 : v_CameraScrollZoom; // Clamp max
		v_CameraScrollZoom = (v_CameraScrollZoom<1) ? 1 : v_CameraScrollZoom; // Clamp min

		v.cameraMode = v.defaultCameraMode;
	

		switch(v.cameraMode)
		{
		case 0: 
			{
				CameraControlTypeA(); //Player-locked velocity size-reactive camera
				break;
			}
		case 1: 
			{
				CameraControlTypeB(); //Mouse Directed Camera
				break;
			}
		case 2:
			{
				CameraControlTypeC(); // SuperJump Camera
				break;
			}
		case 3:
			{
				CameraControlTypeD(); // SuperJump Camera
				break;
			}
		case 4:
			{
				CameraControlTypeE(); // Locked Map Location Camera
				break;
			}
		case 5:
			{
                CameraControlTypeA(); //Player-locked velocity size-reactive camera
                break;	
			}
		default:
			{
				throw new Exception("ERROR: CAMERAMODE UNDEFINED.");
			}
		}
		v_CameraFinalSize = o_MainCamera.orthographicSize;

		d_DeltaV = Math.Abs(d_LastFrameSpeed-FighterState.Vel.magnitude);
		d_LastFrameSpeed = FighterState.Vel.magnitude;


		//if(o_Speedometer != null)
		//{
		//	o_Speedometer.text = ""+Math.Round(FighterState.Vel.magnitude,0);
		//	//o_Speedometer.text = ""+Math.Round(d_DeltaV,0);
		//}
	}

	protected void CameraControlTypeA() //CCTA - locked cam
	{
		if(!o_MainCamera){return;}
		if(o_MainCameraTransform.parent==null)
		{
			o_MainCameraTransform.parent = this.transform;
			o_MainCameraTransform.localPosition = new Vector3(0, 0, -10);
		}

		#region zoom
		v_CameraZoomLevel = Mathf.Lerp(v_CameraZoomLevel, v_CameraScrollZoom, Time.deltaTime*10);
		o_MainCamera.orthographicSize = v_CameraZoomLevel;

		//o_MainCameraTransform.position = new Vector3(this.transform.position.x, this.transform.position.y, -10f);
		o_MainCameraTransform.localPosition = new Vector3(0, 0, -10f);
		//o_MainCamera.orthographicSize = 100f; // REMOVE THIS WHEN NOT DEBUGGING.

		#endregion
	}
		
	protected void CameraControlTypeB() //CCTB - simple aim cam
	{
		if(!o_MainCamera){return;}
		if(o_MainCameraTransform.parent==null)
		{
			o_MainCameraTransform.parent = this.transform;
			o_MainCameraTransform.localPosition = new Vector3(0, 0, -10);
		}

		float goalZoom;
		float mySpeed = FighterState.Vel.magnitude;
		float camSpeedModifier = 0;
		if((0.15f*mySpeed)>=5f)
		{
			camSpeedModifier = (0.15f*mySpeed)-5f;
		}

		if(v_CameraScrollZoom+camSpeedModifier >= 40f)
		{
			goalZoom = 40f;
		}
		else
		{
			goalZoom = v_CameraScrollZoom+camSpeedModifier;
		}
		v_CameraZoomLevel = Mathf.Lerp(v_CameraZoomLevel, goalZoom, Time.deltaTime*10);
		o_MainCamera.orthographicSize = v_CameraZoomLevel;


		float camAverageX = (FighterState.MouseWorldPos.x-this.transform.position.x)/3;
		float camAverageY = (FighterState.MouseWorldPos.y-this.transform.position.y)/3;


		Vector3 topRightEdge= new Vector3((1+v.cameraXLeashM)/2, (1+v.cameraYLeashM)/2, 0f);
		Vector3 theMiddle 	= new Vector3(0.5f, 0.5f, 0f);
		topRightEdge = o_MainCamera.ViewportToWorldPoint(topRightEdge);
		theMiddle = o_MainCamera.ViewportToWorldPoint(theMiddle);
		float xDistanceToEdge = topRightEdge.x-theMiddle.x;
		float yDistanceToEdge = topRightEdge.y-theMiddle.y;

		Vector3 topRightMax = new Vector3((1+v.cameraXLeashLim)/2, (1+v.cameraXLeashLim)/2, 0f);
		topRightMax = o_MainCamera.ViewportToWorldPoint(topRightMax);
		float xDistanceToMax = topRightMax.x-theMiddle.x;
		float yDistanceToMax = topRightMax.y-theMiddle.y;

		Vector3 camGoalLocation = new Vector3(camAverageX, camAverageY, -10f);

		o_MainCameraTransform.localPosition = Vector3.Lerp(o_MainCameraTransform.localPosition, camGoalLocation, 10*Time.unscaledDeltaTime); // CAMERA LERP TO POSITION. USUAL MOVEMENT METHOD.
	
	}

	protected void CameraControlTypeC() //CCTC - super jump cam
	{
		if(!o_MainCamera){return;}

		if(o_MainCameraTransform.parent==null)
		{
			o_MainCameraTransform.parent = this.transform;
			o_MainCameraTransform.localPosition = Vector3.zero;
		}
	
		float mySpeed = FighterState.Vel.magnitude;


		float goalZoom;
		float camSpeedModifier = 0;
		if((0.15f*mySpeed)>=5f)
		{
			camSpeedModifier = (0.15f*mySpeed)-5f;
		}

		if(v_CameraScrollZoom+camSpeedModifier >= 40f)
		{
			goalZoom = 40f;
		}
		else
		{
			goalZoom = v_CameraScrollZoom+camSpeedModifier;
		}

		v_CameraZoomLevel = Mathf.Lerp(v_CameraZoomLevel, goalZoom, Time.deltaTime*3);
		o_MainCamera.orthographicSize = v_CameraZoomLevel;

		float camAverageX = (FighterState.MouseWorldPos.x-this.transform.position.x)/3;
		float camAverageY = (FighterState.MouseWorldPos.y-this.transform.position.y)/3;

		Vector3 topRightEdge= new Vector3((1+v.cameraXLeashM)/2, (1+v.cameraYLeashM)/2, 0f);
		Vector3 theMiddle 	= new Vector3(0.5f, 0.5f, 0f);
		topRightEdge = o_MainCamera.ViewportToWorldPoint(topRightEdge);
		theMiddle = o_MainCamera.ViewportToWorldPoint(theMiddle);
		float xDistanceToEdge = topRightEdge.x-theMiddle.x;
		float yDistanceToEdge = topRightEdge.y-theMiddle.y;

		Vector3 topRightMax = new Vector3((1+v.cameraXLeashLim)/2, (1+v.cameraXLeashLim)/2, 0f);
		topRightMax = o_MainCamera.ViewportToWorldPoint(topRightMax);
		float xDistanceToMax = topRightMax.x-theMiddle.x;
		float yDistanceToMax = topRightMax.y-theMiddle.y;

		v_CamWhiplashAmount = Vector2.Lerp(v_CamWhiplashAmount, FighterState.Vel*v_CamWhiplashM, Time.unscaledDeltaTime);
		v_CamWhiplashRecovery = Vector2.Lerp(v_CamWhiplashRecovery, v_CamWhiplashAmount, Time.unscaledDeltaTime);

		float finalXPos = camAverageX-(v_CamWhiplashAmount.x-v_CamWhiplashRecovery.x);
		float finalYPos = camAverageY-(v_CamWhiplashAmount.y-v_CamWhiplashRecovery.y);

		//
		// The following block of code is for when the player hits the maximum bounds. The camera will instantly snap to the edge and won't go any further. Does not use lerp.
		//
		if(Mathf.Abs(finalXPos)>=xDistanceToMax)
		{
			if(finalXPos>0)
			{
				finalXPos = xDistanceToMax;
			}
			else
			{
				finalXPos = -xDistanceToMax;
			}
			//print("Too far horizontal!");
		}
		else
		{
			//print("Mathf.Abs(camAverageX) = "+Mathf.Abs(camAverageX)+" < "+xDistanceToMax+" = xDistanceToMax");
		}

		if(Mathf.Abs(finalYPos)>=yDistanceToMax)
		{
			//print("Too far vertical!");
			if(finalYPos>0)
			{
				finalYPos = yDistanceToMax;
			}
			else
			{
				finalYPos = -yDistanceToMax;
			}
		}
		else
		{
			//	print("Mathf.Abs(camAverageX) = "+Mathf.Abs(camAverageY)+" < "+yDistanceToMax+" = xDistanceToMax");
		}





		Vector3 camGoalLocation = new Vector3(finalXPos, finalYPos, -10f);

		o_MainCameraTransform.localPosition = Vector3.Lerp(o_MainCameraTransform.localPosition, camGoalLocation, 10*Time.unscaledDeltaTime); // CAMERA LERP TO POSITION. USUAL MOVEMENT METHOD.
	}
		
	protected void CameraControlTypeD() //CCTD - whiplash aimcam - Main camera style
	{
		if(!o_MainCamera){return;}
		if(o_MainCameraTransform.parent==null)
		{
			o_MainCameraTransform.parent = this.transform;
			o_MainCameraTransform.localPosition = Vector3.zero;
		}
		#region zoom
		float goalZoom;
		float mySpeed = FighterState.Vel.magnitude;
		float camSpeedModifier = 0;
		if((0.15f*mySpeed)>=5f)
		{
			camSpeedModifier = (0.15f*mySpeed)-5f;
		}

		if(v_CameraScrollZoom+camSpeedModifier >= 40f)
		{
			goalZoom = 40f;
		}
		else
		{
			goalZoom = v_CameraScrollZoom+camSpeedModifier;
		}
		v_CameraZoomLevel = Mathf.Lerp(v_CameraZoomLevel, goalZoom, Time.deltaTime*10);
		o_MainCamera.orthographicSize = v_CameraZoomLevel;

		#endregion
		#region position
		float camAverageX = (FighterState.MouseWorldPos.x-this.transform.position.x)/3;
		float camAverageY = (FighterState.MouseWorldPos.y-this.transform.position.y)/3;


		Vector3 topRightEdge= new Vector3((1+v.cameraXLeashM)/2, (1+v.cameraYLeashM)/2, 0f);
		Vector3 theMiddle 	= new Vector3(0.5f, 0.5f, 0f);
		topRightEdge = o_MainCamera.ViewportToWorldPoint(topRightEdge);
		theMiddle = o_MainCamera.ViewportToWorldPoint(theMiddle);
		float xDistanceToEdge = topRightEdge.x-theMiddle.x;
		float yDistanceToEdge = topRightEdge.y-theMiddle.y;

		Vector3 topRightMax = new Vector3((1+v.cameraXLeashLim)/2, (1+v.cameraXLeashLim)/2, 0f);
		topRightMax = o_MainCamera.ViewportToWorldPoint(topRightMax);
		float xDistanceToMax = topRightMax.x-theMiddle.x;
		float yDistanceToMax = topRightMax.y-theMiddle.y;

		//print("botLeftEdge: "+botLeftEdge);
		//print("topRightEdge: "+topRightEdge);
		//print("Player: "+this.transform.position);
		//print("theMiddle: "+theMiddle);
		//print("player: "+ +"\n lefted: "+botLeftEdge.x);
	
//		if(o_MainCameraTransform.position.x+camAverageX-xDistanceToEdge>this.transform.position.x) //If the edge of the proposed camera position is beyond the player, snap it back
//		{
//			print("Too far left!");
//			//camAverageX = this.transform.position.x+(xDistanceToEdge); //If it's outside of the leash zone, lock it to the edge.
//		}
//		if(o_MainCameraTransform.position.x+camAverageX+xDistanceToEdge<this.transform.position.x)
//		{
//			print("Too far Right!");
//			//camAverageX = this.transform.position.x-(xDistanceToEdge);
//		}
//
//		if(o_MainCameraTransform.position.y+camAverageY-yDistanceToEdge>this.transform.position.y) //If the edge of the proposed camera position is beyond the player, snap it back
//		{
//			print("Too far down!");
//			//camAverageY = this.transform.position.y+(yDistanceToEdge); //If it's outside of the leash zone, lock it to the edge.
//		}
//		if(o_MainCameraTransform.position.y+camAverageY+yDistanceToEdge<this.transform.position.y)
//		{
//			print("Too far up! player: "+this.transform.position.y+", edge: "+topRightEdge.y);
//			//camAverageY = this.transform.position.y-(yDistanceToEdge);
//		}
//

		v_CamWhiplashAmount = Vector2.Lerp(v_CamWhiplashAmount, FighterState.Vel*v_CamWhiplashM, Time.unscaledDeltaTime*2);

//		if(v_CamWhiplashAmount.magnitude>FighterState.Vel.magnitude*v_CamWhiplashM)
//		{
//			v_CamWhiplashAmount = FighterState.Vel*v_CamWhiplashM;
//		}
//
		v_CamWhiplashRecovery = Vector2.Lerp(v_CamWhiplashRecovery, v_CamWhiplashAmount, Time.unscaledDeltaTime*2);


		float finalXPos = camAverageX-(v_CamWhiplashAmount.x-v_CamWhiplashRecovery.x);
		float finalYPos = camAverageY-(v_CamWhiplashAmount.y-v_CamWhiplashRecovery.y);

		if(Mathf.Abs(finalXPos)>=xDistanceToMax)
		{
			if(finalXPos>0)
			{
				finalXPos = xDistanceToMax;
			}
			else
			{
				finalXPos = -xDistanceToMax;
			}
			//print("Too far horizontal!");
		}
		else
		{
			//print("Mathf.Abs(camAverageX) = "+Mathf.Abs(camAverageX)+" < "+xDistanceToMax+" = xDistanceToMax");
		}

		if(Mathf.Abs(finalYPos)>=yDistanceToMax)
		{
			//print("Too far vertical!");
			if(finalYPos>0)
			{
				finalYPos = yDistanceToMax;
			}
			else
			{
				finalYPos = -yDistanceToMax;
			}
		}
		else
		{
		//	print("Mathf.Abs(camAverageX) = "+Mathf.Abs(camAverageY)+" < "+yDistanceToMax+" = xDistanceToMax");
		}


		Vector3 camGoalLocation = new Vector3(finalXPos, finalYPos, -10f);

		o_MainCameraTransform.localPosition = Vector3.Lerp(o_MainCameraTransform.localPosition, camGoalLocation, 10*Time.unscaledDeltaTime); // CAMERA LERP TO POSITION. USUAL MOVEMENT METHOD.
		#endregion

		//
		// The following block of code is for when the player hits the maximum bounds. The camera will instantly snap to the edge and won't go any further. Does not use lerp.
		//


		//o_MainCamera.orthographicSize = 20f; // REMOVE THIS WHEN NOT DEBUGGING.

	}

	protected void CameraControlTypeE() // Scenic stationary camera
	{
		if(!o_MainCamera){return;}
		v_CameraZoomLevel = Mathf.Lerp(v_CameraZoomLevel, 20f, Time.deltaTime*10);
		o_MainCamera.orthographicSize = v_CameraZoomLevel;
		o_MainCameraTransform.parent = null;
		o_MainCameraTransform.position = new Vector3(-4, 263, -10f);
	}


	#endregion
	//###################################################################################################################################
	// PUBLIC FUNCTIONS
	//###################################################################################################################################
	#region PUBLIC FUNCTIONS
	#endregion
}