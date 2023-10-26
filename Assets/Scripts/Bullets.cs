using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullets : MonoBehaviour
{
    GameObject playerGO;
    Vector3 aimedPosition;

    float speed = 5;
    float timer;
    int damage = 20;

    TrailRenderer trailRenderer;

    // Start is called before the first frame update
    void Awake()
    {
        playerGO = GameObject.FindGameObjectWithTag("Player");
        aimedPosition = playerGO.transform.position - transform.position;
        trailRenderer = GetComponent<TrailRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        transform.Translate(aimedPosition * speed * Time.deltaTime);
        if (timer >= 4)
        {
            Destroy(gameObject);
            timer = 0;
        }
    }

    private void OnTriggerEnter(Collider other) 
    {
        if (other.gameObject.CompareTag("Player"))
        {
            Player playerScr = playerGO.GetComponent<Player>();
            StartCoroutine(playerScr.TakeDamage(damage));
            Debug.Log(playerScr.lifes);
        }
    }
}
