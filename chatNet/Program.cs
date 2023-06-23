using System.Net;
using System.Net.Sockets;
using System.Text;

IPAddress localAddress = IPAddress.Parse("127.0.0.1");
Console.Write("Введите своё имя: ");
string? username = Console.ReadLine();
Console.Write("Введите порт для приема сообщений: ");
if(!int.TryParse(Console.ReadLine(), out var localPort)) return;
Console.Write("Введите порт для отправки сообщений: ");
if(!int.TryParse(Console.ReadLine(), out var remotePort)) return;
Console.WriteLine();

// получение сообщений
Task.Run(ReceiveMessageAsync);
// ввод и отправка ссобщений
await SendMessageAsync();

// отправка сообщений
async Task SendMessageAsync()
{
    using Socket sender = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
    Console.WriteLine("Для отправки сообщений введите сообщение и нажмите Enter");

    while(true)
    {
        var message = Console.ReadLine();
        if(string.IsNullOrWhiteSpace(message)) break;

        message = $"{username}: {message}";
        byte[] data = Encoding.UTF8.GetBytes(message);

        await sender.SendToAsync(data, new IPEndPoint(localAddress, remotePort));
    }
}

// вывод сообщений
async Task ReceiveMessageAsync()
{
    byte[] data = new byte[65535];// буфер для получаемых данных
    // сокет для прослушки сообщений
    using Socket receiver = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
    receiver.Bind(new IPEndPoint(localAddress, localPort));
    while(true)
    {
        var result = await receiver.ReceiveFromAsync(data, new IPEndPoint(IPAddress.Any, 0));
        var message = Encoding.UTF8.GetString(data, 0, result.ReceivedBytes);
        Console.WriteLine(message);
    }
}