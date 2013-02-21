using System.Collections.Specialized;
using System.Net;
using System.Text;

namespace POESKillTree
{
    public class CookieAwareWebClient : WebClient
    {
        public CookieContainer cookieContainer { get; private set; }

        public CookieAwareWebClient()
        {
            cookieContainer = new CookieContainer();
        }

        protected override WebRequest GetWebRequest( System.Uri address )
        {
            HttpWebRequest request = (HttpWebRequest) base.GetWebRequest( address );
            request.CookieContainer = cookieContainer;
            return request;
        }
    }

    public class DataRequests
    {
        public static byte[] requestItemData( string characterName, string email, string password )
        {
            using (var client = new CookieAwareWebClient())
            {
                client.Headers[ HttpRequestHeader.Accept ] = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";

                byte[] response;

                if (email != null && password != null)
                {
                    const string loginUrl = "https://www.pathofexile.com/login/";
                    var values = new NameValueCollection
                                     {
                                         { "login_email", email },
                                         { "login_password", password },
                                     };
                    response = client.UploadValues( loginUrl, values );
                    
                    if (!isResponseValid( response ))
                    {
                        return null;
                    }
                }

                string url = "http://www.pathofexile.com/character-window/get-items?character=" + characterName;
                response = client.DownloadData( url );

                if (isResponseValid( response ))
                {
                    return response;
                }

                return null;
            }
        }

        private static bool isResponseValid( byte[] response )
        {
            string responseString = Encoding.ASCII.GetString( response );

            // :TODO: This is quite brittle. Is there a better way to check if we are logged in?
            if (responseString.Contains( "View Profile" ))
            {
                // Seems we already had a cookie. Login was successful
                return true;
            }

            if (responseString.Contains( "<body" ))
            {
                // We got an HTML page meaning we were probably redirected to the login page
                return false;
            }

            return true;
        }
    }
}