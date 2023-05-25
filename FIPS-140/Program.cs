using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace FIPS_140
{
    internal class Program
    {
        /// <summary>
        /// Генерує випадкову послідовність бітів в байтовому представленні
        /// </summary>
        /// <returns>Масив згенерованих бітів</returns>
        static byte[] GenerateRandomRow()
        {
            // в кожному байті буде зберігатися 0 або 1
            Random random = new Random();
            byte[] row = new byte[20000];
            int tabulation = 0;
            while(tabulation < 20000)
            {
                row[tabulation] = Convert.ToByte(random.Next(0, 2));
                tabulation++;
            }
            return row;
        }
        /// <summary>
        /// Метод, який реалізовує монобітний тест
        /// </summary>
        /// <param name="row">послідовність, яку треба перевірити</param>
        /// <returns>Логічне значення, яке свідчить про успішність проходження тесту</returns>
        static bool MonobitTest(byte[] row)
        {
            bool condition; 
            int amount = 0;

            //рахує кількість одиниць
            for (int i = 0; i < row.Length; i++)
            {
                if (row[i] == 1)
                {
                    amount++;
                }
            }

            //перевіряє, чи кількість одиниць входить в заданий проміжок
            if (amount >= 9654 && amount <= 10346) condition = true;
            else condition = false;

            return condition;
        }
        /// <summary>
        /// Метод, який реалізовує тест максимальної довжини серії
        /// </summary>
        /// <param name="row">послідовність яку треба перевірити</param>
        /// <returns>Логічне значення, яке свідчить про успішність проходження тесту</returns>
        static bool MaximumLengthOfSeriesTest(byte[] row)
        {
            bool condition;
            
            // змінні для обчислення максимальної довжини серії
            int seriesLength = 1, maxSeriesLength = 1;

            // реалізація тесту
            for (int i = 0; i < row.Length - 1; i++)
            {
                if (row[i] == row[i + 1])
                {
                    seriesLength++;
                }
                else if (seriesLength > maxSeriesLength)
                {
                    maxSeriesLength = seriesLength;
                    seriesLength = 1;
                }
                else maxSeriesLength = 1;
            }

            //перевірка умови
            if (maxSeriesLength >= 36) condition = false;
            else condition = true;

            return condition;
        }
        /// <summary>
        /// Метод, який реалізовує тест Поккера
        /// </summary>
        /// <param name="row">послідовність, яку треба перевірити</param>
        /// <returns>Логічне значення, яке свідчить про успішність проходження тесту</returns>
        static bool PokkerTest(byte[] row)
        {

            bool condition;

            int m = 4; // довжина блоків Поккера

            // масив для розбиття послідовності на блоки Поккера
            byte[][] dividedRow = new byte[row.Length / m][];

            // процес розбиття на блоки Поккера
            for (int i = 0; i < row.Length / m; i++)
            {
                ArraySegment<byte> segmentation = new ArraySegment<byte>(row, i, m);
                dividedRow[i] = segmentation.ToArray<byte>();
            }

            double sum = 0; // сума квадратів зустрічів i-их блоків Поккера
            int n; // кількісь зустрічання i-го блоку Поккера в послідовності
            byte possibleSequence;

            // обчислення суми
            for (possibleSequence = 0; possibleSequence < Math.Pow(2, m); possibleSequence++)
            {
                n = 0;
                for (int i = 0; i < dividedRow.GetLength(0); i++)
                {
                    if (Convert.ToString(possibleSequence, 2).PadLeft(4, '0') == String.Concat(dividedRow[i]))
                    {
                        n++;
                    }
                }
                sum += Math.Pow(n, 2);
            }

            int k = dividedRow.GetLength(0); // кількість блоків Поккера в послідовності

            double X_3 = ((Math.Pow(2, m) / k) * sum) - k;

            // перевірка умови
            if (X_3 >= 1.03 && X_3 <= 57.4) condition = true;
            else condition = false;

            return condition;
        }
        /// <summary>
        /// Метод, який реалізує тест довжин серій
        /// </summary>
        /// <param name="row">Послідовність, яку потрібно перевірити</param>
        /// <returns>Логічне значення, яке свідчить про успішність проходження тесту</returns>
        static bool SeriesLengthsTest(byte[] row)
        {
            int[] oneSeriesLengths = new int[6]; // Масив для збереження довжин серій одиниць
            int[] zeroSeriesLengths = new int[6]; // Масив для збереження довжин серій нулів

            int currentOneSeriesLength = 0; // Змінна для відстежування поточної довжини серії одиниць
            int currentZeroSeriesLength = 0; // Змінна для відстежування поточної довжини серії нулів

            for (int i = 0; i < row.Length; i++)
            {
                if (row[i] == 1)
                {
                    int checkNextNumber = i;
                    currentZeroSeriesLength = 0; // Скидаємо довжину серії нулів
                    do
                    {
                        currentOneSeriesLength++; // Збільшуємо довжину серії одиниць
                        checkNextNumber++;
                        if (checkNextNumber == row.Length) break;
                    } while (row[checkNextNumber] != 0);
                    i = checkNextNumber - 1;
                }
                else if (row[i] == 0)
                {
                    int checkNextNumber = i;
                    currentOneSeriesLength = 0; // Скидаємо довжину серії одиниць
                    do
                    {
                        currentZeroSeriesLength++; // Збільшуємо довжину серії нулів
                        checkNextNumber++;
                        if (checkNextNumber == row.Length) break;
                    } while (row[checkNextNumber] != 1);
                    i = checkNextNumber - 1;
                }
                

                if (currentOneSeriesLength > 6)
                {
                    // Якщо довжина серії одиниць перевищує 6, збільшуємо лічильник для індексу 5
                    oneSeriesLengths[5]++; 
                }
                else if (currentOneSeriesLength != 0)
                {
                    // В іншому випадку збільшуємо лічильник для поточної довжини серії одиниць
                    oneSeriesLengths[currentOneSeriesLength - 1]++;
                }

                if (currentZeroSeriesLength > 6)
                {
                    // Якщо довжина серії нулів перевищує 6, збільшуємо лічильник для індексу 5
                    zeroSeriesLengths[5]++;
                }
                else if (currentZeroSeriesLength != 0)
                {
                    // В іншому випадку збільшуємо лічильник для поточної довжини серії нулів
                    zeroSeriesLengths[currentZeroSeriesLength - 1]++;
                }
            }

            // Перевіряємо, чи довжини серій потрапляють у відповідні інтервали

            int[] expectedMinLengths = { 2267, 1079, 502, 223, 90, 90 }; // Мінімальні очікувані довжини серій
            int[] expectedMaxLengths = { 2733, 1421, 748, 402, 223, 223 }; // Максимальні очікувані довжини серій

            for (int i = 0; i < 6; i++)
            {
                if ((zeroSeriesLengths[i] < expectedMinLengths[i] || zeroSeriesLengths[i] > expectedMaxLengths[i])
                    || (oneSeriesLengths[i] < expectedMinLengths[i] || oneSeriesLengths[i] > expectedMaxLengths[i]))
                {
                    return false; // Якщо довжина серії не потрапляє у відповідний інтервал, повертаємо false
                }
            }

            return true; // Якщо всі довжини серій потрапляють у відповідні інтервали, повертаємо true

        }

        static void Main()
        {

            byte[] bitsRow = GenerateRandomRow(); // генерування випадкової послідовності

            // перевірка по стандарту FIPS-140
            bool monobitTest = MonobitTest(bitsRow);
            Console.WriteLine("Monobit test: {0}", monobitTest);
            bool maximumLengthOfSeriesTest = MaximumLengthOfSeriesTest(bitsRow);
            Console.WriteLine("Maximum Length Of Series Test: {0}", maximumLengthOfSeriesTest);
            bool pokkerTest = PokkerTest(bitsRow);
            Console.WriteLine("Pokker test: {0}", pokkerTest);
            bool seriesLengthTest = SeriesLengthsTest(bitsRow);
            Console.WriteLine("Series Lengths test: {0}", seriesLengthTest);

            // звітування про послідовность на достатню випадковість
            Console.WriteLine();
            if (monobitTest && maximumLengthOfSeriesTest)
            {
                Console.WriteLine("The row has passed the test!");
            }
            else Console.WriteLine("The row has NOT passed the test!");


        }
    }
}
