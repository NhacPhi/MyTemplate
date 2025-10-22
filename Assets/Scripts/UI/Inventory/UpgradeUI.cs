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
        zeroLayer.SetActive(false);
        firstLayer.SetActive(false);
        secondLayer.SetActive(false);
        switch (layer)
        {
            case 0:
                zeroLayer.SetActive(true);
                break;
            case 1:
                firstLayer.SetActive(true);
                break;
            case 2:
                secondLayer.SetActive(true);
                break;
        }
    }
}
