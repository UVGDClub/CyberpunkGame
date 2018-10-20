using UnityEngine.UI;
using System;
using UnityEngine;
using UnityEngine.Networking;
#if UNITY_EDITOR
using UnityEditor;
#endif
/*
 * AUTHOR'S NOTES:
 * 
 * FighterChar is the basis class for all humanoid combatants. NPC and Player classes extend this class. 
 * Most animation, collision code, and physics is conducted in this base class. Input is processed in FighterChar, but the input is recieved from respective sources set in NPC and Player.
 * A FighterChar, if spawned, would behave simply as an uncontrolled dummy. Spawn players or NPCs instead, and use FighterChar for any code that would affect both of them. 
 * 
 * Naming conventions:
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
 *
*/

[System.Serializable]
public class FighterChar : MonoBehaviour
{        
    [Header("Debug Variables:")]
    [Tooltip("Debug Variables")][SerializeField] public DebugVars d;

    [Header("Movement Variables:")]
    [Tooltip("Movement Variables")][SerializeField] public MovementVars m;

    [Header("Object References:")]
    [Tooltip("Object References")][SerializeField] public ObjectRefs o;

    [Header("Physics Variables:")]
    [Tooltip("Physics Variables")][SerializeField] public PhysicsVars phys;

    [Header("Audio/Visual Variables:")]
    [Tooltip("Audio/Visual Variables")][SerializeField] public AudioVisualVars v;

    //############################################################################################################################################################################################################
    // KINEMATIC VARIABLES
    //###########################################################################################################################################################################
    #region KINEMATIC VARIABLES
    [Header("Kinematic Components:")]
    [SerializeField] protected bool k_IsKinematic; 						//Dictates whether the player is moving in physical fighterchar mode or in some sort of specially controlled fashion, such as in cutscenes or strand jumps
    [SerializeField] protected int k_KinematicAnim; 					//Designates the kinematic animation being played.
    #endregion
    //############################################################################################################################################################################################################
    // OBJECT REFERENCES
    //###########################################################################################################################################################################
    #region OBJECT REFERENCES
    [Header("Prefab References:")]
    [SerializeField] public GameObject p_DebugMarker;           // Reference to a sprite prefab used to mark locations ingame during development.

    #endregion
    //############################################################################################################################################################################################################
    // PHYSICS&RAYCASTING
    //###########################################################################################################################################################################
    #region PHYSICS&RAYCASTING
    [Header("Physics&Raycasting:")]
    // Physics relies on four directional raycasts set up in a cross formation. The vertical raycasts are longer than the horizontal ones. 
    // This results in a diamond shaped hitbox. When multiple raycasts are contacting the ground, priority is chosen based on which one impacts the deepest, or, when equal, the angle of terrain.

    [SerializeField] public LayerMask m_TerrainMask;	// Mask used for terrain collisions.
    [SerializeField] public LayerMask m_FighterMask;	// Mask used for fighter collisions.

    [HideInInspector]public Transform m_GroundFoot; 			// Ground collider.
    [HideInInspector]public Vector2 m_GroundFootOffset; 		// Ground raycast endpoint.
    [ReadOnlyAttribute]public float m_GroundFootLength;		// Ground raycast length.

    [HideInInspector]public Transform m_CeilingFoot; 		// Ceiling collider, middle.
    [HideInInspector]public Vector2 m_CeilingFootOffset;		// Ceiling raycast endpoint.
    [ReadOnlyAttribute]public float m_CeilingFootLength;		// Ceiling raycast length.

    [HideInInspector]public Transform m_LeftSide; 			// LeftWall collider.
    [HideInInspector]public Vector2 m_LeftSideOffset;		// LeftWall raycast endpoint.
    [ReadOnlyAttribute]public float m_LeftSideLength;			// LeftWall raycast length.

    [HideInInspector]public Transform m_RightSide;  			// RightWall collider.
    [HideInInspector]public Vector2 m_RightSideOffset;		// RightWall raycast endpoint.
    [ReadOnlyAttribute]public float m_RightSideLength;			// RightWall raycast length.

    [ReadOnlyAttribute]public Vector2 m_GroundNormal;			// Vector holding the slope of Ground.
    [ReadOnlyAttribute]public Vector2 m_CeilingNormal;		// Vector holding the slope of Ceiling.
    [ReadOnlyAttribute]public Vector2 m_LeftNormal;			// Vector holding the slope of LeftWall.
    [ReadOnlyAttribute]public Vector2 m_RightNormal;			// Vector holding the slope of RightWall.

    [ReadOnlyAttribute]public RaycastHit2D[] directionContacts;

    #endregion
    //##########################################################################################################################################################################
    // FIGHTER INPUT VARIABLES
    //###########################################################################################################################################################################
    #region FIGHTERINPUT
    [Header("Networked Variables:")]
    [SerializeField] protected FighterState FighterState;// Struct holding all networked fighter info.
    [Header("Input:")]
    protected int CtrlH; 													// Tracks horizontal keys pressed. Values are -1 (left), 0 (none), or 1 (right). 
    protected int CtrlV; 													// Tracks vertical keys pressed. Values are -1 (down), 0 (none), or 1 (up).
    #endregion
    //############################################################################################################################################################################################################
    // GAMEPLAY VARIABLES
    //###########################################################################################################################################################################
    #region GAMEPLAY VARIABLES
    [Header("Gameplay:")]
    [SerializeField] public bool g_FighterCollision = true;				// While true, this fighter will collide with other fighters
    protected bool isAPlayer;
    protected bool n_Jumped;
    #endregion
    //########################################################################################################################################
    // CORE FUNCTIONS
    //########################################################################################################################################
    #region CORE FUNCTIONS
    protected virtual void Awake()
    {
        FighterAwake();
    }

    protected virtual void FixedUpdate()
    {
        //EditorApplication.isPaused = true; //Used for debugging.
        d.tickCounter++;
        d.tickCounter = (d.tickCounter > 60) ? 0 : d.tickCounter; // Rolls back to zero when hitting 60
        if(k_IsKinematic)
        {
            FixedUpdateKinematic();	
        }
        else
        {
            FixedUpdateProcessInput();
            FixedUpdatePhysics();
        }
        FixedUpdateLogic();
        //FixedUpdateAnimation();
        FixedUpdateAudio();
    }

    protected virtual void Update()
    {
        UpdatePlayerInput();
        UpdateAnimation();
    }

    protected virtual void LateUpdate()
    {
        // Declared for override purposes
    }


    #endregion
    //###################################################################################################################################
    // CUSTOM FUNCTIONS
    //###################################################################################################################################
    #region CUSTOM FUNCTIONS

    protected void FighterAwake()
    {
        //v.terrainType = new string[]{ "Concrete", "Concrete", "Concrete", "Concrete" };
        directionContacts = new RaycastHit2D[4];

        FighterState.Dead = false;						// True when the fighter's health reaches 0 and they die.
        Vector2 fighterOrigin = new Vector2(this.transform.position.x, this.transform.position.y);

        o.spriteTransform = transform.Find("SpriteTransform");
        o.anim = o.spriteTransform.GetComponentInChildren<Animator>();
        o.spriteRenderer = o.spriteTransform.GetComponent<SpriteRenderer>();
        o.rigidbody2D = GetComponent<Rigidbody2D>();

        m_GroundFoot = transform.Find("MidFoot");
        d.groundLine = m_GroundFoot.GetComponent<LineRenderer>();
        m_GroundFootOffset.x = m_GroundFoot.position.x-fighterOrigin.x;
        m_GroundFootOffset.y = m_GroundFoot.position.y-fighterOrigin.y;
        m_GroundFootLength = m_GroundFootOffset.magnitude;

        m_CeilingFoot = transform.Find("CeilingFoot");
        d.ceilingLine = m_CeilingFoot.GetComponent<LineRenderer>();
        m_CeilingFootOffset.x = m_CeilingFoot.position.x-fighterOrigin.x;
        m_CeilingFootOffset.y = m_CeilingFoot.position.y-fighterOrigin.y;
        m_CeilingFootLength = m_CeilingFootOffset.magnitude;

        m_LeftSide = transform.Find("LeftSide");
        d.leftSideLine = m_LeftSide.GetComponent<LineRenderer>();
        m_LeftSideOffset.x = m_LeftSide.position.x-fighterOrigin.x;
        m_LeftSideOffset.y = m_LeftSide.position.y-fighterOrigin.y;
        m_LeftSideLength = m_LeftSideOffset.magnitude;

        m_RightSide = transform.Find("RightSide");
        d.rightSideLine = m_RightSide.GetComponent<LineRenderer>();
        m_RightSideOffset.x = m_RightSide.position.x-fighterOrigin.x;
        m_RightSideOffset.y = m_RightSide.position.y-fighterOrigin.y;
        m_RightSideLength = m_RightSideOffset.magnitude;

        d.debugLine = GetComponent<LineRenderer>();

        phys.lastSafePosition = new Vector2(0,0);
        phys.remainingMovement = new Vector2(0,0);
        phys.remainingVelM = 1f;

        if(!(d.showVelocityIndicator||FighterState.DevMode)){
            d.debugLine.enabled = false;
        }

        if(!(d.showContactIndicators||FighterState.DevMode))
        {
            d.ceilingLine.enabled = false;
            d.groundLine.enabled = false;
            d.rightSideLine.enabled = false;
            d.leftSideLine.enabled = false;
        }
    }

    protected virtual void FixedUpdatePhysics() //FUP
    {
        this.transform.position = FighterState.FinalPos;
        phys.distanceTravelled = Vector2.zero;

        phys.initialVel = FighterState.Vel;
        v.wallSliding = false; // Set to false, and changed to true in WallTraction().



        if(phys.grounded)
        {//Locomotion!
            Traction(CtrlH, CtrlV);
            m.airborneDelayTimer = m.airborneDelay;
            v.primarySurface = 0;
            v.truePrimarySurface = 0;
        }
        else if(phys.leftWalled)
        {//Wallsliding!
            //print("Walltraction!");
            WallTraction(CtrlH, CtrlV, m_LeftNormal);
            m.airborneDelayTimer = m.airborneDelay;
            v.primarySurface = 2;
            v.truePrimarySurface = 2;
        }
        else if(phys.rightWalled)
        {//Wallsliding!
            WallTraction(CtrlH, CtrlV, m_RightNormal);
            m.airborneDelayTimer = m.airborneDelay;
            v.primarySurface = 3;
            v.truePrimarySurface = 3;
        }
//		else if(phys.ceilinged)
//		{
//			WallTraction(CtrlH, m_CeilingNormal);
//		}
        else if(d.gravityEnabled)
        { // Airborne with gravity!
            if(m.airborneDelayTimer>0)
            {
                m.airborneDelayTimer -= Time.fixedDeltaTime;
            }
            else
            {			
                v.primarySurface = -1;
            }
            v.truePrimarySurface = -1;
            AirControl(CtrlH);
            FighterState.Vel = new Vector2 (FighterState.Vel.x, FighterState.Vel.y - 1f);
        }	
            

        d.errorDetectingRecursionCount = 0; //Used for WorldCollizion(); (note: colliZion is used to help searches for the keyword 'collision' by filtering out extraneous matches)

        //print("Velocity before Collizion: "+FighterState.Vel);
        //print("Position before Collizion: "+this.transform.position);

        phys.remainingVelM = 1f;
        phys.remainingMovement = FighterState.Vel*Time.fixedDeltaTime;
        Vector2 startingPos = this.transform.position;

        WorldCollision();

        //print("Per frame velocity at end of Collizion() "+FighterState.Vel*Time.fixedDeltaTime);
        //print("Velocity at end of Collizion() "+FighterState.Vel);
        //print("Per frame velocity at end of updatecontactnormals "+FighterState.Vel*Time.fixedDeltaTime);
        //print("phys.remainingMovement after collision: "+phys.remainingMovement);

        phys.distanceTravelled = new Vector2(this.transform.position.x-startingPos.x,this.transform.position.y-startingPos.y);
        //print("phys.distanceTravelled: "+phys.distanceTravelled);
        //print("phys.remainingMovement: "+phys.remainingMovement);
        //print("phys.remainingMovement after removing phys.distanceTravelled: "+phys.remainingMovement);

        if(phys.initialVel.magnitude>0)
        {
            phys.remainingVelM = (((phys.initialVel.magnitude*Time.fixedDeltaTime)-phys.distanceTravelled.magnitude)/(phys.initialVel.magnitude*Time.fixedDeltaTime));
        }
        else
        {
            phys.remainingVelM = 1f;
        }

        //print("phys.remainingVelM: "+phys.remainingVelM);
        //print("movement after distance travelled: "+phys.remainingMovement);
        //print("Speed this frame: "+FighterState.Vel.magnitude);

        phys.remainingMovement = FighterState.Vel*phys.remainingVelM*Time.fixedDeltaTime;

        //print("Corrected remaining movement: "+phys.remainingMovement);

        Vector2 deltaV = FighterState.Vel-phys.initialVel;
        phys.IGF = deltaV.magnitude;
        phys.CGF += phys.IGF;
        if(phys.CGF>=1){phys.CGF --;}
        if(phys.CGF>=10){phys.CGF -= (phys.CGF/10);}

        if(phys.worldImpact)
        {
           // Put falldamage or other gamelogic code here.
        }

        this.transform.position = new Vector2(this.transform.position.x+phys.remainingMovement.x, this.transform.position.y+phys.remainingMovement.y);

        UpdateContactNormals(true);

        FighterState.FinalPos = this.transform.position;

        if(FighterState.DevMode&&d.sendCollisionMessages)
        {
            DebugUCN();
        }

        //print("Per frame velocity at end of physics frame: "+FighterState.Vel*Time.fixedDeltaTime);
        //print("phys.remainingMovement at end of physics frame: "+phys.remainingMovement);
        //print("Pos at end of physics frame: "+this.transform.position);
        //print("##############################################################################################");
        //print("FinaL Pos: " + this.transform.position);
        //print("FinaL Vel: " + FighterState.Vel);
        //print("Speed at end of frame: " + FighterState.Vel.magnitude);

    }

    protected virtual void FixedUpdateProcessInput() //FUPI
    {
        phys.worldImpact = false;
        phys.kneeling = false;

        FighterState.PlayerMouseVector = FighterState.MouseWorldPos-Vec2(this.transform.position);
        if(FighterState.LeftClickPress&&(FighterState.DevMode||d.clickToKnockFighter))
        {
            FighterState.Vel += FighterState.PlayerMouseVector*10;
            print("Leftclick detected");
            FighterState.LeftClickPress = false;
        }

        // Once the input has been processed, set the press inputs to false so they don't run several times before being changed by update() again. 
        // FixedUpdate can run multiple times before Update refreshes, so a keydown input can be registered as true multiple times before update changes it back to false, instead of just the intended one time.
        FighterState.LeftClickPress = false; 	
        FighterState.RightClickPress = false;
        FighterState.QKeyPress = false;
        FighterState.ShiftKeyPress = false;
        FighterState.DisperseKeyPress = false;				
        FighterState.JumpKeyPress = false;				
        FighterState.LeftKeyPress = false;
        FighterState.RightKeyPress = false;
        FighterState.UpKeyPress = false;
        FighterState.DownKeyPress = false;
    }

