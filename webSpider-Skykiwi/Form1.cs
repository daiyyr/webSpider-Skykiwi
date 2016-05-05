using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Globalization;
using System.Net.Cache;
using System.Collections;

namespace webSpider_Skykiwi
{
    public partial class Form1 : Form
    {

        /* Utils
         ************************* 
         *************************
         *************************
         */

        public static int timeoutTime = 4000;
        public static bool gForceToStop = false;
        static string gHost = "bbs.skykiwi.com";
        public static bool debug = false;
        public static int retry = 3;

        public delegate void setLog(string str1);
        public void setLogT(string s)
        {
            if (logT.InvokeRequired)
            {
                // 实例一个委托，匿名方法，
                setLog sl = new setLog(delegate(string text)
                {
                    logT.AppendText(DateTime.Now.ToString() + " " + text + Environment.NewLine);
                });
                // 把调用权交给创建控件的线程，带上参数
                logT.Invoke(sl, s);
            }
            else
            {
                logT.AppendText(DateTime.Now.ToString() + " " + s + Environment.NewLine);
            }
        }

        public void setLogtRed(string s)//something wrong, if it's first line, no red
        {
            if (logT.InvokeRequired)
            {
                setLog sl = new setLog(delegate(string text)
                {
                    logT.AppendText(DateTime.Now.ToString() + " " + text + Environment.NewLine);
                    int i = logT.Text.LastIndexOf("\n", logT.Text.Length - 2);
                    if (i > 1)
                    {
                        logT.Select(i, logT.Text.Length);
                        logT.SelectionColor = Color.Red;
                        logT.Select(i, logT.Text.Length);
                        logT.SelectionFont = new Font(logT.Font, FontStyle.Bold);
                    }
                });
                logT.Invoke(sl, s);
            }
            else
            {
                logT.AppendText(DateTime.Now.ToString() + " " + s + Environment.NewLine);
                int i = logT.Text.LastIndexOf("\n", logT.Text.Length - 2);
                if (i > 1)
                {
                    logT.Select(i, logT.Text.Length);
                    logT.SelectionColor = Color.Red;
                    logT.Select(i, logT.Text.Length);
                    logT.SelectionFont = new Font(logT.Font, FontStyle.Bold);
                }
            }
        }

        public int downloadInformation(string prex, string content)
        {
            if (downloadFileName == "")
            {
                downloadFileName = prex + "." + System.DateTime.Now.ToString("yyyyMMddHHmmss", DateTimeFormatInfo.InvariantInfo) + ".csv";
            }
            writeFile(System.Environment.CurrentDirectory + "\\" + downloadFileName, content);
            return 1;
        }


        public static string ToUrlEncode(string strCode)
        {
            StringBuilder sb = new StringBuilder();
            byte[] byStr = System.Text.Encoding.UTF8.GetBytes(strCode); //默认是System.Text.Encoding.Default.GetBytes(str)  
            System.Text.RegularExpressions.Regex regKey = new System.Text.RegularExpressions.Regex("^[A-Za-z0-9]+$");
            for (int i = 0; i < byStr.Length; i++)
            {
                string strBy = Convert.ToChar(byStr[i]).ToString();
                if (regKey.IsMatch(strBy))
                {
                    //是字母或者数字则不进行转换    
                    sb.Append(strBy);
                }
                else
                {
                    sb.Append(@"%" + Convert.ToString(byStr[i], 16));
                }
            }
            return (sb.ToString());
        }

        public static string ToUrlEncode(string strCode, System.Text.Encoding encode)
        {
            StringBuilder sb = new StringBuilder();
            byte[] byStr = encode.GetBytes(strCode); //默认是System.Text.Encoding.Default.GetBytes(str)  
            System.Text.RegularExpressions.Regex regKey = new System.Text.RegularExpressions.Regex("^[A-Za-z0-9]+$");
            for (int i = 0; i < byStr.Length; i++)
            {
                string strBy = Convert.ToChar(byStr[i]).ToString();
                if (regKey.IsMatch(strBy))
                {
                    //是字母或者数字则不进行转换    
                    sb.Append(strBy);
                }
                else
                {
                    sb.Append(@"%" + Convert.ToString(byStr[i], 16));
                }
            }
            return (sb.ToString());
        }

