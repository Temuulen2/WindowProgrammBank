using BankSystem.Shared.Entities;

namespace BankSystem.Server.Services
{
    /// <summary>
    /// Дансны мэдээлэл хадгалж, мөнгө шилжүүлэх логик.
    /// Deadlock-оос сэргийлж lock-уудыг дансны дугаарын эрэмбээр авна.
    /// </summary>
    public class AccountService
    {
        private readonly Dictionary<string, BankAccount> _accounts = new()
        {
            ["ACC001"] = new BankAccount { Id = 1, AccountNumber = "ACC001", OwnerName = "Болд",   Balance = 1_000_000, Currency = "MNT", CreatedAt = DateTime.Now },
            ["ACC002"] = new BankAccount { Id = 2, AccountNumber = "ACC002", OwnerName = "Сарнай", Balance = 500_000,   Currency = "MNT", CreatedAt = DateTime.Now },
            ["ACC003"] = new BankAccount { Id = 3, AccountNumber = "ACC003", OwnerName = "Ганбаяр",Balance = 2_000_000, Currency = "MNT", CreatedAt = DateTime.Now },
        };

        /// <summary>
        /// Данс тус бүрийн тусдаа lock.
        /// Нэг дансны гүйлгээ нөгөө дансны гүйлгээг саатуулахгүй.
        /// </summary>
        private readonly Dictionary<string, SemaphoreSlim> _locks = new();
        private readonly object _lockGuard = new();

        private SemaphoreSlim GetOrCreateLock(string accountNumber)
        {
            lock (_lockGuard)
            {
                if (!_locks.TryGetValue(accountNumber, out var sem))
                {
                    sem = new SemaphoreSlim(1, 1);
                    _locks[accountNumber] = sem;
                }
                return sem;
            }
        }

        /// <summary>
        /// Мөнгө шилжүүлнэ.
        /// Deadlock-оос сэргийлэх арга: хоёр lock-ыг үргэлж
        /// дансны дугаарын цагаан толгойн эрэмбээр авна.
        /// (ACC001, ACC003) → lock1=ACC001, lock2=ACC003
        /// (ACC003, ACC001) → lock1=ACC001, lock2=ACC003 (адилхан дараалал)
        /// Тиймээс хоёр thread хэзээ ч бие биенээ хүлээхгүй.
        /// </summary>
        public async Task<(bool Success, string Message)> TransferAsync(
            string fromAccount, string toAccount, decimal amount)
        {
            var (first, second) = string.Compare(fromAccount, toAccount) < 0
                ? (fromAccount, toAccount)
                : (toAccount, fromAccount);

            var lock1 = GetOrCreateLock(first);
            var lock2 = GetOrCreateLock(second);

            await lock1.WaitAsync();
            await lock2.WaitAsync();
            try
            {
                if (!_accounts.TryGetValue(fromAccount, out var from))
                    return (false, $"'{fromAccount}' данс олдсонгүй");

                if (!_accounts.TryGetValue(toAccount, out var to))
                    return (false, $"'{toAccount}' данс олдсонгүй");

                if (!from.IsActive || !to.IsActive)
                    return (false, "Данс идэвхгүй байна");

                if (from.Balance < amount)
                    return (false, $"Үлдэгдэл хүрэлцэхгүй ({from.Balance:N0}₮ < {amount:N0}₮)");

                from.Balance -= amount;
                to.Balance += amount;

                return (true, $"{amount:N0}₮ амжилттай: {from.OwnerName} → {to.OwnerName}");
            }
            finally
            {
                lock2.Release();
                lock1.Release();
            }
        }

        /// <summary>Нэг дансны мэдээлэл. Байхгүй бол null.</summary>
        public BankAccount? GetAccount(string accountNumber) =>
            _accounts.TryGetValue(accountNumber, out var acc) ? acc : null;

        /// <summary>Бүх дансны жагсаалт.</summary>
        public IEnumerable<BankAccount> GetAllAccounts() => _accounts.Values;
    }
}
