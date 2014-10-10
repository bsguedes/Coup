using System;
using System.Collections.Generic;
using System.Linq;

namespace CoupCore
{
    public class Game
    {
        public List<Player> Players { get; private set; }
        public Deck Deck { get; private set; }

        static readonly Random R = new Random();

        public Game( List<Player> players )
        {
            Players = players;

            InitializeDeck( players.Count );
            GiveCardsAndTwoCoins();
            SignalStatusToEveryPlayer();

            /* the player that starts the game */
            var playerIndex = Players.RandomIndex();

            while ( !IsGameOver() )
            {                
                var playerTurn = players[ playerIndex ];

                if ( playerTurn.IsAlive )
                {
                    var action = playerTurn.Play( playerTurn.Coins >= 10 );

                    if ( ( action is TargettedActionDescriptor && ( action as TargettedActionDescriptor ).IsValid( players, playerIndex, ( action as TargettedActionDescriptor ).TargetIndex ) )
                        || ( action.IsValid( players, playerIndex ) ) )
                    {
                        var playerAction = action.AsAction( Players );

                        /***********
                         * SIGNAL  *
                        ***********/
                        SignalNewTurn( playerIndex );
                        if ( playerAction is TargettedAction )
                        {
                            SignalPlayersTargettedAction( playerTurn, playerAction, ( playerAction as TargettedAction ).Target );
                        }
                        else
                        {
                            SignalPlayerAction( playerTurn, playerAction );
                        }

                        /***********
                         * RESOLVE *
                        ***********/

                        if ( !( playerAction is IChallengeable ) && !( playerAction is IBlockable ) && !( playerAction is ITargetted ) )
                        /* income */
                        {
                            playerAction.ResolveAction( playerTurn );                            
                        }
                        else if ( playerAction is ITargetted && !( playerAction is IBlockable ) && !( playerAction is IChallengeable ) )
                        /* coup d'etat */
                        {
                            playerAction.ResolveAction( playerTurn );
                        }
                        else if ( playerAction is IChallengeable && playerAction is IBlockable && playerAction is ITargetted )
                        /* assassinate, extortion */
                        {
                            ResolveChallengeableBlockableTargettedAction( playerTurn, playerAction );
                        }
                        else if ( playerAction is ITargetted && !( playerAction is IBlockable ) && playerAction is IChallengeable )
                        /* investigate */
                        {
                            ResolveTargettedChallengeableNonBlockableAction( playerTurn, playerAction );
                        }
                        else if ( playerAction is IChallengeable && !( playerAction is IBlockable ) && !( playerAction is ITargetted ) )
                        /* collect taxes, exchange cards */
                        {
                            ResolveChallengeableNonBlockableNonTargettedAction( playerTurn, playerAction );
                        }
                        else if ( playerAction is IBlockable && !( playerAction is IChallengeable ) && !( playerAction is ITargetted ) )
                        /* foreign aid */
                        {
                            ResolveBlockableNonChallengeableNonTargettedAction( playerTurn, playerAction );
                        }

                        /***********
                         * SIGNAL  *
                        ***********/
                        SignalStatusToEveryPlayer();
                    }
                    else
                    {
                        /* invalid action, what should we do ? */
                    }

                }

                playerIndex = ( playerIndex + 1 ) % players.Count;
            }
        }

