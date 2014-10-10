using System;
using System.Collections.Generic;
using System.Linq;

namespace CoupCore
{
    public class Deck
    {
        static readonly Random R = new Random();

        private List<Card> Cards { get; set; }

        public Deck( int numRepetitions )
        {
            Cards = new List<Card>();
            for ( var i = 0 ; i < numRepetitions ; i++ )
            {
                Cards.Add( Card.Assassin );
                Cards.Add( Card.Captain );
                Cards.Add( Card.Contessa );
                Cards.Add( Card.Inquisitor );
                Cards.Add( Card.Duke );
            }
        }

        public void Shuffle()
        {
            for ( var i = 0 ; i < 100 ; i++ )
            {
                Swap( R.Next( Cards.Count ), R.Next( Cards.Count ) );
            }
        }

        private void Swap( int i, int j )
        {
            var c = Cards[ i ];
            Cards[ i ] = Cards[ j ];
            Cards[ j ] = c;
        }

        internal Card DrawCard()
        {
            Shuffle();
            var c = Cards.First();
            Cards.RemoveAt( 0 );
            return c;
        }

        internal void ReturnCard( Card card )
        {
            Cards.Add( card );
        }
    }

    public enum Card
    {
        None,
        Duke,
        Contessa,
        Assassin,
        Captain,
        Inquisitor
    }

    public static class Ext
    {
        static readonly Random R = new Random();

        public static int RandomIndex( this IEnumerable<Player> players )
        {
            return R.Next( players.Count() );
        }
    }
}
