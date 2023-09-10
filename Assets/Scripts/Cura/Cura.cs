using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cura : MonoBehaviour
{
    public GameObject sprite;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 posicionNueva = new Vector3(0,sprite.transform.localPosition.y,Mathf.Sin(Time.time*2.5f)*1);
        Vector3 escalaNueva = new Vector3(Mathf.Sin(Time.time * 2.5f), sprite.transform.localScale.y, sprite.transform.localScale.z);

        sprite.transform.localPosition = posicionNueva;
        sprite.transform.localScale = escalaNueva;
    }
}
