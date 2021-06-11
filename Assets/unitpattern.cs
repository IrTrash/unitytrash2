﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class unitpattern : MonoBehaviour
{
    //좆구린 유닛 자동 행동 체계
    public Unit u;

    private void Start()
    {
        if(u == null)
        {
            u = gameObject.GetComponent<Unit>();          
        }

    }

    private void FixedUpdate()
    {
        proc();
    }

    public bool pactionexist = false;

    void proc()
    {
        pactionexist = currentpaction != null;

        if(u == null)
        {
            return;
        }

        switch (u.type)
        {
            case Unit._type.unit:
                {
                    unitproc();
                }
                break;

            case Unit._type.building:
                {
                    buildingproc();
                }
                break;
        }
    }

    //attackdown 등의 주변 정보가 필요한 action
    public class paction
    {
        public enum typelist : int
        {
            attackdown = 1
        }

        public bool started = false, defered = false;

        public typelist type;
        public int[] i;
        public float[] f;
        public string[] s;        


        public paction(typelist t, int[] idata,float[] fdata, string[] strdata)
        {
            type = t;
            if(idata != null)
            {
                i = (int[])idata.Clone();
            }

            if (fdata != null)
            {
                f = (float[])fdata.Clone();
            }

            if (strdata != null)
            {
                s = (string[])strdata.Clone();
            }
        }
    }
    public paction currentpaction;


    public Unit target;
    public float searchrange = 1;
    public int wpindex = -1; //이거 너무ㅡ 오래끌면 이 무기만 쓸려하지않을까? 고민
    void unitproc() //20210510기준 이동 자체가 끝난 시점에서 처리를 하기 때문에 서로 가까워지려고 좌우로 움직이면 이동이 끝난 시점에 좌우가 뒤바뀌어서 다시 반복하게 됨. 수정이 필요할듯 <= 0512 : 대충랜덤
    {
        //어차피 자동적으로 action을 생성 후 추가하기 위한 거니까 행동불가거나 하고있는게 있으면 안 함.
        if(currentpaction != null)
        {
            if(pactionproc(currentpaction))
            {
                if(currentpaction.defered)
                {
                    
                }
                else
                {
                    currentpaction = null;
                }
                    
            }                        
            else
            {
                return;
            }
        }

        if (target == null)
        {
            target = findenemy(searchrange);
            if (target == null) //찾아도 없으면
            {

            }
        }

        if (!u.canaction || u.actionlist.Count > 0 || u.currentaction != null || u.pushedaction != null)
        {            
            return;
        }

        
        
        if(target != null)
        {
            //타겟 유효성
            if (!system.isin(target.x,target.y,u.x - searchrange, u.y - searchrange, u.x + searchrange, u.y + searchrange) || u.team == target.team)
            {
                target = null;                
            }   
            else
            {
                if(wpindex >= 0) //이미 선택한 무기가 있을 경우 
                {
                    weapon wp = u.findweapon(wpindex);
                    if(Mathf.Abs(u.x - target.x) + Mathf.Abs(u.y - target.y)  <= wp.range + system.unitplacepushdistance)
                    {
                        Debug.Log("( " + target.x + " , " + target.y + " )");
                        u.addaction(unitaction.typelist.useweapon, new int[] { wpindex }, new float[] { target.x, target.y }, null);
                        wpindex = -1;
                    }
                    else //접근 
                    {
                        Debug.Log("asdf");
                        u.addaction(unitaction.typelist.approach, new int[] { target.ix, target.iy ,1 }, null );
                    }                                        
                }
                else
                {
                    bool cantattack;

                    List<weapon> wplist = new List<weapon>(u.GetComponents<weapon>()); //unit에서 index로 weapon을 찾는 건 getcomponents로 하는걸로 동일하므로 여기서도 동일하게 하면 될듯

                    if (wplist.Count <= 0)
                    {
                        cantattack = true;
                    }
                    else
                    {
                        Dictionary<int, int> prdic = new Dictionary<int, int>();
                        //wp 선택
                        int psum = 0;
                        int i = 0;
                        foreach (weapon wp in wplist) 
                        {
                            if (wp.available() && wp.priority > 0)
                            {
                                psum += wp.priority;
                                prdic.Add(i, wp.priority);
                            }
                            i++;
                        }

                        cantattack = psum <= 0;

                        psum = UnityEngine.Random.Range(1, psum + 1);
                        foreach(int prkey in prdic.Keys) //psum 이 0 이하면 실행안됨
                        {
                            psum -= prdic[prkey];
                            if(psum <= 0)
                            {
                                wpindex = prkey;
                                break;
                            }
                        }

                    }

                    if(cantattack)
                    {

                    }
                }

                
                                                
            }
            
        }
    }



    public List<Unit> myunits = new List<Unit>(), defenceunitlist = new List<Unit>(), attackunitlist = new List<Unit>();
    public int defenceunitcount = 4; //방어에 쓸 유닛 수
    public int defendrange = 2;//적이 이 범위안에오면 방어 유닛을 그 적으로 보냄
    public List<Unit> targetlist = new List<Unit>(), deftargetlist = new List<Unit>(); //여러 타겟 
    public Unitbuilder ub;
    void buildingproc() //생산건물 전용 패턴.
    {
        //이 함수가 돌아가야할 상황인지
        


        if (ub == null)
        {
            ub = GetComponent<Unitbuilder>();
        }
        else
        {
            if(ub.able())
            {
                //아무거나 리퀘스트
                Unitbuildinfo[] options = ub.GetComponents<Unitbuildinfo>();
                if(options != null)
                {
                    ub.request(UnityEngine.Random.Range(0, options.Length));
                }
            }


            foreach(Unit bunit in ub.buildedunits)
            {
                if(bunit == null)
                {
                    continue;
                }

                if(defenceunitlist.Count < defenceunitcount)
                {
                    if(!defenceunitlist.Contains(bunit))
                    {
                        defenceunitlist.Add(bunit);
                    }
                }
                else // attackunitlist
                {
                    if(!attackunitlist.Contains(bunit))
                    {
                        attackunitlist.Add(bunit);
                    }

                }
            }

            //타겟 검출
            targetlist = findenemies(searchrange);
            deftargetlist = findenemies(defendrange);

            foreach(Unit tu in deftargetlist)
            {
                targetlist.Remove(tu);
            }


            //내 유닛 처리
            List<Unit> removelist = new List<Unit>();

            int tindex = 0;
            //defunits
            foreach(Unit dunit in defenceunitlist)
            {
                if (dunit == null) //유효성
                {
                    removelist.Add(dunit);
                }
            }
            foreach(Unit runit in removelist)
            {
                defenceunitlist.Remove(runit);
            }

            removelist.Clear();

            
            foreach (Unit aunit in attackunitlist)
            {
                if (aunit == null) //유효성
                {
                    removelist.Add(aunit);
                }                
            }
            foreach (Unit runit in removelist)
            {
                attackunitlist.Remove(runit);
            }


            foreach(Unit dunit in defenceunitlist)
            {
                if(deftargetlist.Count <= 0 )
                {
                    break;
                }

                unitpattern dunitptrn = dunit.GetComponent<unitpattern>();
                if (dunitptrn != null)
                {
                    if (dunitptrn.cancommand())
                    {
                        Unit t;
                        if (tindex >= deftargetlist.Count)
                        {
                            t = deftargetlist[UnityEngine.Random.Range(0, deftargetlist.Count)];
                        }
                        else
                        {
                            t = deftargetlist[tindex++];
                        }

                        dunitptrn.pactionrequest(new paction(paction.typelist.attackdown, new int[] { system.tilex(t.x), system.tiley(t.y) }, null, null));
                    }
                }
            }
            tindex = 0;
            foreach (Unit aunit in attackunitlist)
            {
                if(targetlist.Count < 1)
                {
                    break;
                }

                unitpattern aunitptrn = aunit.GetComponent<unitpattern>();
                if (aunitptrn != null)
                {
                    if (aunitptrn.cancommand())
                    {
                        Unit t;
                        if (tindex >= targetlist.Count)
                        {
                            t = targetlist[UnityEngine.Random.Range(0, targetlist.Count)];
                        }
                        else
                        {
                            t = targetlist[tindex++];
                        }

                        Debug.Log("asdfeqwrqwe");
                        aunitptrn.pactionrequest(new paction(paction.typelist.attackdown, new int[] { system.tilex(t.x), system.tiley(t.y) }, null, null));
                    }

                }
            }
        }


    }

    public bool pactionrequest(paction dest) //이미있으면안함
    {
        if(dest == null || currentpaction != null)
        {
            return false;
        }

        currentpaction = dest;
        return true;
    }

    public void pactionset(paction dest) //강제
    {
        currentpaction = dest;
    }


    private bool pactionproc(paction dest)
    {
        if(dest == null)
        {
            return true;
        }
        //cancommand 쓰면안됨


        bool complete = false;

        switch (dest.type)
        {
            case paction.typelist.attackdown:
                {
                    if(!dest.started)
                    {
                        if(dest.i.Length < 2)
                        {
                            complete = true;
                            break;
                        }
                    }

                    if(system.tilex(u.x) == dest.i[0] && system.tiley(u.y) == dest.i[1])
                    {
                        
                        complete = true;
                        break;
                    }


                    if(target != null)
                    {
                        if (system.isin(target.x, target.y, u.x - searchrange, u.y - searchrange, u.x + searchrange, u.y + searchrange))
                        {
                            target = null;
                        }
                        
                    }

                    if(target == null)
                    {
                        target = findenemy(searchrange);
                        if(target == null)
                        {
                            Debug.Log("attackdown move");

                            //암것도 안하고있으면 목적지 이동
                            if(u.canaction && u.currentaction == null && u.pushedaction == null && u.actionlist.Count < 1)
                            {
                                u.addaction(unitaction.typelist.movedest, new int[] { dest.i[0], dest.i[1] }, null, null);
                                Debug.Log("attackdown move - 1165687489");
                            }

                            dest.defered = false;
                            complete = false;
                            break;
                        }
                    }

                    dest.defered = true;
                    complete = true;
                    //target이 있는 상태니까 그냥 unitproc으로넘겨줌
                }
                break;
        }

        if(!dest.started)
        {
            dest.started = true;
        }
        return complete;
    }


    public bool cancommand() //unitpattern 컴포넌트 기준에서 행동 가능한 상태인지 판별
    {
        if(u == null)
        {
            return false;
        }

        return currentpaction == null && u.canaction && u.actionlist.Count <= 0 && u.currentaction == null && u.pushedaction == null;
    }


    private Unit findenemy(float range)
    {
        system sys = system.findsystem();
        if (sys == null)
        {
            return null;
        }

        List<Unit> candidates = new List<Unit>();
        foreach(Unit c in sys.findunit(u.x, u.y, range, new Unit[] { u }))
        {
            if(c.team != u.team)
            {
                candidates.Add(c);
            }
        }

        if(candidates.Count < 1)
        {
            return null;
        }

        //일단은 랜덤 리턴으로
        return candidates[UnityEngine.Random.Range(0, candidates.Count)];
    }

    private List<Unit> findenemies(float range)
    {
        system sys = system.findsystem();
        if (sys == null)
        {
            return null;
        }

        List<Unit> r = new List<Unit>();
        foreach (Unit c in sys.findunit(u.x, u.y, range, new Unit[] { u }))
        {
            if (c.team != u.team)
            {
                r.Add(c);
            }
        }

        return r;
    }
}