    protected virtual void FixedUpdateKinematic() //FUK
    { // Use this to bypass the conventional physics and build your own. Useful for situations when the normal player movement doesn't apply, such as during a grappling hook swing or scripted sequence.
        switch (k_KinematicAnim)
        {
        case -1:
            {
                k_IsKinematic = false;
                break;
            }
        default:
            {
                k_IsKinematic = false;
                break;
            }
        }
    }

    protected virtual void FixedUpdateLogic() //FUL
    {
    }

    protected virtual void UpdateAnimation()
    {
    }

    protected virtual void FixedUpdateAudio() // FU
    {
    }

    //protected virtual void FixedUpdateAnimation() //FUA
    //{
        
    //    v.sliding = false;
    //    o.anim.SetBool("WallSlide", false);
    //    o.anim.SetBool("Crouch", false);
            
    //    if(v.primarySurface==0)
    //        FixedGroundAnimation();
    //    else if(v.primarySurface==1)
    //    {} 
    //    else if(v.primarySurface>=2)
    //        FixedWallAnimation();
    //    else
    //        FixedAirAnimation();

    //    #region sprite rotation code

    //    float spriteAngle;
    //    float testAngle = 0;

    //    if(v.primarySurface == 0)
    //    {
    //        spriteAngle = Get2DAngle(Perp(m_GroundNormal));
    //    }
    //    else if(v.primarySurface == 1)
    //    {
    //        spriteAngle = Get2DAngle(Perp(m_CeilingNormal));
    //    }
    //    else if(v.primarySurface == 2)
    //    {
    //        spriteAngle = Get2DAngle(Perp(m_LeftNormal));
    //    }
    //    else if(v.primarySurface == 3)
    //    {
    //        spriteAngle = Get2DAngle(Perp(m_RightNormal));
    //    }
    //    else
    //    {
    //        spriteAngle = Get2DAngle(GetVelocity(), 0);
    //        testAngle = spriteAngle;

    //        if(GetVelocity().y<0)
    //        {
    //            if(spriteAngle>0)
    //            {
    //                spriteAngle = -180+spriteAngle;
    //            }
    //            else
    //            {
    //                spriteAngle = 180+spriteAngle;
    //            }
    //            spriteAngle *= 0.5f;
    //        }


    //        //print("spriteAngle: "+spriteAngle);
    //        float angleScaling = 1;

    //        angleScaling = Mathf.Abs(GetVelocity().y/50);//*Mathf.Abs(GetVelocity().y/20); // Parabola approaching zero at y = 0, and extending to positive infinity on either side.

    //        angleScaling = (angleScaling>1) ? 1 : angleScaling; // Clamp at 1.

      
    //        //print("testAngle: "+testAngle+", finalangle: "+(surfaceLeanM*spriteAngle));
    //    }
        
    //    v.leanAngle = spriteAngle; //remove this and enable lerp.
    //    Quaternion finalAngle = new Quaternion();
    //    finalAngle.eulerAngles = new Vector3(0,0, v.leanAngle);
    //    o.spriteTransform.localRotation = finalAngle;
    //    //
    //    // End of sprite transform positioning code
    //    //
    //    #endregion

    //    float relativeAimDirection = -Get2DAngle((Vector2)FighterState.MouseWorldPos-(Vector2)this.transform.position, -v.leanAngle);
    //    if(phys.kneeling&&!phys.airborne)
    //    {
    //        //print("RAD = "+relativeAimDirection+", LeanAngle = "+v.leanAngle);
    //        if(this.isAPlayer)
    //        {
    //            if(relativeAimDirection < 0)
    //            {
    //                v.facingDirection = false;
    //            }
    //            else
    //            {
    //                v.facingDirection = true;
    //            }
    //        }
    //    }


    //    //
    //    // Debug collision visualization code.
    //    //
    //    if( isLocalPlayer )
    //    {
    //        if( FighterState.DevMode )
    //        {
    //            Quaternion debugQuaternion = new Quaternion();
    //            debugQuaternion.eulerAngles = new Vector3(0, 0, testAngle);
    //        }
    //    }
    //    Vector3[] debugLineVector = new Vector3[3];

    //    debugLineVector[0].x = -phys.distanceTravelled.x;
    //    debugLineVector[0].y = -(phys.distanceTravelled.y+(m_GroundFootLength-m.maxEmbed));
    //    debugLineVector[0].z = 0f;

    //    debugLineVector[1].x = 0f;
    //    debugLineVector[1].y = -(m_GroundFootLength-m.maxEmbed);
    //    debugLineVector[1].z = 0f;

    //    debugLineVector[2].x = phys.remainingMovement.x;
    //    debugLineVector[2].y = (phys.remainingMovement.y)-(m_GroundFootLength-m.maxEmbed);
    //    debugLineVector[2].z = 0f;

    //    d.debugLine.SetPositions(debugLineVector);

    //    if(FighterState.Vel.magnitude >= m.tractionChangeT )
    //    {
    //        d.debugLine.endColor = Color.white;
    //        d.debugLine.startColor = Color.white;
    //    }   
    //    else
    //    {   
    //        d.debugLine.endColor = Color.blue;
    //        d.debugLine.startColor = Color.blue;
    //    }

    //    //
    //    // End of debug line code
    //    //

    //    //
    //    // Mecanim variable assignment. Last step of animation code.
    //    //

    //    if(!v.facingDirection) //If facing left
    //    {
    //        o.anim.SetBool("IsFacingRight", false);
    //        o.spriteTransform.localScale = new Vector3(-1f, 1f, 1f);
    //    }
    //    else
    //    {
    //        o.anim.SetBool("IsFacingRight", true);
    //        o.spriteTransform.localScale = new Vector3(1f, 1f, 1f);
    //    }
            
    //    if(phys.kneeling)
    //    {
    //        o.anim.SetFloat("AimAngle", relativeAimDirection);
    //    }
    //    else
    //    {
    //        float stoppingAngle = FighterState.Vel.magnitude*2;
    //        if(stoppingAngle>60)
    //            stoppingAngle = 60;
    //        if(stoppingAngle<30)
    //            stoppingAngle = 30;
            
    //        o.anim.SetFloat("AimAngle", stoppingAngle);
    //    }

    //    o.anim.SetInteger("PrimarySurface", v.primarySurface);
    //    o.anim.SetFloat("Speed", FighterState.Vel.magnitude);
    //    o.anim.SetFloat("hSpeed", Math.Abs(FighterState.Vel.x));
    //    o.anim.SetFloat("vSpeed", Math.Abs(FighterState.Vel.y));
    //    o.anim.SetFloat("hVelocity", FighterState.Vel.x);
    //    o.anim.SetFloat("vVelocity", FighterState.Vel.y);
    //    o.anim.SetInteger("Stance", FighterState.Stance);
    //    o.anim.SetFloat("ProjectileMode", Convert.ToSingle(v.highSpeedMode));

    //    if(v.triggerAtkHit)
    //    {
    //        v.triggerAtkHit = false;
    //        o.anim.SetBool("TriggerPunchHit", true);
    //    }
    //    else if(v.triggerRollOut)
    //    {
    //        v.triggerRollOut = false;
    //        o.anim.SetBool("TriggerRollOut", true);
    //    }
    //    else if(v.triggerFlinched)
    //    {
    //        v.triggerFlinched = false;
    //        o.anim.SetBool("TriggerFlinched", true);
    //    }


    //    float multiplier = 1; // Animation playspeed multiplier that increases with higher velocity

    //    if(FighterState.Vel.magnitude > 20.0f)
    //        multiplier = ((FighterState.Vel.magnitude - 20) / 20)+1;
        
    //    o.anim.SetFloat("Multiplier", multiplier);

    //    //
    //    // Surface-material-type sound code
    //    //


    //    if(v.primarySurface != -1)
    //    {
    //        if(directionContacts[v.primarySurface])
    //        {
    //            String terrainType = "Concrete";
    //            RaycastHit2D surfaceHit = directionContacts[v.primarySurface];
    //            if(surfaceHit.collider.sharedMaterial!=null)
    //            {
    //                terrainType = surfaceHit.collider.sharedMaterial.name;
    //            }
    //            if(terrainType==null || terrainType=="")
    //            {
    //                terrainType = "Concrete";
    //            } 
    //            AkSoundEngine.SetSwitch("TerrainType", terrainType, gameObject);
    //        }
    //    }
    //}

//    protected virtual void FixedGroundAnimation()
//    {		
//        if (!v.facingDirection) //If facing left
//        {
////			o.anim.SetBool("IsFacingRight", false);
////			o.spriteTransform.localScale = new Vector3 (-1f, 1f, 1f);
//            if(FighterState.Vel.x > 0 && !phys.airborne) // && FighterState.Vel.magnitude >= v.reversingSlideT 
//            {
//                o.anim.SetBool("Crouch", true);
//                v.sliding = true;
//            }
//        } 
//        else //If facing right
//        {
////			o.anim.SetBool("IsFacingRight", true);
////			o.spriteTransform.localScale = new Vector3 (1f, 1f, 1f);
//            if(FighterState.Vel.x < 0 && !phys.airborne) // && FighterState.Vel.magnitude >= v.reversingSlideT 
//            {
//                o.anim.SetBool("Crouch", true);
//                v.sliding = true;
//            }
//        }
//    }

    //protected virtual void FixedAirAnimation()
    //{
        
    //}

//    protected virtual void FixedWallAnimation()
//    {
//        if(v.wallSliding)
//        {
//            o.anim.SetBool("WallSlide", true);
//            v.sliding = true;
//        }

//        if (!v.facingDirection) //If facing left
//        {
////			o.anim.SetBool("IsFacingRight", false);
////			o.spriteTransform.localScale = new Vector3 (-1f, 1f, 1f);
//            if(v.primarySurface == 3 && FighterState.Vel.y > 0 && !phys.airborne) // If facing down and moving up, go into crouch stance.
//            {
//                //print("Running down on rightwall!");
//                o.anim.SetBool("Crouch", true);
//                v.sliding = true;
//            }
//        } 
//        else //If facing right
//        {
////			o.anim.SetBool("IsFacingRight", true);
////			o.spriteTransform.localScale = new Vector3 (1f, 1f, 1f);
//            if(v.primarySurface == 2 && FighterState.Vel.y > 0 && !phys.airborne) // If facing down and moving up, go into crouch stance.
//            {
//                //print("Running down on leftwall!");
//                o.anim.SetBool("Crouch", true);
//                v.sliding = true;
//            }
//        }
            
//    }

    protected virtual void UpdatePlayerInput()
    {
        // Extended by Player class.
    }

    protected Vector2 Vec2(Vector3 inputVector)
    {
        return new Vector2(inputVector.x, inputVector.y);
    }
    
    protected void DynamicCollision()
    {
        #region fightercollisions
        if(!this.isAlive()){return;}
        // FIGHTER-FIGHTER COLLISION TESTING IS SEPERATE AND PRECEDES WORLD COLLISIONS
        float crntSpeed = FighterState.Vel.magnitude*Time.fixedDeltaTime; //Current speed.
        RaycastHit2D[] fighterCollision = Physics2D.RaycastAll(this.transform.position, FighterState.Vel, crntSpeed, m_FighterMask);

        foreach(RaycastHit2D hit in fighterCollision)
        {
            if(hit.collider.gameObject != this.gameObject)
            {
                //print("HIT: "+hit.collider.transform.gameObject);//GetComponent<>());
                if(hit.collider.GetComponent<FighterChar>())
                {
                    if(hit.collider.GetComponent<FighterChar>().isAlive())
                    {
                        bool isFighterCol = false; // Change this when you add enemy collisions with player.
                        if(isFighterCol)
                        {
                            v.facingDirection = (hit.collider.transform.position.x-this.transform.position.x < 0) ? false : true; // If enemy is to your left, face left. Otherwise, right.
                            hit.collider.GetComponent<FighterChar>().v.facingDirection = !v.facingDirection;
                        }
                    }
                }
            }
        }
        #endregion
    }

