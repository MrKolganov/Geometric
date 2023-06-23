using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Text;


public class Server : MonoBehaviour
{
    string? username;
    int localPort;
    int remotePort;
    IPAddress localAddress;
    void Start()
    {
        localAddress = IPAddress.Parse("127.0.0.1");
        Console.Write("Введите своё имя: ");
        username = Console.ReadLine();
        Console.Write("Введите порт для приема сообщений: ");
        if(!int.TryParse(Console.ReadLine(), out localPort)) return;
        Console.Write("Введите порт для отправки сообщений: ");
        if(!int.TryParse(Console.ReadLine(), out remotePort)) return;
        Console.WriteLine();

        Task.Run(ReceiveMessageAsync);
        // ввод и отправка ссобщений
        Task.Run(SendMessageAsync);
    }

    async Task SendMessageAsync()
    {
        using Socket sender = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

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
}
