using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Configuration;
using System.IO;

namespace B2CGraphShell
{
    // This program is a conversion from B2CGraphClient using the ADAL library to B2CGraphClient using the MSAL library

    public class Program
    {
        public static string authority = ConfigurationManager.AppSettings["b2c:Authority"];
        public static string tenant = ConfigurationManager.AppSettings["b2c:Tenant"];
        public static string clientId = ConfigurationManager.AppSettings["b2c:ClientId"];
        public static string clientSecret = ConfigurationManager.AppSettings["b2c:ClientSecret"];
        public static string environment = ConfigurationManager.AppSettings["b2c:Environment"];
        public static B2CGraphClientMSAL client = null;
        public static ConsoleColor init = ConsoleColor.White;

        static void Main(string[] args)
        {
            init = Console.ForegroundColor;

            if (args.Length <= 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Please enter a command as the first argument.  Try 'B2C Help' for a list of commands.");
                Console.ForegroundColor = init;
                return;
            }

            client = new B2CGraphClientMSAL(clientId, clientSecret, tenant, authority);

            try
            {
                switch (args[0].ToUpper())
                {
                    case "GET-USER":
                        GetUser(args);
                        break;
                    case "CREATE-USER":
                        CreateUser(args);
                        break;
                    case "UPDATE-USER":
                        UpdateUser(args);
                        break;
                    case "DELETE-USER":
                        DeleteUser(args);
                        break;
                    case "GET-EXTENSION-ATTRIBUTE":
                        GetExtensionAttribute(args);
                        break;
                    case "GET-B2C-APPLICATION":
                        GetB2CExtensionApplication(args);
                        break;
                    case "HELP":
                        PrintHelp(args);
                        break;
                    case "SYNTAX":
                        PrintSyntax(args);
                        break;
                    default:
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Invalid command.  Try 'B2C Help' for a list of commands.");
                        break;
                }
            }

            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;

                var innerException = ex.InnerException;
                if (innerException != null)
                {
                    while (innerException != null)
                    {
                        Console.WriteLine(innerException.Message);
                        innerException = innerException.InnerException;
                    }
                }
                else
                {
                    Console.WriteLine(ex.Message);
                }
            }
            finally
            {
                Console.ForegroundColor = init;

                // Console.ReadLine();
            }
        }

        private static void GetUser(string[] args)
        {
            Guid temp;
            string result;
            if (args.Length <= 1)
            {
                result = client.GetAllUsers(null).Result;
            }
            else if (Guid.TryParse(args[1], out temp))
            {
                result = client.GetUserByObjectId(args[1]).Result;
            }
            else
            {
                result = client.GetAllUsers(args[1]).Result;
            }

            object formatted = JsonConvert.DeserializeObject(result);

            string returnString = formatted.ToString();

            if (returnString.Contains("error"))
                Console.ForegroundColor = ConsoleColor.Red;
            else
                Console.ForegroundColor = ConsoleColor.White;

            Console.WriteLine(JsonConvert.SerializeObject(formatted, Formatting.Indented));
        }

        private static void CreateUser(string[] args)
        {
            if (args.Length <= 1)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Please include a path to a .json file.  Run B2C Syntax to see examples.");
                Console.ForegroundColor = init;
                return;
            }

            string json = File.ReadAllText(args[1]);
            object formatted = JsonConvert.DeserializeObject(client.CreateUser(json).Result);

            string returnString = formatted.ToString();

            if (returnString.Contains("error"))
                Console.ForegroundColor = ConsoleColor.Red;
            else
                Console.ForegroundColor = ConsoleColor.White;

            Console.WriteLine(JsonConvert.SerializeObject(formatted, Formatting.Indented));

            JObject app = JObject.Parse(formatted.ToString());
            string id = (string)app["id"];
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\nB2C user ID is " + id + "\n");

