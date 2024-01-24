using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;
using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Hornbill
{


    #region HttpResponse
    internal class HttpResponse
    {
        private bool _success = false;
        private string _xmlResults = string.Empty;
        private string _lasterrmsg = string.Empty;
        private int _errorCode = 0;

        internal string LastErrorMessage
        {
            get { return _lasterrmsg; }
            set { _lasterrmsg = value; }
        }
        internal int ErrorCode { get { return _errorCode; } set { _errorCode = value; } }
        internal string XmlResults { get { return _xmlResults; } set { _xmlResults = value; } }
        internal bool Success { get { return _success; } set { _success = value; } }
        internal void Clear()
        {
            _xmlResults = string.Empty;
            _success = false;
            _lasterrmsg = string.Empty;
        }
    }
    #endregion

    #region XmlmcService
    /************************************************************************/
    /// <summary>
    /// Provides a high level abstraction for communication with the Hornbill server
    /// </summary>
    public class XmlmcService
    {
        private const int WEBDAV_TIMEOUT = 300000; // millisecs

        #region AcceptType
        private enum AcceptType
        {
            XML = 0,
            JSON
        }
        #endregion
        
        #region Private Members
        private string _serverURL = string.Empty;
        private string _dav    = string.Empty;
        private string _xmlmc  = string.Empty;
        private string _apiKey = string.Empty;
        private string _instanceName = string.Empty;
        private AcceptType _acceptType = AcceptType.XML;
        private readonly HttpResponse _httpResponse = new HttpResponse();
        private CookieContainer _cookies = new CookieContainer();
        private string _sessionId = string.Empty;
        private List<XmlmcParam> _params = new List<XmlmcParam>();
        private readonly System.Xml.XmlDocument _lastXmlResponse = new System.Xml.XmlDocument();
        #endregion

        #region Construtors
        /// <summary>
        /// Creates an XmlmcService object.
        /// </summary>
        /// <param name="instanceOrURL">The instance name or the server URL.</param>
        /// <param name="serviceEntryPoint">The XMLMC entry point.</param>
        /// <param name="webDavEntryPoint">Tthe WebDav entry point.</param>
        /// <param name="apiKey">The API key.</param>
        public XmlmcService(string instanceOrURL, string serviceEntryPoint, string webDavEntryPoint, string apiKey = "")
        {
            if(string.IsNullOrWhiteSpace(instanceOrURL) == false)
            {
                if (instanceOrURL.ToLower().StartsWith("http"))
                {
                    _instanceName = string.Empty;
                    _serverURL = instanceOrURL;
                }
                else
                {
                    Uri uri = ResolveInstanceName(instanceOrURL);
                    if(uri != null)
                    {
                        _serverURL = uri.AbsoluteUri;
                    }
                    _instanceName = instanceOrURL;
                }
            }
            _dav    = webDavEntryPoint;
            _xmlmc = serviceEntryPoint;
            _apiKey = apiKey;
        }

        #endregion

        #region Properties

        #region Server
        /// <summary>
        /// Gets the URL of the server.
        /// </summary>
        public string ServerURL
        {
            get { return _serverURL; }
        }
        #endregion

        #region InstanceName
        /// <summary>
        /// Gets or Sets the name of the instance
        /// </summary>
        public string InstanceName
        {
            get { return _instanceName; }
            set { _instanceName = value; }
        }
        #endregion

        #region ServiceEntryPoint
        /// <summary>
        /// Gets or Sets the service entry point.
        /// </summary>
        public string ServiceEntryPoint
        {
            get { return _xmlmc; }
            set { _xmlmc = value; }
        }
        #endregion

        #region WebDavEntryPoint
        /// <summary>
        /// Gets or Sets the WebDav entry point.
        /// </summary>
        public string WebDavEntryPoint
        {
            get { return _dav; }
            set { _dav = value; }
        }

        #region Accept
        /* 
        JSON is not currently supported
        public AcceptType Accept
        {
            set
            {
                _acceptType = value;
            }
            get { return _acceptType; }
        }
        */
        #endregion

        #endregion

        #region APIKey
        /// <summary>
        /// Gets or sets the API key.
        /// </summary>
        public string APIKey
        {
            get { return _apiKey; }
            set { _apiKey = value; }
        }
        #endregion

        #region Params
        /// <summary>
        /// Gets or sets the list of the parameters.
        /// </summary>
        public List<XmlmcParam> Params
        {
            get { return _params; }
            set { _params = value; }
        }
        #endregion

        #endregion

        #region Functions

        #region clearLastResponse
        private void clearLastResponse()
        {
            if (_lastXmlResponse != null)
            {
                _lastXmlResponse.RemoveAll();
            }
            if (_httpResponse != null)
            {
                _httpResponse.LastErrorMessage = string.Empty;
            }
        }
        #endregion

        #region checkStatusCode
        /// <summary>
        /// 
        /// </summary>
        /// <param name="xmlResponse"></param>
        /// <returns>True if success, otherwise false</returns>
        /// <exception cref="System.Xml.XmlException">Throw if the XML is not valid.</exception>
        private bool checkStatusCode(string xmlResponse)
        {
            // clear the last response
            _httpResponse.Clear();
            if (string.IsNullOrWhiteSpace(xmlResponse))
                return false;

            if (xmlResponse.StartsWith("{"))
            {
                // parse json - to be implemented

            }
            else
            {
                _lastXmlResponse.LoadXml(xmlResponse);
                System.Xml.XmlNodeList nodeList = _lastXmlResponse.DocumentElement.SelectNodes("//methodCallResult");
                if (nodeList.Count == 1)
                {
                    System.Xml.XmlNode nodeAttrib = nodeList.Item(0).Attributes.GetNamedItem("status");
                    if (nodeAttrib != null)
                    {
                        if (nodeAttrib.Value.ToLower().CompareTo("ok") == 0)
                        {
                            _httpResponse.Success = true;
                        }
                        else
                        {                            
                            System.Xml.XmlNode node = _lastXmlResponse.DocumentElement.SelectSingleNode("/methodCallResult/state/code");
                            if(node != null)
                            {
                                int errorCode;
                                if(int.TryParse(node.InnerText, out errorCode) == false)
                                {
                                    errorCode = 0;
                                }
                                _httpResponse.ErrorCode = errorCode;
                            }
                            // Error message
                            node = _lastXmlResponse.DocumentElement.SelectSingleNode("/methodCallResult/state/error");
                            if (node != null)
                            {
                                _httpResponse.LastErrorMessage = node.InnerText;
                            }
                            _httpResponse.Success = false;
                        }
                    }
                }
                /* 
                // Could throw XmlException
                catch (System.Xml.XmlException e)
                {
                    throw e;
                }
                */
            }
            return _httpResponse.Success;
        }
        #endregion

        #region addParams

        /// <summary>
        /// Prepare string for XML
        /// </summary>
        /// <param name="name"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        private string convertToXmlNode(string name, string data)
        {
            if (string.Compare(name, "password", true) != 0)
            {
                data = data.Replace("&", "&amp;");
                data = data.Replace("<", "&lt;");
                data = data.Replace(">", "&gt;");
                data = data.Replace("\'", "&apos;");
                data = data.Replace("\"", "&quot;");
            }
            return "<" + name + ">" + data + "</" + name + ">";
        }

        /// <summary>
        /// Add parameters to the XMLMC method call.
        /// </summary>
        /// <param name="name">The name of the parameter.</param>
        /// <returns>The XMLMC parameter object.</returns>
        public XmlmcParam AddParam(string name)
        {
            XmlmcParam param = new XmlmcParam(name);
            _params.Add(param);
            return param;
        }

        /// <summary>
        /// Add parameters to the XMLMC method call.
        /// </summary>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="inParams">The value of the parameter.</param>
        /// <returns>The XMLMC parameter object.</returns>
        public XmlmcParam AddParam(string name, List<XmlmcParam> inParams)
        {
            XmlmcParam param = new XmlmcParam(name, inParams);
            _params.Add(param);
            return param;
        }

        /// <summary>
        /// Add parameters to the XMLMC method call.
        /// </summary>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="inParam">The value of the parameter.</param>
        /// <returns>The XMLMC parameter object.</returns>
        public XmlmcParam AddParam(string name, XmlmcParam inParam)
        {
            XmlmcParam param = new XmlmcParam(name, inParam);
            _params.Add(param);
            return param;
        }

        /// <summary>
        /// Add parameters to the XMLMC method call.
        /// </summary>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="value">The value of the parameter.</param>
        /// <returns>The XMLMC parameter object.</returns>
        public XmlmcParam AddParam(string name, string value)
        {
            XmlmcParam param = new XmlmcParam(name, value);
            _params.Add(param);
            return param;
        }

        /// <summary>
        /// Add parameters to the XMLMC method call.
        /// </summary>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="idList">The value of the parameter.</param>
        /// <returns>The XMLMC parameter object.</returns>
        public List<XmlmcParam> AddParamIdList <T>(string name, List<T> idList)
        {
            List<XmlmcParam> listParams = new List<XmlmcParam>();
            foreach (object objVal in idList)
            {
                XmlmcParam param = new XmlmcParam(name, objVal.ToString());
                listParams.Add(param);
                _params.Add(param);
            }
            return listParams;
        }

        /// <summary>
        /// Add parameters to the XMLMC method call.
        /// </summary>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="value">The value of the parameter.</param>
        /// <returns>The XMLMC parameter object.</returns>
        public XmlmcParam AddParam(string name, int value)
        {
            XmlmcParam param = new XmlmcParam(name, value);
            _params.Add(param);
            return param;
        }

        /// <summary>
        /// Add parameters to the XMLMC method call.
        /// </summary>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="value">The value of the parameter.</param>
        /// <returns>The XMLMC parameter object.</returns>
        public XmlmcParam AddParam(string name, long value)
        {
            XmlmcParam param = new XmlmcParam(name, value);
            _params.Add(param);
            return param;
        }

         /// <summary>
        /// Add parameters to the XMLMC method call.
        /// </summary>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="value">The value of the parameter.</param>
        /// <returns>The XMLMC parameter object.</returns>
        public XmlmcParam AddParam(string name, float value)
        {
            XmlmcParam param = new XmlmcParam(name, value);
            _params.Add(param);
            return param;
        }

        /// <summary>
        /// Add parameters to the XMLMC method call.
        /// </summary>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="value">The value of the parameter.</param>
        /// <returns>The XMLMC parameter object.</returns>
        public XmlmcParam AddParam(string name, double value)
        {
            XmlmcParam param = new XmlmcParam(name, value);
            _params.Add(param);
            return param;
        }

        /// <summary>
        /// Add parameters to the XMLMC method call.
        /// </summary>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="value">The value of the parameter.</param>
        /// <returns>The XMLMC parameter object.</returns>
        public XmlmcParam AddParam(string name, DateTime value)
        {
            XmlmcParam param = new XmlmcParam(name, value);
            _params.Add(param);
            return param;
        }

        /// <summary>
        /// Add parameters to the XMLMC method call.
        /// </summary>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="value">The value of the parameter.</param>
        /// <returns>The XMLMC parameter object.</returns>
        public XmlmcParam AddParam(string name, bool value)
        {
            XmlmcParam param = new XmlmcParam(name, value);
            _params.Add(param);
            return param;
        }

        /// <summary>
        /// Clear all the parameters.
        /// </summary>
        public void ClearParams()
        {
            _params.Clear();
        }


        /// <summary>
        /// Returns the XML that will be sent to the server.
        /// </summary>
        /// <param name="service">Service to be invoked</param>
        /// <param name="method">Method to be invoked</param>
        /// <returns>The XML that will be sent to the server</returns>
        /// <exception cref="System.ArgumentNullException">Throw when the argument is not valid.</exception>
        public string GetInvokeXML(string service, string method)
        {
            if (string.IsNullOrWhiteSpace(service) || string.IsNullOrWhiteSpace(method))
            {
                throw new ArgumentNullException("The sevice/method name cannot be null or empty.");
            }
            // Start building the request XML file
            StringWriter writer = new StringWriter();
            System.Xml.XmlWriterSettings settings = new System.Xml.XmlWriterSettings();
            settings.Indent = true;
            settings.OmitXmlDeclaration = true;
            settings.ConformanceLevel = System.Xml.ConformanceLevel.Fragment;
            System.Xml.XmlWriter xmlWriter = System.Xml.XmlWriter.Create(writer, settings);
            xmlWriter.WriteStartElement("methodCall");
            xmlWriter.WriteAttributeString("service", service);
            xmlWriter.WriteAttributeString("method", method);

            string xml = string.Empty;
            foreach (XmlmcParam param in _params)
            {
                xml += param.XmlValue;
            }
            if (string.IsNullOrWhiteSpace(xml) == false)
            {
                xmlWriter.WriteRaw("<params>" + xml + "</params>");
            }
            xmlWriter.WriteEndElement();
            xmlWriter.Flush();
            return writer.ToString();
        }

        /// <summary>
        /// Returns the XML of the current parameters
        /// </summary>
        /// <returns>The XML of the current parameters</returns>
        public string GetParamsXML()
        {
            if (_params == null)
                return string.Empty;

            string xml = string.Empty;
            foreach(XmlmcParam param in _params)
            {
                xml += param.XmlValue;
            }
            return xml;
        }
        #endregion

        #region propFind
        /// <summary>
        /// PropFind
        /// </summary>
        /// <param name="path"></param>
        /// <returns>The string contains the file/folder info.</returns>
        /// <exception cref="System.ArgumentNullException">Throw when the argument is not valid.</exception>
        /// <exception cref="System.Net.WebException">Throw when the request has failed.</exception>
        private string PropFind(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentNullException("The path cannot be null or empty.");
            }

            clearLastResponse();

            string url = stringHelper.AppendUrl(stringHelper.AppendUrl(_serverURL, "dav"), path,false);
            string retval = string.Empty;
            Stream responseStream = null;
            try
            {
                string data = "<?xml version=\"1.0\" encoding=\"utf-8\" ?><d:propfind xmlns:d=\"DAV:\"><d:allprop/></d:propfind>";
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url.ToString());
                webRequest.CookieContainer = _cookies;
                webRequest.ContentType = "text/xml; charset=utf-8";
                // Prepare the headers
                webRequest.Headers.Add("Pragma", "no-cache");
                webRequest.Headers.Add("Translate", "f");
                webRequest.Headers.Add("Depth", "1");
                webRequest.KeepAlive = false;
                webRequest.Timeout = WEBDAV_TIMEOUT;
                webRequest.Method = "PROPFIND";

                addApiKeyHeader(webRequest);

                byte[] buff = stringHelper.UTF8Encode(data);
                webRequest.ContentLength = buff.Length;
                using (Stream postData = webRequest.GetRequestStream())
                {
                    postData.Write(buff, 0, buff.Length);
                }

                using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
                {
                    responseStream = webResponse.GetResponseStream();
                    if(responseStream == null)
                    {
                        throw new RequestFailureException("Unable to get the response stream.");
                    }
                    using (StreamReader sr = new StreamReader(responseStream, Encoding.UTF8))
                    {
                        retval = sr.ReadToEnd();
                    }
                }
            }
            catch (System.Net.WebException err)
            {
                _httpResponse.LastErrorMessage = err.Message;
                HttpWebResponse response = err.Response as HttpWebResponse;
                if(response != null)
                {
                    if(response.StatusCode == HttpStatusCode.NotFound)
                    {
                        return string.Empty;
                    }
                }
                throw;
            }
            return retval;
        }
        #endregion

        #region propGet
        /// <summary>
        /// PropGet
        /// </summary>
        /// <param name="path"></param>
        /// <returns>Return MemoryStream</returns>
        /// <exception cref="System.ArgumentNullException">Throw when the argument is not valid.</exception>
        /// <exception cref="System.Net.WebException">Throw when the request has failed.</exception>
        private MemoryStream PropGet(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentNullException("The path cannot be null or empty.");
            }

            clearLastResponse();

            string url = stringHelper.AppendUrl(stringHelper.AppendUrl(_serverURL, "dav"), path,false);
            Stream responseStream = null;
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url.ToString());
                webRequest.CookieContainer = _cookies;
                // Prepare the headers
                webRequest.Headers.Add("Pragma", "no-cache");
                webRequest.KeepAlive = false;
                webRequest.Timeout = WEBDAV_TIMEOUT;
                webRequest.Method = "GET";

                using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
                {
                    responseStream = webResponse.GetResponseStream();
                    if(responseStream == null)
                    {
                        throw new RequestFailureException("Unable to get the response stream.");
                    }
                    MemoryStream memoryStream = new MemoryStream();
                    responseStream.CopyTo(memoryStream);
                    return memoryStream;
                }
            }
            finally
            {
                if(responseStream != null)
                {
                    responseStream.Close();
                }
            }
        }
        #endregion

        #region DoesFileExists
        /// <summary>
        /// Check if the file or directory exists.
        /// </summary>
        /// <param name="path">The path of the file or directory.</param>
        /// <returns>Return true if exists, otherwise false.</returns>
        /// <exception cref="System.ArgumentNullException">Throw when the argument is not valid.</exception>
        /// <exception cref="System.Net.WebException">Throw when the request has failed.</exception>
        public bool DoesFileExists(string path)
        {
            if (PropFind(path).Length > 0)
                return true;
            return false;
        }
        #endregion

        #region fileOperation
        /// <summary>FileOperation</summary>
        private void fileOperation(string srcPath, string destPath, string command, bool overwrite)
        {
            if (string.IsNullOrWhiteSpace(srcPath) || string.IsNullOrWhiteSpace(destPath))
            {
                throw new ArgumentNullException("The path cannot be null or empty.");
            }
            if (string.IsNullOrWhiteSpace(command))
            {
                throw new ArgumentNullException("The command cannot be null or empty.");
            }

            clearLastResponse();

            string srcUrl = stringHelper.AppendUrl(stringHelper.AppendUrl(_serverURL, _dav), srcPath,false);
            string destUrl = stringHelper.AppendUrl(stringHelper.AppendUrl(_serverURL, _dav), destPath,false);
            Stream responseStream = null;
            try
            {

                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(srcUrl.ToString());
                webRequest.CookieContainer = _cookies;
                // Prepare the headers
                webRequest.Headers.Add("Cache-control", "no-cache");
                webRequest.Headers.Add("Destination", destUrl.ToString());
                webRequest.Headers.Add("Overwrite", overwrite == true ? "T" : "F");
                webRequest.KeepAlive = false;
                webRequest.Timeout = WEBDAV_TIMEOUT;
                webRequest.Method = command;
                
                addApiKeyHeader(webRequest);
                using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
                {
                    bool success = false;
                    responseStream = webResponse.GetResponseStream();
                    if (responseStream == null)
                    {
                        throw new RequestFailureException("Unable to get the response stream.");
                    }
                    using (StreamReader sr = new StreamReader(responseStream, Encoding.UTF8))
                    {
                        success = string.IsNullOrEmpty(sr.ReadToEnd());
                    }
                    if (success == false)
                    {
                        throw new RequestFailureException(string.Format("{0} failed. Source: {1} Target: {2}", command, srcPath, destPath));
                    }
                }
            }
            finally
            {
                if (responseStream != null)
                {
                    responseStream.Close();
                }
            }
        }
        #endregion

        #region MoveFile
        /// <summary>
        /// Moves an existing file or folder, including its children. The file or folder will be ovewritten if already exists.
        /// </summary>
        /// <param name="srcPath">The name of an existing file or directory.</param>
        /// <param name="destPath">The new name of the file or directory.</param>
        /// <exception cref="System.ArgumentNullException">Throw when the argument is not valid.</exception>
        /// <exception cref="System.Net.WebException">Throw when the request has failed.</exception>
        public void MoveFile(string srcPath, string destPath)
        {
            fileOperation(srcPath, destPath, "MOVE", true);
        }
        #endregion

        #region CopyFile
        /// <summary>
        /// Copies an existing file to a new file. The file will be overwritten if already exists.
        /// </summary>
        /// <param name="srcPath">The name of an existing file or directory.</param>
        /// <param name="destPath">The new name of the file or directory.</param>
        /// <exception cref="System.ArgumentNullException">Throw when the argument is not valid.</exception>
        /// <exception cref="System.Net.WebException">Throw when the request has failed.</exception>
        public void CopyFile(string srcPath, string destPath)
        {
            fileOperation(srcPath, destPath, "COPY", true);
        }
        #endregion

        #region RemoveFile
        /// <summary>
        /// Deletes an existing file
        /// </summary>
        /// <param name="filePath">The path of the file to be removed</param>
        /// <exception cref="System.ArgumentNullException">Throw when the argument is not valid.</exception>
        /// <exception cref="System.Net.WebException">Throw when the request has failed.</exception>
        public void RemoveFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentNullException("The path cannot be null or empty.");
            }

            clearLastResponse();

            string url = stringHelper.AppendUrl(stringHelper.AppendUrl(_serverURL, _dav), filePath,false);
            Stream responseStream = null;
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url.ToString());
                webRequest.CookieContainer = _cookies;
                // Prepare the headers
                webRequest.Headers.Add("Pragma", "no-cache");
                webRequest.KeepAlive = false;
                webRequest.Timeout = WEBDAV_TIMEOUT;
                webRequest.Method = "DELETE";

                addApiKeyHeader(webRequest);

                bool success = false;
                using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
                {
                    responseStream = webResponse.GetResponseStream();
                    if (responseStream == null)
                    {
                        throw new RequestFailureException("Unable to get the response stream.");
                    }
                    using (StreamReader sr = new StreamReader(responseStream, Encoding.UTF8))
                    {
                        success = string.IsNullOrEmpty(sr.ReadToEnd());
                    }
                    if(success == false)
                    {
                        throw new RequestFailureException(string.Format("Unable to remove the file from {0}", filePath));
                    }
                }
            }
            catch (System.Net.WebException err)
            {
                _httpResponse.LastErrorMessage = err.Message;
                HttpWebResponse response = err.Response as HttpWebResponse;
                if (response != null)
                {
                    if (response.StatusCode == HttpStatusCode.NotFound)
                    {
                        // Fail sliently if file doesn't exists
                        return;
                    }
                }
                throw;
            }
            finally
            {
                if(responseStream != null)
                {
                    responseStream.Close();
                }
            }
        }
        #endregion

        #region PutFile
        /// <summary>
        /// Uploads a file to the server. The file on the server will be overwritten if already exists.
        /// </summary>
        /// <param name="sourceFile">The path of an existing local file.</param>
        /// <param name="destFile">The path of the new file on the server.</param>
        /// <exception cref="System.ArgumentNullException">Throw when the argument is not valid.</exception>
        /// <exception cref="System.Net.WebException">Throw when the request has failed.</exception>
        /// <exception cref="System.IO.FileNotFoundException">Throw if unable to locate the source file.</exception>
        /// <exception cref="System.IO.IOException">Throw if not able to open the source file.</exception>
        public void PutFile(string sourceFile, string destFile)
        {
            if (string.IsNullOrWhiteSpace(sourceFile) || string.IsNullOrWhiteSpace(destFile))
            {
                throw new ArgumentNullException("The path cannot be null or empty.");
            }

            if(File.Exists(sourceFile) == false)
            {
                throw new FileNotFoundException(string.Format("Unable to locate the file - {0}",sourceFile));
            }

            FileAttributes attr = File.GetAttributes(sourceFile);
            if((attr & FileAttributes.Directory) == FileAttributes.Directory)
            {
                throw new ArgumentException(string.Format("The source file cannot be a folder."));
            }


            Stream responseStream = null;
            try
            {
                bool overwrite = true;
                clearLastResponse();

                string url = stringHelper.AppendUrl(stringHelper.AppendUrl(_serverURL, _dav), destFile,false);
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url.ToString());
                webRequest.CookieContainer = _cookies;
                // Prepare the headers
                webRequest.KeepAlive = true;
                webRequest.Timeout = WEBDAV_TIMEOUT; // 5 min
                webRequest.Method = "PUT";
                webRequest.Headers.Add("Overwrite", overwrite ? "T" : "F");

                addApiKeyHeader(webRequest);

                // Read the file
                using (Stream fileStream = File.OpenRead(sourceFile))
                {
                    if (fileStream == null)
                        throw new System.IO.IOException(string.Format("Unable to read the file - {0}", sourceFile));

                    webRequest.ContentType = "application/octet-stream";
                    FileInfo fi = new FileInfo(sourceFile);
                    webRequest.ContentLength = fi.Length;

                    int bytesRead = 0, totalBytesRead = 0;
                    using (Stream postData = webRequest.GetRequestStream())
                    {
                        postData.ReadTimeout = WEBDAV_TIMEOUT;
                        postData.WriteTimeout = WEBDAV_TIMEOUT;
                        byte[] buffer = new byte[8192];
                        while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            postData.Write(buffer, 0, bytesRead);
                            totalBytesRead += bytesRead;
                        }
                    }
                    if(totalBytesRead != fi.Length)
                    {
                        throw new RequestFailureException("Unable to upload the file to the server (File size mismatch).");
                    }
                }

                bool success = false;
                using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
                {
                    responseStream = webResponse.GetResponseStream();
                    if (responseStream == null)
                    {
                        throw new RequestFailureException("Unable to get the response stream.");
                    }
                    using (StreamReader sr = new StreamReader(responseStream, Encoding.UTF8))
                    {
                        success = string.IsNullOrEmpty(sr.ReadToEnd());
                    }
                }
                if(success == false)
                {
                    throw new RequestFailureException(string.Format("Unable to copy the file to {0}", url));
                }
            }
            finally
            {
                if(responseStream != null)
                {
                    responseStream.Close();
                }
            }

        }

        /// <summary>
        /// Uploads a text file to the server.
        /// </summary>
        /// <param name="path">The path of the file.</param>
        /// <param name="contents">The contents of the file. It can be plain text or other formatted text.</param>
        /// <exception cref="System.ArgumentNullException">Throw when the argument is not valid.</exception>
        /// <exception cref="System.Net.WebException">Throw when the request has failed.</exception>
        public void PutText(string path, string contents)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentNullException("The path cannot be null or empty.");
            }

            Stream responseStream = null;
            try
            {
                bool overwrite = true;
                clearLastResponse();

                string url = stringHelper.AppendUrl(stringHelper.AppendUrl(_serverURL, _dav), path,false);
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url.ToString());
                webRequest.CookieContainer = _cookies;
                // Prepare the headers
                webRequest.KeepAlive = true;
                webRequest.Timeout = WEBDAV_TIMEOUT; // 5 min
                webRequest.Method = "PUT";
                webRequest.Headers.Add("Overwrite", overwrite ? "T" : "F");

                addApiKeyHeader(webRequest);

                if (contents != null && contents.Length > 0)
                {
                    byte[] buff = stringHelper.UTF8Encode(contents);
                    webRequest.ContentLength = buff.Length;
                    webRequest.ContentType = "text/plain";
                    using (Stream postData = webRequest.GetRequestStream())
                    {
                        postData.WriteTimeout = WEBDAV_TIMEOUT;
                        if (postData != null)
                        {
                            postData.Write(buff, 0, buff.Length);
                        }
                    }
                }

                bool success = false;
                using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
                {
                    responseStream = webResponse.GetResponseStream();
                    if (responseStream == null)
                    {
                        throw new RequestFailureException("Unable to get the response stream.");
                    }
                    using (StreamReader sr = new StreamReader(responseStream, Encoding.UTF8))
                    {
                        success = string.IsNullOrEmpty(sr.ReadToEnd());
                    }
                    if(success == false)
                    {
                        throw new RequestFailureException(string.Format("Unable to put the text to {0}",path));
                    }
                }
            }
            finally
            {
                if(responseStream != null)
                {
                    responseStream.Close();
                }
            }
        }
        #endregion

        #region CreateFolder
        /// <summary>
        /// Creates a new folder.
        /// </summary>
        /// <param name="path">The path of the folder to be created.</param>
        /// <exception cref="System.ArgumentNullException">Throw when the argument is not valid.</exception>
        /// <exception cref="System.Net.WebException">Throw when the request has failed.</exception>
        public void CreateFolder(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentNullException("The path cannot be null or empty.");
            }
            if (DoesFileExists(path) == true)
            {
                return;
            }

            string url = stringHelper.AppendUrl(stringHelper.AppendUrl(_serverURL, _dav), path,false);
            Stream responseStream = null;
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url.ToString());
                webRequest.CookieContainer = _cookies;
                // Prepare the headers
                webRequest.Headers.Add("Cache-control", "no-cache");
                webRequest.KeepAlive = false;
                webRequest.Timeout = WEBDAV_TIMEOUT;
                webRequest.Method = "MKCOL";

                addApiKeyHeader(webRequest);

                bool success = false;
                using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
                {
                    responseStream = webResponse.GetResponseStream();
                    if (responseStream == null)
                    {
                        throw new RequestFailureException("Unable to get the response stream.");
                    }
                    using (StreamReader sr = new StreamReader(responseStream, Encoding.UTF8))
                    {
                        success = string.IsNullOrEmpty(sr.ReadToEnd());
                    }
                    if(success == false)
                    {
                        throw new RequestFailureException(string.Format("Unable to create the folder - {0}", path));
                    }
                }
            }
            finally
            {
                if(responseStream != null)
                {
                    responseStream.Close();
                }
            }
        }
        #endregion

        private void addApiKeyHeader(HttpWebRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(_apiKey))
                return;

            // add authorization header
            request.Headers.Add("Authorization", string.Format("ESP-APIKEY {0}", _apiKey));
        }

        #region invoke
        /// <summary>
        /// Invokes an API call. 
        /// </summary>
        /// <param name="service">The name of the service.</param>
        /// <param name="method">The name of the method.</param>
        /// <exception cref="System.ArgumentNullException">Throw when the argument is not valid.</exception>
        /// <exception cref="System.Net.WebException">Throw when the request has failed.</exception>
        public void Invoke(string service, string method)
        {
            if (string.IsNullOrWhiteSpace(_serverURL))
            {
                throw new ArgumentNullException("The server URL cannot be null or empty.");
            }
            if (string.IsNullOrWhiteSpace(_xmlmc))
            {
                throw new ArgumentNullException("The service entry point cannot be null or empty.");
            }
            if (string.IsNullOrWhiteSpace(service))
            {
                throw new ArgumentNullException("The service name cannot be null or empty.");
            }
            if (string.IsNullOrWhiteSpace(method))
            {
                throw new ArgumentNullException("The method name cannot be null or empty.");
            }
            // reset the last action
            _httpResponse.Clear();
            string data = string.Empty;
            string url = stringHelper.AppendUrl(stringHelper.AppendUrl(_serverURL, _xmlmc), service);
            Stream responseStream = null;
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url.ToString());
                // Set cookies (session Id)
                request.CookieContainer = _cookies;
                request.Method = "POST";
                request.ContentType = "text/xmlmc; charset=utf-8";
                request.Accept = _acceptType == AcceptType.JSON ? "text/json" : "text/xml";

                addApiKeyHeader(request);
                
                // Building the request XML
                data = GetInvokeXML(service, method);

                ClearParams();

                if (data.Length > 0)
                {
                    byte[] buff = stringHelper.UTF8Encode(data);
                    request.ContentLength = buff.Length;
                    using (Stream postData = request.GetRequestStream())
                    {
                        postData.Write(buff, 0, buff.Length);
                    }
                }
                else
                {
                    request.ContentLength = 0;
                }

                using (HttpWebResponse webResponse = (HttpWebResponse)request.GetResponse())
                {
                    responseStream = webResponse.GetResponseStream();
                    if (responseStream == null)
                    {
                        throw new RequestFailureException("Unable to get the response stream.");
                    }
                    else
                    {
                        using (StreamReader sr = new StreamReader(responseStream, Encoding.UTF8))
                        {
                            _httpResponse.XmlResults = sr.ReadToEnd();
                        }
                    }
                }

                string szXmlResult = _httpResponse.XmlResults;
                if (!checkStatusCode(szXmlResult))
                {
                    throw new RequestFailureException(_httpResponse.LastErrorMessage);
                }
            }
            finally
            {
                if(responseStream != null)
                {
                    responseStream.Close();
                }
            }

        }

        #endregion

        #region reqeust
        /// <summary>
        /// Send a request to the server
        /// </summary>
        /// <param name="uri">The uri of the request.</param>
        /// <returns>The response from the server.</returns>
        /// <exception cref="System.ArgumentNullException">Throw when the argument is not valid.</exception>
        /// <exception cref="System.Net.WebException">Throw when the request has failed.</exception>
        private string request(Uri uri)
        {
            if(uri == null)
            {
                throw new ArgumentNullException("The request URL cannot be null.");
            }

            string results = string.Empty;
            Stream responseStream = null;
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
                // Set cookies (session Id)
                request.CookieContainer = _cookies;
                request.Accept = _acceptType == AcceptType.JSON ? "text/json" : "text/xml";

                using (HttpWebResponse webResponse = (HttpWebResponse)request.GetResponse())
                {
                    responseStream = webResponse.GetResponseStream();
                    if (responseStream == null)
                    {
                        throw new RequestFailureException("Unable to get the response stream.");
                    }

                    using (StreamReader sr = new StreamReader(responseStream, Encoding.UTF8))
                    {
                        results = sr.ReadToEnd();
                    }
                    return results;
                }
            }

            finally
            {
                if(responseStream != null)
                {
                    responseStream.Close();
                }
            }
        }
        #endregion

        #region getParams
        /// <summary>
        /// Validate parameter path
        /// </summary>
        /// <param name="path"></param>
        /// <returns>True if valid otherwise false.</returns>
        /// <exception cref="System.ArgumentNullException">Throw when the argument is not valid.</exception>
        /// <exception cref="System.InvalidOperationException">Throw if the last response XML is null.</exception>
        private void validateGetParams(string path)
        {
            if (_lastXmlResponse == null)
            {
                throw new InvalidOperationException("The XML document is empty.");
            }
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentNullException("The path cannot be null or empty.");
            }
        }

        /// <summary>
        /// Gets the value using a XPath expression.
        /// </summary>s
        /// <param name="path">A String that contains an XPath expression.</param>
        /// <returns>The value from the specified path.</returns>
        /// <exception cref="System.ArgumentNullException">Throw when the argument is not valid.</exception>
        /// <exception cref="System.InvalidOperationException">Throw if the last response XML is null.</exception>
        public string GetResponseParamAsString(string path)
        {
            validateGetParams(path);
            return XmlHelper.getParmAsString(_lastXmlResponse, path);
        }
        /// <summary>
        /// Gets the value using a XPath expression.
        /// </summary>s
        /// <param name="path">A String that contains an XPath expression.</param>
        /// <returns>The value from the specified path.</returns>
        /// <exception cref="System.ArgumentNullException">Throw when the argument is not valid.</exception>
        /// <exception cref="System.InvalidOperationException">Throw if the last response XML is null.</exception>
        public bool GetResponseParamAsBool(string path)
        {
            validateGetParams(path);

            return XmlHelper.getParmAsBool(_lastXmlResponse, path);
        }
        /// <summary>
        /// Gets the value using a XPath expression.
        /// </summary>s
        /// <param name="path">A String that contains an XPath expression.</param>
        /// <returns>The value from the specified path.</returns>
        /// <exception cref="System.ArgumentNullException">Throw when the argument is not valid.</exception>
        /// <exception cref="System.InvalidOperationException">Throw if the last response XML is null.</exception>
        public long GetResponseParamAsNumber(string path)
        {
            validateGetParams(path);
            return XmlHelper.getParmAsNumber(_lastXmlResponse, path);
        }
        /// <summary>
        /// Gets the value using a XPath expression.
        /// </summary>s
        /// <param name="path">A String that contains an XPath expression.</param>
        /// <returns>The value from the specified path.</returns>
        /// <exception cref="System.ArgumentNullException">Throw when the argument is not valid.</exception>
        /// <exception cref="System.InvalidOperationException">Throw if the last response XML is null.</exception>
        public DateTime GetResponseParamAsTime(string path)
        {
            validateGetParams(path);
            return XmlHelper.getParmAsTime(_lastXmlResponse, path);
        }
        /// <summary>
        /// Gets the value using a XPath expression.
        /// </summary>s
        /// <param name="path">A String that contains an XPath expression.</param>
        /// <returns>The value from the specified path.</returns>
        /// <exception cref="System.ArgumentNullException">Throw when the argument is not valid.</exception>
        /// <exception cref="System.InvalidOperationException">Throw if the last response XML is null.</exception>
        public int GetResponseParamCount(string path)
        {
            validateGetParams(path);
            return XmlHelper.getParmAsCount(_lastXmlResponse, path);
        }
        /// <summary>
        /// Gets the value using a XPath expression.
        /// </summary>s
        /// <param name="path">A String that contains an XPath expression.</param>
        /// <returns>The value from the specified path.</returns>
        /// <exception cref="System.ArgumentNullException">Throw when the argument is not valid.</exception>
        /// <exception cref="System.InvalidOperationException">Throw if the last response XML is null.</exception>
        public List<string> GetResponseParamAsStringArray(string path)
        {
            validateGetParams(path);
            return XmlHelper.getParmAsStringArray(_lastXmlResponse, path);
        }


        /// <summary>
        /// Gets the value using a XPath expression.
        /// </summary>
        /// <param name="path">A String that contains an XPath expression.</param>
        /// <param name="nodePosition">The position of the node.</param>
        /// <returns>The XML document from the specified path.</returns>
        /// <exception cref="System.ArgumentNullException">Throw when the argument is not valid.</exception>
        /// <exception cref="System.InvalidOperationException">Throw if the last response XML is null.</exception>
        public System.Xml.XmlDocument GetResponseParamAsComplexType(string path, int nodePosition)
        {
            validateGetParams(path);
            return XmlHelper.getParmAsComplexType(_lastXmlResponse, path, nodePosition);
        }

        /// <summary>
        /// Gets the XML document from the last response.
        /// </summary>
        /// <returns>The XML document from the last response.</returns>
        public System.Xml.XmlDocument GetResponseXMLDocument() 
        {
            return _lastXmlResponse;
        }

        /// <summary>
        /// Gets the XML formatted string from the last response.
        /// </summary>
        /// <returns>XML formatted string from the last response.</returns>
        public string GetResponseXML()
        {
            if (_lastXmlResponse == null)
            {
                return null;
            }
            return _lastXmlResponse.InnerXml;
        }

        /// <summary>
        /// Gets the error message from the last response. 
        /// </summary>
        /// <returns>The error message from the last response.</returns>
        public string GetLastResponseErrorMessage()
        {
            if (_httpResponse != null)
            {
                if (_httpResponse.Success == false && _lastXmlResponse != null)
                {
                    string errorMessage = XmlHelper.getResponseElementAsString(_lastXmlResponse, "state/error");
                    if (string.IsNullOrWhiteSpace(errorMessage) == false)
                    {
                        return errorMessage;
                    }
                }
                return _httpResponse.LastErrorMessage;
            }
            return string.Empty;
        }
        #endregion

        #region getFileContents
        /// <summary>
        /// Downloads a text file from the server.
        /// </summary>
        /// <param name="path">The path of the file.</param>
        /// <returns>The contents of the file.</returns>
        /// <exception cref="System.ArgumentNullException">Throw when the argument is not valid.</exception>
        public string GetFileContents(string path)
        {
            if(string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentNullException("The path cannot be null or empty.");
            }
            MemoryStream stream = PropGet(path);
            if(stream == null)
            {
                return string.Empty;
            }
            using(stream)
            {
                string res = stringHelper.UTF8Decode(stream.ToArray());
                return res;
            }
        }
        #endregion

        #region GetFile
        /// <summary>
        /// Downloads a file from the server.
        /// </summary>
        /// <param name="path">The path of the file.</param>
        /// <param name="memoryStream">The contents of the file.</param>
        /// <exception cref="System.ArgumentNullException">Throw when the argument is not valid.</exception>
        public void GetFile(string path, out MemoryStream memoryStream)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentNullException("The path cannot be null or empty.");
            }
            memoryStream = PropGet(path);
        }
        #endregion

        #region ResolveInstanceName
        /// <summary>
        /// Resolves the server address from the instance name
        /// </summary>
        /// <param name="instanceName">The name of the instance.</param>
        /// <returns>The server address.</returns>
        /// <exception cref="System.ArgumentNullException">Throw when the argument is not valid.</exception>
        /// <exception cref="System.UriFormatException">Throw when the URI format is not valid.</exception>
        /// <exception cref="System.Net.WebException">Throw when the request has failed.</exception>
        public Uri ResolveInstanceName(string instanceName)
        {
            if (string.IsNullOrWhiteSpace(instanceName))
            {
                throw new ArgumentNullException("The instance name cannot be null or empty.");
            }

            string name = instanceName.ToLower();
            if (name.StartsWith("http://") || name.StartsWith("https://"))
            {
                return new Uri(instanceName);
            }

            string jsonString;
            try
            {
                jsonString = request(new Uri(string.Format("https://files.hornbill.com/instances/{0}/zoneinfo", instanceName)));
            } catch (Exception) {
                try
                {
                    //This is the backup URL incase the file.hornbill.com server is done
                    jsonString = request(new Uri(string.Format("https://files.hornbill.co/instances/{0}/zoneinfo", instanceName)));
                } catch (Exception backupError)
                {
                    throw new Exception(backupError.ToString());
                }
            }

            if  (string.IsNullOrWhiteSpace(jsonString))
            {
                throw new RequestFailureException("Invalid response from the server.");
            }
            JsonSerializer serializer = new JsonSerializer();
            var jsonObject = serializer.Deserialize(new JsonTextReader(new StringReader(jsonString))) as IJEnumerable<JToken>;
            if (jsonObject != null)
            {
                foreach (var json in jsonObject)
                {
                    if (json.Type == JTokenType.Property)
                    {
                        JProperty prop = json as JProperty;
                        if (string.Compare(prop.Name, "zoneinfo") == 0)
                        {
                            string instance = string.Empty, zone = string.Empty;
                            IJEnumerable<JToken> childrens = prop.Children();
                            foreach (var children in childrens)
                            {
                                if (children.Type == JTokenType.Object)
                                {
                                    JObject obj = children as JObject;
                                    if (obj.HasValues == false)
                                        return null;
                                    string message = obj.Value<string>("message");
                                    if(string.IsNullOrWhiteSpace(message) || string.Compare(message, "Success", true) != 0)
                                    {
                                        throw new RequestFailureException(message);
                                    }

                                    string endPoint = obj.Value<string>("endpoint");
                                    if (string.IsNullOrWhiteSpace(endPoint) == false)
                                        return new Uri(endPoint);

                                    instance = obj.Value<string>("name");
                                    Debug.Write(instance);

                                    zone = obj.Value<string>("zone");
                                    if(string.IsNullOrWhiteSpace(zone) == false)
                                    {
                                        zone = zone.Replace("_", "");
                                    }
                                    Debug.Write(zone);
                                }
                            }
                            return new Uri(string.Format("https://{0}api.hornbill.com/{1}", zone, instance));
                        }
                    }
                }
            }
            return null;
        }
        #endregion
    }
    #endregion

    #endregion
}
