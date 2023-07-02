using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HexGame
{
    internal class HexEventSource : EventSource
    {
        public static HexEventSource Log = new HexEventSource();
        [Event(1)]
        public void GameStart(string args) => WriteEvent(1, args);

        [Event(2)]
        public void GameStop(string whoWon) => WriteEvent(2, whoWon);

        [Event(3, Keywords = Keywords.MonteCarloSimulation, Level = EventLevel.Verbose)]
        public void SuccessRatioForMove(int row, int column, double ratio) => WriteEvent(3, row, column, ratio);
        [Event(4, Level = EventLevel.Informational)]
        public void TimeForMove(double time) => WriteEvent(4, time);
        public static class Keywords
        {
            public const EventKeywords MonteCarloSimulation = (EventKeywords)(1 << 1);
        }
    }

}
