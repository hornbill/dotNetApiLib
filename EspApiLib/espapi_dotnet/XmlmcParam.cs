using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hornbill
{
    /// <summary>
    /// Represents a character encoding.
    /// </summary>
    public enum XmlmcEncoding
    {
        /// <summary>
        /// Encoding for the ASCII (7-bit) character set.
        /// </summary>
        ASCII,
        /// <summary>
        /// Encoding for the Base64 format.
        /// </summary>
        Base64
        //UTF8
    }

    /// <summary>
    /// XmlmcParam class.
    /// </summary>
    public class XmlmcParam
    {
        #region Fields
        private string _name;
        private string _value;
        private XmlmcEncoding _encoding = XmlmcEncoding.ASCII;
        private bool _inCData = false;
        private List<XmlmcParam> _childrens;
        #endregion

        #region Construtors
        /// <summary>
        /// Creates a new XmlmcParam.
        /// </summary>
        public XmlmcParam()
        {
            
        }

        /// <summary>
        /// Creates a new XmlmcParam.
        /// </summary>
        /// <param name="name">The name of the parameter.</param>
        public XmlmcParam(string name)
        {
            validateParamName(name);
            initParams(name, string.Empty);
        }

        /// <summary>
        /// Creates a new XmlmcParam.
        /// </summary>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="inParams">The value of the parameter.</param>
        public XmlmcParam(string name, List<XmlmcParam> inParams)
        {
            validateParamName(name);
            if (inParams == null)
            {
                initParams(name, string.Empty);
            }
            else
            {
                this._name = name;
                foreach(XmlmcParam param in inParams)
                {
                    this.Children.Add(param);
                }
            }
        }

        /// <summary>
        /// Creates a new XmlmcParam.
        /// </summary>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="inParam">The value of the parameter.</param>
        public XmlmcParam(string name, XmlmcParam inParam)
        {
            validateParamName(name);
            if(inParam == null)
            {
                initParams(name, string.Empty);
            }
            else
            {
                this._name = name;
                this._value = string.Empty;
                this.Children.Add(inParam);
            }
        }

        /// <summary>
        /// Creates a new XmlmcParam.
        /// </summary>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="value">The value of the parameter.</param>
        public XmlmcParam(string name, string value)
        {
            initParams(name, value);
        }

        /// <summary>
        /// Creates a new XmlmcParam.
        /// </summary>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="value">The value of the parameter.</param>
        public XmlmcParam(string name, float value) : this(name, string.Format("{0}", value))
        {
            
        }

        /// <summary>
        /// Creates a new XmlmcParam.
        /// </summary>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="value">The value of the parameter.</param>
        public XmlmcParam(string name, long value) : this(name, string.Format("{0}", value))
        {
        }

        /// <summary>
        /// Creates a new XmlmcParam.
        /// </summary>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="value">The value of the parameter.</param>
        public XmlmcParam(string name, double value) : this(name, string.Format("{0}", value))
        {
        }

        /// <summary>
        /// Creates a new XmlmcParam.
        /// </summary>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="value">The value of the parameter.</param>
        public XmlmcParam(string name, int value) : this(name, string.Format("{0}", value))
        {
        }

        /// <summary>
        /// Creates a new XmlmcParam.
        /// </summary>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="value">The value of the parameter.</param>
        public XmlmcParam(string name, bool value) : this(name, value ? "true" : "false")
        {
        }

        /// <summary>
        /// Creates a new XmlmcParam.
        /// </summary>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="value">The value of the parameter.</param>
        public XmlmcParam(string name, DateTime value)
        {
            initParams(name, value != null ? string.Format("{0:u}", value) : string.Empty);
        }

        private void initParams(string name, string value)
        {
            validateParamName(name);
            this.Name = name;
            this.Value = value;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets or Sets the name of the parameter
        /// </summary>
        public string Name
        {
            get { return _name; }
            set {
                _name = value; }
        }

        /// <summary>
        /// Gets or Sets the value of the parameter
        /// </summary>
        public string Value
        {
            get { return _value; }
            set { _value = value; }
        }

        /// <summary>
        /// Gets the encoding of the parameter
        /// </summary>
        public XmlmcEncoding Encoding
        {
            get { return _encoding; }
        }

        /// <summary>
        /// Returns true if the parameter is in CDATA block
        /// </summary>
        public bool IsInCData { get { return _inCData; } }

        /// <summary>
        /// Gets or Sets the parameter list
        /// </summary>
        public List<XmlmcParam> Children
        {
            get {
                if (_childrens == null)
                {
                    _childrens = new List<XmlmcParam>();
                }
                return _childrens;
            }
            set { _childrens = value; }
        }

        /// <summary>
        /// Gets the count of the parameters
        /// </summary>
        public int ChildrentCount
        {
            get { return _childrens != null ? _childrens.Count : 0; }
        }

        /// <summary>
        /// Gets the XML value of the parameters
        /// </summary>
        public string XmlValue
        {
            get
            {
                string xml = string.Empty;
                if(_childrens != null && _childrens.Count > 0)
                {
                    // ignore the value of this param
                    foreach(XmlmcParam children in _childrens)
                    {
                        xml += children.XmlValue;
                    }
                    xml = string.Format("<{0}>{1}</{0}>", _name, xml);
                }
                else
                {
                    string v = string.Empty;
                    switch (_encoding)
                    {
                        case XmlmcEncoding.Base64:
                            v = stringHelper.Base64Encode(_value);
                            break;
                            /*
                        case XmlmcEncoding.UTF8:
                            v = stringHelper.PrepareForXml(stringHelper.UTF8Encode(v));
                            break;
                            */
                        default:
                            v = _value;
                            break;
                    }
                    if (v == null)
                    {
                        xml = string.Format("<{0} nil=\"true\"/>", stringHelper.PrepareForXml(_name));
                    }
                    else
                    {
                        if (_inCData)
                        {
                            v = string.Format("<![CDATA[{0}]]>", v);
                        }
                        else
                        {
                            v = stringHelper.PrepareForXml(v);
                        }
                        xml = string.Format("<{0}>{1}</{0}>", stringHelper.PrepareForXml(_name), v);
                    }
                }

                return xml;
            }
        }
        #endregion

        #region InCData
        /// <summary>
        /// Create a CDATA block for the parameters.
        /// </summary>
        /// <param name="putInCDataBlock"><b>True</b> to put the parameters in a CDATA block</param>
        public void inCData(bool putInCDataBlock = true)
        {
            _inCData = putInCDataBlock;
        }
        #endregion

        #region EncodeValue
        /// <summary>
        /// Encodes the value of the parameter.
        /// </summary>
        /// <param name="encoding">The encoding for the parameters (Currently only supports Base64)</param>
        public void EncodeValue(XmlmcEncoding encoding)
        {
            _encoding = encoding;
        }
        #endregion

        #region DecodeValue
        /// <summary>
        /// Decodes the value of the parameter.
        /// </summary>
        /// <returns>The value of the parameter.</returns>
        public string DecodeValue()
        {
            if (_value == null)
                return string.Empty;
            switch(_encoding)
            {
                case XmlmcEncoding.Base64:
                    return stringHelper.Base64Decode(_value);
                /*
            case XmlmcEncoding.UTF8:
                return stringHelper.UTF8Decode(stringHelper.GetBytes(_value));
            */
                default:
                    return _value;
            }
        }
        #endregion

        #region Add
        private void validateParamName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException("The name cannot be null or empty.");
            }
        }

        /// <summary>
        /// Add parameters to the XMLMC method call.
        /// </summary>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="value">The value of the parameter.</param>
        /// <returns>The XMLMC parameter object.</returns>
        public XmlmcParam Add(string name, string value)
        {
            XmlmcParam param = new XmlmcParam(name, value);
            Children.Add(param);
            return param;
        }

        /// <summary>
        /// Add parameters to the XMLMC method call.
        /// </summary>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="value">The value of the parameter.</param>
        /// <returns>The XMLMC parameter object.</returns>
        public XmlmcParam Add(string name, bool value)
        {
            XmlmcParam param = new XmlmcParam(name, value);
            Children.Add(param);
            return param;
        }

        /// <summary>
        /// Add parameters to the XMLMC method call.
        /// </summary>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="value">The value of the parameter.</param>
        /// <returns>The XMLMC parameter object.</returns>
        public XmlmcParam Add(string name, DateTime value)
        {
            XmlmcParam param = new XmlmcParam(name, value);
            Children.Add(param);
            return param;
        }

        /// <summary>
        /// Add parameters to the XMLMC method call.
        /// </summary>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="value">The value of the parameter.</param>
        /// <returns>The XMLMC parameter object.</returns>
        public XmlmcParam Add(string name, double value)
        {
            XmlmcParam param = new XmlmcParam(name, value);
            Children.Add(param);
            return param;
        }

        /// <summary>
        /// Add parameters to the XMLMC method call.
        /// </summary>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="value">The value of the parameter.</param>
        /// <returns>The XMLMC parameter object.</returns>
        public XmlmcParam Add(string name, float value)
        {
            XmlmcParam param = new XmlmcParam(name, value);
            Children.Add(param);
            return param;
        }

        /// <summary>
        /// Add parameters to the XMLMC method call.
        /// </summary>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="value">The value of the parameter.</param>
        /// <returns>The XMLMC parameter object.</returns>
        public XmlmcParam Add(string name, long value)
        {
            XmlmcParam param = new XmlmcParam(name, value);
            Children.Add(param);
            return param;
        }

        /// <summary>
        /// Add parameters to the XMLMC method call.
        /// </summary>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="value">The value of the parameter.</param>
        /// <returns>The XMLMC parameter object.</returns>
        public XmlmcParam Add(string name, int value)
        {
            XmlmcParam param = new XmlmcParam(name, value);
            Children.Add(param);
            return param;
        }
        #endregion

        #region InsertAt
        /// <summary>
        /// Inserts an parameter at a specified index.
        /// </summary>
        /// <param name="index">An integer index of the paramater.</param>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="value">The value of the parameter.</param>
        /// <returns>The XMLMC parameter object.</returns>
        public XmlmcParam InsertAt(int index, string name, string value)
        {
            XmlmcParam param = new XmlmcParam(name, value);
            Children.Insert(index, param);
            return param;
        }

        /// <summary>
        /// Inserts an parameter at a specified index.
        /// </summary>
        /// <param name="index">An integer index of the paramater.</param>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="value">The value of the parameter.</param>
        /// <returns>The XMLMC parameter object.</returns>
        public XmlmcParam InsertAt(int index, string name, bool value)
        {
            XmlmcParam param = new XmlmcParam(name, value);
            Children.Insert(index, param);
            return param;
        }

        /// <summary>
        /// Inserts an parameter at a specified index.
        /// </summary>
        /// <param name="index">An integer index of the paramater.</param>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="value">The value of the parameter.</param>
        /// <returns>The XMLMC parameter object.</returns>
        public XmlmcParam InsertAt(int index, string name, long value)
        {
            XmlmcParam param = new XmlmcParam(name, value);
            Children.Insert(index, param);
            return param;
        }

        /// <summary>
        /// Inserts an parameter at a specified index.
        /// </summary>
        /// <param name="index">An integer index of the paramater.</param>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="value">The value of the parameter.</param>
        /// <returns>The XMLMC parameter object.</returns>
        public XmlmcParam InsertAt(int index, string name, float value)
        {
            XmlmcParam param = new XmlmcParam(name, value);
            Children.Insert(index, param);
            return param;
        }

        /// <summary>
        /// Inserts an parameter at a specified index.
        /// </summary>
        /// <param name="index">An integer index of the paramater.</param>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="value">The value of the parameter.</param>
        /// <returns>The XMLMC parameter object.</returns>
        public XmlmcParam Insert(int index, string name, double value)
        {
            XmlmcParam param = new XmlmcParam(name, value);
            Children.Insert(index, param);
            return param;
        }

        /// <summary>
        /// Inserts an parameter at a specified index.
        /// </summary>
        /// <param name="index">An integer index of the paramater.</param>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="value">The value of the parameter.</param>
        /// <returns>The XMLMC parameter object.</returns>
        public XmlmcParam InsertAt(int index, string name, int value)
        {
            XmlmcParam param = new XmlmcParam(name, value);
            Children.Insert(index, param);
            return param;
        }

        /// <summary>
        /// Inserts an parameter at a specified index.
        /// </summary>
        /// <param name="index">An integer index of the paramater.</param>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="value">The value of the parameter.</param>
        /// <returns>The XMLMC parameter object.</returns>
        public XmlmcParam InsertAt(int index, string name, DateTime value)
        {
            XmlmcParam param = new XmlmcParam(name, value);
            Children.Insert(index, param);
            return param;
        }
        #endregion
    }
}
