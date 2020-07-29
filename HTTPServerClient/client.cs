using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HTTPServerClient
{

    class client
    {
        public static List<string> words = new List<string>();
        public static string[] files = Directory.GetFiles("input", "*.txt", SearchOption.AllDirectories); //получаем файлы из папки input у клиента

        const int port = 8888;
        const string address = "127.0.0.1";
        static void Main(string[] args)
        {
            //createlist(); функция которая покажет все текстовые файлы и их содержимое
            Console.Write("Введите свое имя:");
            
            string userName = Console.ReadLine();
            Console.WriteLine("");
            TcpClient client = null;
            //принимаем клиента
            while (true)
            {
                try
                {
                    client = new TcpClient(address, port);
                    NetworkStream stream = client.GetStream();

                    while (true)
                    {
                        //читаем содержимое текстовых файлов из папки
                        for (int i = 0; i < files.Length; i++)
                        {
                            using (StreamReader sr = new StreamReader(files[i], System.Text.Encoding.Default))
                            {
                                string line;
                                while ((line = sr.ReadLine()) != null)
                                {                                    
                                    Console.Write(userName + ": ");
                                    // ввод сообщения
                                    //string message = Console.ReadLine();
                                    string message = line;
                                    Console.Write("текст '" + line + "' из файла " + "'" + files[i] + "'" + "\n");                                    
                                    message = String.Format("{0}: {1}", userName, message);
                                    // преобразуем сообщение в массив байтов
                                    byte[] data = Encoding.Unicode.GetBytes(message);
                                    // отправка сообщения
                                    stream.Write(data, 0, data.Length);


                                    // получаем ответ
                                    data = new byte[64]; // буфер для получаемых данных

                                    StringBuilder builder = new StringBuilder();
                                    int bytes = 0;

                                    do
                                    {
                                        bytes = stream.Read(data, 0, data.Length);
                                        builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
                                    }
                                    while (stream.DataAvailable);

                                    message = builder.ToString();
                                    Console.WriteLine("Сервер: {0}", message);  
                                }
                            }
                           
                            
                            
                        }
                        Console.WriteLine("\nНажмите любую клавишу, чтобы отправить запросы еще раз\n");
                        Console.ReadKey();

                    }
                    
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                finally
                {
                    client.Close();
                }
            }
        }
        public static void createlist()
        {
            //показать файлы в дериктории
            Console.WriteLine("Файлы в директории");
            string[] files = Directory.GetFiles("input", "*.txt", SearchOption.AllDirectories);
            foreach (string dir in files)
            {
                Console.WriteLine(dir);
            }
           

            //чтение текстовых файлов и запись в список
            for (int i = 0; i < files.Length; i++)
            {
                using (StreamReader sr = new StreamReader(files[i], System.Text.Encoding.Default))
                {
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        //  Console.WriteLine(line);
                        words.Add(line);
                     
                    }
                }
            }
            Console.WriteLine("\nСписок на экран\n");
            foreach (string w in words)
            {
                Console.WriteLine(w);
            }
            Console.WriteLine("");
        }
    }

}
