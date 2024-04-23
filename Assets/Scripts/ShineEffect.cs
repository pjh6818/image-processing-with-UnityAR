using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;

public class ShineEffect : MonoBehaviour
{
    public RawImage image;
    public Material shineMaterial;
    public float minIntensity = 0.0f;
    public float maxIntensity = 0.5f;
    public float pulseSpeed = 3.0f;

    Color shineColor;
    bool shine;

    void Awake()
    {
        shineColor = shineMaterial.GetColor("_ShineColor");
        shineMaterial.SetColor("_ShineColor", new Color(0.0f, 0.0f, 0.0f, 0.0f));
    }

    public void LoadMaskTexture(Texture2D maskTexture)
    {
        image.texture = maskTexture;
        shineMaterial.SetTexture("_MaskTexture", maskTexture);
    }

    public void startShine()
    {
        shine = true;
    }

    public void stopShine()
    {
        shine = false;
    }

    void Update()
    {
#if !UNITY_EDITOR
        if (shine)
#endif
        {
            float shineIntensity = (Mathf.Sin(Time.time * pulseSpeed) + 1.0f) / 2.0f * (maxIntensity - minIntensity) + minIntensity;
            shineMaterial.SetColor("_ShineColor", new Color(shineColor.r, shineColor.g, shineColor.b, shineIntensity));
        }
    }
}
