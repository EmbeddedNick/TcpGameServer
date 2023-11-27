using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TcpGameServer
{
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
        protected abstract bool CheckWinner();
        protected abstract void Action(object obj);
        protected abstract void InitField();
        protected abstract void ResetField();
        public abstract int WriteFieldInArray(byte[] arr, int offset);

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
            Console.WriteLine("The game is over the winner is {0}", playerNumber);
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
}
