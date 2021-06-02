using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class unitaction : MonoBehaviour
{
    //전과 달리 하나의 액션을 담당하는 컴포넌트임
    //완수되면 파괴됨. 관리하는건 다른 컴포넌트에서 함

    public enum typelist : int
    {
         stop = 1, wait, movedirec, movedest, approach, useweapon, buildunit
    }
    public typelist type;
    public int[] i;
    public float[] f;
    public string[] s;


    bool started = false, completed = false;
         

    Unit u;

    private void Start()
    {
        load();

    }

    private void load()
    {
        if (u == null)
        {
            u = gameObject.GetComponent<Unit>();
            if (u == null)
            {
                return;
            }
        }
    }

    public void set(typelist desttype, int[] i, float[] f)
    {
        type = desttype;
        if(i != null)
        {
            this.i = (int[])i.Clone();
        }
        if(f != null)
        {
            this.f = (float[])f.Clone();
        }
    }

    public void set(typelist desttype, int[] i, float[] f, string[] s)
    {
        set(desttype, i, f);
        if(s != null)
        {
            this.s = (string[])s.Clone();
        }
    }

    public bool execute()
    {
        if(u == null)
        {
            load();
            if(u == null)
            {
                return true;
            }
        }

        switch (type)
        {
            case typelist.stop:
                {
                    u.actionlist.Clear();
                    completed = true;
                }
                break;

            case typelist.wait:
                {
                    if(i == null)
                    {
                        completed = true;
                        break;
                    }

                    //i[0]이 남아있는동안 아무것도 안함
                    if(i[0] <= 0)
                    {
                        completed = true;
                    }
                    else
                    {
                        i[0]--;
                    }
                }
                break;

            case typelist.movedirec:
                {
                    if(!started)
                    {
                        if(i == null)
                        {
                            completed = true;
                            break;
                        }
                    }
                    else
                    {
                        if(!u.canmove)
                        {
                            completed = false;
                            break;
                        }

                        int time = 1;
                        if(i.Length >= 2)
                        {
                            time = i[1];
                        }

                        u.startmove((Unit._direction)i[0], time);
                        completed = true;
                        break;
                    }
                }
                break;

            case typelist.movedest:
                {
                    if(!started)
                    {
                        if (i == null)
                        {
                            completed = true;
                            break;
                        }
                        else if (i.Length < 2)
                        {
                            completed = true;
                            break;
                        }

                        system sys = system.findsystem();
                        Unit._direction[] d = sys.findway(u.ix, u.iy, i[0], i[1]);
                        if(d == null)
                        {
                            completed = true;
                            break;
                        }
                        else
                        {
                            List<int> ilist = new List<int>
                            {
                                1 //index
                            };
                            foreach (Unit._direction dn in d)
                            {
                                ilist.Add((int)dn);
                            }
                            i = ilist.ToArray();
                        }
                    }
                    else
                    {
                        if(i[0] >= i.Length) //방향의 리스트만 그대로 수행하기 때문에 위치가 변경되면 더이상 의미가 없게 되므로 멈춰주는것이 필요할듯
                        {
                            completed = true;
                            break;
                        }

                        //pushaction                        
                        u.pushaction(typelist.movedirec, new int[] { i[i[0]], 100 }, null, null);
                        
                        i[0]++;
                    }
                }
                break;

            case typelist.approach: //기본적으로는 movedest와 동일하지만 일정 거리만 간다는게 차이
                {
                    if (!started)
                    {
                        if (i == null)
                        {
                            completed = true;
                            break;
                        }
                        else if (i.Length < 3) 
                        {
                            completed = true;
                            break;
                        }

                        
                        system sys = system.findsystem();
                        Unit._direction[] d = sys.findway(u.ix, u.iy, i[0], i[1]);                        
                        if (d == null)
                        {
                            completed = true;
                            break;
                        }
                        else
                        {
                            List<int> ilist = new List<int>
                            {
                                1 //index
                            };
                            foreach (Unit._direction dn in d)
                            {
                                if(ilist.Count > i[2])
                                {
                                    break;
                                }
                                ilist.Add((int)dn);
                            }
                            i = ilist.ToArray();
                        }
                    }
                    else
                    {
                        if (i[0] >= i.Length) //방향의 리스트만 그대로 수행하기 때문에 위치가 변경되면 더이상 의미가 없게 되므로 멈춰주는것이 필요할듯
                        {
                            completed = true;
                            break;
                        }

                        //pushaction                        
                        u.pushaction(typelist.movedirec, new int[] { i[i[0]++], 100 }, null, null);

                    }
                }
                break;
                


            case typelist.useweapon:
                {
                    if(f == null)
                    {
                        completed = true;
                        break;
                    }
                    else if(f.Length < 2)
                    {
                        completed = true;
                        break;
                    }


                    weapon w;
                    //i(인덱스)를 먼저 체크후 없으면 s(이름)로 찾음
                    if(i == null)
                    {
                        if(s == null)
                        {
                            completed = true;
                            break;
                        }
                        else
                        {
                            w = u.findweapon(s[0]);
                        }
                    }
                    else
                    {
                        w = u.findweapon(i[0]);
                    }

                    if(w == null)
                    {
                        completed = true;
                        break;
                    }

                    if(!w.available()) //굳이 기다릴필요없이 걍 끝내는걸로 해놨음. 기다리는게 낫다 싶으면 complete를 false로
                    {
                        completed = true;
                        break;
                    }

                    w.activate(f[0], f[1]);
                    completed = true;
                }
                break;

            case typelist.buildunit: //생산한 unitbuildinfo의 인덱스(getcomponent로함), 렐리포인트를 변경할 것이라면 i[1],[2]에 정보 있음
                {

                }
                break;

            default:
                completed = true;
                break;
        }

        if(!started)
        {
            started = true;
        }

        return completed;

    }


}
