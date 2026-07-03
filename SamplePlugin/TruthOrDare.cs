using Dalamud.Game.ClientState.Party;
using Dalamud.Plugin.Services;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace SamplePlugin
{
    
    internal class TruthOrDare(IPartyList partyList)
    {
        private readonly IPartyList partyList = partyList;
        public string? lastWinner { get; private set; }
        public string? lastLoser { get; private set; }

        private DateTime lastPlayed = DateTime.MinValue;
        public int CurrentRound { get; private set; } = 1;

        public List<(string Winner, string Loser, int CurrentRound)> history { get; private set; } = [];

        private const int PlayTimeout = 30; // 30 seconds timeout

        public Result<(string Winner, string Loser)> Play()
        {
            if (partyList.Length < 3)
            {
                return Result<(string, string)>.Fail(" Not enough party members to play Truth or Dare. You need at least 3. ");
            }

            if (lastPlayed.AddSeconds(PlayTimeout) > DateTime.Now)
            {
                return Result<(string, string)>.Fail("  You can only play Truth or Dare once every 30 seconds.  ");
            }

            // Logic for selecting a winner and loser
            var playerPool = partyList.Select(p => p.Name.TextValue).ToList();
            var possibleWinners = playerPool.Where(name => name != lastWinner).ToList();

            // Pick a random winner
            var currentWinner = possibleWinners[Random.Shared.Next(possibleWinners.Count)];

            //remove winner from playerpool
            playerPool.Remove(currentWinner);

            var possibleLosers = playerPool.Where(name => name != lastLoser).ToList();

            // Pick a random loser
            var currentLoser = possibleLosers[Random.Shared.Next(possibleLosers.Count)];

            // Saves the current winner and loser for the next round
            lastLoser = currentLoser;
            lastWinner = currentWinner;

            // Update the last played time and increment the round
            lastPlayed = DateTime.Now;
            CurrentRound += 1;

            // Updates History
            history.Add((currentWinner, currentLoser, CurrentRound));

            return Result<(string, string)>.Ok((currentWinner, currentLoser));
        }

        public List<(string Winner, string Loser, int CurrentRound)> GetLast(int amount = 1)
        {
            return history.TakeLast(amount).ToList();
        }

        public void Reset()
        {
            lastWinner = null;
            lastLoser = null;
            CurrentRound = 1;
            history.Clear();
        }
    }
}
