using System;
using System.Collections.Generic;
using System.Linq;
using CoupCore;

namespace EasyBot
{
    public class EasyBot : Player
    {
        List<PlayerDescriptor> _players = new List<PlayerDescriptor>();

        public EasyBot( int index )
            : base( index )
        {
            
        }

        public override ActionDescriptor Play(bool mustCoup)
        {
            var r = new Random();
            if (mustCoup || (Coins >= 7 && r.Next(2) == 1))
            {
                var firstOrDefault =
                    _players.Where(x => x.Index != Index)
                        .OrderByDescending(x => x.Coins)
                        .FirstOrDefault(x => x.LostCard1 == Card.None || x.LostCard2 == Card.None);
                if (firstOrDefault != null)
                    return new CoupDEtatDescriptor(firstOrDefault.Index);
            }
            else
            {
                if (Card1 == Card.Duke || Card2 == Card.Duke)
                {
                    return new CollectTaxesDescriptor();
                }
                if ((Card1 == Card.Captain || Card2 == Card.Captain) && r.Next(3) > 2)
                {
                    var firstOrDefault =
                        _players.Where(x => x.Index != Index)
                            .OrderByDescending(x => x.Coins)
                            .FirstOrDefault(x => x.LostCard1 == Card.None || x.LostCard2 == Card.None);
                    if (
                        firstOrDefault != null)
                        return new ExtortionDescriptor(firstOrDefault.Index);
                }
                else if ((Card1 == Card.Assassin || Card2 == Card.Assassin && Coins >= 3))
                {
                    var firstOrDefault =
                        _players.Where(x => x.Index != Index)
                            .OrderByDescending(x => x.Coins)
                            .FirstOrDefault(x => x.LostCard1 == Card.None || x.LostCard2 == Card.None);
                    if (firstOrDefault != null)
                        return new AssassinateDescription(firstOrDefault.Index);
                }
                else if (r.Next(3) == 1)
                {
                    return new ForeignAidDescriptor();
                }
                else if (r.Next(5) == 1)
                {
                    return new ExchangeDescriptor();
                }
                else if (r.Next(4) == 1)
                {
                    PlayerDescriptor firstOrDefault =
                        _players.Where(x => x.Index != Index)
                            .OrderByDescending(x => x.Coins)
                            .FirstOrDefault(x => x.LostCard1 == Card.None || x.LostCard2 == Card.None);
                    if (firstOrDefault != null)
                        return new InvestigateDescriptor(firstOrDefault.Index);
                }
                else
                {
                    return new IncomeDescriptor();
                }
            }

            throw new NotImplementedException();
        }

        public override Card TriesToBlock( ActionType playerAction, int playerTurnIndex )
        {
            switch (playerAction)
            {
                case ActionType.Extortion:
                    if ( Card1 == Card.Captain || Card2 == Card.Captain )
                    {
                        return Card.Captain;
                    }
                    else if ( Card1 == Card.Inquisitor || Card2 == Card.Inquisitor )
                    {
                        return Card.Inquisitor;
                    }
                    break;
                case ActionType.ForeignAid:
                {
                    var r = new Random();
                    if ( Card1 == Card.Duke || Card2 == Card.Duke || r.Next( 5 ) == 1 )
                    {
                        return Card.Duke;
                    }
                }
                    break;
                case ActionType.Assassinate:
                    if ( Card1 == Card.Contessa || Card2 == Card.Contessa )
                    {
                        return Card.Contessa;
                    }
                    break;
            }
            return Card.None;
        }

        public override bool Challenge( int playerIndex, Card card )
        {
            var r = new Random();
            if ( card == Card.Duke && r.Next( 1 ) == 0 )
            {
                return true;
            }
            return false;
        }

        public override Card ChooseLoseInfluence()
        {
            return Card1 != Card.None ? Card1 : Card2;
        }

        public override Card GiveCardToInquisitor( int targetIndex )
        {
            return ChooseLoseInfluence();
        }

        public override bool ShouldTargetChangeCard( int targetIndex, Card targetCard )
        {
            return true;
        }

        public override Card ChooseOneToRemove( Card newCard )
        {
            return Card1 != Card.None ? Card1 : Card2;
        }

        public override void SendStatus( List<PlayerDescriptor> players )
        {
            _players = players;
        }

        public override void SignalNewTurn( int playerIndex )
        {

        }

        public override void SignalBlocking( int p1, ActionType actionType, int p2, Card cardBlocking )
        {

        }

        public override void SignalLostInfluence( int p, Card influence )
        {

        }

        public override void SignalChallenge( int challenger, int challenged, Card card )
        {

        }

        public override void SignalPlayersTargettedAction( int p1, ActionType actionType, int p2 )
        {

        }

        public override void SignalPlayerAction( int p, ActionType actionType )
        {

        }
    }
}
