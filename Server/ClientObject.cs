using System;
using System.Net.Sockets;
using System.Text;

namespace Server
{
    public class ClientObject
    {
        protected internal string Id { get; private set; }
        protected internal NetworkStream Stream { get; private set; }
        protected internal string userName;
        private readonly TcpClient client;
        private readonly ServerObject server;

        public ClientObject(TcpClient tcpClient, ServerObject serverObject)
        {
            Id = Guid.NewGuid().ToString();
            client = tcpClient;
            server = serverObject;
            serverObject.AddConnection(this);
        }

        public void Process()
        {
            try
            {
                Stream = client.GetStream();

                // получаем имя пользователя
                string message = "Здравствуйте! Как вас зовут?";

                server.BroadcastMessage(message, this.Id);
                Console.WriteLine($"Бот: {message}");

                message = GetMessage();
                userName = message;
                Console.WriteLine(message);
                // в бесконечном цикле получаем сообщения от клиента
                while (true)
                {
                    try
                    {
                        message = GetListQuestion();
                        Console.WriteLine($"Бот: {message}");
                        server.BroadcastMessage(message, this.Id);
                        message = GetMessage();
                        Console.WriteLine($"{userName}: {message}");
                        message = GetAnswer(message);
                        Console.WriteLine($"Бот: {message}");
                        server.BroadcastMessage(message, this.Id);
                        if (message == "пока")
                        {
                            server.RemoveConnection(this.Id);
                            Close();
                        }
                    }
                    catch
                    {
                        message = String.Format($"{userName}: покинул чат");
                        Console.WriteLine(message);
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                // в случае выхода из цикла закрываем ресурсы
                server.RemoveConnection(this.Id);
                Close();
            }
        }

        // чтение входящего сообщения и преобразование в строку
        private string GetMessage()
        {
            byte[] data = new byte[64]; // буфер для получаемых данных
            StringBuilder builder = new StringBuilder();
            int bytes = 0;
            do
            {
                bytes = Stream.Read(data, 0, data.Length);
                builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
            }
            while (Stream.DataAvailable);

            return builder.ToString();
        }

        // закрытие подключения
        protected internal void Close()
        {
            if (Stream != null)
                Stream.Close();
            if (client != null)
                client.Close();
        }

        private string GetListQuestion()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine();
            builder.AppendLine("Список вопросов который можно задать:");
            builder.AppendLine("Как дела?");
            builder.AppendLine("чем занят?");
            builder.AppendLine("Для завершения введите: Пока");

            return builder.ToString();
        }

        private string GetAnswer(string message)
        {
            string answer = null;

            switch (message.ToLower())
            {
                case "как дела?":
                    answer = "Все хорошо!";
                    break;
                case "чем занят?":
                    answer = "В чате общаюсь!";
                    break;
                case "пока":
                    answer = "пока";
                    break;
                case "all":
                    answer = server.GetNameUser();
                    break;
                default:
                    answer = "Я пока не знаю что сказать... :-(";
                    break;
            }

            return answer;
        }
    }
}