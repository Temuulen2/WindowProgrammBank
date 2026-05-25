namespace BankSystem.Shared.Entities
{
    /// <summary>
    /// Теллер хэрэглэгч — системд нэвтэрч ажиллах банкны ажилтан
    /// </summary>
    public class User
    {
        /// <summary>Өгөгдлийн сангийн өвөрмөц дугаар</summary>
        public int Id { get; set; }

        /// <summary>Нэвтрэх нэр</summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>Нууц үгийн hash (plain text хадгалахгүй)</summary>
        public string PasswordHash { get; set; } = string.Empty;

        /// <summary>Хэрэглэгч идэвхтэй эсэх</summary>
        public bool IsActive { get; set; } = true;

        /// <summary>Ажиллаж байгаа теллерийн цонхны ID (нэвтрээгүй бол null)</summary>
        public int? TellerWindowId { get; set; }

        /// <summary>Ажиллаж байгаа теллерийн цонх</summary>
        public TellerWindow? TellerWindow { get; set; }
    }
}
