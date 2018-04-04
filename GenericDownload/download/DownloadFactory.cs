using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenericDownload.download
{
    public static class DownloadFactory
    {
        //use getShape method to get object of type shape 
        public static IDownload createDownloader(String downloadtype)
        {
            if (downloadtype == null)
            {
                return null;
            }
            else if (downloadtype.ToUpper().Equals("OIS"))
            {
                return new OISDownloader();

            }
            else if (downloadtype.ToUpper().Equals("SVURSCRAPER"))
            {
                return new SVURScraper();

            }
            
            return null;
        }
    }
}
    