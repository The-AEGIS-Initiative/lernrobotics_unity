using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloorTrigger : MonoBehaviour
{
    private SpriteRenderer sr;
    private bool isTriggered;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
    }
    void OnTriggerStay2D(Collider2D collider)
    {
        if(collider.gameObject.name == "Robot(Clone)")
        {
            if(Vector2.Distance((Vector2) transform.position, (Vector2) collider.transform.position) < 0.5)
            {
                if (!isTriggered)
                {
                    StartCoroutine(Activate());
                    isTriggered = true;
                }              
            }
        }
        
    }

    private IEnumerator Activate()
    {
        // Ripple outwards activate glow
        sr.material.SetFloat("_Deactivation", 2);
        for (float i = 0; i < 2; i += 0.2f)
        {
            sr.material.SetFloat("_Activation", i);
            yield return new WaitForSeconds(0.05f);
        }

        yield return new WaitForSeconds(0.2f);

        // Ripple outwards deactivate glow
        sr.material.SetFloat("_Activation", 0);
        sr.material.SetFloat("_Deactivation", 0);
        for (float i = 0; i < 2; i += 0.2f)
        {
            sr.material.SetFloat("_Deactivation", i);
            yield return new WaitForSeconds(0.05f);
        }

        // Reset
        sr.material.SetFloat("_Activation", 0);
        sr.material.SetFloat("_Deactivation", 2);
        isTriggered = false;
    }
}
