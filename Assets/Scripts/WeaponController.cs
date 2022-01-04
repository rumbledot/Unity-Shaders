using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WeaponController : MonoBehaviour
{
    [SerializeField] private GameObject BulletPrefabs;
    private List<Transform> BulletSources;
    [SerializeField] private float BulletSpeed = 1.0f;
    [SerializeField] private float CoolDownTime = 1.0f;
    private float CoolDownCounter = 0;
    [SerializeField] private int SourceTotal;
    [SerializeField] private int CurrentSource = 0;

    private void Awake()
    {
        this.BulletSources = gameObject.transform.GetComponentsInChildren<Transform>().AsEnumerable()
            .Where(c => c.name.Equals("Bullet Source"))
            .ToList();

        this.SourceTotal = this.BulletSources.Count();

        this.CoolDownCounter = this.CoolDownTime;
    }

    private void Update()
    {
        this.HandleCoolDownTime();

        this.HandleShooting();
    }

    private void HandleCoolDownTime() 
    {
        if (this.CoolDownCounter > 0)
        {
            this.CoolDownCounter -= 0.1f;
        }
        else
        {
            this.CoolDownCounter = 0f;
        }
    }

    private void HandleShooting()
    {
        if (Input.GetButton("Fire1"))
        {
            this.Shoot();
        }
    }

    public void Shoot()
    {
        if (this.CoolDownCounter > 0) return;

        var bullet = Instantiate(this.BulletPrefabs, this.BulletSources[this.CurrentSource].transform.position, Quaternion.LookRotation(this.transform.forward, this.transform.up));

        bullet.GetComponent<Rigidbody>().AddForce(this.transform.up * this.BulletSpeed);

        this.CoolDownCounter = this.CoolDownTime;

        this.CurrentSource++;

        if (this.CurrentSource >= this.SourceTotal)
        {
            this.CurrentSource = 0;
        }
    }
}
