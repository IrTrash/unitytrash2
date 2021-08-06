using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class patternaction : MonoBehaviour
{
    public unitpattern dest;
    unitpattern.paction body;

    public unitpattern.paction.typelist type;
    public int[] i;
    public float[] f;
    public string[] str;


    private void Start()
    {        
        body = new unitpattern.paction(type, i, f, str);        
    }


    private void FixedUpdate()
    {
        if (dest == null)
        {
            dest = gameObject.GetComponent<unitpattern>();
        }

        if (dest != null)
        {
            if(dest.pactionrequest(body))
            {
                Destroy(this);
            }
        }
    }
}
