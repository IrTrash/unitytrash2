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


        crtr = gameObject.GetComponent<corruptor>();
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
    public float searchrange = 2; //1은 너무 짧다
    public int wpindex = -1; //이거 너무ㅡ 오래끌면 이 무기만 쓸려하지않을까? 고민
    public corruptor mycorruptor;
    public prisoner findingprisoner;
    void unitproc() //20210510기준 이동 자체가 끝난 시점에서 처리를 하기 때문에 서로 가까워지려고 좌우로 움직이면 이동이 끝난 시점에 좌우가 뒤바뀌어서 다시 반복하게 됨. 수정이 필요할듯 <= 0512 : 대충랜덤
    {
        //어차피 자동적으로 action을 생성 후 추가하기 위한 거니까 행동불가거나 하고있는게 있으면 안 함.
        if(currentpaction != null)
        {
            if(pactionproc(currentpaction))
            {
                currentpaction = null;
                    
            }                        
            else if(!currentpaction.defered)
            {
                return;
            }
        }

        if(!u.actionavailable())
        {
            return;
        }
        
        //일반 유닛
        if (target == null)
        {
            target = findenemy(searchrange);
            if (target == null) //찾아도 없으면
            {
                PrisonerCarrier pcr = gameObject.GetComponent<PrisonerCarrier>();
                //carrior
                if (mycorruptor != null && pcr != null)
                {
                    if (pcr.currentprisoner != null)
                    {
                        if (!pcr.sendprisoner(mycorruptor))
                        {
                            //위치가 안되는것인지 여부는 나중에 따지기로
                            u.addaction(unitaction.typelist.movedest, new int[] { system.tilex(mycorruptor.transform.position.x), system.tiley(mycorruptor.transform.position.y) }, null);
                        }

                    }
                    else
                    {
                        if (pcr.pickprionserinrange())
                        {
                            Debug.Log("3311");
                        }
                        else
                        {
                            if(findingprisoner == null)
                            {
                                //find prisoner
                                system sys = system.findsystem();
                                List<prisoner> candidates = sys.findprisoner(sys.left, sys.bottom, sys.right, sys.top, u.team);
                                if(candidates.Count > 0)
                                {
                                    findingprisoner = candidates[UnityEngine.Random.Range(0, candidates.Count)];
                                }
                            }                            
                            else
                            {                                
                                u.addaction(unitaction.typelist.approach, new int[] {system.tilex(findingprisoner.transform.position.x),system.tiley(findingprisoner.transform.position.y), 1 }, null);
                            }
                        }
                    }                       
                }
                else //그냥 유닛
                {
                    //가만히 있게 아무것도 안썼는데 배회하게 하는게 나을수도?
                }                
            }
            else //찾았는데 있으면
            {
                
            }
        }
        else  //타겟이 있을때
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
                        u.addaction(unitaction.typelist.useweapon, new int[] { wpindex }, new float[] { target.x, target.y }, null);
                        wpindex = -1;
                    }
                    else //접근 
                    {
                        u.addaction(unitaction.typelist.approach, new int[] { target.tx, target.ty ,1 }, null );
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

    public corruptor crtr;
    public List<PrisonerCarrier> carrierlist = new List<PrisonerCarrier>();
    public int carriercount = 2;

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

                if(!myunits.Contains(bunit))
                {
                    myunits.Add(bunit);
                }
            }

           
        }

        corruptor crt = gameObject.GetComponent<corruptor>();

        foreach (Unit myunit in myunits) //이런거 너무 불필요한 체크 많이 할거같은데 괜찮을까?
        {
            if (myunit == null)
            {
                continue;
            }

            


            if (defenceunitlist.Count < defenceunitcount)
            {
                if (!defenceunitlist.Contains(myunit))
                {
                    defenceunitlist.Add(myunit);
                }
            }            
            else // prisonercarrier -> attackunitlist
            {
                if (carrierlist.Count < carriercount)
                {
                    PrisonerCarrier pscr = myunit.GetComponent<PrisonerCarrier>();
                    if (pscr != null)
                    {
                        if (!carrierlist.Contains(pscr))
                        {
                            carrierlist.Add(pscr);
                            unitpattern myunitp = myunit.GetComponent<unitpattern>();
                            if(myunitp != null)
                            {
                                myunitp.mycorruptor = crt;
                            }
                        }
                    }
                }
                else if (!attackunitlist.Contains(myunit))
                {
                    attackunitlist.Add(myunit);
                }

            }
        }

        //타겟 검출
        targetlist = findenemies(searchrange);
        deftargetlist = findenemies(defendrange);

        foreach (Unit tu in deftargetlist)
        {
            targetlist.Remove(tu);
        }

        //방어,공격 유닛 운용            
        List<Unit> removelist = new List<Unit>();
        int tindex = 0;

        removelist.Clear();
        //defunits
        foreach (Unit dunit in defenceunitlist)
        {
            if (dunit == null) //유효성
            {
                removelist.Add(dunit);
            }
        }
        foreach (Unit runit in removelist)
        {
            defenceunitlist.Remove(runit);
        }

        removelist.Clear();


        //atkunits
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

        //타겟 운용 방식 잘못됨(0804)

        foreach (Unit dunit in defenceunitlist)
        {
            if (deftargetlist.Count <= 0)
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
            if (targetlist.Count < 1)
            {
                break;
            }

            unitpattern aunitptrn = aunit.GetComponent<unitpattern>();
            if (aunitptrn != null)
            {
                if (aunitptrn.cancommand())
                {
                    Debug.Log("attackunit command");
                    Unit t;
                    if (tindex >= targetlist.Count)
                    {
                        t = targetlist[UnityEngine.Random.Range(0, targetlist.Count)];
                    }
                    else
                    {
                        t = targetlist[tindex++];
                    }

                    aunitptrn.pactionrequest(new paction(paction.typelist.attackdown, new int[] { system.tilex(t.x), system.tiley(t.y) }, null, null));
                }

            }
        }

        List<PrisonerCarrier> pcremove = new List<PrisonerCarrier>();
        foreach(PrisonerCarrier pc in carrierlist)
        {
            if(pc == null)
            {
                pcremove.Add(pc);
            }
        }
        foreach(PrisonerCarrier pc in pcremove)
        {
            carrierlist.Remove(pc);
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
        dest.defered = false;

        switch (dest.type)
        {
            case paction.typelist.attackdown:
                {
                    if (dest.started)
                    {
                        if(dest.i == null)
                        {
                            complete = true;
                            break;
                        }
                        else if(dest.i.Length < 2)
                        {
                            complete = true;
                            break;
                        }
                    }

                    if(target == null)
                    {
                        if (system.tilex(u.x) == dest.i[0] && system.tiley(u.y) == dest.i[1])
                        {
                            complete = true;
                            break;
                        }
                        else
                        {
                            target = findenemy(searchrange);
                            if (target != null)
                            {
                                u.clearaction();
                                dest.defered = true;
                                break;
                            }
                            else
                            {
                                u.addaction(unitaction.typelist.movedest, new int[] {dest.i[0],dest.i[1] }, null);
                                break;
                            }
                        }
                    }
                    else
                    {
                        dest.defered = true;
                    }
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
