using System.Net;
using System.Net.Sockets;

namespace API.Helpers
{
    public class Helper
    {
        public static bool IsValidIp(string ip)
        {
            return IPAddress.TryParse(ip, out var address) &&
                   (address.AddressFamily == AddressFamily.InterNetwork ||
                    address.AddressFamily == AddressFamily.InterNetworkV6);
        }

        public static bool IsInternalIp(string ip)
        {
            if (!IPAddress.TryParse(ip, out var address))
                return false;

            if (IPAddress.IsLoopback(address))
                return true;

            // Check for private IPv4 ranges (10.0.0.0/8, 172.16.0.0/12, 192.168.0.0/16)
            if (address.AddressFamily == AddressFamily.InterNetwork)
            {
                byte[] bytes = address.GetAddressBytes();
                return bytes[0] == 10 ||
                       (bytes[0] == 172 && bytes[1] >= 16 && bytes[1] <= 31) ||
                       (bytes[0] == 192 && bytes[1] == 168);
            }

            return false;
        }
    }
}

