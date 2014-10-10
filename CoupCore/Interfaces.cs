using System.Collections.Generic;

namespace CoupCore
{
    public interface IChallengeable
    {
        Card Card { get; }
    }

    public interface IBlockable
    {
        IEnumerable<Card> Blockers { get; }
    }

    public interface ITargetted
    {
        Player Target { get; set; }
    }    

    public interface INeedDeck
    {
        void ResolveAction( Deck deck, Player playerTurn );
    }
    
}
