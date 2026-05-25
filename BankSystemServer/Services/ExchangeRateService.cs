using BankSystem.Shared.Entities;
using System.Collections.Concurrent;

namespace BankSystem.Server.Services
{
    /// <summary>
    /// Валютын ханш санах ойд хадгална.
    /// ConcurrentDictionary: нэмэлт lock бичилгүйгээр олон thread аюулгүй унших/бичих боломжтой.
    /// </summary>
    public class ExchangeRateService
    {
        private readonly ConcurrentDictionary<string, CurrencyRate> _rates = new()
        {
            ["USD"] = new CurrencyRate { Id = 1, CurrencyCode = "USD", CurrencyName = "Америк доллар", BuyRate = 3440, SellRate = 3460, UpdatedAt = DateTime.Now },
            ["EUR"] = new CurrencyRate { Id = 2, CurrencyCode = "EUR", CurrencyName = "Евро",           BuyRate = 3750, SellRate = 3780, UpdatedAt = DateTime.Now },
            ["CNY"] = new CurrencyRate { Id = 3, CurrencyCode = "CNY", CurrencyName = "Хятад юань",     BuyRate = 475,  SellRate = 480,  UpdatedAt = DateTime.Now },
            ["RUB"] = new CurrencyRate { Id = 4, CurrencyCode = "RUB", CurrencyName = "Оросын рубль",   BuyRate = 38,   SellRate = 40,   UpdatedAt = DateTime.Now },
        };

        /// <summary>Бүх валютын ханш.</summary>
        public IEnumerable<CurrencyRate> GetAll() => _rates.Values;

        /// <summary>Нэг валютын ханш. Байхгүй бол null.</summary>
        public CurrencyRate? Get(string currencyCode) =>
            _rates.TryGetValue(currencyCode.ToUpper(), out var rate) ? rate : null;

        /// <summary>
        /// Ханш шинэчилнэ. Амжилттай бол шинэ утгыг буцаана, олдохгүй бол null.
        /// Controller энэ утгыг SignalR-ээр Blazor дэлгэцэнд дамжуулна.
        /// </summary>
        public CurrencyRate? Update(string currencyCode, decimal buyRate, decimal sellRate, int tellerWindowId)
        {
            var code = currencyCode.ToUpper();
            if (!_rates.TryGetValue(code, out var existing)) return null;

            var updated = new CurrencyRate
            {
                Id = existing.Id,
                CurrencyCode = code,
                CurrencyName = existing.CurrencyName,
                BuyRate = buyRate,
                SellRate = sellRate,
                UpdatedAt = DateTime.Now,
                UpdatedByTellerWindowId = tellerWindowId
            };
            _rates[code] = updated;
            return updated;
        }
    }
}