    protected void WorldCollision()	// Handles all collisions with terrain geometry (and fighters).
    {
        //print ("Collision->phys.grounded=" + phys.grounded);
        float crntSpeed = FighterState.Vel.magnitude*Time.fixedDeltaTime; //Current speed.
        //print("DC Executing");
        d.errorDetectingRecursionCount++;

        if(d.errorDetectingRecursionCount >= 5)
        {
            throw new Exception("Your recursion code is not working!");
            //return;
        }

        if(FighterState.Vel.x > 0.001f)
        {
            phys.leftWallBlocked = false;
        }

        if(FighterState.Vel.x < -0.001f)
        {
            phys.rightWallBlocked = false;
        }
        #region worldcollision raytesting

        Vector2 adjustedBot = m_GroundFoot.position; // AdjustedBot marks the end of the ground raycast, but 0.02 shorter.
        adjustedBot.y += m.maxEmbed;

        Vector2 adjustedTop = m_CeilingFoot.position; // AdjustedTop marks the end of the ceiling raycast, but 0.02 shorter.
        adjustedTop.y -= m.maxEmbed;

        Vector2 adjustedLeft = m_LeftSide.position; // AdjustedLeft marks the end of the left wall raycast, but 0.02 shorter.
        adjustedLeft.x += m.maxEmbed;

        Vector2 adjustedRight = m_RightSide.position; // AdjustedRight marks the end of the right wall raycast, but 0.02 shorter.
        adjustedRight.x -= m.maxEmbed;

        //RaycastHit2D groundCheck = Physics2D.Raycast(this.transform.position, Vector2.down, m_GroundFootLength, mask);
        RaycastHit2D[] predictedLoc = new RaycastHit2D[4];
        //These raycasts fire from the 4 edges of the player collider in the direction of travel, effectively forming a projection of the player. This is a form of continuous collision detection.
        predictedLoc[0] = Physics2D.Raycast(adjustedBot, FighterState.Vel, crntSpeed, m_TerrainMask); 	// Ground
        predictedLoc[1] = Physics2D.Raycast(adjustedTop, FighterState.Vel, crntSpeed, m_TerrainMask); 	// Ceiling
        predictedLoc[2] = Physics2D.Raycast(adjustedLeft, FighterState.Vel, crntSpeed, m_TerrainMask); 	// Left
        predictedLoc[3] = Physics2D.Raycast(adjustedRight, FighterState.Vel, crntSpeed, m_TerrainMask);	// Right  

        float[] rayDist = new float[4];
        rayDist[0] = predictedLoc[0].distance; // Ground dist
        rayDist[1] = predictedLoc[1].distance; // Ceiling dist
        rayDist[2] = predictedLoc[2].distance; // Left dist
        rayDist[3] = predictedLoc[3].distance; // Right dist


        int shortestVertical = -1;
        int shortestHorizontal = -1;
        int shortestRaycast = -1;

        //Shortest non-zero vertical collision.
        if(rayDist[0] != 0 && rayDist[1]  != 0)
        {
            if(rayDist[0] <= rayDist[1])
            {
                shortestVertical = 0;
            }
            else
            {
                shortestVertical = 1;
            }
        }
        else if(rayDist[0] != 0)
        {
            shortestVertical = 0;
        }
        else if(rayDist[1] != 0)
        {
            shortestVertical = 1;
        }

        //Shortest non-zero horizontal collision.
        if(rayDist[2] != 0 && rayDist[3]  != 0)
        {
            if(rayDist[2] <= rayDist[3])
            {
                shortestHorizontal = 2;
            }
            else
            {
                shortestHorizontal = 3;
            }
        }
        else if(rayDist[2] != 0)
        {
            shortestHorizontal = 2;
        }
        else if(rayDist[3] != 0)
        {
            shortestHorizontal = 3;
        }

        //non-zero, shortest distance of all four colliders. This selects the collider that hits an obstacle the earliest. Zero is excluded because a non-colliding raycast returns a distance of 0.
        if(shortestVertical >= 0 && shortestHorizontal >= 0)
        {
            //print("Horiz dist="+shortestHorizontal);
            //print("Verti dist="+shortestVertical);
            //print("Verti-horiz="+(shortestVertical-shortestHorizontal));
            if(rayDist[shortestVertical] < rayDist[shortestHorizontal])
            {
                shortestRaycast = shortestVertical;
            }
            else
            {
                shortestRaycast = shortestHorizontal;
            }
        }
        else if(shortestVertical >= 0)
        {
            //print("Shortest is vertical="+shortestVertical);
            shortestRaycast = shortestVertical;
        }
        else if(shortestHorizontal >= 0)
        {
            //print("Shortest is horizontal="+shortestHorizontal);
            shortestRaycast = shortestHorizontal;
        }
        else
        {
            //print("NOTHING?");	
        }

        //print("G="+gDist+" C="+cDist+" R="+rDist+" L="+lDist);
        //print("VDist: "+shortestDistV);
        //print("HDist: "+shortestDistH);
        //print("shortestDist: "+rayDist[shortestRaycast]);

        //Count the number of sides colliding during this movement for debug purposes.
        int collisionNum = 0;

        if(predictedLoc[0])
        {
            collisionNum++;
        }
        if(predictedLoc[1])
        {
            collisionNum++;
        }
        if(predictedLoc[2])
        {
            collisionNum++;
        }
        if(predictedLoc[3])
        {
            collisionNum++;
        }

        if(collisionNum>0)
        {
            //print("TOTAL COLLISIONS: "+collisionNum);
        }

        #endregion

        Vector2 moveDirectionNormal = Perp(FighterState.Vel.normalized);
        Vector2 invertedDirectionNormal = -moveDirectionNormal;//This is made in case one of the raycasts is inside the collider, which would cause it to return an inverted normal value.

        switch (shortestRaycast)
        {
        case -1:
            {
                //print("No collision!");
                break;
            }
        case 0://Ground collision with feet
            {
                if ((moveDirectionNormal != predictedLoc[0].normal) && (invertedDirectionNormal != predictedLoc[0].normal)) 
                { // If the slope you're hitting is different than your current slope.

                    //
                    // Start of Ascendant Snag detection
                    // This is when the player's foot hits a surface that is less vertical than its current trajectory, and gets pulled downward onto the flatter slope. 
                    // The code detects this circumstance by comparing the new surface's y value to the y value of the player trajectory. If the player's y value is higher, they are hitting the surface from the underside with their foot collider, which should be ignored.

                    Vector2 predictedPerp = Perp(predictedLoc[0].normal);
                    if(predictedPerp.x==0)
                    {
                        predictedPerp = Perp(predictedPerp);
                    }

                    if((predictedPerp.x<0 && FighterState.Vel.x>0) || (predictedPerp.x>0 && FighterState.Vel.x<0))
                    {
                        //print("flipping predictedPerp...");
                        predictedPerp *= -1;
                    }

                    if(FighterState.Vel.normalized.y>predictedPerp.y)
                    {
                        if(d.sendCollisionMessages)
                        {
                            print("MD: "+FighterState.Vel.normalized);
                            print("PDL: "+predictedPerp);
                            print("Ascendant snag detected.");
                        }
                        return;
                    }
                    //
                    // End of Ascendant Snag detection
                    //
                    if(ToGround(predictedLoc[0]))
                    {
                        DirectionChange(m_GroundNormal);
                    }


                    return;
                }
                else // When the slope you're hitting is the same as your current slope, no action is needed.
                {
                    if(invertedDirectionNormal == predictedLoc[0].normal&&d.sendCollisionMessages)
                    {
                        throw new Exception("INVERTED GROUND IMPACT NORMAL DETECTED!");
                    }
                    return;
                }
            }
        case 1: //Ceiling collision
            {
                if ((moveDirectionNormal != predictedLoc[1].normal) && (invertedDirectionNormal != predictedLoc[1].normal)) 
                { // If the slope you're hitting is different than your current slope.
                    //print("CEILINm_Impact");
                    if(ToCeiling(predictedLoc[1]))
                    {
                        DirectionChange(m_CeilingNormal);
                    }
                    return;
                }
                else // When the slope you're hitting is the same as your current slope, no action is needed.
                {
                    if(invertedDirectionNormal == predictedLoc[1].normal&&d.sendCollisionMessages)
                    {
                        throw new Exception("INVERTED CEILING IMPACT NORMAL DETECTED!");
                    }
                    return;
                }
            }
        case 2: //Left contact collision
            {
                if ((moveDirectionNormal != predictedLoc[2].normal) && (invertedDirectionNormal != predictedLoc[2].normal)) 
                { // If the slope you're hitting is different than your current slope.
                    //print("LEFT_IMPACT");
                    if(ToLeftWall(predictedLoc[2]))
                    {
                        DirectionChange(m_LeftNormal);
                    }
                    return;
                }
                else // When the slope you're hitting is the same as your current slope, no action is needed.
                {
                    if(invertedDirectionNormal == predictedLoc[2].normal&&d.sendCollisionMessages)
                    {
                        throw new Exception("INVERTED LEFT IMPACT NORMAL DETECTED!");
                    }
                    return;
                }
            }
        case 3: //Right contact collision
            {
                if ((moveDirectionNormal != predictedLoc[3].normal) && (invertedDirectionNormal != predictedLoc[3].normal)) 
                { // If the slope you're hitting is different than your current slope.
                    //print("RIGHT_IMPACT");
                    //print("predictedLoc[3].normal=("+predictedLoc[3].normal.x+","+predictedLoc[3].normal.y+")");
                    //print("moveDirectionNormal=("+moveDirectionNormal.x+","+moveDirectionNormal.y+")");
                    //print("moveDirectionNormal="+moveDirectionNormal);
                    if(ToRightWall(predictedLoc[3])) // If you hit something on the rightwall, change direction.
                    {
                        DirectionChange(m_RightNormal);
                    }
                    return;
                }
                else // When the slope you're hitting is the same as your current slope, no action is needed.
                {
                    if(invertedDirectionNormal == predictedLoc[3].normal&&d.sendCollisionMessages)
                    {
                        throw new Exception("INVERTED RIGHT IMPACT NORMAL DETECTED!");
                    }
                    return;
                }
            }
        default:
            {
                print("ERROR: DEFAULTED.");
                break;
            }
        }
    }

    protected float GetSteepness(Vector2 vectorPara)
    {
        return 0f;
    }

    protected void Traction(float horizontalInput, float inputV)
    {
        Vector2 groundPara = Perp(m_GroundNormal);
        if(d.sendTractionMessages){print("Traction");}

        float linAccel = this.m.linearAccelRate;
        float fastAccel = this.m.startupAccelRate;
        float topSpeed = this.m.maxRunSpeed;

        if(FighterState.Stance==2) // If in guard stance
        {
            linAccel /= 2;
            fastAccel /= 2;
            topSpeed /= 2;
        }

        // This block of code makes the player treat very steep left and right surfaces as walls when they aren't going fast enough to reasonably climb them. 
        // This aims to prevent a jittering effect when the player build small amounts of speed, then hits the steeper slope and starts sliding down again 
        // as frequently as 60 times a second.
        if (this.GetSpeed() <= 0.0001f) 
        {
            //print("Hitting wall slowly, considering correction.");
            float wallSteepnessAngle;

            if ((phys.leftWalled) && (horizontalInput < 0)) 
            {
                //print("Trying to run up left wall slowly.");
                Vector2 wallPara = Perp (m_LeftNormal);
                wallSteepnessAngle = Vector2.Angle (Vector2.up, wallPara);
                if (wallSteepnessAngle == 180) 
                {
                    wallSteepnessAngle = 0;
                }
                if (wallSteepnessAngle >= m.tractionLossMaxAngle) 
                { //If the wall surface the player is running
                    //print("Wall steepness of "+wallSteepnessAngle+" was too steep for speed "+this.GetSpeed()+", stopping.");
                    FighterState.Vel = Vector2.zero;
                    phys.leftWallBlocked = true;
                }
            } 
            else if ((phys.rightWalled) && (horizontalInput > 0)) 
            {
                //print("Trying to run up right wall slowly.");
                Vector2 wallPara = Perp(m_RightNormal);
                wallSteepnessAngle = Vector2.Angle (Vector2.up, wallPara);
                wallSteepnessAngle = 180f - wallSteepnessAngle;
                if (wallSteepnessAngle == 180) 
                {
                    wallSteepnessAngle = 0;
                }
                if (wallSteepnessAngle >= m.tractionLossMaxAngle) 
                { //If the wall surface the player is running
                    //print("Wall steepness of "+wallSteepnessAngle+" was too steep for speed "+this.GetSpeed()+", stopping.");
                    FighterState.Vel = Vector2.zero;
                    phys.rightWallBlocked = true;
                }
            }
            else 
            {
                //print("Only hitting groundcontact, test ground steepness.");
            }

        }
        // End of anti-slope-jitter code.

        if(inputV<0)
        {
            phys.kneeling = true;
            horizontalInput = 0;
        }
//		else
//		{
//			v.cameraMode = v.defaultCameraMode;
//		}

        if(groundPara.x > 0)
        {
            groundPara *= -1;
        }

        if(d.sendTractionMessages){print("gp="+groundPara);}

        float steepnessAngle = Vector2.Angle(Vector2.left,groundPara);

        steepnessAngle = (float)Math.Round(steepnessAngle,2);
        if(d.sendTractionMessages){print("SteepnessAngle:"+steepnessAngle);}

        float slopeMultiplier = 0;

        if(steepnessAngle > m.tractionLossMinAngle)
        {
            if(steepnessAngle >= m.tractionLossMaxAngle)
            {
                if(d.sendTractionMessages){print("MAXED OUT!");}
                slopeMultiplier = 1;
            }
            else
            {
                slopeMultiplier = ((steepnessAngle-m.tractionLossMinAngle)/(m.tractionLossMaxAngle-m.tractionLossMinAngle));
            }

            if(d.sendTractionMessages){print("slopeMultiplier: "+slopeMultiplier);}
            //print("groundParaY: "+groundParaY+", slopeT: "+slopeT);
        }


        if(((phys.leftWallBlocked)&&(horizontalInput < 0)) || ((phys.rightWallBlocked)&&(horizontalInput > 0)))
        {// If running at an obstruction you're up against.
            //print("Running against a wall.");
            horizontalInput = 0;
        }

        //print("Traction executing");
        float rawSpeed = FighterState.Vel.magnitude;
        if(d.sendTractionMessages){print("FighterState.Vel.magnitude: "+FighterState.Vel.magnitude);}

        if (horizontalInput == 0||phys.kneeling) 
        {//if not pressing any move direction, slow to zero linearly.
            if(d.sendTractionMessages){print("No input, slowing...");}
            if(phys.kneeling) // Decelerate faster if crouching
            {
                if(rawSpeed <= 0.5f)
                {
                    FighterState.Vel = Vector2.zero;	
                }
                else
                {
                    FighterState.Vel = ChangeSpeedLinear(FighterState.Vel, -m.linearStopRate);
                }
            }
            else 
            {
                if(rawSpeed <= 0.5f)
                {
                    FighterState.Vel = Vector2.zero;	
                }
                else
                {
                    FighterState.Vel = ChangeSpeedLinear(FighterState.Vel, -m.linearSlideRate);
                }
            }
        }
        else if((horizontalInput > 0 && FighterState.Vel.x >= 0) || (horizontalInput < 0 && FighterState.Vel.x <= 0))
        {//if pressing same button as move direction, move to MAXSPEED.
            if(d.sendTractionMessages){print("Moving with keypress");}
            if(rawSpeed < topSpeed)
            {
                if(d.sendTractionMessages){print("Rawspeed("+rawSpeed+") less than max");}
                if(rawSpeed > m.tractionChangeT)
                {
                    if(d.sendTractionMessages){print("LinAccel-> " + rawSpeed);}
                    if(FighterState.Vel.y > 0)
                    { 	// If climbing, recieve uphill movement penalty.
                        FighterState.Vel = ChangeSpeedLinear(FighterState.Vel, linAccel*(1-slopeMultiplier));
                    }
                    else
                    {
                        FighterState.Vel = ChangeSpeedLinear(FighterState.Vel, linAccel);
                    }
                }
                else if(rawSpeed < 0.001f)
                {
//					if(slopeMultiplier<0.5)
//					{
//						FighterState.Vel = new Vector2((m_Acceleration)*horizontalInput*(1-slopeMultiplier), 0);
//					}
//					else
//					{
//						if(d.sendTractionMessages){print("Too steep!");}
//					}
//					if(d.sendTractionMessages){print("Starting motion. Adding " + m_Acceleration);}
                    if(d.sendTractionMessages){print("HardAccel-> " + rawSpeed);}
                    if(FighterState.Vel.y > 0)
                    { 	// If climbing, recieve uphill movement penalty.
                        FighterState.Vel = new Vector2(fastAccel*(1-slopeMultiplier)*horizontalInput, 0);
                    }
                    else
                    {
                        FighterState.Vel = new Vector2(fastAccel*horizontalInput, 0);
                    }
                }
                else
                {
//					//print("ExpAccel-> " + rawSpeed);
//					float eqnX = (1+Mathf.Abs((1/m.tractionChangeT )*rawSpeed));
//					float curveMultiplier = 1+(1/(eqnX*eqnX)); // Goes from 1/4 to 1, increasing as speed approaches 0.
//
//					float addedSpeed = curveMultiplier*(m_Acceleration);
//					if(FighterState.Vel.y > 0)
//					{ // If climbing, recieve uphill movement penalty.
//						addedSpeed = curveMultiplier*(m_Acceleration)*(1-slopeMultiplier);
//					}
//					if(d.sendTractionMessages){print("Addedspeed:"+addedSpeed);}
//					FighterState.Vel = (FighterState.Vel.normalized)*(rawSpeed+addedSpeed);
//					if(d.sendTractionMessages){print("FighterState.Vel:"+FighterState.Vel);}
                    if(d.sendTractionMessages){print("HardAccel-> " + rawSpeed);}
                    if(FighterState.Vel.y > 0)
                    { 	// If climbing, recieve uphill movement penalty.
                        FighterState.Vel = ChangeSpeedLinear(FighterState.Vel, fastAccel*(1-slopeMultiplier));
                    }
                    else
                    {
                        FighterState.Vel = ChangeSpeedLinear(FighterState.Vel, fastAccel);
                    }
                }
            }
            else
            {
                if(rawSpeed < topSpeed+1)
                {
                    rawSpeed = topSpeed;
                    SetSpeed(FighterState.Vel,topSpeed);
                }
                else
                {
                    if(d.sendTractionMessages){print("Rawspeed("+rawSpeed+") more than max.");}
                    FighterState.Vel = ChangeSpeedLinear (FighterState.Vel, -m.linearOverSpeedRate);
                }
            }
        }
        else if((horizontalInput > 0 && FighterState.Vel.x < 0) || (horizontalInput < 0 && FighterState.Vel.x > 0))
        {//if pressing button opposite of move direction, slow to zero quickly.
            if(d.sendTractionMessages){print("LinDecel");}
            FighterState.Vel = ChangeSpeedLinear (FighterState.Vel, -m.linearStopRate);

            //float modifier = Mathf.Abs(FighterState.Vel.x/FighterState.Vel.y);
            //print("SLOPE MODIFIER: " + modifier);
            //FighterState.Vel = FighterState.Vel/(1.25f);
        }

        Vector2 downSlope = FighterState.Vel.normalized; // Normal vector pointing down the current slope!
        if (downSlope.y > 0) //Make sure the vector is descending.
        {
            downSlope *= -1;
        }



        if(downSlope == Vector2.zero)
        {
            downSlope = Vector2.down;
        }

        FighterState.Vel += downSlope*m.slippingAcceleration*slopeMultiplier;

        //	TESTINGSLOPES
        if(d.sendTractionMessages){print("downSlope="+downSlope);}
        if(d.sendTractionMessages){print("m.slippingAcceleration="+m.slippingAcceleration);}
        if(d.sendTractionMessages){print("slopeMultiplier="+slopeMultiplier);}

        //ChangeSpeedLinear(FighterState.Vel, );
        if(d.sendTractionMessages){print("PostTraction velocity: "+FighterState.Vel);}
    }