        public static void writeFile(string file, string content)
        {
            FileStream aFile;
            StreamWriter sw;
            aFile = new FileStream(file, FileMode.Append);
            //sw = new StreamWriter(aFile);
            sw = new StreamWriter(aFile, Encoding.GetEncoding("GB2312"));
            sw.Write(content);
            sw.Close();
        }




        public static void setRequest(HttpWebRequest req, CookieCollection cookies)
        {
            //req.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
            //req.Accept = "*/*";
            //req.Connection = "keep-alive";
            //req.KeepAlive = true;
            //req.UserAgent = "Mozilla/4.0 (compatible; MSIE 8.0; Windows NT 6.1; WOW64; Trident/4.0; SLCC2; .NET CLR 2.0.50727; .NET CLR 3.5.30729; .NET CLR 3.0.30729; Media Center PC 6.0; InfoPath.3; .NET4.0C; .NET4.0E";
            //req.Headers["Accept-Encoding"] = "gzip, deflate";
            //req.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;



            req.Timeout = timeoutTime;

            req.Host = gHost;

            req.UserAgent = "Mozilla/5.0 (Macintosh; Intel Mac OS X 10.10; rv:40.0) Gecko/20100101 Firefox/40.0";
            req.AllowAutoRedirect = false;
            req.CookieContainer = new CookieContainer();
            req.CookieContainer.PerDomainCapacity = 40;
            if (cookies != null)
            {
                req.CookieContainer.Add(cookies);
            }
            req.ContentType = "application/x-www-form-urlencoded";
        }

        public static int writePostData(Form1 form1, HttpWebRequest req, string Encode)
        {
            byte[] postBytes = Encoding.UTF8.GetBytes(Encode);
            //req.ContentLength = postBytes.Length;  // cause InvalidOperationException: 写入开始后不能设置此属性。
            Stream postDataStream = null;
            try
            {
                postDataStream = req.GetRequestStream();
                postDataStream.Write(postBytes, 0, postBytes.Length);
            }
            catch (WebException webEx)
            {
                form1.setLogT("While writing post data," + webEx.Status.ToString());
                return -1;
            }

            postDataStream.Close();
            return 1;
        }

        public static string resp2html(HttpWebResponse resp)
        {

            if (resp.StatusCode == HttpStatusCode.OK || resp.StatusCode == HttpStatusCode.Found)
            {
                StreamReader stream = new StreamReader(resp.GetResponseStream());
                return stream.ReadToEnd();
                //Shift_JIS
            }
            else
            {
                return resp.StatusDescription;
            }

        }

        public static string resp2html(HttpWebResponse resp, string charSet, Form1 form1)
        {
            var buffer = GetBytes(form1, resp);
            if (resp.StatusCode == HttpStatusCode.OK || resp.StatusCode == HttpStatusCode.Found)
            {
                if (String.IsNullOrEmpty(charSet) || string.Compare(charSet, "ISO-8859-1") == 0)
                {
                    charSet = GetEncodingFromBody(buffer);
                }

                try
                {
                    var encoding = Encoding.GetEncoding(charSet);  //Shift_JIS
                    var str = encoding.GetString(buffer);

                    return str;
                }
                catch (Exception ex)
                {
                    form1.setLogT("resp2html, " + ex.ToString());
                    return string.Empty;
                }


                /*
                string respHtml = "";
                char[] cbuffer = new char[256];
                Stream respStream = resp.GetResponseStream();
                StreamReader respStreamReader = new StreamReader(respStream, encoding);//respStream,Encoding.UTF8
                int byteRead = 0;
                try
                {
                    byteRead = respStreamReader.Read(cbuffer, 0, 256);

                }
                catch (WebException webEx)
                {
                    setLogT("respStreamReader, " + webEx.Status.ToString());
                    return "";
                }
                while (byteRead != 0)
                {
                    string strResp = new string(cbuffer, 0, byteRead);
                    respHtml = respHtml + strResp;
                    try
                    {
                        byteRead = respStreamReader.Read(cbuffer, 0, 256);
                    }
                    catch (WebException webEx)
                    {
                        setLogT("respStreamReader, " + webEx.Status.ToString());
                        return "";
                    }

                }
                respStreamReader.Close();
                respStream.Close();
                return respHtml;

                */

            }
            else
            {
                return resp.StatusDescription;
            }

        }

