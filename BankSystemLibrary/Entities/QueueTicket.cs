using BankSystem.Shared.Enums;

namespace BankSystem.Shared.Entities
{
    /// <summary>
    /// Банкны үүдний терминалаас харилцагчид олгогдох дугаарын тасалбар
    /// </summary>
    public class QueueTicket
    {
        /// <summary>Өгөгдлийн сангийн өвөрмөц дугаар</summary>
        public int Id { get; set; }

        /// <summary>Харилцагчид олгогдсон дараалалын дугаар</summary>
        public int TicketNumber { get; set; }

        /// <summary>Тасалбар олгогдсон огноо цаг</summary>
        public DateTime IssuedAt { get; set; }

        /// <summary>Теллер дуудсан огноо цаг (дуудаагүй бол null)</summary>
        public DateTime? CalledAt { get; set; }

        /// <summary>Үйлчилгээ эхэлсэн огноо цаг (эхлээгүй бол null)</summary>
        public DateTime? ServedAt { get; set; }

        /// <summary>Үйлчилгээ дууссан огноо цаг (дуусаагүй бол null)</summary>
        public DateTime? CompletedAt { get; set; }

        /// <summary>Тасалбарын одоогийн төлөв</summary>
        public TicketStatus Status { get; set; }

        /// <summary>Үйлчилж буй теллерийн цонхны ID (хуваарилаагүй бол null)</summary>
        public int? TellerWindowId { get; set; }
    }
}