    protected void AirControl(float horizontalInput)
    {
        FighterState.Vel += new Vector2(horizontalInput*m.airControlStrength, 0);
    }
        
    protected void WallTraction(float hInput, float vInput, Vector2 wallSurface)
    {
        if(vInput>0)
        {
            //print("FALLIN OFF YO!");
            AirControl(vInput);
            return;
        }
        else if(vInput<0)
        {
            phys.kneeling = true;
            hInput = 0;
//			if(v.defaultCameraMode==3)
//			{
//				v.cameraMode = 2;
//			}
        }

        if(phys.leftWalled) 	// If going up the left side wall, reverse horizontal input. This makes it so when control scheme is rotated 90 degrees, the key facing the wall will face up always. 
        {					// On walls the horizontal movekeys control vertical movement.
            hInput *= -1;
        }

        ////////////////////
        // Variable Setup //
        ////////////////////
        Vector2 wallPara = Perp(wallSurface);

        //print("hInput="+hInput);


        if(wallPara.x > 0)
        {
            wallPara *= -1;
        }

        float steepnessAngle = Vector2.Angle	(Vector2.up,wallPara);

        if(phys.rightWalled)
        {
            steepnessAngle = 180f - steepnessAngle;
        }

        if(steepnessAngle == 180)
        {
            steepnessAngle=0;
        }

        if(steepnessAngle > 90 && (wallSurface != m.expiredNormal)) //If the sliding surface is upside down, and hasn't already been clung to.
        {
            if(!phys.surfaceCling)
            {
                m.timeSpentHanging = 0;
                m.maxTimeHanging = 0;
                phys.surfaceCling = true;
                if(phys.CGF >= m.clingReqGForce)
                {
                    m.maxTimeHanging = m.surfaceClingTime;
                }
                else
                {
                    m.maxTimeHanging = m.surfaceClingTime*(phys.CGF/m.clingReqGForce);
                }
                //print("m.maxTimeHanging="+m.maxTimeHanging);
            }
            else
            {
                m.timeSpentHanging += Time.fixedDeltaTime;
                //print("time=("+m.timeSpentHanging+"/"+m.maxTimeHanging+")");
                if(m.timeSpentHanging>=m.maxTimeHanging)
                {
                    phys.surfaceCling = false;
                    m.expiredNormal = wallSurface;
                    //print("EXPIRED!");
                }
            }
        }
        else
        {
            phys.surfaceCling = false;
            m.timeSpentHanging = 0;
            m.maxTimeHanging = 0;
        }


        //
        // This code block is likely unnecessary
        // Anti-Jitter code for transitioning to a steep slope that is too steep to climb.
        //
        //		if (this.GetSpeed () <= 0.0001f) 
        //		{
        //			print ("RIDING WALL SLOWLY, CONSIDERING CORRECTION");
        //			if ((phys.leftWalled) && (hInput < 0)) 
        //			{
        //				if (steepnessAngle >= m.tractionLossMaxAngle) { //If the wall surface the player is running
        //					print ("Wall steepness of " + steepnessAngle + " was too steep for speed " + this.GetSpeed () + ", stopping.");
        //					//FighterState.Vel = Vector2.zero;
        //					phys.leftWallBlocked = true;
        //					hInput = 0;
        //					phys.surfaceCling = false;
        //				}
        //			} 
        //			else if ((phys.rightWalled) && (hInput > 0)) 
        //			{
        //				print ("Trying to run up right wall slowly.");
        //				if (steepnessAngle >= m.tractionLossMaxAngle) { //If the wall surface the player is running
        //					print ("Wall steepness of " + steepnessAngle + " was too steep for speed " + this.GetSpeed () + ", stopping.");
        //					//FighterState.Vel = Vector2.zero;
        //					phys.rightWallBlocked = true;
        //					hInput = 0;
        //					phys.surfaceCling = false;
        //				}
        //			} 
        //			else 
        //			{
        //				print ("Not trying to move up a wall; Continue as normal.");
        //			}
        //		}


        //print("Wall Steepness Angle:"+steepnessAngle);

        ///////////////////
        // Movement code //
        ///////////////////

        if(phys.surfaceCling)
        {
            //print("SURFACECLING!");
            if(FighterState.Vel.y > 0)
            {
                FighterState.Vel = ChangeSpeedLinear(FighterState.Vel,-0.8f);
            }
            else if(FighterState.Vel.y <= 0)
            {
                if( (hInput<0 && phys.leftWalled) || (hInput>0 && phys.rightWalled) )
                {
                    FighterState.Vel = ChangeSpeedLinear(FighterState.Vel,0.1f);
                }
                else
                {
                    FighterState.Vel = ChangeSpeedLinear(FighterState.Vel,1f);
                }
            }
        }
        else
        {
            if(FighterState.Vel.y>0) 	// If ascending...
            {		
                if(phys.leftWalled)
                    v.facingDirection = false;
                if(phys.rightWalled)
                    v.facingDirection = true;

                if(hInput>0) 				// ...and pressing key upward...
                {
                    FighterState.Vel.y -= 0.8f; // ... then decelerate slower.
                }
                else if(hInput<0) 			// ...and pressing key downward...
                {
                    FighterState.Vel.y -= 1.2f; // ...decelerate quickly.
                    if(phys.leftWalled)
                        v.facingDirection = true;
                    if(phys.rightWalled)
                        v.facingDirection = false;
                }
                else 						// ...and pressing nothing...
                {
                    FighterState.Vel.y -= 1f; 	// ...decelerate.
                }
            }
            else if(FighterState.Vel.y<=0) // If descending...
            {
                if(phys.leftWalled)
                    v.facingDirection = true;
                if(phys.rightWalled)
                    v.facingDirection = false;

                if(hInput>0) 					// ...and pressing key upward...
                {
                    FighterState.Vel.y -= 0.1f; 	// ...then wallslide.
                    v.wallSliding = true;
                }
                else if(hInput<0) 				// ...and pressing key downward...
                {
                    FighterState.Vel.y -= 1.2f; 	// ...accelerate downward quickly.
                }
                else 							// ...and pressing nothing...
                {
                    FighterState.Vel.y -= 1f; 		// ...accelerate downward.
                    v.wallSliding = true;
                }
            }
        }
    }


    protected bool ToLeftWall(RaycastHit2D leftCheck) 
    { //Sets the new position of the fighter and their m_LeftNormal.

        if(d.sendCollisionMessages){print("We've hit LeftWall, sir!!");}

        if (phys.airborne)
        {
            if(d.sendCollisionMessages){print("Airborne before impact.");}
            phys.worldImpact = true;
        }
        //phys.leftSideContact = true;
        phys.leftWalled = true;

        if(!phys.grounded&&!phys.ceilinged)
        {
            v.primarySurface = 2;
            v.truePrimarySurface = 2;
        }

        Vector2 setCharPos = leftCheck.point;
        setCharPos.x += (m_LeftSideLength-m.minEmbed); //Embed slightly in wall to ensure raycasts still hit wall.
        //setCharPos.y -= m.minEmbed;
        //print("Sent to Pos:" + setCharPos);

        this.transform.position = setCharPos;

        //print ("Final Position:  " + this.transform.position);

        RaycastHit2D leftCheck2 = Physics2D.Raycast(this.transform.position, Vector2.left, m_LeftSideLength, m_TerrainMask);
        if (leftCheck2) 
        {
            m_LeftNormal = leftCheck2.normal;
        }
        else
        {
            m_LeftNormal = leftCheck.normal;
        }
            
        if(Mathf.Abs(m_LeftNormal.x)<0.00001f) // Floating point imprecision correction for 90 degree angle errors
        {
            m_LeftNormal.x = 0;
        }

        if(Mathf.Abs(m_LeftNormal.y)<0.00001f) // Floating point imprecision correction for 90 degree angle errors
        {
            m_LeftNormal.y = 0;
        }
        return true;


        //print ("Final Position2:  " + this.transform.position);
    }

    protected bool ToRightWall(RaycastHit2D rightCheck) 
    { //Sets the new position of the fighter and their m_RightNormal.

        if(d.sendCollisionMessages){print("We've hit RightWall, sir!!");}
        //print ("groundCheck.normal=" + groundCheck.normal);
        //print("prerightwall Pos:" + this.transform.position);

        if (phys.airborne)
        {
            if(d.sendCollisionMessages){print("Airborne before impact.");}
            phys.worldImpact = true;
        }
        phys.rightSideContact = true;
        phys.rightWalled = true;

        if(!phys.grounded && !phys.ceilinged)
        {
            v.primarySurface = 3;
            v.truePrimarySurface = 3;
        }

        Vector2 setCharPos = rightCheck.point;
        setCharPos.x -= (m_RightSideLength-m.minEmbed); //Embed slightly in wall to ensure raycasts still hit wall.

        //print("Sent to Pos:" + setCharPos);
        //print("Sent to normal:" + groundCheck.normal);

        this.transform.position = setCharPos;

        //print ("Final Position:  " + this.transform.position);

        RaycastHit2D rightCheck2 = Physics2D.Raycast(this.transform.position, Vector2.right, m_RightSideLength, m_TerrainMask);
        if (rightCheck2) 
        {
            m_RightNormal = rightCheck2.normal;
            //print("rightCheck2.normal="+rightCheck2.normal);
        }
        else
        {
            m_RightNormal = rightCheck.normal;
            //print("rightCheck1.normal="+rightCheck.normal);
        }

        if(Mathf.Abs(m_RightNormal.x)<0.00001f) // Floating point imprecision correction for 90 degree angle errors
        {
            m_RightNormal.x = 0;
        }

        if(Mathf.Abs(m_RightNormal.y)<0.00001f) // Floating point imprecision correction for 90 degree angle errors
        {
            m_RightNormal.y = 0;
        }
        return true;
    }

    protected bool ToGround(RaycastHit2D groundCheck) 
    { //Sets the new position of the fighter and their ground normal.
        //print ("phys.grounded=" + phys.grounded);

        if (phys.airborne)
        {
            phys.worldImpact = true;
        }


            
        phys.grounded = true;
        v.primarySurface = 0;
        v.truePrimarySurface = 0;

        Vector2 setCharPos = groundCheck.point;
        setCharPos.y = setCharPos.y+m_GroundFootLength-m.minEmbed; //Embed slightly in ground to ensure raycasts still hit ground.
        this.transform.position = setCharPos;

        //print("Sent to Pos:" + setCharPos);
        //print("Sent to normal:" + groundCheck.normal);

        RaycastHit2D groundCheck2 = Physics2D.Raycast(this.transform.position, Vector2.down, m_GroundFootLength, m_TerrainMask);

        if(groundCheck.normal.y == 0f)
        {//If vertical surface
            //throw new Exception("Existence is suffering");
            if(d.sendCollisionMessages)
            {
                //print("GtG VERTICAL :O");
            }
        }

//		if(phys.ceilinged)
//		{
//			if(d.sendCollisionMessages)
//			{
//				print("CeilGroundWedge detected during ground collision.");
//			}
//			OmniWedge(0,1);
//		}
//
//		if(phys.leftWalled)
//		{
//			if(d.sendCollisionMessages)
//			{
//				print("LeftGroundWedge detected during ground collision.");
//			}
//			OmniWedge(0,2);
//		}
//
//		if(phys.rightWalled)
//		{
//			if(d.sendCollisionMessages)
//			{
//				print("RightGroundWedge detected during groundcollision.");
//			}
//			OmniWedge(0,3);
//		}

        if((GetSteepness(groundCheck2.normal)>=((m.tractionLossMaxAngle+m.tractionLossMinAngle)/2)) && this.GetSpeed()<=0.001f) 
        { //If going slow and hitting a steep slope, don't move to the new surface, and treat the new surface as a wall on that side.
            if(this.GetVelocity().x>0)
            {
                print("Positive slope ground acting as right wall due to steepness.");
                phys.rightWallBlocked = true;
            }
            else
            {
                print("Negative slope ground acting as left wall due to steepness.");
                phys.leftWallBlocked = true;
            }
            return false;
        }
        else
        {
            m_GroundNormal = groundCheck2.normal;
            return true;
        }



        //print ("Final Position2:  " + this.transform.position);
    }

