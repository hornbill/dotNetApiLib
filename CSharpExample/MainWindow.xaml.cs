using System;
using System.Windows;
using Hornbill;

namespace CSharpExample
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public partial class MainWindow : Window
    {
        #region Fields
        private readonly string instanceName = "";
        private readonly string userId = "admin";
        private readonly string password = "";
        private readonly string serviceEntryPoint = "xmlmc";
        private readonly string davEntryPoint = "dav";
        private readonly bool shouldLogoff = false;
        private readonly string apiKey = "";
        private int rowStart = 0;
        private int rowLimit = 3;
        private XmlmcService xmlmc;
        #endregion


        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Initialise the MethodCall object with instance name and entry point
            // Login/Logout are not required if API key is specified
            try
            {
                // The first parameter can be the URL or instance name. It'll throw an exception if the instance name cannot be resolved.
                xmlmc = new XmlmcService(instanceName, serviceEntryPoint, davEntryPoint, string.Empty);
            }
            catch (RequestFailureException)
            {
                showWarningMessage(xmlmc.GetLastResponseErrorMessage());
            }
            catch (System.Net.WebException)
            {
                showWarningMessage(xmlmc.GetLastResponseErrorMessage());
            }
            catch (Exception ex)
            {
                showWarningMessage(ex.Message);
            }
        }

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            if(shouldLogoff)
            {
                try
                {
                    xmlmc.Invoke("session", "userLogoff");
                }
                catch (RequestFailureException)
                {
                    showWarningMessage(xmlmc.GetLastResponseErrorMessage());
                }
                catch (System.Net.WebException)
                {
                    showWarningMessage(xmlmc.GetLastResponseErrorMessage());
                }
                catch (Exception ex)
                {
                    showWarningMessage(ex.Message);
                }
            }
        }

        private void showWarningMessage(string message)
        {
            if (message == null)
                return;
            MessageBox.Show(message, "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private void btnClearRequest_Click(object sender, RoutedEventArgs e)
        {
            // Clear the request xml box
            txtRequest.Clear();
        }

        private void btnClearOutput_Click(object sender, RoutedEventArgs e)
        {
            // Clear the output box
            txtOutput.Clear();
        }

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            // Only if API key is not specified
            // Login to the server
            // For more information: https://api.hornbill.com/docs/session/?op=userLogon
            try
            {
                xmlmc.AddParam("userId", userId);
                xmlmc.AddParam("password", password).EncodeValue(XmlmcEncoding.Base64);
                xmlmc.Invoke("session", "userLogon");
                // Send the request xml to text box
                sendTextToRequestBox(xmlmc.GetInvokeXML("session", "userLogon"));
                // Send the result xml to text box
                sendTextToOutputBox(xmlmc.GetResponseXML());
                // To get the session ID from the XML results
                string sessionId = xmlmc.GetResponseParamAsString("sessionId");
                sendTextToOutputBox(Environment.NewLine + "Session ID: " + sessionId, true);

                // See how to construct a complex type
                // registerUserLocation();
            }
            catch (RequestFailureException)
            {
                showWarningMessage(xmlmc.GetLastResponseErrorMessage());
            }
            catch (System.Net.WebException)
            {
                showWarningMessage(xmlmc.GetLastResponseErrorMessage());
            }
            catch (Exception ex)
            {
                showWarningMessage(ex.Message);
            }
        }

        private void sendTextToOutputBox(string inputText, bool append = false)
        {
            if (inputText == null)
                return;
            if (append)
            {
                txtOutput.AppendText(inputText + Environment.NewLine);
            }
            else
            {
                txtOutput.Text = inputText;
            }
        }

        private void sendTextToRequestBox(string inputText, bool append = false)
        {
            if (inputText == null)
                return;
            if (append)
            {
                txtRequest.AppendText(inputText + Environment.NewLine);
            }
            else
            {
                txtRequest.Text = inputText;
            }
        }


        private void registerUserLocation()
        {
            // Example of how to construct a complex type
            XmlmcParam param = xmlmc.AddParam("location");
            param.Add("latitude", 51.5575);
            param.Add("longitude", 0.4026);
            param.Add("elevation", 0);
            param.Add("placeName", "Odyssey Business Park");
            param.Add("timestamp", DateTime.Now.ToUniversalTime());

            // To output window
            System.Diagnostics.Debug.Print(xmlmc.GetParamsXML());

           xmlmc.Invoke("session", "locationRegisterCurrent");
        }

        private void btnListRequests_Click(object sender, RoutedEventArgs e)
        {
            // Get requests
            // For more information: https://api.hornbill.com/docs/apps/com.hornbill.servicemanager/Requests?op=smGetRequests
            try
            {
                xmlmc.AddParam("loggedAfter", "2016-05-24 00:00");
                xmlmc.AddParam("rowstart", rowStart);
                xmlmc.AddParam("limit", rowLimit);
                xmlmc.AddParam("orderByColumn", "h_datelogged");
                xmlmc.AddParam("orderByDirection", "ascending");
                rowStart += rowLimit;

                // Send the request xml to text box
                sendTextToRequestBox(xmlmc.GetInvokeXML("apps/com.hornbill.servicemanager/Requests", "smGetRequests"));

                xmlmc.Invoke("apps/com.hornbill.servicemanager/Requests", "smGetRequests");
                // Send the result xml to output box
                sendTextToOutputBox(xmlmc.GetResponseXML());
            }
            catch (RequestFailureException)
            {
                showWarningMessage(xmlmc.GetLastResponseErrorMessage());
            }
            catch (System.Net.WebException)
            {
                showWarningMessage(xmlmc.GetLastResponseErrorMessage());
            }
            catch (Exception ex)
            {
                showWarningMessage(ex.Message);
            }
        }

        private void btnLogIncident_Click(object sender, RoutedEventArgs e)
        {
            // Log a new incident
            // For more information: https://api.hornbill.com/docs/apps/com.hornbill.servicemanager/Incidents?op=logIncident
            xmlmc.AddParam("summary", "This is a summary");
            xmlmc.AddParam("description", "This is my description");
            xmlmc.AddParam("requestType", "Incident");
            xmlmc.AddParam("ownerId", userId);
            // Get the team ID from admin::groupGetList - https://api.hornbill.com/docs/admin/?op=groupGetList
            xmlmc.AddParam("teamId", "1stLineSupport");
            xmlmc.AddParam("status", "status.open");

            // String remoteFilePath = "/session/api_lib_example.txt";
            // xmlmc.AddParam("fileName", remoteFilePath)
            try
            {
                // upload file to the server
                // xmlmc.PutText(remoteFilePath, "My Text")

                // Send the request xml to text box
                sendTextToRequestBox(xmlmc.GetInvokeXML("apps/com.hornbill.servicemanager/Incidents", "logIncident"));

                xmlmc.Invoke("apps/com.hornbill.servicemanager/Incidents", "logIncident");
                // Send the result xml to output box
                sendTextToOutputBox(xmlmc.GetResponseXML());

                // xmlmc.RemoveFile(remoteFilePath);
            }
            catch (RequestFailureException)
            {
                showWarningMessage(xmlmc.GetLastResponseErrorMessage());
            }
            catch (System.Net.WebException)
            {
                showWarningMessage(xmlmc.GetLastResponseErrorMessage());
            }
            catch (Exception ex)
            {
                showWarningMessage(ex.Message);
            }
        }

        private void chkUseApiKey_Click(object sender, RoutedEventArgs e)
        {
            bool enabled = chkUseApiKey.IsChecked.HasValue ? chkUseApiKey.IsChecked.Value : false;
            btnLogin.IsEnabled = !enabled;
            btnLogout.IsEnabled = !enabled;
            if (enabled)
            {
                xmlmc.APIKey = apiKey;
            }
            else
            {
                xmlmc.APIKey = string.Empty;
            }
        }

        private void btnLogout_Click(object sender, RoutedEventArgs e)
        {
            // Only if API key is not specified
            // Logout from the server
            // For more information: https://api.hornbill.com/docs/session/?op=userLogoff
            try
            {
                // Send the request xml to text box
                sendTextToRequestBox(xmlmc.GetInvokeXML("session", "userLogoff"));

                xmlmc.Invoke("session", "userLogoff");
                // Send the result xml to output box
                sendTextToOutputBox(xmlmc.GetResponseXML());
            }
            catch (RequestFailureException)
            {
                showWarningMessage(xmlmc.GetLastResponseErrorMessage());
            }
            catch (System.Net.WebException)
            {
                showWarningMessage(xmlmc.GetLastResponseErrorMessage());
            }
            catch (Exception ex)
            {
                showWarningMessage(ex.Message);
            }
        }
    }

}
