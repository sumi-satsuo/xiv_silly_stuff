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

        private static readonly List<string> DebugPlayers =
            [
                "d_Sumi",
                "d_Kana",
                "d_Kitten"
            ];

        public List<(string Winner, string Loser, int CurrentRound)> History { get; private set; } = [];

        public Result<TruthOrDareResult> Play(Configuration configuration)
        {
            List<string> playerPool;

            if (configuration.debugMode)
            {
                playerPool = [.. DebugPlayers];
            }
            else
            {
                if (partyList.Length < 3)
                {
                    return Result<TruthOrDareResult>.Fail(
                        " Not enough party members to play Truth or Dare. You need at least 3. ");
                }

                if (lastPlayed.AddSeconds(configuration.CooldownSeconds) > DateTime.Now)
                {
                    return Result<TruthOrDareResult>.Fail(
                        $"You can only play every {configuration.CooldownSeconds} seconds.");
                }

                playerPool = partyList
                    .Select(p => p.Name.TextValue)
                    .ToList();
            }

            // Logic for selecting a winner and loser
            //var playerPool = partyList.Select(p => p.Name.TextValue).ToList();
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
            History.Add((currentWinner, currentLoser, CurrentRound));

            var roll = Random.Shared.Next(1, 21);

            var effect = roll switch
            {
                20 => SpecialEffect.EveryoneAnswers,
                19 => SpecialEffect.DoubleQuestion,
                18 => SpecialEffect.DareOnly,
                2 => SpecialEffect.TruthOnly,
                1 => SpecialEffect.ReverseRoles,
                _ => SpecialEffect.None
            };

            return Result<TruthOrDareResult>.Ok(
                new TruthOrDareResult
                {
                    Winner = currentWinner,
                    Loser = currentLoser,
                    Round = CurrentRound,
                    D20Roll = roll,
                    Effect = effect
                });
        }

        public List<(string Winner, string Loser, int CurrentRound)> GetLast(int amount = 1)
        {
            return History.TakeLast(amount).ToList();
        }

        public void Reset()
        {
            lastWinner = null;
            lastLoser = null;
            CurrentRound = 1;
            History.Clear();
        }
    }
}