            Console.WriteLine("\ne.g. B2C Get-User " + id);
            Console.WriteLine("\n     B2C Update-User " + id + " path to json file");
            Console.WriteLine("\n     B2C Delete-User " + id);
        }

        private static void UpdateUser(string[] args)
        {
            if (args.Length <= 2)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Please include an objectId and a path to a .json file.  Run B2C Syntax to see examples.");
                Console.ForegroundColor = init;
                return;
            }

            string json = File.ReadAllText(args[2]);
            object formatted = JsonConvert.DeserializeObject(client.UpdateUser(args[1], json).Result);

        }

        private static void DeleteUser(string[] args)
        {
            if (args.Length <= 1)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Please include an objectId.  Run B2CGraphShell Syntax to see examples.");
                Console.ForegroundColor = init;
                return;
            }

            object formatted = JsonConvert.DeserializeObject(client.DeleteUser(args[1]).Result);

        }

        private static void GetExtensionAttribute(string[] args)
        {
            if (args.Length <= 1)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Please include the b2c-extensions-app objectId.  Run B2C Syntax to see examples.");
                Console.ForegroundColor = init;
                return;
            }

            object formatted = JsonConvert.DeserializeObject(client.GetExtensions(args[1]).Result);
            string returnString = formatted.ToString();

            if (returnString.Contains("error"))
                Console.ForegroundColor = ConsoleColor.Red;
            else
                Console.ForegroundColor = ConsoleColor.White;

            Console.WriteLine(JsonConvert.SerializeObject(formatted, Formatting.Indented));
        }

        private static void GetB2CExtensionApplication(string[] args)
        {
            object formatted = JsonConvert.DeserializeObject(client.GetApplications("$filter=startswith(displayName, 'b2c-extensions-app')").Result);

            string returnString = formatted.ToString();

            if (returnString.Contains("error"))
                Console.ForegroundColor = ConsoleColor.Red;
            else
                Console.ForegroundColor = ConsoleColor.White;

            Console.WriteLine(JsonConvert.SerializeObject(formatted, Formatting.Indented));

            JObject app = JObject.Parse(formatted.ToString());
            string id = (string)app["value"][0]["id"];
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\nB2C application ID is " + id);
            Console.WriteLine("\nRun B2C Get-Extension-Attribute " + id);

        }

        private static void PrintSyntax(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("- Square brackets indicate cmdcmdoptional arguments");
            Console.WriteLine("- Curly brackets indicate valid choices for a parameter");
            Console.WriteLine("- For information on supported query expressions, including $filter, $top, $orderby, and $expand, see https://docs.microsoft.com/en-us/graph/query-parameters");
            Console.WriteLine("- To find the objectId of the b2c-extensions-app, run Get-B2C-Application");
            Console.WriteLine("");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("Get-User                     : B2C Get-User [UserObjectId || Query]");
            Console.WriteLine("                             : B2C Get-User 6d51065f-2e1d-4707-8ec9-ad491bae55dd");
            Console.WriteLine("Create-User                  : B2C Create-User RelativePathToJson");
            Console.WriteLine("                             : B2C Create-User ..\\..\\..\\usertemplate-email.json");
            Console.WriteLine("Update-User                  : B2C Update-User UserObjectId RelativePathToJson");
            Console.WriteLine("                             : B2C Update-User 6d51065f-2e1d-4707-8ec9-ad491bae55dd ..\\..\\..\\usertemplate-email.json");
            Console.WriteLine("Delete-User                  : B2C Delete-User UserObjectId");
            Console.WriteLine("                             : B2C Delete-User 6d51065f-2e1d-4707-8ec9-ad491bae55dd");
            Console.WriteLine("Get-Extension-Attribute      : B2C Get-Extension-Attribute B2CExtensionsApplicationObjectId");
            Console.WriteLine("                             : B2C Get-Extension-Attribute 909544d8-f8c0-49c7-b137-a89faff6f882");
            Console.WriteLine("Get-B2C-Application          : B2C Get-B2C-Application");
            Console.WriteLine("Help                         : B2C Help");
            Console.WriteLine("Syntax                       : B2C Syntax");
        }

        private static void PrintHelp(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("Get-User                     : Read users from your B2C directory.  Optionally accepts an ObjectId as a 2nd argument, and query expression as a 3rd argument.");
            Console.WriteLine("Create-User                  : Create a new user in your B2C directory.  Requires a path to a .json file which contains required and optional information as a 2nd argument.");
            Console.WriteLine("Update-User                  : Update an existing user in your B2C directory.  Requires an objectId as a 2nd arguemnt & a path to a .json file as a 3rd argument.");
            Console.WriteLine("Delete-User                  : Delete an existing user in your B2C directory.  Requires an objectId as a 2nd argument.");
            Console.WriteLine("Get-Extension-Attribute      : Lists all extension attributes in your B2C directory.  Requires the b2c-extensions-app objectId as the 2nd argument.");
            Console.WriteLine("Get-B2C-Application          : Get the B2C Extensions Application in your B2C directory, so you can retrieve the objectId and pass it to other commands.");
            Console.WriteLine("Help                         : Prints this help menu.");
            Console.WriteLine("Syntax                       : Gives syntax information for each command, along with examples.");
        }
    }
}
