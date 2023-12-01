using System.Collections;
using System.Collections.Generic;
using UnityEngine;






public class WeatherControl : MonoBehaviour
{
    public Material sunnySkybox;
    public Material rainySkybox;
    public Material foggySkybox;
    public Material snowySkybox;

    public GameObject rain;
    public GameObject hail;

    public float SunFog = 0.0001f;
    public float RainyFog = 0.02f;
    public float SnowFog = 0.01f;
    public float Fog = 0.04f;

    private void Start()
    {
        RenderSettings.skybox = sunnySkybox;
        RenderSettings.fogDensity = SunFog;
        rain.SetActive(false);
        hail.SetActive(false);
    }

    public void Sunny()
    {
        RenderSettings.skybox = sunnySkybox;
        rain.SetActive(false);
        hail.SetActive(false);
        RenderSettings.fogDensity = SunFog;
    }

    public void Rainy()
    {
        RenderSettings.skybox = rainySkybox;
        rain.SetActive(true);
        hail.SetActive(false);
        RenderSettings.fogDensity = RainyFog;
        
    }

    public void Foggy()
    {
        RenderSettings.skybox = foggySkybox;
        rain.SetActive(false);
        hail.SetActive(false);
        RenderSettings.fogDensity = Fog;
    }

    public void Snowy()
    {
        RenderSettings.skybox = snowySkybox;
        rain.SetActive(false);
        hail.SetActive(true);
        RenderSettings.fogDensity = SnowFog;
        
    }




}
        
