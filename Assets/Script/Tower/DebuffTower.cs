using UnityEngine;
using System.Collections.Generic;

public class DebuffTower : Tower
{
    public enum DebuffStatType { SpeedDecrease, AttackDecrease, DamageOverTime, ScrapDropIncrease }
    
    [Header("디버프 설정")]
    public DebuffStatType debuffType;
    [Range(1, 3)]
    public int debuffLevel = 1;
    public float debuffAmount = 0.05f; 

    private List<Enemy> activeTargets = new List<Enemy>();

    protected override void Update()
    {
        base.Update(); 

        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, attackRange);
        List<Enemy> currentTargets = new List<Enemy>();

        foreach (Collider2D col in colliders)
        {
            if (col.CompareTag("Enemy"))
            {
                Enemy enemy = col.GetComponent<Enemy>();
                // 적의 체력이 남아있는 경우에만 리스트에 추가합니다.
                if (enemy != null && enemy.hp > 0) 
                {
                    currentTargets.Add(enemy);
                }
            }
        }

        // 1. 기존에 있었지만 사거리를 벗어나거나 죽은 타겟의 디버프 해제
        for (int i = activeTargets.Count - 1; i >= 0; i--)
        {
            Enemy enemy = activeTargets[i];
            if (enemy == null || !currentTargets.Contains(enemy))
            {
                if (enemy != null && debuffType != DebuffStatType.DamageOverTime)
                {
                    enemy.RemoveDebuff(this);
                }
                activeTargets.RemoveAt(i);
            }
        }

        // 2. 새로 사거리에 들어온 타겟에게 디버프 적용
        foreach (Enemy enemy in currentTargets)
        {
            if (!activeTargets.Contains(enemy))
            {
                activeTargets.Add(enemy);
                if (debuffType != DebuffStatType.DamageOverTime)
                {
                    enemy.AddDebuff(this, debuffType, debuffLevel, debuffAmount);
                }
            }
        }
    }

    // 공격 주기(attackCooldown)에 맞춰 호출되는 함수
    protected override void PerformAction()
    {
        if (debuffType == DebuffStatType.DamageOverTime)
        {
            bool damagedAnyone = false;
            foreach (Enemy enemy in activeTargets)
            {
                if (enemy != null && enemy.hp > 0)
                {
                    // 적의 TakeDamage 함수를 호출하여 지속 데미지를 입힙니다.
                    enemy.TakeDamage(debuffAmount);
                    damagedAnyone = true;
                    Debug.DrawLine(transform.position, enemy.transform.position, Color.magenta, 0.2f);
                }
            }
            
            if (damagedAnyone)
            {
                lastAttackTime = Time.time;
                if (cdBarFill != null) cdBarFill.fillAmount = 0f;
            }
        }
        else
        {
            lastAttackTime = Time.time;
        }
    }
}