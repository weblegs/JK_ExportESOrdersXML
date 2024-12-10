using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Odbc;
using System.Net.Mail;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;
namespace WeblegsClasses
{
    public class Operations
    {

        public static DataSet ReadCSVFile(string path, string fileName)
        {
            DataSet dataSet = new DataSet();
            try
            {
                string connectionString = @"Driver={Microsoft Text Driver (*.txt; *.csv)}; DBQ=" + path + ";Extensions=asc,csv,tab,txt;Persist Security Info=False";
                string query = String.Format("select * from " + fileName);
                OdbcDataAdapter dataAdapter = new OdbcDataAdapter(query, connectionString);

                dataAdapter.Fill(dataSet);
                return dataSet;
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message);
                throw ex;
            }
        }



        /// <summary>
        /// Send Mail to Client and Error mails to developer
        /// </summary>
        /// <param name="SendTo"></param>
        /// <param name="Subject"></param>
        /// <param name="Body"></param>
        /// <returns></returns>
        public static bool SendMail(string SendTo, string Subject, string Body)
        {
            try
            {

                MailMessage mail = new MailMessage();
                mail.To.Add(SendTo);
                mail.From = new MailAddress("weblegs.developer1@gmail.com");
                mail.Subject = Subject;
                mail.Body = Body;
                mail.IsBodyHtml = true;
                SmtpClient smtp = new SmtpClient();
                smtp.Host = "SMTP.gmail.com";
                smtp.Port = 587;
                smtp.Credentials = new System.Net.NetworkCredential("weblegs.developer1@gmail.com", "tortoise1");
                smtp.EnableSsl = true;
                smtp.Send(mail);
            }
            catch
            {
                return false;
            }
            return true;
        }

        public static bool UploadFileToChannel(string ftpPath, string localFilePath, string userName, string password, bool isPassive)
        {
            try
            {
                FtpWebRequest Request = (FtpWebRequest)WebRequest.Create(ftpPath);
                Request.Credentials = new NetworkCredential(userName, password);
                Request.Method = WebRequestMethods.Ftp.UploadFile;
                Request.UsePassive = isPassive;
                Request.UseBinary = true;
                Request.KeepAlive = false;

                int length = 2048, contentsRead = 0;

                FileInfo FInfo = new FileInfo(localFilePath);
                Stream WriteStream = Request.GetRequestStream();
                byte[] dataBuffer = new byte[length];



                FileStream strReader = FInfo.OpenRead();
                contentsRead = strReader.Read(dataBuffer, 0, length);

                while (contentsRead > 0)
                {
                    WriteStream.Write(dataBuffer, 0, dataBuffer.Length);
                    dataBuffer = new byte[length];
                    contentsRead = strReader.Read(dataBuffer, 0, length);
                }




                strReader.Close();


                WriteStream.Close();

                FtpWebResponse response = (FtpWebResponse)Request.GetResponse();
                string sResult = response.StatusDescription;
                if (sResult.Contains("226"))
                    return true;
                else
                    return false;
            }
            catch (Exception ex)
            {
                throw ex;
            }


        }

        public static bool UploadFileToChannelSecured(string ftpPath, string localFilePath, string userName, string password, bool isPassive, bool isSecured)
        {
            try
            {
                FtpWebRequest Request = (FtpWebRequest)WebRequest.Create(ftpPath);
                Request.Credentials = new NetworkCredential(userName, password);
                Request.Method = WebRequestMethods.Ftp.UploadFile;
                Request.UsePassive = isPassive;
                Request.UseBinary = true;
                Request.KeepAlive = false;
                Request.EnableSsl = isSecured;
                int length = 2048, contentsRead = 0;

                FileInfo FInfo = new FileInfo(localFilePath);
                Stream WriteStream = Request.GetRequestStream();
                byte[] dataBuffer = new byte[length];



                FileStream strReader = FInfo.OpenRead();
                contentsRead = strReader.Read(dataBuffer, 0, length);

                while (contentsRead > 0)
                {
                    WriteStream.Write(dataBuffer, 0, dataBuffer.Length);
                    dataBuffer = new byte[length];
                    contentsRead = strReader.Read(dataBuffer, 0, length);
                }




                strReader.Close();


                WriteStream.Close();

                FtpWebResponse response = (FtpWebResponse)Request.GetResponse();
                string sResult = response.StatusDescription;
                if (sResult.Contains("226"))
                    return true;
                else
                    return false;
            }
            catch (Exception ex)
            {
                throw ex;
            }


        }

