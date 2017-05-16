using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinecaRTS
{
    public static class TimeSlicedPathManager
    {
        public const int SEARCH_CYCLES_PER_TICK = 100;
        private static List<Pathfinder> _activeSearches = new List<Pathfinder>();

        public static void AddSearch(Pathfinder newSearch)
        {
            _activeSearches.Add(newSearch);
        }

        public static void Update()
        {
            int numCycles = 0;
            int i = 0;

            while (_activeSearches.Count > 0 && numCycles < SEARCH_CYCLES_PER_TICK)
            {
                SearchState searchState = _activeSearches[i].SingleIteration();

                if (searchState == SearchState.Complete || searchState == SearchState.Failed)
                {
                    MsgBoard.AddMessage(null, _activeSearches[i].handler.ID, MessageType.SearchComplete, info: searchState);
                    _activeSearches.RemoveAt(i);
                }
                else
                {
                    ++i;
                }

                ++numCycles;

                if (i >= _activeSearches.Count)
                    i = 0;
            }          
        }
    }
}
