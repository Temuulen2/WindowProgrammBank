using BankSystem.Server.Services;

namespace BankSystemTest
{
    [TestClass]
    public class AuthServiceTests
    {
        /// <summary>
        /// Зөв нэр, нууц үгээр нэвтрэхэд token буцаах ёстой.
        /// </summary>
        [TestMethod]
        public void Login_ValidCredentials_ReturnsToken()
        {
            var service = new AuthService();

            var (success, token, message) = service.Login("teller1", "1234");

            Assert.IsTrue(success);
            Assert.IsNotNull(token);
            Assert.IsNull(message);
        }

        /// <summary>
        /// Буруу нууц үгээр нэвтрэхэд амжилтгүй болох ёстой.
        /// </summary>
        [TestMethod]
        public void Login_WrongPassword_Fails()
        {
            var service = new AuthService();

            var (success, token, message) = service.Login("teller1", "wrongpassword");

            Assert.IsFalse(success);
            Assert.IsNull(token);
            Assert.IsNotNull(message);
        }

        /// <summary>
        /// Байхгүй хэрэглэгчийн нэрээр нэвтрэхэд амжилтгүй болох ёстой.
        /// </summary>
        [TestMethod]
        public void Login_InvalidUsername_Fails()
        {
            var service = new AuthService();

            var (success, token, _) = service.Login("nobody", "1234");

            Assert.IsFalse(success);
            Assert.IsNull(token);
        }

        /// <summary>
        /// Нэвтэрсний дараа token-оор хэрэглэгч олдох ёстой.
        /// </summary>
        [TestMethod]
        public void GetUserByToken_ValidToken_ReturnsUser()
        {
            var service = new AuthService();
            var (_, token, _) = service.Login("teller1", "1234");

            var user = service.GetUserByToken(token!);

            Assert.IsNotNull(user);
            Assert.AreEqual("teller1", user.Username);
        }

        /// <summary>
        /// Logout хийсний дараа token хүчингүй болох ёстой.
        /// </summary>
        [TestMethod]
        public void Logout_InvalidatesToken()
        {
            var service = new AuthService();
            var (_, token, _) = service.Login("teller1", "1234");

            service.Logout(token!);

            Assert.IsNull(service.GetUserByToken(token!));
        }

        /// <summary>
        /// Буруу token-оор хэрэглэгч олдохгүй байх ёстой.
        /// </summary>
        [TestMethod]
        public void GetUserByToken_InvalidToken_ReturnsNull()
        {
            var service = new AuthService();

            var user = service.GetUserByToken("invalid-token-xyz");

            Assert.IsNull(user);
        }
    }
}
