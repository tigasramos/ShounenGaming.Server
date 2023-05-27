using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using MingweiSamuel.Camille.LolChallengesV1;
using ShounenGaming.Business.Interfaces.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShounenGaming.Business.Hubs
{
    public interface ILobbiesHubClient
    {
        Task MessageReceived(string message);
        Task LobbyUpdated();
        Task UpdateLobbiesList(List<LobbyInfo> lobbies);
    }

    [Authorize]
    public class LobbiesHub : Hub<ILobbiesHubClient>
    {
        private static readonly string GROUPLESS_NAME = "NO_GROUP";
        private readonly GameManager _gameManager;

        public LobbiesHub( )
        {
        }
        public override async Task OnConnectedAsync()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, GROUPLESS_NAME);
            await base.OnConnectedAsync();
        }
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            //If any lobby remove 
            //otherwise
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, GROUPLESS_NAME);
            await base.OnDisconnectedAsync(exception);
        }
        public async Task CreateLobby()
        {
            var lobby = _gameManager.CreateLobby();
            await Groups.AddToGroupAsync(Context.ConnectionId, lobby.Name);
        }
        public async Task UpdateLobby()
        {
            var lobby = _gameManager.ChangeLobby();

            //Let connected members
            await Clients.Groups(lobby.Name).LobbyUpdated();

            //Everyone gets the all lobbies update
            //await Clients.Groups(GROUPLESS_NAME).UpdateLobbiesList();
        }

        public void SendMessage(string message)
        {
           // _gameManager.SendMessage(, message);
        }
        
       
    }

    public class LobbyInfo
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public int CurrentMemberCount { get; set; }
        public int? MaxSize { get; set; }
        public int OwnerId { get; set; }
        public string OwnerNickname { get; set; }
        public bool Locked { get; set; }
    }

    public class Lobby
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public int? MaxSize { get; set; }

        public int OwnerId { get; set; }
        public string OwnerNickname { get; set; }

        public string Password { get; set; }
        public bool Locked { get; set; }

        public LobbyGame GameType { get; set; }
        public Game? Game { get; set; }
        public bool InGame => Game != null;

        public List<int> PlayersIds { get; set; }
        public List<int> SpectatorsIds { get; set; }

        public DateTime CreatedAt { get; set; }
    }

    public enum LobbyGame
    {
        NONE,
        ROCK_PAPER_SCISSORS,
        TIC_TAC_TOE,
        CARDS_AGAINST_HUMANITY,
        GARTIC,
        NAVAL_SHIP,
        CONNECT_4,
        CHESS,
        GUESS_THE_SONG,
        BINGO,
        MEMORY_GAME,
    }

    /*
     * UpdateGameState -> Send everyone
     * Play -> Client send to server
     * 
     */

    public abstract class Game 
    { 
        public abstract void Play(int playerId, Play play);
        public abstract void ResetGame(); 
    }
    public abstract class RoundGame : Game
    {
        protected int currentRound;
        protected int nRounds; 
        public abstract void EndRound(); 
    }

    public abstract class Play { }
    public class RockPaperScissorsGame : RoundGame
    {
        public static readonly int GAME_MAX_SIZE = 2;
        public static readonly int GAME_MIN_SIZE = 2;
        enum RockPaperScissorsGameState
        {
            PLAYING, FINISHED
        }
        enum RockPaperScissorsOptions
        {
            ROCK,
            PAPER,
            SCISSORS
        }
        enum RockPaperScissorsGameResult
        {
            DRAW, PLAYER_1_WINS, PLAYER_2_WINS, NO_WINNER
        }
        class RockPaperScissorsPlay : Play
        {
            public RockPaperScissorsOptions Option { get; set; }
        }

        
        private Dictionary<int, RockPaperScissorsOptions?> playersOptions;

        private List<int> players;
        private DateTime timeToFinishPlay;
        private RockPaperScissorsGameState state;
        private RockPaperScissorsGameResult result;

        //int - rounds
        public RockPaperScissorsGame(List<int> players)
        {
            if (players.Count < GAME_MIN_SIZE)
                throw new Exception("Needs more players");
            if (players.Count > GAME_MAX_SIZE)
                throw new Exception("Too many players");

            this.players = players; 
            playersOptions = new Dictionary<int, RockPaperScissorsOptions?>();
        }


        
        public override void Play(int playerId, Play play)
        {
            if (!players.Contains(playerId)) return;

            var rpsPlay = (RockPaperScissorsPlay)play;

            //Game not started
            if (state == RockPaperScissorsGameState.FINISHED) return;

            //Game should be over
            if (DateTime.UtcNow > timeToFinishPlay) EndRound();


            //Make a Play
            if (!playersOptions.ContainsKey(playerId))
                playersOptions.Add(playerId, null);

            playersOptions[playerId] = rpsPlay.Option;

            //Both played then finish
            if (players.Count == playersOptions.Count) EndRound(); 
        }

        public override void ResetGame()
        {
            if (state != RockPaperScissorsGameState.FINISHED) return;

            playersOptions = new Dictionary<int, RockPaperScissorsOptions?>();
            timeToFinishPlay = DateTime.UtcNow.AddSeconds(30);

            Task.Delay(timeToFinishPlay.Subtract(DateTime.UtcNow)).ContinueWith((_) => {
                EndRound();
            });
        }

        public override void EndRound()
        {
            if (state != RockPaperScissorsGameState.PLAYING) return;
            state = RockPaperScissorsGameState.FINISHED;
            CalculateWinner();
        }

        private void CalculateWinner()
        {
            var player1Play = playersOptions[0];
            var player2Play = playersOptions[1];

            if (!player1Play.HasValue && !player2Play.HasValue) 
                result = RockPaperScissorsGameResult.NO_WINNER;

            if (player1Play.HasValue && !player2Play.HasValue)
                result = RockPaperScissorsGameResult.PLAYER_1_WINS;

            if (!player1Play.HasValue && player2Play.HasValue)
                result = RockPaperScissorsGameResult.PLAYER_2_WINS;

            if (player1Play == player2Play)
                result = RockPaperScissorsGameResult.DRAW;


            if (player1Play == RockPaperScissorsOptions.ROCK)
            {
                if (player2Play == RockPaperScissorsOptions.PAPER)
                    result = RockPaperScissorsGameResult.PLAYER_2_WINS;
                else result = RockPaperScissorsGameResult.PLAYER_1_WINS;
            }

            if (player1Play == RockPaperScissorsOptions.PAPER)
            {
                if (player2Play == RockPaperScissorsOptions.SCISSORS)
                    result = RockPaperScissorsGameResult.PLAYER_2_WINS;
                else result = RockPaperScissorsGameResult.PLAYER_1_WINS;
            }

            if (player1Play == RockPaperScissorsOptions.SCISSORS)
            {
                if (player2Play == RockPaperScissorsOptions.ROCK)
                    result = RockPaperScissorsGameResult.PLAYER_2_WINS;
                else result = RockPaperScissorsGameResult.PLAYER_1_WINS;
            }
        }
    }

    public class GuessTheSongGame : RoundGame
    {
        public static readonly int GAME_MIN_SIZE = 2;
        public static readonly int? GAME_MAX_SIZE = null;

        enum GuessTheSongGameState
        {
            SELECTING_SONG, GUESSING_THE_SONG, FINISHED
        }
        enum GuessTheSongPlayType
        {
            CHOOSING_SONG, SUBMITING_SONG, GUESSING_SONG
        }
        class GuessTheSongPlay : Play
        {
            public GuessTheSongPlayType Type { get; set; }
            public string SongName { get; set; }
        }

        private int? playerIdSongChooser;
        private string? currentGuessingSong;
        private DateTime playStartedAt;
        private DateTime playEndsAt;

        private readonly List<int> players;
        private Dictionary<int, string> playersRoundSongs;
        private int roundGuessers;
        private Dictionary<int, int> totalScore;
        private GuessTheSongGameState state;

        public override void Play(int playerId, Play play)
        {
            var gtsPlay = (GuessTheSongPlay)play;

            //Game not started
            if (state == GuessTheSongGameState.FINISHED) return;

            //Game should be over
            if (DateTime.UtcNow > playEndsAt) EndRound();
            

            //Player not playing
            if (!players.Contains(playerId)) return;

            if (state == GuessTheSongGameState.GUESSING_THE_SONG)
            {
                if (currentGuessingSong == gtsPlay.SongName)
                {
                    //TODO: Change Formula
                    var points = Convert.ToInt32((playEndsAt - playStartedAt).TotalSeconds);

                    if (!totalScore.ContainsKey(playerId)) 
                        totalScore.Add(playerId, 0);

                    totalScore[playerId] += points;
                    roundGuessers++;
                }
            }

            if (state == GuessTheSongGameState.SELECTING_SONG)
            {
                if (!playersRoundSongs.ContainsKey(playerId))
                    playersRoundSongs.Add(playerId, "");

                //TODO: Think about this, here only submits , it doesnt show options
                playersRoundSongs[playerId] = gtsPlay.SongName;


                if (playersRoundSongs.Count == players.Count) EndRound();
                
            }

        }

        public override void ResetGame()
        {
            throw new NotImplementedException();
        }

        public override void EndRound()
        {
            if (state == GuessTheSongGameState.SELECTING_SONG)
            {
                state = GuessTheSongGameState.GUESSING_THE_SONG;

                //Case nobody selects a song
                if (playersRoundSongs.Count < 1)
                {
                    playStartedAt = DateTime.UtcNow;
                    playEndsAt = DateTime.UtcNow.AddSeconds(90);

                    Task.Delay(playEndsAt.Subtract(DateTime.UtcNow)).ContinueWith(_ =>
                    {
                        if (state == GuessTheSongGameState.SELECTING_SONG) EndRound();
                    });
                }

                PrepareNextGuess();

            } 
            else if (state == GuessTheSongGameState.GUESSING_THE_SONG)
            {
                //Give score to Song Chooser
                if (!totalScore.ContainsKey(playerIdSongChooser!.Value))
                    totalScore.Add(playerIdSongChooser!.Value, 0);
                totalScore[playerIdSongChooser!.Value] += roundGuessers / (players.Count - 1);


                //Select Songs again
                if (playersRoundSongs.Last().Key == playerIdSongChooser)
                {
                    state = GuessTheSongGameState.SELECTING_SONG;
                    currentRound++;
                    playersRoundSongs = new Dictionary<int, string>();
                    currentGuessingSong = null;
                    playerIdSongChooser = null;
                    playStartedAt = DateTime.UtcNow;
                    playEndsAt = DateTime.UtcNow.AddSeconds(90);

                    Task.Delay(playEndsAt.Subtract(DateTime.UtcNow)).ContinueWith(_ =>
                    {
                        if (state == GuessTheSongGameState.SELECTING_SONG) EndRound();
                    });
                } 
                else
                {
                    PrepareNextGuess();
                } 

            }
        }

        private void PrepareNextGuess()
        {
            var playersOrder = playersRoundSongs.Keys.ToList();
            playerIdSongChooser = playersOrder[playersOrder.IndexOf(playerIdSongChooser!.Value) + 1];
            currentGuessingSong = playersRoundSongs[playerIdSongChooser!.Value];
            playStartedAt = DateTime.UtcNow;
            playEndsAt = DateTime.UtcNow.AddSeconds(30);
            roundGuessers = 0;
        }
    }
    
    public class MemoryGame : Game
    {
        class MemoryGamePlay : Play
        {
            public int CardIndex { get; set; }
        }
        class MemoryGameCard
        {
            public enum MemoryGameCardState
            {
                ELIMINATED,
                SHOWING,
                TURNED
            }
            public MemoryGameCardState State { get; set; }
            public int CardImage { get; set; }
        }

        private int currentPlayerTurnIndex;

        private List<int> players;
        private Dictionary<int, int> totalScores;
        private List<MemoryGameCard> shuffledCards;

        public override void Play(int playerId, Play play)
        {
            if (playerId != players[currentPlayerTurnIndex]) return;

            var mgPlay = (MemoryGamePlay)play;

            var showingCards = shuffledCards.Where(c => c.State == MemoryGameCard.MemoryGameCardState.SHOWING).ToList();
            
            //First Card
            if (showingCards.Count == 0)
            {
                var firstCard = shuffledCards[mgPlay.CardIndex];
                firstCard.State = MemoryGameCard.MemoryGameCardState.SHOWING;

                //TODO: Send Info Back
            }
            //Second Card
            else if (showingCards.Count == 1)
            {
                var firstCard = showingCards[0];
                var secondCard = shuffledCards[mgPlay.CardIndex];
                secondCard.State = MemoryGameCard.MemoryGameCardState.SHOWING;

                //TODO: Send Info Back

                Task.Delay(2000).ContinueWith(_ =>
                {
                    if (firstCard.CardImage == secondCard.CardImage)
                    {
                        //Won
                        shuffledCards.ForEach(c =>
                        {
                            if (c.State == MemoryGameCard.MemoryGameCardState.SHOWING)
                                c.State = MemoryGameCard.MemoryGameCardState.ELIMINATED;
                        });

                        var playerId = players[currentPlayerTurnIndex];
                        if (!totalScores.ContainsKey(playerId))
                            totalScores.Add(playerId, 0);
                        totalScores[playerId]++;
                    }
                    else
                    {
                        //Lost
                        shuffledCards.ForEach(c =>
                        {
                            if (c.State == MemoryGameCard.MemoryGameCardState.SHOWING)
                                c.State = MemoryGameCard.MemoryGameCardState.TURNED;

                        });
                        currentPlayerTurnIndex++;
                    }

                    //TODO: Send Info Back
                });
            }

        }

        public override void ResetGame()
        {
            throw new NotImplementedException();
        }
    }



    public class GameManager
    {
        private List<Lobby> lobbies;
        private readonly IHubContext<LobbiesHub, ILobbiesHubClient> _lobbiesHub;

        public Lobby CreateLobby()
        {
            throw new NotImplementedException();
        }
        public void DeleteLobby()
        {
            throw new NotImplementedException();
        }
        public Lobby ChangeLobby()
        {
            throw new NotImplementedException();
        }
        public void EnterLobby()
        {
            throw new NotImplementedException();
        }
        public void LeaveLobby()
        {
            throw new NotImplementedException();
        }
        public void KickPlayer()
        {
            throw new NotImplementedException();
        }
        public void SendMessage(int userId, string message)
        {
            //Get Lobby from User

            //Send Message to everyone in that group
           // _lobbiesHub.Groups.
        }
        public void StartGame()
        {
            throw new NotImplementedException();
        }
        public void Play()
        {
            //Find Player Lobby

            //Check if its in Game

            //If not error
            //If in game play
        }
        public void SetReadyToPlay()
        {
            throw new NotImplementedException();
        }
        public void RestartGame()
        {
            throw new NotImplementedException();
        }
        public List<LobbyInfo> GetLobbiesInfo()
        {
            throw new NotImplementedException();
        }
        public List<Lobby> GetLobbyById(int id)
        {
            throw new NotImplementedException();
        }
    }
}
