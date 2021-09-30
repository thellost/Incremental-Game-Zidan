using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.EventSystems;

public class TapArea : MonoBehaviour , IPointerDownHandler
{
    public void OnPointerDown(PointerEventData eventData)

    {
        Debug.Log("test tap");

        GameManager.Instance.CollectByTap (eventData.position, transform);

    }
}
