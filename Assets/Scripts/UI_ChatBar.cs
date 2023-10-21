using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_ChatBar : MonoBehaviour
{
    [SerializeField] Text T_Text;
    RectTransform Rt_RectTransform;
    

    private void Awake()
    {
        Rt_RectTransform = GetComponent<RectTransform>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public float SetInit(string str, float posY)
    {
        SetInit(str, Color.black);
        Rt_RectTransform.anchoredPosition = new Vector2(Rt_RectTransform.anchoredPosition.x, -posY);
        return Rt_RectTransform.sizeDelta.y;
    }

    public void SetInit(string str, Color color)
    {
        T_Text.color = color;   
        T_Text.text = str;
        Rt_RectTransform.sizeDelta = new Vector2(Rt_RectTransform.sizeDelta.x, T_Text.preferredHeight + 2);
            
    }
}