        private void ResolveBlockableNonChallengeableNonTargettedAction( Player playerTurn, Action playerAction )
        {
            bool noBlock = true, challengeWon = false;
            foreach ( var playerBlocker in RandomOtherPlayers( playerTurn.Index ) )
            {
                // in this case every player may block, because this action is not targetted
                var card = playerBlocker.TriesToBlock( playerAction.ActionType, playerTurn.Index );
                if ( card != Card.None ) // there is a block, card is the alledged card that blocks the action
                {
                    SignalBlocking( playerTurn, playerAction, playerBlocker, card );
                    noBlock = false;
                    if ( playerTurn.Challenge( playerBlocker.Index, card ) ) // the blocked player can challenge the blocker
                    {
                        SignalChallenge( playerBlocker, card, playerTurn );
                        if ( playerBlocker.HasCard( card ) )
                        {
                            var influence = playerTurn.LoseInfluence();
                            playerBlocker.ChangeCard( Deck, card );
                            SignalLostInfluence( playerTurn, influence );
                        }
                        else
                        {
                            var influence = playerBlocker.LoseInfluence();
                            challengeWon = true;
                            SignalLostInfluence( playerBlocker, influence );
                        }
                    }
                    else // if the blocked player refuses to challenge, other players can challenge in its place
                    {
                        foreach ( var spectator in RandomOtherPlayers( playerBlocker.Index, playerTurn.Index ) )
                        {
                            if ( spectator.Challenge( playerBlocker.Index, card ) )
                            {
                                SignalChallenge( playerBlocker, card, spectator );
                                if ( playerBlocker.HasCard( card ) )
                                {
                                    var influence = spectator.LoseInfluence();
                                    playerBlocker.ChangeCard( Deck, card );
                                    SignalLostInfluence( spectator, influence );
                                }
                                else
                                {
                                    var influence = playerBlocker.LoseInfluence();
                                    challengeWon = true;
                                    SignalLostInfluence( playerBlocker, influence );
                                }
                                break;
                            }
                        }
                    }
                    break;
                }
            }
            if ( noBlock || challengeWon )
            {
                playerAction.ResolveAction( playerTurn );
            }
        }

        private void ResolveChallengeableNonBlockableNonTargettedAction( Player playerTurn, Action playerAction )
        {
            bool noChallenge = true, challengeWon = false;
            var card = ( (IChallengeable) playerAction ).Card;
            foreach ( var challengerPlayer in RandomOtherPlayers( playerTurn.Index ) )
            {
                if ( challengerPlayer.Challenge( playerTurn.Index, card ) )
                {
                    SignalChallenge( playerTurn, card, challengerPlayer );
                    noChallenge = false;
                    if ( playerTurn.HasCard( card ) )
                    {
                        var influence = challengerPlayer.LoseInfluence();
                        playerTurn.ChangeCard( Deck, card );
                        challengeWon = true;
                        SignalLostInfluence( challengerPlayer, influence );
                    }
                    else
                    {
                        var influence = playerTurn.LoseInfluence();
                        SignalLostInfluence( playerTurn, influence );
                    }
                    break;
                }
            }
            if ( noChallenge || challengeWon )
            {
                if ( playerAction is INeedDeck )
                {
                    ( playerAction as INeedDeck ).ResolveAction( Deck, playerTurn );
                }
                else
                {
                    playerAction.ResolveAction( playerTurn );
                }
            }
        }

        private void ResolveTargettedChallengeableNonBlockableAction( Player playerTurn, Action playerAction )
        {
            bool noChallenge = true, challengeWon = false;
            var card = ( (IChallengeable) playerAction ).Card;
            var playerTarget = ( (ITargetted) playerAction ).Target;
            if ( playerTarget.Challenge( playerTurn.Index, card ) )
            {
                SignalChallenge( playerTurn, card, playerTarget );
                noChallenge = false;
                if ( playerTurn.HasCard( card ) )
                {
                    var influence = playerTarget.LoseInfluence();
                    playerTurn.ChangeCard( Deck, card );
                    challengeWon = true;
                    SignalLostInfluence( playerTarget, influence );
                }
                else
                {
                    var influence = playerTurn.LoseInfluence();
                    SignalLostInfluence( playerTurn, influence );
                }
            }
            else
            {
                foreach ( var spectator in RandomOtherPlayers( playerTurn.Index, playerTarget.Index ) )
                {
                    if ( spectator.Challenge( playerTurn.Index, card ) )
                    {
                        SignalChallenge( playerTurn, card, spectator );
                        noChallenge = false;
                        if ( playerTurn.HasCard( card ) )
                        {
                            var influence = spectator.LoseInfluence();
                            playerTurn.ChangeCard( Deck, card );
                            challengeWon = true;
                            SignalLostInfluence( spectator, influence );
                        }
                        else
                        {
                            var influence = playerTurn.LoseInfluence();
                            SignalLostInfluence( playerTurn, influence );
                        }
                        break;
                    }
                }
            }

            if ( noChallenge || challengeWon )
            {
                if ( playerAction is INeedDeck )
                {
                    ( playerAction as INeedDeck ).ResolveAction( Deck, playerTurn );
                }
                else
                {
                    playerAction.ResolveAction( playerTurn );
                }
            }
        }

