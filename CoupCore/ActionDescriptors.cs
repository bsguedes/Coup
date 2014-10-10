using System.Collections.Generic;

namespace CoupCore
{
    public abstract class TargettedActionDescriptor : ActionDescriptor
    {
        internal int TargetIndex { get; set; }
        internal abstract bool IsValid( List<Player> players, int playerIndex, int playerTargetIndex );
    }

    public abstract class ActionDescriptor
    {
        internal abstract Action AsAction( List<Player> players );
        internal abstract bool IsValid( List<Player> players, int playerIndex );
    }

    public class InvestigateDescriptor : TargettedActionDescriptor
    {
        public InvestigateDescriptor( int index )
        {
            TargetIndex = index;
        }

        internal override bool IsValid( List<Player> players, int playerIndex, int playerTargetIndex )
        {
            return players[ playerIndex ].Coins < 10 && players[ playerTargetIndex ].IsAlive && playerIndex != playerTargetIndex;
        }

        internal override Action AsAction( List<Player> players )
        {
            return new Investigate( players[ TargetIndex ] );
        }

        internal override bool IsValid( List<Player> players, int playerIndex )
        {
            return false;
        }
    }

    public class ExchangeDescriptor : ActionDescriptor
    {
        internal override Action AsAction( List<Player> players )
        {
            return new Exchange();
        }

        internal override bool IsValid( List<Player> players, int playerIndex )
        {
            return players[ playerIndex ].Coins < 10;
        }
    }

    public class IncomeDescriptor : ActionDescriptor 
    {
        internal override Action AsAction( List<Player> players )
        {
            return new Income();
        }

        internal override bool IsValid( List<Player> players, int playerIndex )
        {
            return players[ playerIndex ].Coins < 10;
        }
    }

    public class ForeignAidDescriptor : ActionDescriptor
    {
        internal override Action AsAction( List<Player> players )
        {
            return new ForeignAid();
        }

        internal override bool IsValid( List<Player> players, int playerIndex )
        {
            return players[ playerIndex ].Coins < 10;
        }
    }

    public class CollectTaxesDescriptor : ActionDescriptor
    {

        internal override Action AsAction( List<Player> players )
        {
            return new CollectTaxes();
        }

        internal override bool IsValid( List<Player> players, int playerIndex )
        {
            return players[ playerIndex ].Coins < 10;
        }
    }

    public class AssassinateDescription : TargettedActionDescriptor
    {
        public AssassinateDescription( int index )
        {
            TargetIndex = index;
        }

        internal override bool IsValid( List<Player> players, int playerIndex, int playerTargetIndex )
        {
            return players[ playerIndex ].Coins < 10 && players[ playerIndex ].Coins >= 3 && players[ playerTargetIndex ].IsAlive && playerIndex != playerTargetIndex;
        }   

        internal override Action AsAction( List<Player> players )
        {
            return new Assassinate( players[ TargetIndex ] );
        }

        internal override bool IsValid( List<Player> players, int playerIndex )
        {
            return false;
        }
    }

    public class ExtortionDescriptor : TargettedActionDescriptor
    {
        public ExtortionDescriptor( int index )
        {
            TargetIndex = index;
        }

        internal override bool IsValid( List<Player> players, int playerIndex, int playerTargetIndex )
        {
            return players[ playerIndex ].Coins < 10 && players[ playerTargetIndex ].IsAlive && playerIndex != playerTargetIndex;            
        }

        internal override Action AsAction( List<Player> players )
        {
            return new Extortion( players[ TargetIndex ] );
        }

        internal override bool IsValid( List<Player> players, int playerIndex )
        {
            return false;
        }
    }

    public class CoupDEtatDescriptor : TargettedActionDescriptor
    {
        public CoupDEtatDescriptor( int index )
        {
            TargetIndex = index;
        }
        
        internal override Action AsAction( List<Player> players )
        {
            return new CoupDEtat( players[ TargetIndex ] );
        }

        internal override bool IsValid( List<Player> players, int playerIndex, int playerTargetIndex )
        {
            return players[ playerIndex ].Coins >= 7 && players[ playerTargetIndex ].IsAlive && playerTargetIndex != playerIndex;
        }

        internal override bool IsValid( List<Player> players, int playerIndex )
        {
            return false;
        }
    }


}
