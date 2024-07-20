using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour
{
    // ??? ???? 
    [SerializeField]
    private GunContoroller theGuncontroller;
    private Gun currentGun; //Guncontroller?? ?? Gun? ?? ??

    // ?? ui ??/??? ??
    [SerializeField]
    private GameObject go_BulletUIHUD;
    //?? Text ??
    [SerializeField]
    private Text[] text_Bullet;

    // Update is called once per frame
    void Update()
    {
        CheckBullet();
    }

    private void CheckBullet()
    {
        currentGun = theGuncontroller.GetGun(); // ??? ???
        text_Bullet[0].text = currentGun.carryBulletCount.ToString();
        text_Bullet[1].text = currentGun.reloadBulletCount.ToString();
        text_Bullet[2].text = currentGun.currentBulletCount.ToString();
    }
}
