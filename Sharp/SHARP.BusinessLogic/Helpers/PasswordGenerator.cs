using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace SHARP.BusinessLogic.Helpers
{


    public class PasswordGenerator
    {
        public static string GeneratePassword(string baseWord, int length = 10)
        {
            if (baseWord.Length >= length)
                throw new ArgumentException("Password length must be greater than the base word length.");

            const string Symbols = "!@#$%&*()-_=+[]{}|;:<>?";
            const string Digits = "0123456789";

            var random = new Random();
            var password = new StringBuilder(baseWord);

            int remainingLength = length - baseWord.Length;

            // Randomly decide if it ends with two numbers or mix of symbol and number
            bool endsWithTwoNumbers = random.Next(2) == 0;

            if (endsWithTwoNumbers && remainingLength >= 2)
            {
                // Add two numbers at the end
                password.Append(Digits[random.Next(Digits.Length)]);
                password.Append(Digits[random.Next(Digits.Length)]);
                remainingLength -= 2;
            }

            // Add remaining random symbols and numbers
            string allChars = Symbols + Digits;
            while (password.Length < length)
            {
                password.Append(allChars[random.Next(allChars.Length)]);
            }

            return password.ToString();
        }
    }

}
