using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unitbuildinfo : MonoBehaviour
{
    public string buildname = "noname";
    //자원 등은 나중에 구현
    public int reqtime;
    public GameObject result;


    public GameObject create(float dx, float dy, int team)
    {
        if(result == null)
        {
            return null;
        }


        GameObject r = Instantiate(result, new Vector3(dx, dy, 0), Quaternion.identity);
        Unit u = r.GetComponent<Unit>();
        if(u != null)
        {
            u.team = team;
            u.x = dx;
            u.y = dy;
        }

        return r;
    }
}
