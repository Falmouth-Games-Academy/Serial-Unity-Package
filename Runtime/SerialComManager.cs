using UnityEngine;
#if SUP_API_SET
using System.IO.Ports;
#endif
namespace SerialUnityPackage
{

    // Add to a namespace
    public class SerialComManager : MonoBehaviour
    {


#if SUP_API_SET

    public static SerialComManager instance;
    private SerialPort sp = new SerialPort();

    private const string handshake = "x";
    [SerializeField, Tooltip("Number of milliseconds before a serial read will timeout and return nothing")]
    private int serialReadTimeout = 500;
    [SerializeField, Tooltip("This needs to be the same as in your arduino code")]
    private int baudRate = 9600;
    private bool serialReady = false;

    private void Start()
    {
        //Initialise the singleton
        if (instance != null)
        {
            Destroy(this);
            return;
        }
        instance = this;

        //Find the arduino
        FindSerialPort();
    }

    private void FindSerialPort()
    {
        //Look at all serial ports available and send the handshake character to each of them
        string[] availablePorts = SerialPort.GetPortNames();
        foreach (string port in availablePorts)
        {
            sp = new SerialPort(port, 9600);
            sp.ReadTimeout = serialReadTimeout;
            sp.Open();
            sp.Write(handshake);
            string response = sp.ReadLine();
            //if you receive the handshake character back then a connection is made and the port is open
            if (response == handshake)
                break;
        }


        //If you have no open port then throw a debug error
        if (!sp.IsOpen)
        {
            Debug.LogError("No COM Useable Devices Found\n Is your device plugged in and implementing the handshake?");
            return;
        }
        //Otherwise, we have found the device and are ready to communicate
        print($"Found Device on: {sp.PortName}");

        serialReady = true;

    }

    /// <summary>
    /// Get info from the arduino by sending the id of the info you want
    /// </summary>
    /// <param name="_infoID"></param>
    /// <returns></returns>
    public string GetDataFromArduino(string _infoID)
    {
        if (!serialReady) return "";
        string _data = "";

        //Write the data to the arduino and then immediately try to read the response
        sp.Write(_infoID);
        _data = sp.ReadLine();

        return _data;
    }

    public void SendDataToArduino(string _data)
    {
        //Send the info to the arduino
        if (!serialReady) return;
        sp.Write(_data);
    }

    private void OnDestroy()
    {
        //Send the kill character to the arduino when this gameobject is destroyed to close the serial port
        SendDataToArduino("-");
        sp.Close();

        instance = null;
    }
#else
        private void Awake()
        {
            Debug.LogError("Please update .Net Version");
            Debug.Break();
        }
#endif
    }

}