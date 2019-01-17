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
        private static List<string> _lstGeoJSONPoints = null;
        private static List<string> _lstGeoJSONMBRs = null;


        static dynamic _result = null;
        private static string _latestURL = "";
        private static string _serverURL = "";
        private static string _userName = "";
        private static string _passWord = "";
        private static byte[] _aditionalEntropy = { 9, 4, 2, 6, 9 };
        #endregion

        //********************************************************************************************************
        #region Initialisation
        //********************************************************************************************************
        //********************************************************************************************************
        public static void Initiate(int timeout, string serverURL, string userName, string passWord)
        {
                if (serverURL == "" || serverURL == string.Empty)
                {
                    _serverURL = "http://localhost:8080/rest/CustomFind/results.json";
                }
                else
                {
                    _serverURL = serverURL;
                }

                if (userName != "")
                {
                    _userName = userName;
                    _passWord = passWord;
                }
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
        public static string GetResultDetails(   int element, ref string geojsonPoint, ref string geojsonMBR)
        {
            geojsonPoint = _lstGeoJSONPoints[element];
            geojsonMBR = _lstGeoJSONMBRs[element];

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
    }
}
