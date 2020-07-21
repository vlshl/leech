using LeechPipe;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LeechSvc.LeechPipeClient
{
    public class LpClientSocket : ILpTransport
    {
        private ClientWebSocket _socket;
        private HttpClient _client;
        private CancellationTokenSource _cts;
        private CancellationTokenSource _recvCts;

        public LpClientSocket()
        {
            _cts = new CancellationTokenSource();
            _recvCts = new CancellationTokenSource();
            _client = new HttpClient();
        }

        public void Cancel()
        {
            _cts.Cancel();
        }

        public async Task<bool> OpenAsync(string url, string login, string password, string accountCode)
        {
            if (_socket != null) return false;

            string httpProto = "";
            string wsProto = "";
            string urlBody = "";
            if (url.StartsWith("http://"))
            {
                httpProto = "http://";
                wsProto = "ws://";
                urlBody = url.Substring(httpProto.Length, url.Length - httpProto.Length);
            }
            else if (url.StartsWith("https://"))
            {
                httpProto = "https://";
                wsProto = "wss://";
                urlBody = url.Substring(httpProto.Length, url.Length - httpProto.Length);
            }
            else
            {
                httpProto = "http://";
                wsProto = "ws://";
                urlBody = url;
            }

            // auth
            var pairs = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("login", login),
                    new KeyValuePair<string, string> ("password", password)
                };
            var content = new FormUrlEncodedContent(pairs);

            AuthUser authUser;
            using (var client = new HttpClient())
            {
                try
                {
                    var response = await client.PostAsync(httpProto + urlBody + "/auth", content);
                    var result = await response.Content.ReadAsStringAsync();
                    authUser = JsonConvert.DeserializeObject<AuthUser>(result);
                }
                catch (Exception ex)
                {
                    return false;
                }
            }

            _socket = new ClientWebSocket();
            var uri = new Uri(wsProto + urlBody + "/ws/" + Convert.ToBase64String(Encoding.UTF8.GetBytes(accountCode)));
            _socket.Options.SetRequestHeader("Authorization", "Bearer " + authUser.Token);
            await _socket.ConnectAsync(uri, _cts.Token);

            return _socket.State == WebSocketState.Open;
        }

        public async Task<bool> CloseAsync()
        {
            if (_socket == null) return false;

            _recvCts.Cancel();
            try
            {
                await _socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", _cts.Token);
            }
            catch (Exception ex)
            {
            }

            bool isSuccess = _socket.State == WebSocketState.Closed;
            _socket = null;

            return isSuccess;
        }

        public async Task<byte[]> RecvMessageAsync()
        {
            byte[] buffer = new byte[LpCore.SEGMENT_SIZE];
            WebSocketReceiveResult res;

            using (var ms = new MemoryStream())
            {
                do
                {
                    ArraySegment<byte> segm = new ArraySegment<byte>(buffer, 0, buffer.Length);
                    res = await _socket.ReceiveAsync(segm, _recvCts.Token);
                    ms.Write(buffer, 0, res.Count);
                } while (!res.EndOfMessage);

                return ms.ToArray();
            }
        }

        public async Task SendMessageAsync(byte[] buffer)
        {
            int offset = 0;
            while (offset < buffer.Length)
            {
                int restLen = buffer.Length - offset;
                bool endOfMessage = restLen <= LpCore.SEGMENT_SIZE;
                int count = endOfMessage ? restLen : LpCore.SEGMENT_SIZE;

                await _socket.SendAsync(new ArraySegment<byte>(buffer, offset, count),
                    WebSocketMessageType.Binary, endOfMessage, _cts.Token);

                offset += count;
            }
        }
    }
}
