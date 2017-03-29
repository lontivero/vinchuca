using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Xml;

namespace Vinchuca.Upnp
{
    internal class SoapClient
    {
        private readonly string _serviceType;
        private readonly Uri _url;

        public SoapClient(Uri url, string serviceType)
        {
            _url = url;
            _serviceType = serviceType;
        }

        public XmlDocument Invoke(string operationName, IDictionary<string, object> args)
        {
            byte[] messageBody = BuildMessageBody(operationName, args);
            HttpWebRequest request = BuildHttpWebRequest(operationName, messageBody);

            var requestStream = request.GetRequestStream();
            requestStream.Write(messageBody, 0, messageBody.Length);
            requestStream.Close();

            WebResponse response;
            try
            {
                response = request.GetResponse();
            }
            catch (WebException ex)
            {
                response = ex.Response as HttpWebResponse;

                if (response == null)
                {
                    throw;
                }
            }

            var stream = response.GetResponseStream();
            var contentLength = response.ContentLength;

            var reader = new StreamReader(stream, Encoding.UTF8);

            int bytesToRead = (int) contentLength;
            var buffer = new char[bytesToRead];
            reader.ReadBlock(buffer, 0, bytesToRead);
            var responseBody = contentLength != -1
                ? new string(buffer)
                : reader.ReadToEnd();

            var responseXml = GetXmlDocument(responseBody);
            response.Close();
            return responseXml;
        }

        private HttpWebRequest BuildHttpWebRequest(string operationName, byte[] messageBody)
        {
            var request = (HttpWebRequest) WebRequest.Create(_url);
            request.Proxy = null;
            request.KeepAlive = false;
            request.Method = "POST";
            request.ContentType = "text/xml; charset=\"utf-8\"";
            request.Headers.Add("SOAPACTION", "\"" + _serviceType + "#" + operationName + "\"");
            request.ContentLength = messageBody.Length;
            return request;
        }

        private byte[] BuildMessageBody(string operationName, IEnumerable<KeyValuePair<string, object>> args)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<s:Envelope ");
            sb.AppendLine("   xmlns:s=\"http://schemas.xmlsoap.org/soap/envelope/\" ");
            sb.AppendLine("   s:encodingStyle=\"http://schemas.xmlsoap.org/soap/encoding/\">");
            sb.AppendLine("   <s:Body>");
            sb.AppendLine("	  <u:" + operationName + " xmlns:u=\"" + _serviceType + "\">");
            foreach (var a in args)
            {
                sb.AppendLine("		 <" + a.Key + ">" + Convert.ToString(a.Value, CultureInfo.InvariantCulture) +
                              "</" + a.Key + ">");
            }
            sb.AppendLine("	  </u:" + operationName + ">");
            sb.AppendLine("   </s:Body>");
            sb.Append("</s:Envelope>\r\n\r\n");
            string requestBody = sb.ToString();

            byte[] messageBody = Encoding.UTF8.GetBytes(requestBody);
            return messageBody;
        }

        private XmlDocument GetXmlDocument(string response)
        {
            XmlNode node;
            var doc = new XmlDocument();
            doc.LoadXml(response);

            var nsm = new XmlNamespaceManager(doc.NameTable);

            nsm.AddNamespace("errorNs", "urn:schemas-upnp-org:control-1-0");

            if ((node = doc.SelectSingleNode("//errorNs:UPnPError", nsm)) != null)
            {
                int code = Convert.ToInt32(Utils.GetXmlElementText(node, "errorCode"), CultureInfo.InvariantCulture);
                string errorMessage = Utils.GetXmlElementText(node, "errorDescription");
                throw new MappingException(code, errorMessage);
            }

            return doc;
        }
    }
}
