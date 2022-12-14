using System;
using System.IO.Ports;

internal class Program
{
    // Create the serial port with basic settings 
    private static SerialPort port = new SerialPort("COM13", 9600, Parity.None, 8, StopBits.One);
    private static SensorsInfo sensorsInfo = new SensorsInfo();
    private static MessageSender messageSender = new MessageSender(60000, sensorsInfo);

    [STAThread]
    static void Main(string[] args)
    {
        // New monitoring thread in background
        Thread monitoringThread = new Thread(messageSender.StartMonitoring) { IsBackground = true }; 
        monitoringThread.Start();  // Start thread
        Console.WriteLine("Incoming Data:"); 
        port.DataReceived += new SerialDataReceivedEventHandler(port_DataReceived);
        // Begin communications 
        port.Open();
        Console.ReadLine();
    }

    private static void port_DataReceived(object sender, SerialDataReceivedEventArgs e)
    {
        sensorsInfo.ParseSensorsLineData(port.ReadLine());
        Console.WriteLine(sensorsInfo.ToString());
    }
}