using AllCommands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TcpGameServer
{
    
    internal class Program
    {
        const int kPORT = 35702;
        const string kIpAddress = "192.168.2.115";//"127.0.0.1"; //"192.168.2.115";
        
        const int kMaxPlayerCount = 2;
        static bool _isServerRun = true;
        static int _curClientsCount = 0;

        static EServerStages _stage = EServerStages.InitStage;
        static object _lock = new object();
        static object _lockForList = new object();
        static List<ClientMetaInfo> _clientsMeta = new List<ClientMetaInfo>();
        static BaseGame _game = new TicTacToeGame(kMaxPlayerCount);
        
        static int IncPlayerCount()
        {
            int curClintCount = -1;
            
            lock (_lock)
            {
                if (_curClientsCount < kMaxPlayerCount)
                {
                    curClintCount = _curClientsCount;
                    ++_curClientsCount;

                    if (_curClientsCount == kMaxPlayerCount)
                    {
                        Console.WriteLine("All two players added");
                    }
                    else 
                    {
                        Console.WriteLine("Need one more player");
                    }
                }
            }
            return curClintCount;
        }
        static int DecPlayerCount() 
        {
            lock (_lock)
            {
                if (_curClientsCount > 0)
                {
                    --_curClientsCount;
                    return 0;
                }
                return -1;
            }
        }
        
        static void OneClientDisconnected() 
        {   
            lock (_lockForList)
            {
                DecPlayerCount();
                _clientsMeta.RemoveAll((ClientMetaInfo cmi) => { if (cmi == null) return true; if (cmi.ClientSocket == null) return true; return !cmi.ClientSocket.Connected; });
                _stage = EServerStages.WaitingForConnection;
                foreach (var client in _clientsMeta)
                {
                    client.Number = 0;
                    client.IsConnected = true;
                    client.IsReadyForGame = false;
                    try
                    {
                        client.ClientSocket.Send(new byte[] { CMD_AllCommands.kCMD_YouAreFirstPlayer });
                    }
                    catch
                    {
                    }
                }
            }   
        }
        // thread for working with client
        static void ClientThread(object obj) 
        {
            Socket sock = obj as Socket;
            int curClientNumber = 0;
            byte[] message = new byte[1024];
            bool otherPlayerStep = true;
            if (sock != null) 
            {
                if (sock.Connected) 
                {
                    ClientMetaInfo clientMeta = null;
                    curClientNumber = IncPlayerCount();
                    Console.WriteLine("Connected client number {0}", curClientNumber);
                    try
                    {
                        if (curClientNumber < 0) 
                        {
                            throw new Exception("Increment player count error.");
                        }
                        clientMeta = new ClientMetaInfo();
                        clientMeta.Number = curClientNumber;
                        clientMeta.IsReadyForGame = false;
                        clientMeta.ClientSocket = sock;
                        clientMeta.IsConnected = false;
                        lock (_lockForList)
                        {
                            _clientsMeta.Add(clientMeta);
                        }

                        while (_isServerRun) 
                        {
                             switch (_stage) 
                            {
                                case EServerStages.InitStage:
                                // pass
                                case EServerStages.WaitingForConnection:
                                    Console.WriteLine(" WaitingForConnection for client " + clientMeta.Number);
                                    // pass
                                    if (!clientMeta.IsConnected)
                                    {
                                        sock.Send(new byte[] { (byte)(CMD_AllCommands.kCMD_YouAreFirstPlayer + clientMeta.Number) });
                                        clientMeta.IsConnected = true;
                                    }
                                    else
                                    {
                                        if (clientMeta.Number == 1)
                                            _stage = EServerStages.WaitingForReadyForGame;
                                    }
                                    break;
                                case EServerStages.WaitingForReadyForGame:
                                    if (!clientMeta.IsReadyForGame)
                                    {
                                        sock.Send(new byte[] { CMD_AllCommands.kCMD_AreYouReady }); // are you ready
                                        int receiveCount = sock.Receive(message);
                                        if (receiveCount != 0)
                                        {
                                            if (message[0] == CMD_AllCommands.kCMD_IAmReady)
                                            {
                                                lock (_lockForList)
                                                {
                                                    clientMeta.IsReadyForGame = true;
                                                    Console.WriteLine("The {0} is ready", clientMeta.Number);
                                                    if (_clientsMeta.Count((ClientMetaInfo cmi) => { return cmi.IsReadyForGame; }) == kMaxPlayerCount)
                                                    {
                                                        _stage = EServerStages.Game;
                                                        _game.ResetGame();
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    break;
                                case EServerStages.Game:
                                    if (_game.CurPlayerNumber() == clientMeta.Number) // 0, 1
                                    {
                                        otherPlayerStep = true;
                                        message[0] = CMD_AllCommands.kCMD_Field;  // field
                                        int messageLength = _game.WriteFieldInArray(message, 1);
                                        clientMeta.ClientSocket.Send(message, messageLength + 1, SocketFlags.None);
                                        clientMeta.ClientSocket.Send(new byte[] { CMD_AllCommands.kCMD_YourTurn /* 0x34 */ });
                                        clientMeta.ClientSocket.Receive(message);
                                        if (message[0] == CMD_AllCommands.kCMD_MyTurn /* 0x33*/) 
                                        {
                                            try
                                            {
                                                _game.GameStep(new byte[] { message[1], message[2] });
                                            }
                                            catch (Exception)
                                            {
                                                break;
                                            }
                                            if (_game.TheWinner() != -1) 
                                            {
                                                clientMeta.ClientSocket.Send(new byte[] { CMD_AllCommands.kCMD_TheWinnerIs, (byte)_game.TheWinner()});
                                                foreach (var cmi in _clientsMeta) 
                                                {
                                                    try
                                                    {
                                                        cmi.ClientSocket.Send(new byte[] { CMD_AllCommands.kCMD_TheWinnerIs, (byte)_game.TheWinner() });
                                                        cmi.IsReadyForGame = false;
                                                    }
                                                    finally 
                                                    { 
                                                    }
                                                }
                                                _stage = EServerStages.WaitingForReadyForGame;
                                            }
                                        }
                                    }
                                    else 
                                    {
                                        if (otherPlayerStep)
                                            clientMeta.ClientSocket.Send(new byte[] { CMD_AllCommands.kCMD_PlayerNIsGoingNow /* 0x33 */, (byte)_game.CurPlayerNumber() });
                                        otherPlayerStep = false;
                                    }
                                    break;

                            }
                            System.Threading.Thread.Sleep(70);
                        }
                    }
                    catch (Exception ex) 
                    {
                        sock.Shutdown(SocketShutdown.Both);
                        sock.Close();
                        if (clientMeta != null)
                            clientMeta.ClientSocket = null;
                        OneClientDisconnected();
                    }
                } 
            }
        }

        static void Main(string[] args)
        {
            Socket serverSocket = null;
            Socket newClientSocket = null;
            _game = new TicTacToeGame(kMaxPlayerCount);
            _stage = EServerStages.WaitingForConnection;

            try
            {

                serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                serverSocket.Bind(new IPEndPoint(IPAddress.Parse(kIpAddress), kPORT));
                serverSocket.Listen(kMaxPlayerCount);    // only two players
                Console.WriteLine("Server Ip address: {0} port: {1}", kIpAddress, kPORT);

                while (_isServerRun) 
                {
                    newClientSocket = serverSocket.Accept();
                    new System.Threading.Thread(ClientThread).Start(newClientSocket);
                    // run thread
                }
            }
            catch(Exception ex)
            {
            
            }
        }
    }
}
