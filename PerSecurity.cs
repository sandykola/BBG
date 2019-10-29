/*
*THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT
*WARRANTY OF ANY KIND, EITHER EXPRESSED OR IMPLIED,
*INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES
*OF MERCHANTABILITY AND/OR FITNESS FOR A  PARTICULAR
*PURPOSE.
*
*/
namespace PerSecurity_Dotnet
{
    using CommandLine;
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Security.Cryptography.X509Certificates;
    using PerSecurity_Dotnet;
    using PerSecurity_Dotnet.PerSecurityWSDL;

    public class Options
    {
        public const string ProgramFlagHelpText = "The program flag to be used with the sample " +
                    "code. NOTE: This choice may have financial " +
                    "implications, please refer to the Data License " +
                    "User Guide for more details. Options are: " +
                    "|scheduled|adhoc|";

        public const string SampleNameHelpText = "Name of the sample code to run. Options are: " +
                    "|GetHistory|GetCompany|GetFields|Cancel|GetDataMacro|GetHistoryMacro|GetAllQuotes| " +
                    "GetDataFieldSet|GetDataBulkFields|GetQuotes|Scheduled|GetDataPricingSourceHeader| " +
                    "GetDataOverrides|GetData|GetActions|PortfolioValidation|GetCompanyFieldSet| " +
                    "GetHistoryPricingSourceHeader|";

        [Option('p', "programFlag", Required = true, HelpText = ProgramFlagHelpText)]
        public string ProgramFlag { get; set; }

        [Option('s', "sampleName", Required = true, HelpText = SampleNameHelpText)]
        public string SampleName { get; set; }
    }

    public class PerSecurity
    {
        public const int DataNotAvailable = 100;
        public const int Success = 0;
        public const int RequestError = 200;
        public const int PollInterval = 30000;

