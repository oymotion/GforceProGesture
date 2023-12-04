using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DeviceConnectPanel : MonoBehaviour
{
    [SerializeField] Button CloseButton;
    // Start is called before the first frame update
    void Start()
    {
        CloseButton.onClick.AddListener(() =>
        {
            gameObject.SetActive(false);
        });
    }
}
