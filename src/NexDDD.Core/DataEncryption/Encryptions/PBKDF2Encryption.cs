using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace NexDDD.Core.DataEncryption.Encryptions
{
    /// <summary>
    /// 负责数据加密模块中的 PBKDF2Encryption 能力，并限定其与上层调用者和基础设施组件的协作边界。
    /// </summary>
    public class PBKDF2Encryption
    {
        private const string SaltHashSeparator = ":";

        /// <summary>
        /// PBKDF2 加密
        /// </summary>
        /// <param name="text">加密文本</param>
        /// <param name="saltSize">随机 salt 大小</param>
        /// <param name="iterationCount">迭代次数</param>
        /// <param name="derivedKeyLength">密钥长度</param>
        /// <returns></returns>
        public static string Encrypt(string text, int saltSize = 16, int iterationCount = 10000, int derivedKeyLength = 32)
        {
            using var rng = RandomNumberGenerator.Create();
            var salt = new byte[saltSize];
            rng.GetBytes(salt);

            using var pbkdf2 = new Rfc2898DeriveBytes(text, salt, iterationCount, HashAlgorithmName.SHA256);
            var hash = pbkdf2.GetBytes(derivedKeyLength);


            // 分别编码盐和哈希，并用分隔符拼接
            return Convert.ToBase64String(salt) + SaltHashSeparator + Convert.ToBase64String(hash);
        }

        /// <summary>
        /// PBKDF2 比较
        /// </summary>
        /// <param name="text">加密文本</param>
        /// <param name="hash">PBKDF2 字符串</param>
        /// <param name="saltSize">随机 salt 大小</param>
        /// <param name="iterationCount">迭代次数</param>
        /// <param name="derivedKeyLength">密钥长度</param>
        /// <returns></returns>
        public static bool Compare(string text, string hash, int saltSize = 16, int iterationCount = 10000, int derivedKeyLength = 32)
        {
            try
            {
                var parts = hash.Split(new[] { SaltHashSeparator }, StringSplitOptions.None);
                if (parts.Length != 2)
                    return false;

                var saltBytes = Convert.FromBase64String(parts[0]);
                var storedHashBytes = Convert.FromBase64String(parts[1]);

                if (saltBytes.Length != saltSize || storedHashBytes.Length != derivedKeyLength)
                    return false;

                using var pbkdf2 = new Rfc2898DeriveBytes(text, saltBytes, iterationCount, HashAlgorithmName.SHA256);
                var computedHash = pbkdf2.GetBytes(derivedKeyLength);

                return computedHash.SequenceEqual(storedHashBytes);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 适用于密码 加密,增加了 版本标识
        /// </summary>
        /// <param name="text"></param>
        /// <param name="saltSize"></param>
        /// <param name="iterationCount"></param>
        /// <param name="derivedKeyLength"></param>
        /// <returns></returns>
        public static string HashPasswordV3(string password, int saltSize = 16, int iterationCount = 10000, int derivedKeyLength = 32)
        {
            using var rng = RandomNumberGenerator.Create();

           return Convert.ToBase64String(HashPasswordV3(password, rng,
                prf: KeyDerivationPrf.HMACSHA512,
                iterCount: iterationCount,
                saltSize: saltSize,
                numBytesRequested: derivedKeyLength));
        }

        /// <summary>
        /// PBKDF2 比较
        /// </summary>
        /// <param name="hashedPassword"></param>
        /// <param name="providedPassword"></param>
        /// <param name="saltSize"></param>
        /// <param name="iterationCount"></param>
        /// <param name="derivedKeyLength"></param>
        /// <returns></returns>
        public static bool VerifyHashedPassword(string hashedPassword, string providedPassword, int saltSize = 16, int iterationCount = 10000, int derivedKeyLength = 32)
        {
            byte[] decodedHashedPassword = Convert.FromBase64String(hashedPassword);

            if (decodedHashedPassword.Length == 0)
            {
                return false;
            }

            if (VerifyHashedPasswordV3(decodedHashedPassword, providedPassword, out int embeddedIterCount, out KeyDerivationPrf prf))
            {
                // 当前配置的迭代次数更高时，调用方应在验证后更新密码散列。
                if (embeddedIterCount < iterationCount)
                {
                    return true;
                }

                // V3 要求使用 SHA512；旧 PRF 为 SHA1 或 SHA256 时，调用方应升级并重新散列。
                if (prf == KeyDerivationPrf.HMACSHA1 || prf == KeyDerivationPrf.HMACSHA256)
                {
                    return true;
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        private static byte[] HashPasswordV3(string password, RandomNumberGenerator rng, KeyDerivationPrf prf, int iterCount, int saltSize, int numBytesRequested)
        {
            // 生成版本 3 格式的文本散列。
            byte[] salt = new byte[saltSize];
            rng.GetBytes(salt);
            byte[] subkey = KeyDerivation.Pbkdf2(password, salt, prf, iterCount, numBytesRequested);

            var outputBytes = new byte[13 + salt.Length + subkey.Length];
            outputBytes[0] = 0x01; // 格式标记。
            WriteNetworkByteOrder(outputBytes, 1, (uint)prf);
            WriteNetworkByteOrder(outputBytes, 5, (uint)iterCount);
            WriteNetworkByteOrder(outputBytes, 9, (uint)saltSize);
            Buffer.BlockCopy(salt, 0, outputBytes, 13, salt.Length);
            Buffer.BlockCopy(subkey, 0, outputBytes, 13 + saltSize, subkey.Length);
            return outputBytes;
        }

        private static bool VerifyHashedPasswordV3(byte[] hashedPassword, string password, out int iterCount, out KeyDerivationPrf prf)
        {
            iterCount = default(int);
            prf = default(KeyDerivationPrf);

            try
            {
                // 读取散列头部信息。
                prf = (KeyDerivationPrf)ReadNetworkByteOrder(hashedPassword, 1);
                iterCount = (int)ReadNetworkByteOrder(hashedPassword, 5);
                int saltLength = (int)ReadNetworkByteOrder(hashedPassword, 9);

                // 读取盐值，长度必须至少为 128 位。
                if (saltLength < 128 / 8)
                {
                    return false;
                }
                byte[] salt = new byte[saltLength];
                Buffer.BlockCopy(hashedPassword, 13, salt, 0, salt.Length);

                // 读取剩余载荷中的子密钥，长度必须至少为 128 位。
                int subkeyLength = hashedPassword.Length - 13 - salt.Length;
                if (subkeyLength < 128 / 8)
                {
                    return false;
                }
                byte[] expectedSubkey = new byte[subkeyLength];
                Buffer.BlockCopy(hashedPassword, 13 + salt.Length, expectedSubkey, 0, expectedSubkey.Length);

                // 对输入密码进行散列并以固定时间方式验证。
                byte[] actualSubkey = KeyDerivation.Pbkdf2(password, salt, prf, iterCount, subkeyLength);

                return CryptographicOperations.FixedTimeEquals(actualSubkey, expectedSubkey);
            }
            catch
            {
                // 仅格式损坏的载荷可能触发越界等异常，此时统一视为验证失败。
                return false;
            }
        }


        private static uint ReadNetworkByteOrder(byte[] buffer, int offset)
        {
            return ((uint)(buffer[offset + 0]) << 24)
                | ((uint)(buffer[offset + 1]) << 16)
                | ((uint)(buffer[offset + 2]) << 8)
                | ((uint)(buffer[offset + 3]));
        }

        private static void WriteNetworkByteOrder(byte[] buffer, int offset, uint value)
        {
            buffer[offset + 0] = (byte)(value >> 24);
            buffer[offset + 1] = (byte)(value >> 16);
            buffer[offset + 2] = (byte)(value >> 8);
            buffer[offset + 3] = (byte)(value >> 0);
        }
    }
}
