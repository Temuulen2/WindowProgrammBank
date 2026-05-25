using BankSystem.Shared.Entities;

namespace BankSystem.Shared.Interfaces
{
    /// <summary>
    /// Теллерийн цонх удирдах repository-н гэрээ
    /// </summary>
    public interface ITellerWindowRepository
    {
        /// <summary>ID-аар теллерийн цонх хайх</summary>
        Task<TellerWindow?> GetByIdAsync(int id);

        /// <summary>Бүх теллерийн цонхыг жагсаах</summary>
        Task<IEnumerable<TellerWindow>> GetAllAsync();

        /// <summary>Теллерийн цонхны мэдээлэл шинэчлэх</summary>
        Task UpdateAsync(TellerWindow window);
    }
}
