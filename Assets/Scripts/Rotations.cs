using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotations : MonoBehaviour
{
    Vector3 aimedPosition;
    Vector3 initialPosition;

    bool moving;

    // Start is called before the first frame update
    void Start()
    {
        aimedPosition = transform.position + new Vector3();
        initialPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(new Vector3(0, 1, 0) * 20 * Time.deltaTime);
        
        transform.position = Vector3.MoveTowards(initialPosition, aimedPosition, 2 * Time.deltaTime);

        if (transform.position == aimedPosition && !moving)
        {
            StartCoroutine(WaitUntilMove());
            aimedPosition = -aimedPosition;
        }
    }

    IEnumerator WaitUntilMove()
    {
        moving = true;
        yield return new WaitForSecondsRealtime(0.2f);
        moving = false;
    }
}
