using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;


namespace Mastermind.Models
{
    public class MastermindContext : DbContext
    {
        public MastermindContext(DbContextOptions<MastermindContext> options)
        : base(options)
        {
        }
        public DbSet<Game> Games { get; set; }
        public DbSet<Player> Players { get; set; }
        public DbSet<Guess> Guesses { get; set; }
        public DbSet<GuessResult> GuessResult { get; set; }
        public DbSet<Score> Scores{ get; set; }

        //public MastermindDbContext(DbContextOptions<MastermindDbContext> options)
        //    : base(options)
        //{
        //}

        
    }
}
