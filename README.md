# Metasys Basic Services <!-- omit in toc --> <a href="https://www.nuget.org/packages/JohnsonControls.Metasys.BasicServices/" rel="Johnson Controls Metasys BasicServices">![Nuget](https://img.shields.io/nuget/v/JohnsonControls.Metasys.BasicServices)</a>

This project provides a library for accessing the most common services of the Metasys Server API.
The intent is to provide an API that is very similar to the original MSSDA
(Metasys System Secure Data Access) library.

For versioning information see the [changelog](CHANGELOG).

## Contents <!-- omit in toc -->

- [Installation](#installation)
  - [Prerequisites](#prerequisites)
  - [.NET](#net)
  - [COM (Component Object Model)](#com-component-object-model)
  - [Excel and Visual Basic for Applications (VBA)](#excel-and-visual-basic-for-applications-vba)
- [Usage (.NET)](#usage-net)
  - [Creating a Client](#creating-a-client)
  - [Login and Access Tokens](#login-and-access-tokens)
  - [Get an Object Id](#get-an-object-id)
  - [Get a Property](#get-a-property)
  - [Write a Property](#write-a-property)
  - [Get and Send Commands](#get-and-send-commands)
  - [Get Network Devices and other Objects](#get-network-devices-and-other-objects)
  - [Localization of Metasys Enumerations](#localization-of-metasys-enumerations)
- [License](#license)
- [Contributing](#contributing)
- [Additional Information](#additional-information)
  - [Supported Localization Languages](#supported-localization-languages)

## Installation

### Prerequisites

- A method of installing NuGet packages as described by the Microsoft
[Package Consumpion Workflow](https://docs.microsoft.com/en-us/nuget/consume-packages/overview-and-workflow).

### .NET

To use in a .NET application include the following in the target source files:

```csharp
using JohnsonControls.Metasys.BasicServices;
```

Option 1: Consume the package by adding a PackageReference.

1. Add the following to the project's .csproj file:

    ```csharp
    <ItemGroup>
        <PackageReference Include="JohnsonControls.Metasys.BasicServices" Version="<target version>" />
    </ItemGroup>
    ```

Option 2: Consume the package by installing on the command line.
This workflow shows how to use the nuget.exe CLI (command line interface).

1. Open a command line and navigate to your project's main directory.
2. Execute the following command:

    ```cli
    nuget install JohnsonControls.Metasys.BasicServices -OutputDirectory packages
    ```

3. Update all packages with the command:

    ```cli
    nuget restore
    ```

### COM (Component Object Model)

Use the one click setup provided with the release tag to install the required dependencies in your system and register the COM DLL.
Keep in mind that for developing purposes the solution is already configured to register with the COM interop when the project is built. Alternatively, you can manually register or unregister the DLL use the RegCOM.bat and UnregCOM.bat scripts located in the Scripts directory.

To manually register for COM interop you can also use the Regasm.exe (Assembly Registration Tool) or similar tool.
[See more information here](https://docs.microsoft.com/en-us/dotnet/framework/tools/regasm-exe-assembly-registration-tool).

### Excel and Visual Basic for Applications (VBA)

LegacyMetasysClient allows you to interact with your Metasys server from a VBA application using COM interop.
To get start, enable the Developer Tab in Excel from the menu File-->Options-->Customize Ribbon.
[See more information here](https://support.office.com/en-us/article/show-the-developer-tab-e1192344-5e56-4d45-931b-e5fd9bea2d45).
Finally, from the Developer Tab click the Visual Basic button to open the editor and add the reference to the Metasys Services Object Library from the references list.
[See more information here](https://docs.microsoft.com/en-us/office/vba/language/how-to/check-or-add-an-object-library-reference).

## Usage (.NET)

This section demonstrates how to use the MetasysClient to interact with your Metasys server from a .NET application.
<!-- Download the sample project [here](https://github.com/metasys-server). -->

### Creating a Client

To create a new client and connect to a Metasys server with the default settings use:

```csharp
var client = new MetasysClient("hostname");
```

There are three optional parameters when creating a new client:

- ignoreCertificateErrors: If your server does not have a valid certificate the MetasysClient will not behave as expected and will likely block the connection. Setting the ignoreCertificateErrors = true will ignore this error and make an insecure connection with the server. To avoid this problem ensure the Metasys server has a valid certificate.
- apiVersion: If your server is not a current 10.1 Metasys server or later this SDK will not function correctly. The version parameter takes in an ApiVersion enumeration value that defaults to the most current release of Metasys. For Metasys 10.1 the api version is V2.
- cultureInfo: To set the language for localization specify the target culture with a CultureInfo object. The default culture is en-US.

To create a client that ignores certificate errors for a 10.1 Metasys server with Italian translations of values:

```csharp
CultureInfo culture = new CultureInfo("it-IT");
var client = new MetasysClient(hostname, true, ApiVersion.V2, culture);
```

### Login and Access Tokens

After creating the client, to login use the TryLogin method which takes a username, password, and an optional parameter to automatically refresh the access token during the client's lifetime. The default token refresh policy is true.

```csharp
// Automatically refresh token
client.TryLogin("username", "password");

// Do not automatically refresh token
client.TryLogin("username", "password", false);
```

At any time you want to manually refresh the access token before it expires use the following:

```csharp
client.Refresh();
```

To use the authorization token in an different HttpClient use the AccessToken object returned by these methods. A successful token will be in the form "Bearer ...".

```csharp
AccessToken token = client.Refresh();
Console.WriteLine($"Token: {token.Token}\nExpires: {dateToDisplay.ToString("g", token.Expires)}");

// Token: Bearer eyJ0eXAi...
// Expires: 10/1/2019 5:04 PM
```

### Get an Object Id

In order to use most of the methods in MetasysClient the id of the target object must be known. This id is in the form of a Guid and can be requested using the following given you know the item reference of the object:

```csharp
Guid id = client.GetObjectIdentifier("siteName:naeName/Folder1.AV1");
```

### Get a Property

In order to get a property you must know the Guid of the target object. An object called "Variant" is returned when getting a property from an object. Variant contains the property received in many different forms. There is a bit of intuition when handling a Variant since it will not explicitly provide the type of object received by the server. If the server cannot find the target object or attribute on the object this method will return a null value.

```csharp
Variant result = client.ReadProperty(id, "presentValue");
string stringValue = result.StringValue;
double numericValue = result.NumericValue;
bool booleanValue = result.BooleanValue;
Variant[] array = result.ArrayValue;
```

There is a method to get multiple properties from multiple objects. This can be very useful if the objects all are of the same type or have the same target properties.

```csharp
List<Guid> ids = new List<Guid> { id1, id2 };
List<string> attributes = new List<string> { "name", "description" };

// Do not throw exceptions for missing resources
IEnumerable<VariantMultiple> results = client.ReadPropertyMultiple(ids, attributes);

// Throw exceptions if an attribute is not found by the server
IEnumerable<VariantMultiple> results = client.ReadPropertyMultiple(ids, attributes, true);

IEnumerable<Variant> id1Variants = results.ElementAt(0).Variants;
```

### Write a Property

In order to write a property you must have the Guid of the object and know the attribute name and type. This method contains an optional priority parameter to specify the write priority of the value. This priority is in the form of an enumeration such as "writePriorityEnumSet.priorityNone". To see more options use the "api/v2/enumSets/1/members" or "api/v2/schemas/enums/writePriorityEnumSet" http requests defined in the [Metasys API](https://metasys-server.github.io/api-landing/api/v2/).

```csharp
Guid id = client.GetObjectIdentifier("siteName:naeName/Folder1.AV1");
client.WriteProperty(id, "description", "This is an AV.");
client.WriteProperty(id, "description", "This is an AV.", "writePriorityEnumSet.priorityNone");
```

To change the same attribute values of many objects use the WritePropertyMultiple method. This method also accepts an optional write priority.

```csharp
List<Guid> ids = new List<Guid> { id1, id2 };
List<string> attributes = new List<string> { ("description", "This is an AV.") };
client.WritePropertyMultiple(ids, attributes);
```

### Get and Send Commands

To get all available commands on an object use the GetCommands method. This method will return a list of Command objects. The ToString() method is a useful tool to display the available commands and any information associated with it. When sending a command the Command.CommandId attribute is used as the parameter:

```csharp
var commands = client.GetCommands(id).ToList();
var command = commands[0].CommandId; // EnableAlarms, DisableAlarms, ReleaseAll, etc.
client.SendCommand(command);
```

When sending a command there may or may not be a single value or list of values that needs to be sent with the command. The Command.Items property will list all of these values as Items which contains the Title and Type of the value to change. If the type of an Item is "oneOf" this indicates the values is an enumeration and the possible values will be contained in the EnumerationValues list. Keep in mind the values to be sent in the command is the TitleEnumerationKey not the Title. The Title is the user friendly translated value that describes the enumeration. For example, an "AV Mapper" object has the following commands:

- "Adjust" that accepts a number value to adjust the present value.
- "TemporaryOperatorOverride" that accepts the value to adjust the present value, the hours, and the minutes for the temporary adjustment.
- "Release" which accepts two enumeration values for the attribute and new priority level.

The values on the Commands will model the following:

```csharp
var commands = client.GetCommands(id).ToList();
var commandAdjust = commands[0].CommandId; // Adjust
var commandOverride = commands[1].CommandId; // TemporaryOperatorOverride
var commandRelease = commands[2].CommandId; // Release

var adjustItems = commands[0].Items.ToList();
var overrideItems = commands[1].Items.ToList();
var releaseItems = commands[2].Items.ToList();

adjustItems[0].Title; // Value
adjustItems[0].Type; // number

overrideItems[0].Title; // Value
overrideItems[0].Type; // number
overrideItems[1].Title; // Hours
overrideItems[1].Type; // number
overrideItems[2].Title; // Minutes
overrideItems[2].Type; // number

releaseItems[0].Title; // oneOf
releaseItems[0].Type; // enum
var enumValues = releaseItems[0].EnumerationValues.ToList();
enumValues[0].TitleEnumerationKey; // attributeEnumSet.presentValue
releaseItems[1].Title; // oneOf
releaseItems[1].Type; // enum
var enumValues2 = releaseItems[1].EnumerationValues.ToList();
enumValues2[0].TitleEnumerationKey; // writePriorityEnumSet.priorityNone
enumValues2[1].TitleEnumerationKey; // writePriorityEnumSet.priorityManualEmergency
// ...etc
```

To send the command for each of these it would model the following:

```csharp
var list1 = new List<object> { 70 };
client.SendCommand(commandAdjust, list1);

var list2 = new List<object> { 70, 1, 30 };
client.SendCommand(commandOverride, list2);

var list3 = new List<object> { "attributeEnumSet.presentValue", "writePriorityEnumSet.priorityNone" };
client.SendCommand(commandRelease, list3);
```

### Get Network Devices and other Objects

To get all the available network devices use the GetNetworkDevices method which returns a list of MetasysObjects. This accepts an optional type number as a string to filter the response. To get all of the available types on your server use the GetNetworkDeviceTypes method which returns a list of MetasysObjectType.

```csharp
List<MetasysObjectType> types = client.GetNetworkDeviceTypes().ToList();
int type1 = types[0].Id;
List<MetasysObject> devices = client.GetNetworkDevices(type1.ToString()).ToList();
```

To get the child devices or objects of an object use the GetObjects method. This takes the Guid of the parent object and an optional number of levels to retrieve. The default is 1 level or just the immediate children of the object. Depending on the number of objects on your server this method can take a very long time to complete.

```csharp
List<MetasysObject> devices = client.GetObjects(id).ToList();
```

### Localization of Metasys Enumerations

To get automatically translated enumerations on enumerations returned from a Metasys server you must specify the culture during client creation, or set the "Culture" property before using the "get" methods. The default language for translations will be the machine's current culture ([see more information here](https://docs.microsoft.com/en-us/dotnet/api/system.globalization.cultureinfo.currentculture)) or en-US (American English) if the language is not supported (see [Supported Localization Languages](#supported-localization-languages)).

```csharp
client.Culture = new CultureInfo("it-IT");
```

Enumerations returned from a Metasys server will be in a format similar to: "reliabilityEnumSet.reliable". MetasysClient has a method to translate these enumerations manually. This method can be very useful if using an external HttpClient since Metasys servers do not hold translation information.

```csharp
// Access from the client object
string translated = client.Localize("reliabilityEnumSet.reliable"); // Reliable

// Access without instantiating a client
string translated = MetasysClient.StaticLocalize("reliabilityEnumSet.reliable",
    new CultureInfo("it-IT"));  // Affidabile
```

Note: If an automatically translated value (such as Variant.StringValue) contains an enumeration value and not a translated string there could be an error with the MetasysClient globalization setup.

If the enumeration key is desired over the translated value use the EnumerationKey attribute. For example, the translated Variant.Reliability has the enumeration key under the attribute: Variant.ReliabilityEnumerationKey. See the documentation of each Model for more information.

<!-- ## Usage (COM) -->

## License

See [LICENSE](LICENSE).

## Contributing

See [CONTRIBUTING](CONTRIBUTING).

## Additional Information

### Supported Localization Languages

- en-US
- cs-CZ
- de-DE
- es-ES
- fr-FR
- hu-HU
- it-IT
- ja-JP
- ko-KR
- nb-NO
- nl-NL
- pl-PL
- pt-BR
- ru-RU
- sv-SE
- tr-TR
- zh-CN
- zh-Hans-CN
- zh-Hant-TW
- zh-TW
