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
        public async Task SendAsync(string message)
        {
            if (pipe == null || !pipe.IsConnected)
            {
                if (PipeDisconnected == false)
                {
                    PipeDisconnected = true;
                }
                return;
            }
            else if (PipeDisconnected == true)
            {
                PipeDisconnected = false;
            }

            try
            {
                if (writer != null)
                {
                    try { writer.WriteLine(message); }
                    catch { }
                }
            }
            catch (IOException)
            {
                pipe?.Dispose();
            }
        }
    }
}
