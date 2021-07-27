using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cocoon : MonoBehaviour
{
    public Unit u;
    public int time = 10;
    public GameObject result;

    public float dtime = 0;

    public Animator anim;

    // Start is called before the first frame update
    void Start()
    {
        u = gameObject.GetComponent<Unit>();
        if(u == null)
        {
            u = gameObject.AddComponent<Unit>();
        }

        anim = GetComponent<Animator>();
        if(anim != null)
        {
            anim.SetInteger("Time", time);
        }
               
    }

    private void FixedUpdate()
    {
        dtime += Time.fixedDeltaTime;
        if(dtime >= 1)
        {
            time -= 1;
        }
        else
        {
            return;
        }

        if (anim != null)
        {
            anim.SetInteger("Time", time);
        }


        if (--time <= 0)
        {
            if(result != null)
            {
                GameObject robj = Instantiate(result, gameObject.transform.position, Quaternion.identity);
                Unit ru = robj.GetComponent<Unit>();
                ru.team = u.team;
            }

            Destroy(this.gameObject);
        }
    }
}
