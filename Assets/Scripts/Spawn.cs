using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawn : MonoBehaviour
{
    [SerializeField] GameObject[] enemies;
    [SerializeField] Transform[] spawns;
    [SerializeField] Transform[] spawnsHighGround;
    [SerializeField] ParticleSystem smoke;

    float randomResult;

    bool activated;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.gM.finalEntered && !activated)
        {
            StartCoroutine(Spawning());
        }
    }

    IEnumerator Spawning()
    {
        activated = true;

        for (int i = 0; i < 4; i++)
        {
            yield return new WaitForSeconds(5);
            
            for (int j = 0; j < 10; j++)
            {
                randomResult = Random.Range(0, 101);
                if (randomResult >= 0 && randomResult <= 70)
                {
                    Instantiate(enemies[Random.Range(0, 2)], spawns[Random.Range(0, 4)].position + 
                    new Vector3(Random.Range(-5, 6), 1, Random.Range(-5, 6)), Quaternion.identity);
                }
                else if (randomResult > 70 && randomResult <= 100)
                {
                    Instantiate(enemies[1], spawnsHighGround[Random.Range(0, 2)].position + 
                    new Vector3(Random.Range(-10, 11), 1, 0), Quaternion.identity);
                }
                yield return new WaitForSeconds(4);
            }
        }
    }
}
