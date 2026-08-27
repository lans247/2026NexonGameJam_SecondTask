using UnityEngine;
using System.Collections.Generic;

public class BuffTower : Tower
{
    public enum BuffStatType { Range, PeriodicHeal, AttackSpeed, AttackDamage }
    
    [Header("버프 설정")]
    public BuffStatType buffType;
    [Range(1, 3)]
    public int buffLevel = 1;
    public float buffAmount = 0.5f; 

    private List<Defender> activeTargets = new List<Defender>();

    protected override void Update()
    {
        base.Update(); // Tower.cs의 사거리 표시 로직 등 실행

        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, attackRange);
        List<Defender> currentTargets = new List<Defender>();

        foreach (Collider2D col in colliders)
        {
            if (col.CompareTag("Defender"))
            {
                Defender def = col.GetComponent<Defender>();
                if (def != null && def.hp > 0)
                {
                    currentTargets.Add(def);
                }
            }
        }

        // 1. 기존에 있었지만 사거리를 벗어나거나 죽은 타겟의 오라 해제
        for (int i = activeTargets.Count - 1; i >= 0; i--)
        {
            Defender def = activeTargets[i];
            if (def == null || !currentTargets.Contains(def))
            {
                if (def != null && buffType != BuffStatType.PeriodicHeal)
                {
                    def.RemoveAura(this);
                }
                activeTargets.RemoveAt(i);
            }
        }

        // 2. 새로 사거리에 들어온 타겟에게 오라 적용
        foreach (Defender def in currentTargets)
        {
            if (!activeTargets.Contains(def))
            {
                activeTargets.Add(def);
                if (buffType != BuffStatType.PeriodicHeal)
                {
                    def.AddAura(this, buffType, buffLevel, buffAmount);
                }
            }
        }
    }

    // base.Update()에서 attackCooldown 주기에 맞춰 호출되는 함수
    protected override void PerformAction()
    {
        if (buffType == BuffStatType.PeriodicHeal)
        {
            bool healedAnyone = false;
            foreach (Defender def in activeTargets)
            {
                if (def != null && def.hp < def.maxHp)
                {
                    def.Heal(buffAmount);
                    healedAnyone = true;
                    // 디버깅/시각화용 초록색 라인
                    Debug.DrawLine(transform.position, def.transform.position, Color.green, 0.2f);
                }
            }
            
            // 한 명이라도 힐을 줬다면 쿨타임을 초기화
            if (healedAnyone)
            {
                lastAttackTime = Time.time;
                if (cdBarFill != null) cdBarFill.fillAmount = 0f;
            }
        }
        else
        {
            // 상시 오라 타워의 경우 쿨타임이 무의미하므로 타이머만 갱신
            lastAttackTime = Time.time;
        }
    }
}