using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TheTCPGaneClient
{
    class Program
    {
        static void ClientThread(object obj) 
        {
        
        }
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
            for (int i = 0; i < field.Length; ++i)
            {
                for (int j = 0; j < field[i].Length; ++j)
                {
                    switch (field[i][j]) 
                    {
                        case 13:
                            Console.Write("- ");
                            break;
                        case 0:
                            Console.Write("x ");
                            break;
                        case 1:
                            Console.Write("o ");
                            break;
                    }
                }
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
                    field[i][j] = 13;
                }
            }

            if (sock.Connected) 
            {
                while (true)
                {
                    messageLength = sock.Receive(message);

                    switch (GetStage(message[0]))
                    {
                        case 0:
                            break;
                        case 1: // first stage
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
                        case 2: // second stage
                            switch (GetCommand(message[0]))
                            {
                                case 1:
                                    Console.WriteLine("Are you ready? 1 - yes, 0 - no");

                                    if (int.Parse(Console.ReadLine()) == 1)
                                    {
                                        sock.Send(new byte[] { 0x21 });
                                    }
                                    else
                                    {
                                        sock.Send(new byte[] { 0x22 });
                                    }
                                    break;
                                default:
                                    break;
                            }
                            break;
                        case 3:
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
                                        sock.Send(new byte[] { 0x31 });
                                    }
                                    else
                                    {
                                        sock.Send(new byte[] { 0x32 });
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
                                    sock.Send(new byte[] { 0x33, (byte)row, (byte)col });
                                    break;
                            }
                            break;

                        case 4:
                            switch (GetCommand(message[0]))
                            {
                                case 1:
                                    if (message[1] == 13)
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
