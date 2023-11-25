using System.Text;
using System.Security.Cryptography;

namespace CSharpServer
{
    public class SHA
    {
        public static string ComputeSHA256Hash(string input)
        {
            // Преобразуем входную строку в массив байт
            byte[] inputBytes = Encoding.UTF8.GetBytes(input);

            // Создаем объект SHA-256
            using (SHA256 sha256 = SHA256.Create())
            {
                // Вычисляем хэш и возвращаем его в виде строки
                byte[] hashBytes = sha256.ComputeHash(inputBytes);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        }

        public static float HashToNumeric(string hash)
        {
            // Берем первые 8 символов хэша
            string substring = hash.Substring(0, 8);

            // Преобразуем их в десятичное число
            long decimalValue = long.Parse(substring, System.Globalization.NumberStyles.HexNumber);

            // Нормализуем в интервале [0, 1]
            float normalizedValue = (float)decimalValue / long.MaxValue;

            return normalizedValue;
        }
    }
}
