﻿using System;
using System.Net.Sockets;

public class ClientTCP
{
	private static TcpClient clientSocket;
	private static NetworkStream myStream;
	private static byte[] recieveBuffer;

	public static void InitializeClientSocket(string address, int port)
	{
		clientSocket = new TcpClient();
		clientSocket.ReceiveBufferSize = 4096;
		clientSocket.SendBufferSize = 4096;
		recieveBuffer = new byte[4096 * 2];
		clientSocket.BeginConnect(address, port, new AsyncCallback(ClientConnectCallback), clientSocket);
	}

	private static void ClientConnectCallback(IAsyncResult result)
	{
		clientSocket.EndConnect(result);
		if (clientSocket.Connected == false)
		{
			return;
		}
		else
		{
			myStream = clientSocket.GetStream();
			myStream.BeginRead(recieveBuffer, 0, 4096 * 2, RecieveCallback, null);
		}
	}

	private static void RecieveCallback(IAsyncResult result)
	{
		try
		{
			int readBytes = myStream.EndRead(result);

			if (readBytes <= 0)
			{
				return;
			}

			byte[] newBytes = new byte[readBytes];
			Buffer.BlockCopy(recieveBuffer, 0, newBytes, 0, readBytes);

			ClientHandleData.HandleData(newBytes);

			myStream.BeginRead(recieveBuffer, 0, 4096 * 2, RecieveCallback, null);
		}
		catch (Exception)
		{
			throw;
		}
	}

	public static void SendData(byte[] data)
	{
		ByteBuffer buffer = new ByteBuffer();

		buffer.WriteInteger((data.GetUpperBound(0) - data.GetLowerBound(0)) + 1);
		buffer.WriteBytes(data);
		myStream.Write(buffer.ToArray(), 0, buffer.ToArray().Length);
		buffer.Dispose();
	}

	public static void PACKET_ThankYou()
	{
		ByteBuffer buffer = new ByteBuffer();
		buffer.WriteInteger((int)ClientPackages.CThankYouMsg);
		buffer.WriteString("Thank You");
		SendData(buffer.ToArray());
	}
}
