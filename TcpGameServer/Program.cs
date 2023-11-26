using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TcpGameServer
{
    /// <summary>
    /// Класс, описывающий состояние клиента
    /// </summary>
    class ClientMetaInfo
    {
        public enum EClientState
        {
            WaitingForSecondPlayer,

        };
        public bool IsReadyForGame { get; set; } = false;
        public bool IsConnected { get; set; } = false;
        public int Number { get; set; } = 0;
        public Socket ClientSocket { get; set; } = null;

    }
    abstract class BaseGame
    {
        protected readonly int kMaxPlayerCount;
        protected int _curPlayerCount;
        protected bool _isGameRunning = false;
        private int _theWinner = -1;
        public BaseGame(int kMaxPlayerCount) 
        {
            this.kMaxPlayerCount = kMaxPlayerCount;
            _curPlayerCount = 0;
            _isGameRunning = true;
            _theWinner = -1;
        }
        public int TheWinner() 
        {
            return _theWinner;
        }
        public int CurPlayerNumber() 
        {
            return _curPlayerCount; 
        }
        protected void TheGameOwnedBy(int playerNumber) 
        {
            Console.WriteLine("The game is over the winner is {0}",playerNumber);
            _isGameRunning = false;
            _theWinner = playerNumber;
        }
        public void GameStep(object obj) 
        {
            if (_isGameRunning)
            {
                Action(obj);
                if (!CheckWinner())            
                    CurPlayerStep();
            }
        }
        protected abstract bool CheckWinner();
        protected abstract void Action(object obj);
        protected abstract void InitField();
        protected abstract void ResetField();
        public abstract int WriteFieldInArray(byte[] arr, int offset);
        private void ResetPlayers() 
        {
            _curPlayerCount = 0;
        }
        public void ResetGame()
        {
            _isGameRunning = true;
            _theWinner = -1;
            ResetPlayers();
            ResetField();
        }
        protected void InitGame()
        {
            ResetPlayers();
            InitField();
        }
        public int CurPlayerStep()
        {
            ++_curPlayerCount;
            if (_curPlayerCount == kMaxPlayerCount)
                _curPlayerCount = 0;

            return _curPlayerCount;
        }
    }
    class TicTacToeGame : BaseGame
    {
        public enum ETTT_Items : byte
        { 
            Empty = 13,
            Tic = 0, 
            Tac = 1
        }
        private byte [][] _field;
        
        public TicTacToeGame(int kMaxPlayerCount) : base(kMaxPlayerCount)
        {
            InitGame();
            ResetGame();
        }

        protected override void InitField()
        {
            _field = new byte[3][];
            for (int i = 0; i < 3; ++i)
            {
                _field[i] = new byte[3];
            }
        }
        
        protected override void Action(object obj)
        {
            try
            {
                byte[] data = obj as byte[];
                int row = data[0], col = data[1];

                if (_field[row][col] == (byte)ETTT_Items.Empty)
                {
                    _field[row][col] = (byte)_curPlayerCount;
                }
                else 
                {
                    throw new Exception();
                }
            }
            catch (Exception ex) 
            {
                throw ex;
            }
        }

        protected override void ResetField()
        {
            for (int i = 0; i < 3; ++i)
            {
                for (int j = 0; j < 3; ++j)
                {
                    _field[i][j] = (byte)ETTT_Items.Empty;
                }
            }
        }

        protected override bool CheckWinner()
        {
            bool winner = true;
            // check rows
            for (int i = 0; i < _field.Length; ++i) 
            {
                winner = true;
                for (int j = 1; j < _field[i].Length && winner; ++j) 
                {
                    if (_field[i][j] == (byte)(ETTT_Items.Empty) || (_field[i][j] != _field[i][j - 1]))
                        winner = false;
                }
                if (winner)
                {
                    TheGameOwnedBy(_field[i][0]);
                    return true;
                }
            }
            // check cols
            for (int j = 0; j < _field.Length; ++j)
            {
                winner = true;
                for (int i = 1; i < _field.Length && winner; ++j)
                {
                    if (_field[i][j] == (byte)(ETTT_Items.Empty) || (_field[i][j] != _field[i-1][j]))
                        winner = false;
                }
                if (winner)
                {
                    TheGameOwnedBy(_field[0][j]);
                    return true;
                }
            }

            // check crosslines
            if (_field.Length == _field[0].Length) 
            {
                winner = true;
                for (int i = 1; i < _field.Length; ++i) 
                {
                    if (_field[i][i] == (byte)(ETTT_Items.Empty) || (_field[i][i] != _field[i - 1][i-1]))
                        winner = false;
                }
                if (winner)
                {
                    TheGameOwnedBy(_field[0][0]);
                    return true;
                }

                winner = true;
                for (int i = 1; i < _field.Length; ++i)
                {
                    if (_field[i][_field.Length - i - 1] == (byte)(ETTT_Items.Empty) || (_field[i][i] != _field[i - 1][_field.Length - i - 1]))
                        winner = false;
                }
                if (winner)
                {
                    TheGameOwnedBy(_field[0][_field.Length - 1]);
                    return true;
                }
            }

            for (int i = 0; i < _field.Length; ++i) 
            {
                for (int j = 0; j < _field.Length; ++j) 
                {
                    if (_field[i][j] == (byte)(ETTT_Items.Empty))
                        return false;
                }
            }

            TheGameOwnedBy(kMaxPlayerCount); // friends is winner;
            return true;
        }

        public override int WriteFieldInArray(byte[] arr, int offset)
        {
            int k = 0;
            for (int i = 0; i < _field.Length; ++i) 
            {
                for (int j = 0; j < _field[i].Length; ++j) 
                {
                    arr[offset + k] = _field[i][j];
                    ++k;
                }
            }
            return k;
        }
    }
    internal class Program
    {
        const int kPORT = 35702;
        const string kIpAddress = "127.0.0.1";//"192.168.2.115";
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
                Console.WriteLine("DecPlayerCount");
                if (_curClientsCount > 0)
                {
                    --_curClientsCount;
                    return 0;
                }
                return -1;
            }
        }
        private enum EServerStages 
        {
            InitStage,  
            WaitingForConnection,   
            WaitingForReadyForGame,
            Game,
            GameOver
        }
        static void OneClientDisconnected() 
        {   
            lock (_lockForList)
            {
                DecPlayerCount();
                Console.WriteLine("Count before removeAll " + _clientsMeta.Count);
                _clientsMeta.RemoveAll((ClientMetaInfo cmi) => { if (cmi == null) return true; if (cmi.ClientSocket == null) return true; return !cmi.ClientSocket.Connected; });
                Console.WriteLine("Count after removeAll " + _clientsMeta.Count);
                _stage = EServerStages.WaitingForConnection;
                foreach (var client in _clientsMeta)
                {
                    client.Number = 0;
                    client.IsConnected = true;
                    client.IsReadyForGame = false;
                    try
                    {
                        client.ClientSocket.Send(new byte[] { 0x11 });
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
                                        Console.WriteLine("Send");
                                        sock.Send(new byte[] { (byte)(0x11 + clientMeta.Number) });
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
                                        sock.Send(new byte[] { 0x21 }); // are you ready
                                        int receiveCount = sock.Receive(message);
                                        if (receiveCount != 0)
                                        {
                                            if (message[0] == 0x21)
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
                                        message[0] = 0x31;  // field
                                        int messageLength = _game.WriteFieldInArray(message, 1);
                                        clientMeta.ClientSocket.Send(message, messageLength + 1, SocketFlags.None);
                                        clientMeta.ClientSocket.Send(new byte[] { 0x34 });
                                        clientMeta.ClientSocket.Receive(message);
                                        if (message[0] == 0x33) 
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
                                                clientMeta.ClientSocket.Send(new byte[] { 0x41, (byte)_game.TheWinner()});
                                                foreach (var cmi in _clientsMeta) 
                                                {
                                                    try
                                                    {
                                                        cmi.ClientSocket.Send(new byte[] { 0x41, (byte)_game.TheWinner() });
                                                        cmi.IsReadyForGame = false;
                                                    }
                                                    finally { }
                                                }
                                                _stage = EServerStages.WaitingForReadyForGame;
                                            }
                                        }
                                    }
                                    else 
                                    {
                                        if (otherPlayerStep)
                                            clientMeta.ClientSocket.Send(new byte[] { 0x33, (byte)_game.CurPlayerNumber() });
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

        static void ThreadServerStateMachine() 
        {
            while (_isServerRun) 
            {
                switch (_stage)
                {
                    case EServerStages.InitStage:
                    case EServerStages.WaitingForConnection:
                        lock (_lockForList)
                        {
                            if (_clientsMeta.Count() == kMaxPlayerCount)
                            {
                                _stage = EServerStages.WaitingForReadyForGame;
                            }
                        }
                        break;
                    case EServerStages.WaitingForReadyForGame:
                        // send request for be ready for game
                        // receive answer 
                        lock (_lockForList) 
                        {
                            if (_clientsMeta.Count(
                                (ClientMetaInfo cl) =>
                                {
                                    if (cl == null)
                                        return false;
                                    return cl.IsReadyForGame;
                                }
                            ) == kMaxPlayerCount)
                            {
                                _stage = EServerStages.Game;
                                _game.ResetGame();
                            }
                        }
                        break;
                    case EServerStages.Game:
                        lock (_lockForList)
                        {
                            if (_clientsMeta.Count(
                                (ClientMetaInfo cl) =>
                                {
                                    if (cl == null)
                                        return false;
                                    return cl.IsReadyForGame;
                                }
                                ) != kMaxPlayerCount)
                            {
                                _stage = EServerStages.WaitingForConnection;
                                OneClientDisconnected();
                            }
                        }
                        break;

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
