using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GenericDownload.ServiceConfiguration;
using ScrapySharp.Network;
using HtmlAgilityPack;
using ScrapySharp.Extensions;
using System.Configuration;
using System.Net;
using System.IO;
using System.Data;
using System.Data.OleDb;
using System.Threading;

namespace GenericDownload.download
{
    class SVURScraper : IDownload
    {
        private string databaseInsertSQL, databaseUpdateSQL;
        private String SvurUrl;
        private SqlConnection myConnection = null;
        private Semaphore _pool;
        private int threadCount = 0;

        public override void setExportSettings(SqlConnection connection)
        {
            myConnection = connection;
        }

        public override void setImportSettings(ConfigurationSection element)
        {
            databaseInsertSQL = ((ScrapingServiceConfigurationElement)element).DatabaseInsertSQL;
            databaseUpdateSQL = ((ScrapingServiceConfigurationElement)element).DatabaseUpdateSQL;
            SvurUrl = ((ScrapingServiceConfigurationElement)element).SvurUrl;
        }

        public override void startDownload(long CRUD_ID)
        {
            throw new NotImplementedException();
        }

        public override void startDownload(Boolean firstRunner)
        {
            // Create a semaphore that can satisfy up to three
            // concurrent requests. Use an initial count of zero,
            // so that the entire semaphore count is initially
            // owned by the main program thread.
            //
            _pool = new Semaphore(0, 200);
            _pool.Release(200);

            // predecessor: cleanup script - ONLY RUN THIS ONCE for every download!!
            if (firstRunner)
            {
                // predecessor: if controller table count is different from ESR_EJERVIEW table count
                // ! this does even apply, when controller table is empty
                // fill out the missing lines
                this.destinationDataExectute("INSERT INTO SVUR_HIST_STYR (CRUD_ID, KOMMUNE_NR, EJD_NR) " + 
                    "SELECT CO11700T_view.CRUD_ID, CO11700T_view.KOMMUNE_NR, CO11700T_view.EJD_NR " + 
                    "FROM CO11700T_view " + 
                    "LEFT JOIN SVUR_HIST_STYR ON(CO11700T_view.KOMMUNE_NR = SVUR_HIST_STYR.KOMMUNE_NR AND CO11700T_view.EJD_NR = SVUR_HIST_STYR.EJD_NR) " + 
                    "WHERE SVUR_HIST_STYR.CRUD_ID IS NULL", null);

                // predecessor: remove unfinished downloads from SVUR_HISTORIK table
                // This may occur, when previous session has not been completed
                // or interruption has occured because of illegal website response.
                // unfinished downloads are indicated by 'occupied = true' and 'finished = false'
                this.destinationDataExectute("delete historik " +
                    "FROM SVUR_HISTORIK historik " +
                    "left JOIN SVUR_HIST_STYR styretabel " +
                    "ON historik.KOMMUNE_NR = styretabel.KOMMUNE_NR AND historik.EJD_NR = styretabel.EJD_NR " +
                    "WHERE styretabel.OCCUPIED = 1 AND styretabel.SVUR_HIST_COMPLETE = 0", null);

                // predecessor: Also remove unfinished downloads from controller table 'SVUR_HIST_STYR'
                // unfinished downloads are indicated by 'occupied = true' and 'finished = false'
                this.destinationDataExectute("delete FROM SVUR_HIST_STYR where OCCUPIED = 1 AND SVUR_HIST_COMPLETE = 0 ", null);

                // predecessor: remove unfinished downloads from previous
                // session, indicated by occupied = true and finished = false
                this.destinationDataExectute("delete historik " +
                    "FROM SVUR_HISTORIK historik " +
                    "left JOIN SVUR_HIST_STYR styretabel " +
                    "ON historik.KOMMUNE_NR = styretabel.KOMMUNE_NR AND historik.EJD_NR = styretabel.EJD_NR " +
                    "WHERE styretabel.OCCUPIED = 1 AND styretabel.SVUR_HIST_COMPLETE = 0", null);
            }

            // NOW we are ready to download data
            // meaning we are looping through SVUR_HIST_STYR with field 
            // SVUR_HIST_COMPLETE equal to '0'
            // run this loop, while there is elements to update
            while (!isDataCompleted())
            {
                Thread t = new Thread(new ParameterizedThreadStart(Worker));
                // TODO: Make WORKER ABLE TO HANDLE TWO ARGUMENTS.

                // Start the thread, passing the number.
                t.Start(null);
                threadCount++;

                // make thread slowly
                Thread.Sleep(10000);
            }

            while (threadCount > 0)
            {
                Thread.Sleep(60000); //Make it pause for one minut until all threads have finished.
            }
        }

