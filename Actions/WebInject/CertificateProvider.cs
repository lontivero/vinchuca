using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;
using System.Threading;

namespace DreamBot.Actions.WebInject
{
    public static class CertificateProvider 
    {
        private static readonly CertificateCache Cache = new CertificateCache();
        private const string CN = "DigiTrust Global Assured ID Root";

        public static X509Certificate2 GetCertificateForHost(string hostname)
        {
            X509Certificate2 cert;
            if(Cache.TryGet(hostname, out cert)) return cert;

            var x509Certificate = LoadCertificateFromWindowsStore(hostname);
            if (x509Certificate == null)
            {
                x509Certificate = CreateCertificate(hostname);
                Cache.Put(hostname, x509Certificate);
            }
            return x509Certificate;
        }


        private static X509Certificate2 LoadCertificateFromWindowsStore(string hostname)
        {
            var x509Store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
            x509Store.Open(OpenFlags.ReadOnly);
            var inStr = string.Format("CN={0}", hostname);

            try
            {
                foreach (var certificate in x509Store.Certificates)
                {
                    if (inStr.Equals(certificate.Subject))
                    {
                        return certificate;
                    }
                }
            }
            finally
            {
                x509Store.Close();
            }
            return null;
        }

        private static X509Certificate2 CreateCertificate(string sHostname)
        {
            //makecert -r -ss root -n "CN=DigiTrust Global Assured ID Root, OU=www.digitrust.com, O=DigiTrust Inc, C=US" -sky signature -eku 1.3.6.1.5.5.7.3.1 -h 1 -cy authority -a sha256 -m 60 -b 01/01/2015
            //makecert -pe -ss my -n "CN=*.google.com" -sky exchange -in "DigiTrust Global Assured ID Root" -is root -eku 1.3.6.1.5.5.7.3.1 -cy end -a sha256 -m 60 -b 01/01/2015

            X509Certificate2 x509Certificate = null;
                
            var ecode = -1;

            var process = new Process();
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = false;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.FileName = "mc.exe";
            process.StartInfo.Arguments = string.Format("-pe -ss my -n \"CN={0}\" -sky exchange -in \"DigiTrust Global Assured ID Root\" -is root -eku 1.3.6.1.5.5.7.3.1 -cy end -a sha256 -m 60 -b 01/01/2015", sHostname);
            process.Start();
            process.WaitForExit(1200);
            ecode = process.ExitCode;
            process.Dispose();

            if (ecode == 0)
            {
                var num3 = 6;
                do
                {
                    x509Certificate = LoadCertificateFromWindowsStore(sHostname);
                    Thread.Sleep(50 * (6 - num3));
                    num3--;
                }
                while (x509Certificate == null && num3 >= 0);
            }

            return x509Certificate;
        }
    }
}