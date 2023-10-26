using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ejemplo : MonoBehaviour
{
    Vector3 puntoDestino;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(UpdateFinito());
    }

    IEnumerator UpdateFinito()
    {
        while (transform.position != puntoDestino)
        {
            transform.position = Vector3.MoveTowards(transform.position, puntoDestino, 5 * Time.deltaTime);
            yield return null;
        }
        Debug.Log("hE LLEGADO");
    }
}