        /**
         * Determine if there is more SVUR data to be updated
         * returns true if there is no more items to update
         */ 
        private Boolean isDataCompleted()
        {
            List<Object> moreItems = this.destinationDataQueryForOnlyOneColumn("SELECT top(1) CO15800T.CRUD_ID, CO15800T.KOMMUNE_NR, CO15800T.EJD_NR " +
                "FROM CO15800T " + 
                "LEFT JOIN SVUR_HIST_STYR ON(CO15800T.KOMMUNE_NR = SVUR_HIST_STYR.KOMMUNE_NR AND CO15800T.EJD_NR = SVUR_HIST_STYR.EJD_NR) " +
                "WHERE SVUR_HIST_STYR.SVUR_HIST_COMPLETE = 0 and SVUR_HIST_STYR.OCCUPIED = 0", null);
            // true if we have finished updating
            if (moreItems.Count == 0) return true;
            // false if there is no more items to update
            return false;
        }

        private KeyValuePair<String, String> getNextSVURRealProperty()
        {
            // occupy and get unique key for element in action
            List<String> nextItem = this.destinationDataQueryForOnlyOneRow("UPDATE TOP (1) SVUR_HIST_STYR " +
                "SET OCCUPIED = 1 " + 
                "OUTPUT Inserted.EJD_NR, Inserted.KOMMUNE_NR " + 
                "WHERE SVUR_HIST_COMPLETE = 0 AND OCCUPIED = 0", null);
            /*
            List<String> nextItem = this.destinationDataQueryForOnlyOneRow("SELECT top(1) SVUR_HIST_STYR.EJD_NR, SVUR_HIST_STYR.KOMMUNE_NR " +
                     "FROM SVUR_HIST_STYR " +
                     "WHERE SVUR_HIST_COMPLETE = 0 AND OCCUPIED = 0", null);
            // true if we have finished updating
            if (nextItem.Count == 0) throw new Exception("no more items found");

            // mark item as occupied
            this.destinationDataExectute("UPDATE SVUR_HIST_STYR SET OCCUPIED = 1 " +
                "WHERE EJD_NR = # AND KOMMUNE_NR = #", nextItem);
                */
            /*
            // clean up prevoius items from destination database table
            this.destinationDataExectute("DELETE FROM SVUR_HISTORIK WHERE EJD_NR = # AND KOMMUNE_NR = #", nextItem);
            */
            // false if there is no more items to update
            return new KeyValuePair<String, String>(Convert.ToString(nextItem.ElementAt(0)), Convert.ToString(nextItem.ElementAt(1)));
        }
        
        private void markItemAsFinished(KeyValuePair<String, String> arguments)
        {
            List<String> sqlArguments = new List<String>();
            // ejdnr is key
            sqlArguments.Add(arguments.Key);
            // komnr is value
            sqlArguments.Add(arguments.Value);
            this.destinationDataExectute("UPDATE SVUR_HIST_STYR SET SVUR_HIST_COMPLETE = 1, OCCUPIED = 0 " +
                    "WHERE SVUR_HIST_STYR.EJD_NR = # AND SVUR_HIST_STYR.KOMMUNE_NR = #", sqlArguments);
        }

