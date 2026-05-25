using BankSystem.Shared.Entities;
using BankSystem.Shared.Enums;
using System.Threading.Channels;

namespace BankSystem.Server.Services
{
    /// <summary>
    /// Дугаарын дарааллыг удирдана. Гурван асуудлыг шийдсэн:
    /// 1. Давхар дугаар олгохгүй  — Interlocked.Increment
    /// 2. Нэг дугаарыг хоёр теллер авахгүй — Channel.TryRead (atomic)
    /// 3. FIFO дараалал алдагдахгүй — Channel (unbounded биш, bounded)
    /// </summary>
    public class TicketQueueService
    {
        /// <summary>
        /// Сүүлд олгосон дугаар. volatile: CPU cache биш санах ойгоос уншина.
        /// Interlocked.Increment-тэй хослуулснаар бүрэн thread-safe.
        /// </summary>
        private volatile int _lastNumber = 0;

        /// <summary>Теллер хамгийн сүүлд дуудсан дугаар.</summary>
        private volatile int _currentCalledNumber = 0;

        /// <summary>
        /// FIFO дараалал. Capacity=100: дараалал дүүрвэл шинэ хүсэлт хүлээнэ, хаяхгүй.
        /// WriteAsync → эцэст нэмнэ. TryRead → эхнээс атомар авна.
        /// </summary>
        private readonly Channel<QueueTicket> _queue =
            Channel.CreateBounded<QueueTicket>(new BoundedChannelOptions(100)
            {
                FullMode = BoundedChannelFullMode.Wait
            });

        /// <summary>
        /// Async lock. lock{} keyword нь await-тай хамт ашиглаж болохгүй тул
        /// SemaphoreSlim(1,1) ашиглана — нэг үед нэг thread л орно.
        /// </summary>
        private readonly SemaphoreSlim _issueLock = new(1, 1);

        /// <summary>
        /// Хүлээж буй дугааруудын snapshot жагсаалт.
        /// Channel-г enumerate хийж болохгүй тул тусад нь хадгална.
        /// _snapshotLock: энгийн lock{} — await дотор ашиглахгүй тул SemaphoreSlim хэрэггүй.
        /// </summary>
        private readonly List<int> _waitingSnapshot = new();
        private readonly object _snapshotLock = new();

        /// <summary>
        /// Шинэ дугаар олгоно. Interlocked.Increment атомар тул
        /// 50 давхар хүсэлт ирсэн ч давтагдсан дугаар гарахгүй.
        /// </summary>
        public async Task<QueueTicket> IssueTicketAsync()
        {
            await _issueLock.WaitAsync();
            try
            {
                var number = Interlocked.Increment(ref _lastNumber);
                var ticket = new QueueTicket
                {
                    TicketNumber = number,
                    IssuedAt = DateTime.Now,
                    Status = TicketStatus.Waiting
                };
                await _queue.Writer.WriteAsync(ticket);
                lock (_snapshotLock) _waitingSnapshot.Add(ticket.TicketNumber);
                return ticket;
            }
            finally
            {
                // finally: алдаа гарсан ч lock заавал суллагдана
                _issueLock.Release();
            }
        }

        /// <summary>
        /// Дарааллаас дараагийн дугаар авна.
        /// TryRead атомар тул хоёр теллер нэгэн зэрэг дарсан ч нэг дугаар хоёрт очихгүй.
        /// </summary>
        public async Task<QueueTicket?> CallNextAsync(int tellerWindowId)
        {
            if (await _queue.Reader.WaitToReadAsync())
            {
                if (_queue.Reader.TryRead(out var ticket))
                {
                    ticket.Status = TicketStatus.Called;
                    ticket.CalledAt = DateTime.Now;
                    ticket.TellerWindowId = tellerWindowId;
                    _currentCalledNumber = ticket.TicketNumber;
                    lock (_snapshotLock) _waitingSnapshot.Remove(ticket.TicketNumber);
                    return ticket;
                }
            }
            return null;
        }

        /// <summary>Дараалалд хүлээж буй хүний тоо.</summary>
        public int GetQueueCount() => _queue.Reader.Count;

        /// <summary>Хамгийн сүүлд дуудагдсан дугаар.</summary>
        public int GetCurrentNumber() => _currentCalledNumber;

        /// <summary>
        /// Хүлээж буй дугааруудын жагсаалт (snapshot хуулбар).
        /// Channel enumerate хийж болохгүй тул тусдаа жагсаалт хадгалдаг.
        /// </summary>
        public List<int> GetWaitingNumbers()
        {
            lock (_snapshotLock) return new List<int>(_waitingSnapshot);
        }
    }
}
