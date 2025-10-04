using Business.Interfaces;
using Business.Record;
using System.Collections.Concurrent;

namespace Infrastructure.Services
{
    public class RequestLogStore : IRequestLogStore
    {
        private readonly ConcurrentQueue<IpCheckLog> _logs = new();

        public void AddLog(IpCheckLog log) => _logs.Enqueue(log);

        public IEnumerable<IpCheckLog> GetAll() => _logs.ToArray();
    }
}
