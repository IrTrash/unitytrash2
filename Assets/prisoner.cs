using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class prisoner : MonoBehaviour
{
    public int life = 60, sanity = 10 , team = 0;
    public bool free = false;


    public GameObject corruptresult;

    system sys;    

    private void Start()
    {
        sys = system.findsystem();
        if(sys != null)
        {
            sys.prisonerlist.Add(this);
        }
    }

    private void OnDestroy()
    {
        if (sys != null)
        {
            sys.prisonerlist.Remove(this);
        }
    }

}