        public static bool DownloadFileFromFtp(string ftpPath, string savePath, string userName, string password, bool isPassive)
        {
            try
            {
                FtpWebRequest Request = (FtpWebRequest)WebRequest.Create(ftpPath);
                Request.Credentials = new NetworkCredential(userName, password);
                Request.Method = WebRequestMethods.Ftp.DownloadFile;
                Request.UsePassive = isPassive;
                Request.UseBinary = true;
                Request.KeepAlive = false;

                int length = 1, contentsRead = 0;

                FtpWebResponse response = (FtpWebResponse)Request.GetResponse();
                long ContentLength = GetFileSizeFromFtp(ftpPath, userName, password, isPassive, false);

                Stream ResStream = response.GetResponseStream();



                FileStream strWriter = new FileStream(savePath, FileMode.Create);

                if ((int)ContentLength < length)
                {
                    length = (int)ContentLength;
                    ContentLength = 0;
                }
                //else
                //    ContentLength = ContentLength - (long)length;

                byte[] dataBuffer = new byte[length];
                contentsRead = ResStream.Read(dataBuffer, 0, length);

                while (contentsRead > 0)
                {
                    strWriter.Write(dataBuffer, 0, dataBuffer.Length);


                    if (ContentLength < (long)length)
                    {
                        length = (int)ContentLength;
                        ContentLength = 0;
                    }
                    else
                        ContentLength = ContentLength - ((long)length);

                    dataBuffer = new byte[length];
                    contentsRead = ResStream.Read(dataBuffer, 0, length);
                    //DataGet += contentsRead;

                }

                ResStream.Close();
                strWriter.Close();
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public static long GetFileSizeFromFtp(string ftpPath, string userName, string password, bool isPassive, bool ISsecured)
        {
            FtpWebRequest Request = (FtpWebRequest)WebRequest.Create(ftpPath);
            Request.Credentials = new NetworkCredential(userName, password);
            Request.Method = WebRequestMethods.Ftp.GetFileSize;
            Request.UsePassive = isPassive;
            Request.UseBinary = true;
            Request.KeepAlive = false;
            Request.EnableSsl = ISsecured;

            int length = 2048, contentsRead = 0;

            FtpWebResponse response = (FtpWebResponse)Request.GetResponse();
            long ContentLength = response.ContentLength;
            response.Close();
            Request.Abort();
            return ContentLength;
        }

        public static bool DownloadFileFromFtp(string ftpPath, string savePath, string userName, string password, bool isPassive, bool isSecured)
        {
            try
            {
                FtpWebRequest Request = (FtpWebRequest)WebRequest.Create(ftpPath);
                Request.Credentials = new NetworkCredential(userName, password);
                Request.Method = WebRequestMethods.Ftp.DownloadFile;
                Request.UsePassive = isPassive;
                Request.UseBinary = true;
                Request.KeepAlive = false;
                Request.EnableSsl = isSecured;

                int length = 1, contentsRead = 0;

                FtpWebResponse response = (FtpWebResponse)Request.GetResponse();
                long ContentLength = GetFileSizeFromFtp(ftpPath, userName, password, isPassive, isSecured);

                Stream ResStream = response.GetResponseStream();



                FileStream strWriter = new FileStream(savePath, FileMode.Create);

                if ((int)ContentLength < length)
                {
                    length = (int)ContentLength;
                    ContentLength = 0;
                }
                //else
                //    ContentLength = ContentLength - (long)length;

                byte[] dataBuffer = new byte[length];
                contentsRead = ResStream.Read(dataBuffer, 0, length);

                while (contentsRead > 0)
                {
                    strWriter.Write(dataBuffer, 0, dataBuffer.Length);


                    if (ContentLength < (long)length)
                    {
                        length = (int)ContentLength;
                        ContentLength = 0;
                    }
                    else
                        ContentLength = ContentLength - ((long)length);

                    dataBuffer = new byte[length];
                    contentsRead = ResStream.Read(dataBuffer, 0, length);
                    //DataGet += contentsRead;

                }

                ResStream.Close();
                strWriter.Close();
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public static bool DownloadFileFromHttp(string HttpWebRequestURL, string savePath, string userName, string password)
        {
            string url = HttpWebRequestURL;
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
            string proxy = null;

            string data = String.Format("username={0}&password={1}", userName, password);
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(data);

            req.Method = "POST";
            req.ContentType = "application/x-www-form-urlencoded";
            req.ContentLength = buffer.Length;
            req.Proxy = new WebProxy(proxy, true); // ignore for local addresses 
            req.CookieContainer = new CookieContainer(); // enable cookies 

            Stream reqst = req.GetRequestStream(); // add form data to request stream 
            reqst.Write(buffer, 0, buffer.Length);
            reqst.Flush();
            reqst.Close();

            HttpWebResponse response = (HttpWebResponse)req.GetResponse();

            if (response.StatusCode != HttpStatusCode.OK)
            {
                return false;
            }
            else
            {
                Stream ResponseStream = response.GetResponseStream();


                

                long cl = response.ContentLength;
                int length = 2048;
                byte[] Buffer = new byte[length];
                int bytesread = ResponseStream.Read(Buffer, 0, length);
                FileStream writeStream ;
                if (bytesread > 0)
                {
                    writeStream = new FileStream(savePath, FileMode.Create);
                    while (bytesread > 0)
                    {
                        writeStream.Write(Buffer, 0, bytesread);
                        bytesread = ResponseStream.Read(Buffer, 0, length);
                    }
                    writeStream.Close();
                }
                
                response.Close();
                return true;
            }
        }
        public static bool DownloadFileFromHttp(string HttpWebRequestURL, string savePath)
        {

            string url = HttpWebRequestURL;
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
            string proxy = null;

            string data = "";
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(data);

            req.Method = "GET";
            //req.ContentType = "application/x-www-form-urlencoded";
            req.ContentLength = buffer.Length;
            req.Proxy = new WebProxy(proxy, true); // ignore for local addresses 
            req.CookieContainer = new CookieContainer(); // enable cookies 

            //Stream reqst = req.GetRequestStream(); // add form data to request stream 
            //reqst.Write(buffer, 0, buffer.Length);
            //reqst.Flush();
            //reqst.Close();

            HttpWebResponse response = (HttpWebResponse)req.GetResponse();

            if (response.StatusCode != HttpStatusCode.OK)
            {
                return false;
            }
            else
            {
                Stream ResponseStream = response.GetResponseStream();


               
                long cl = response.ContentLength;
                int length = 2048;
                byte[] Buffer = new byte[length];
                int bytesread = ResponseStream.Read(Buffer, 0, length);
                FileStream writeStream;
                if (bytesread > 0)
                {
                    writeStream = new FileStream(savePath, FileMode.Create);
                    while (bytesread > 0)
                    {
                        writeStream.Write(Buffer, 0, bytesread);
                        bytesread = ResponseStream.Read(Buffer, 0, length);
                    }
                    writeStream.Close();
                }
                response.Close();
                return true;
            }
        }

        public static String RemoveHtmlCharacters(String st)
        {
            st = st.Replace(",", "");
            st = Regex.Replace(st, "\"", "");
            st = Regex.Replace(st, "\n", "");
            st = Regex.Replace(st, "\t", "");
            st = Regex.Replace(st, "\r", "");
            st = Regex.Replace(st, "&nbsp;", "");
            st = Regex.Replace(st, "&#149;", "");
            st = Regex.Replace(st, "&#168714 ;", "");
            st = Regex.Replace(st, "&#174;", "");
            st = Regex.Replace(st, "&#176;", "");
            st = Regex.Replace(st, "&nb45974;", "");
            st = Regex.Replace(st, "&#153;", "");
            st = Regex.Replace(st, "&#8226;", "");
            st = Regex.Replace(st, "&nbs49445;", "");
            st = Regex.Replace(st, "&#14952321;", "");
            st = Regex.Replace(st, "&#145;", "");
            st = Regex.Replace(st, "&#134;", "");
            st = Regex.Replace(st, "&Atilde;", "");
            st = Regex.Replace(st, "Ã", "");
            st = Regex.Replace(st, "&#169;", "");
            st = Regex.Replace(st, "˜", "");
            st = Regex.Replace(st, "&#128;", "");
            st = Regex.Replace(st, "Â°", "");
            st = Regex.Replace(st, "Â®", "");
            st = Regex.Replace(st, "Â°", "");
            st = Regex.Replace(st, "Â²", "");
            st = Regex.Replace(st, "Ã˜", "");
            st = Regex.Replace(st, "€²", "");
            st = Regex.Replace(st, "¢„¢", "");
            st = Regex.Replace(st, "¢", "");
            st = Regex.Replace(st, "&Atilde;‚·", "");
            st = Regex.Replace(st, "Å¡", "");
            st = Regex.Replace(st, "Â", "");
            st = Regex.Replace(st, "â€™", "");
            st = Regex.Replace(st, "â", "");
            st = Regex.Replace(st, "€", "");
            st = Regex.Replace(st, "&quot;", "");
            return st;
        }
    }


}
