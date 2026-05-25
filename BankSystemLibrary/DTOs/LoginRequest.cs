namespace BankSystem.Shared.DTOs
{
    /// <summary>
    /// Теллер нэвтрэх хүсэлт — клиентээс сервер рүү явуулна
    /// </summary>
    public class LoginRequest
    {
        /// <summary>Нэвтрэх нэр</summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>Нууц үг (plain text — сервер hash хийж шалгана)</summary>
        public string Password { get; set; } = string.Empty;

        /// <summary>Нэвтрэх гэж буй теллерийн цонхны ID</summary>
        public int TellerWindowId { get; set; }
    }
}
