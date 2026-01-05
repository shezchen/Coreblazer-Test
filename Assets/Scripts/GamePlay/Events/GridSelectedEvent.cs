namespace GamePlay.Events
{
    public record GridSelectedEvent
    {
        public (int, int) Position;
        public int Content;

        public GridSelectedEvent((int, int) position, int content)
        {
            Position = position;
            Content = content;
        }
    }

    public record GridDeSelectedEvent
    {
        public (int, int) Position;

        public GridDeSelectedEvent((int, int) position)
        {
            Position = position;
        }
    }
}