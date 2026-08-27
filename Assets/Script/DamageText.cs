using TMPro;
using UnityEngine;

public class DamageText : MonoBehaviour
{
    public float moveSpeed = 1f;
    public float destroyTime = 0.8f;
    
    private TextMeshPro textMesh;
    private Color textColor;

    void Awake()
    {
        textMesh = GetComponent<TextMeshPro>();
    }

    
    public void Setup(float damageAmount)
    {
        if (textMesh != null)
        {
            textMesh.text = damageAmount.ToString();
            textMesh.color = Color.red;
            textColor = textMesh.color;
        }
        Destroy(gameObject, destroyTime);
    }

    public void Setup(float amount, Color color, string prefix = "")
    {
        if (textMesh != null)
        {
            textMesh.text = prefix + amount.ToString("F0");
            textMesh.color = color;
            textColor = textMesh.color;
        }
        Destroy(gameObject, destroyTime);
    }

    void Update()
    {
        // 위로 이동하면서 투명해지기
        transform.position += Vector3.up * moveSpeed * Time.deltaTime;
        
        if (textMesh != null)
        {
            textColor.a -= Time.deltaTime / destroyTime;
            textMesh.color = textColor;
        }
    }
}