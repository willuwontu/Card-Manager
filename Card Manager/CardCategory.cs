namespace R3DCore
{
    public struct CardCategory
    {
        public readonly string name;

        public CardCategory(string name)
        {
            this.name = name;
        }

        private bool Equals(CardCategory cardCategory)
        {
            return cardCategory.name == this.name;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            if (obj is CardCategory cardCategory)
            {
                return this.Equals(cardCategory);
            }

            return false;
        }

        public override int GetHashCode()
        {
            return name.GetHashCode();
        }

        public override string ToString()
        {
            return $"CardCategory: {this.name}";
        }
    }
}
