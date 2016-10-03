using System;
using System.Collections.Generic;
using System.IO;

namespace Hornbill
{
    internal class XmlHelper
    {
        internal static string getParmAsString(System.Xml.XmlDocument doc, string szName)
        {
            string path = stringHelper.AppendUrl("/methodCallResult/params/", szName,false);
            System.Xml.XmlNodeList nodeList = doc.SelectNodes(path);
            System.Xml.XmlNode node = nodeList.Item(0);
            if (node != null)
            {
                return node.InnerText;
            }
            return null;
        }

        internal static string getResponseElementAsString(System.Xml.XmlDocument doc, string path)
        {
            System.Xml.XmlNodeList nodeList = doc.SelectNodes(stringHelper.AppendUrl("/methodCallResult/", path,false));
            System.Xml.XmlNode node = nodeList.Item(0);
            if (node != null)
            {
                return node.InnerText;
            }
            return null;
        }

        internal static bool getParmAsBool(System.Xml.XmlDocument doc, string path)
        {
            String szValue = getParmAsString(doc, path);
            if (szValue != null && szValue.CompareTo("true") == 0)
            {
                return true;
            }
            return false;
        }
        internal static long getParmAsNumber(System.Xml.XmlDocument doc, string path) 
        {
            string szValue = getParmAsString(doc, path);
            if (szValue != null && !(szValue.Length == 0))
            {
                return Int64.Parse(szValue);
            }
            return 0;
        }
        internal static DateTime getParmAsTime(System.Xml.XmlDocument doc, string path)
        {
            string szValue = getParmAsString(doc, path);
            DateTime MyDateTime = new DateTime();
            MyDateTime = DateTime.ParseExact(szValue, "yyyy-MM-dd HH:mm:ssZ", null);
            return MyDateTime;
        }
        internal static int getParmAsCount(System.Xml.XmlDocument doc, string szName)
        {
            System.Xml.XmlNodeList nodeList = doc.SelectNodes(stringHelper.AppendUrl("/methodCallResult/params/",szName,false));
            return nodeList.Count;
        }
        internal static List<string> getParmAsStringArray(System.Xml.XmlDocument doc, String szName)
        {
            System.Xml.XmlNodeList nodeList = doc.SelectNodes(stringHelper.AppendUrl("/methodCallResult/params/",szName,false));
            List<String> list = new List<String>();
            for(int i = 0; i < nodeList.Count; i++)
            {
                System.Xml.XmlNode childNode = nodeList.Item(i);
                if (childNode.Value != null)
                {
                    list.Add(childNode.Value);
                }
            }
            return list;
        }
        internal static System.Xml.XmlDocument getParmAsComplexType(System.Xml.XmlDocument doc, String szName, int nodePosition)
        {
            System.Xml.XmlDocument xDoc = new System.Xml.XmlDocument();
            System.Xml.XmlNodeList nodeList = doc.SelectNodes(stringHelper.AppendUrl("/methodCallResult/params/",szName,false));
            if (nodePosition != 0)
                nodePosition--;
            if (nodeList.Count > nodePosition)
            {
                xDoc.AppendChild(xDoc.ImportNode(nodeList[nodePosition], true));
            }
            return xDoc;
        }
    }
}
