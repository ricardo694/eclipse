using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BotonUI : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    public void OnPointerEnter(PointerEventData eventData)
    {
        UISoundController.Instance.PlayHover();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        UISoundController.Instance.PlayClick();
    }
}