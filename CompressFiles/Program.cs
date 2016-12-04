using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Reflection;

namespace GZipTest
{
    class Program
    {
        #region
        private static string line = "", firName = "", secName = "", template = "", operation = "";
        private static Regex regExp;
        private static bool returned;
        #endregion

        /// <summary>
        /// Главный метод
        /// </summary>
        /// <param name="args">Массив строк, передаваемый в главный метод</param>
        static int Main(string[] args)
        {
            
            char questRep = 'x';
            //line = args.ToString();
            for (int i = 0; i < args.Length; i++)
            {
                Console.WriteLine(args[i]);
                line = line + args[i] + " ";
            }
            //line = "GZipTest.exe compress [1.rtf] [1.zip]";
            //line = line.Remove(0, 13);
            line = line.Remove(line.Length - 1, 1);
            Console.WriteLine(line);
            do
            {
                HelpMessage();
                returned = true;
                PartitionLine();
                Console.WriteLine("Повторить? \n Y - да \n N- нет");
                questRep = Console.ReadKey().KeyChar;
                if (questRep == 'Y' || questRep == 'y') line = Console.ReadLine();
            } while (questRep != 'N' || questRep != 'n');
            Console.WriteLine("Нажмите любую клавишу, чтобы выйти!");
            Console.Read();
            if (returned) return 1;
            else return 2;
        }
        #region
        private static void HelpMessage()
        {
            string helpMsg = string.Format("Чтобы заархивировать используйте команду: \n CompressFiles.exe compress [Имя исходного файла] [Имя архива] \n"
                + "Чтобы разархивировать используйте команду: \n CompressFiles.exe decompress [Имя архива] [Имя распакованного файла]");
            Console.WriteLine(helpMsg);
        }

        private static void PartitionLine()
        {
            template = @"^(\b((c|dec)ompress)\b\s[[][^\[\]]{1,100}[]]\s[[][^\[\]]{1,100}[]])$"; // Шаблон для регулярного выражения
            try
            {
                regExp = new Regex(template); // Инициализируем новый экземпляр класса Regex с использованием шаблона регулярного выражения
                Console.WriteLine(line);

                if (regExp.IsMatch(line))
                {
                    Parser(line); // Извлекаем из введенной строки необходимые параметры
                    MethodInfo method = Type.GetType("GZipTest.Program").GetMethod(operation); // Ищем и запускаем нужный метод в зависимости от введенной команды
                    method.Invoke(null, null);
                }
                else throw new Exception("Строка задана неверно! \n");
            }
            catch (FileNotFoundException exept)
            {
                returned = false;
                Console.WriteLine(exept.Message);
            }
            catch (Exception exept)
            {
                returned = false;
                Console.WriteLine(exept.Message);
            }
            finally
            { }
        }
        /// <summary>
        /// Извлечение из входной строки введенных параметров
        /// </summary>
        /// <param name="inputStr">Входная строка</param>
        private static void Parser(string inputStr)
        {
            template = @"\b((c|dec)ompress)\b"; // Шаблон для регулярного выражения
            regExp = new Regex(template); // Инициализируем новый экземпляр класса Regex с использованием шаблона регулярного выражения
            operation = regExp.Match(inputStr).ToString(); // Ищем во входной строке первое вхождение указанного регулярного выражения
            inputStr = DelSubstr(inputStr, operation); // Извлекаем строку
            template = @"^([[][^\[\]]{1,100}[]])"; // Шаблон для регулярного выражения
            regExp = new Regex(template); // Инициализируем новый экземпляр класса Regex с использованием шаблона регулярного выражения
            firName = regExp.Match(inputStr).ToString(); // Извлекаем имя в первых квадратных скобках
            inputStr = DelSubstr(inputStr, firName);
            firName = firName.Substring(1, firName.Length - 2); // Удаляем квадратные скобки
            secName = regExp.Match(inputStr).ToString(); // Извлекаем имя во вторых квадратных скобках
            secName = secName.Substring(1, secName.Length - 2); // Удаляем квадратные скобки
        }
        /// <summary>
        /// Удаление подстроки из строки
        /// </summary>
        /// <param name="firstStr">Подстрока для удаления</param>
        /// <param name="secondStr">Строка, из которой удаляем</param>
        /// <returns></returns>
        public static string DelSubstr(string firstStr, string secondStr)
        {
            return firstStr.Substring(firstStr.IndexOf(secondStr) + secondStr.Length + 1);
        }
        /// <summary>
        /// Метод архивирования файла
        /// </summary>
        public static void compress()
        {
            string dir = Directory.GetCurrentDirectory(); // Запоминаем рабочий каталог приложения
            FileInfo fCompress = new FileInfo(dir + "\\" + firName); // Путь до файлпа для сжатия
            FileInfo archive = new FileInfo(dir + "\\" + secName); // Путь до архива
            try
            {
                if (fCompress.Exists) //Если файл для архивирования существует
                {
                    using (FileStream fStream = fCompress.OpenRead()) //Создаем новый поток для чтения заданного файла
                    {
                        if ((File.GetAttributes(fCompress.FullName) & FileAttributes.Hidden) != FileAttributes.Hidden & fCompress.Extension != ".zip") // Если файл не имеет расширение .zip и файл не скрыт 
                        {
                            using (FileStream compressFStream = File.Create(archive.FullName + ".zip")) // Создаем новый поток записи в файл с расширением .zip
                            {
                                using (System.IO.Compression.GZipStream compressStream = new System.IO.Compression.GZipStream(compressFStream, System.IO.Compression.CompressionMode.Compress)) // Сжимаем файл
                                {
                                    fStream.CopyTo(compressStream); // Записываем изменения в новый файл
                                }
                            }
                            FileInfo inf = new FileInfo(archive.FullName + ".zip"); // Даем файлу имя
                            Console.WriteLine("Сжатие файла {0} выполнено успешно!", fCompress.Name); // Информационное сообщение
                        }
                    }
                }
                else throw new FileNotFoundException(string.Format("Файл {0} не найден по пути {1}", firName, dir)); // Исклюение
            }
            catch
            {
                
            }
        }
        /// <summary>
        /// Метод разархивирования архива
        /// </summary>
        public static void decompress()
        {
            string dir = Directory.GetCurrentDirectory(); // Запоминаем рабочий каталог приложения
            FileInfo fDecompress = new FileInfo(dir + "\\" + firName); // Путь до файла
            FileInfo archive = new FileInfo(dir + "\\" + secName); // ПУть до архива
            try
            {
                if (fDecompress.Exists) // Если файл для разархивирования существует
                {
                    using (FileStream decompFStream = fDecompress.OpenRead()) //Создаем новый поток для чтения заданного файла
                    {
                        using (FileStream decompressFStream = File.Create(secName))
                        {
                            using (System.IO.Compression.GZipStream decompStream = new System.IO.Compression.GZipStream(decompFStream, System.IO.Compression.CompressionMode.Decompress))
                            {
                                decompStream.CopyTo(decompressFStream);
                                Console.WriteLine("Разархивирование файла {0} выполнено успешно!", fDecompress.Name);
                            }
                        }
                    }
                }
                else throw new FileNotFoundException(string.Format("Архив {0} не найден по пути {1}", firName, dir));
            }
            catch
            {
            }
        }
        #endregion
    }
}
