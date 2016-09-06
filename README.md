
 Hornbill .NET / C# API Lib
========

Integration
===
Various Hornbill Integration methods are documeted here: https://wiki.hornbill.com/index.php/Integration

DLL
===

```
espapi_dotnet.dll can be found here: https://github.com/hornbill/dotNetApiLib/blob/master/EspApiLib/espapi_dotnet.dll
```


Logging in
===

Using Hornbill API's required an authenticated session the first way to create a session it to call session::userlogon
```
//-- Initiate Class
xmlmc = new XmlmcService("instanceName", "xmlmc", "dav", string.Empty);

// Username
xmlmc.AddParam("userId", "admin");
//-- Password must be base64 encoded
xmlmc.AddParam("password", "password").EncodeValue(XmlmcEncoding.Base64);

//-- Invoke session::userLogon
xmlmc.Invoke("session", "userLogon");

//-- Get SessionId from the API Response
string sessionId = xmlmc.GetResponseParamAsString("sessionId");
```

Using API Keys
===

An Alternative method for logging into Hornbill is to use API Keys these are associated to users in the Administration Tool and are passed with every API Call removing the need to login.
```
//-- Initiate Class
xmlmc = new XmlmcService("instanceName", "xmlmc", "dav", string.Empty);

//-- Set API Key
xmlmc.APIKey = "MyAPIKeyHere";

//-- Invoke session::getSessionInfo
xmlmc.Invoke("session", "getSessionInfo");

//-- Get SessionId from the API Response
string sessionId = xmlmc.GetResponseParamAsString("sessionId");
```

Example C# Application
===

An Example C# Application has been provided here:
https://github.com/hornbill/dotNetApiLib/blob/master/CSharpExample

Functionality Includes
* Login
* Logout
* Get List of Requests
* Log a Request
* User API Keys

The following file needs to be updated to include your instance details:

https://github.com/hornbill/dotNetApiLib/blob/master/CSharpExample/MainWindow.xaml.cs


These strings need to be updated:
```
private readonly string instanceName = ""; //-- Instance Name
private readonly string userId = ""; //-- UserName
private readonly string password = ""; //-- Password
private readonly string apiKey = ""; //-- API Key
```
