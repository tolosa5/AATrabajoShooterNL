using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangedEnemie : MonoBehaviour
{
    int lifes = 100;

    //mano para ver si contacta con el player al atacar
    [SerializeField] Transform handAttack, gunTip;
    [SerializeField] GameObject detectionArea;
    [SerializeField] GameObject bullet;

    GameObject playerGO;

    //RigidBodies del ragdoll
    Rigidbody[] rbs;
    Player playerScr;

    Animator anim;
    float lastShot;
    public bool rangedDetected;

    //para el overlap, saber si pilla algo de player
    [SerializeField] LayerMask isAttackable;

    void Start()
    {
        playerGO = GameObject.FindGameObjectWithTag("Player");

        anim = GetComponent<Animator>();

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
        if (rangedDetected || GameManager.gM.finalEntered)
        {
            Attack();
        }
    }

    void Attack()
    {
        lastShot += Time.deltaTime;
        if (lastShot >= 3)
        {
            Instantiate(bullet, gunTip.position, Quaternion.identity);
            Debug.Log("pium");
            lastShot = 0;
        }
    }

    void LookPlayer()
    {

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
        /*
        foreach (Rigidbody rb in rbs)
        {
            //por cada rb en rbs, ponerlo e cinematico
            rb.isKinematic = false;
        }
        */
        Destroy(gameObject);
        this.enabled = false;
    }
}
