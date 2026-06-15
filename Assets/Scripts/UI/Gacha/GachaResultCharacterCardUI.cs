using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GachaResultCharacterCardUI : GachaResultCardUI
{
    [Header("Shard Conversion UI")]
    [Tooltip("The parent GameObject containing the shard icon and text, which will be enabled if converted.")]
    [SerializeField] private GameObject objShardGroup;
    
    [Tooltip("Text component to display the shard amount (e.g., '+30').")]
    [SerializeField] private TextMeshProUGUI txtShardAmount;

    public void SetShardConversion(bool isConverted, int shardAmount)
    {
        if (objShardGroup != null)
        {
            objShardGroup.SetActive(isConverted);
        }

        if (isConverted && txtShardAmount != null)
        {
            // Có thể thêm localize hoặc text tuỳ chỉnh nếu cần
            txtShardAmount.text = $"+{shardAmount}";
        }
    }
}
