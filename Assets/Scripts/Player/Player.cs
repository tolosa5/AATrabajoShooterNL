using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class Player : MonoBehaviour
{
    [SerializeField] GameObject sword;
    [SerializeField] Transform gameZone;
    
    bool swordActive;

    [HideInInspector] public int lifes = 100;

    CharacterController characterController;

    Animator anim;
    AnimatorStateInfo animState;


    FpsController controllerScr;

    //Ataques
    [SerializeField] Transform blade;
    Enemies meleeScr;
    RangedEnemie rangedScr;
    float lastAttack;
    float attackRate;
    int damage = 50;

    GameObject rangedEnemy;
    GameObject meleeEnemy;

    
    //Coger objetos
    GameObject heldObject;
    [SerializeField] Transform holder;


    //Liana
    [SerializeField] GameObject grapplingGun;
    Rigidbody rb;
    LineRenderer lR;
    float maxDistance = 500f;
    Vector3 aimedPoint;
    Vector3 gunTip;
    [SerializeField] GameObject hook;
    [SerializeField] LayerMask isHookeable, isInteractable, isPickeable, isAttackable;
    GameObject cam;
    SpringJoint joint;
    bool stoppedHooking;

    public bool stopFpsWall;

    void Start()
    {
        anim = GetComponent<Animator>();
        controllerScr = GetComponent<FpsController>();
        characterController = GetComponent<CharacterController>();

        rb = GetComponent<Rigidbody>();

        cam = Camera.main.gameObject;

        lR = hook.GetComponent<LineRenderer>();
        
    }

    void Update()
    {
        Inputs();
        Animations();
        Interact();
        
        gunTip = hook.transform.position;
        lastAttack += Time.deltaTime;

        if (heldObject != null) //no lo voy a usar porque no me da tiempo a darle utilidad pero esta guapo
        {
            MovePickObject();
        }
    }

    //animator
    void Attack()
    {
        Collider[] colls = Physics.OverlapSphere(blade.position, 0.2f, isAttackable);
        for (int i = 0; i < colls.Length; i++)
        {
            if (colls[i].gameObject.CompareTag("Melee"))
            {
                meleeScr = colls[i].gameObject.GetComponent<Enemies>();
                meleeScr.TakeDamage(damage);
            }
            else if (colls[i].gameObject.CompareTag("Ranged"))
            {
                rangedScr = colls[i].gameObject.GetComponent<RangedEnemie>();
                rangedScr.TakeDamage(damage);
            }
        }
    }
    
    public IEnumerator TakeDamage(int damageTaken)
    {
        lifes -= damageTaken;

        Debug.Log(lifes);
        if (lifes <= 0)
        {
            Death();
        }

        Color color = Color.red;
        yield return new WaitForSeconds(0.2f);
        color = Color.white;
    }

    void Interact()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            Collider[] colls = Physics.OverlapSphere(cam.transform.position + cam.transform.forward * 1.5f, 0.7f, isInteractable);
            for (int i = 0; i < colls.Length; i++)
            {
                if (colls[i].gameObject.CompareTag("TP"))
                {
                    this.gameObject.transform.position = new Vector3(180.95f, 3.34f, -420.82f);
                    Debug.Log("tp");
                }
                else if (colls[i].gameObject.CompareTag("GrapplingGun"))
                {
                    grapplingGun.SetActive(true);
                    Destroy(colls[i].gameObject);
                }
            }
        }
    }
    
    void Animations()
    {
        if (controllerScr.h != 0 || controllerScr.v != 0)
        {
            anim.SetBool("WalkingBool", true);
        }
        else
        {
            anim.SetBool("WalkingBool", false);
        }
    }

    void Inputs()
    {
        if (Input.GetMouseButtonDown(2))
        {
            StartGrappling();

        }
        if (Input.GetMouseButtonUp(2))
        {
            StopGrappling();
        }

        //agarrar objetos
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (heldObject == null)
            {
                //si al pulsar la e no tienes un objeto sujetado ya, puedes sujetar uno
                RaycastHit hit;
                if (Physics.Raycast(transform.position, transform.forward, out hit, 5, isPickeable))
                {
                    PickUpObject(hit.transform.gameObject);
                }
            }
            else
            {
                //si ya tienes uno sujetado y pulsas la e, lo dropeas
                DropObjects();
            }
        }
        
        if (Input.GetMouseButtonDown(0) && lastAttack >= attackRate)
        {
            anim.SetTrigger("AttackTrigger");
        }
    }

    private void OnDrawGizmos()
    {
        cam = Camera.main.gameObject;
        Gizmos.DrawSphere(cam.transform.position + cam.transform.forward * 1.5f, 0.7f);
    }

    void Death()
    {
        GameManager.gM.death = true;
    }


    #region Liana
    private void LateUpdate() 
    {
        //para que pueda seguir al update y no se anteponga y no aparezca rara
        //hay que llamarlo despues del update
        RopeMaker();
    }

    void StartGrappling()
    {
        stopFpsWall = true;
        //para evitar problema logico con el de cuando ha acabado, se pone en falso si esta activo
        if (stoppedHooking)
        {
            stoppedHooking = false;
        }

        RaycastHit hit;
        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out hit, maxDistance, isHookeable))
        {
            //el player se hace dinamico, pierde el chc y el collider, asi se evitan problemas
            controllerScr.GoDinamic();
            //donde impacte es el sitio que se esta apuntando y que se va a usar ahora
            aimedPoint = hit.point;
            
            //anclaje creado al disparar, crear un joint y dispararlo un joint
            joint = gameObject.AddComponent<SpringJoint>();
            //evitar que se calcule automaticamente, lo calculas tu dandole valor
            joint.autoConfigureConnectedAnchor = false;
            //pilla valor el joint, el sitio al que se ha disparado el raycast
            joint.connectedAnchor = aimedPoint;

            //calcular la distancia, no deja restar sin mas
            float distance = Vector3.Distance(transform.position, aimedPoint);

            //hace que te atraiga y que la cuerda ese en tesion
            //multiplicacion para no dejarte nunca en el suelo de ragdoll
            joint.maxDistance = distance * 0.7f;
            joint.minDistance = distance * 0.3f;

            joint.spring = 500f;
            joint.damper = 600f;
            joint.massScale = 300f;

            //crea la cuerda con dos vertices, lo que se ve en pantalla
            lR.positionCount = 2;
        }
    }

    void RopeMaker()
    {
        //si no hay anclaje, osea, no esta activado, devuelve vacio: no pasa nada
        if (!joint)
        {
            return;
        }
        
        //indicas donde debe estar cada vertice, uno desde donde lo lanzas
        //y otro en donde es lanzado
        lR.SetPosition(1, aimedPoint);
        lR.SetPosition(0, gunTip);
    }

    void StopGrappling()
    {
        //da la señal de haber dejado de estar sujeto
        stoppedHooking = true;
        
        //se eliminan los vertices asi que no aparece la cuerda que te une con el joint
        lR.positionCount = 0;
        //se destruye el joint
        Destroy(joint);
        stopFpsWall = false;

        rb.AddForce(transform.forward * 300, ForceMode.Impulse);
        rb.AddForce(transform.up * 50, ForceMode.Impulse);
    }

    private void OnCollisionEnter(Collision collision)
    {
        //al tocar algo, deja de tener la inercia del movimiendo dinamico y 
        //se vuelve kinematico de nuevo
        if (stoppedHooking)
        {
            controllerScr.GoKinematic();
        }
    }

    #endregion

    private void OnTriggerEnter(Collider other) 
    {
        if (other.gameObject.CompareTag("RangedDetection"))
        {
            rangedEnemy = other.transform.parent.gameObject;
            rangedScr = rangedEnemy.GetComponent<RangedEnemie>();
            rangedScr.rangedDetected = true;
        }
        else if (other.gameObject.CompareTag("MeleeDetection"))
        {
            Debug.Log("pito");
            meleeEnemy = other.transform.parent.gameObject;
            Debug.Log("ey");
            meleeScr = meleeEnemy.GetComponent<Enemies>();
            meleeScr.meleeDetected = true;
        }
        if (other.gameObject.CompareTag("Final"))
        {
            GameManager.gM.finalEntered = true;
            Debug.Log("Final!!");
        }
        if (other.gameObject.CompareTag("Death"))
        {
            Death();
        }
    }

    private void OnTriggerExit(Collider other) 
    {
        if (other.gameObject.CompareTag("RangedDetection"))
        {
            rangedEnemy = other.transform.parent.gameObject;
            rangedScr = rangedEnemy.GetComponent<RangedEnemie>();
            rangedScr.rangedDetected = false;
        }
        else if (other.gameObject.CompareTag("MeleeDetection"))
        {
            meleeEnemy = other.transform.parent.gameObject;
            meleeScr = meleeEnemy.GetComponent<Enemies>();
            meleeScr.meleeDetected = false;
        }
    }

    #region CogerObjetos

    void PickUpObject(GameObject pickObject)
    {
        //pilla el rb del objeto que coge
        Rigidbody pickRB = pickObject.GetComponent<Rigidbody>();
        //le quito gravedad para asi poder "levantarlo"
        pickRB.useGravity = false;
        //pa que gire 
        pickRB.drag = 10;
        
        //le hago hijo del holder, donde irá al ser cogido, para que asi se mueva respecto a la camara
        pickObject.transform.parent = holder;
        //le doy valor al objeto para saber que tengo uno cogid, cual es y operar con el
        heldObject = pickObject;
    }

    void MovePickObject()
    {
        if (Vector3.Distance(heldObject.transform.position, holder.position) > 0.1f)
        {
            //encuentro la diferencia entre la posicion del objeto y la de mi holder
            Vector3 moveDirection = (holder.position - heldObject.transform.position);
            //le añado fuerzas fisicas para moverlo respecto al movimiento de mi camara
            heldObject.GetComponent<Rigidbody>().AddForce(moveDirection * 250);
        }
    }

    void DropObjects()
    {
        //reset de todo
        Rigidbody pickRB = heldObject.GetComponent<Rigidbody>();
        pickRB.useGravity = true;
        pickRB.drag = 1;

        heldObject.transform.parent = null;
        heldObject = null;
    }
    #endregion
}
