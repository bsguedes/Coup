using System;
using System.Collections.Generic;

namespace CoupCore
{

    class CoupDEtat : TargettedAction
    {
        public override ActionType ActionType { get { return ActionType.CoupDEtat; } }

        public CoupDEtat( Player target )
            : base( target )
        {

        }

        public override void ResolveAction( Player playerTurn )
        {
            playerTurn.DeltaCoins( -7 );
            Target.LoseInfluence();
        }

    }

    class ForeignAid : Action, IBlockable
    {
        public override ActionType ActionType { get { return ActionType.ForeignAid; } }

        public override void ResolveAction( Player playerTurn )
        {
            playerTurn.DeltaCoins( 2 );
        }

        public IEnumerable<Card> Blockers
        {
            get { return new[] { Card.Duke }; }
        }
    }

    class Income : Action
    {
        public override ActionType ActionType { get { return ActionType.Income; } }

        public override void ResolveAction( Player playerTurn )
        {
            playerTurn.DeltaCoins( 1 );
        }
    }

    class CollectTaxes : Action, IChallengeable
    {
        public override ActionType ActionType { get { return ActionType.CollectTaxes; } }

        public override void ResolveAction( Player playerTurn )
        {
            playerTurn.DeltaCoins( 3 );
        }

        public Card Card
        {
            get { return Card.Duke; }
        }
    }

    class Investigate : TargettedAction, IChallengeable, INeedDeck
    {
        public override ActionType ActionType { get { return ActionType.Investigate; } }

        public Investigate( Player target )
            : base( target )
        {

        }

        public override void ResolveAction( Player playerTurn )
        {
            throw new NotImplementedException();
        }

        public Card Card
        {
            get { return Card.Inquisitor; }
        }

        public void ResolveAction( Deck deck, Player playerTurn )
        {
            var targetCard = Target.GiveCardToInquisitor( playerTurn.Index );
            if ( playerTurn.ShouldTargetChangeCard( Target.Index, targetCard ) )
            {
                Target.SendCardBackToDeckAndDrawCard( deck, targetCard );                
            }            
        }
    }

    class Exchange : Action, IChallengeable, INeedDeck
    {
        public override ActionType ActionType { get { return ActionType.Exchange; } }

        public Card Card
        {
            get { return Card.Inquisitor; }
        }

        public override void ResolveAction( Player playerTurn )
        {
            throw new NotImplementedException();
        }

        public void ResolveAction( Deck deck, Player playerTurn )
        {
            var newCard = deck.DrawCard();
            var removed = playerTurn.ChooseOneToRemove( newCard );
            playerTurn.ChangeCards( deck, newCard, removed );
        }
    }


    class Assassinate : TargettedAction, IBlockable, IChallengeable
    {
        public override ActionType ActionType { get { return ActionType.Assassinate; } }

        public Assassinate( Player target )
            : base( target )
        {

        }

        public override void ResolveAction( Player playerTurn )
        {
            playerTurn.DeltaCoins( -3 );
            Target.LoseInfluence();
        }

        public IEnumerable<Card> Blockers
        {
            get { return new[] { Card.Contessa }; }
        }

        public Card Card
        {
            get { return Card.Assassin; }
        }

    }

    class Extortion : TargettedAction, IBlockable, IChallengeable
    {
        public override ActionType ActionType { get { return ActionType.Extortion; } }

        public Extortion ( Player target )
            : base ( target )
        {

        }

        public override void ResolveAction( Player playerTurn )
        {
            if ( Target.Coins >= 2 )
            {
                playerTurn.DeltaCoins( 2 );
                Target.DeltaCoins( -2 );
            }
            else if ( Target.Coins == 1 )
            {
                playerTurn.DeltaCoins( 1 );
                Target.DeltaCoins( -1 );
            }
        }

        public IEnumerable<Card> Blockers
        {
            get { return new[] { Card.Captain, Card.Inquisitor }; }
        }

        public Card Card
        {
            get { return Card.Captain; }
        }
    }    

    abstract class Action
    {
        public abstract ActionType ActionType { get; }
        public abstract void ResolveAction( Player playerTurn );
    }

    public enum ActionType
    {
        CoupDEtat,
        Income,
        ForeignAid,
        Assassinate,
        Extortion,
        Exchange,
        CollectTaxes,
        Investigate
    }

    abstract class TargettedAction : Action, ITargetted
    {
        public Player Target { get; set; }

        protected TargettedAction( Player target )
        {
            Target = target;
        }
    }

}
