using System;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;

namespace IpSwitcher2;

internal sealed class Program
{
    // Set variables for single instance
    private static readonly Mutex Mutex = new(true, "{b3fa3552-9bbc-4817-bae0-f971c1dca5ee}");
    private const string PipeName = "IpSwitcherPipe";
    private static CancellationTokenSource? _cancellationTokenSource;
    
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        // Get platform-specific pipe name
        
        // Check if there's already an instance running
        if (Mutex.WaitOne(TimeSpan.Zero, true))
        {
            // First instance: start pipe server and release mutex
            Console.WriteLine("Starting as first instance");
            _cancellationTokenSource = new CancellationTokenSource();
            
            Task.Run(ListenForConnections, _cancellationTokenSource.Token);
            Mutex.ReleaseMutex();
            
            BuildAvaloniaApp()
                .StartWithClassicDesktopLifetime(args);
            
            // Clean up at exit
            _cancellationTokenSource.Cancel();
        }
        else
        {
            Console.WriteLine("Starting as second instance");
            // Second instance: connect to pipe and send message
            try
            {
                const string message = "Please show!";
                SendMessageToFirstInstance(message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while connecting to pipe: {ex.Message}");
            }
            
            Environment.Exit(0);
        }
    }

    private static void ShowMainWindow()
    {
        if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop) return;
        var mainWindow = desktop.MainWindow;
        if (mainWindow == null) return;
        mainWindow.Show();
        mainWindow.Activate();
        if (mainWindow.WindowState == WindowState.Minimized)
        {
            mainWindow.WindowState = WindowState.Normal;
        }
        mainWindow.BringIntoView();
    }

    private static async Task? ListenForConnections()
    {
        Console.WriteLine("Listening for connections...");

        while (!_cancellationTokenSource?.Token.IsCancellationRequested ?? false)
        {
            NamedPipeServerStream? pipeServer = null;
            try
            {
                pipeServer = new NamedPipeServerStream(
                    PipeName,
                    PipeDirection.In,
                    1,
                    PipeTransmissionMode.Byte,
                    PipeOptions.Asynchronous);

                // Wait for a connection
                Console.WriteLine("Waiting for connection...");
                await pipeServer.WaitForConnectionAsync(_cancellationTokenSource.Token);
                Console.WriteLine("Connected!");

                // Read the message
                using var reader = new StreamReader(pipeServer);
                var message = await reader.ReadLineAsync();

                if (!string.IsNullOrEmpty(message))
                {
                    Console.WriteLine($"Received message: {message}");
                    // TODO: Show window
                    await Dispatcher.UIThread.InvokeAsync(ShowMainWindow);
                }

                // Disconnect and reconnect the pipe for the next client
                pipeServer.Disconnect();
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Operation cancelled");
                break;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while listening for connections: {ex.Message}");
                await Task.Delay(1000);
            }
            finally
            {
                if (pipeServer != null)
                {
                    if (pipeServer.IsConnected)
                        pipeServer.Disconnect();
                    await pipeServer.DisposeAsync();
                }
            }
        }
    }

    private static void SendMessageToFirstInstance(string message)
    {
        Console.WriteLine($"Attempting to connect to first instance...");
        using var pipeClient = new NamedPipeClientStream(".", PipeName, PipeDirection.Out);
        
        Console.WriteLine($"Connecting to pipe...");
        pipeClient.Connect(3000);
        Console.WriteLine($"Connected to pipe");
        
        using var writer = new StreamWriter(pipeClient);
        writer.AutoFlush = true;
        writer.WriteLine(message);
        Console.WriteLine($"Sent message to first instance: {message}");
    }
    
    
    
    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
    {
        return AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
    }
}