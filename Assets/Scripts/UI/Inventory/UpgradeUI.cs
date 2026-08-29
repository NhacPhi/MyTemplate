using UnityEngine.UI;
using UnityEngine;

public class UpgradeUI : MonoBehaviour
{
    [SerializeField] private GameObject zeroLayer;
    [SerializeField] private GameObject firstLayer;
    [SerializeField] private GameObject secondLayer;
    private void Start()
    {
        //ActiveLayer(0);
    }
    public void ActiveLayer(int layer)
    {
        if (zeroLayer != null) zeroLayer.SetActive(false);
        if (firstLayer != null) firstLayer.SetActive(false);
        if (secondLayer != null) secondLayer.SetActive(false);
        switch (layer)
        {
            case 0:
                if (zeroLayer != null) zeroLayer.SetActive(true);
                break;
            case 1:
                if (firstLayer != null) firstLayer.SetActive(true);
                break;
            case 2:
                if (secondLayer != null) secondLayer.SetActive(true);
                break;
        }
    }
}
