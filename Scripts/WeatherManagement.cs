using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Mirror;

public class WeatherManagement : NetworkBehaviour
{
    public static WeatherManagement wm;
    public event Action onRain; 
    public event Action onSnow;
    public event Action onClear;

    public GameObject rainControl;
    public GameObject snowControl;
    [SyncVar(hook = nameof(triggerWeather))]
    public string weather = "clear";

    private NetworkManage room;
    private NetworkManage Room
    {
        get
        {
            if (room != null) { return room; }
            return room = NetworkManager.singleton as NetworkManage;
        }
    }

    public override void OnStartClient() {
        Room.WeatherManage.Add(this); 
    }

    public override void OnStopClient() {
        Room.WeatherManage.Remove(this);
    }

    public void updateWeather(string w) {
        foreach (WeatherManagement wm in Room.WeatherManage) {
            wm.weather = w;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        if (wm == null)
            wm = this;
        DontDestroyOnLoad(gameObject);
    }

    IEnumerator delay(float t)
    {
        yield return new WaitForSeconds(t);
    }

    public void setWeather(string w) {
        
    }

    public void triggerWeather(string oldValue, string newValue) {
        weather = newValue;
        updateWeather(newValue);
        if (this != wm) return;
        setup();
        if (weather == "clear") {
            Debug.Log ("clear");
            isClear();
            clearWeather();
        }
        else if (weather == "snow") {
            Debug.Log("snow");
            isSnowing();
            snowing();
        }
        else if (weather == "rain") {
            Debug.Log("rain");
            isRaining();
            raining();
        }
    }

    public void setup() {
        rainControl = GameObject.Find("Weather_rain");
        snowControl = GameObject.Find("Weather_snow");
    }

    public void isRaining() {
        if (onRain != null)
            onRain();
    }

    public void isSnowing() {
        if (onSnow != null)
            onSnow();
    }

    public void isClear() {
        if (onClear != null)
            onClear();
    }

    public void clearWeather() {
        if (rainControl != null)
            rainControl.GetComponent<ParticleSystem>().Stop();
        if (snowControl != null)
            snowControl.GetComponent<ParticleSystem>().Stop();
    }

    public void raining() {
        if (snowControl != null)
            snowControl.GetComponent<ParticleSystem>().Stop();
        else if (snowControl.GetComponent<ParticleSystem>().IsAlive())
            StartCoroutine(delay(3));
        if (rainControl != null)
            rainControl.GetComponent<ParticleSystem>().Play();
        
    }

    public void snowing() {
        if (rainControl != null)
            rainControl.GetComponent<ParticleSystem>().Stop();
        else if (rainControl.GetComponent<ParticleSystem>().IsAlive())
            StartCoroutine(delay(1));
        if (snowControl != null)
            snowControl.GetComponent<ParticleSystem>().Play();
    }    
}
