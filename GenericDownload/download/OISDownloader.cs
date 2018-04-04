using GenericDownload.ServiceConfiguration;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml;
using System.Configuration;

namespace GenericDownload.download
{
    class OISDownloader: IDownload
    {
        private string databaseSQL;
        private List<KeyValuePair<string, string>> postDataStrList = new List<KeyValuePair<string, string>>();
        private SqlConnection myConnection = null;

        public override void setImportSettings(ConfigurationSection element)
        {
            databaseSQL = ((OISServiceConfigurationElement)element).DatabaseSQL;

            postDataStrList.Add(new KeyValuePair<string, string>("UID", ((OISServiceConfigurationElement)element).Uid));
            postDataStrList.Add(new KeyValuePair<string, string>("PWD", ((OISServiceConfigurationElement)element).Pwd));
            postDataStrList.Add(new KeyValuePair<string, string>("SQL", ((OISServiceConfigurationElement)element).Sql));
            postDataStrList.Add(new KeyValuePair<string, string>("Meta", ((OISServiceConfigurationElement)element).Meta));
        }

        public override void setExportSettings(SqlConnection connection)
        {
            myConnection = connection;
        }

        public override void startDownload(Boolean firstRunner)
        {
            this.startDownload(0);
        }

        private string replaceHashmarks(String input, long replacement)
        {
            return input.Replace("#", replacement.ToString());
        }

        /** recurse until no more elements **/
        public override void startDownload(long CRUD_ID)
        {
            var request = (HttpWebRequest)WebRequest.Create("https://www.lifaois.dk/OISservice/OISService.asmx/Query");

            /* build postdata string from settings */
            StringBuilder postData = new StringBuilder();
            foreach (var kvp in postDataStrList)
            {
                postData.Append(kvp.Key + "="  + replaceHashmarks(kvp.Value, CRUD_ID) + "&");
            }

            var data = Encoding.ASCII.GetBytes(postData.ToString());
            request.Method = "POST";

            request.Headers["Accept-Encoding"] = "gzip";
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = data.Length;

            using (var stream = request.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
            }

            var response = (HttpWebResponse)request.GetResponse();

            StreamReader responseReader = new StreamReader(response.GetResponseStream());
            /*
            var responseString = responseReader.ReadToEnd();
            */
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(responseReader);

            int noOfElements = Int32.Parse(xmlDoc.GetElementsByTagName("Metadata")[0].Attributes[0].InnerText);

            XmlNodeList address = xmlDoc.GetElementsByTagName("Data");

            // Example #2: Write one string to a text file.
            string text = ((XmlNode)address[0]).InnerText;

            long lastCRUDID = bulkInsertData(noOfElements, text);

            // recurse until no more elements are returned
            if (lastCRUDID > 0)
                startDownload(lastCRUDID);
        }

        public long bulkInsertData(int noOfColumns, String csv)
        {
            int newLineStartIndex = 0;
            int newLineEndIndex = 0;
            long lastCRUDID = -1;

            List<Object> rowArr = new List<Object>();
            int newLineCount = 0;
            while (newLineEndIndex < csv.Length)
            {
                // handle row
                if (newLineEndIndex > 0)
                {
                    newLineStartIndex = csv.IndexOf(Environment.NewLine, newLineStartIndex);
                    newLineStartIndex += Environment.NewLine.Length;
                }
                newLineEndIndex = csv.IndexOf(Environment.NewLine, newLineStartIndex);
                newLineCount++;

                if (newLineStartIndex > csv.Length || newLineEndIndex == -1)
                    break;

                // handle columns
                String myColumn = csv.Substring(newLineStartIndex, (newLineEndIndex - newLineStartIndex));
                String[] myColumnArr = myColumn.Split(';');
                // transform data into rows and cols
                List<String> row = new List<string>();
                for (int i = 0 ; i < myColumnArr.Length; i++)
                {
                    row.Add(myColumnArr[i].Trim());
                }
                rowArr.Add(row);

            }

            
            myConnection.Open();
            for (int i = 0; i < rowArr.Count; i++)
            {
                List<String> strlist = (List<String>)rowArr.ElementAt(i);
                // insert data into database
                // Ejer view med begrænsninger af ejers navn
                string saveStr = handleOutmaps(databaseSQL, strlist);


                SqlCommand querySaveRecords = new SqlCommand(saveStr, myConnection);
                querySaveRecords.ExecuteNonQuery();


                lastCRUDID = Convert.ToInt64(strlist.ElementAt(strlist.Count() - 1));
            }
            myConnection.Close();

            return lastCRUDID;
        }        
    }
}
