namespace Mastermind.Models
{
    public class Game
    {
        public string GameId { get; set; }
        public string FirstPlayerId { get; set; } 
        public Player FirstPlayer { get; set; }
        public Player SecondPlayer { get; set; }
        public List<Guess> Guesses { get; set; }
        public List<string> SecretCode { get; set; }
        public int MaxAttempts { get; set; }
        public bool GameOver { get; set; }
        public bool Won { get; set; }

    }
}
