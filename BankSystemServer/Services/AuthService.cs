using BankSystem.Shared.Entities;
using System.Collections.Concurrent;

namespace BankSystem.Server.Services
{
    /// <summary>
    /// Теллерийн нэвтрэлт болон session удирдана.
    /// Token-based auth: нэвтрэхэд Guid token үүсгэж санах ойд хадгална.
    /// </summary>
    public class AuthService
    {
        /// <summary>
        /// Seed хэрэглэгчид. Жинхэнэ системд DB-с уншина.
        /// PasswordHash энд plain text — production-д BCrypt ашиглана.
        /// </summary>
        private readonly List<User> _users = new()
        {
            new User { Id = 1, Username = "teller1", PasswordHash = "1234", IsActive = true, TellerWindowId = 1 },
            new User { Id = 2, Username = "teller2", PasswordHash = "1234", IsActive = true, TellerWindowId = 2 },
        };

        /// <summary>
        /// Идэвхтэй session-ууд. Key = token, Value = нэвтэрсэн хэрэглэгч.
        /// ConcurrentDictionary: олон теллер нэгэн зэрэг нэвтрэхэд thread-safe.
        /// </summary>
        private readonly ConcurrentDictionary<string, User> _sessions = new();

        /// <summary>
        /// Нэвтрэнэ. Амжилттай бол session token буцаана.
        /// </summary>
        public (bool Success, string? Token, string? Message) Login(string username, string password)
        {
            var user = _users.FirstOrDefault(u =>
                u.Username == username &&
                u.PasswordHash == password &&
                u.IsActive);

            if (user is null)
                return (false, null, "Нэвтрэх нэр эсвэл нууц үг буруу");

            // Guid: давтагдашгүй, тааж мэдэхийн аргагүй token
            var token = Guid.NewGuid().ToString();
            _sessions[token] = user;
            return (true, token, null);
        }

        /// <summary>
        /// Token-оор нэвтэрсэн хэрэглэгч хайна. Олдохгүй бол null.
        /// </summary>
        public User? GetUserByToken(string token) =>
            _sessions.TryGetValue(token, out var user) ? user : null;

        /// <summary>
        /// Системээс гарна — session устгана.
        /// </summary>
        public void Logout(string token) =>
            _sessions.TryRemove(token, out _);
    }
}
