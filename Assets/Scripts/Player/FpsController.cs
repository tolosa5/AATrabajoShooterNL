using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FpsController : MonoBehaviour
{
    GameObject cam;
    Camera camComp;
    [HideInInspector] public Vector3 movementY;
    Vector3 direction;
    float factorG = -12f;
    float jumpHeight = 1.5f;
    
    //Doble salto?
    int numberOfJumps;

    CharacterController chC;
    Animator anim;
    Player playerScr;
    Rigidbody rb;


    //cosas para movimiento
    [HideInInspector] public float h;
    [HideInInspector] public float v;
    float currentSpeed;
    float standingSpeed = 7;
    float crouchingSpeed = 5;
    float runningSpeed = 11;
    float slidingSpeed = 20;
    float stamina;
    float slideCooldown;
    float dashCooldown;
    bool ableToSlide;
    bool ableToDash = true;
    bool isSliding;
    bool isCrouching;
    bool isRunning;
    bool isDashing;
    bool isGrappling;

    bool isHooking;


    //Gancho
    GameObject lastWall;
    [HideInInspector] public bool onWall;
    [HideInInspector] public bool onAir;
    Vector3 hookShot;
    [SerializeField] LayerMask isHookeable, isWalkable;
    bool activated;
    bool corniseHooked;
    bool wallHooked;
    Vector3 aimedPoint;

    //Muros
    List<Collider> nearbyColls = new List<Collider>();
    GameObject closestWall;
    [SerializeField] MouseMovement mousemove;
    float wallSpeed = 12;
    float smoothRotationFactor = 0.1f;
    float smoothRotationVel;
    float wallJumping;
    public bool wall;
    bool goneFromWall;

    bool left;
    bool right;

    //Estados del Script
    State state;


    enum State
    {
        Normal,
        Flying,
        Hanging,
        Sliding,
        WallWalking,
        Climbing
    }

    void Start()
    {
        cam = Camera.main.gameObject;
        camComp = cam.GetComponent<Camera>();
        chC = GetComponent<CharacterController>();
        anim = GetComponent<Animator>();
        playerScr = GetComponent<Player>();
        rb = GetComponent<Rigidbody>();

        currentSpeed = standingSpeed;
        ableToSlide = true;
    }

    void Update()
    {
        #region MaquinaEstados

        switch (state)
        {
            default:
            case State.Normal:
                Movement();
                Inputs();
                Jump();
                Run();
                StopRunning();
                Crouch();
                StopCrouching();
                Dash();
                StartsSliding();
                FovChanges();
                HookStart();
                WallDetecting();
                
                break;
            
            case State.Flying:
                if (corniseHooked)
                {
                    HookMovementCornise();
                }
                else if (wallHooked)
                {
                    HookMovementWall();
                }
                FovChanges();
                
                break;

            case State.Hanging:
                Inputs();
                Climb();
                Dash();
                HookStart();

                break;
                
            case State.Sliding:
                Slide();
                FovChanges();

                break;

            case State.WallWalking:
                Inputs();
                MovementWall();
                WallWalk();
                HookStart();
                StopWallWalking();

                break;

            case State.Climbing:
                Climbing();
                break;

        }
        #endregion

        //Funcionamiento del dash
        if (isDashing)
        {
            dashCooldown += Time.deltaTime;
            if(dashCooldown <= 0.2f)
            {
                chC.Move(cam.transform.forward * 80 * Time.deltaTime);
                
            }
            else if (dashCooldown > 0.2f && dashCooldown <= 2f)
            {
                if (state == State.Hanging)
                {
                    state = State.Normal;
                }
            }
            else if(dashCooldown > 2f)
            {
                isDashing = false;
                dashCooldown = 0;
            }
        }
    }
    
    void Inputs()
    {
        h = Input.GetAxisRaw("Horizontal");
        if (state != State.Hanging)
        {
            v = Input.GetAxisRaw("Vertical");
        }
    }

    void Movement()
    {
        if (isRunning)
        {
            stamina += (int)Time.deltaTime;
        }

        direction = transform.forward * v + transform.right * h;

        if (!isDashing || (isDashing && dashCooldown > 0.2f && dashCooldown < 2f))
        {
            chC.Move(direction.normalized * currentSpeed * Time.deltaTime);
        }
    }

    void MovementWall()
    {
        currentSpeed = wallSpeed;
        if (left)
        {
            direction = transform.forward * v + transform.up * -h;

        }
        else if (right)
        {
            direction = transform.forward * v + transform.up * h;
        }

        if (!isDashing || (isDashing && dashCooldown > 0.15f && dashCooldown < 2f))
        {
            chC.Move(direction.normalized * currentSpeed * Time.deltaTime);

        }
    }

    void Jump()
    {
        //Translate para la gravedad. Dos deltas porque m/s^2
        movementY.y += factorG * Time.deltaTime;

        chC.Move(movementY * Time.deltaTime);

        if (chC.isGrounded)
        {
            //Saltar
            movementY.y = 0;
            if (Input.GetKeyDown(KeyCode.Space) && !isCrouching)
            {
                movementY.y = Mathf.Sqrt(jumpHeight * -2f * factorG);
            }
        }
    }
    
    void Crouch()
    {
        if (chC.isGrounded)
        {
            if (!isRunning)
            {
                if (Input.GetKeyDown(KeyCode.C))
                {
                    anim.SetBool("CrouchBool", true);
                    currentSpeed = crouchingSpeed;
                    isCrouching = true;
                }
            }
        }
    }
    
    void StopCrouching()
    {
        if (!isRunning)
        {
            if (Input.GetKeyUp(KeyCode.C))
            {
                anim.SetBool("CrouchBool", false);
                currentSpeed = standingSpeed;
                isCrouching = false;
            }
        }
    }  

    void Run()
    {
        if(chC.isGrounded)
        {
            //Correr y dejar de correr
            if (Input.GetKey(KeyCode.LeftControl) && isRunning == false && !isCrouching && stamina <= 5)
            {
                isRunning = true;
                currentSpeed = runningSpeed;
                anim.SetBool("RunningBool", true);
            }
        }
    }

    void StopRunning()
    {
        if (Input.GetKeyUp(KeyCode.LeftControl) && isRunning == true || stamina > 5 )
        {
            isRunning = false;
            currentSpeed = standingSpeed;
            anim.SetBool("RunningBool", false);
            camComp.fieldOfView = Mathf.Lerp(camComp.fieldOfView, 85, 5 * Time.deltaTime);
        
            stamina = 0; 
        }
    }

    void StartsSliding()
    {
        if (isRunning && Input.GetKeyDown(KeyCode.C) && ableToSlide)
        {
            state = State.Sliding;
            isSliding = true;
        }
    }

    void FovChanges()
    {
        if (isSliding)
        {
            camComp.fieldOfView = Mathf.Lerp(camComp.fieldOfView, 60, 5 * Time.deltaTime);
        }
        else if (!isSliding)
        {
            camComp.fieldOfView = Mathf.Lerp(camComp.fieldOfView, 85, 5 * Time.deltaTime);
        }
        
        if (isRunning || wall)
        {
            camComp.fieldOfView = Mathf.Lerp(camComp.fieldOfView, 100, 5 * Time.deltaTime);
        }
        else if (!isRunning || !wall)
        {
            camComp.fieldOfView = Mathf.Lerp(camComp.fieldOfView, 85, 5 * Time.deltaTime);
        }

        if (isHooking)
        {
            camComp.fieldOfView = Mathf.Lerp(camComp.fieldOfView, 100, 5 * Time.deltaTime);
        }
        else if (!isHooking)
        {
            camComp.fieldOfView = Mathf.Lerp(camComp.fieldOfView, 85, 5 * Time.deltaTime);
        }
    }

    void Slide()
    {
        slideCooldown += Time.deltaTime;
        
        if (slideCooldown >= 0 && slideCooldown <= 0.5f)
        {
            chC.Move(transform.forward * slidingSpeed * Time.deltaTime);
            anim.SetBool("Dashing", true);
        }

        else if (slideCooldown > 0.5f)
        {
            anim.SetBool("Dashing", false);
            slideCooldown = 0;
            isSliding = false;
            ableToSlide = true;
            state = State.Normal;
        }
    }

    void Dash()
    {
        if (Input.GetKeyDown(KeyCode.F) && !isDashing)
        {
            isDashing = true;
        }
    }
    

    #region Ganchos

    void HookStart()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            RaycastHit hit;
            if(Physics.Raycast(cam.transform.position, cam.transform.forward, out hit, 200, isHookeable))
            {
                if (hit.transform.gameObject.CompareTag("Cornise"))
                {
                    hookShot = hit.point - new Vector3(0, -1f, 0.5f);
                    if (wallHooked)
                    {
                        wallHooked = false;
                    }
                    corniseHooked = true;
                    state = State.Flying;
                }
                else if (hit.transform.gameObject.CompareTag("Wall"))
                {
                    hookShot = hit.point;
                    if (corniseHooked)
                    {
                        corniseHooked = false;
                    }
                    wallHooked = true;
                    state = State.Flying;
                }
            }
        }
    }

    void HookMovementCornise()
    {
        isHooking = true;
        Vector3 hookshotDirection = (hookShot - transform.position).normalized;

        float minSpeed = 40f;
        float maxSpeed = 100f;
        float hookShotSpeed = Mathf.Clamp(Vector3.Distance(transform.position, hookShot), minSpeed, maxSpeed);
        float hookSpeedMultiplier = 3f;
        
        chC.Move(hookshotDirection * hookShotSpeed * hookSpeedMultiplier * Time.deltaTime);

        float minimumDistanceToHook = 1.5f;
        if (Vector3.Distance(transform.position, hookShot) < minimumDistanceToHook)
        {
            isHooking = false;
            state = State.Hanging;
            movementY.y = 0f;
        }
    }

    void HookMovementWall()
    {
        Vector3 hookshotDirection = (hookShot - transform.position).normalized;

        float minSpeed = 40f;
        float maxSpeed = 100f;
        float hookShotSpeed = Mathf.Clamp(Vector3.Distance(transform.position, hookShot), minSpeed, maxSpeed);
        float hookSpeedMultiplier = 3f;

        chC.Move(hookshotDirection * hookShotSpeed * hookSpeedMultiplier * Time.deltaTime);

        float minimumDistanceToHook = 1.5f;
        if (Vector3.Distance(transform.position, hookShot) < minimumDistanceToHook)
        {
            wallHooked = false;
            state = State.Normal;
            movementY.y = 0f;
        }
    }


    void Climb()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            aimedPoint = transform.position + (Vector3.up * 2);
            state = State.Climbing;
        }
    }

    void Climbing()
    {
        transform.position = Vector3.MoveTowards(transform.position, aimedPoint, 10 * Time.deltaTime);
        //moverlo en finiteUpdate o hacerlo animacion
            
        if (transform.position == aimedPoint)
        {
            state = State.Normal;
        }
    }

    #endregion

    #region CaminarPared

    void WallDetecting()
    {
        if (!chC.isGrounded)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                GoKinematic();
                //pillo los collider de izquierda y derecha
                Collider[] collsRight = Physics.OverlapSphere(transform.position + transform.right * 1, 0.5f, isWalkable);
                Collider[] collsLeft = Physics.OverlapSphere(transform.position + transform.right * -1, 0.5f, isWalkable);
                
                if(collsRight.Length > 0)
                {
                    wall = true;
                    foreach (Collider item in collsRight)
                    {
                        //se a�aden del array a una lista de los dos
                        nearbyColls.Add(item);
                    }
                    ClosestRightWall();
                }
                else if (collsLeft.Length > 0)
                {
                    foreach (Collider item in collsLeft)
                    {
                        nearbyColls.Add(item);
                    }
                    ClosestLeftWall();
                }
            }
        }
    }

    void ClosestLeftWall()
    {
        float bet = 0;
        foreach (Collider item in nearbyColls)
        {
            //pillo el vector entre los dos
            Vector3 dirToWall = (item.transform.position - transform.position).normalized;
            //pillo el producto de los dos vectores
            float dotProduct = Vector3.Dot(transform.forward, dirToWall);
            //valor absoluto comparandolo porque da igual que este delante o atras
            if(Mathf.Abs(dotProduct) > bet)
            {
                //le digo cual es el muro
                closestWall = item.transform.gameObject;
                bet = dotProduct;
                wallJumping = 1;
            }
        }
        if(closestWall != null)
        {
            Debug.Log(closestWall.name);
            if (!left)
            {
                left = true;
                Debug.Log(left);
            }
            state = State.WallWalking;
        }
    }
    
    void ClosestRightWall()
    {
        float bet = 0;
        foreach (Collider item in nearbyColls)
        {
            Vector3 dirToWall = (item.transform.position - transform.position).normalized;
            float dotProduct = Vector3.Dot(transform.forward, dirToWall);
            if (Mathf.Abs(dotProduct) > bet)
            {
                closestWall = item.transform.gameObject;
                bet = dotProduct;
                wallJumping = -1;
            }
        }
        if (closestWall != null)
        {
            Debug.Log(closestWall.name);
            if (!right)
            {
                right = true;

            }
            state = State.WallWalking;
        }
    }
    
    void WallWalk()
    {
        movementY.y += -2.5f * Time.deltaTime;
        if (movementY.y >= 5)
        {
            movementY.y = 5;
        }
        chC.Move(movementY * Time.deltaTime);
        
        if (left)
        {
            float newAngle = Mathf.SmoothDampAngle(cam.transform.localEulerAngles.z, -30, ref smoothRotationVel, smoothRotationFactor);
            cam.transform.localEulerAngles = new Vector3(cam.transform.localEulerAngles.x, cam.transform.localEulerAngles.y, newAngle);
        }
        else if (right)
        {
            float newAngle = Mathf.SmoothDampAngle(cam.transform.localEulerAngles.z, 30, ref smoothRotationVel, smoothRotationFactor);
            cam.transform.localEulerAngles = new Vector3(0, 0, newAngle);
        }

    }

    void StopWallWalking()
    {
        if (playerScr.stopFpsWall || chC.isGrounded || goneFromWall)
        {
            goneFromWall = false;
            wall = false;
            state = State.Normal;
        }
    }

    private void OnCollisionExit(Collision collision) 
    {
        if (collision.gameObject.CompareTag("WallRun"))
        {
            goneFromWall = true;
        }
    }

    #endregion

    //Se llaman en PlayerScr
    public void GoDinamic()
    {
        isGrappling = true;
        rb.isKinematic = false;
        chC.enabled = false;
    }

    public void GoKinematic()
    {
        isGrappling = false;
        rb.isKinematic = true;
        chC.enabled = true;
    }

    IEnumerator TimeScaling()
    {
        activated = true;
        Time.timeScale = 0.3f;
        yield return new WaitForSecondsRealtime(0.1f);
        Time.timeScale = 1;
        activated = false;
    }

}