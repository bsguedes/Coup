using System.Collections.Generic;

namespace CoupCore
{
    public abstract class Player
    {
        public int Index { get; private set; }
        public Card Card1 { get; private set; }
        public Card Card2 { get; private set; }
        public int Coins { get; private set; }
        public Card LostCard1 { get; private set; }
        public Card LostCard2 { get; private set; }
        private bool _canSetCards;

        #region Constructor

        protected Player( int index )
        {
            _canSetCards = true;
            Index = index;
        }

        #endregion
        
        #region Public methods

        public override string ToString()
        {
            return string.Format( "{0}: {1}, {2}, Coins: {3}", Index, Card1, Card2, Coins );
        }

        /// <summary>
        /// Checks if a player still has influences.
        /// </summary>
        internal bool IsAlive
        {
            get
            {
                return Card1 != Card.None || Card2 != Card.None;
            }
        }

        #endregion

        #region Abstract methods

        /// <summary>
        /// Executes a turn play.
        /// </summary>
        /// <returns>Returns the action the player wishes to execute.</returns>
        public abstract ActionDescriptor Play( bool mustCoup );

        /// <summary>
        /// Signals the intention of this player to block another players' action.
        /// </summary>
        /// <param name="playerAction">The action that may be blocked.</param>
        /// <param name="playerTurnIndex">The player.</param>
        /// <returns>The card that can block this action, Card.None otherwise.</returns>
        public abstract Card TriesToBlock( ActionType playerAction, int playerTurnIndex );

        /// <summary>
        /// Signals the intention of this player to challenge other players.
        /// </summary>
        /// <param name="playerIndex">The player being challenged.</param>
        /// <param name="card">The card that this player will question.</param>
        /// <returns>true if this player is challenging, false otherwise.</returns>
        public abstract bool Challenge( int playerIndex, Card card );

        /// <summary>
        /// Asks the player to give up one of his influences.        
        /// </summary>
        /// <returns>The influence that will be lost.</returns>
        public abstract Card ChooseLoseInfluence();
        
        /// <summary>
        /// Choose one of your cards to give it to the inquisitor.
        /// </summary>
        /// <param name="targetIndex">The inquisitor.</param>        
        /// <returns>The card that the inquisitor will see.</returns>
        public abstract Card GiveCardToInquisitor( int targetIndex );
        
        /// <summary>
        /// You are the inquisitor. You have to decide if target keeps targetCard.
        /// </summary>
        /// <param name="targetIndex">The target.</param>
        /// <param name="targetCard">The card he choose for you to see.</param>
        /// <returns>true if you want the player to change the card, otherwise false.</returns>
        public abstract bool ShouldTargetChangeCard( int targetIndex, Card targetCard );        

        /// <summary>
        /// You receive a third card. You return one card back to the deck.
        /// </summary>
        /// <returns>The card that goes back to the deck.</returns>
        public abstract Card ChooseOneToRemove( Card newCard );

        /// <summary>
        /// Reports status to all players.
        /// </summary>
        /// <param name="players"></param>
        public abstract void SendStatus( List<PlayerDescriptor> players );

        /// <summary>
        /// Returns a new turn to all players.
        /// </summary>
        /// <param name="playerIndex">The player that will play in the next turn.</param>
        public abstract void SignalNewTurn( int playerIndex );

        /// <summary>
        /// Signals that the active player tried to execute action actionType but was blocked by cardBlocking that belongs to p2.
        /// </summary>
        /// <param name="p1">The player that did the action.</param>
        /// <param name="actionType">The action the first player tried to do.</param>
        /// <param name="p2">The player that blocks the action.</param>
        /// <param name="cardBlocking">The card used by the second player to block the first action.</param>
        public abstract void SignalBlocking( int p1, ActionType actionType, int p2, Card cardBlocking );

        /// <summary>
        /// Signals all players that a player lost an influence.
        /// </summary>
        /// <param name="p">The index of the player that lost the influence.</param>
        /// <param name="influence">The card that was lost.</param>        
        public abstract void SignalLostInfluence( int p, Card influence );

        /// <summary>
        /// Signals that a challenger is questioning that a challenged has a card.
        /// </summary>
        /// <param name="challenger">The challenger.</param>
        /// <param name="challenged">The player that is being challenged.</param>
        /// <param name="card">The card that is being challenged.</param>
        public abstract void SignalChallenge( int challenger, int challenged, Card card );

