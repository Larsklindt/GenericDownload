using GenericDownload.ServiceConfiguration;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenericDownload.download
{
    public abstract class IDownload
    {

        abstract public void setImportSettings(ConfigurationSection element);
        abstract public void setExportSettings(SqlConnection connection);
        abstract public void startDownload(Boolean firstRunner);
        abstract public void startDownload(long CRUD_ID);

        protected String handleOutmaps(String query, List<String> outmaps)
        {
            // return if no outmap values
            if (outmaps == null || outmaps.Count == 0) return query;

            String outmapKey = "#";
            // if no outmapping to perform, return roginal query
            if (query.IndexOf(outmapKey) < 0)
                return query;

            String result = "";
            int resultStartIndex = 0;
            int queryStartIndex = 0;
            // bit for bit, construct the query string and replace hashmaps with outmap tokens
            for (int i = 0; i < outmaps.Count; i++)
            {
                queryStartIndex = query.IndexOf(outmapKey, queryStartIndex);
                if (queryStartIndex < 0)
                    throw new Exception("The number of outmaps did not correspond to outmpas in specified query");

                String outmapValue = handleSpecialCharacters(outmaps.ElementAt(i));
                result += query.Substring(resultStartIndex, queryStartIndex - resultStartIndex) + outmapValue;
                // offset string to correct length after outmap has been found
                queryStartIndex += outmapKey.Length;
                // offset string to correct length after token is inserted
                resultStartIndex = queryStartIndex;
            }
            // get the last part of the query string
            int lastIndex = query.LastIndexOf(outmapKey);
            result += query.Substring(lastIndex + outmapKey.Length);
            return result;
        }


        /**
         * Helper method for replacing special characters
         */
        private string handleSpecialCharacters(string input)
        {
            string str = input;
            str = str.Replace("'", "''");
            return str;
        }
    }
}
