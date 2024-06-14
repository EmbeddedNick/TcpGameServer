using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TheTCPGaneClient
{
    public class CMD_AllCommands
    {
        // first stage
        public const byte kCMD_YouAreFirstPlayer = 0x11;
        public const byte kCMD_YouAreSecondPlayer = 0x12;

        // second stage
        public const byte kCMD_AreYouReady = 0x21;
        public const byte kCMD_IAmReady = 0x21;
        public const byte kCMD_IAmNotReady = 0x22;

        // third stage
        public const byte kCMD_Field = 0x31;
        public const byte kCMD_HaveYouBeenDrawnField = 0x32;
        public const byte kCMD_IHaveBeenDrawnField = 0x31;
        public const byte kCMD_IHaventBeenDrawnField = 0x32;
        public const byte kCMD_PlayerNIsGoingNow = 0x33;
        public const byte kCMD_YourTurn = 0x34;
        public const byte kCMD_MyTurn = 0x33;

        // fourth stage
        public const byte kCMD_TheWinnerIs = 0x41;
    }
    enum EServerStages
    {
        InitStage,
        WaitingForConnection,
        WaitingForReadyForGame,
        Game,
        GameOver
    }
    public enum EPlayerType : byte
    {
        TIC = 0,
        TAC = 1,
        FREE_FRIENDSHIP = 13
    }
    class Program
    {
        static int GetCommand(byte b) 
        {
            return b & 0x0f;
        }
        static int GetStage(byte b)
        {
            switch (b & 0xf0) 
            {
                case 0:
                    return 0;
                case 0x10:
                    return 1;
                case 0x20:
                    return 2;
                case 0x30:
                    return 3;
                case 0x40:
                    return 4;
                default:
                    return 0xf0;
            }
        }
        static void DrawField(byte[][] field) 
        {
            Console.WriteLine();
            for (int i = 0; i < field.Length; ++i)
            {
                for (int j = 0; j < field[i].Length; ++j)
                {
                    switch (field[i][j]) 
                    {
                        case (byte)EPlayerType.FREE_FRIENDSHIP:
                            Console.Write("- ");
                            break;
                        case (byte)EPlayerType.TIC:
                            Console.Write("x ");
                            break;
                        case (byte)EPlayerType.TAC:
                            Console.Write("o ");
                            break;
                    }
                }
                Console.WriteLine();
                Console.WriteLine();
            }
        }
        static void CopyField(byte[] message, int offset, int count,byte[][] field) 
        {
            int row = 0, col = 0;
            for (int i = 0; i < count; ++i) 
            {
                field[row][col] = message[i + offset];
                ++col;
                if (col == field[row].Length)
                {
                    row++;
                    col = 0; 
                }
            }
        }
        static void Main(string[] args)
        {
            Socket sock;
            sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            sock.Connect("127.0.0.1", 35702);
            byte[] message = new byte[1024];
            int playerNumber = -1;
            byte[][] field = new byte[3][];
            bool bFieldDraw = false;
            int messageLength = 0;

            for (int i = 0; i < field.Length; ++i)
                field[i] = new byte[3];

            for (int i = 0; i < field.Length; ++i) 
            {
                for (int j = 0; j < field[i].Length; ++j) 
                {
                    field[i][j] = (byte)EPlayerType.FREE_FRIENDSHIP;
                }
            }

            if (sock.Connected) 
            {
                while (true)
                {
                    messageLength = sock.Receive(message);

                    switch (GetStage(message[0]))
                    {
                        case (byte)EServerStages.InitStage:
                            break;
                        case (byte)EServerStages.WaitingForConnection: // first stage
                            switch (GetCommand(message[0]))
                            {
                                case 1:
                                    playerNumber = 0;
                                    break;
                                case 2:
                                    playerNumber = 1;
                                    break;
                                default:
                                    playerNumber = -1;
                                    break;
                            }
                            if (playerNumber != -1)
                            {
                                if (playerNumber == 0)
                                    Console.WriteLine("You are the first player (x) and we are waiting for second player");
                                if (playerNumber == 1)
                                    Console.WriteLine("You are the second player (o) and we are ready for game");
                            }
                            break;
                        case (byte)EServerStages.WaitingForReadyForGame: // second stage
                            switch (GetCommand(message[0]))
                            {
                                case 1:
                                    Console.WriteLine("Are you ready? 1 - yes, 0 - no");

                                    if (int.Parse(Console.ReadLine()) == 1)
                                    {
                                        sock.Send(new byte[] { CMD_AllCommands.kCMD_IAmReady /*0x21*/ });
                                    }
                                    else
                                    {
                                        sock.Send(new byte[] { CMD_AllCommands.kCMD_IAmNotReady /*0x22*/ });
                                    }
                                    break;
                                default:
                                    break;
                            }
                            break;
                        case (byte)EServerStages.Game: // third state
                            switch (GetCommand(message[0]))
                            {
                                case 1: // field
                                    CopyField(message, 1, messageLength - 1, field);
                                    bFieldDraw = false;
                                    DrawField(field);
                                    bFieldDraw = true;
                                    break;
                                case 2:
                                    if (bFieldDraw)
                                    {
                                        sock.Send(new byte[] { CMD_AllCommands.kCMD_IHaveBeenDrawnField /*0x31*/ });
                                    }
                                    else
                                    {
                                        sock.Send(new byte[] { CMD_AllCommands.kCMD_IHaventBeenDrawnField /*0x32*/ });
                                    }
                                    break;
                                case 3:
                                    Console.WriteLine("Now player №{0} is going", (int)message[1]);
                                    break;
                                case 4:
                                    Console.WriteLine("Now is your turn:");
                                    int row = -1;
                                    int col = -1;
                                    do
                                    {
                                        try
                                        {
                                            row = -1;
                                            col = -1;
                                            Console.WriteLine("row - ?");

                                            row = int.Parse(Console.ReadLine());
                                            Console.WriteLine("col - ?");
                                            col = int.Parse(Console.ReadLine());
                                        }
                                        catch (Exception)
                                        {

                                        }
                                    } while (row == -1 || col == -1);
                                    sock.Send(new byte[] { CMD_AllCommands.kCMD_MyTurn, (byte)row, (byte)col });
                                    break;
                            }
                            break;

                        case (byte)EServerStages.GameOver:
                            switch (GetCommand(message[0]))
                            {
                                case 1:
                                    if (message[1] == (byte)EPlayerType.FREE_FRIENDSHIP)
                                        Console.WriteLine("The winner is friends");
                                    else if (message[1] == playerNumber)
                                        Console.WriteLine("You win!");
                                    else
                                        Console.WriteLine("You lose!");
                                    break;
                            }

                            break;
                    }
                }
            }
        }
    }
}