        private void ResolveChallengeableBlockableTargettedAction( Player playerTurn, Action playerAction )
        {
            bool noChallenge = true, challengeWon = false, noBlock = true;
            var card = ( (IChallengeable) playerAction ).Card;
            var playerTarget = ( (ITargetted) playerAction ).Target;
            if ( playerTarget.Challenge( playerTurn.Index, card ) )
            {
                SignalChallenge( playerTurn, card, playerTarget );
                noChallenge = false;
                if ( playerTurn.HasCard( card ) )
                {
                    var influence = playerTarget.LoseInfluence();
                    playerTurn.ChangeCard( Deck, card );
                    challengeWon = true;
                    SignalLostInfluence( playerTarget, influence );
                }
                else
                {
                    var influence = playerTurn.LoseInfluence();
                    SignalLostInfluence( playerTurn, influence );
                }
            }
            else
            {
                foreach ( var spectator in RandomOtherPlayers( playerTurn.Index, playerTarget.Index ) )
                {
                    if ( spectator.Challenge( playerTurn.Index, card ) )
                    {
                        SignalChallenge( playerTurn, card, spectator );
                        noChallenge = false;
                        if ( playerTurn.HasCard( card ) )
                        {
                            var influence = spectator.LoseInfluence();
                            playerTurn.ChangeCard( Deck, card );
                            challengeWon = true;
                            SignalLostInfluence( spectator, influence );
                        }
                        else
                        {
                            var influence = playerTurn.LoseInfluence();
                            SignalLostInfluence( playerTurn, influence );
                        }
                        break;
                    }
                }
            }

            if ( noChallenge )
            {
                var cardBlocking = playerTarget.TriesToBlock( playerAction.ActionType, playerTurn.Index );
                if ( cardBlocking != Card.None ) // target tried to block
                {
                    SignalBlocking( playerTurn, playerAction, playerTarget, cardBlocking );
                    noBlock = false;
                    if ( playerTurn.Challenge( playerTarget.Index, card ) ) // the blocked player can challenge the blocker
                    {
                        SignalChallenge( playerTarget, cardBlocking, playerTurn );
                        if ( playerTarget.HasCard( card ) )
                        {
                            var influence = playerTurn.LoseInfluence();
                            playerTarget.ChangeCard( Deck, card );
                            SignalLostInfluence( playerTurn, influence );
                        }
                        else
                        {
                            var influence = playerTarget.LoseInfluence();
                            challengeWon = true;
                            SignalLostInfluence( playerTarget, influence );
                        }
                    }
                    else // if the blocked player refuses to challenge, other players can challenge in its place
                    {
                        foreach ( var spectator in RandomOtherPlayers( playerTarget.Index, playerTurn.Index ) )
                        {
                            if ( spectator.Challenge( playerTarget.Index, card ) )
                            {
                                SignalChallenge( playerTarget, cardBlocking, spectator );
                                if ( playerTarget.HasCard( card ) )
                                {
                                    var influence = spectator.LoseInfluence();
                                    playerTarget.ChangeCard( Deck, card );
                                    SignalLostInfluence( spectator, influence );
                                }
                                else
                                {
                                    var influence = playerTarget.LoseInfluence();
                                    challengeWon = true;
                                    SignalLostInfluence( playerTarget, influence );
                                }
                                break;
                            }
                        }
                    }
                }
            }
            if ( ( noBlock && noChallenge ) || challengeWon )
            {
                playerAction.ResolveAction( playerTurn );
            }
        }  

