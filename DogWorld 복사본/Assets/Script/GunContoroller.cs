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
    //?????? ??�착??? �? 
    [SerializeField]
    private Gun currentGun;
    private CrossHair theCrossHair;

    //??��?? ?????? �????
    private float currentFireRate;
    // ?????? �????
    private bool isReload = false;
    private bool isFineSightMode = false;

    // 본�?? ???�???? �?
    private Vector3 originPos;
    // ???과�??
    private AudioSource audioSource;
    private RaycastHit hitInfo; // hit??? �?체�?? ???보를 �???????
    // ??�격 ??��????? 보�?????
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
    // ??��???????? ???�????
    private void GunFireRateCalc()
    {
        if (currentFireRate > 0)
            currentFireRate -= Time.deltaTime; // 1?? ??? ?? ?? 1/60    
    }
    // �??????????
    private void TryFire()
    {
        if (Input.GetButton("Fire1") && currentFireRate <= 0 && isReload == false)
        {
            Fire();
        }
    }
    // �???? ??? �????
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
    // �???? ??? �????
    private void Shoot()
    {//???
        theCrossHair.Fireanimation();
        currentGun.currentBulletCount--;
        currentFireRate = currentGun.fireRate; // ???? ???x
        PlaySE(currentGun.fire_Sound);
        currentGun.muzzleFlash.Play();
        Hit();

        StopAllCoroutines(); // ??��?????�? ?????? �?�???? ???�? while�? ??��?? ??��?? �?�?
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
            , out hitInfo, currentGun.range))// 1�?�? ??��???????? �?�????�??????? ??????�? 그�?? ???�??????? ??? ??��????? ????????? �????�? ??????�? ?????? 기�??????????? �????�? �???��??�? ???문�??
        {
            var clone = Instantiate(hit_effect_prefab, hitInfo.point, UnityEngine.Quaternion.LookRotation(hitInfo.normal));
            Destroy(clone, 2.0f);
        }
    }

    // ?????��?? ??????
    private void TryReload()
    {

        if (Input.GetKeyDown(KeyCode.R) && !isReload && currentGun.currentBulletCount < currentGun.reloadBulletCount)
        {
            CancleFineSight();
            StartCoroutine(ReloadCoroutine());
        }


    }
    // ???조�?? ??????
    private void TryFineSight()
    {
        //??? ????
        if (Input.GetButtonDown("Fire2") && !isReload)
        {
            Finesight();
        }
    }
    // ???조�?? �????
    public void CancleFineSight()
    {
        if (isFineSightMode)
        {
            Finesight();
        }
    }
    // ???조�?? �?�? �????
    private void Finesight()
    {
        isFineSightMode = !isFineSightMode;
        currentGun.animator.SetBool("FineSight", isFineSightMode);
        theCrossHair.FineSightAnimation(isFineSightMode);
        if (isFineSightMode == true)
        {
            StopAllCoroutines(); // ??��?? �???? �?�?
            StartCoroutine(FineSightActivateCoroutine());
        }
        else
        {
            StopAllCoroutines();
            StartCoroutine(FineSightDeActivateCoroutine());
        }

    }
    // ???조�?? ?????��??
    IEnumerator FineSightActivateCoroutine()
    {
        while (currentGun.transform.localPosition != currentGun.fineSightOriginPos)// ??? ???? ???? ??? ?? ??
        {
            currentGun.transform.localPosition = Vector3.Lerp(currentGun.transform.localPosition, currentGun.fineSightOriginPos, 0.2f);
            yield return null;
        }
    }
    // ???조�?? �??????��??
    IEnumerator FineSightDeActivateCoroutine()
    {
        while (currentGun.transform.localPosition != originPos)// origin ???? ???? ??? ?? ??
        {
            currentGun.transform.localPosition = Vector3.Lerp(currentGun.transform.localPosition, originPos, 0.2f);
            yield return null;
        }
    }
    // ?????��?? ?????��??
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
            Debug.Log("�??????? ?????��?????.");
        }

    }
    // �???? ?????��??
    IEnumerator RetroActionCoroutine()
    {
        Vector3 reCoilBack = new Vector3(currentGun.retroActionForce, originPos.y, originPos.z); // ???조�????? ?????? ??? �???? 맥�??
        Vector3 retroActionreCoilBack = new Vector3(currentGun.retroActionFineSightForce, currentGun.fineSightOriginPos.y, currentGun.fineSightOriginPos.z); // ???조�????? �????맥�??

        if (!isFineSightMode) //???조�?? ??????�? ???????????? �????
        {
            currentGun.transform.localPosition = originPos;

            // �??????????
            while (currentGun.transform.localPosition.x <= currentGun.retroActionForce - 0.02f) // -0.02??? Lerp??? �???? ????????? ?????��??�? ?????? while문�?? ???�????�? ?????? ??��??
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
        { // ???조�?? ?????? ??��?? �????

            currentGun.transform.localPosition = currentGun.fineSightOriginPos; // ???조�?? ????????? �? ???�? �?

            // �??????????
            while (currentGun.transform.localPosition.x <= currentGun.retroActionFineSightForce - 0.02f) // -0.02??? Lerp??? �???? ????????? ?????��??�? ?????? while문�?? ???�????�? ?????? ??��??
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
    // ?????��?? ??????
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
