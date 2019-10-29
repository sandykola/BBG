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
    * GetHistoryMacro - This class submits a gethistory request by specifying the instruments as macros and fields
    * for the request parameters. This is followed by retrieve GetHistory request, to get the values
    * for the fields. This class also illustrates the case when an incorrect macro is passed in the request.
    */
    using System;
    using System.Collections.Generic;
    using System.Text;
    using PerSecurity_Dotnet.PerSecurityWSDL;

    internal class GetHistoryMacro
    {
        public void run(PerSecurity_Dotnet.PerSecurityWSDL.PerSecurityWS ps)
        {
            // Setting request headers
            GetHistoryHeaders getHistHeaders = new GetHistoryHeaders();
            DateRange dtRange = new DateRange();
            Duration duration = new Duration();
            duration.days = 3;
            dtRange.duration = duration;
            getHistHeaders.daterange = dtRange;

            // Setting instruments
            Instruments instrs = new Instruments();
            instrs.macro = new Macro[2];
            instrs.macro[0] = new Macro();
            instrs.macro[0].primaryQualifier = new PrimaryQualifier();
            instrs.macro[0].primaryQualifier.primaryQualifierType = MacroType.SECTYP;
            instrs.macro[0].primaryQualifier.primaryQualifierValue = "OPT_CHAIN";

            instrs.macro[0].secondaryQualifier = new SecondaryQualifier[1];
            instrs.macro[0].secondaryQualifier[0] = new SecondaryQualifier();
            instrs.macro[0].secondaryQualifier[0].secondaryQualifierOperator = SecondaryQualifierOperator.Equals;
            instrs.macro[0].secondaryQualifier[0].secondaryQualifierType = SecondaryQualifierType.TICKER;
            instrs.macro[0].secondaryQualifier[0].secondaryQualifierValue = "AMZN US Equity";

            // A sample of incorrect macro
            instrs.macro[1] = new Macro();
            instrs.macro[1].primaryQualifier = new PrimaryQualifier();
            instrs.macro[1].primaryQualifier.primaryQualifierType = MacroType.SECTYP;
            instrs.macro[1].primaryQualifier.primaryQualifierValue = "converts_err";

            instrs.macro[1].secondaryQualifier = new SecondaryQualifier[1];
            instrs.macro[1].secondaryQualifier[0] = new SecondaryQualifier();
            instrs.macro[1].secondaryQualifier[0].secondaryQualifierOperator = SecondaryQualifierOperator.Equals;
            instrs.macro[1].secondaryQualifier[0].secondaryQualifierType = SecondaryQualifierType.TICKER;
            instrs.macro[1].secondaryQualifier[0].secondaryQualifierValue = "F";

            // Setting GetHistory request parameters
            string[] fields = new string[] { "PX_LAST" };
            SubmitGetHistoryRequest sbmtGtHistReq = new SubmitGetHistoryRequest();
            sbmtGtHistReq.headers = getHistHeaders;
            sbmtGtHistReq.instruments = instrs;
            sbmtGtHistReq.fields = fields;

            try
            {
                Console.WriteLine("Submit gethistory request");

                submitGetHistoryRequestRequest sbmtGtHistReqReq = new submitGetHistoryRequestRequest(sbmtGtHistReq);
                submitGetHistoryRequestResponse sbmtGtHistRespResp = ps.submitGetHistoryRequest(sbmtGtHistReqReq);
                SubmitGetHistoryResponse sbmtGtHistResp = sbmtGtHistRespResp.submitGetHistoryResponse;

                System.Console.WriteLine("status " + sbmtGtHistResp.statusCode.description);
                System.Console.WriteLine("Submit gethistory request -  response ID = " + sbmtGtHistResp.responseId);

                Console.WriteLine("Retrieve gethistory request");
                RetrieveGetHistoryRequest rtrvGtHistReq = new RetrieveGetHistoryRequest();
                rtrvGtHistReq.responseId = sbmtGtHistResp.responseId;

                retrieveGetHistoryResponseRequest rtrvGtHistRespReq = new retrieveGetHistoryResponseRequest(rtrvGtHistReq);
                retrieveGetHistoryResponseResponse rtrvGtHistRespResp;

                RetrieveGetHistoryResponse rtrvGtHistResp;

                // keep polling until data becomes available
                do
                {
                    System.Threading.Thread.Sleep(PerSecurity.PollInterval);

                    rtrvGtHistRespResp = ps.retrieveGetHistoryResponse(rtrvGtHistRespReq);
                    rtrvGtHistResp = rtrvGtHistRespResp.retrieveGetHistoryResponse;
                }
                while (rtrvGtHistResp.statusCode.code == PerSecurity.DataNotAvailable);

                // Displaying data
                if (rtrvGtHistResp.statusCode.code == PerSecurity.Success)
                {
                    for (int i = 0; i < rtrvGtHistResp.instrumentDatas.Length; i++)
                    {
                        if (rtrvGtHistResp.instrumentDatas[i].code.Equals("0"))
                        {
                            System.Console.WriteLine("Data for :" + rtrvGtHistResp.instrumentDatas[i].instrument.id +
                            "  " + rtrvGtHistResp.instrumentDatas[i].instrument.yellowkey);
                            Console.WriteLine(rtrvGtHistResp.instrumentDatas[i].date.ToString());
                            for (int j = 0; j < rtrvGtHistResp.instrumentDatas[i].data.Length; j++)
                            {
                                Console.WriteLine(sbmtGtHistReq.fields[j] + " : " + rtrvGtHistResp.
                                    instrumentDatas[i].data[j].value);
                            }
                        }
                        else
                        {
                            System.Console.WriteLine("\nError Code " + rtrvGtHistResp.instrumentDatas[i].code + ": incorrect macro. The Macro object is as follows:");
                            Console.WriteLine("Primary Qualifier - ");
                            Console.WriteLine("Primary Qualifier type:" + rtrvGtHistResp.instrumentDatas[i].
                                macro.primaryQualifier.primaryQualifierType);
                            Console.WriteLine("Primary Qualifier value:" + rtrvGtHistResp.instrumentDatas[i].
                                macro.primaryQualifier.primaryQualifierValue);
                            Console.WriteLine("Secondary Qualifier -");
                            for (int l = 0; l < rtrvGtHistResp.instrumentDatas[i].macro.secondaryQualifier.
                                Length; l++)
                            {
                                Console.WriteLine("Secondary Qualifier type :" + rtrvGtHistResp.instrumentDatas[i].
                                    macro.secondaryQualifier[l].secondaryQualifierType);
                                Console.WriteLine("Secondary Qualifier Value :" + rtrvGtHistResp.instrumentDatas[i]
                                    .macro.secondaryQualifier[l].secondaryQualifierValue);
                                Console.WriteLine("Secondary Qualifier Operator :" + rtrvGtHistResp.instrumentDatas[i]
                                    .macro.secondaryQualifier[l].secondaryQualifierOperator);
                            }
                        }
                    }
                }
                else if (rtrvGtHistResp.statusCode.code == PerSecurity.RequestError)
                {
                    Console.WriteLine("Error in the submitted request");
                }
            }

            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

        }

    }
}
