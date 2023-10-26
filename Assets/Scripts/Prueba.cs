using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Prueba : MonoBehaviour
{
    GameObject cam;
    [SerializeField] LayerMask esCornisa;
    Rigidbody rb;
    bool cornisa;
    GameObject ultimaCornisa;
    //CharacterController controller;

    Vector3 movementY;
    float factorG = -9.81f;
    float jumpHeight = 0.8f;

    float currentSpeed;

    float h;
    float v;
    Vector3 direccion;
    // Start is called before the first frame update
    void Start()
    {
        cam = Camera.main.gameObject;
        rb = GetComponent<Rigidbody>();
        //controller = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        h = Input.GetAxisRaw("Horizontal");
        v = Input.GetAxisRaw("Vertical");
    
        direccion = transform.forward * v + transform.right * h;
        //controller.Move(direccion.normalized * currentSpeed * Time.deltaTime);

        //Translate para la gravedad. Dos deltas porque m/s^2
        movementY.y += factorG * Time.deltaTime;

        //controller.Move(movementY * Time.deltaTime);
        
        RaycastHit hit;
        if(Input.GetMouseButtonDown(2))
        {
            if(Physics.Raycast(cam.transform.position, cam.transform.forward, out hit, 200, esCornisa))
            {
//controller.enabled = false;
                Debug.Log("Rayo");

                if(hit.collider.CompareTag("Cornisa"))
                {
                    ultimaCornisa = hit.collider.gameObject;
                    rb.isKinematic = true;
                    cornisa = true;
                }
                //else if()
                //{

                //}
            }

        }
        if(cornisa)
        {
            transform.position = Vector3.MoveTowards(transform.position, ultimaCornisa.transform.position 
                + new Vector3(0, -1f, -1.5f), 100 * Time.deltaTime);
            
            if(transform.position == ultimaCornisa.transform.position + new Vector3(0, -1f, -1.5f))
            {
                //controller.enabled = true;
                Debug.Log("afohqafo");
                cornisa = false;
                ultimaCornisa = null;
            }
        }
    }
    void FixedUpdate()
    {
        rb.AddForce(direccion * 20, ForceMode.Acceleration);
        //rb.velocity.y -= 

    }
}
