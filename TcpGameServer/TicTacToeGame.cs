using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TcpGameServer
{
    class TicTacToeGame : BaseGame
    {
        public enum ETTT_Items : byte
        {
            Empty = 13,
            Tic = 0,
            Tac = 1
        }
        private byte[][] _field;

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
                    if (_field[i][j] == (byte)(ETTT_Items.Empty) || (_field[i][j] != _field[i - 1][j]))
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
                    if (_field[i][i] == (byte)(ETTT_Items.Empty) || (_field[i][i] != _field[i - 1][i - 1]))
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

}
