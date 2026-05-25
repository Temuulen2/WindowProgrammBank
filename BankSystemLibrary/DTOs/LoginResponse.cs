namespace BankSystem.Shared.DTOs
{
    /// <summary>
    /// Нэвтрэх хүсэлтийн хариу — сервер клиент рүү буцаана
    /// </summary>
    public class LoginResponse
    {
        /// <summary>Нэвтрэлт амжилттай эсэх</summary>
        public bool Success { get; set; }

        /// <summary>Амжилтгүй бол алдааны мэдэгдэл, амжилттай бол null</summary>
        public string? Message { get; set; }

        /// <summary>Нэвтэрсэн теллерийн цонхны ID</summary>
        public int? TellerWindowId { get; set; }

        /// <summary>Session token — дараагийн хүсэлт бүрт дамжуулна</summary>
        public string? Token { get; set; }
    }
}
