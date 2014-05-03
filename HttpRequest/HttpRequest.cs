using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;

namespace Muka
{
    public class HttpRequest
    {
        #region Public Properties

        public HttpWebRequest Request
        {
            get { return _request; }
        }

        public CookieContainer Cookies
        {
            get { return _cookieJar; }
            set { _cookieJar = value; }
        }

        public bool SupressWebExceptions
        {
            get { return _supressWebExceptions; }
            set { _supressWebExceptions = value; }
        }

        public Encoding DefaultEncoding
        {
            get { return _defaultEncoding; }
            set { _defaultEncoding = value; }
        }

        public string RawPostData
        {
            get { return _rawPostData; }
            set { _rawPostData = value; }
        }

        public Dictionary<string, string> PostData
        {
            get { return _postData; }
            set { _postData = value; }
        }

        #endregion

        #region Private Properties

        private HttpWebRequest _request;
        private CookieContainer _cookieJar;
        private bool _supressWebExceptions;
        private Encoding _defaultEncoding;
        private string _rawPostData;
        private Dictionary<string, string> _postData;

        #endregion

        #region Constructors

        public HttpRequest(string url) : this(new Uri(url)) { }

        public HttpRequest(Uri uri)
        {
            _supressWebExceptions = false;
            _defaultEncoding = Encoding.UTF8;
            _rawPostData = string.Empty;
            _postData = new Dictionary<string, string>();

            _request = (HttpWebRequest)WebRequest.Create(uri);
            _request.UserAgent = "Mozilla/5.0 (Windows NT 5.1; rv:31.0) Gecko/20100101 Firefox/31.0";
            _request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            _request.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip,deflate");

            _cookieJar = new CookieContainer();
            _request.CookieContainer = _cookieJar;

        }

        #endregion

        #region Public Methods

        public string DownloadString()
        {
            PreparePost();

            try
            {
                using (HttpWebResponse response = (HttpWebResponse)_request.GetResponse())
                {
                    _cookieJar.Add(response.Cookies);

                    Encoding responseEncoding;
                    try
                    {
                        responseEncoding = Encoding.GetEncoding(response.CharacterSet);
                    }
                    catch(ArgumentException)
                    {
                        responseEncoding = _defaultEncoding;
                    }

                    using (Stream dataStream = response.GetResponseStream())
                    using (StreamReader reader = new StreamReader(dataStream, responseEncoding))
                    {
                        return reader.ReadToEnd();
                    }
                }
            }
            catch (WebException)
            {
                if (_supressWebExceptions)
                    return string.Empty;
                else
                    throw;
            }
        }

        public int DownloadFile(string filePath)
        {
            byte[] buffer = new byte[1024];
            int fileSize = 0;
            int bytesRead = 0;

            try
            {
                using (HttpWebResponse response = (HttpWebResponse)_request.GetResponse())
                {
                    _cookieJar.Add(response.Cookies);

                    using (Stream responseStream = response.GetResponseStream())
                    using (Stream fileStream = File.Create(filePath))
                    {
                        do
                        {
                            bytesRead = responseStream.Read(buffer, 0, buffer.Length);
                            fileStream.Write(buffer, 0, bytesRead);
                            fileSize += bytesRead;
                        } while (bytesRead > 0);
                    }
                }

                return fileSize;
            }
            catch (WebException)
            {
                if (_supressWebExceptions)
                    return -1;
                else
                    throw;
            }
        }

        #endregion

        #region Private Methods

        private void PreparePost()
        {
            if(_rawPostData.Length > 0 || _postData.Count > 0)
            {
                _request.Method = "POST";

                string post = string.Empty;

                foreach (KeyValuePair<string, string> kvp in _postData)
                    post += string.Format("{0}={1}&", Uri.EscapeUriString(kvp.Key), Uri.EscapeUriString(kvp.Value));

                post += _rawPostData;

                byte[] byteArray = Encoding.UTF8.GetBytes(post);
                _request.ContentType = "application/x-www-form-urlencoded";
                _request.ContentLength = byteArray.Length;

                using (Stream dataStream = _request.GetRequestStream())
                    dataStream.Write(byteArray, 0, byteArray.Length);
            }
        }

        #endregion
    }


}
