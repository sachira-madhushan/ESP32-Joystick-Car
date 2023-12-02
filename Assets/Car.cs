using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System.IO;
using System.Threading.Tasks;
using System.IO.Ports;
public class Car : MonoBehaviour
{
    SerialPort serialPort;
    public WheelCollider backLeft, backRight, frontLeft, frontRight;
    public Transform backLeftTrans, backRightTrans, frontLeftTrans, frontRightTrans;

    float _angle = 25f;
    float _motorSpeed = 100;

    float h, v;

    private TcpClient tcpClient;
    private StreamWriter writer;
    private StreamReader reader;

    bool idDriving = true;
    void Start()
    {
        //ConnectToESP32();
        //Task.Run(() => CheckPIRStatusPeriodically());
        Task.Run(() => getJoyStickData());
        serialPort = new SerialPort("COM4", 115200);
        serialPort.Open();
    }

    async void getJoyStickData()
    {
        while (true)
        {
            SerialRead();
        }
    }

    void SerialRead()
    {
        if (serialPort.IsOpen)
        {
            string data = serialPort.ReadLine();
            string[] dataList = data.Split(",");
            int x = int.Parse(dataList[0]);
            int y = int.Parse(dataList[1]);
            if ((x == 0 || x == -1))
            {
                h = 0;
                idDriving = false;
            }
            else if (x > 0)
            {
                h = -1;
                idDriving = true;
            }
            else if (x < 0)
            {
                h = 1;
                idDriving = true;
            }

            if ((y == 0 || y == -1))
            {
                v = 0;
                idDriving = false;
            }
            else if (y > 0)
            {
                v = 1;
                idDriving = true;
            }
            else if (y < 0)
            {
                v = -1;
                idDriving = true;
            }


        }
    }
    async void CheckPIRStatusPeriodically()
    {
        while (true)
        {
            //await Task.Delay(500);
            RequestPIRStatus();
        }
    }

    void ConnectToESP32()
    {
        tcpClient = new TcpClient("192.168.4.1", 80);

        if (tcpClient.Connected)
        {
            Debug.Log("Connected to ESP32");
            writer = new StreamWriter(tcpClient.GetStream());
            reader = new StreamReader(tcpClient.GetStream());
        }
    }

    void RequestPIRStatus()
    {
        if (tcpClient != null && tcpClient.Connected)
        {
            string response = reader.ReadLine();

            if (response != null)
            {
                if (response.Contains("Respond:1"))
                {
                    Debug.Log("Motion detected in Unity!");
                    idDriving = false;
                }
                else if (response.Contains("Respond:0"))
                {
                    Debug.Log("No motion detected in Unity.");
                    idDriving = true;
                }
            }
        }
    }

    void OnDestroy()
    {
        if (tcpClient != null)
            tcpClient.Close();
    }

    void Update()
    {

        

        //h = 0;
        //v = 1;
        //Debug.Log(idDriving);
        frontLeft.steerAngle = _angle * h;
        frontRight.steerAngle = _angle * h;

        if (idDriving)
        {
            Drive();
        }
        else
        {
            Brake();

        }
        UpdateWheel(backLeft, backLeftTrans);
        UpdateWheel(backRight, backRightTrans);
        UpdateWheel(frontLeft, frontLeftTrans);
        UpdateWheel(frontRight, frontRightTrans);
    }

    void Drive()
    {
        backLeft.brakeTorque = 0;
        backRight.brakeTorque = 0;
        backLeft.motorTorque = _motorSpeed * v;
        backRight.motorTorque = _motorSpeed * v;
    }

    void Brake()
    {
        backLeft.brakeTorque = 50;
        backRight.brakeTorque = 50;
    }

    void UpdateWheel(WheelCollider col, Transform t)
    {
        Vector3 position = t.position;
        Quaternion rotation = t.rotation;

        col.GetWorldPose(out position, out rotation);

        t.position = position;
        t.rotation = rotation;
    }

    private void OnApplicationQuit()
    {
        serialPort.Close();
    }
}
