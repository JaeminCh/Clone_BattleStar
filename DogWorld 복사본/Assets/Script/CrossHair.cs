using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrossHair : MonoBehaviour
{
    [SerializeField]
    private Animator animator;

    //????? ??? ?? ?????
    private float gunAccuracy;
    
    //????? ?? ??
    [SerializeField]
    private GameObject go_CrossHairHUD;

    [SerializeField]
    private GunContoroller theGuncontroller;

    public void WalkingAnimation(bool _flag) {
        animator.SetBool("Walking", _flag);
    }
    public void RunningAnimation(bool _flag) {
        animator.SetBool("Running", _flag);
    }
    public void CrounchingAnimation(bool _flag) {
        animator.SetBool("Crounching", _flag);
    }    
    public void FineSightAnimation(bool _flag) {
        animator.SetBool("FineSight", _flag);
    }

    public void Fireanimation() 
    {
        if(animator.GetBool("Walking"))
        animator.SetTrigger("WalkFire");
        else if (animator.GetBool("Crunching"))
        animator.SetTrigger("CrunchFire");
        else
        animator.SetTrigger("IdleFire");
    }

    public float GetAccuracy() 
    {
        if(animator.GetBool("Walking"))
            gunAccuracy = 0.08f;
        else if (animator.GetBool("Crunching"))
            gunAccuracy = 0.02f;
        else if (theGuncontroller.GetFineSightMode())
            gunAccuracy = 0.001f;
        else
            gunAccuracy = 0.04f;

        return gunAccuracy;
    }   
}

