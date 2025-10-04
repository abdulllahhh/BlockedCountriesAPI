using Business.Record;

namespace Business.Interfaces
{
    public interface IRequestLogStore
    {
        void AddLog(IpCheckLog log);
        IEnumerable<IpCheckLog> GetAll();
    }
}
