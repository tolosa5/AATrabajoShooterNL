using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemies : MonoBehaviour
{
    int lifes = 100;
    int damage = 20;

    //mano para ver si contacta con el player al atacar
    [SerializeField] Transform handAttack;
    [SerializeField] GameObject detectionArea;

    NavMeshAgent agent;

    //RigidBodies del ragdoll
    Rigidbody[] rbs;

    GameObject playerGO;
    Player playerScr;

    Animator anim;
    AnimatorStateInfo animInfo;

    public bool meleeDetected;

    //para el overlap, saber si pilla algo de player
    [SerializeField] LayerMask isAttackable;

    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        playerGO = GameObject.FindGameObjectWithTag("Player");

        anim = GetComponent<Animator>();

        //para pillar todos los rigidbodies de los hijos y meterlos en un array
        rbs = GetComponentsInChildren<Rigidbody>();
        //con un foreach se ponen en cinematico
        foreach (Rigidbody rb in rbs)
        {
            //por cada rb en rbs, ponerlo e cinematico
            rb.isKinematic = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (meleeDetected || GameManager.gM.finalEntered)
        {
            agent.SetDestination(playerGO.transform.position); //esto tampoco va, creo que hay algo raro con el transform
            //transform position del player, ya que el tp tambien la toca y no va
            
            if (agent.remainingDistance <= 2f)
            {
                agent.isStopped = true;
                anim.SetBool("AttackingBool", true);
            }

            else
            {
                if(!animInfo.IsName("Attack"))
                {
                    agent.isStopped = false;
                    anim.SetBool("AttackingBool", false);
                }
            }
        }

        //animator
        
        void Attack()
        {
            Collider[] colls = Physics.OverlapSphere(handAttack.position, 0.3f, isAttackable);
            if (colls.Length > 0)
            {
                for (int i = 0; i < colls.Length; i++)
                {
                    if (colls[i].gameObject.CompareTag("Player"))
                    {
                        playerScr = playerGO.GetComponent<Player>();
                        StartCoroutine(playerScr.TakeDamage(damage));
                    }
                }
            }
        }
    }

    public void TakeDamage(int damageTaken)
    {
        lifes -= damageTaken;
        Debug.Log(lifes);
        if (lifes <= 0)
        {
            Debug.Log("noooo chimuelooooo");
            Death();
        }
    }

    void Death()
    {
        foreach (Rigidbody rb in rbs)
        {
            //por cada rb en rbs, ponerlo e cinematico
            rb.isKinematic = false;
        }
        agent.enabled = false;
        anim.enabled = false;
        this.enabled = false;
    }
}