    protected bool ToCeiling(RaycastHit2D ceilingCheck) 
    { //Sets the new position of the fighter when they hit the ceiling.
        //print ("We've hit ceiling, sir!!");
        //print ("ceilingCheck.normal=" + ceilingCheck.normal);

        if (phys.airborne)
        {
            phys.worldImpact = true;
        }




        //m_Impact = true;
        phys.ceilinged = true;
        Vector2 setCharPos = ceilingCheck.point;
        setCharPos.y -= (m_GroundFootLength-m.minEmbed); //Embed slightly in ceiling to ensure raycasts still hit ceiling.
        this.transform.position = setCharPos;

        RaycastHit2D ceilingCheck2 = Physics2D.Raycast(this.transform.position, Vector2.up, m_GroundFootLength, m_TerrainMask);
        if (ceilingCheck2) 
        {
            //			if(d.antiTunneling){
            //				Vector2 surfacePosition = ceilingCheck2.point;
            //				surfacePosition.y -= (m_CeilingFootLength-m.minEmbed);
            //				this.transform.position = surfacePosition;
            //			}
        }
        else
        {
            if(d.sendCollisionMessages)
            {
                print("Ceilinged = false?");
            }
            phys.ceilinged = false;
        }

        if(ceilingCheck.normal.y == 0f)
        {//If vertical surface
            //throw new Exception("Existence is suffering");
            if(d.sendCollisionMessages)
            {
                print("CEILING VERTICAL :O");
            }
        }

        m_CeilingNormal = ceilingCheck2.normal;

//		if(phys.grounded)
//		{
//			if(d.sendCollisionMessages)
//			{
//				print("CeilGroundWedge detected during ceiling collision.");
//			}
//			OmniWedge(0,1);
//		}
//
//		if(phys.leftWalled)
//		{
//			if(d.sendCollisionMessages)
//			{
//				print("LeftCeilWedge detected during ceiling collision.");
//			}
//			OmniWedge(2,1);
//		}
//
//		if(phys.rightWalled)
//		{
//			if(d.sendCollisionMessages)
//			{
//				print("RightGroundWedge detected during ceiling collision.");
//			}
//			OmniWedge(3,1);
//		}
        //print ("Final Position2:  " + this.transform.position);
        return true;
    }

    protected Vector2 ChangeSpeedMult(Vector2 inputVelocity, float multiplier)
    {
        Vector2 newVelocity;
        float speed = inputVelocity.magnitude*multiplier;
        Vector2 direction = inputVelocity.normalized;
        newVelocity = direction * speed;
        return newVelocity;
    }

    protected Vector2 ChangeSpeedLinear(Vector2 inputVelocity, float changeAmount)
    {
        Vector2 newVelocity;
        float speed = inputVelocity.magnitude+changeAmount;
        Vector2 direction = inputVelocity.normalized;
        newVelocity = direction * speed;
        return newVelocity;
    }

    protected void DirectionChange(Vector2 newNormal)
    {
        //print("DirectionChange");
        m.expiredNormal = new Vector2(0,0); //Used for wallslides. This resets the surface normal that wallcling is set to ignore.

        Vector2 initialDirection = FighterState.Vel.normalized;
        Vector2 newPara = Perp(newNormal);
        Vector2 AdjustedVel;

        float initialSpeed = FighterState.Vel.magnitude;
        float testNumber = newPara.y/newPara.x;

        if(float.IsNaN(testNumber))
        {
            if(d.sendCollisionMessages){print("NaN value found on DirectionChange");}
        }

        if((initialDirection == newPara)||initialDirection == Vector2.zero)
        {
            //print("same angle");
            return;
        }

        float impactAngle = Vector2.Angle(initialDirection,newPara);
        //print("TrueimpactAngle: " +impactAngle);
        //print("InitialDirection: "+initialDirection);
        //print("GroundDirection: "+newPara);

        impactAngle = (float)Math.Round(impactAngle,2);

        if(impactAngle >= 180)
        {
            impactAngle = 180f - impactAngle;
        }

        if(impactAngle > 90)
        {
            impactAngle = 180f - impactAngle;
        }

        AdjustedVel = Proj(FighterState.Vel, newPara);

        FighterState.Vel = AdjustedVel;

        //Speed loss from impact angle handling beyond this point. The player loses speed based on projection angle, but that is purely mathematical. The proceeding code is intended to simulate ground traction.

        float speedRetentionMult = 1; // The % of speed retained, based on sharpness of impact angle. A direct impact = full stop.

        if(impactAngle <= m.impactDecelMinAngle)
        { // Angle lower than min, no speed penalty.
            speedRetentionMult = 1;
        }
        else if(impactAngle < m.impactDecelMaxAngle)
        { // In the midrange, administering momentum loss on a curve leading from min to max.
            speedRetentionMult = 1-Mathf.Pow((impactAngle-m.impactDecelMinAngle)/(m.impactDecelMaxAngle-m.impactDecelMinAngle),2); // See Workflowy notes section for details on this formula.
        }
        else
        { // Angle beyond max, momentum halted. 
            speedRetentionMult = 0;
            phys.worldImpact = true;
        }

        if(initialSpeed <= 2f)
        { // If the fighter is near stationary, do not remove any velocity because there is no impact!
            speedRetentionMult = 1;
        }
            
        //print("SPLMLT " + speedRetentionMult);

        SetSpeed(FighterState.Vel, initialSpeed*speedRetentionMult);
        //print("Final Vel " + FighterState.Vel);
        //print ("DirChange Vel:  " + FighterState.Vel);
    }

    protected void OmniWedge(int lowerContact, int upperContact)
    {//Executes when the fighter is moving into a corner and there isn't enough room to fit them. It halts the fighter's momentum and sets off a blocked-direction flag.

        if(d.sendCollisionMessages){print("OmniWedge("+lowerContact+","+upperContact+")");}

        RaycastHit2D lowerHit;
        Vector2 lowerDirection = Vector2.down;
        float lowerLength = m_GroundFootLength;

        RaycastHit2D upperHit;
        Vector2 upperDirection = Vector2.up;
        float upperLength = m_CeilingFootLength;


        switch(lowerContact)
        {
        case 0: //lowercontact is ground
            {
                lowerDirection = Vector2.down;
                lowerLength = m_GroundFootLength;
                break;
            }
        case 1: //lowercontact is ceiling
            {
                throw new Exception("ERROR: Ceiling cannot be lower contact.");
            }
        case 2: //lowercontact is left
            {
                if(d.sendCollisionMessages){print("Omniwedge: lowercontact is left");}
                lowerDirection = Vector2.left;
                lowerLength = m_LeftSideLength;
                break;
            }
        case 3: //lowercontact is right
            {
                if(d.sendCollisionMessages){print("Omniwedge: lowercontact is right");}
                lowerDirection = Vector2.right;
                lowerLength = m_RightSideLength;
                break;
            }
        default:
            {
                throw new Exception("ERROR: DEFAULTED ON LOWERHIT.");
            }
        }

        lowerHit = Physics2D.Raycast(this.transform.position, lowerDirection, lowerLength, m_TerrainMask);

        float embedDepth;
        Vector2 gPara; //lowerpara, aka groundparallel
        Vector2 cPara; //upperpara, aka ceilingparallel
        Vector2 correctionVector = new Vector2(0,0);

        if(!lowerHit)
        {
            //throw new Exception("Bottom not wedged!");
            if(d.sendCollisionMessages){print("Bottom not wedged!");}
            //gPara.x = m_GroundNormal.x;
            //gPara.y = m_GroundNormal.y;
            return;
        }
        else
        {
            gPara = Perp(lowerHit.normal);
            Vector2 groundPosition = lowerHit.point;
            if(lowerContact == 0) //ground contact
            {
                groundPosition.y += (m_GroundFootLength-m.minEmbed);
            }
            else if(lowerContact == 1) //ceiling contact
            {
                throw new Exception("CEILINGCOLLIDER CAN'T BE LOWER CONTACT");
            }
            else if(lowerContact == 2) //left contact
            {
                groundPosition.x += (m_LeftSideLength-m.minEmbed);
            }
            else if(lowerContact == 3) //right contact
            {
                groundPosition.x -= (m_RightSideLength-m.minEmbed);
            }

            this.transform.position = groundPosition;
            //print("Hitting bottom, shifting up!");
        }

        switch(upperContact)
        {
        case 0: //uppercontact is ground
            {
                throw new Exception("FLOORCOLLIDER CAN'T BE UPPER CONTACT");
            }
        case 1: //uppercontact is ceiling
            {
                upperDirection = Vector2.up;
                upperLength = m_CeilingFootLength;
                break;
            }
        case 2: //uppercontact is left
            {
                if(d.sendCollisionMessages){print("Omniwedge: uppercontact is left");}
                upperDirection = Vector2.left;
                upperLength = m_LeftSideLength;
                break;
            }
        case 3: //uppercontact is right
            {
                if(d.sendCollisionMessages){print("Omniwedge: uppercontact is right");}
                upperDirection = Vector2.right;
                upperLength = m_RightSideLength;
                break;
            }
        default:
            {
                throw new Exception("ERROR: DEFAULTED ON UPPERHIT.");
            }
        }

        upperHit = Physics2D.Raycast(this.transform.position, upperDirection, upperLength, m_TerrainMask);
        embedDepth = upperLength-upperHit.distance;

        if(!upperHit)
        {
            //throw new Exception("Top not wedged!");
            cPara = Perp(upperHit.normal);
            if(d.sendCollisionMessages)
            {
                print("Top not wedged!");
            }
            return;
        }
        else
        {
            //print("Hitting top, superunwedging..."); 
            cPara = Perp(upperHit.normal);
        }

        //print("Embedded ("+embedDepth+") units into the ceiling");

        //Rounding the perpendiculars to 4 decimal places to eliminate error prone edge-cases and floating point imprecision.
        gPara.x = Mathf.Round(gPara.x * 10000f) / 10000f;
        gPara.y = Mathf.Round(gPara.y * 10000f) / 10000f;

        cPara.x = Mathf.Round(cPara.x * 10000f) / 10000f;
        cPara.y = Mathf.Round(cPara.y * 10000f) / 10000f;

        float cornerAngle = Vector2.Angle(cPara,gPara);

        //print("Ground Para = " + gPara);
        //print("Ceiling Para = " + cPara);
        //print("cornerAngle = " + cornerAngle);

        Vector2 cParaTest = cPara;
        Vector2 gParaTest = gPara;

        if(cParaTest.x < 0)
        {
            cParaTest *= -1;
        }
        if(gParaTest.x < 0)
        {
            gParaTest *= -1;
        }

        //print("gParaTest = " + gParaTest);
        //print("cParaTest= " + cParaTest);

        float convergenceValue = cParaTest.y-gParaTest.y;
        //print("ConvergenceValue =" + convergenceValue);

        if(lowerContact == 2 || upperContact == 2){convergenceValue = 1;}; // PLACEHOLDER CODE! It just sets it to converging left when touching left contact.
        if(lowerContact == 3 || upperContact == 3){convergenceValue =-1;}; // PLACEHOLDER CODE! It just sets it to converging right when touching right contact.

        if(cornerAngle > 90f)
        {
            if(convergenceValue > 0)
            {
                if(d.sendCollisionMessages){print("Left wedge!");}
                correctionVector = SuperUnwedger(cPara, gPara, true, embedDepth);
                if(d.sendCollisionMessages){print("correctionVector:"+correctionVector);}
                phys.leftWallBlocked = true;
            }
            else if(convergenceValue < 0)
            {
                //print("Right wedge!");
                correctionVector = SuperUnwedger(cPara, gPara, false, embedDepth);
                phys.rightWallBlocked = true;
            }
            else
            {
                throw new Exception("CONVERGENCE VALUE OF ZERO ON CORNER!");
            }
            FighterState.Vel = new Vector2(0f, 0f);
        }
        else
        {
            if(d.sendCollisionMessages){print("Obtuse wedge angle detected!");}
            correctionVector = (upperDirection*(-(embedDepth-m.minEmbed)));
        }

        this.transform.position = new Vector2((this.transform.position.x + correctionVector.x), (this.transform.position.y + correctionVector.y));
    }

    protected Vector2 Perp(Vector2 input) //Perpendicularizes the vector.
    {
        Vector2 output;
        output.x = input.y;
        output.y = -input.x;
        return output;
    }

    protected Vector2 Proj(Vector2 A, Vector2 B) //Projects vector A onto vector B.
    {
        float component = Vector2.Dot(A,B)/B.magnitude;
        return component*B.normalized;
    }		

