using System.Xml;

namespace Vinchuca.Upnp
{
    internal static class Utils
    {
        internal static string GetXmlElementText(XmlNode node, string elementName)
        {
            var element = node[elementName];
            return element != null ? element.InnerText : string.Empty;
        }
    }
}