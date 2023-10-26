using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TextWritter : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI text;
    float charsPerSecond = 60f;

    public IEnumerator TextBuilder(string message)
    {
        text.text = "";
        foreach (char character in message)
        {
            text.text += character;
            yield return new WaitForSeconds(1/charsPerSecond);
        }
    }
}
