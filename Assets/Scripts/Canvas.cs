using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class Canvas : MonoBehaviour
{
    [SerializeField] GameObject messages;
    [SerializeField] Slider lifeSlider;
    [SerializeField] TextWritter textsWriterScr;
    Animator anim;
    int timer;
    bool activated;

    // Start is called before the first frame update
    void Awake()
    {
        anim = GetComponent<Animator>();
        messages.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.gM.death)
        {
            StartCoroutine(Death());
        }
        if (!activated)
        {
            Messages();

        }
    }

    IEnumerator Death()
    {
        anim.SetTrigger("DeathTrigger");
        yield return new WaitForSeconds(2);
        SceneManager.LoadScene(0);
    }

    void Messages()
    {
        activated = true;
        StartCoroutine(MessagesSpawnDespawn());
        StartCoroutine(textsWriterScr.TextBuilder("E para teleportarse a cornisas"));
        
        StartCoroutine(MessagesSpawnDespawn());
        StartCoroutine(textsWriterScr.TextBuilder("Rueda de raton para liana"));

        StartCoroutine(MessagesSpawnDespawn());
        StartCoroutine(textsWriterScr.TextBuilder("Click izquierdo para golpear"));
       
        StartCoroutine(MessagesSpawnDespawn());
        StartCoroutine(textsWriterScr.TextBuilder("Saltar y E para caminar por muros"));

        StartCoroutine(MessagesSpawnDespawn());
        StartCoroutine(textsWriterScr.TextBuilder("Ctrl (correr) y C (agacharse) para deslizarse"));
    }
    
    IEnumerator MessagesSpawnDespawn()
    {
        messages.SetActive(true);
        yield return new WaitForSeconds(3);
        messages.SetActive(false);
        
    }
    
}
