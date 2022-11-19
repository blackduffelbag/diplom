using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using System.Linq;

public class TCPConnect
{
    public bool CanStartReconnect = false;
    public List<string> RecivedMessages = new List<string>();

    private const int ConnectionTimedOut = 3000;

    private TcpClient _client;
    private Thread _clientListener;
    private NetworkStream _NS;
    private bool _working = true;

    /// <summary>
    /// Выбор типа сервера и попытка подключения.
    /// </summary>
    public TCPConnect(string ip, int _port)
    {
        try
        {
            _client = new TcpClient();
            var result = _client.BeginConnect(ip, _port, null, null);
            if (result.AsyncWaitHandle.WaitOne(ConnectionTimedOut, true))
            {
                _client.EndConnect(result);

                _clientListener = new Thread(ReceivingMessagesLoop);
                _clientListener.Start();
            }
            else
            {
                CloseClient();
            }
        }
        catch
        {
            CloseClient();
        }
    }

    /// <summary>
    /// Приём TCP-потока с сервера с разделением потока на разные сообщения по байтам. 
    /// Полученные сообщения помещаются в DataHolder.MessageTCP.
    /// </summary>
    private void ReceivingMessagesLoop()
    {
        _NS = _client.GetStream();
        while (_working)
        {
            List<byte> buffer = new List<byte>();
            try
            {
                while (!Encoding.UTF8.GetString(buffer.ToArray()).Contains(";") )
                {
                    GetByteFromStream(buffer);
                    if (buffer.Count() == 0)
                        break;
                }
            }
            catch
            {
                
                break;
            }

            if (buffer.Count() != 0)
            {
                string message = Encoding.UTF8.GetString(buffer.ToArray());
                RecivedMessages.Add(message);
            }
        }
    }

    /// <summary>
    /// Проверка потока на наличие полученных байт и добавление их к _receivedBytesBuffer.
    /// </summary>
    private void GetByteFromStream(List<byte> buffer)
    {
        if (_NS.DataAvailable)
        {
            int ReadByte = _NS.ReadByte();
            if (ReadByte > -1)
            {
                buffer.Add((byte)ReadByte);
            }
        }
    }

    /// <summary>
    /// Закрытие TCP соединения.
    /// </summary>
    public void CloseClient()
    {
        _working = false;

        if (_client != null)
        {
            _client.LingerState = new LingerOption(true, 0);
            _client.Close();
            _client = null;
        }
        
        if (_NS != null)
        {
            _NS.Close();
            _NS = null;
        }
    }          

    ~TCPConnect()
    {
        CloseClient();
    }
}
