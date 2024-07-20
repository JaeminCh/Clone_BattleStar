using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.ExceptionServices;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEditor.Search;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

public class GunContoroller : MonoBehaviour
{
    //?????? ??¥ì°©??? ì´? 
    [SerializeField]
    private Gun currentGun;
    private CrossHair theCrossHair;

    //??°ì?? ?????? ê°????
    private float currentFireRate;
    // ?????? ë³????
    private bool isReload = false;
    private bool isFineSightMode = false;

    // ë³¸ë?? ???ì§???? ê°?
    private Vector3 originPos;
    // ???ê³¼ì??
    private AudioSource audioSource;
    private RaycastHit hitInfo; // hit??? ê°?ì²´ì?? ???ë³´ë¥¼ ë°???????
    // ??¼ê²© ??´í????? ë³´ê?????
    [SerializeField]
    private GameObject hit_effect_prefab;


    [SerializeField]
    private Camera theCam;

    void Start()
    {
        originPos = Vector3.zero;
        audioSource = GetComponent<AudioSource>();
        theCrossHair = FindObjectOfType<CrossHair>();
        // originPos = transform.localPosition;
    }
    void Update()
    {
        GunFireRateCalc();
        TryFire();
        TryReload();
        TryFineSight();
    }
    // ??°ì???????? ???ê³????
    private void GunFireRateCalc()
    {
        if (currentFireRate > 0)
            currentFireRate -= Time.deltaTime; // 1?? ??? ?? ?? 1/60    
    }
    // ë°??????????
    private void TryFire()
    {
        if (Input.GetButton("Fire1") && currentFireRate <= 0 && isReload == false)
        {
            Fire();
        }
    }
    // ë°???? ??? ê³????
    public void Fire()
    {
        if (!isReload)
        {
            if (currentGun.currentBulletCount > 0)
                Shoot();
            else
            {
                CancleFineSight();
                StartCoroutine(ReloadCoroutine());
            }

        }

    }
    // ë°???? ??? ê³????
    private void Shoot()
    {//???
        theCrossHair.Fireanimation();
        currentGun.currentBulletCount--;
        currentFireRate = currentGun.fireRate; // ???? ???x
        PlaySE(currentGun.fire_Sound);
        currentGun.muzzleFlash.Play();
        Hit();

        StopAllCoroutines(); // ??¤í?????ê³? ?????? ì½?ë£???? ???ì§? whileë¬? ??´ì?? ??¤í?? ë°?ì§?
        StartCoroutine(RetroActionCoroutine());
        // Debug.Log("Fire");
    }

    private void Hit()
    {
        
        // Debug.Log("Hit");
        if (Physics.Raycast(theCam.transform.position, theCam.transform.forward +
            new Vector3(Random.Range(-theCrossHair.GetAccuracy() - currentGun.accuracy, theCrossHair.GetAccuracy() + currentGun.accuracy),
                        Random.Range(-theCrossHair.GetAccuracy() - currentGun.accuracy, theCrossHair.GetAccuracy() + currentGun.accuracy),
                        0)
            , out hitInfo, currentGun.range))// 1ë²?ì¨? ??¸ì???????? ë¡?ì»????ì§??????? ??????ê³? ê·¸ë?? ???ì§??????? ??? ??´ì????? ????????? ì¢????ê°? ??????ê³? ?????? ê¸°ì??????????? ì¢????ë¡? ë´???¼í??ê¸? ???ë¬¸ì??
        {
            var clone = Instantiate(hit_effect_prefab, hitInfo.point, UnityEngine.Quaternion.LookRotation(hitInfo.normal));
            Destroy(clone, 2.0f);
        }
    }

