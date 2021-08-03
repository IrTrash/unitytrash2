using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class prisoner : MonoBehaviour
{
    public int life = 60, maxsanity = 10, sanity = 10 , team = 0;
    public bool free = true, corrupting = false;


    public GameObject corruptresult;

    system sys;

    public Animator anim;

    private void Start()
    {
        sys = system.findsystem();
        if(sys != null)
        {
            sys.prisonerlist.Add(this);
        }

        anim = gameObject.GetComponent<Animator>();

        sanity = maxsanity;
    }

    private void OnDestroy()
    {
        if (sys != null)
        {
            sys.prisonerlist.Remove(this);
        }
    }

    float dtime = 0;
    private void FixedUpdate()
    {
        dtime += Time.fixedDeltaTime;
        if(dtime >= 1)
        {
            dtime -= 1;
            if(free && sanity < maxsanity)
            {
                sanity++;
            }
        }

        if(anim != null)
        {
            anim.SetBool("corrupting", corrupting);
            anim.SetInteger("sanity", sanity);
        }

    }

}
