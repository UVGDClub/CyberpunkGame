using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ClickHandler : MonoBehaviour, IPointerClickHandler {

    public UnityEvent Click;

    public void OnPointerClick( PointerEventData eventData ) {
        Click.Invoke();
    }
}
