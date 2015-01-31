using System;
using System.IO;

namespace DreamBot.Utils
{
    internal class TunnelingContext
    {
        public TunnelingContext(Stream @from, Stream to, byte[] buffer, Action callback)
        {
            From = @from;
            To = to;
            Buffer = buffer;
            Callback = callback;
        }

        public Stream From { get; private set; }
        public Stream To { get; private set; }
        public byte[] Buffer { get; private set; }
        public Action Callback { get; set; }
    }

    internal class StreamTunnel
    {
        private readonly Stream _from;
        private readonly Stream _to;

        public StreamTunnel(Stream from, Stream to)
        {
            _from = @from;
            _to = to;
        }

        public void Start(Action callback)
        {
            var buffer = new byte[8*1024];
            var ctx = new TunnelingContext(_from, _to, buffer, callback);
            _from.BeginRead(buffer, 0, buffer.Length, OnEndReceiveFrom, ctx);
        }

        private static void OnEndReceiveFrom(IAsyncResult ar)
        {
            var ctx = ar.AsyncState as TunnelingContext;
            try
            {
                var read = ctx.From.EndRead(ar);
                if(read <= 0)
                {
                    CloseStream(ctx.From);
                    ctx.Callback();
                    return;
                }
                ctx.To.BeginWrite(ctx.Buffer, 0, read, OnEndWriteTo, ctx );
            }
            catch (Exception)
            {
                CloseStream(ctx.From);
                ctx.Callback();
            }
        }

        private static void OnEndWriteTo(IAsyncResult ar)
        {
            var ctx = ar.AsyncState as TunnelingContext;
            try
            {
                ctx.To.EndWrite(ar);

                ctx.From.BeginRead(ctx.Buffer, 0, ctx.Buffer.Length, OnEndReceiveFrom, ctx);
            }
            catch (Exception)
            {
                ctx.To.Flush();
                CloseStream(ctx.To);
            }
        }

        private static void CloseStream(Stream stream)
        {
            stream.Close();
            stream.Dispose();
        }

    }
}
