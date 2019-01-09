# route-planner-csharp

DEPRECATION NOTICE UPDATE: January 9, 2019
As of December 31, 2018, official Esri support and contribution to this sample has ended. Some components of this project have become
out-of-date and it is no longer practical for the project as a whole to be maintained. The project as-built will continue to run when
utilizing on-premise (ArcGIS Server) service endpoints for Route, Solve Vehicle Routing Problem, and Geocode. However the project will
cease to function using cloud services in early 2019 when ArcGIS Online suport for Transport Layer Security (TLS) Protocol Support
1.0/1.2 support ends. For more information on this change and its impact, see https://support.esri.com/en/tls

Esri has provided one final update to the source code consisting of a sample upgrade to .NET Framework 4.6.2 to provide TLS1.2 support.
Please find this update within the branch named "TLS-Update". Note that the application with this changes has not been feature tested
and an updated applcation installer including these changes is not being provided. 

Routing and scheduling sample application using arcgis.com directions (network analysis) services.

![App](https://raw.github.com/Esri/route-planner-csharp/master/RoutePlanner80.png)

## Features

Route Planner is a complete sample application for scheduling multiple vehicles.  Describe your routes, import your orders, build routes, and send driving directions to your drivers.
The Route Planner sample application:
* Uses ArcGIS.com services for mapping, geocoding and routing (organizational account required).
* Supports building optimal routes and route editing.
* Print route reports for drivers or summary information for management.
* Publish routes to a feature service for sharing across organizations.
* Extensible plugin framework for buttons, panels, and tasks.


## Instructions

1. To install and use the pre-built binaries, download and run RoutePlannerSetup.EXE.

2. To build the application, download RoutePlanner_DeveloperTools\Source and see ReadMe.txt for build instructions. 

3. To contribute: Fork and then clone the repository.


## Requirements

To rebuild and redeploy the application, you will need the below developer components.  To simply use the existing pre-built application, no additional components are necessrily needed - just run the installer.
* ArcGIS Runtime SDK for WPF
* Xceed Data Grid for WPF (www.xceed.com).
* ActiveReports for .NET 3.0 (www.datadynamics.com).

## Resources

* [ArcGIS Runtime SDK for WPF](http://resources.arcgis.com/en/communities/runtime-wpf/index.html)
* [ArcGIS Blog](http://blogs.esri.com/esri/arcgis/)
* [twitter@esri](http://twitter.com/esri)

## Issues

Find a bug or want to request a new feature?  Please let us know by submitting an issue. Note that official bug fixes to this project will end on December 31, 2018.

## Contributing

Esri welcomes contributions from anyone and everyone. Please see our [guidelines for contributing](https://github.com/esri/contributing).

## Licensing

Copyright 2013 Esri

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

   http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.

A copy of the license is available in the repository's [license.txt]( https://raw.github.com/Esri/route-planner-csharp/master/License.txt) file.

[](Esri Tags: ArcGIS Runtime Route Planner ArcLogistics)
[](Esri Language: C-Sharp)
