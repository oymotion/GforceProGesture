using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hand : MonoBehaviour
{
    public Transform touchGameobj;

    public Quaternion nextQuaternion;

    Quaternion quaternion = Quaternion.Euler(new Vector3(0, 0, 0));

    private void OnTriggerEnter(Collider other)
    {
        touchGameobj = other.transform;
    }

    private void OnTriggerExit(Collider other)
    {
        touchGameobj = null;
    }

    public void SetQuaternion(Quaternion _nextQuaternion)
    {
        _nextQuaternion *= Quaternion.Inverse(quaternion);
        nextQuaternion = _nextQuaternion;
    }

    private void Update()
    {
        transform.rotation = nextQuaternion;
    }
}
