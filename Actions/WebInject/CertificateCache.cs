using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Threading;

namespace DreamBot.Actions.WebInject
{
    public class CertificateCache
    {
        private readonly Dictionary<string, X509Certificate2> _certServerCache = new Dictionary<string, X509Certificate2>();
        private readonly ReaderWriterLock _oRwLock = new ReaderWriterLock();

        public void Put(string key, X509Certificate2 cert)
        {
            if(string.IsNullOrEmpty(key)) throw new ArgumentNullException();
            if (cert == null) throw new ArgumentNullException();

            try
            {
                _oRwLock.AcquireWriterLock(-1);
                _certServerCache["CN=" + key] = cert;
            }
            finally
            {
                _oRwLock.ReleaseWriterLock();
            }
        }

        public bool TryGet(string key, out X509Certificate2 cert)
        {
            try
            {
                _oRwLock.AcquireReaderLock(-1);
                var cn = "CN=" + key;
                if (_certServerCache.ContainsKey(cn))
                {
                    cert = _certServerCache[cn];
                    return true;
                }
            }
            finally
            {
                _oRwLock.ReleaseReaderLock();
            }
            cert = null;
            return false;
        }
    }
}