        private static byte[] GetBytes(Form1 form1, WebResponse response)
        {
            var length = (int)response.ContentLength;
            byte[] data;

            using (var memoryStream = new MemoryStream())
            {
                var buffer = new byte[0x100];
                try
                {
                    using (var rs = response.GetResponseStream())
                    {
                        for (var i = rs.Read(buffer, 0, buffer.Length); i > 0; i = rs.Read(buffer, 0, buffer.Length))
                        {
                            memoryStream.Write(buffer, 0, i);
                        }
                    }
                }
                catch (Exception e)
                {
                    form1.setLogT("read ResponseStream: " + e.ToString()); //500
                }


                data = memoryStream.ToArray();
            }

            return data;
        }

        private static string GetEncodingFromBody(byte[] buffer)
        {
            var regex = new Regex(@"<meta(\s+)http-equiv(\s*)=(\s*""?\s*)content-type(\s*""?\s+)content(\s*)=(\s*)""text/html;(\s+)charset(\s*)=(\s*)(?<charset>[a-zA-Z0-9-]+?)""(\s*)(/?)>", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            var str = Encoding.ASCII.GetString(buffer);
            var regMatch = regex.Match(str);
            if (regMatch.Success)
            {
                var charSet = regMatch.Groups["charset"].Value;
                return charSet;
            }

            return Encoding.ASCII.BodyName;
        }

        /* 
         * return response status
         * especially, if found, return"found: http......"
         */
        public static string weLoveMuYue(Form1 form1, string url, string method, string referer, bool allowAutoRedirect, string postData, ref CookieCollection cookies)
        {
            string result;
            for (int i = 0; i < retry; i++)
            {
                if (gForceToStop)
                {
                    break;
                }
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
                HttpWebResponse resp = null;
                setRequest(req, cookies);
                req.Method = method;
                req.Referer = referer;
                if (allowAutoRedirect)
                {
                    req.AllowAutoRedirect = true;
                }
                if (method.Equals("POST"))
                {
                    if (writePostData(form1, req, postData) < 0)
                    {
                        continue;
                    }
                }
                try
                {
                    resp = (HttpWebResponse)req.GetResponse();
                }
                catch (WebException webEx)
                {
                    form1.setLogT("GetResponse, " + webEx.Status.ToString());
                    if (webEx.Status == WebExceptionStatus.ConnectionClosed)
                    {
                        form1.setLogT("wrong address"); //地址错误
                    }
                    if (webEx.Status == WebExceptionStatus.ProtocolError)
                    {
                        form1.setLogT("本次请求被服务器拒绝，可尝试调高间隔时间"); //500
                    }
                    continue;
                }
                if (resp != null)
                {
                    result = resp.StatusDescription;
                    if (result == "Found")
                    {
                        result += ":" + resp.Headers["location"];
                    }
                }
                else
                {
                    continue;
                }
                cookies = req.CookieContainer.GetCookies(req.RequestUri);
                resp.Close();
                return result;
            }
            return string.Empty;
        }

        /* unregular host
         * return response status
         * especially, if found, return"found: http......"
         * 
         */
        public static string weLoveMuYue(Form1 form1, string url, string method, string referer, bool allowAutoRedirect, string postData, ref CookieCollection cookies, string host)
        {
            string result;
            for (int i = 0; i < retry; i++)
            {
                if (gForceToStop)
                {
                    break;
                }
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
                HttpWebResponse resp = null;
                setRequest(req, cookies);
                req.Host = host;
                req.Method = method;
                req.Referer = referer;
                if (allowAutoRedirect)
                {
                    req.AllowAutoRedirect = true;
                }
                if (method.Equals("POST"))
                {
                    if (writePostData(form1, req, postData) < 0)
                    {
                        continue;
                    }
                }
                try
                {
                    resp = (HttpWebResponse)req.GetResponse();
                }
                catch (WebException webEx)
                {
                    form1.setLogT("GetResponse, " + webEx.Status.ToString());
                    if (webEx.Status == WebExceptionStatus.ConnectionClosed)
                    {
                        form1.setLogT("wrong address"); //地址错误
                    }
                    if (webEx.Status == WebExceptionStatus.ProtocolError)
                    {
                        form1.setLogT("本次请求被服务器拒绝，可尝试调高间隔时间"); //500
                    }
                    continue;
                }
                if (resp != null)
                {
                    result = resp.StatusDescription;
                    if (result == "Found")
                    {
                        result += ":" + resp.Headers["location"];
                    }
                }
                else
                {
                    continue;
                }
                cookies = req.CookieContainer.GetCookies(req.RequestUri);
                resp.Close();
                return result;
            }
            return string.Empty;
        }


        /* 
         * return response HTML
         */
        public static string weLoveYue(Form1 form1, string url, string method, string referer, bool allowAutoRedirect, string postData, ref CookieCollection cookies, bool responseInUTF8)
        {
            if (form1 == null)
            {
                return string.Empty;
            }
            string respHtml = "";
            for (int i = 0; i < retry; i++)
            {
                if (gForceToStop)
                {
                    break;
                }
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
                HttpWebResponse resp = null;
                setRequest(req, cookies);
                req.Method = method;
                req.Referer = referer;
                if (allowAutoRedirect)
                {
                    req.AllowAutoRedirect = true;
                }
                if (method.Equals("POST"))
                {
                    if (writePostData(form1, req, postData) < 0)
                    {
                        continue;
                    }
                }

                try
                {
                    resp = (HttpWebResponse)req.GetResponse();
                }
                catch (WebException webEx)
                {
                    form1.setLogT("GetResponse, " + webEx.Status.ToString());
                    if (webEx.Status == WebExceptionStatus.ConnectionClosed)
                    {
                        return "wrong address"; //地址错误
                    }
                    if (webEx.Status == WebExceptionStatus.ProtocolError)
                    {
                        form1.setLogT("本次请求被服务器拒绝，可尝试调高间隔时间"); //500
                    }
                    continue;
                }
                if (resp != null)
                {
                    if (responseInUTF8)
                    {
                        respHtml = resp2html(resp);
                    }
                    else
                    {
                        respHtml = resp2html(resp, resp.CharacterSet, form1); // like  Shift_JIS
                    }

                    if (respHtml.Equals(""))
                    {
                        continue;
                    }
                    cookies = req.CookieContainer.GetCookies(req.RequestUri);
                    resp.Close();
                    return respHtml;
                }
                else
                {
                    continue;
                }
            }
            return respHtml;
        }

        /* 
         * return responsive HTML
         * unregular host
         */
        public static string weLoveYue(Form1 form1, string url, string method, string referer, bool allowAutoRedirect, string postData, ref CookieCollection cookies, string host, bool responseInUTF8)
        {
            for (int i = 0; i < retry; i++)
            {
                if (gForceToStop)
                {
                    break;
                }
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
                HttpWebResponse resp = null;
                setRequest(req, cookies);
                req.Method = method;
                req.Referer = referer;
                if (allowAutoRedirect)
                {
                    req.AllowAutoRedirect = true;
                }
                req.Host = host;
                if (method.Equals("POST"))
                {
                    if (writePostData(form1, req, postData) < 0)
                    {
                        continue;
                    }
                }
                string respHtml = "";
                try
                {
                    resp = (HttpWebResponse)req.GetResponse();
                }
                catch (WebException webEx)
                {
                    form1.setLogT("GetResponse, " + webEx.Status.ToString());
                    if (webEx.Status == WebExceptionStatus.ConnectionClosed)
                    {
                        return "wrong address"; //地址错误
                    }
                    if (webEx.Status == WebExceptionStatus.ProtocolError)
                    {
                        form1.setLogT("本次请求被服务器拒绝，可尝试调高间隔时间"); //500
                    }
                    continue;
                }
                if (resp != null)
                {
                    if (responseInUTF8)
                    {
                        respHtml = resp2html(resp);
                    }
                    else
                    {
                        respHtml = resp2html(resp, resp.CharacterSet, form1); // like  Shift_JIS
                    }
                    if (respHtml.Equals(""))
                    {
                        continue;
                    }
                    cookies = req.CookieContainer.GetCookies(req.RequestUri);
                    resp.Close();
                    return respHtml;
                }
                else
                {
                    continue;
                }
            }
            return "";
        }

        /*
         * do not handle the response
         */
        public static HttpWebResponse weLoveYueer(Form1 form1, string url, string method, string referer, bool allowAutoRedirect, string postData, ref CookieCollection cookies)
        {
            HttpWebResponse resp = null;
            for (int i = 0; i < retry; i++)
            {
                if (gForceToStop)
                {
                    break;
                }
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
                setRequest(req, cookies);
                req.Method = method;
                req.Referer = referer;
                if (allowAutoRedirect)
                {
                    req.AllowAutoRedirect = true;
                }
                if (method.Equals("POST"))
                {
                    if (writePostData(form1, req, postData) < 0)
                    {
                        continue;
                    }
                }
                try
                {
                    resp = (HttpWebResponse)req.GetResponse();
                }
                catch (WebException webEx)
                {
                    form1.setLogT("GetResponse, " + webEx.Status.ToString());
                    if (webEx.Status == WebExceptionStatus.ConnectionClosed)
                    {
                        form1.setLogT("wrong address"); //地址错误
                    }
                    if (webEx.Status == WebExceptionStatus.ProtocolError)
                    {
                        form1.setLogT("本次请求被服务器拒绝，可尝试调高间隔时间"); //500
                    }
                    continue;
                }
                if (resp != null)
                {
                    cookies = req.CookieContainer.GetCookies(req.RequestUri);
                    return resp;
                }
                else
                {
                    continue;
                }
            }
            return resp;
        }

        /*
         * do not handle the response
         * with host
         */
        public static HttpWebResponse weLoveYueer(Form1 form1, string url, string method, string referer, bool allowAutoRedirect, string postData, ref CookieCollection cookies, string host)
        {
            HttpWebResponse resp = null;
            for (int i = 0; i < retry; i++)
            {
                if (gForceToStop)
                {
                    break;
                }
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
                setRequest(req, cookies);
                req.Method = method;
                req.Referer = referer;
                if (allowAutoRedirect)
                {
                    req.AllowAutoRedirect = true;
                }
                req.Host = host;
                if (method.Equals("POST"))
                {
                    if (writePostData(form1, req, postData) < 0)
                    {
                        continue;
                    }
                }
                try
                {
                    resp = (HttpWebResponse)req.GetResponse();
                }
                catch (WebException webEx)
                {
                    form1.setLogT("GetResponse, " + webEx.Status.ToString());
                    if (webEx.Status == WebExceptionStatus.ConnectionClosed)
                    {
                        form1.setLogT("wrong address"); //地址错误
                    }
                    if (webEx.Status == WebExceptionStatus.ProtocolError)
                    {
                        form1.setLogT("本次请求被服务器拒绝，可尝试调高间隔时间"); //500
                    }
                    continue;
                }
                if (resp != null)
                {
                    cookies = req.CookieContainer.GetCookies(req.RequestUri);
                    return resp;
                }
                else
                {
                    continue;
                }
            }
            return resp;
        }
        /* Utils end
         ************************* 
         *************************
         *************************
         */


















        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Thread t = new Thread(probe);
            t.Start();
        }

