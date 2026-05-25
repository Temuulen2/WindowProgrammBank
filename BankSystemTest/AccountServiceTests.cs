using BankSystem.Server.Services;

namespace BankSystemTest
{
    [TestClass]
    public class AccountServiceTests
    {
        /// <summary>
        /// Хүрэлцэхүйц үлдэгдэлтэй үед гүйлгээ амжилттай болох ёстой.
        /// </summary>
        [TestMethod]
        public async Task Transfer_SufficientBalance_Succeeds()
        {
            var service = new AccountService();

            var (success, message) = await service.TransferAsync("ACC001", "ACC002", 100_000);

            Assert.IsTrue(success);
            Assert.AreEqual(900_000, service.GetAccount("ACC001")!.Balance);
            Assert.AreEqual(600_000, service.GetAccount("ACC002")!.Balance);
        }

        /// <summary>
        /// Үлдэгдэл хүрэлцэхгүй бол гүйлгээ татгалзах ёстой.
        /// </summary>
        [TestMethod]
        public async Task Transfer_InsufficientBalance_Fails()
        {
            var service = new AccountService();

            var (success, message) = await service.TransferAsync("ACC002", "ACC001", 999_999_999);

            Assert.IsFalse(success);
            Assert.Contains("хүрэлцэхгүй", message);
        }

        /// <summary>
        /// Байхгүй данс ашиглавал алдаа гарах ёстой.
        /// </summary>
        [TestMethod]
        public async Task Transfer_AccountNotFound_Fails()
        {
            var service = new AccountService();

            var (success, message) = await service.TransferAsync("ACC999", "ACC001", 1000);

            Assert.IsFalse(success);
            Assert.Contains("олдсонгүй", message);
        }

        /// <summary>
        /// 20 давхар гүйлгээ ирэхэд үлдэгдэл хоёр дахин хасагдахгүй байх ёстой.
        /// Per-account lock зөв ажиллаж байгааг баталгаажуулна.
        /// </summary>
        [TestMethod]
        public async Task Transfer_Concurrent_BalanceCorrect()
        {
            var service = new AccountService();
            // ACC001 үлдэгдэл: 1,000,000
            // 20 удаа 10,000 хасна = 200,000 нийт хасагдах ёстой
            var tasks = Enumerable.Range(0, 20)
                .Select(_ => service.TransferAsync("ACC001", "ACC002", 10_000));

            await Task.WhenAll(tasks);

            Assert.AreEqual(800_000, service.GetAccount("ACC001")!.Balance);
        }

        /// <summary>
        /// GetAccount байхгүй дансанд null буцаах ёстой.
        /// </summary>
        [TestMethod]
        public void GetAccount_NotFound_ReturnsNull()
        {
            var service = new AccountService();

            var account = service.GetAccount("INVALID");

            Assert.IsNull(account);
        }
    }
}
