using BL;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace LeechSvc
{
    /// <summary>
    /// Защита данных соединения с сервером брокера
    /// </summary>
    public class DataProtect
    {
        private const byte ENTROPY_SIZE = 32;
        private static string BROKER_CONNECT_KEY = "connect";
        private static string PULXER_CONNECT_KEY = "pulxer";
        private static byte[] _salt = {
            0x84, 0x9a, 0x63, 0xc4, 0x19, 0x51, 0x49, 0x59,
            0x9e, 0x0d, 0xe3, 0xf4, 0x36, 0x1f, 0xab, 0xc0,
            0xfe, 0x7a, 0x9c, 0xb6, 0xd1, 0xd8, 0xe5, 0x69,
            0x1c, 0xfb, 0x8e, 0xad, 0x41, 0x93, 0xf4, 0xd6 };
        private IRepositoryBL _reposBL = null;

        public DataProtect(IRepositoryBL reposBL)
        {
            _reposBL = reposBL;
        }

        /// <summary>
        /// Зашифровка данных подключения к серверу
        /// </summary>
        /// <param name="server">Адрес сервера</param>
        /// <param name="login">Логин</param>
        /// <param name="password">Пароль</param>
        /// <param name="isLocalMachineProtection">Уровень защиты (на уровне компьютера или на уровне пользователя)</param>
        /// <returns>Результат в base64</returns>
        public static string Encrypt(string server, string login, string password, bool isLocalMachineProtection = false)
        {
            if (server == null || login == null || password == null)
                throw new ArgumentNullException();

            byte[] entropy = new byte[ENTROPY_SIZE];
            new Random().NextBytes(entropy);

            byte[] fullEntropy = new byte[_salt.Length + ENTROPY_SIZE];
            Array.Copy(entropy, 0, fullEntropy, 0, ENTROPY_SIZE);
            Array.Copy(_salt, 0, fullEntropy, ENTROPY_SIZE, _salt.Length);

            string dataStr = server + "\n" + login + "\n" + password;
            byte[] data = Encoding.UTF8.GetBytes(dataStr);
            byte[] encryptedData = ProtectedData.Protect(data, fullEntropy, 
                isLocalMachineProtection ? DataProtectionScope.LocalMachine : DataProtectionScope.CurrentUser);

            List<byte> resList = new List<byte>();
            resList.Add((byte)(isLocalMachineProtection ? 0x00 : 0xff));
            resList.AddRange(entropy);
            resList.AddRange(encryptedData);

            return Convert.ToBase64String(resList.ToArray());
        }

        /// <summary>
        /// Расшифровка данных подключения к серверу
        /// </summary>
        /// <param name="data">Зашифрованные данные в base64</param>
        /// <param name="server">Адрес сервера</param>
        /// <param name="login">Логин</param>
        /// <param name="password">Пароль</param>
        /// <returns>true - успешно, false - ошибка</returns>
        public static bool Decrypt(string data, out string server, out string login, out string password)
        {
            if (data == null)
                throw new ArgumentNullException();

            server = login = password = "";

            try
            {
                byte[] bytes = Convert.FromBase64String(data);
                if (bytes == null || bytes.Length < ENTROPY_SIZE + 2) return false; // первый байт + энтропия + еще хотя бы один байт

                bool isLocalMachineProtection = bytes[0] == 0x00;
                byte[] fullEntropy = new byte[_salt.Length + ENTROPY_SIZE];
                Array.Copy(bytes, 1, fullEntropy, 0, ENTROPY_SIZE);
                Array.Copy(_salt, 0, fullEntropy, ENTROPY_SIZE, _salt.Length);

                byte[] encData = new byte[bytes.Length - 1 - ENTROPY_SIZE]; // первый байт и энтропию убираем, остается хотя бы еще один байт
                Array.Copy(bytes, ENTROPY_SIZE + 1, encData, 0, encData.Length);

                byte[] decData = ProtectedData.Unprotect(encData, fullEntropy, 
                    isLocalMachineProtection ? DataProtectionScope.LocalMachine : DataProtectionScope.CurrentUser);
                if (decData == null || decData.Length == 0)
                    return false;

                string dataStr = Encoding.UTF8.GetString(decData);
                if (dataStr == null || dataStr.Length == 0)
                    return false;

                string[] parts = dataStr.Split('\n');
                if (parts == null || parts.Length < 3)
                    return false;

                server = parts[0];
                login = parts[1];
                password = parts[2];
            }
            catch(Exception ex)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Получить параметры соединения с сервером брокера
        /// </summary>
        /// <param name="server">Адрес сервера</param>
        /// <param name="login">Логин</param>
        /// <param name="password">Пароль</param>
        /// <returns>true - успешно, false - данные получить не удалось</returns>
        public bool GetBrokerParams(out string server, out string login, out string password)
        {
            server = login = password = "";
            string data = _reposBL.GetStrParam(BROKER_CONNECT_KEY);
            if (string.IsNullOrEmpty(data)) return false;

            return DataProtect.Decrypt(data, out server, out login, out password);
        }

        /// <summary>
        /// Получить параметры соединения с сервером pulxer
        /// </summary>
        /// <param name="server">Url сервера</param>
        /// <param name="login">Логин</param>
        /// <param name="password">Пароль</param>
        /// <returns>true - успешно, false - данные получить не удалось</returns>
        public bool GetPulxerParams(out string server, out string login, out string password)
        {
            server = login = password = "";
            string data = _reposBL.GetStrParam(PULXER_CONNECT_KEY);
            if (string.IsNullOrEmpty(data)) return false;

            return DataProtect.Decrypt(data, out server, out login, out password);
        }

        /// <summary>
        /// Сохранить параметры соединения с сервером брокера
        /// </summary>
        /// <param name="server">Адрес сервера</param>
        /// <param name="login">Логин</param>
        /// <param name="password">Пароль</param>
        /// <param name="isLocalMachineProtection">Уровень защиты (на уровне компьютера или на уровне пользователя)</param>
        public void SetBrokerParams(string server, string login, string password, bool isLocalMachineProtection = false)
        {
            string data = DataProtect.Encrypt(server, login, password, isLocalMachineProtection);
            _reposBL.SetStrParam(BROKER_CONNECT_KEY, data);
        }

        /// <summary>
        /// Сохранить параметры соединения с сервером Pulxer
        /// </summary>
        /// <param name="server">Url сервера</param>
        /// <param name="login">Логин</param>
        /// <param name="password">Пароль</param>
        /// <param name="isLocalMachineProtection">Уровень защиты (на уровне компьютера или на уровне пользователя)</param>
        public void SetPulxerParams(string server, string login, string password, bool isLocalMachineProtection = false)
        {
            string data = DataProtect.Encrypt(server, login, password, isLocalMachineProtection);
            _reposBL.SetStrParam(PULXER_CONNECT_KEY, data);
        }
    }
}
