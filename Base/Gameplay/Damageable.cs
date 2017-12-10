﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class Damageable : MonoBehaviour
{
    public bool InitOnStart;
    
    public bool Invincible;
    public int  MaxHealth;
    public int  StartingHealth;
    public float Delay = 1;

    private float LastHurt;

    public int  CurrentHealth { get; private set; }

    public bool IsAlive { get { return CurrentHealth > 0; } }

    public GameObject OnDead;
    public AnimationPlayer DeathAnim;
    public float DedDuration;
    
    void Start()
    {
        if (!InitOnStart)
            return;

        Init();
    }

    public void Init()
    {
        CurrentHealth = StartingHealth;
    }

    public void Heal(int amount)
    {
        CurrentHealth += amount;
        CurrentHealth  = Mathf.Clamp(CurrentHealth, 0, MaxHealth);
    }

    public void Hurt(int amount)
    {
        if (Invincible || !IsAlive)
            return;

        if (Time.time - LastHurt < Delay)
            return;

        LastHurt = Time.time;
        CurrentHealth -= amount;
        CurrentHealth  = Mathf.Clamp(CurrentHealth, 0, MaxHealth);
        
        if (!IsAlive)
            Dead();
    }

    public void Dead()
    {
        StartCoroutine(CoDead());
    }

    IEnumerator CoDead()
    {
        DeathAnim.Play();
        var go = Instantiate(OnDead, transform.position, Quaternion.identity);
        yield return new WaitForSeconds(DedDuration);
        DestroyObject(go);
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        if (!Application.isEditor)
            return;
        
        StartingHealth = Mathf.Clamp(StartingHealth, 0, MaxHealth);
    }

    void OnDrawGizmosSelected()
    {
        Handles.Label(transform.position + Vector3.up * 1, "HP : " + CurrentHealth + "/" + MaxHealth);
    }
#endif
}