        public CookieCollection cookieContainer = null;
        string rgx;
        Match myMatch;
        List<Client> clients = new List<Client>();
        string downloadFileName = "";
        int clientCount = 0;
        List<string> phones = new List<string>();

        private void probe()
        {
            downloadInformation("clients", "姓名,电话,区域,地址,网页,是否已安装HRV" + "\n");

            for (int page = 1; page < 919; page++)
            {

               string respHtml = Form1.weLoveYue(
                  this,
                  "http://bbs.skykiwi.com/forum.php?mod=forumdisplay&fid=19&page=" + page,
                  "GET",
                  "",
                  false,
                  "",
                 ref cookieContainer,
                  true);

               rgx = (
                   page > 1 ? @"\[\<a(\s|\S)+?\<\/a\>\]\<\/em\> \<a(\s|\S)+?\<\/a\>"
                            : @"(?<=版块主题(\s|\S)+?)\[\<a(\s|\S)+?\<\/a\>\]\<\/em\> \<a(\s|\S)+?\<\/a\>"
                    );
               myMatch = (new Regex(rgx)).Match(respHtml);
               while (myMatch.Success)
               {
                   
                   string zone="", address="", href="", phone="";
                   Regex rex1 = new Regex(@"(?<=\>)(\s|\S)+?(?=\<\/a\>)");
                   Match match1;

                   match1 = rex1.Match(myMatch.Groups[0].Value);
                   if (match1.Success)
                   {
                       zone = match1.Groups[0].Value;
                   }

                   rex1 = new Regex(@"(?<=class=""xst"" \>)(\s|\S){0,10}[a-z0-9A-Z_]*");
                   match1 = rex1.Match(myMatch.Groups[0].Value);
                   if (match1.Success)
                   {
                       address = match1.Groups[0].Value.Replace(",", "，"); //半角逗号换成全角, 以免csv转Excel格式时出错
                   }

                   rex1 = new Regex(@"(?<=\<\/em\> \<a href\="")(\s|\S)+?(?="")");
                   match1 = rex1.Match(myMatch.Groups[0].Value);
                   if (match1.Success)
                   {
                       href = "http://bbs.skykiwi.com/" + match1.Groups[0].Value.Replace(";", "&");
                   }

                   respHtml = Form1.weLoveYue(
                      this,
                      href,
                      "GET",
                      "",
                      false,
                      "",
                        ref cookieContainer,
                        true);

                   rex1 = new Regex(@"(?<!\/)(?<!\d)(?<!\=)([a-zA-Z ]{0,10}|.先生\s*|.女士\s*|.小姐\s*|.经理\s*|微信\s*|电话\s*|手机\s*)[0-9 \-]{10,13}(?!\d)(\s*[a-zA-Z ]{0,10}|\s*.先生|.女士\s*|\s*.小姐|\s*.经理)");
                   match1 = rex1.Match(respHtml);
                   while (match1.Success)
                   {
                       //如果符合4个数字-, 则为日期, 略过
                       if(new Regex(@"\d{4}\-").IsMatch(match1.Groups[0].Value)
                           || phones.Contains(match1.Groups[0].Value)
                           )
                       {                       
                           match1 = match1.NextMatch();
                           continue;
                       }
                   
                   

                       phones.Add(match1.Groups[0].Value);
                       if (phone != "")
                       {
                           phone += ";";
                       }
                       phone += match1.Groups[0].Value;
                       match1 = match1.NextMatch();
                   }
                   string haveHRV ;
                   if (respHtml.Contains("HRV") 
                       || respHtml.Contains("hrv"))
                   {
                       haveHRV = "yes";
                   }
                   else
                   {
                       haveHRV = "not mention";
                   }

                   //写入
                   if (phone != "")
                   {
                       downloadInformation("clients", "," + phone + "," + zone + "," + address + "," + href + "," + haveHRV + "\n");
                   }
                   setLogT("download client " + (++clientCount).ToString());
                   myMatch = myMatch.NextMatch();
               }

            }

        }
    }
}
