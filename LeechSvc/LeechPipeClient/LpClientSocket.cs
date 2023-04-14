using LeechPipe;
using LeechSvc.Logger;
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
        private readonly ILogger _logger;

        public LpClientSocket(ILogger logger)
        {
            _cts = new CancellationTokenSource();
            _recvCts = new CancellationTokenSource();
            _client = new HttpClient();
            _logger = logger;
        }

        public void Cancel()
        {
            _cts.Cancel();
        }

        public async Task<ConnectResult> ConnectAsync(string url, string login, string password)
        {
            string httpProto;
            string wsProto;
            string urlBody;
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
                    new KeyValuePair<string, string>("password", password)
                };
            var content = new FormUrlEncodedContent(pairs);

            AuthUser authUser;
            using (var client = new HttpClient())
            {
                try
                {
                    var response = await client.PostAsync(httpProto + urlBody + "/auth/user", content);
                    if (!response.IsSuccessStatusCode) return new ConnectResult(false, response.ReasonPhrase);
                    var result = await response.Content.ReadAsStringAsync();
                    authUser = JsonConvert.DeserializeObject<AuthUser>(result);
                    if (authUser == null) return new ConnectResult(false, "Ошибка аутентификации");
                }
                catch (Exception ex)
                {
                    _logger.AddException("LpClientSocket:ConnectAsync:Auth", ex);
                    return new ConnectResult(false, ex.Message);
                }
            }

            try
            {
                _socket = new ClientWebSocket();
                var uri = new Uri(wsProto + urlBody + "/ws");
                _socket.Options.SetRequestHeader("Authorization", "Bearer " + authUser.Token);
                await _socket.ConnectAsync(uri, _cts.Token);
            }
            catch(Exception ex)
            {
                _logger.AddException("LpClientSocket:ConnectAsync:Connect", ex);
                return new ConnectResult(false, ex.Message);
            }

            return new ConnectResult(true);
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

            try
            {
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
            catch(Exception ex)
            {
                _logger.AddException("LpClientSocket:RecvMessageAsync", ex);
                return null;
            }
        }

        public async Task SendMessageAsync(byte[] buffer)
        {
            try
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
            catch(Exception ex)
            {
                _logger.AddException("LpClientSocket:SendMessageAsync", ex);
            }
        }
    }

    public class ConnectResult
    {
        public bool IsSuccess { get; private set; }
        public string Message { get; private set; }

        public ConnectResult(bool isSuccess, string message = "")
        {
            IsSuccess = isSuccess;
            Message = message;
        }
    }
}
