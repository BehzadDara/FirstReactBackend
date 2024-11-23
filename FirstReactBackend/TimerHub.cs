using Microsoft.AspNetCore.SignalR;

namespace FirstReactBackend;

public class TimerHub : Hub
{
    public async Task StartTimer(int time)
    {
        for (int i = 0; i < time; i++)
        {
            await Clients.Caller.SendAsync("TimerUpdate", i);
            await Task.Delay(1000);
        }

        await Clients.Caller.SendAsync("TimerUpdate", time);
        await Clients.Caller.SendAsync("TimerComplete", "Timer finished!");
    }
}
