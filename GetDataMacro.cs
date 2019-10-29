/*
*THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT
*WARRANTY OF ANY KIND, EITHER EXPRESSED OR IMPLIED,
*INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES
*OF MERCHANTABILITY AND/OR FITNESS FOR A  PARTICULAR
*PURPOSE.
*/

namespace PerSecurity_Dotnet
{
    /*
    * GetDataMacro - This class submits a getdata request by specifying the instruments as macros and fields for
    * the request parameters. This is followed by retrieve getdata request, to get the values for the fields. This class
    * also illustrates the case when an incorrect macro is passed in the request.
    */
    using System;
    using System.Collections.Generic;
    using System.Text;
    using PerSecurity_Dotnet.PerSecurityWSDL;

    internal class GetDataMacro
    {
        public void run(PerSecurity_Dotnet.PerSecurityWSDL.PerSecurityWS ps)
        {
            try
            {
                // Setting request header information
                GetDataHeaders getDataHdrs = new GetDataHeaders();
                getDataHdrs.secmasterSpecified = true;
                getDataHdrs.secmaster = true;

                // Setting instruments for request
                Instruments instrs = new Instruments();
                instrs.macro = new Macro[2];
                instrs.macro[0] = new Macro();
                instrs.macro[0].primaryQualifier = new PrimaryQualifier();
                instrs.macro[0].primaryQualifier.primaryQualifierType = MacroType.SECTYP;
                instrs.macro[0].primaryQualifier.primaryQualifierValue = "OPT_CHAIN";

                instrs.macro[0].secondaryQualifier = new SecondaryQualifier[1];
                instrs.macro[0].secondaryQualifier[0] = new SecondaryQualifier();
                instrs.macro[0].secondaryQualifier[0].secondaryQualifierOperator = SecondaryQualifierOperator.Equals;
                instrs.macro[0].secondaryQualifier[0].secondaryQualifierType = SecondaryQualifierType.SECURITY_DES;
                instrs.macro[0].secondaryQualifier[0].secondaryQualifierValue = "AMZN US Equity";
                instrs.macro[0].overrides = new Override[1];

                // A sample of incorrect macro
                instrs.macro[1] = new Macro();
                instrs.macro[1].primaryQualifier = new PrimaryQualifier();
                instrs.macro[1].primaryQualifier.primaryQualifierType = MacroType.SECTYP;
                instrs.macro[1].primaryQualifier.primaryQualifierValue = "OPTCHAIN";

                instrs.macro[1].secondaryQualifier = new SecondaryQualifier[1];
                instrs.macro[1].secondaryQualifier[0] = new SecondaryQualifier();
                instrs.macro[1].secondaryQualifier[0].secondaryQualifierOperator = SecondaryQualifierOperator.Equals;
                instrs.macro[1].secondaryQualifier[0].secondaryQualifierType = SecondaryQualifierType.SECURITY_DES;
                instrs.macro[1].secondaryQualifier[0].secondaryQualifierValue = "AMZN US Equity";

                // Setting fields for the request
                string[] field = new string[] { "NAME", "TICKER", "PX_LAST" };

                // Submit getdata request
                Console.WriteLine("Sending submit getdata request");
                SubmitGetDataRequest sbmtGtDtReq = new SubmitGetDataRequest();
                sbmtGtDtReq.headers = getDataHdrs;
                sbmtGtDtReq.fields = field;
                sbmtGtDtReq.instruments = instrs;

                submitGetDataRequestRequest sbmtGtDtReqReq = new submitGetDataRequestRequest(sbmtGtDtReq);
                submitGetDataRequestResponse sbmtGtDtReqResp = ps.submitGetDataRequest(sbmtGtDtReqReq);
                SubmitGetDataResponse sbmtGtDtResp = sbmtGtDtReqResp.submitGetDataResponse;

                System.Console.WriteLine("status " + sbmtGtDtResp.statusCode.description);
                System.Console.WriteLine("Submit getdata request -  response ID = " + sbmtGtDtResp.responseId);

                // retrieve getdata request. The response ID sent for the request is the response ID
                // received from SubmitGetDataRequest()

                RetrieveGetDataRequest rtrvGtDrReq = new RetrieveGetDataRequest();
                rtrvGtDrReq.responseId = sbmtGtDtResp.responseId;

                retrieveGetDataResponseRequest rtrvGtDrRespReq = new retrieveGetDataResponseRequest(rtrvGtDrReq);
                retrieveGetDataResponseResponse rtrvGtDrRespResp;

                RetrieveGetDataResponse rtrvGtDrResp;

                Console.WriteLine("Retrieve getdata request");

                // Keep polling until data becomes available
                do
                {
                    System.Threading.Thread.Sleep(PerSecurity.PollInterval);
                    rtrvGtDrRespResp = ps.retrieveGetDataResponse(rtrvGtDrRespReq);
                    rtrvGtDrResp = rtrvGtDrRespResp.retrieveGetDataResponse;
                }
                while (rtrvGtDrResp.statusCode.code == PerSecurity.DataNotAvailable);

                if (rtrvGtDrResp.statusCode.code == PerSecurity.Success)
                {
                    Console.WriteLine("Retrieve getdata request successful");
                    for (int i = 0; i < rtrvGtDrResp.instrumentDatas.Length; i++)
                    {
                        if (rtrvGtDrResp.instrumentDatas[i].code.Equals("0"))
                        {
                            System.Console.WriteLine("Data for :" + rtrvGtDrResp.instrumentDatas[i].instrument.id +
                                "  " + rtrvGtDrResp.instrumentDatas[i].instrument.yellowkey);
                            for (int j = 0; j < rtrvGtDrResp.instrumentDatas[i].data.Length; j++)
                            {
                                if (rtrvGtDrResp.instrumentDatas[i].data[j].isArray == true)
                                {
                                    // In case this is a bulk field request
                                    for (int k = 0; k < rtrvGtDrResp.instrumentDatas[i].data[j].
                                        bulkarray.Length; k++)
                                    {
                                        Console.WriteLine("-------------------------");
                                        for (int l = 0; l < rtrvGtDrResp.instrumentDatas[i].data[j].
                                            bulkarray[k].data.Length; l++)
                                        {
                                            Console.WriteLine(rtrvGtDrResp.instrumentDatas[i].data[j]
                                                .bulkarray[k].data[l].value);
                                        }
                                    }
                                }
                                else
                                {
                                    System.Console.WriteLine("	" + rtrvGtDrResp.fields[j] + " : " +
                                        rtrvGtDrResp.instrumentDatas[i].data[j].value);
                                }
                            }
                        }

                        // If an incorrect macro was passed in the request, display the error code and the incorrect macro
                        else
                        {
                            System.Console.WriteLine("\n Error Code " + rtrvGtDrResp.instrumentDatas[i].code +
                                ": incorrect macro. The Macro object is as follows:");
                            Console.WriteLine("Primary Qualifier -");
                            Console.WriteLine("Primary Qualifier type:" + rtrvGtDrResp.instrumentDatas[i].
                                macro.primaryQualifier.primaryQualifierType);
                            Console.WriteLine("Primary Qualifier value:" + rtrvGtDrResp.instrumentDatas[i].
                                macro.primaryQualifier.primaryQualifierValue);
                            Console.WriteLine("Secondary Qualifier -");
                            for (int l = 0; l < rtrvGtDrResp.instrumentDatas[i].macro.secondaryQualifier.Length; l++)
                            {
                                Console.WriteLine("Secondary Qualifier type :" + rtrvGtDrResp.instrumentDatas[i].
                                    macro.secondaryQualifier[l].secondaryQualifierType);
                                Console.WriteLine("Secondary Qualifier Value :" + rtrvGtDrResp.instrumentDatas[i].
                                    macro.secondaryQualifier[l].secondaryQualifierValue);
                                Console.WriteLine("Secondary Qualifier Operator :" + rtrvGtDrResp.instrumentDatas[i].
                                    macro.secondaryQualifier[l].secondaryQualifierOperator);
                            }
                        }

                    }

                }
                else if (rtrvGtDrResp.statusCode.code == PerSecurity.RequestError)
                    Console.WriteLine("Error in the submitted request");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + "  " + e.StackTrace);
            }
        }
    }

}
