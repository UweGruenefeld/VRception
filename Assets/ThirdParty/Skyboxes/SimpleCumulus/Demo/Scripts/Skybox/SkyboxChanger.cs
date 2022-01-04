using UnityEngine;
using UnityEngine.UI;

#pragma warning disable CS0649
public class SkyboxChanger : MonoBehaviour
{
    [SerializeField]
    private Dropdown _dropdown;

    public Material[] Skyboxes;

    //public void Awake()
    //{
    //    _dropdown = GetComponent<Dropdown>();
    //}

    public void ChangeSkybox()
    {
        RenderSettings.skybox = Skyboxes[_dropdown.value];
        RenderSettings.skybox.SetFloat("_Rotation", 0);
    }

    public void NextSkybox()
    {
        _dropdown.value = (_dropdown.value < Skyboxes.Length - 1) ? _dropdown.value + 1 : _dropdown.value = 0;
        ChangeSkybox();
    }
}