    protected void UpdateContactNormals(bool posCorrection) // UCN - Updates the present-time state of the player's contact with surrounding world geometry. Corrects the player's position if it is embedded in geometry, and gathers information about where the player can move.
    {
        phys.grounded = false;
        phys.ceilinged = false;
        phys.leftWalled = false;
        phys.rightWalled = false;
        phys.airborne = false;

        if(m.jumpBufferG>0){m.jumpBufferG--;}
        if(m.jumpBufferC>0){m.jumpBufferC--;}
        if(m.jumpBufferL>0){m.jumpBufferL--;}
        if(m.jumpBufferR>0){m.jumpBufferR--;}

        phys.groundContact = false;
        phys.ceilingContact = false;
        phys.leftSideContact = false;
        phys.rightSideContact = false;

        d.groundLine.endColor = Color.red;
        d.groundLine.startColor = Color.red;
        d.ceilingLine.endColor = Color.red;
        d.ceilingLine.startColor = Color.red;
        d.leftSideLine.endColor = Color.red;
        d.leftSideLine.startColor = Color.red;
        d.rightSideLine.endColor = Color.red;
        d.rightSideLine.startColor = Color.red;

        directionContacts[0] = Physics2D.Raycast(this.transform.position, Vector2.down, m_GroundFootLength, m_TerrainMask); 	// Ground
        directionContacts[1] = Physics2D.Raycast(this.transform.position, Vector2.up, m_CeilingFootLength, m_TerrainMask);  	// Ceiling
        directionContacts[2] = Physics2D.Raycast(this.transform.position, Vector2.left, m_LeftSideLength, m_TerrainMask); 	// Left
        directionContacts[3] = Physics2D.Raycast(this.transform.position, Vector2.right, m_RightSideLength, m_TerrainMask);	// Right  

        if (directionContacts[0]) 
        {
            m_GroundNormal = directionContacts[0].normal;
            phys.groundContact = true;
            d.groundLine.endColor = Color.green;
            d.groundLine.startColor = Color.green;
            phys.grounded = true;
            m.jumpBufferG = m.jumpBufferFrameAmount;
            if(Mathf.Abs(m_GroundNormal.x)<0.00001f) // Floating point imprecision correction for 90 degree angle errors
                m_GroundNormal.x = 0;
            if(Mathf.Abs(m_GroundNormal.y)<0.00001f) // Floating point imprecision correction for 90 degree angle errors
                m_GroundNormal.y = 0;
        } 

        if (directionContacts[1]) 
        {
            m_CeilingNormal = directionContacts[1].normal;
            phys.ceilingContact = true;
            d.ceilingLine.endColor = Color.green;
            d.ceilingLine.startColor = Color.green;
            phys.ceilinged = true;
            m.jumpBufferC = m.jumpBufferFrameAmount;
            if(Mathf.Abs(m_CeilingNormal.x)<0.00001f) // Floating point imprecision correction for 90 degree angle errors
                m_CeilingNormal.x = 0;
            if(Mathf.Abs(m_CeilingNormal.y)<0.00001f) // Floating point imprecision correction for 90 degree angle errors
                m_CeilingNormal.y = 0;
        } 


        if (directionContacts[2])
        {
            m_LeftNormal = directionContacts[2].normal;
            phys.leftSideContact = true;
            d.leftSideLine.endColor = Color.green;
            d.leftSideLine.startColor = Color.green;
            phys.leftWalled = true;
            m.jumpBufferL = m.jumpBufferFrameAmount;
            if(Mathf.Abs(m_LeftNormal.x)<0.00001f) // Floating point imprecision correction for 90 degree angle errors
                m_LeftNormal.x = 0;
            if(Mathf.Abs(m_LeftNormal.y)<0.00001f) // Floating point imprecision correction for 90 degree angle errors
                m_LeftNormal.y = 0;

        } 

        if (directionContacts[3])
        {
            m_RightNormal = directionContacts[3].normal;
            phys.rightSideContact = true;
            d.rightSideLine.endColor = Color.green;
            d.rightSideLine.startColor = Color.green;
            phys.rightWalled = true;
            m.jumpBufferR = m.jumpBufferFrameAmount;
            if(Mathf.Abs(m_RightNormal.x)<0.00001f) // Floating point imprecision correction for 90 degree angle errors
                m_RightNormal.x = 0;
            if(Mathf.Abs(m_RightNormal.y)<0.00001f) // Floating point imprecision correction for 90 degree angle errors
                m_RightNormal.y = 0;
        } 

        if(!(phys.grounded&&phys.ceilinged)) //Resets wall blocker flags if the player isn't touching a blocking surface.
        {
            if(!phys.rightWalled)
            {
                phys.rightWallBlocked = false;
            }
            if(!phys.leftWalled)
            {
                phys.leftWallBlocked = false;
            }
        }

        if(d.antiTunneling&&posCorrection)
        {
            AntiTunneler(directionContacts);
        }
        if(!(phys.grounded||phys.ceilinged||phys.leftWalled||phys.rightWalled))
        {
            phys.airborne = true;
            phys.surfaceCling = false;
        }
    }

    protected void DebugUCN() //Like normal UCN but doesn't affect anything and prints results.
    {
        print("DEBUG_UCN");
        d.groundLine.endColor = Color.red;
        d.groundLine.startColor = Color.red;
        d.ceilingLine.endColor = Color.red;
        d.ceilingLine.startColor = Color.red;
        d.leftSideLine.endColor = Color.red;
        d.leftSideLine.startColor = Color.red;
        d.rightSideLine.endColor = Color.red;
        d.rightSideLine.startColor = Color.red;

        RaycastHit2D[] directionContacts = new RaycastHit2D[4];
        directionContacts[0] = Physics2D.Raycast(this.transform.position, Vector2.down, m_GroundFootLength, m_TerrainMask); 	// Ground
        directionContacts[1] = Physics2D.Raycast(this.transform.position, Vector2.up, m_CeilingFootLength, m_TerrainMask);  	// Ceiling
        directionContacts[2] = Physics2D.Raycast(this.transform.position, Vector2.left, m_LeftSideLength, m_TerrainMask); 	// Left
        directionContacts[3] = Physics2D.Raycast(this.transform.position, Vector2.right, m_RightSideLength, m_TerrainMask);	// Right  

        if (directionContacts[0]) 
        {
            d.groundLine.endColor = Color.green;
            d.groundLine.startColor = Color.green;
        } 

        if (directionContacts[1]) 
        {
            d.ceilingLine.endColor = Color.green;
            d.ceilingLine.startColor = Color.green;
        } 


        if (directionContacts[2])
        {
            d.leftSideLine.endColor = Color.green;
            d.leftSideLine.startColor = Color.green;
        } 

        if (directionContacts[3])
        {
            d.rightSideLine.endColor = Color.green;
            d.rightSideLine.startColor = Color.green;
        } 

        int contactCount = 0;
        if(phys.groundContact){contactCount++;}
        if(phys.ceilingContact){contactCount++;}
        if(phys.leftSideContact){contactCount++;}
        if(phys.rightSideContact){contactCount++;}

        int embedCount = 0;
        if(d.sendCollisionMessages&&phys.groundContact && ((m_GroundFootLength-directionContacts[0].distance)>=0.011f))	{ print("Embedded in grnd by amount: "+((m_GroundFootLength-directionContacts[0].distance)-m.minEmbed)); embedCount++;} //If embedded too deep in this surface.
        if(d.sendCollisionMessages&&phys.ceilingContact && ((m_CeilingFootLength-directionContacts[1].distance)>=0.011f))	{ print("Embedded in ceil by amount: "+((m_CeilingFootLength-directionContacts[1].distance)-m.minEmbed)); embedCount++;} //If embedded too deep in this surface.
        if(d.sendCollisionMessages&&phys.leftSideContact && ((m_LeftSideLength-directionContacts[2].distance)>=0.011f))	{ print("Embedded in left by amount: "+((m_LeftSideLength-directionContacts[2].distance)-m.minEmbed)); embedCount++;} //If embedded too deep in this surface.
        if(d.sendCollisionMessages&&phys.rightSideContact && ((m_RightSideLength-directionContacts[3].distance)>=0.011f))	{ print("Embedded in rigt by amount: "+((m_RightSideLength-directionContacts[3].distance)-m.minEmbed)); embedCount++;} //If embedded too deep in this surface.

        if(d.sendCollisionMessages){print(contactCount+" sides touching, "+embedCount+" sides embedded");}
    }

    protected void AntiTunneler(RaycastHit2D[] contacts)
    {
        bool[] isEmbedded = {false, false, false, false};
        int contactCount = 0;
        if(phys.groundContact){contactCount++;}
        if(phys.ceilingContact){contactCount++;}
        if(phys.leftSideContact){contactCount++;}
        if(phys.rightSideContact){contactCount++;}

        int embedCount = 0;
        if(phys.groundContact && ((m_GroundFootLength-contacts[0].distance)>=0.011f))	{isEmbedded[0]=true; embedCount++;} //If embedded too deep in this surface.
        if(phys.ceilingContact && ((m_CeilingFootLength-contacts[1].distance)>=0.011f))	{isEmbedded[1]=true; embedCount++;} //If embedded too deep in this surface.
        if(phys.leftSideContact && ((m_LeftSideLength-contacts[2].distance)>=0.011f))	{isEmbedded[2]=true; embedCount++;} //If embedded too deep in this surface.
        if(phys.rightSideContact && ((m_RightSideLength-contacts[3].distance)>=0.011f))	{isEmbedded[3]=true; embedCount++;} //If embedded too deep in this surface.

        switch(contactCount)
        {
        case 0: //No embedded contacts. Save this position as the most recent valid one and move on.
            {
                //print("No embedding! :)");
                phys.lastSafePosition = this.transform.position;
                break;
            }
        case 1: //One side is embedded. Simply push out to remove it.
            {
                //print("One side embed!");
                if(isEmbedded[0])
                {
                    Vector2 surfacePosition = contacts[0].point;
                    surfacePosition.y += (m_GroundFootLength-m.minEmbed);
                    this.transform.position = surfacePosition;
                }
                else if(isEmbedded[1])
                {
                    Vector2 surfacePosition = contacts[1].point;
                    surfacePosition.y -= (m_CeilingFootLength-m.minEmbed);
                    this.transform.position = surfacePosition;
                }
                else if(isEmbedded[2])
                {
                    Vector2 surfacePosition = contacts[2].point;
                    surfacePosition.x += ((m_LeftSideLength)-m.minEmbed);
                    this.transform.position = surfacePosition;
                }
                else if(isEmbedded[3])
                {
                    Vector2 surfacePosition = contacts[3].point;
                    surfacePosition.x -= ((m_RightSideLength)-m.minEmbed);
                    this.transform.position = surfacePosition;
                }
                else
                {
                    phys.lastSafePosition = this.transform.position;
                }
                break;
            }
        case 2: //Two sides are touching. Use the 2-point unwedging algorithm to resolve.
            {
                if(phys.groundContact&&phys.ceilingContact)
                {
                    //if(m_GroundNormal != m_CeilingNormal)
                    {
                        if(d.sendCollisionMessages)
                        {
                            print("Antitunneling omniwedge executed");		
                        }
                        OmniWedge(0,1);
                    }
                }
                else if(phys.groundContact&&phys.leftSideContact)
                {
                    if(m_GroundNormal != m_LeftNormal)
                    {
                        OmniWedge(0,2);
                    }
                    else
                    {
                        //print("Same surface, 1-point unwedging.");
                        Vector2 surfacePosition = contacts[0].point;
                        surfacePosition.y += (m_GroundFootLength-m.minEmbed);
                        this.transform.position = surfacePosition;
                    }
                }
                else if(phys.groundContact&&phys.rightSideContact)
                {
                    if(m_GroundNormal != m_RightNormal)
                    {
                        OmniWedge(0,3);
                    }
                    else
                    {
                        //print("Same surface, 1-point unwedging.");
                        Vector2 surfacePosition = contacts[0].point;
                        surfacePosition.y += (m_GroundFootLength-m.minEmbed);
                        this.transform.position = surfacePosition;
                    }
                }
                else if(phys.ceilingContact&&phys.leftSideContact)
                {
                    //if(m_CeilingNormal != m_LeftNormal)
                    {
                        OmniWedge(2,1);
                    }
                }
                else if(phys.ceilingContact&&phys.rightSideContact)
                {
                    //if(m_CeilingNormal != m_RightNormal)
                    {
                        OmniWedge(3,1);
                    }
                }
                else if(phys.leftSideContact&&phys.rightSideContact)
                {
                    throw new Exception("Unhandled horizontal wedge detected.");
                    //OmniWedge(3,2);
                }
                break;
            }
        case 3: //Three sides are embedded. Not sure how to handle this yet besides reverting.
            {
                if(d.sendCollisionMessages)
                {
                    print("Triple Embed.");
                }
                break;
            }
        case 4:
            {
                if(d.sendCollisionMessages)
                {
                    print("FULL embedding!");
                }
                if(d.recoverFromFullEmbed)
                {
                    this.transform.position = phys.lastSafePosition;
                }
                break;
            }
        default:
            {
                if(d.sendCollisionMessages)
                {
                    print("ERROR: DEFAULTED ON ANTITUNNELER.");
                }
                break;
            }
        }

    }

    protected Vector2 SuperUnwedger(Vector2 cPara, Vector2 gPara, bool cornerIsLeft, float embedDistance)
    {
        if(d.sendCollisionMessages)
        {
            print("Ground Para = ("+gPara.x+", "+gPara.y+")");
            print("Ceiling Para = ("+cPara.x+", "+cPara.y+")");
        }
        if(!cornerIsLeft)
        {// Setting up variables	
            //print("Resolving right wedge.");


            if(gPara.x>0)
            {// Ensure both perpendicular vectors are pointing left, out of the corner the fighter is lodged in.
                gPara *= -1;
            }

            if(cPara.x>=0)
            {// Ensure both perpendicular vectors are pointing left, out of the corner the fighter is lodged in.
                //print("("+cPara.x+", "+cPara.y+") is cPara, inverting this...");
                cPara *= -1;
            }

            if(cPara.x != -1)
            {// Multiply/Divide the top vector so that its x = -1.
                //if(Math.Abs(cPara.x) < 1)
                if(Math.Abs(cPara.x) == 0)
                {
                    return new Vector2(0, -embedDistance);
                }
                else
                {
                    cPara /= Mathf.Abs(cPara.x);
                }
            }

            if(gPara.x != -1)
            {// Multiply/Divide the bottom vector so that its x = -1.
                if(gPara.x == 0)
                {
                    //throw new Exception("Your ground has no horizontality. What are you even doing?");
                    return new Vector2(0, embedDistance);
                }
                else
                {
                    gPara /= Mathf.Abs(gPara.x);
                }
            }
        }
        else
        {
            if(d.sendCollisionMessages){print("Resolving left wedge.");}

            if(gPara.x<0)
            {// Ensure both surface-parallel vectors are pointing right, out of the corner the fighter is lodged in.
                gPara *= -1;
            }

            if(cPara.x<0)
            {// Ensure both surface-parallel vectors are pointing left, out of the corner the fighter is lodged in.
                cPara *= -1;
            }

            if(cPara.x != 1)
            {// Multiply/Divide the top vector so that its x = 1.
                if(Math.Abs(cPara.x) == 0)
                {
                    if(d.sendCollisionMessages){print("It's a wall, bro");}
                    //return new Vector2(0, -embedDistance);
                    return new Vector2(embedDistance-m.minEmbed,0);
                }
                else
                {
                    cPara /= cPara.x;
                }
            }

            if(gPara.x != -1)
            {// Multiply/Divide the bottom vector so that its x = -1.
                if(gPara.x == 0)
                {
                    //throw new Exception("Your ground has no horizontality. What are you even doing?");
                    return new Vector2(0, -embedDistance);
                }
                else
                {
                    gPara /= gPara.x;
                }
            }
        }

        if(d.sendCollisionMessages)
        {
            print("Adapted Ground Para = "+gPara);
            print("Adapted Ceiling Para = "+cPara);
        }
        //
        // Now, the equation for repositioning two points that are embedded in a corner, so that both points are touching the lines that comprise the corner
        // In other words, here is the glorious UNWEDGER algorithm.
        //
        float B = gPara.y;
        float A = cPara.y;
        float H = embedDistance; //Reduced so the fighter stays just embedded enough for the raycast to detect next frame.
        float DivX;
        float DivY;
        float X;
        float Y;

        if(d.sendCollisionMessages){print("(A, B)=("+A+", "+B+").");}

        if(B <= 0)
        {
            if(d.sendCollisionMessages){print("B <= 0, using normal eqn.");}
            DivX = B-A;
            DivY = -(DivX/B);
        }
        else
        {
            if(d.sendCollisionMessages){print("B >= 0, using alternate eqn.");}
            DivX = 1f/(B-A);
            DivY = -(A*DivX);
        }

        if(DivX != 0)
        {
            X = H/DivX;
        }
        else
        {
            X = 0;
        }

        if(DivY != 0)
        {
            Y = H/DivY;
        }
        else
        {
            Y = 0;
        }

        if((cornerIsLeft)&&(X<0))
        {
            X = -X;
        }

        if((!cornerIsLeft)&&(X>0))
        {
            X = -X;
        }

        //print("Adding the movement: ("+ X +", "+Y+").");
        if(Math.Abs(X) >= 1000 || Math.Abs(Y) >= 1000)
        {
            print("ERROR: HYPERMASSIVE CORRECTION OF ("+X+","+Y+")");
            return new Vector2 (0, 0);
        }
        if(d.sendCollisionMessages){print("SuperUnwedger push of: ("+X+","+Y+")");}
        return new Vector2(X,Y); // Returns the distance the object must move to resolve wedging.
    }

