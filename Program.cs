using LowLevelDesign.Hexify;
using System;
using System.IO;
using System.Net.Sockets;

class HelloClient
{
    static void Main(string[] args)
    {
        if (args.Length != 3 || !int.TryParse(args[1], out var port) || !File.Exists(args[2]))
        {
            Console.WriteLine("Usage hashclient <server> <port> <filepath>");
            return;
        }

        using var input = File.OpenRead(args[2]);

        using var client = new TcpClient(args[0], port);
        using var stream = client.GetStream();

        var lenBytes = BitConverter.GetBytes(input.Length);
        if (BitConverter.IsLittleEndian) { Array.Reverse(lenBytes); }
        stream.Write(lenBytes);

        input.CopyTo(stream);

        var data = new byte[16 + 20 + 32]; // MD5 + SHA1 + SHA256
        var offset = 0;

        while (offset < data.Length)
        {
            var nread = stream.Read(data, offset, data.Length - offset);
            if (nread == 0)
            {
                Console.WriteLine("Error: invalid response from the server");
                return;
            }
            offset += nread;
            if (offset == data.Length)
            {
                Console.WriteLine($"MD5   : 0x{Hex.ToHexString(data, 0, 16)}");
                Console.WriteLine($"SHA1  : 0x{Hex.ToHexString(data, 16, 20)}");
                Console.WriteLine($"SHA256: 0x{Hex.ToHexString(data, 36, 32)}");
            }
        }
    }
}