        private static void Main(string[] args)
        {
            try
            {
                ProgramFlag programFlag = default(ProgramFlag);
                string sampleName = string.Empty;

                Dictionary<string, ProgramFlag> programFlagRef = new Dictionary<string, ProgramFlag>();
                programFlagRef.Add("adhoc", ProgramFlag.adhoc);
                programFlagRef.Add("scheduled", ProgramFlag.oneshot);

                Parser.Default.ParseArguments<Options>(args)
                    .WithParsed<Options>(opts =>
                    {
                        if (!programFlagRef.ContainsKey(opts.ProgramFlag))
                        {
                            Console.WriteLine("Unrecognized programFlag " + opts.ProgramFlag + "\n");
                            Console.WriteLine(Options.ProgramFlagHelpText);
                            Environment.Exit(1);
                        }
                        programFlag = programFlagRef[opts.ProgramFlag];
                        sampleName = opts.SampleName;
                    })
                    .WithNotParsed<Options>(opts =>
                    {
                        Environment.Exit(1);
                    });

                string clientCertFilePath = ConfigurationManager.AppSettings["clientCertFilePath"];
                string clientCertPassword = ConfigurationManager.AppSettings["clientCertPassword"];
                X509Certificate2 clientCert = new X509Certificate2(clientCertFilePath, clientCertPassword);

                PerSecurity_Dotnet.PerSecurityWSDL.PerSecurityWSClient ps = new PerSecurityWSClient("PerSecurityWSPort");
                ps.ClientCredentials.ClientCertificate.Certificate = clientCert;

                /*Add following lines for proxy
                *NetworkCredential nc = new NetworkCredential("username", "password", "domain");
                WebProxy proxy = new WebProxy("http//webproxyserver:80");
                proxy.Credentials = nc;
                ps.Proxy = proxy;
                 */
                switch (sampleName)
                {
                    case "GetData":
                        // Making request for non-bulk fields
                        Console.WriteLine("\n**********Running " + sampleName + " sample**********\n");
                        GetData smpReq = new GetData();
                        smpReq.run(ps);
                        break;

                    case "GetDataPricingSourceHeader":
                        // Making GetData request with a Pricing Source in header
                        Console.WriteLine("\n**********Running " + sampleName + " sample**********\n");
                        GetDataPricingSourceHeader getDataPricingSrcHdrReq = new GetDataPricingSourceHeader();
                        getDataPricingSrcHdrReq.run(ps);
                        break;

                    case "Scheduled":
                        // Making scheduled request
                        Console.WriteLine("\n**********Running " + sampleName + " sample**********\n");
                        Scheduled schReq = new Scheduled();
                        schReq.run(ps);
                        break;

                    case "GetDataBulkFields":
                        // Making GetData request with bulk fields
                        Console.WriteLine("\n**********Running " + sampleName + " sample**********\n");
                        GetDataBulkFields blkReq = new GetDataBulkFields();
                        blkReq.run(ps);
                        break;

                    case "GetDataMacro":
                        // Making request for Macro requests
                        Console.WriteLine("\n**********Running " + sampleName + " sample**********\n");
                        GetDataMacro mcroReq = new GetDataMacro();
                        mcroReq.run(ps);
                        break;

                    case "GetHistory":
                        // Making History request
                        Console.WriteLine("\n**********Running " + sampleName + " sample**********\n");
                        GetHistory histReq = new GetHistory();
                        histReq.run(ps);
                        break;

                    case "GetHistoryMacro":
                        // Making History request with a macro
                        Console.WriteLine("\n**********Running " + sampleName + " sample**********\n");
                        GetHistoryMacro histMacrReq = new GetHistoryMacro();
                        histMacrReq.run(ps);
                        break;

                    case "GetHistoryPricingSourceHeader":
                        // Making History request with a Pricing Source in header
                        Console.WriteLine("\n**********Running " + sampleName + " sample**********\n");
                        GetHistoryPricingSourceHeader pricingSrcHdrReq = new GetHistoryPricingSourceHeader();
                        pricingSrcHdrReq.run(ps);
                        break;

                    case "GetCompany":
                        // Making a Get Company request
                        Console.WriteLine("\n**********Running " + sampleName + " sample**********\n");
                        GetCompany getCompReq = new GetCompany();
                        getCompReq.run(ps);
                        break;

                    case "GetQuotes":
                        // Making a Get Quotes request
                        Console.WriteLine("\n**********Running " + sampleName + " sample**********\n");
                        GetQuotes getQuotesReq = new GetQuotes();
                        getQuotesReq.run(ps, programFlag);
                        break;

                    case "GetActions":
                        // Making a Get Actions request
                        Console.WriteLine("\n**********Running " + sampleName + " sample**********\n");
                        GetActions getActionsReq = new GetActions();
                        getActionsReq.run(ps);
                        break;

                    case "Cancel":
                        // Making a Cancel request
                        Console.WriteLine("\n**********Running " + sampleName + " sample**********\n");
                        Cancel cancelReq = new Cancel();
                        cancelReq.run(ps);
                        break;

                    case "PortfolioValidation":
                        // Making a get portfolio validation request
                        Console.WriteLine("\n**********Running " + sampleName + " sample**********\n");
                        PortfolioValidation portFolioVldtn = new PortfolioValidation();
                        portFolioVldtn.run(ps);
                        break;

                    case "GetDataFieldSet":
                        // Making a getdata request with fieldset
                        Console.WriteLine("\n**********Running " + sampleName + " sample**********\n");
                        GetDataFieldSet gtDtFldStReq = new GetDataFieldSet();
                        gtDtFldStReq.run(ps);
                        break;

                    case "GetAllQuotes":
                        // Making a getallquotes request
                        Console.WriteLine("\n**********Running " + sampleName + " sample**********\n");
                        GetAllQuotes gtAllQtsReq = new GetAllQuotes();
                        gtAllQtsReq.run(ps, programFlag);
                        break;

                    case "GetCompanyFieldSet":
                        // Making a getcompany fieldset request
                        Console.WriteLine("\n**********Running " + sampleName + " sample**********\n");
                        GetCompanyFieldSet gtCmpnyFldSet = new GetCompanyFieldSet();
                        gtCmpnyFldSet.run(ps);
                        break;

                    case "GetDataOverrides":
                        // Making a simple getdata request with overrides
                        Console.WriteLine("\n**********Running " + sampleName + " sample**********\n");
                        GetDataOverrides smpReqOvr = new GetDataOverrides();
                        smpReqOvr.run(ps);
                        break;

                    case "GetFields":
                        // Making a getFields request
                        Console.WriteLine("\n**********Running " + sampleName + " sample**********\n");
                        GetFields sbmtGtFldsReq = new GetFields();
                        sbmtGtFldsReq.run(ps);
                        break;

                    case "GetCorrections":
                        // Making a getCorrections request
                        Console.WriteLine("\n**********Running " + sampleName + " sample**********\n");
                        GetCorrections sbmtGtCorrReq = new GetCorrections();
                        sbmtGtCorrReq.run(ps);
                        break;

                    default:
                        Console.WriteLine("Unrecognized sampleName " + sampleName + "\n");
                        Console.WriteLine(Options.SampleNameHelpText);
                        break;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
