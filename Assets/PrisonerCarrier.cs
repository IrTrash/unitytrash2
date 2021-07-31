using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrisonerCarrier : MonoBehaviour
{
    int team = 0;
    public prisoner currentprisoner;
    public corruptor destcorruptor;
    public float pickrange = 1;

    public float cx, cy, sendrange = 1;


    private void Start()
    {
        Unit u = gameObject.GetComponent<Unit>();
        if(u != null)
        {
            team = u.team;
        }
    }

    private void OnDestroy()
    {
        if(currentprisoner != null)
        {
            currentprisoner.free = true;
        }
    }

    private void FixedUpdate()
    {
        proc();
    }


    void proc()
    {
        if(currentprisoner != null)
        {
            currentprisoner.free = false;
            currentprisoner.gameObject.transform.position = new Vector3(gameObject.transform.position.x + cx, gameObject.transform.position.y + cy);
        }
    }

    public bool pickprionserinrange()
    {
        system sys = system.findsystem();
        if(sys == null)
        {
            return false;
        }

        List<prisoner> candidates = sys.findprisoner(transform.position.x - pickrange, transform.position.y - pickrange, transform.position.x + pickrange, transform.position.y + pickrange, team);
        if(candidates.Count < 1)
        {
            return false;
        }


        currentprisoner = candidates[UnityEngine.Random.Range(0, candidates.Count)];

        return true;
    }




    public bool sendprisoner(corruptor dest)
    {
        if(dest == null || currentprisoner == null)
        {
            return false;
        }

        if(dest.receiveprisoner(currentprisoner))
        {
            currentprisoner = null;
            return true;
        }

        return false;
    }
}