    protected void Jump(float horizontalInput)
    {
        Vector2 preJumpVelocity = FighterState.Vel;
        float jumpVelBonusM = 1;

        if(phys.grounded&&phys.ceilinged)
        {
            if(d.sendCollisionMessages)
            {print("Grounded and Ceilinged, nowhere to jump!");}
            //FighterState.JumpKey = false;
        }
        else if(m.jumpBufferG>0)
        {
            //phys.leftWallBlocked = false;
            //phys.rightWallBlocked = false;

            n_Jumped = true;

            if(FighterState.Vel.y >= 0) // If falling, jump will nullify downward momentum.
            {
                FighterState.Vel = new Vector2(FighterState.Vel.x+(m.hJumpForce*horizontalInput*jumpVelBonusM), FighterState.Vel.y+(m.vJumpForce*jumpVelBonusM));
            }
            else
            {
                FighterState.Vel = new Vector2(FighterState.Vel.x+(m.hJumpForce*horizontalInput*jumpVelBonusM), (m.vJumpForce*jumpVelBonusM));
            }
            phys.grounded = false; // Watch this.
            v.primarySurface = -1;
            m.airborneDelayTimer = -1;
            v.truePrimarySurface = -1;
        }
        else if(m.jumpBufferL>0)
        {
            if(d.sendCollisionMessages)
            {
                print("Leftwalljumping!");
            }
            if(FighterState.Vel.y < 0)
            {
                FighterState.Vel = new Vector2( (m.wallHJumpForce*jumpVelBonusM), (m.wallVJumpForce*jumpVelBonusM) );
            }
            else if(FighterState.Vel.y <= (2*m.wallVJumpForce)) // If not ascending too fast, add vertical jump power to jump.
            {
                FighterState.Vel = new Vector2( (m.wallHJumpForce*jumpVelBonusM), FighterState.Vel.y+(m.wallVJumpForce*jumpVelBonusM) );
            }
            else // If ascending too fast, add no more vertical speed and just add horizontal.
            {
                FighterState.Vel = new Vector2( (m.wallHJumpForce*jumpVelBonusM), FighterState.Vel.y);
            }
            //FighterState.JumpKey = false;
            phys.leftWalled = false;
            n_Jumped = true;

            v.primarySurface = -1;
            m.airborneDelayTimer = -1;
            v.truePrimarySurface = -1;
        }
        else if(m.jumpBufferR>0)
        {
            if(d.sendCollisionMessages)
            {
                print("Rightwalljumping!");
            }
            if(FighterState.Vel.y < 0)
            {
                FighterState.Vel = new Vector2( (-m.wallHJumpForce*jumpVelBonusM), (m.wallVJumpForce*jumpVelBonusM) );
            }
            else if(FighterState.Vel.y <= m.wallVJumpForce)
            {
                FighterState.Vel = new Vector2( (-m.wallHJumpForce*jumpVelBonusM), FighterState.Vel.y+(m.wallVJumpForce*jumpVelBonusM) );
            }
            else
            {
                FighterState.Vel = new Vector2( (-m.wallHJumpForce*jumpVelBonusM), FighterState.Vel.y );
            }

            //FighterState.JumpKey = false;
            phys.rightWalled = false;
            n_Jumped = true;
            v.primarySurface = -1;
            m.airborneDelayTimer = -1;
            v.truePrimarySurface = -1;
        }
        else if(m.jumpBufferC>0)
        {
            if(FighterState.Vel.y <= 0)
            {
                FighterState.Vel = new Vector2(FighterState.Vel.x+(m.hJumpForce*horizontalInput*jumpVelBonusM), FighterState.Vel.y-(m.vJumpForce*jumpVelBonusM));
            }
            else
            {
                FighterState.Vel = new Vector2(FighterState.Vel.x+(m.hJumpForce*horizontalInput*jumpVelBonusM), -(m.vJumpForce*jumpVelBonusM));
            }
            //FighterState.JumpKey = false;
            n_Jumped = true;
            phys.ceilinged = false;
            v.primarySurface = -1;
            m.airborneDelayTimer = -1;
            v.truePrimarySurface = -1;
        }
        else
        {
            //print("Can't jump, airborne!");
        }
            
        m.jumpBufferG = 0;
        m.jumpBufferC = 0;
        m.jumpBufferL = 0;
        m.jumpBufferR = 0;
    }
    
    #endregion
    //###################################################################################################################################
    // PUBLIC FUNCTIONS
    //###################################################################################################################################
    #region PUBLIC FUNCTIONS

    public float Get2DAngle(Vector2 vector2, float degOffset) // Get angle, from -180 to +180 degrees. Degree offset shifts the origin from up, clockwise, by the amount of degrees specified. For example, 90 degrees shifts the origin to horizontal right.
    {
        float angle = Mathf.Atan2(vector2.x, vector2.y)*Mathf.Rad2Deg;
        angle = degOffset-angle;
        if(angle>180)
            angle = -360+angle;
        if(angle<-180)
            angle = 360+angle;
        return angle;
    }

    public float Get2DAngle(Vector2 vector2) // Get angle, from -180 to +180 degrees. Degree offset to horizontal right.
    {
        float angle = Mathf.Atan2(vector2.x, vector2.y)*Mathf.Rad2Deg;
        angle = 90-angle;
        if(angle>180)
            angle = -360+angle;
        return angle;
    }

    public float Get2DAngle(Vector3 vector3, float degOffset)
    {
        float angle = Mathf.Atan2(vector3.x, vector3.y)*Mathf.Rad2Deg;
        angle = degOffset-angle;
        if(angle>180)
            angle = -360+angle;
        return angle;
    }

    public float Get2DAngle(Vector3 vector3)
    {
        float angle = Mathf.Atan2(vector3.x, vector3.y)*Mathf.Rad2Deg;
        angle = 90-angle;
        if(angle>180)
            angle = -360+angle;
        return angle;
    }

    public bool isAlive()
    {
        return !FighterState.Dead;
    }

    public bool isSliding()
    {
        return v.sliding;
    }

    public void InstantForce(Vector2 newDirection, float speed)
    {
        //newDirection.Normalize();
        SetSpeed(newDirection, speed);
        //DirectionChange(newDirection);
        //print("Changing direction to" +newDirection);
    }

