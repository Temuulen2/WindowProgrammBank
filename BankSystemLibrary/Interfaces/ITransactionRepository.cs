using BankSystem.Shared.Entities;

namespace BankSystem.Shared.Interfaces
{
    /// <summary>
    /// Мөнгөн гүйлгээ удирдах repository-н гэрээ
    /// </summary>
    public interface ITransactionRepository
    {
        /// <summary>ID-аар гүйлгээ хайх</summary>
        Task<Transaction?> GetByIdAsync(int id);

        /// <summary>Теллерийн цонхоор гүйлгээний түүх авах</summary>
        Task<IEnumerable<Transaction>> GetByTellerWindowAsync(int tellerWindowId);

        /// <summary>Шинэ гүйлгээ бүртгэх</summary>
        Task<Transaction> AddAsync(Transaction transaction);

        /// <summary>Гүйлгээний мэдээлэл шинэчлэх</summary>
        Task UpdateAsync(Transaction transaction);
    }
}
