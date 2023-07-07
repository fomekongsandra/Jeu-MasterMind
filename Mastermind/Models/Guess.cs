namespace Mastermind.Models
{
    public class Guess
    {
        public string PlayerId { get; set; }
        public string GameId { get; set; }
        public List<string> Colors { get; set; }
        public int CorrectPositions { get; set; }
        public int CorrectColors { get; set; }
        public Player Player { get; set; }

    }
}