        private void Worker(object obj)
        {
            // pick next element as a KeyValuePair
            KeyValuePair<String, String> kvp = getNextSVURRealProperty();

            String ejdNr = ((KeyValuePair < String, String>)kvp).Key.ToString();
            String komNr = ((KeyValuePair<String, String>)kvp).Value.ToString();
            
            // Each worker thread begins by requesting the semaphore.
            Console.WriteLine("Thread begins komnr: " + komNr + " ejdNr: " + ejdNr + " and waits for the semaphore.");
            _pool.WaitOne();

            Console.WriteLine("Thread " + ejdNr + " enters the semaphore.");
            try
            {
                String ejdNrPrefixed = ejdNr.ToString().PadLeft(6, '0');
                String komNrPrefixed = komNr.PadLeft(4, '0');
                // important! String replacement with database records
                String url = SvurUrl.Replace("#EJDNR#", ejdNrPrefixed).Replace("#KOMNR#", komNrPrefixed);
 
                // 1. st fetch links for retrieving yearly assessment data
                List<KeyValuePair<String, String>> SVUR_links = retriveLinksFromRealPropertyAndMunicipalityNumber(url);
                foreach (KeyValuePair<String, String> link in SVUR_links)
                {
                    String year = link.Key;
                    String assesmentUrl = link.Value;

                    List<String> destinationSQLArguments = new List<String>();
                    // 2. nd store initial values into database
                    // we are putting invalid properties in it, in order 
                    // to be abel to detect insertion errors from the rest
                    // of this thread
                    // important! - order must match aoutmap in app.config file
                    destinationSQLArguments.Add(komNr);
                    // important! - order must match aoutmap in app.config file
                    destinationSQLArguments.Add(ejdNr.ToString());

                    // important! - order must match outmap in app.config file
                    destinationSQLArguments.Add(link.Key);
                    // put in illegal values
                    destinationSQLArguments.Add("-1");
                    destinationSQLArguments.Add("-1");
                    destinationSQLArguments.Add("-1");
                    destinationSQLArguments.Add("-1");

                    // insert assesment values with '-1' data
                    destinationDataExectute(databaseInsertSQL, destinationSQLArguments);

                    // clean up properties
                    destinationSQLArguments.RemoveAt(destinationSQLArguments.Count - 1);
                    destinationSQLArguments.RemoveAt(destinationSQLArguments.Count - 1);
                    destinationSQLArguments.RemoveAt(destinationSQLArguments.Count - 1);
                    destinationSQLArguments.RemoveAt(destinationSQLArguments.Count - 1);

                    // insert real assesment values 
                    List<String> assesments = retriveAssessmentsFromLinks(assesmentUrl);
                    // important! - order must match aoutmap in app.config file
                    destinationSQLArguments.Insert(0, assesments[0]);
                    destinationSQLArguments.Insert(1, assesments[1]);
                    destinationSQLArguments.Insert(2, assesments[2]);
                    destinationSQLArguments.Insert(3, assesments[3]);

                    // 4. th store retrieved assesments into database
                    destinationDataExectute(databaseUpdateSQL, destinationSQLArguments);
                }


                // all items complete, mark item as completed in controller table
                // reelase item from occupied and mark as finished
                markItemAsFinished(new KeyValuePair<string, string>(ejdNr, komNr));

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                //TODO: Write error handling, which preset '-1' in values for current record.
            } finally {
                Console.WriteLine("Thread  komnr: " + komNr + " ejdNr: " + ejdNr + " releases the semaphore.");
                _pool.Release();
                // decrease internally thread count
                threadCount--;
            }
        }

        private void destinationDataExectute(String sql, List<String> arguments)
        {
            string qryStr = handleOutmaps(sql, arguments);

            lock (myConnection)
            {
                myConnection.Open();
                SqlCommand querySaveRecords = new SqlCommand(qryStr, myConnection);
                querySaveRecords.ExecuteNonQuery();
                myConnection.Close();
            }
        }

        private List<Object> destinationDataQueryForOnlyOneColumn(String sql, List<String> arguments)
        {
            List<Object> result = new List<Object>();
            string qryStr = handleOutmaps(sql, arguments);

            lock (myConnection)
            {
                myConnection.Open();
                SqlCommand queryRecords = new SqlCommand(qryStr, myConnection);
                SqlDataReader reader = queryRecords.ExecuteReader();

                // Data is accessible through the DataReader object here.
                while (reader.Read())
                {
                    Type t = reader.GetFieldType(1);
                    if (t.Name == "Int64")
                        result.Add(reader.GetInt64(1));
                    else if (t.Name == "Int32")
                        result.Add(reader.GetInt32(1));
                    else if (t.Name == "Int16")
                        result.Add(reader.GetInt16(1));
                    else if (t.Name == "String")
                        result.Add(reader.GetString(1));
                    else
                        throw new Exception("Unimplemented field type");                    
                }
                myConnection.Close();
            }
            return result;
        }

