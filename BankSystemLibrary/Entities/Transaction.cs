using BankSystem.Shared.Enums;

namespace BankSystem.Shared.Entities
{
    /// <summary>
    /// А данснаас Б данс руу хийсэн мөнгөн гүйлгээний бүртгэл
    /// </summary>
    public class Transaction
    {
        /// <summary>Өгөгдлийн сангийн өвөрмөц дугаар</summary>
        public int Id { get; set; }

        /// <summary>Мөнгө гарах дансны дугаар</summary>
        public string FromAccountNumber { get; set; } = string.Empty;

        /// <summary>Мөнгө орох дансны дугаар</summary>
        public string ToAccountNumber { get; set; } = string.Empty;

        /// <summary>Шилжүүлсэн дүн</summary>
        public decimal Amount { get; set; }

        /// <summary>Гүйлгээний валют (үндсэн утга: MNT)</summary>
        public string Currency { get; set; } = "MNT";

        /// <summary>Гүйлгээний төлөв</summary>
        public TransactionStatus Status { get; set; }

        /// <summary>Гүйлгээ үүссэн огноо цаг</summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>Гүйлгээ дууссан огноо цаг (дуусаагүй бол null)</summary>
        public DateTime? CompletedAt { get; set; }

        /// <summary>Гүйлгээ хийсэн теллерийн цонхны ID</summary>
        public int TellerWindowId { get; set; }

        /// <summary>Гүйлгээний нэмэлт тайлбар (заавал биш)</summary>
        public string? Note { get; set; }
    }
}
