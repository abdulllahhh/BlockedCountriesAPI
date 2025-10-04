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
    }
}
