using System;
using System.Net.NetworkInformation;
using DreamBot.Workers;

namespace DreamBot
{
    internal class ConnectivityTester
    {
        private bool _hasInternetAccess;
        private readonly string[] _ip2Ping = new[] {
            "4.2.2.1", "4.2.2.2", 
            "4.2.2.3", "4.2.2.4", 
            "4.2.2.5", "4.2.2.6", 
            "8.8.8.8", "8.8.4.4", 
            "208.67.222.222"
        };
        private int _nextIp;
        private readonly TimedWorker _worker;
        public EventHandler<EventArgs> OnConnectivityStatusChanged;
 
        public ConnectivityTester()
        {
            _worker = new TimedWorker();
            _worker.QueueForever(TestInternetAccess, TimeSpan.FromSeconds(5));
            _worker.Start();
        }

        public bool IsConnected
        {
            get { return _hasInternetAccess;  }
        }

        private void TestInternetAccess()
        {
            var ping = new Ping();
            ping.SendAsync(_ip2Ping[_nextIp], 1000);
            if((++_nextIp) >= _ip2Ping.Length) _nextIp = 0;
 
            ping.PingCompleted += (sender, args) => {
                var currConnectionStatus = args.Reply.Status == IPStatus.Success;
                var prevConnectionStatus = _hasInternetAccess;
                _hasInternetAccess = currConnectionStatus;
                if (currConnectionStatus == prevConnectionStatus) return;

                var handler = OnConnectivityStatusChanged;
                if(handler!=null) handler(this, new EventArgs());
            };
        }
    }
}