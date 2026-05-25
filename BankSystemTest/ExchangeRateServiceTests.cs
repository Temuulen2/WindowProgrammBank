using BankSystem.Server.Services;

namespace BankSystemTest
{
    [TestClass]
    public class ExchangeRateServiceTests
    {
        /// <summary>
        /// GetAll 4 валют буцаах ёстой (USD, EUR, CNY, RUB).
        /// </summary>
        [TestMethod]
        public void GetAll_ReturnsFourCurrencies()
        {
            var service = new ExchangeRateService();

            var rates = service.GetAll().ToList();

            Assert.HasCount(4, rates);
        }

        /// <summary>
        /// Зөв валютын кодоор ханш олдох ёстой.
        /// </summary>
        [TestMethod]
        public void Get_ValidCode_ReturnsRate()
        {
            var service = new ExchangeRateService();

            var rate = service.Get("USD");

            Assert.IsNotNull(rate);
            Assert.AreEqual("USD", rate.CurrencyCode);
        }

        /// <summary>
        /// Байхгүй валютын кодод null буцаах ёстой.
        /// </summary>
        [TestMethod]
        public void Get_InvalidCode_ReturnsNull()
        {
            var service = new ExchangeRateService();

            var rate = service.Get("XYZ");

            Assert.IsNull(rate);
        }

        /// <summary>
        /// Ханш шинэчилсний дараа шинэ утга хадгалагдах ёстой.
        /// </summary>
        [TestMethod]
        public void Update_ValidRate_UpdatesSuccessfully()
        {
            var service = new ExchangeRateService();

            var updated = service.Update("USD", 3500, 3520, 1);

            Assert.IsNotNull(updated);
            Assert.AreEqual(3500, updated.BuyRate);
            Assert.AreEqual(3520, updated.SellRate);
            Assert.AreEqual(3500, service.Get("USD")!.BuyRate);
        }

        /// <summary>
        /// Байхгүй валютыг шинэчлэхэд null буцаах ёстой.
        /// </summary>
        [TestMethod]
        public void Update_InvalidCode_ReturnsNull()
        {
            var service = new ExchangeRateService();

            var result = service.Update("XYZ", 100, 110, 1);

            Assert.IsNull(result);
        }

        /// <summary>
        /// Шинэчилсэн цаг UpdatedAt өөрчлөгдөх ёстой.
        /// </summary>
        [TestMethod]
        public void Update_ChangesUpdatedAt()
        {
            var service = new ExchangeRateService();
            var before = service.Get("EUR")!.UpdatedAt;

            System.Threading.Thread.Sleep(10);
            service.Update("EUR", 3800, 3840, 1);

            var after = service.Get("EUR")!.UpdatedAt;
            Assert.IsTrue(after > before);
        }
    }
}
