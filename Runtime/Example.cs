using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Example : MonoBehaviour
{

    [SerializeField] private float brightness;
    [SerializeField] private Image im;
    private bool led = false;


    private void Update()
    {
        //Try to read value "a" from the arduino and store it in "brightness"
        float.TryParse(SerialComManager.instance.GetDataFromArduino("a"), out brightness);

        //Set the image colour to brightness
        im.color = new Color(brightness / 255f, brightness / 255f, brightness / 255f);
    }

    public void ButtonClick()
    {
        //Send the value "b" to the arduino along with the intended state of the LED
        char lPin = led ? '1' : '0';
        led = !led;
        SerialComManager.instance.SendDataToArduino($"b{lPin}");
    }

}
