This document describes how to build ArcLogistics.sln.

Prerequisites
- Microsoft Visual Studio 2012+
- Xceed Data Grid for WPF (www.xceed.com)
- ActiveReports for .NET 3.0 (www.datadynamics.com)
- .NET Framework 4.6.2

You can obtain evaluation licenses for the Xceed Data Grid and for ActiveReports.

Unpack, install the application, and make sure it works before you build anything.

1) Unpack or copy the code to a local folder.  For example, C:\SourceCode.
2) Install the application.  This will gve you a copy of the ActiveReports assemblies you'll reference next.
3) Run the application to make sure it works and you have valid services.

Update ActiveReports references and license.

4) Obtain your ActiveReports license.  Activate it on the computer you are using to build the application (see http://www.datadynamics.com/Help/arnet3/activereports3_start.htm).
5) Update \ArcLogisticsApp\Properties\licenses.licx.  Swap in the file resulting from activating the license above.
6) Open ArcLogistics.sln in Visual Studio.
7) Check the References for the ArcLogisticsApp.  The 9 ActiveReports* assemby references are probably broken.  Delete these and add new references to the ActiveReports assemblies installed into the application folder.

Update Xceed Data Grid license.

8) In App.xaml.cs, Application_Startup, insert your XCeed Data Grid License key.  Look for "LicenseKey" and edit that line of code with your key.

9) Rebuild the solution.

  