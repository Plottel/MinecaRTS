using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinecaRTS
{
    public static class TimeSlicedPathManager
    {
        public const int SEARCH_CYCLES_PER_TICK = 1;
        private static List<Pathfinder> _activeSearches = new List<Pathfinder>();

        public static void AddSearch(Pathfinder newSearch)
        {
            _activeSearches.Add(newSearch);
        }

        public static void Update()
        {
            for (int i = 0; i < SEARCH_CYCLES_PER_TICK; ++i)
            {
                for (int j = _activeSearches.Count - 1; j >= 0; --j)
                {
                    SearchState searchState = _activeSearches[j].SingleIterationGreedy();

                    if (searchState != SearchState.Searching)
                    {
                        MsgBoard.AddMessage(null, _activeSearches[j].handler.ID, MessageType.SearchComplete, info:searchState);
                        _activeSearches.RemoveAt(j);
                    }                        
                }
            }            
        }
    }
}
