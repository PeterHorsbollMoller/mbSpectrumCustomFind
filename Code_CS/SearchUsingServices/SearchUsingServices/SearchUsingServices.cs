using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace SearchUsingServices
{
    public class SpectrumCustomFind
    {
        //********************************************************************************************************
        #region Private Variables
        private static List<string> _lstDescriptions = null;
        private static List<string> _lstLatitudes = null;
        private static List<string> _lstLongitudes = null;

        static dynamic _result = null;
        private static string _latestURL = "";
        private static string _serverURL = "";
        private static string _userName = "";
        private static string _passWord = "";
        private static int _timeout = 5000;
        private static byte[] _aditionalEntropy = { 9, 4, 2, 6, 9 };
        #endregion

        //********************************************************************************************************
        #region Initialisation
        //********************************************************************************************************
        //********************************************************************************************************
        public static void Initiate(int timeout, string serverURL, string userName, string passWord)
        {
            _serverURL = serverURL;
       
            if (userName != "")
            {
                _userName = userName;
                _passWord = passWord;
            }

            if (timeout > 0)
                _timeout = timeout;
        }
        #endregion

        //********************************************************************************************************
        #region Search Methods
        //********************************************************************************************************
        //********************************************************************************************************
        public static int DoSearch(string valueSearchFor)
        {
            _latestURL = string.Empty;

            if (valueSearchFor != string.Empty)
                _latestURL = string.Format("{0}?Data.SearchValue={1}", _serverURL, valueSearchFor);
            else
               return 0;

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(_latestURL);

            request.Method = "GET";
            request.ContentLength = 0;
            request.ContentType = "text/xml";
            request.Timeout = _timeout;
            if (_userName != "")
                request.Credentials = new NetworkCredential(_userName, _passWord);

            try
            {
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                string responseValue = string.Empty;

                if (response.StatusCode != HttpStatusCode.OK)
                {
                    string message = String.Format("Request failed. Received HTTP {0}", response.StatusCode);
                    throw new ApplicationException(message);
                }

                using (Stream responseStream = response.GetResponseStream())
                {
                    using (var reader = new StreamReader(responseStream))
                    {
                        string responseBody = reader.ReadToEnd();

                        JObject jsonResult = JObject.Parse(responseBody);
                        JArray _results = jsonResult["Output"].Value<JArray>();

                        _lstDescriptions = new List<string>();
                        _lstLatitudes = new List<string>();
                        _lstLongitudes = new List<string>();

                        foreach (dynamic resultItem in _results)
                        {
                            if ((string)resultItem["Status_Code"] == "101")
                            {
                                //Ignore this record
                                //return -1;
                            }
                            else if ((string)resultItem["Status_Code"] == "NoMatchingRecordsFound")
                            {
                                //Ignore this record
                                //return -2;
                            }
                            else
                            { 
                                _lstDescriptions.Add((string)resultItem["VALUE"]);

                                _lstLatitudes.Add((string)resultItem["Y"]);
                                _lstLongitudes.Add((string)resultItem["X"]);
                            }
                        }
                        return _lstDescriptions.Count;
                    }
                }
            }
            catch (WebException e)
            {
                if (e.Status == WebExceptionStatus.ProtocolError)
                {
                    MessageBox.Show(String.Format("DoSearch: Status Description : {0} {1}", ((HttpWebResponse)e.Response).StatusDescription, e.Message));
                }
            }
            return 0;
        }
        #endregion
        
        //********************************************************************************************************
        #region Credentials
        //********************************************************************************************************
        //********************************************************************************************************
        public static void SaveCredentials()
        {
            // Data to protect. Convert a string to a byte[] using Encoding.UTF8.GetBytes().
            byte[] plaintext = Encoding.UTF8.GetBytes(_passWord);
            byte[] ciphertext = ProtectedData.Protect(plaintext, _aditionalEntropy, DataProtectionScope.CurrentUser);
        }

        //********************************************************************************************************
        //********************************************************************************************************
        public static void LoadCredentials()
        {
            byte[] plaintext = Encoding.UTF8.GetBytes(_passWord);
            byte[] ciphertext = ProtectedData.Unprotect(plaintext, _aditionalEntropy, DataProtectionScope.CurrentUser);
        }
        #endregion Credentials

        //********************************************************************************************************
        #region Server URL
        //********************************************************************************************************
        //********************************************************************************************************
        public static string GetServerURL()
        {
            return _serverURL;
        }
        //********************************************************************************************************
        //********************************************************************************************************
        public static string GetLatestURL()
        {
            return _latestURL;
        }
        #endregion

        //********************************************************************************************************
        #region Getting Result Values
        //********************************************************************************************************
        //********************************************************************************************************
        public static int GetResultDescriptions(string[] arrDescriptions)
        {
            int count = 0;
            foreach (string text in _lstDescriptions)
            {
                arrDescriptions[count] = text;
                count++;
            }

            return arrDescriptions.GetLength(0);
        }

        //********************************************************************************************************
        //********************************************************************************************************
        public static string GetResultAttribute(int element, string attributeName)
        {
              return _result[element][attributeName];
        }

        //********************************************************************************************************
        //********************************************************************************************************
        public static string GetResultDetails(   int element, ref string longitude, ref string latitude)
        {
            longitude = _lstLongitudes[element];
            latitude = _lstLatitudes[element];

            return _lstDescriptions[element];
        }

        //********************************************************************************************************
        //********************************************************************************************************
        public static string GetResultX(int element)
        {
            return _lstLongitudes[element];
        }

        //********************************************************************************************************
        //********************************************************************************************************
        public static string GetResultY(int element)
        {
            return _lstLatitudes[element];
        }
        #endregion
    }   //SpectrumCustomFind

    public class SpectrumMapFind
    {
        //********************************************************************************************************
        #region Private Variables
        private static List<string> _lstNames = null;
        private static List<string> _lstLatitudes = null;
        private static List<string> _lstLongitudes = null;
        private static List<string> _lstPB_ID = null;
        private static List<string> _lstAddressline = null;
        private static List<string> _lstZipcode = null;
        private static List<string> _lstAreaname = null;

        static dynamic _result = null;
        private static string _latestURL = "";
        private static string _serverURL = "";
        private static string _userName = "";
        private static string _passWord = "";
        private static int _timeout = 5000;
        private static byte[] _aditionalEntropy = { 9, 4, 2, 6, 9 };
        #endregion

        //********************************************************************************************************
        #region Initialisation
        //********************************************************************************************************
        //********************************************************************************************************
        public static void Initiate(int timeout, string serverURL, string userName, string passWord)
        {
            _serverURL = serverURL;

            if (userName != "")
            {
                _userName = userName;
                _passWord = passWord;
            }

            if (timeout > 0)
                _timeout = timeout;
        }
        #endregion

        //********************************************************************************************************
        #region Search Methods
        //********************************************************************************************************
        //********************************************************************************************************
        public static int DoSearch(string longitude, string latitude, string distance, string epsg)
        {
            _latestURL = string.Empty;

            _latestURL = string.Format("{0}?Data.longitude={1}&Data.latitude={2}&Data.distance={3}", _serverURL, longitude, latitude, distance);


            if (epsg != string.Empty)
                _latestURL = string.Format("{0}&Option.CoordSys={1}", _latestURL, epsg);

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(_latestURL);

            request.Method = "GET";
            request.ContentLength = 0;
            request.ContentType = "text/xml";
            request.Timeout = _timeout;
            if (_userName != "")
                request.Credentials = new NetworkCredential(_userName, _passWord);

            try
            {
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                string responseValue = string.Empty;

                if (response.StatusCode != HttpStatusCode.OK)
                {
                    string message = String.Format("Request failed. Received HTTP {0}", response.StatusCode);
                    throw new ApplicationException(message);
                }

                using (Stream responseStream = response.GetResponseStream())
                {
                    using (var reader = new StreamReader(responseStream))
                    {
                        string responseBody = reader.ReadToEnd();

                        JObject jsonResult = JObject.Parse(responseBody);
                        JArray _results = jsonResult["Output"].Value<JArray>();

                        _lstNames = new List<string>();
                        _lstLatitudes = new List<string>();
                        _lstLongitudes = new List<string>();
                        _lstPB_ID = new List<string>();
                        _lstAddressline = new List<string>();
                        _lstZipcode = new List<string>();
                        _lstAreaname = new List<string>();

                        foreach (dynamic resultItem in _results)
                        {
                            if ((string)resultItem["Status_Code"] == "101")
                            {
                                //Ignore this record
                                //return -1;
                            }
                            else if ((string)resultItem["Status_Code"] == "NoMatchingRecordsFound")
                            {
                                //Ignore this record
                                //return -2;
                            }
                            else
                            {
                                _lstNames.Add((string)resultItem["NAME"]);

                                _lstPB_ID.Add((string)resultItem["PB_ID"]);
                                _lstAddressline.Add((string)resultItem["MAINADDRESSLINE"]);
                                _lstZipcode.Add((string)resultItem["PostCode"]);
                                _lstAreaname.Add((string)resultItem["AreaName3"]);

                                _lstLatitudes.Add((string)resultItem["store_lat"]);
                                _lstLongitudes.Add((string)resultItem["store_lon"]);
                            }
                        }
                        return _lstNames.Count;
                    }
                }
            }
            catch (WebException e)
            {
                if (e.Status == WebExceptionStatus.ProtocolError)
                {
                    MessageBox.Show(String.Format("DoSearch: Status Description : {0} {1}", ((HttpWebResponse)e.Response).StatusDescription, e.Message));
                }
            }
            return 0;
        }
        #endregion

        //********************************************************************************************************
        #region Credentials
        //********************************************************************************************************
        //********************************************************************************************************
        public static void SaveCredentials()
        {
            // Data to protect. Convert a string to a byte[] using Encoding.UTF8.GetBytes().
            byte[] plaintext = Encoding.UTF8.GetBytes(_passWord);
            byte[] ciphertext = ProtectedData.Protect(plaintext, _aditionalEntropy, DataProtectionScope.CurrentUser);
        }

        //********************************************************************************************************
        //********************************************************************************************************
        public static void LoadCredentials()
        {
            byte[] plaintext = Encoding.UTF8.GetBytes(_passWord);
            byte[] ciphertext = ProtectedData.Unprotect(plaintext, _aditionalEntropy, DataProtectionScope.CurrentUser);
        }
        #endregion Credentials

        //********************************************************************************************************
        #region Server URL
        //********************************************************************************************************
        //********************************************************************************************************
        public static string GetServerURL()
        {
            return _serverURL;
        }
        //********************************************************************************************************
        //********************************************************************************************************
        public static string GetLatestURL()
        {
            return _latestURL;
        }
        #endregion

        //********************************************************************************************************
        #region Getting Result Values
        //********************************************************************************************************
        //********************************************************************************************************
        public static int GetResultNames(string[] arrDescriptions)
        {
            int count = 0;
            foreach (string text in _lstNames)
            {
                arrDescriptions[count] = text;
                count++;
            }

            return arrDescriptions.GetLength(0);
        }

        //********************************************************************************************************
        //********************************************************************************************************
        public static string GetResultAttribute(int element, string attributeName)
        {
            return _result[element][attributeName];
        }

        //********************************************************************************************************
        //********************************************************************************************************
        public static string GetResultDetails(int element, ref string longitude, ref string latitude)
        {
            longitude = _lstLongitudes[element];
            latitude = _lstLatitudes[element];

            return _lstNames[element];
        }

        //********************************************************************************************************
        //********************************************************************************************************
        public static string GetName(int element)
        {
            return _lstNames[element];
        }

        //********************************************************************************************************
        //********************************************************************************************************
        public static string GetPB_ID(int element)
        {
            return _lstPB_ID[element];
        }
        //********************************************************************************************************
        //********************************************************************************************************
        public static string GetAddressline(int element)
        {
            return _lstAddressline[element];
        }
        //********************************************************************************************************
        //********************************************************************************************************
        public static string GetZipcode(int element)
        {
            return _lstZipcode[element];
        }
        //********************************************************************************************************
        //********************************************************************************************************
        public static string GetAreaname(int element)
        {
            return _lstAreaname[element];
        }

        //********************************************************************************************************
        //********************************************************************************************************
        public static string GetResultX(int element)
        {
            return _lstLongitudes[element];
        }

        //********************************************************************************************************
        //********************************************************************************************************
        public static string GetResultY(int element)
        {
            return _lstLatitudes[element];
        }
        #endregion
    }   //SpectrumMapFind
}
