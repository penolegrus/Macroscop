using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Server
{
    
    public class ClientObject
    {       
        public TcpClient client;
        //создаем нового клиента
        public ClientObject(TcpClient tcpClient)
        {
            client = tcpClient;
        }
        //функция для проверки строки на палиндром, игнорируя пробелы и знаки препинания
        public static bool Palindrom(string s)
        {
            int i = 0, j = s.Length - 1;
            while (i < j)
            {
                if (char.IsWhiteSpace(s[i]) || char.IsPunctuation(s[i]))
                    i++;
                else if (char.IsWhiteSpace(s[j]) || char.IsPunctuation(s[j]))
                    j--;
                else if (char.ToLowerInvariant(s[i++]) != char.ToLowerInvariant(s[j--]))
                    return false;
            }
            return true;
        }
        public static int _counter;
        public static int zapros;

        //асинхронная функция которая каждую секунду обнуляет счетчик запросов
        public static async Task reset()
        {
            while (true)
            {
                await Task.Delay(TimeSpan.FromSeconds(1));
                _counter = 0;
            }
        }
        public static async Task mai()
        {
            await Task.Run(() => reset());
            for (int i = 0; i < 100; i++)
            {
                await Task.Delay(TimeSpan.FromSeconds(.1));

                Console.WriteLine(_counter++);
            }
        }


      
        public void Process()
        {
           
             mai();
            NetworkStream stream = null;            
            try
            {
                stream = client.GetStream();
                byte[] data = new byte[1024]; // буфер для получаемых данных
                while (true)
                {
                    
                    _counter++; //счетчик запросов
                    if(_counter>zapros)
                    {
                        Console.WriteLine("Превышено количество обрабатываемых запросов: {0} запросов, допустимо {1} запросов", _counter,zapros);
                       
                        Thread.Sleep(2000);
                        // Console.ReadKey();
                        
                    }
                    // получаем сообщение
                    StringBuilder builder = new StringBuilder();
                    int bytes = 0;
                    do
                    {
                        bytes = stream.Read(data, 0, data.Length);
                        builder.Append(Encoding.Unicode.GetString(data, 0, bytes)); //превратить байты в текст
                    }
                    while (stream.DataAvailable);
                    
                    string message = builder.ToString();
                    //так как сообщение приходит в формате имя: текст, берем только текстовую часть
                    string pattern = @"(\s.+$)";                   
                    Regex regex = new Regex(pattern);
                    MatchCollection match = regex.Matches(message);
                    string done = Regex.Replace(match[0].Value, @"^.", ""); 

                    Console.WriteLine(message);
                    //проверям текстовую часть на палиндром
                    if (Palindrom(done) == true)
                    {
                        message = "сообщение " + "'" + done + "'" + " является палиндром";
                    }
                    else
                    {
                        message = "сообщение " + "'" + done + "'" + " не является палиндром";
                    }
                    //отправляем ответ клиенту
                    Thread.Sleep(server.time);
                    data = Encoding.Unicode.GetBytes(message);
                    stream.Write(data, 0, data.Length);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Клиент отключился");
                //Console.WriteLine(ex.Message);
            }
            finally
            {
                if (stream != null)
                    stream.Close();
                if (client != null)
                    client.Close();
            }
        }  
    }

    class server
    {
        const int port = 8888;
        static TcpListener listener;
        public static int time;

        static void Main(string[] args)
        {

            Console.Write("Введите макс. количество одновременных обрабатываемых запросов: ");
            ClientObject.zapros = Convert.ToInt32(Console.ReadLine());
            Console.Write("Введите время обработки запроса в милисекундах(1 сек = 1000 мсек): ");            
            time = Convert.ToInt32(Console.ReadLine());
            try
            {
                listener = new TcpListener(IPAddress.Parse("127.0.0.1"), port);
                listener.Start();
                Console.WriteLine("Ожидание подключений...");

                while (true)
                {
                    //ждем пока подключится клиент
                    TcpClient client = listener.AcceptTcpClient();
                    ClientObject clientObject = new ClientObject(client);

                    // создаем новый поток для обслуживания нового клиента
                    Thread clientThread = new Thread(new ThreadStart(clientObject.Process));
                    clientThread.Start();
                }
            }
            catch (Exception ex)
            {
                
                Console.WriteLine(ex.Message);
            }
            finally
            {
                if (listener != null)
                    listener.Stop();
            }
        }
        
    }
}
