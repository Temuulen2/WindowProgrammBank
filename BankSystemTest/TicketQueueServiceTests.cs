using BankSystem.Server.Services;

namespace BankSystemTest
{
    [TestClass]
    public class TicketQueueServiceTests
    {
        /// <summary>
        /// Эхний дугаар 1-ээс эхлэх ёстой.
        /// </summary>
        [TestMethod]
        public async Task IssueTicket_FirstTicket_NumberIsOne()
        {
            var service = new TicketQueueService();

            var ticket = await service.IssueTicketAsync();

            Assert.AreEqual(1, ticket.TicketNumber);
        }

        /// <summary>
        /// Дараалан дугаар авахад нэгээр нэмэгдэх ёстой.
        /// </summary>
        [TestMethod]
        public async Task IssueTicket_Sequential_Increments()
        {
            var service = new TicketQueueService();

            var t1 = await service.IssueTicketAsync();
            var t2 = await service.IssueTicketAsync();
            var t3 = await service.IssueTicketAsync();

            Assert.AreEqual(1, t1.TicketNumber);
            Assert.AreEqual(2, t2.TicketNumber);
            Assert.AreEqual(3, t3.TicketNumber);
        }

        /// <summary>
        /// 50 давхар хүсэлт ирэхэд давтагдсан дугаар гарахгүй байх ёстой.
        /// Interlocked.Increment зөв ажиллаж байгааг баталгаажуулна.
        /// </summary>
        [TestMethod]
        public async Task IssueTicket_Concurrent_NoDuplicates()
        {
            var service = new TicketQueueService();
            var tasks = Enumerable.Range(0, 50)
                .Select(_ => service.IssueTicketAsync());

            var tickets = await Task.WhenAll(tasks);
            var numbers = tickets.Select(t => t.TicketNumber).ToList();

            // Давтагдсан дугаар байхгүй
            Assert.AreEqual(50, numbers.Distinct().Count());
            // 1-50 бүгд байна
            Assert.AreEqual(1, numbers.Min());
            Assert.AreEqual(50, numbers.Max());
        }

        /// <summary>
        /// CallNext нь FIFO дарааллыг баримтлах ёстой — эхэлсэн дугаараас эхлэн авна.
        /// </summary>
        [TestMethod]
        public async Task CallNext_ReturnsFIFO()
        {
            var service = new TicketQueueService();
            await service.IssueTicketAsync(); // 1
            await service.IssueTicketAsync(); // 2
            await service.IssueTicketAsync(); // 3

            var first = await service.CallNextAsync(1);
            var second = await service.CallNextAsync(1);

            Assert.AreEqual(1, first!.TicketNumber);
            Assert.AreEqual(2, second!.TicketNumber);
        }

        /// <summary>
        /// Дугаар дуудсаны дараа дарааллын тоо буурах ёстой.
        /// </summary>
        [TestMethod]
        public async Task CallNext_DecreasesQueueCount()
        {
            var service = new TicketQueueService();
            await service.IssueTicketAsync();
            await service.IssueTicketAsync();

            Assert.AreEqual(2, service.GetQueueCount());

            await service.CallNextAsync(1);

            Assert.AreEqual(1, service.GetQueueCount());
        }

        /// <summary>
        /// GetCurrentNumber нь хамгийн сүүлд дуудагдсан дугаарыг буцаах ёстой.
        /// </summary>
        [TestMethod]
        public async Task GetCurrentNumber_ReturnsLastCalledNumber()
        {
            var service = new TicketQueueService();
            await service.IssueTicketAsync();
            await service.IssueTicketAsync();

            await service.CallNextAsync(1);
            await service.CallNextAsync(1);

            Assert.AreEqual(2, service.GetCurrentNumber());
        }
    }
}
