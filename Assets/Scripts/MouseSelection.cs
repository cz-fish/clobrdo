using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseSelection : MonoBehaviour
{
    public GameObject hoverObject;

    void OnMouseDown()
    {
    }
    
    // Update is called once per frame
    void Update()
    {
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitInfo;
        if (Physics.Raycast(ray, out hitInfo)) {
            hoverObject = hitInfo.transform.root.gameObject;
        } else {
            hoverObject = null;
        }
    }
}
