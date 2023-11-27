using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TcpGameServer
{
    enum EServerStages
    {
        InitStage,
        WaitingForConnection,
        WaitingForReadyForGame,
        Game,
        GameOver
    }
}
