using System.Threading.Channels;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<MyQueue>();
builder.Services.AddHostedService<MyQueueProcessor>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();


app.MapPost("/api/channel", async (string description, MyQueue queue, CancellationToken cancellation) =>
{

    await queue.Writer.WriteAsync(new MyEvent() { Description = description }, cancellation);
    return Results.Created();
});


app.Run();


public sealed class MyQueueProcessor(MyQueue queue) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var @event in queue.Reader.ReadAllAsync(stoppingToken))
        {
            await Task.Delay(1000, stoppingToken);
            Console.WriteLine(@event.Description);
        }
    }
}
public class MyEvent
{
    public string Description { get; set; }
}

public class MyQueue
{
    //private readonly Channel<MyEvent> _channel2 = Channel.CreateBounded<MyEvent>(10);
    private readonly Channel<MyEvent> _channel = Channel.CreateUnbounded<MyEvent>();

    public ChannelWriter<MyEvent> Writer => _channel.Writer;

    public ChannelReader<MyEvent> Reader => _channel.Reader;

}