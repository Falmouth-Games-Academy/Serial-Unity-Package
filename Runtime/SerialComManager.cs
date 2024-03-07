using UnityEngine;
using System.Collections;
using System.Linq;
#if SUP_API_SET
using System.IO.Ports;
#endif

namespace GamesAcademy.SerialPackage
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
    [SerializeField, Tooltip("Enter a serial port manually (e.g. COM3 or /dev/ttyUSB0) to skip the automatic search. Leave blank to search for all available ports.")]
    private string serialPortOverride = "";
    [SerializeField, Tooltip("This needs to match your Arduino code")]
    private int baudRate = 9600;
    [SerializeField, Tooltip("Enable extra serial debug messages in the console")]
    private bool debug = false;
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

        //Find the Arduino
        StartCoroutine(ConnectSerialPort());
    }

    private IEnumerator ConnectSerialPort()
    {
        // First we determine whether to use the override or to search for all available ports
        string[] availablePorts; 
        if(!string.IsNullOrEmpty(serialPortOverride))
        {
            availablePorts = new string[] { serialPortOverride };            
        } 
        else 
        {
            // Query the system for all available serial ports
            availablePorts = SerialPort.GetPortNames();

            // Platform specific code to filter out ports that are not likely to be Arduinos
#if UNITY_STANDALONE_WIN
                // Remove COM1 from the list of available ports, as it is usually an onboard serial port
                availablePorts = availablePorts.Where(port => !port.Equals("COM1")).ToArray();
#elif UNITY_STANDALONE_OSX
                // MacOS will populate allPorts with /dev/tty* and we don't want to try all of them
                // Remove any ports that don't start with /dev/tty.usb
                availablePorts = availablePorts.Where(port => port.StartsWith("/dev/tty.usb")).ToArray();
#endif
            Debug.Log("Looking for Arduino on available serial ports...");
        }
        
        bool foundArduino = false;
        
        foreach (string port in availablePorts)
        {
            if(debug) Debug.Log($"Trying {port}");
            sp = new SerialPort(port, baudRate);
            sp.DtrEnable = true;
            sp.ReadTimeout = serialReadTimeout;
            try
            {
                sp.Open();
            }
            catch(System.Exception e)
            {
                if(debug) Debug.Log($"Could not open port: {e.Message}");

#if UNITY_STANDALONE_LINUX
                    if(e.Message.Contains("Permission denied")) 
                        Debug.LogError("Your user does not have permission to access the serial port. On most distros you can run the following command, then reboot:\nsudo usermod -aG dialout yourusername");
#endif

                continue;
            }


            // The Arduino bootloader will first listen for code upload before running the sketch,
            // so we wait for 2 seconds for the sketch to start running before attempting communication
            yield return new WaitForSeconds(2);

            // Attempt to write the handshake character
            try
            {
                sp.Write(handshake);
            } 
            catch(System.Exception e)
            {
                if(debug) Debug.Log($"Could not write to port: {e.Message}");
                continue;
            }

            // Attempt to read the Arduino's response to the handshake character 
            string response = "";
            try
            {
                response = sp.ReadLine();
            } 
            catch(System.Exception e)
            {
                if(debug) Debug.Log($"Could not read from port: {e.Message}");
                continue;
            }
            
            // Check that the recevied character matches the configured handshake character 
            // We also trim any newline characters or whitespace
            if (response.Trim() == handshake)
            {
                foundArduino = true;
                break;
            }
        }


        //If you have no open port then throw a debug error
        if (!sp.IsOpen || !foundArduino)
        {
            Debug.LogError("No usable serial devices found.\n Is your device plugged in and implementing the handshake?");
            yield break;
        }
        else 
        {
            //Otherwise, we have found the device and are ready to communicate
            print($"Connected to Arduino on port: {sp.PortName}");
            serialReady = true;
        }
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

    public bool IsConnected()
    {
        return serialReady;
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