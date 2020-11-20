using System;
using System.Net.Sockets;
using System.Text;

namespace Server
{
    public class ClientObject
    {
        protected internal string Id { get; private set; }
        protected internal NetworkStream Stream { get; private set; }

        private string userName;
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

                // посылаем сообщение о входе в чат всем подключенным пользователям
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
                        switch (message.ToLower())
                        {
                            case "как дела?":
                                message = "Все хорошо!";
                                break;
                            case "чем занят?":
                                message = "В чате общаюсь!";
                                break;
                            case "пока":
                                message = "пока";
                                client.Close();
                                break;
                            default:
                                message = "Я пока не знаю что сказать... :-(";
                                break;
                        }
                        Console.WriteLine($"Бот: {message}");
                        server.BroadcastMessage(message, this.Id);
                    }
                    catch
                    {
                        message = String.Format($"{userName}: покинул чат");
                        Console.WriteLine(message);
                        server.BroadcastMessage(message, this.Id);
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
            builder.AppendLine("Список вопросов который можно задать:");
            builder.AppendLine("Как дела?");
            builder.AppendLine("чем занят?");
            builder.AppendLine("Для завершения введите: Пока");

            return builder.ToString();
        }
    }
}