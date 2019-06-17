using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace VaultApp.Service
{
    class Crypto
    {
        public static int saltSize = 20;

        public static char[] AllowedCharacters =
            {'1', '2', '3', '4', '5', '6', '7', '8', '9', '0', '-', '=',
            '!', '@', '#', '$', '%', '^', '&', '*', '(', ')', '_', '+',
            'q', 'w', 'e', 'r', 't', 'y', 'u', 'i', 'o', 'p', '[', ']',
            'a', 's', 'd', 'f', 'g', 'h', 'j', 'k', 'l', ';', ':',
            'z', 'x', 'c', 'v', 'b', 'n', 'm', ',', '.', '<', '>', '?', '/',
            'Q', 'W', 'E', 'R', 'T', 'Y', 'U', 'I', 'O', 'P', 'A', 'S', 'D',
            'F', 'G', 'H', 'J', 'K', 'L', 'Z', 'X', 'C', 'V', 'B', 'N', 'M'};

        public static char[] AllowedNumbers = { '1', '2', '3', '4', '5', '6', '7', '8', '9', '0' };
        public static char[] AllowedSymbols =
            { '-', '=', '!', '@', '#', '$', '%', '^', '&', '*', '(',
            ')', '_', '+', '[', ']', ';', ':', ',', '.', '<', '>', '?', '/'};
        public static char[] AllowedLowercaseLetters = { 'q', 'w', 'e', 'r', 't', 'y', 'u', 'i', 'o', 'p',
            'a', 's', 'd', 'f', 'g', 'h', 'j', 'k', 'l', 'z', 'x', 'c', 'v', 'b', 'n', 'm'};
        public static char[] AllowedUppercaseLetters = {'Q', 'W', 'E', 'R', 'T', 'Y', 'U', 'I', 'O', 'P', 'A', 'S', 'D',
            'F', 'G', 'H', 'J', 'K', 'L', 'Z', 'X', 'C', 'V', 'B', 'N', 'M'};

        private static string validRegex = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*(_|[^\w])).+$";

        private static Random rand = new Random();

        public static string Encrypt(string clearText, string EncryptionKey)
        {
            byte[] clearBytes = Encoding.Unicode.GetBytes(clearText);
            using (Aes encryptor = Aes.Create())
            {
                byte[] IV = new byte[15];
                rand.NextBytes(IV);
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, IV);
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(clearBytes, 0, clearBytes.Length);
                        cs.Close();
                    }
                    clearText = Convert.ToBase64String(IV) + Convert.ToBase64String(ms.ToArray());
                }
            }
            return clearText;
        }

        public static string Decrypt(string cipherText, string EncryptionKey)
        {
            byte[] IV = Convert.FromBase64String(cipherText.Substring(0, 20));
            cipherText = cipherText.Substring(20).Replace(" ", "+");
            byte[] cipherBytes = Convert.FromBase64String(cipherText);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, IV);
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(cipherBytes, 0, cipherBytes.Length);
                        cs.Close();
                    }
                    cipherText = Encoding.Unicode.GetString(ms.ToArray());
                }
            }
            return cipherText;
        }

        public static string GetRandomPassword()
        {
            string pass = string.Empty;
            for (int tries = 0; tries < 100; tries++)
            {
                char[] password = new char[16];
                double nrOfUppercaseLetters = rand.Next(1, 12);
                if (!ApplicationSettings.AllowBigLetters)
                {
                    nrOfUppercaseLetters = 0;
                }
                double nrOfLowecaseLetter = rand.Next(1, 13 - (int)nrOfUppercaseLetters);
                if (!ApplicationSettings.AllowSmallLetters)
                {
                    nrOfLowecaseLetter = 0;
                }
                double nrOfNumbers = rand.Next(1, 14 - (int)(nrOfUppercaseLetters + nrOfLowecaseLetter));
                if (!ApplicationSettings.AllowNumbers)
                {
                    nrOfNumbers = 0;
                }
                double nrOfSymbols = 16 - (nrOfUppercaseLetters + nrOfLowecaseLetter + nrOfNumbers);
                if (!ApplicationSettings.AllowSymbols)
                {
                    nrOfSymbols = 0;
                }

                double randSum = nrOfLowecaseLetter + nrOfUppercaseLetters + nrOfNumbers + nrOfSymbols;
                double multiplier = 16 / randSum;
                nrOfUppercaseLetters *= multiplier;
                nrOfLowecaseLetter *= multiplier;
                nrOfNumbers *= multiplier;
                nrOfSymbols *= multiplier;

                List<int> positions = new List<int>();
                for (int i = 0; i < 16; i++)
                {
                    positions.Add(i);
                }
                Shuffle(positions, rand);

                for (int i = 0; i < 16; i++)
                {
                    if (nrOfUppercaseLetters > 0)
                    {
                        password[positions[i]] = AllowedUppercaseLetters[rand.Next(0, AllowedUppercaseLetters.Length)];
                        nrOfUppercaseLetters--;
                    }
                    else if (nrOfLowecaseLetter > 0)
                    {
                        password[positions[i]] = AllowedLowercaseLetters[rand.Next(0, AllowedLowercaseLetters.Length)];
                        nrOfLowecaseLetter--;
                    }
                    else if (nrOfNumbers > 0)
                    {
                        password[positions[i]] = AllowedNumbers[rand.Next(0, AllowedNumbers.Length)];
                        nrOfNumbers--;
                    }
                    else if (nrOfSymbols > 0)
                    {
                        password[positions[i]] = AllowedSymbols[rand.Next(0, AllowedSymbols.Length)];
                        nrOfSymbols--;
                    }
                }
                pass = new string(password);
                if(Regex.IsMatch(pass, validRegex))
                {
                    return pass;
                }
            }
            return pass;
        }

        public static bool PasswordIsValid(string password)
        {
            return Regex.IsMatch(password, validRegex);
        }

        private static void Shuffle(List<int> list, Random rnd)
        {
            for (var i = 0; i < list.Count; i++)
                Swap(list, i, rnd.Next(i, list.Count));
        }

        private static void Swap(List<int> list, int i, int j)
        {
            var temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }

        public static string GenerateSHA512String(string inputString)
        {
            SHA512 sha512 = SHA512Managed.Create();
            byte[] bytes = Encoding.UTF8.GetBytes(inputString);
            byte[] hash = sha512.ComputeHash(bytes);
            return GetStringFromHash(hash);
        }

        private static string GetStringFromHash(byte[] hash)
        {
            StringBuilder result = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                result.Append(hash[i].ToString("X2"));
            }
            return result.ToString();
        }
    }
}