        private List<String> destinationDataQueryForOnlyOneRow(String sql, List<String> arguments)
        {
            List<String> result = new List<String>();
            string qryStr = handleOutmaps(sql, arguments);

            lock (myConnection)
            {
                myConnection.Open();
                SqlCommand queryRecords = new SqlCommand(qryStr, myConnection);
                SqlDataReader reader = queryRecords.ExecuteReader();

                // Data is accessible through the DataReader object here.
                // NOTE: reads only ONE row
                if (reader.Read())
                {
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        Type t = reader.GetFieldType(i);
                        if (t.Name == "Int16")
                            result.Add(reader.GetInt16(i).ToString());
                        else if (t.Name == "Int32")
                            result.Add(reader.GetInt32(i).ToString());
                        else if (t.Name == "String")
                            result.Add(reader.GetString(i).ToString());
                        else
                            throw new Exception("Unimplemented field type");
                    }
                }
                myConnection.Close();
            }
            return result;
        }

        private List<String> retriveAssessmentsFromLinks(String url)
        {
            HtmlDocument doc = this.GetPage(url);
            List<String> assesments = new List<String>(4) { "0", "0", "0", "0" };
            // find value for tag "EJD_VAERDI"
            if (doc.DocumentNode.SelectNodes("//*[contains(text(),'Ejendomsv')]") != null)
            {
                foreach (HtmlNode n in doc.DocumentNode.SelectNodes("//*[contains(text(),'Ejendomsv')]"))
                {
                    String n1 = n.NextSibling.NextSibling.NextSibling.InnerHtml;
                    try
                    {
                        assesments[0] = Double.Parse(n1).ToString();
                    } catch (FormatException)
                    {
                        assesments[0] = "-1";
                    }
                }
            }
            // find value for tag "GRUND_VAERDI"
            if (doc.DocumentNode.SelectNodes("//*[contains(text(),'Grundv')]") != null)
            {
                foreach (HtmlNode n in doc.DocumentNode.SelectNodes("//*[contains(text(),'Grundv')]"))
                {
                    String n1 = n.NextSibling.NextSibling.NextSibling.InnerHtml;
                    try { 
                        assesments[1] = Double.Parse(n1).ToString();
                    }
                    catch (FormatException)
                    {
                        assesments[1] = "-1";
                    }
                }
            }
            // find value for tag "STUEHUS_VAERDI"
            if (doc.DocumentNode.SelectNodes("//*[contains(text(),'Ejerboligv')]") != null)
            {
                foreach (HtmlNode n in doc.DocumentNode.SelectNodes("//*[contains(text(),'Ejerboligv')]"))
                {
                    String n1 = n.NextSibling.NextSibling.NextSibling.InnerHtml;
                    try {
                        assesments[2] = Double.Parse(n1).ToString();
                    }
                    catch (FormatException)
                    {
                        assesments[2] = "-1";
                    }
                }
            }
            // find value for tag "[STUE_GRUND_VAERDI]"
            if (doc.DocumentNode.SelectNodes("//*[contains(text(),'Stuehus grundv')]") != null)
            {
                foreach (HtmlNode n in doc.DocumentNode.SelectNodes("//*[contains(text(),'Stuehus grundv')]"))
                {
                    String n1 = n.NextSibling.NextSibling.NextSibling.InnerHtml;
                    try { 
                        assesments[3] = Double.Parse(n1).ToString();
                    }
                    catch (FormatException)
                    {
                        assesments[3] = "-1";
                    }
                }
            }
            return assesments;
        }

        private List<KeyValuePair<String, String>> retriveLinksFromRealPropertyAndMunicipalityNumber(String url)
        {
            HtmlDocument doc = this.GetPage(url);
            List<KeyValuePair<String, String>> SVUR_links = new List<KeyValuePair<String, String>>();
            foreach (HtmlNode link in doc.DocumentNode.SelectNodes("//ul//li//a/@href"))
            {
                if (link.OuterHtml.IndexOf("boligejer.dk/ejendomsdata") > 0 && link.OuterHtml.IndexOf("Vurdering - ") > 0)
                {
                    KeyValuePair<String, String> kvp = new KeyValuePair<String, String>(
                        link.GetAttributeValue("title").Substring(("Vurdering - ".Length)),
                        link.GetAttributeValue("href"));
                    SVUR_links.Add(kvp);
                }

            }
            return SVUR_links;
        }

        private HtmlDocument GetPage(string url)
        {
        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
        request.Method = "GET";

        HttpWebResponse response = (HttpWebResponse)request.GetResponse();
        var stream = response.GetResponseStream();

        //When you get the response from the website, the cookies will be stored
        //automatically in "_cookies".

        using (var reader = new StreamReader(stream))
        {
            string html = reader.ReadToEnd();
            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            return doc;
        }
    }

    }
}
