using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hand_Controller : MonoBehaviour
{
    // ?? ??? hand Type ? ??
    [SerializeField]
    private Hand currentHand; //Hand.c??? ???? ? hand? ???? public?? ?????? ??? ????

    // ???
    private bool isAttack = false;
    private bool isSwing = false;

    // RayCastHit:Laser? ?? ??? ?? ?? ?? ??? ??
    private RaycastHit hitInfo;

    void Update()
    {
        TryAttack();
    }

    private void TryAttack()
    {
        // Fire1? ????? ?? ????? ????? ?? ???? ??? ?? ?????? ??.(?? ??? ????? ?? ???)
        if (Input.GetButton("Fire1"))
        {
            if (!isAttack)
            {
                //???? ???? ?? ??? ???? ????.
                StartCoroutine(AttackCoroutine());
            }

        }
    }

    IEnumerator AttackCoroutine()
    {
        isAttack = true;

        // hand? ???? ???? Attack?????? ???? ????.
        currentHand.animator.SetTrigger("Attack");

        yield return new WaitForSeconds(currentHand.attackAbleDelay);
        isSwing = true;

        //??? ????? ???? ???? ???? ?? (isSwing? True?? ??? ??? ??? ??? ?? ??)
        StartCoroutine(HitCoroutine());

        yield return new WaitForSeconds(currentHand.attackDisAbleDelay);
        isSwing = false;

        //  ??? ???? ?? ??
        yield return new WaitForSeconds(currentHand.attackRetryDelay - currentHand.attackAbleDelay - currentHand.attackDisAbleDelay);
        isAttack = false;
    }

    IEnumerator HitCoroutine()
    {
        while (isSwing)
        {
            if (CheckObject())
            {
                //??? ??? ??? isSwing? ??? ???? ? ?? ????? ??
                isSwing = false;
                //CheckObject? True? ??? ??? ?? ??? ??
                Debug.Log(hitInfo.transform.name);
            }
            //???? ????? ??? ??? ???? while? ?? ??? 1??? ??
            yield return null;
        }
    }

    private bool CheckObject()
    {
        //?? ?? 1:??? ????, 2:???? ??, 3:???? ?? ??? ?? ??, 4:??(??? ????)
        if (Physics.Raycast(transform.position, transform.forward, out hitInfo, currentHand.range))
        {
            return true;
        }
        return false;
    }
}
