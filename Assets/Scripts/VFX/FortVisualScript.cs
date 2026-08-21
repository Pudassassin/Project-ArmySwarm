using UnityEngine;

public class FortVisualScript : MonoBehaviour
{
    // Handle team color and interaction with UI / UX
    // prototype
    public GameObject VGOLight, VGOShade, VGOSelected;

    bool isSelected = false;
    SpriteRenderer spriteLight, spriteShade, spriteSelected;

    void Start()
    {
        spriteLight = VGOLight.GetComponent<SpriteRenderer>();
        spriteShade = VGOShade.GetComponent<SpriteRenderer>();
        spriteSelected = VGOSelected.GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        
    }

    public void SetColor(Color lightColor, Color shadeColor)
    {
        spriteLight.color = lightColor;
        spriteShade.color = shadeColor;
    }

    public void SetSelected(bool value)
    {
        isSelected = value;
        VGOSelected.SetActive(value);
    }
}
