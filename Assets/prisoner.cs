using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class prisoner : MonoBehaviour
{
    public int life = 60;
    public bool free = false;


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
