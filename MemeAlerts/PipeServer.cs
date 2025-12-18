using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemeAlerts
{
    public class PipeServer
    {
        public string PipeName = "MyOverlayPipe";
        private NamedPipeClientStream pipe;
        private CancellationTokenSource cts = new CancellationTokenSource();
        private StreamWriter writer;
        public PipeServer(string pipeName)
        {
            PipeName = pipeName;
            _ = Task.Run(() => MonitorConnectionAsync(cts.Token));
        }
        private async Task MonitorConnectionAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    if (pipe == null || !pipe.IsConnected)
                    {
                        pipe?.Dispose();
                        pipe = new NamedPipeClientStream(".", PipeName, PipeDirection.Out);

                        await pipe.ConnectAsync(2000, token);
                        writer = new StreamWriter(pipe) { AutoFlush = true };

                    }

                    await Task.Delay(1000, token);
                }
                catch (System.TimeoutException)
                {

                    await Task.Delay(2000, token);
                }
                catch (IOException)
                {

                    pipe?.Dispose();
                    await Task.Delay(2000, token);
                }
                catch (System.Exception ex)
                {

                    await Task.Delay(2000, token);
                }
            }
        }
        bool PipeDisconnected = false;
        public async Task<bool> SendAsync(string message)
        {
            if (pipe == null || !pipe.IsConnected)
            {
                if (PipeDisconnected == false)
                {
                    PipeDisconnected = true;
                }
                return false;
            }
            else if (PipeDisconnected == true)
            {
                PipeDisconnected = false;
            }

            try
            {
                if (writer != null)
                {
                    try 
                    { 
                        writer.WriteLine(message);
                        return true;
                    }
                    catch (Exception ex)
                    {
                        File.AppendAllText("error.log", $"[{DateTime.Now}] {ex.Message}\r\n{ex.StackTrace}");
                        return false;
                    }
                }
                else
                {
                    File.AppendAllText("error.log", $"[{DateTime.Now}] Writer is null");
                    return false;
                }
            }
            catch (IOException ex)
            {
                pipe?.Dispose();
                File.AppendAllText("error.log", $"[{DateTime.Now}] {ex.Message}\r\n{ex.StackTrace}");
                return false;
            }

        }
    }
}
