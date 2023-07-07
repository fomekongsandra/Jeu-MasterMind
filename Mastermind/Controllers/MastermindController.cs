using Mastermind.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

using static System.Formats.Asn1.AsnWriter;

namespace Mastermind.Controllers
{
    [ApiController]
    [Route("api/mastermind")]
    public class MastermindController : ControllerBase
    {
        private readonly MastermindContext _dbContext;

        public MastermindController(MastermindContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpPost("start")]
        public ActionResult<string> StartGame([FromBody] Player player)
        {
            // Vérifier si le nom du premier joueur est fourni
            if (player == null || string.IsNullOrEmpty(player.Name))
                return BadRequest("Le nom du premier joueur est requis.");

            // Créer une nouvelle partie
            Game game = new Game
            {
                GameId = Guid.NewGuid().ToString(),
                FirstPlayer = player,
                SecondPlayer = null,
                SecretCode = null,
                MaxAttempts = 12,
                GameOver = false,
                Won = false,
                Guesses = new List<Guess>()
            };

            // Attribuer la passe au premier joueur et générer la combinaison secrète
            if (string.IsNullOrEmpty(game.FirstPlayerId))
            {
                game.FirstPlayerId = player.PlayerId;
                game.SecretCode = GenerateSecretCode();
            }
            else
            {
                return BadRequest("La passe est déjà attribuée à un autre joueur.");
            }

            // Ajouter la partie à la base de données
            _dbContext.Games.Add(game);
            _dbContext.SaveChanges();

            return Ok(game.GameId);
        }

        [HttpPost("guess")]
        public ActionResult<GuessResult> MakeGuess([FromBody] Guess guess)
        {
            // Vérifier si l'identifiant de la partie est fourni
            if (string.IsNullOrEmpty(guess.GameId))
                return BadRequest("L'identifiant de la partie est requis.");

            // Récupérer la partie correspondante depuis la base de données
            var game = _dbContext.Games.Include(g => g.Guesses).FirstOrDefault(g => g.GameId == guess.GameId);

            if (game == null)
                return NotFound();

            if (game.GameOver)
                return BadRequest("La partie est déjà terminée.");

            if (game.SecondPlayer == null)
            {
                // Attribuer la passe au deuxième joueur
                game.SecondPlayer = guess.Player;

                _dbContext.Games.Update(game);
                _dbContext.SaveChanges();

                return Ok("Le deuxième joueur peut maintenant faire une proposition.");
            }
            else
            {
                if (game.SecondPlayer.PlayerId != guess.Player.PlayerId)
                {
                    int correctPositions = 0;
                    int correctColors = 0;

                    // Comparer la proposition avec la combinaison secrète
                    for (int i = 0; i < guess.Colors.Count; i++)
                    {
                        if (guess.Colors[i] == game.SecretCode[i])
                            correctPositions++;
                        else if (game.SecretCode.Contains(guess.Colors[i]))
                            correctColors++;
                    }

                    GuessResult result = new GuessResult
                    {
                        CorrectPositions = correctPositions,
                        CorrectColors = correctColors
                    };

                    // Enregistrer la proposition du joueur
                    Guess playerGuess = new Guess
                    {
                        PlayerId = guess.Player.PlayerId,
                        GameId = guess.GameId,
                        Colors = guess.Colors,
                        CorrectPositions = correctPositions,
                        CorrectColors = correctColors
                    };

                    game.Guesses.Add(playerGuess);

                    // Vérifier si le joueur a gagné ou si la partie est terminée
                    if (correctPositions == 4)
                    {
                        game.GameOver = true;
                        game.Won = true;
                    }
                    else if (game.Guesses.Count >= game.MaxAttempts)
                    {
                        game.GameOver = true;
                    }

                    // Mettre à jour la partie dans la base de données
                    _dbContext.Games.Update(game);
                    _dbContext.SaveChanges();

                    return Ok(result);
                }
                else
                {
                    return BadRequest("Le premier joueur ne peut pas faire de proposition.");
                }
            }
        }

        [HttpPost("end")]
        public ActionResult EndGame([FromBody] GameResult gameResult)
        {
            // Vérifier si l'identifiant de la partie est fourni
            if (string.IsNullOrEmpty(gameResult.GameId))
                return BadRequest("L'identifiant de la partie est requis.");

            // Récupérer la partie correspondante depuis la base de données
            var game = _dbContext.Games.Include(g => g.Guesses).FirstOrDefault(g => g.GameId == gameResult.GameId);

            if (game == null)
                return NotFound();

            if (game.GameOver && game.Won)
            {
                var winner = game.SecondPlayer;

                // Calculer le score du joueur gagnant
                Score score = new Score
                {
                    PlayerId = winner.PlayerId,
                    GameId = gameResult.GameId,
                    Points = CalculateScore(game.Guesses)
                };

                // Ajouter le score à la base de données
                _dbContext.Scores.Add(score);
                _dbContext.SaveChanges();

                return Ok("Score enregistré avec succès !");
            }

            return BadRequest("La partie n'est pas terminée ou n'a pas été gagnée.");
        }

        private List<string> GenerateSecretCode()
        {
            List<string> colors = new List<string> { "R", "G", "B", "Y", "O", "P" };
            Random random = new Random();
            List<string> secretCode = new List<string>();

            // Générer une combinaison secrète aléatoire de 4 couleurs
            for (int i = 0; i < 4; i++)
            {
                int index = random.Next(colors.Count);
                secretCode.Add(colors[index]);
            }

            return secretCode;
        }

        private int CalculateScore(List<Guess> guesses)
        {
            // Calculer le score en fonction du nombre d'essais
            return (12 - guesses.Count) * 10;
        }
    }
}

