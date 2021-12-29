/****************************************************
    文件：Ctrl.cs
	作者：Plane
    邮箱: 1785275942@qq.com
    日期：#CreateTime#
	功能：Nothing
*****************************************************/

using System.Collections;
using UnityEngine;
using UnityEngine.UI;


public class Ctrl : MonoBehaviour 
{



    //头顶血条
    public Image hpPrg;
    public RectTransform hpTrans;

    
    public bool roundEnd = false;
   
    public int rest;
   
    public int attackRange;
    private Animator animator;
    public Vector3 startpoint;
    public Vector3 goMapPos;
    
   



    protected virtual  void  Init()
    {
        animator = this.GetComponent<Animator>();
        startpoint = this.transform.position;

    }
    public void ChangeAnimation(string state)
    {
        animator.CrossFade(state, 0.2f);
    }



    public   void Move(Cell start,Cell end)
    {
        
        Astar.Instance.FindPath(this, start, end);
        ChangeAnimation("Move");
    }
  


}