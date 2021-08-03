using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class dropobject : MonoBehaviour
{
    public GameObject result;
    public float xplus = 0, yplus = 0;
    public float rate;

    public float r = 0;

    private void Start()
    {
        r = UnityEngine.Random.Range(0, 1f);
    }


    private void OnDestroy()
    {
        if (r <= rate && result != null)
        {
            GameObject robj = Instantiate(result, new Vector3(gameObject.transform.position.x + xplus, gameObject.transform.position.y + yplus), Quaternion.identity);

            //추가 처리

            prisoner p = robj.GetComponent<prisoner>();
            Unit u = gameObject.GetComponent<Unit>();
            if (p != null && u != null)
            {
                p.team = u.team;
            }
        }
        else 
        {

            Debug.Log("dd");
        }
    }
}
