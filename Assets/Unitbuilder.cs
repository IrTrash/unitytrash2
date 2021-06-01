using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unitbuilder : MonoBehaviour
{
    public const int buildlistmax = 5;

    //unit을 생산하는 컴포넌트
    public List<Unitbuildinfo> list = new List<Unitbuildinfo>(); //fifo
    public Unitbuildinfo current;

    public int currenttime = 0;

    public bool rellypoint;
    public int rellypointx, rellypointy;

    public unitpattern up;

    public List<Unit> buildedunits = new List<Unit>();

    public int unitcountmax = 10;


    private void Start()
    {
        if(up == null)
        {
            up = GetComponent<unitpattern>();
        }
    }

    private void FixedUpdate()
    {
        proc();
    }


    void proc()
    {
        if(current == null)
        {
            if(list.Count > 0)
            {
                current = list[0];
                currenttime = list[0].reqtime;
                if (list.Count > 1)
                {
                    list = new List<Unitbuildinfo>(list.GetRange(1, list.Count - 1));
                }
                else
                {
                    list.Clear();
                }
            }
        }
        else
        {
            //처리
            if(currenttime <= 0)
            {
                Unit u = gameObject.GetComponent<Unit>();
                if(u != null)
                {
                    GameObject buf = current.create(u.x, u.y, u.team);
                    //추가 처리(렐리 포인트 등)

                    if(rellypoint)
                    {
                        //rellyx,rellyy로 buf에 이동 명령 추가
                        Unit bufunit = buf.GetComponent<Unit>();
                        if(bufunit != null)
                        {
                            buildedunits.Add(bufunit);

                            bufunit.addaction(unitaction.typelist.movedest, new int[] { rellypointx, rellypointy }, null) ; //이시점에서 제대로 수행될 수 있나?
                        }
                    }


                    //up가 존재하면 myunit에 추가
                    if(up != null)
                    {
                        up.myunits.Add(u);
                    }
                }
                

                current = null;
            }
            else
            {
                currenttime--;
            }
        }
    }


    public bool request(int index)
    {
        if(index < 0 || !able())
        {
            return false;
        }

        List<Unitbuildinfo> bilist = new List<Unitbuildinfo>(gameObject.GetComponents<Unitbuildinfo>());
        if(bilist.Count <= index) //인덱스가 리스트 범위 밖이거나 큐가 꽉 찼을 경우(그냥 제한해둔거 )
        {
            return false;
        }

        if(list.Count == 0 && current == null)
        {
            current = bilist[index];
        }
        else
        {
            list.Add(bilist[index]);
        }

        return true;
    }


    public bool able()
    {
        return list.Count < buildlistmax && ((current != null && unitcountmax > buildedunits.Count + 1) || (current == null && unitcountmax > buildedunits.Count));  //
    }
}
