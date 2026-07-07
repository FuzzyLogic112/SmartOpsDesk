using SmartOpsDesk.Models;

namespace SmartOpsDesk.Services;

public interface ITicketRepository
{
    List<WorkTicket> Load();
    void Save(IEnumerable<WorkTicket> tickets);
}
