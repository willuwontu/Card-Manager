namespace R3DCore
{
    public class CardInfo
    {
        public readonly CardUpgrade card;
        public readonly string cardName;
        public readonly string modName;
        public readonly bool allowMultiple = true;
        public readonly bool hidden = false;
        public bool enabled = true;
        public int weight;
        public readonly bool canBeReassigned = true;
        public CardCategory[] categories = new CardCategory[0];
        public CardCategory[] blacklistedCategories = new CardCategory[0];
        public string description;

        public CardInfo(CardUpgrade card, string cardName, string modName, int weight, bool canBeReassigned, bool hidden, bool allowMultiple)
        {
            this.card = card;
            this.cardName = cardName;
            this.modName = modName;
            this.weight = weight;
            this.hidden = hidden;
            this.allowMultiple = allowMultiple;
            this.canBeReassigned = canBeReassigned;
        }

        public override string ToString()
        {
            return $"[{modName}] {cardName}";
        }
    }
}
