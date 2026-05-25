using BankSystem.Shared.Entities;
using BankSystem.Shared.Enums;

namespace BankSystem.Shared.Interfaces
{
    /// <summary>
    /// Дугаарын дараалал удирдах repository-н гэрээ
    /// </summary>
    public interface IQueueRepository
    {
        /// <summary>Дараагийн дугаарын тасалбар олгох</summary>
        Task<QueueTicket> IssueNextTicketAsync();

        /// <summary>Дараалалд хүлээж буй дараагийн тасалбарыг авах</summary>
        Task<QueueTicket?> GetNextWaitingTicketAsync();

        /// <summary>ID-аар тасалбар хайх</summary>
        Task<QueueTicket?> GetByIdAsync(int id);

        /// <summary>Төлөвөөр тасалбаруудыг жагсаах</summary>
        Task<IEnumerable<QueueTicket>> GetByStatusAsync(TicketStatus status);

        /// <summary>Тасалбарын мэдээлэл шинэчлэх</summary>
        Task UpdateAsync(QueueTicket ticket);
    }
}
