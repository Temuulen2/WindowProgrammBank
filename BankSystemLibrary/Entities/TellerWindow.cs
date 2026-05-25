using BankSystem.Shared.Enums;

namespace BankSystem.Shared.Entities
{
    /// <summary>
    /// Теллерийн ажлын цонх — тодорхой теллер нэвтэрч ажиллаж байгаа цонхыг илэрхийлнэ
    /// </summary>
    public class TellerWindow
    {
        /// <summary>Өгөгдлийн сангийн өвөрмөц дугаар</summary>
        public int Id { get; set; }

        /// <summary>Цонхны нэр (жишээ: Цонх 1)</summary>
        public string WindowName { get; set; } = string.Empty;

        /// <summary>Цонхны одоогийн төлөв</summary>
        public WindowStatus Status { get; set; }

        /// <summary>Одоо үйлчилж буй дугаарын тасалбарын дугаар (байхгүй бол null)</summary>
        public int? CurrentTicketNumber { get; set; }

        /// <summary>Одоо үйлчилж буй дугаарын тасалбарын ID (байхгүй бол null)</summary>
        public int? CurrentTicketId { get; set; }

        /// <summary>Энэ цонхонд нэвтэрсэн теллер хэрэглэгчийн ID (нэвтрээгүй бол null)</summary>
        public int? UserId { get; set; }

        /// <summary>Нэвтэрсэн теллер хэрэглэгч</summary>
        public User? User { get; set; }
    }
}
