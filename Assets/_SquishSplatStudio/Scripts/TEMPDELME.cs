using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TEMPDELME : MonoBehaviour
{
    public bool triggerAccelerator;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (triggerAccelerator)
        {
            triggerAccelerator = false;

            GetComponent<Animator>().SetTrigger("acceleratorStart");
        }
    }
}
