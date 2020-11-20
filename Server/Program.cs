using System;
using System.Threading;

/*
Написать чат-сервер и чат-клиент. Сервер может давать ответы на простые предустановленные вопросы, например “Как дела?” - ”хорошо”.
Сценарий:
1. при присоединении сервер спрашивает имя клиента.
2. выводим перечень возможных вопросов.
3. по команде “пока” завершаем чат.
4. команда для получения списка всех присоединенных пользователей в чате.

Пишем обе части (общение по TCP):
Реализация сервера - C#, console, dotNet core
Реализация клиента - C#, console / gui
*/

namespace Server
{
    class Program
    {
        static ServerObject server; // сервер
        static Thread listenThread; // потока для прослушивания
        static void Main(string[] args)
        {
            try
            {
                server = new ServerObject();
                listenThread = new Thread(new ThreadStart(server.Listen));
                listenThread.Start(); //старт потока
            }
            catch (Exception ex)
            {
                server.Disconnect();
                Console.WriteLine(ex.Message);
            }
        }
    }
}
