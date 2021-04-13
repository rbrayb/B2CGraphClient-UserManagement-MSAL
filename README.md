---
services: active-directory-b2c
platforms: dotnet
---

# A Console application for Azure AD B2C User Management the Azure AD Graph
This sample demonstrates how to perform user management by calling the Microsoft Graph in an automated fashion. This aproach is similar to a service account scenario where the application acts as itself, not as a user that signed-in via an interactive user login. This is done by using the OAuth 2.0 client credentials grant.

The original sample used the ADAL library (that called the Azure AD Graph API) that is no longer supported so this is a port of the same functionality to the MSAL library (that calls the Microsoft Graph API).

This uses standard .NET (not .NET Core).

The application covered by this sample is a Windows command-line interface (CLI) that allows you to invoke various methods.

Some sample JSON files are included in the "JSON files" folder.

## Steps to Run
For detailed instructions on how to run this sample, checkout [this document](https://docs.microsoft.com/azure/active-directory-b2c/active-directory-b2c-devquickstarts-graph-dotnet).

Note: This link now points to a .NET Core version.

In "app.config":

Update with your tenant and application details.

* b2c:Authority value="https://login.microsoftonline.com/tenant.onmicrosoft.com"
* b2c:Tenant value="tenant.onmicrosoft.com"
* b2c:ClientId value="cd56788c-1234-4e9e-bbeb-5d9975456705"
* b2c:ClientSecret value="Gy6O.sustgd2_RApD5z-g3lH-6R4278qpe"  

Refer [this](https://docs.microsoft.com/en-us/azure/active-directory-b2c/microsoft-graph-get-started?tabs=app-reg-ga).

Note above values are examples only!

In the JSON files, for [extension attributes](https://docs.microsoft.com/en-us/azure/active-directory-b2c/user-flow-custom-attributes?pivots=b2c-user-flow#using-custom-attribute-with-ms-graph-api), add the application ID of the extensions application e.g.

"extension_application ID_employeeID": "123456"	

Run "B2C Help" for reference:

* Get-User                     : Read users from your B2C directory.  Optionally accepts an ObjectId as a 2nd argument, and query expression as a 3rd argument.
* Create-User                  : Create a new user in your B2C directory.  Requires a path to a .json file which contains required and optional information as a 2nd argument.
* Update-User                  : Update an existing user in your B2C directory.  Requires an objectId as a 2nd arguemnt & a path to a .json file as a 3rd argument.
* Delete-User                  : Delete an existing user in your B2C directory.  Requires an objectId as a 2nd argument.
* Get-Extension-Attribute      : Lists all extension attributes in your B2C directory.  Requires the b2c-extensions-app objectId as the 2nd argument.
* Get-B2C-Application          : Get the B2C Extensions Application in your B2C directory, so you can retrieve the objectId and pass it to other commands.
* Help                         : Prints this help menu.
* Syntax                       : Gives syntax information for each command, along with examples.

## Questions & Issues
Please file any questions or problems with the sample as a github issue.  
