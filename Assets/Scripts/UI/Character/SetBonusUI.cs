using TMPro;
using UnityEngine;

public class SetBonusUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI txtSetBonusName;
    [SerializeField] private TextMeshProUGUI txtSetBonusContent;

    public void UpdateSetBonusUI(string name, string content)
    {
        txtSetBonusName.text = name;
        txtSetBonusContent.text = content;
    }
}