        private List<PlayerDescriptor> GetStatus()
        {
            return Players.Select( x => new PlayerDescriptor
                {
                    Index = x.Index,
                    LostCard1 = x.LostCard1,
                    LostCard2 = x.LostCard2,
                    Coins = x.Coins
                } ).ToList();
        }

        private void SignalStatusToEveryPlayer()
        {
            Players.ForEach( x => x.SendStatus( GetStatus() ) );
            //Console.WriteLine( string.Join( " ### ", GetStatus().Select( x => x.ToString() ) ) );
            Console.WriteLine( string.Join( " ### ", Players.Select( x => x.ToString() ) ) );
        }

        private void SignalNewTurn( int playerIndex )
        {
            Players.ForEach( x => x.SignalNewTurn( playerIndex ) );
            Console.WriteLine( "####################### player {0} turn #######################", playerIndex );
        }

        private void SignalBlocking( Player player, Action action, Player blocker, Card cardBlocking )
        {
            Players.ForEach( x => x.SignalBlocking( blocker.Index, action.ActionType, player.Index, cardBlocking ) );
            Console.WriteLine( "player {0} tries to block {1} from player {2} using {3}", blocker.Index, action.ActionType, player.Index, cardBlocking );
        }

        private void SignalLostInfluence( Player player, Card influence )
        {
            Players.ForEach( x => x.SignalLostInfluence( player.Index, influence ) );
            Console.WriteLine( "player {0} lost a {1}", player.Index, influence );
        }

        private void SignalChallenge( Player challenged, Card card, Player challenger )
        {
            Players.ForEach( x => x.SignalChallenge( challenger.Index, challenged.Index, card ) );
            Console.WriteLine( "player {0} challenges player {1} that he does not have a {2}", challenger.Index, challenged.Index, card );
        }

        private void SignalPlayersTargettedAction( Player player, Action action, Player target )
        {
            Players.ForEach( x => x.SignalPlayersTargettedAction( player.Index, action.ActionType, target.Index ) );
            SignalStatusToEveryPlayer();
            Console.WriteLine( "player {0} does {1} against {2}", player.Index, action.ActionType, target.Index );
        }

        private void SignalPlayerAction( Player player, Action action )
        {
            Players.ForEach( x => x.SignalPlayerAction( player.Index, action.ActionType ) );
            SignalStatusToEveryPlayer();
            Console.WriteLine( "player {0} does {1}", player.Index, action.ActionType );
        }

        private IEnumerable<Player> RandomOtherPlayers( int playerIndex1, int playerIndex2 )
        {
            var list = new List<Player>();
            list.AddRange( Players );
            var p1 = Players[ playerIndex1 ];
            var p2 = Players[ playerIndex2 ];
            list.Remove( p1 );
            list.Remove( p2 );
            while ( list.Count > 0 )
            {
                var i = R.Next( list.Count );
                var p = list[ i ];
                yield return p;
                list.RemoveAt( i );
            }
        }

        private IEnumerable<Player> RandomOtherPlayers( int playerIndex )
        {
            var list = new List<Player>();
            list.AddRange( Players );
            list.RemoveAt( playerIndex );            
            while ( list.Count > 0 )
            {
                var i = R.Next( list.Count );
                var p = list[ i ];
                yield return p;
                list.RemoveAt( i );
            }
        }            

        private bool IsGameOver()
        {
            return Players.Count( x=> x.IsAlive ) == 1;
        }

        private void GiveCardsAndTwoCoins()
        {
            foreach (Player player in Players)
            {
                player.DeltaCoins( 2 );
                player.GiveCards( Deck.DrawCard(), Deck.DrawCard() );
            }
        }

        private void InitializeDeck( int playerCount )
        {
            var numRepeat = playerCount <= 6 ? 3 : playerCount <= 8 ? 4 : 5;

            Deck = new Deck( numRepeat );
        }
    }
    
}
