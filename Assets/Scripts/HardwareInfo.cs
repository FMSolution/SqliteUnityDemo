using UnityEngine;
using System;
using System.Net.NetworkInformation;

public class HardwareInfo : MonoBehaviour
{
    void Start()
    {
        Debug.Log("--- Device MAC Address ---");
        Debug.Log(GetMacAddress());

        Debug.Log("--- Computer Specifications ---");
        LogComputerInfo();
    }

    // Retrieves the MAC address of the active network interface
    private string GetMacAddress()
    {
        try
        {
            NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();

            foreach (NetworkInterface adapter in nics)
            {
                // Filter out loopback and non-operational interfaces
                if (adapter.OperationalStatus == OperationalStatus.Up &&
                    adapter.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                {
                    PhysicalAddress address = adapter.GetPhysicalAddress();
                    string mac = address.ToString();

                    if (!string.IsNullOrEmpty(mac))
                    {
                        // Format the string into standard XX:XX:XX:XX:XX:XX format
                        return InsertSeparators(mac, ":");
                    }
                }
            }
        }
        catch (Exception e)
        {
            return $"Error retrieving MAC: {e.Message}";
        }

        return "No active MAC address found.";
    }

    private string InsertSeparators(string mac, string separator)
    {
        string[] bytes = new string[mac.Length / 2];
        for (int i = 0; i < bytes.Length; i++)
        {
            bytes[i] = mac.Substring(i * 2, 2);
        }
        return string.Join(separator, bytes);
    }

    // Logs general system and hardware information
    private void LogComputerInfo()
    {
        // Unique ID (Fallback for MAC address tracking)
        Debug.Log($"Unique Device ID: {SystemInfo.deviceUniqueIdentifier}");

        // Device Identity
        Debug.Log($"Device Name: {SystemInfo.deviceName}");
        Debug.Log($"Device Model: {SystemInfo.deviceModel}");
        Debug.Log($"Operating System: {SystemInfo.operatingSystem}");
        Debug.Log($"Device Type: {SystemInfo.deviceType}"); // Desktop, Handheld, Console

        // CPU Information
        Debug.Log($"Processor Type: {SystemInfo.processorType}");
        Debug.Log($"Processor Count: {SystemInfo.processorCount} cores");

        // Memory Information
        Debug.Log($"System Memory Size: {SystemInfo.systemMemorySize} MB");

        // GPU Information
        Debug.Log($"Graphics Device Name: {SystemInfo.graphicsDeviceName}");
        Debug.Log($"Graphics Memory Size: {SystemInfo.graphicsMemorySize} MB");
        Debug.Log($"Graphics API: {SystemInfo.graphicsDeviceType}");
    }
}