        /// <summary>
        /// Signals to all players that player p1 is trying to do an action against p2.
        /// </summary>
        /// <param name="p1">The player that does the action.</param>
        /// <param name="actionType">The action.</param>
        /// <param name="p2">The target player to the action.</param>
        public abstract void SignalPlayersTargettedAction( int p1, ActionType actionType, int p2 );

        /// <summary>
        /// Signals to all players that player p is doing a targetless action.
        /// </summary>
        /// <param name="p">The index of the player that does the action.</param>
        /// <param name="actionType">The action.</param>
        public abstract void SignalPlayerAction( int p, ActionType actionType );

        #endregion

        #region Common interactions

        /// <summary>
        /// Checks if this player has a given card.
        /// </summary>
        /// <param name="card">The card that is being checked.</param>
        /// <returns>true if this player has the card, false otherwise.</returns>
        internal bool HasCard( Card card )
        {
            return ( card == Card1 || card == Card2 );
        }

        /// <summary>
        /// Removes one of the influences of this player.
        /// </summary>
        internal Card LoseInfluence()
        {
            var lose = ChooseLoseInfluence();
            if ( lose == Card1 || lose == Card2 )
            {
                RemoveCard( lose );
                if ( LostCard1 == Card.None )
                {
                    LostCard1 = lose;
                }
                else if ( LostCard2 == Card.None )
                {
                    LostCard2 = lose;
                }                
            }
            else
            {
                RemoveCard(Card1 != Card.None ? Card1 : Card2);
            }
            return lose;
        }

        /// <summary>
        /// Returns a card to the deck and draws a new one.
        /// </summary>
        /// <param name="deck">The deck.</param>
        /// <param name="card">The card going back to the deck.</param>
        internal void ChangeCard( Deck deck, Card card )
        {
            RemoveCard( card ); /* remove from player */
            deck.ReturnCard( card ); /* returns it to the deck */
            TakeCardFromDeck( deck ); /* and then draws */
        }

        /// <summary>
        /// Draws a new card and then returns a card to the deck.
        /// </summary>
        /// <param name="deck">The deck.</param>
        /// <param name="targetCard">The card returned to the deck.</param>
        internal void SendCardBackToDeckAndDrawCard( Deck deck, Card targetCard )
        {
            RemoveCard( targetCard ); /* remove from player */
            TakeCardFromDeck( deck ); /* draws the card */
            deck.ReturnCard( targetCard ); /* returns the old card to deck */
        }

        /// <summary>
        /// Changes the removed card from this players by the newCard.
        /// </summary>
        /// <param name="deck">The deck.</param>
        /// <param name="newCard">The card that will stay with the player.</param>
        /// <param name="removed">The card that will go back to the deck.</param>
        internal void ChangeCards( Deck deck, Card newCard, Card removed )
        {
            if ( Card1 == removed )
            {
                Card1 = newCard;
                deck.ReturnCard( removed );
            }
            else if ( Card2 == removed )
            {
                Card2 = newCard;
                deck.ReturnCard( removed );
            }
            else
            {
                deck.ReturnCard( newCard );
            }
        }

        /// <summary>
        /// Gives two cards to a player.
        /// </summary>
        /// <param name="card1"></param>
        /// <param name="card2"></param>
        internal void GiveCards( Card card1, Card card2 )
        {
            if ( _canSetCards )
            {
                Card1 = card1;
                Card2 = card2;
                _canSetCards = false;
            }
        }

        /// <summary>
        /// Adds diff coins to a player.
        /// </summary>
        /// <param name="diff"></param>
        internal void DeltaCoins( int diff )
        {
            Coins += diff;
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Remove a card from this player.
        /// </summary>
        /// <param name="targetCard">The removed card.</param>
        private void RemoveCard( Card targetCard )
        {
            if ( Card1 == targetCard )
            {
                Card1 = Card.None;
            }
            else if ( Card2 == targetCard )
            {
                Card2 = Card.None;
            }
        }

        /// <summary>
        /// Gives a card to a player.
        /// </summary>
        /// <param name="card">The card that is given.</param>
        private void AddCard( Card card )
        {
            if ( Card1 == Card.None )
            {
                Card1 = card;
            }
            else if ( Card2 == Card.None )
            {
                Card2 = card;
            }
        }

        /// <summary>
        /// Draw a card from the deck and gives it to this player.
        /// </summary>
        /// <param name="deck">The deck.</param>
        private void TakeCardFromDeck( Deck deck )
        {
            var card = deck.DrawCard();
            AddCard( card );
        }

        #endregion        
                        
    }   
}
