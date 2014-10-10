namespace CoupCore
{
    public class PlayerDescriptor
    {
        public int Index { get; set; }
        public Card LostCard1 { get; set; }
        public Card LostCard2 { get; set; }
        public int Coins { get; set; }

        public override string ToString()
        {
            return string.Format( "{0}: {1}, {2}, Coins: {3}", Index, LostCard1, LostCard2, Coins );
        }
    }
}
