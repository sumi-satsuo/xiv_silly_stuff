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

        public (string Winner, string Loser)? Play()
        {
            if (partyList.Length < 3)
            {
                return null;
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


            return (Winner: currentWinner, Loser: currentLoser);
        }
    }
}
