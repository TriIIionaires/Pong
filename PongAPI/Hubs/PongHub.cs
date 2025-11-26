using Microsoft.AspNetCore.SignalR;
using PongAPI.Logic;

namespace PongAPI.Hubs
{
	public class PongHub : Hub
	{
		public override async Task OnConnectedAsync()
		{
			PongLogic.Initialize(Context.GetHttpContext().RequestServices.GetRequiredService<IHubContext<PongHub>>());
			await base.OnConnectedAsync();
		}

		public override async Task OnDisconnectedAsync(Exception? exception)
		{
			await PongLogic.RemovePlayer(Context.ConnectionId);
			await base.OnDisconnectedAsync(exception);
		}

		public async Task GetPlayerQueue()
		{
			await PongLogic.GetPlayerQueue();
		}

		public async Task RegisterPlayer(string username)
		{
			await PongLogic.RegisterPlayer(username, Context.ConnectionId);
		}

		public async Task MoveUp(int player_id)
		{
			await PongLogic.MoveUp(player_id);
		}

		public async Task MoveDown(int player_id)
		{
			await PongLogic.MoveDown(player_id);
		}

		public async Task StartGame()
		{
			await PongLogic.StartGame();
		}
	}
}
