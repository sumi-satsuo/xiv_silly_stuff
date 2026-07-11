using System;
using System.Collections.Generic;
using System.Text;

namespace SamplePlugin
{
    public enum SpecialEffect
    {
        None,
        DoubleQuestion,
        DareOnly,
        TruthOnly,
        ReverseRoles,
        EveryoneAnswers,
    }

    public class TruthOrDareResult
    {
        public string Winner { get; init; } = "";
        public string Loser { get; init; } = "";

        public int Round { get; init; }

        public int D20Roll { get; init; }

        public SpecialEffect Effect { get; init; } = SpecialEffect.None;

        public string GetSpecialEffectDescription()
        {
            return Effect switch
            {
                SpecialEffect.DoubleQuestion => "The winner gets to ask two questions!",
                SpecialEffect.DareOnly => "The loser must do a dare!",
                SpecialEffect.TruthOnly => "The loser must answer a truth question~",
                SpecialEffect.ReverseRoles => "Ops~ The roles are reversed for this round.",
                SpecialEffect.EveryoneAnswers => "Everyone in the party must answer a question. OwO",
                _ => ""
            };
        }
    }
}