    // ?????¥ì?? ??????
    private void TryReload()
    {

        if (Input.GetKeyDown(KeyCode.R) && !isReload && currentGun.currentBulletCount < currentGun.reloadBulletCount)
        {
            CancleFineSight();
            StartCoroutine(ReloadCoroutine());
        }


    }
    // ???ì¡°ì?? ??????
    private void TryFineSight()
    {
        //??? ????
        if (Input.GetButtonDown("Fire2") && !isReload)
        {
            Finesight();
        }
    }
    // ???ì¡°ì?? ì·????
    public void CancleFineSight()
    {
        if (isFineSightMode)
        {
            Finesight();
        }
    }
    // ???ì¡°ì?? ë¡?ì§? ê°????
    private void Finesight()
    {
        isFineSightMode = !isFineSightMode;
        currentGun.animator.SetBool("FineSight", isFineSightMode);
        theCrossHair.FineSightAnimation(isFineSightMode);
        if (isFineSightMode == true)
        {
            StopAllCoroutines(); // ??´ì?? ë£???? ë°?ì§?
            StartCoroutine(FineSightActivateCoroutine());
        }
        else
        {
            StopAllCoroutines();
            StartCoroutine(FineSightDeActivateCoroutine());
        }

    }
    // ???ì¡°ì?? ?????±í??
    IEnumerator FineSightActivateCoroutine()
    {
        while (currentGun.transform.localPosition != currentGun.fineSightOriginPos)// ??? ???? ???? ??? ?? ??
        {
            currentGun.transform.localPosition = Vector3.Lerp(currentGun.transform.localPosition, currentGun.fineSightOriginPos, 0.2f);
            yield return null;
        }
    }
    // ???ì¡°ì?? ë¹??????±í??
    IEnumerator FineSightDeActivateCoroutine()
    {
        while (currentGun.transform.localPosition != originPos)// origin ???? ???? ??? ?? ??
        {
            currentGun.transform.localPosition = Vector3.Lerp(currentGun.transform.localPosition, originPos, 0.2f);
            yield return null;
        }
    }
    // ?????¥ì?? ?????±í??
    IEnumerator ReloadCoroutine()
    {
        if (currentGun.carryBulletCount > 0)
        {
            isReload = true;
            currentGun.animator.SetTrigger("Reload");

            currentGun.carryBulletCount += currentGun.currentBulletCount;
            currentGun.currentBulletCount = 0;

            yield return new WaitForSeconds(currentGun.reloadTime);

            if (currentGun.carryBulletCount >= currentGun.reloadBulletCount)
            {
                currentGun.currentBulletCount = currentGun.reloadBulletCount;
                currentGun.carryBulletCount -= currentGun.reloadBulletCount;
            }
            else
            {
                currentGun.currentBulletCount = currentGun.carryBulletCount;
                currentGun.carryBulletCount = 0;
            }
            isReload = false;
        }
        else
        {
            Debug.Log("ì´??????? ?????µë?????.");
        }

    }
    // ë°???? ?????±í??
    IEnumerator RetroActionCoroutine()
    {
        Vector3 reCoilBack = new Vector3(currentGun.retroActionForce, originPos.y, originPos.z); // ???ì¡°ì????? ?????? ??? ë°???? ë§¥ì??
        Vector3 retroActionreCoilBack = new Vector3(currentGun.retroActionFineSightForce, currentGun.fineSightOriginPos.y, currentGun.fineSightOriginPos.z); // ???ì¡°ì????? ë°????ë§¥ì??

        if (!isFineSightMode) //???ì¡°ì?? ??????ê°? ???????????? ë°????
        {
            currentGun.transform.localPosition = originPos;

            // ë°??????????
            while (currentGun.transform.localPosition.x <= currentGun.retroActionForce - 0.02f) // -0.02??? Lerp??? ê°???? ????????? ?????´ì??ì§? ?????? whileë¬¸ì?? ???ì§????ê¸? ?????? ??µì??
            {
                currentGun.transform.localPosition = Vector3.Lerp(currentGun.transform.localPosition, reCoilBack, 0.4f);
                yield return null;
            }

            while (currentGun.transform.localPosition != originPos)
            {
                currentGun.transform.localPosition = Vector3.Lerp(-currentGun.transform.localPosition, originPos, 0.1f);
                yield return null;
            }
        }
        else
        { // ???ì¡°ì?? ?????? ??¼ë?? ë°????

            currentGun.transform.localPosition = currentGun.fineSightOriginPos; // ???ì¡°ì?? ????????? ì²? ???ì¹? ê°?

            // ë°??????????
            while (currentGun.transform.localPosition.x <= currentGun.retroActionFineSightForce - 0.02f) // -0.02??? Lerp??? ê°???? ????????? ?????´ì??ì§? ?????? whileë¬¸ì?? ???ì§????ê¸? ?????? ??µì??
            {
                currentGun.transform.localPosition = Vector3.Lerp(currentGun.transform.localPosition, retroActionreCoilBack, 0.4f);
                yield return null;
            }

            while (currentGun.transform.localPosition != currentGun.fineSightOriginPos)
            {
                currentGun.transform.localPosition = Vector3.Lerp(currentGun.transform.localPosition, currentGun.fineSightOriginPos, 0.1f);
                yield return null;
            }
        }
    }
    // ?????´ë?? ??????
    private void PlaySE(AudioClip clip)
    {
        audioSource.clip = clip;
        audioSource.Play();
    }

    public Gun GetGun()
    {
        return currentGun;
    }

    public bool GetFineSightMode()
    {
        return isFineSightMode;
    }
}
