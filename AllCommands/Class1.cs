using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AllCommands
{
    public  class CMD_AllCommands
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
        public const byte kCMD_HaveYouBeenDrawnField =0x32;
        public const byte kCMD_IHaveBeenDrawnField = 0x31;
        public const byte kCMD_IHaventBeenDrawnField = 0x32;
        public const byte kCMD_PlayerNIsGoingNow = 0x33;
        public const byte kCMD_YourTurn = 0x34;
        public const byte kCMD_MyTurn = 0x33;
        
        // fourth stage
        public const byte kCMD_TheWinnerIs=0x41;
    }
}
