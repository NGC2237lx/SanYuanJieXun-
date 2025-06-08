using UnityEngine;
using UnityEngine.UI;

public class DisplayPanel : MonoBehaviour
{
    public Text nameText;
    public Image iconImage;
    public Text descriptionText;

    public void Show(string name, Sprite sprite, string description)
    {
        // 更新UI显示
        nameText.text = name;
        iconImage.sprite = sprite;
        descriptionText.text = description;

        // 显示Panel（假设默认隐藏）
        //gameObject.SetActive(true);
    }
}