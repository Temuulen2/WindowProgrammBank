namespace BankSystem.Shared.Enums
{
    /// <summary>
    /// Дугаарын тасалбарын төлөв
    /// </summary>
    public enum TicketStatus
    {
        /// <summary>Хүлээж байна</summary>
        Waiting,
        /// <summary>Теллер дуудсан</summary>
        Called,
        /// <summary>Үйлчилгээ явагдаж байна</summary>
        Serving,
        /// <summary>Үйлчилгээ дууссан</summary>
        Done,
        /// <summary>Алгасагдсан</summary>
        Skipped
    }

    /// <summary>
    /// Мөнгөн гүйлгээний төлөв
    /// </summary>
    public enum TransactionStatus
    {
        /// <summary>Хүлээгдэж байна</summary>
        Pending,
        /// <summary>Амжилттай дууссан</summary>
        Completed,
        /// <summary>Амжилтгүй болсон</summary>
        Failed,
        /// <summary>Цуцлагдсан</summary>
        Cancelled
    }

    /// <summary>
    /// Теллерийн цонхны төлөв
    /// </summary>
    public enum WindowStatus
    {
        /// <summary>Нээлттэй, харилцагч хүлээн авч байна</summary>
        Open,
        /// <summary>Хаалттай</summary>
        Closed,
        /// <summary>Харилцагчтай ажиллаж байна</summary>
        Busy
    }
}
