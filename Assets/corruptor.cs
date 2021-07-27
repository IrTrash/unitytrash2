using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class corruptor : MonoBehaviour
{
    public Unit u;
    public List<prisoner> list = new List<prisoner>();
    public prisoner current;

    public GameObject defaultresult;

    public float dtime = 0;


    private void Start()
    {
        u = gameObject.GetComponent<Unit>();            
    }

    private void FixedUpdate()
    {
        proc();
    }

    void proc()
    {
        dtime += Time.fixedDeltaTime;
        if (dtime >= 1)
        {
            dtime -= 1;
            current.sanity--;
        }


        if (current == null)
        {
            if(list.Count > 0)
            {
                current = list[0];
                if(list.Count > 1)
                {
                    list = new List<prisoner>(list.GetRange(1, list.Count - 1));
                }

            }
        }
        else
        {
            if(current.sanity <= 0)
            {
                if(current.corruptresult != null)
                {
                    float rx, ry;
                    rx = current.gameObject.transform.position.x;
                    ry = current.gameObject.transform.position.y;


                    GameObject result = GameObject.Instantiate(current.corruptresult, new Vector3(rx, ry), Quaternion.identity);
                    //추가 정보 입력
                    Unit ru = result.GetComponent<Unit>();
                    ru.team = u.team;

                    Destroy(current.gameObject);

                }
                else
                {
                    float rx, ry;
                    rx = current.gameObject.transform.position.x;
                    ry = current.gameObject.transform.position.y;

                    GameObject result = GameObject.Instantiate(defaultresult, new Vector3(rx, ry), Quaternion.identity);
                    //추가 정보 입력
                    Unit ru = result.GetComponent<Unit>();
                    ru.team = u.team;

                    Destroy(current.gameObject);

                }

                current = null;
            }
        }
    }
}
