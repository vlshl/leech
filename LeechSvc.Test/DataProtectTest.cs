using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LeechSvc.Test
{
    [TestClass]
    public class DataProtectTest
    {
        /// <summary>
        /// Тест нормальных значений
        /// </summary>
        [TestMethod]
        public void EncryptDecrypt_correctData_correctDecrypt()
        {
            string server = "server.com";
            string login = "логин";
            string password = "пароль";

            string encData = DataProtect.Encrypt(server, login, password);

            string server1; string login1; string password1;
            bool isSuccess = DataProtect.Decrypt(encData, out server1, out login1, out password1);

            Assert.IsTrue(isSuccess);
            Assert.AreEqual(server, server1);
            Assert.AreEqual(login, login1);
            Assert.AreEqual(password, password1);
        }

        /// <summary>
        /// Тест нормальных значений (шифрование в режиме LocalMachine)
        /// </summary>
        [TestMethod]
        public void EncryptDecrypt_localMachineScope_correctDecrypt()
        {
            string server = "server.com";
            string login = "login";
            string password = "password";

            string encData = DataProtect.Encrypt(server, login, password, true);

            string server1; string login1; string password1;
            bool isSuccess = DataProtect.Decrypt(encData, out server1, out login1, out password1);

            Assert.IsTrue(isSuccess);
            Assert.AreEqual(server, server1);
            Assert.AreEqual(login, login1);
            Assert.AreEqual(password, password1);
        }

        /// <summary>
        /// Пустые строки
        /// </summary>
        [TestMethod]
        public void EncryptDecrypt_empty_empty()
        {
            string server; string login; string password;
            string encData = DataProtect.Encrypt("", "", "");
            bool isSuccess = DataProtect.Decrypt(encData, out server, out login, out password);

            Assert.IsTrue(isSuccess);
            Assert.AreEqual("", server);
            Assert.AreEqual("", login);
            Assert.AreEqual("", password);
        }

        /// <summary>
        /// null-аргументы
        /// </summary>
        [TestMethod]
        public void Encrypt_null_argNullExp()
        {
            Assert.ThrowsException<ArgumentNullException>(() => 
            {
                DataProtect.Encrypt(null, null, null);
            });
        }

        /// <summary>
        /// null-аргументы
        /// </summary>
        [TestMethod]
        public void Decrypt_null_argNullExp()
        {
            Assert.ThrowsException<ArgumentNullException>(() =>
            {
                string server;
                string login;
                string password;
                DataProtect.Decrypt(null, out server, out login, out password); // null на входе
            });
        }

        /// <summary>
        /// Неверные аргументы
        /// </summary>
        [TestMethod]
        public void Decrypt_incorrectData_false()
        {
            string server;
            string login;
            string password;
            bool isSuccess;

            isSuccess = DataProtect.Decrypt("", out server, out login, out password); // пустая строка на входе
            Assert.AreEqual(false, isSuccess);
            Assert.AreEqual("", server);
            Assert.AreEqual("", login);
            Assert.AreEqual("", password);

            isSuccess = DataProtect.Decrypt("привет", out server, out login, out password); // не base64 на входе
            Assert.AreEqual(false, isSuccess);
            Assert.AreEqual("", server);
            Assert.AreEqual("", login);
            Assert.AreEqual("", password);
        }
    }
}