    public bool IsDisabled()
    {
        if(FighterState.Dead)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool IsKinematic()
    {
        return k_IsKinematic;
    }

    public Vector2 GetVelocity()
    {
        return FighterState.Vel;
    }

    public Vector2 GetPosition()
    {
        return FighterState.FinalPos;
    }

    public Vector2 GetFootPosition()
    {
        return m_GroundFoot.position;
    }

    public float GetSpeed()
    {
        return FighterState.Vel.magnitude;
    }

    public void SetSpeed(Vector2 inputVelocity, float speed)
    {
        //print("SetSpeed");
        Vector2 newVelocity;
        Vector2 direction = inputVelocity.normalized;
        newVelocity = direction * speed;
        FighterState.Vel = newVelocity;
    }

    public void SetSpeed(float speed)
    {
        //print("SetSpeed");
        Vector2 newVelocity;
        Vector2 direction = FighterState.Vel.normalized;
        newVelocity = direction * speed;
        FighterState.Vel = newVelocity;
    }
    #endregion
}

[System.Serializable] public class AudioVisualVars
{

    [SerializeField][ReadOnlyAttribute]public bool facingDirection; 	// True means right, false means left.
    [SerializeField][ReadOnlyAttribute]public int facingDirectionV; 	// 1 means up, -1 means down, and 0 means horizontal.
    [SerializeField][Range(0,10)]public float reversingSlideT;		// How fast the fighter must be going to go into a slide posture when changing directions.
    [SerializeField][Range(0,1)] public int cameraMode; 			 	// What camera control type is in use.
    [SerializeField][Range(0,1)] public int defaultCameraMode;		// What camera control type to default to in normal gameplay.
    [SerializeField][Range(0,1)] public float cameraXLeashM; 			// How close the player can get to the edge of the screen horizontally. 1 is at the edge, whereas 0 is locked to the center of the screen.
    [SerializeField][Range(0,1)] public float cameraYLeashM; 			// How close the player can get to the edge of the screen horizontally. 1 is at the edge, whereas 0 is locked to the center of the screen.
    [SerializeField][Range(0,1)] public float cameraXLeashLim; 	 	// MUST BE SET HIGHER THAN LEASHM. Same as above, except when it reaches this threshold it instantly stops the camera at the edge rather than interpolating it there.
    [SerializeField][Range(0,1)] public float cameraYLeashLim; 	 	// MUST BE SET HIGHER THAN LEASHM. Same as above, except when it reaches this threshold it instantly stops the camera at the edge rather than interpolating it there.
    [SerializeField][ReadOnlyAttribute] public float airForgiveness;	// Amount of time the player can be in the air without animating as airborne. Useful for micromovements. NEEDS TO BE IMPLEMENTED
    [SerializeField][Range(0,1)]public float punchStrengthSlowmoT;	// Percent of maximum clash power at which a player's attack will activate slow motion.
    [SerializeField][ReadOnlyAttribute]public float leanAngle;		// The angle the sprite is rotated to simulate leaning. Used to improve animation realism by leaning against GForces and wind drag. 
    [SerializeField][ReadOnlyAttribute]public int primarySurface;		// The main surface the player is running on. -1 is airborne, 0 is ground, 1 is ceiling, 2 is leftwall, 3 is rightwall. Lingers for a moment before going airborne, in order to hide microbumps in the terrain which would cause animation stuttering.
    [SerializeField][ReadOnlyAttribute]public int truePrimarySurface;	// The main surface the player is running on. -1 is airborne, 0 is ground, 1 is ceiling, 2 is leftwall, 3 is rightwall. More accurate version of primary surface that does not linger for a moment upon leaving a surface. 
    [SerializeField][ReadOnlyAttribute]public bool wallSliding;		// Whether or not the player is wallsliding.
    [SerializeField][ReadOnlyAttribute]public bool sliding;			// Whether or not the player is sliding.
    [SerializeField][ReadOnlyAttribute]public string[] terrainType;	// Type of terrain the player is stepping on. Used for audio like footsteps.



    public void SetDefaults()
    {
        reversingSlideT = 5;			// How fast the fighter must be going to go into a slide posture when changing directions.
        defaultCameraMode = 1;		// What camera control type to default to in normal gameplay.
        cameraXLeashM = 0.5f; 		// How close the player can get to the edge of the screen horizontally. 1 is at the edge, whereas 0 is locked to the center of the screen.
        cameraYLeashM = 0.5f; 		// How close the player can get to the edge of the screen horizontally. 1 is at the edge, whereas 0 is locked to the center of the screen.
        cameraXLeashLim = 0.8f; 	 	// MUST BE SET HIGHER THAN LEASHM. Same as above, except when it reaches this threshold it instantly stops the camera at the edge rather than interpolating it there.
        cameraYLeashLim = 0.8f;; 	 	// MUST BE SET HIGHER THAN LEASHM. Same as above, except when it reaches this threshold it instantly stops the camera at the edge rather than interpolating it there.
        punchStrengthSlowmoT = 0.5f;	// Percent of maximum clash power at which a player's attack will activate slow motion.
    }
}

[System.Serializable] public struct PhysicsVars
{
    [SerializeField][ReadOnlyAttribute] public float IGF; 					//"Instant G-Force" of the impact this frame.
    [SerializeField][ReadOnlyAttribute] public float CGF; 					//"Continuous G-Force" over time.
    [SerializeField][ReadOnlyAttribute] public float remainingVelM;		    //Remaining velocity proportion after an impact. Range: 0-1.
    [SerializeField][ReadOnlyAttribute] public Vector2 initialVel;			//Velocity at the start of the physics frame.
    [SerializeField][ReadOnlyAttribute] public Vector2 distanceTravelled;	//(x,y) distance travelled on current frame. Inversely proportional to remainingMovement.
    [SerializeField][ReadOnlyAttribute] public Vector2 remainingMovement; 	//Remaining (x,y) movement after impact.
    [SerializeField][ReadOnlyAttribute] public bool groundContact;			//True when touching surface.
    [SerializeField][ReadOnlyAttribute] public bool ceilingContact;			//True when touching surface.
    [SerializeField][ReadOnlyAttribute] public bool leftSideContact;		//True when touching surface.
    [SerializeField][ReadOnlyAttribute] public bool rightSideContact;		//True when touching surface.
    [Space(10)]						    
    [SerializeField][ReadOnlyAttribute] public bool grounded;				// True when making contact with this direction.
    [SerializeField][ReadOnlyAttribute] public bool ceilinged; 			    // True when making contact with this direction.
    [SerializeField][ReadOnlyAttribute] public bool leftWalled; 			// True when making contact with this direction.
    [SerializeField][ReadOnlyAttribute] public bool rightWalled;			// True when making contact with this direction.
    [Space(10)]						    
    [SerializeField][ReadOnlyAttribute] public bool groundBlocked;		    // True when the player cannot move in this direction and movement input towards it is ignored.
    [SerializeField][ReadOnlyAttribute] public bool ceilingBlocked; 		// True when the player cannot move in this direction and movement input towards it is ignored.
    [SerializeField][ReadOnlyAttribute] public bool leftWallBlocked; 		// True when the player cannot move in this direction and movement input towards it is ignored.
    [SerializeField][ReadOnlyAttribute] public bool rightWallBlocked; 	    // True when the player cannot move in this direction and movement input towards it is ignored.
    [Space(10)]						    
    [SerializeField][ReadOnlyAttribute] public bool surfaceCling;			//True when the player is clinging to an upside down surface. Whenever the player hits an upside down surface they have a grace period before gravity pulls them off.
    [SerializeField][ReadOnlyAttribute] public bool airborne;
    [SerializeField][ReadOnlyAttribute] public bool kneeling;				//True when fighter kneeling.
    [SerializeField][ReadOnlyAttribute] public bool worldImpact;			//True when the fighter has hit terrain on the current frame.
    [SerializeField] [ReadOnlyAttribute] public Vector3 lastSafePosition;	//Used to revert player position if they get totally stuck in something.

}

[System.Serializable] public struct DebugVars //Debug variables.
{
    [SerializeField] public int errorDetectingRecursionCount; 	//Iterates each time recursive trajectory correction executes on the current frame. Not currently used.
    [SerializeField] public bool autoPressLeft; 				// When true, fighter will behave as if the left key is pressed.
    [SerializeField] public bool autoPressRight; 				// When true, fighter will behave as if the right key is pressed.	
    [SerializeField] public bool autoPressDown; 				// When true, fighter will behave as if the left key is pressed.
    [SerializeField] public bool autoPressUp; 					// When true, fighter will behave as if the right key is pressed.
    [SerializeField] public bool autoJump;						// When true, fighter jumps instantly on every surface.
    [SerializeField] public bool autoLeftClick;					// When true, fighter will behave as if left click is pressed.
    [SerializeField] public bool antiTunneling;					// When true, fighter will be pushed out of objects they are stuck in.
    [SerializeField] public bool gravityEnabled;				// Enables gravity.
    [SerializeField] public bool showVelocityIndicator;			// Shows a line tracing the character's movement path.
    [SerializeField] public bool showContactIndicators;			// Shows fighter's surface-contact raycasts, which turn green when touching something.
    [SerializeField] public bool recoverFromFullEmbed;			// When true and the fighter is fully stuck in something, teleports fighter to last good position.
    [SerializeField] public bool clickToKnockFighter;			// When true and you left click, the fighter is propelled toward where you clicked.
    [SerializeField] public bool sendCollisionMessages;			// When true, the console prints messages related to collision detection
    [SerializeField] public bool sendTractionMessages;			// When true, the console prints messages related to collision detection
    [SerializeField] public bool invincible;					// When true, the fighter does not take damage of any kind.	[SerializeField]private int d.tickCounter; 								// Counts which game logic tick the game is on. Rolls over at 60.
    [SerializeField] public int tickCounter; 					// Counts which game logic tick the game is on. Rolls over at 60
    [SerializeField] public LineRenderer debugLine; 			// Part of above indicators.
    [SerializeField] public LineRenderer groundLine;			// Part of above indicators.		
    [SerializeField] public LineRenderer ceilingLine;			// Part of above indicators.		
    [SerializeField] public LineRenderer leftSideLine;			// Part of above indicators.		
    [SerializeField] public LineRenderer rightSideLine;			// Part of above indicators.
}
[System.Serializable] public struct MovementVars 
{
    [Tooltip("The instant starting speed while moving")]
    [SerializeField] public float minSpeed; 						

    [Tooltip("The fastest the fighter can travel along land.")]
    [SerializeField] public float maxRunSpeed;					

    [Tooltip("Speed the fighter accelerates within the traction change threshold. (acceleration while changing direction)")]
    [Range(0,2)][SerializeField] public float startupAccelRate;   

    [Tooltip("How fast the fighter accelerates with input.")]
    [Range(0,5)][SerializeField] public float linearAccelRate;		

    [Tooltip("Amount of vertical force added when the fighter jumps.")]
    [SerializeField] public float vJumpForce;                  		

    [Tooltip("Amount of horizontal force added when the fighter jumps.")]
    [SerializeField] public float hJumpForce;  						

    [Tooltip("Amount of vertical force added when the fighter walljumps.")]
    [SerializeField] public float wallVJumpForce;                  	

    [Tooltip("Amount of horizontal force added when the fighter walljumps.")]
    [SerializeField] public float wallHJumpForce;  					

    [Tooltip("Threshold where movement changes from exponential to linear acceleration.")]
    [SerializeField] public float tractionChangeT;					

    [Tooltip("Speed threshold at which wallsliding traction changes.")]	
    [SerializeField] public float wallTractionT;						

    [Tooltip("How fast the fighter decelerates when changing direction.")]
    [Range(0,5)][SerializeField] public float linearStopRate; 		

    [Tooltip("How fast the fighter decelerates with no input.")]
    [Range(0,5)][SerializeField] public float linearSlideRate;		

    [Tooltip("How fast the fighter decelerates when running too fast.")]
    [Range(0,5)][SerializeField] public float linearOverSpeedRate;	

    [Tooltip("Any impacts at sharper angles than this will start to slow the fighter down.")]
    [Range(1,89)][SerializeField] public float impactDecelMinAngle;	

    [Tooltip("Any impacts at sharper angles than this will result in a full halt.")]
    [Range(1,89)][SerializeField] public float impactDecelMaxAngle;	

    [Tooltip("Changes the angle at which steeper angles start to linearly lose traction")]
    [Range(1,89)][SerializeField] public float tractionLossMinAngle; 

    [Tooltip("Changes the angle at which fighter loses ALL traction")][Range(45,90)]
    [SerializeField] public float tractionLossMaxAngle;

    [Tooltip("Changes how fast the fighter slides down overly steep slopes.")]
    [Range(0,2)][SerializeField] public float slippingAcceleration;  	

    [Tooltip("How long the fighter can cling to walls before gravity takes over.")]
    [Range(0.5f,3)][SerializeField] public float surfaceClingTime; 	

    [Tooltip("This is the amount of impact GForce required for a full-duration ceiling cling.")]
    [Range(20,70)][SerializeField] public float clingReqGForce;		

    [Tooltip("This is the normal of the last surface clung to, to make sure the fighter doesn't repeatedly cling the same surface after clingtime expires.")]
    [ReadOnlyAttribute]public Vector2 expiredNormal;						

    [Tooltip("Amount of time the fighter has been clung to a wall.")]
    [ReadOnlyAttribute]public float timeSpentHanging;					

    [Tooltip("Max time the fighter can cling to a wall.")]
    [ReadOnlyAttribute]public float maxTimeHanging;					

    [Tooltip("How deep into objects the character can be before actually colliding with the ")]
    [Range(0,0.5f)][SerializeField]public float maxEmbed;			

    [Tooltip("How deep into objects the character will sit by default. A value of zero will cause physics errors because the fighter is not technically *touching* the surface.")]
    [Range(0.01f,0.4f)][SerializeField]public float minEmbed;

    [Tooltip("How much horizontal movement control the player has while airborne")]
    [Range(0.01f, 0.9f)] [SerializeField] public float airControlStrength;


    [Space(10)]

    [Tooltip("Provides an n frame buffer to allow players to jump after leaving the ground.")]
    [SerializeField][ReadOnlyAttribute]public int jumpBufferG;

    [Tooltip("Provides an n frame buffer to allow players to jump after leaving the ceiling.")]
    [SerializeField][ReadOnlyAttribute]public int jumpBufferC;

    [Tooltip("Provides an n frame buffer to allow players to jump after leaving the leftwall.")]
    [SerializeField][ReadOnlyAttribute]public int jumpBufferL;

    [Tooltip("Provides an n frame buffer to allow players to jump after leaving the rightwall.")]
    [SerializeField][ReadOnlyAttribute]public int jumpBufferR;

    [Tooltip("Dictates the duration of the jump buffer (in physics frames).")]
    [SerializeField][Range(1,600)] public int jumpBufferFrameAmount;

    [Tooltip("Amount of time after leaving the ground that the player behaves as if they are airborne. Prevents jittering caused by small bumps in the environment.")]
    [SerializeField][Range(0,2)] public float airborneDelay;

    [Tooltip("Time remaining before the player is treated as airborne upon leaving a surface.")]
    [SerializeField][ReadOnlyAttribute] public float airborneDelayTimer;



    public void SetDefaults()
    {
        airborneDelayTimer = 0;
        expiredNormal = Vector2.zero;
        jumpBufferC = 0;
        jumpBufferG = 0;
        jumpBufferL = 0;
        jumpBufferR = 0;

        minSpeed = 10f; 						
        maxRunSpeed = 200f;					
        startupAccelRate = 0.8f;   
        linearAccelRate = 0.4f;		
        vJumpForce = 25f;                  		
        hJumpForce = 5f;  						
        wallVJumpForce = 20f;                  	
        wallHJumpForce = 10f;  					
        tractionChangeT = 20f;					
        wallTractionT = 20f;						
        linearStopRate = 2f; 		
        linearSlideRate = 0.35f;		
        linearOverSpeedRate = 0.1f;	
        impactDecelMinAngle = 20f;	
        impactDecelMaxAngle = 80f;	
        tractionLossMinAngle = 45f; 
        tractionLossMaxAngle = 78f;
        slippingAcceleration = 1f;  	
        surfaceClingTime = 1f; 	
        clingReqGForce = 50f;		
        timeSpentHanging = 0f;					
        maxTimeHanging = 0f;					
        maxEmbed = 0.02f;			
        minEmbed = 0.01f; 
        jumpBufferFrameAmount = 10; 			//Dictates the duration of the jump buffer (in physics frames).
        airborneDelay = 0.5f;                   //Amount of time after leaving the ground that the player behaves as if they are airborne. Prevents jittering caused by small bumps in the environment.
        airControlStrength = 0.05f;

}
}
[System.Serializable] public struct ObjectRefs 
{
    [SerializeField][ReadOnlyAttribute] public Transform spriteTransform;			// Reference to the velocity punch visual effect entity attached to the character.
    [SerializeField][ReadOnlyAttribute] public SpriteRenderer spriteRenderer;	// Reference to the character's sprite renderer.
    [SerializeField][ReadOnlyAttribute] public Animator anim;           		// Reference to the character's animator component.
    [SerializeField][ReadOnlyAttribute] public Rigidbody2D rigidbody2D;		// Reference to the character's physics body.
}
[System.Serializable] public struct FighterState
{
    [SerializeField][ReadOnlyAttribute]public bool DevMode;					// Turns on all dev cheats.
    [SerializeField][ReadOnlyAttribute]public bool Dead;					// True when the fighter's health reaches 0 and they die.
    [SerializeField][ReadOnlyAttribute]public int Stance;					// Combat stance which dictates combat actions and animations. 0 = neutral, 1 = attack(leftmouse), 2 = guard(rightclick). 

    [SerializeField][ReadOnlyAttribute]public bool JumpKeyPress;
    [SerializeField][ReadOnlyAttribute]public bool ShiftKeyPress;
    [SerializeField][ReadOnlyAttribute]public bool LeftClickPress;
    [SerializeField][ReadOnlyAttribute]public bool RightClickPress;
    [SerializeField][ReadOnlyAttribute]public bool LeftKeyPress;
    [SerializeField][ReadOnlyAttribute]public bool RightKeyPress;
    [SerializeField][ReadOnlyAttribute]public bool UpKeyPress;
    [SerializeField][ReadOnlyAttribute]public bool DownKeyPress;

    [SerializeField][ReadOnlyAttribute]public float ScrollWheel;

    [SerializeField][ReadOnlyAttribute]public bool LeftClickHold;
    [SerializeField][ReadOnlyAttribute]public bool RightClickHold;
    [SerializeField][ReadOnlyAttribute]public bool ShiftKeyHold;
    [SerializeField][ReadOnlyAttribute]public bool LeftKeyHold;
    [SerializeField][ReadOnlyAttribute]public bool RightKeyHold;
    [SerializeField][ReadOnlyAttribute]public bool UpKeyHold;
    [SerializeField][ReadOnlyAttribute]public bool DownKeyHold;

    [SerializeField][ReadOnlyAttribute]public bool RightClickRelease;
    [SerializeField][ReadOnlyAttribute]public bool LeftClickRelease;
    [SerializeField][ReadOnlyAttribute]public bool ShiftKeyRelease;
    [SerializeField][ReadOnlyAttribute]public bool LeftKeyRelease;
    [SerializeField][ReadOnlyAttribute]public bool RightKeyRelease;
    [SerializeField][ReadOnlyAttribute]public bool UpKeyRelease;
    [SerializeField][ReadOnlyAttribute]public bool DownKeyRelease;

    [SerializeField][ReadOnlyAttribute]public bool LeftKeyDoubleTapReady;
    [SerializeField][ReadOnlyAttribute]public bool RightKeyDoubleTapReady;
    [SerializeField][ReadOnlyAttribute]public bool UpKeyDoubleTapReady;
    [SerializeField][ReadOnlyAttribute]public bool DownKeyDoubleTapReady;

    [SerializeField][ReadOnlyAttribute]public float LeftKeyDoubleTapDelay;
    [SerializeField][ReadOnlyAttribute]public float RightKeyDoubleTapDelay;
    [SerializeField][ReadOnlyAttribute]public float UpKeyDoubleTapDelay;
    [SerializeField][ReadOnlyAttribute]public float DownKeyDoubleTapDelay;



    [SerializeField][ReadOnlyAttribute]public bool QKeyPress;
    [SerializeField][ReadOnlyAttribute]public bool DisperseKeyPress;
    [SerializeField][ReadOnlyAttribute]public bool DevkeyTilde;
    [SerializeField][ReadOnlyAttribute]public bool DevKey1;
    [SerializeField][ReadOnlyAttribute]public bool DevKey2;
    [SerializeField][ReadOnlyAttribute]public bool DevKey3;
    [SerializeField][ReadOnlyAttribute]public bool DevKey4;
    [SerializeField][ReadOnlyAttribute]public bool DevKey5;
    [SerializeField][ReadOnlyAttribute]public bool DevKey6;
    [SerializeField][ReadOnlyAttribute]public bool DevKey7;
    [SerializeField][ReadOnlyAttribute]public bool DevKey8;
    [SerializeField][ReadOnlyAttribute]public bool DevKey9;
    [SerializeField][ReadOnlyAttribute]public bool DevKey10;
    [SerializeField][ReadOnlyAttribute]public bool DevKey11;
    [SerializeField][ReadOnlyAttribute]public bool DevKey12;
    [SerializeField][ReadOnlyAttribute]public float LeftClickHoldDuration;
    [SerializeField][ReadOnlyAttribute]public Vector2 MouseWorldPos;				// Mouse position in world coordinates.
    [SerializeField][ReadOnlyAttribute]public Vector2 PlayerMouseVector;			// Vector pointing from the player to their mouse position.
    [SerializeField][ReadOnlyAttribute]public Vector2 Vel;							//Current (x,y) velocity.
    [SerializeField]public Vector2 FinalPos;						//The final position of the character at the end of the physics frame.
}