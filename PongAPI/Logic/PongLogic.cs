using Microsoft.AspNetCore.SignalR;
using PongAPI.Hubs;
using static System.Random;

namespace PongAPI.Logic
{
	public class PongLogic
	{
		public static Dictionary<string, string> PlayerConnections { get; private set; } = new();
		public static Dictionary<string, int> PlayerScores { get; private set; } = new();
		public static bool isGameActive { get; private set; } = false;
		public static double[] PosBall { get; private set; } = [49, 47]; // Max Left - 97.5%, Max Top - 95.5%
		public static double[] VelBall { get; private set; } = [0.5 , 0.5];
		public static int[] PosPlayers { get; private set; } = [40, 40];
		private static IHubContext<PongHub>? HubContext;
		private static System.Timers.Timer? BallTimer;
		private static Random random = new Random();

		public static void Initialize(IHubContext<PongHub> hubContext)
		{
			HubContext = hubContext;
		}

		public static async Task GetPlayerQueue()
		{
			await HubContext?.Clients.All.SendAsync("UpdateLeaderboard_PONG", PlayerScores);
		}

		public static async Task RegisterPlayer(string username, string connectionId)
		{
			if (!PlayerConnections.ContainsKey(connectionId) && !PlayerScores.ContainsKey(username) && PlayerScores.Count < 2 && !isGameActive)
			{
				PlayerConnections[connectionId] = username;
				PlayerScores[username] = 0;
				await HubContext?.Clients.All.SendAsync("UpdateLeaderboard_PONG", PlayerScores);

				if (PlayerScores.Count == 2)
				{
					await Task.Delay(3000);
					await StartGame();
				}
			}
		}

		public static async Task RemovePlayer(string connectionId)
		{
			if (PlayerConnections.ContainsKey(connectionId) && !isGameActive)
			{
				PlayerScores.Remove(PlayerConnections[connectionId]);
				PlayerConnections.Remove(connectionId);
				await HubContext?.Clients.All.SendAsync("UpdateLeaderboard_PONG", PlayerScores);
			}
		}

		public static async Task IncrementScore(int player_id)
		{
			if (isGameActive)
			{
				PlayerScores[PlayerScores.ElementAt(player_id).Key]++;
				await HubContext?.Clients.All.SendAsync("UpdateLeaderboard_PONG", PlayerScores);

				if (PlayerScores[PlayerScores.ElementAt(player_id).Key] == 5)
				{
					isGameActive = false;
					PosPlayers = [40, 40];

					PlayerConnections.Clear();
					PlayerScores.Clear();

					await HubContext?.Clients.All.SendAsync("GameOver_PONG", PlayerScores);
				}
			}
		}

		public static async Task MoveUp(int player_id)
		{
			if (isGameActive)
			{
				if (PosPlayers[player_id] > 0)
				{
					PosPlayers[player_id] -= 5;
					await HubContext?.Clients.All.SendAsync("MovePaddle_PONG", player_id, PosPlayers[player_id]);
				}
			}
		}

		public static async Task MoveDown(int player_id)
		{
			if (isGameActive)
			{
				if (PosPlayers[player_id] < 80)
				{
					PosPlayers[player_id] += 5;
					await HubContext?.Clients.All.SendAsync("MovePaddle_PONG", player_id, PosPlayers[player_id]);
				}
			}
		}

		public static async Task StartGame()
		{
			if (!isGameActive && PlayerScores.Count > 0)
			{
				isGameActive = true;

				if (random.Next(2) == 0) // Starting X
				{
					VelBall[0] *= -1; // Left
				}

				VelBall[1] = (double) random.Next(1, 5) / 10; // Starting Y

				if (random.Next(2) == 0) // Starting Y
				{
					VelBall[1] *= -1; // Down
				}

				await HubContext?.Clients.All.SendAsync("GameStarted_PONG");
				await Task.Delay(3000);

				BallTimer = new System.Timers.Timer(10); // 60 fps
				BallTimer.Elapsed += async (sender, e) => await BallHandler();
				BallTimer.AutoReset = true;
				BallTimer.Start();

			}
		}

		public static async Task BallHandler()
		{
			if (isGameActive)
			{

				PosBall[0] += VelBall[0];
				PosBall[1] += VelBall[1];

				await HubContext?.Clients.All.SendAsync("MoveBall_PONG", PosBall);

				if (PosBall[1] <= 0 || PosBall[1] >= 95.5)
				{
					VelBall[1] *= -1;
				}

				if ((PosBall[0] == 5 && (PosBall[1] <= PosPlayers[0] + 17.5 && PosBall[1] >= PosPlayers[0] - 3)) || PosBall[0] == 92.5 && (PosBall[1] <= PosPlayers[1] + 17.5 && PosBall[1] >= PosPlayers[1] - 3))
				{
					VelBall[0] *= -1;
				}

				if (PosBall[0] <= 0 || PosBall[0] >= 97.5)
				{
					if (PosBall[0] <= 0) await IncrementScore(1);
					if (PosBall[0] >= 97.5) await IncrementScore(0);

					BallTimer?.Stop();

					PosBall[0] = 49;
					PosBall[1] = 47;
					VelBall[0] = 0.5;
					VelBall[1] = 0.5;
					
					await HubContext?.Clients.All.SendAsync("MoveBall_PONG", PosBall);
					await Task.Delay(3000);

					if (random.Next(2) == 0)
					{
						VelBall[0] *= -1;
					}

					VelBall[1] = (double) random.Next(1, 5) / 10;

					if (random.Next(2) == 0)
					{
						VelBall[1] *= -1;
					}

					if (isGameActive) BallTimer?.Start();

				}

			}
			else
			{
				BallTimer?.Stop();
			}
		}

	}
}
