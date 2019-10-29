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
    /*
    * GetCompanyFieldSet - This class makes get company request. This is followed by retrieve get company request to display data.
    */
    using System;
    using System.Collections.Generic;
    using System.Text;
    using PerSecurity_Dotnet.PerSecurityWSDL;

    internal class GetCompanyFieldSet
    {
        public void run(PerSecurityWSDL.PerSecurityWS ps)
        {
            // Setting request header information
            GetCompanyHeaders gtCompHdrs = new GetCompanyHeaders();
            gtCompHdrs.creditrisk = true;

            // Setting instruments
            Instrument ticker = new Instrument();
            ticker.id = "AAPL US";
            ticker.yellowkey = MarketSector.Equity;
            ticker.yellowkeySpecified = true;

            // Setting the get Company request parameter
            SubmitGetCompanyRequest sbmtGtCompReq = new SubmitGetCompanyRequest();
            sbmtGtCompReq.headers = gtCompHdrs;
            Instruments instrs = new Instruments();
            instrs.instrument = new Instrument[] { ticker };
            sbmtGtCompReq.instruments = instrs;

            // Setting fields for the request
            FieldSet fieldSet = new FieldSet();
            fieldSet.fieldmacro = FieldMacro.BO_CREDIT_RISK_COMPANY;
            sbmtGtCompReq.fieldset = fieldSet;

            try
            {
                Console.WriteLine("Submit Get Company Request");

                submitGetCompanyRequestRequest sbmtGtCompReqReq = new submitGetCompanyRequestRequest(sbmtGtCompReq);
                submitGetCompanyRequestResponse sbmtGtCompReqResp = ps.submitGetCompanyRequest(sbmtGtCompReqReq);
                SubmitGetCompanyResponse sbmtGtCompResp = sbmtGtCompReqResp.submitGetCompanyResponse;

                System.Console.WriteLine("status " + sbmtGtCompResp.statusCode.description);
                System.Console.WriteLine("Submit Get Company request -  response ID = " + sbmtGtCompResp.responseId);

                // retrieve get company request. The response ID sent for the request is the response ID
                // received from SubmitGetCompanyRequest()
                Console.WriteLine("Retrieve Company request");

                RetrieveGetCompanyRequest rtvGtCompReq = new RetrieveGetCompanyRequest();
                rtvGtCompReq.responseId = sbmtGtCompResp.responseId;

                retrieveGetCompanyResponseRequest rtvGtCompRespReq = new retrieveGetCompanyResponseRequest(rtvGtCompReq);
                retrieveGetCompanyResponseResponse rtrvGtCompRespResp;

                RetrieveGetCompanyResponse rtrvGtCompResp;

                // Keep polling until data becomes available
                do
                {
                    System.Threading.Thread.Sleep(PerSecurity.PollInterval);
                    rtrvGtCompRespResp = ps.retrieveGetCompanyResponse(rtvGtCompRespReq);
                    rtrvGtCompResp = rtrvGtCompRespResp.retrieveGetCompanyResponse;
                }
                while (rtrvGtCompResp.statusCode.code == PerSecurity.DataNotAvailable);

                if (rtrvGtCompResp.statusCode.code == PerSecurity.Success)
                {
                    // Displaying the rtrvGtCompResp
                    for (int i = 0; i < rtrvGtCompResp.instrumentDatas.Length; i++)
                    {
                        Console.WriteLine("Data for :" + rtrvGtCompResp.instrumentDatas[i].instrument.id +
                            "  " + rtrvGtCompResp.instrumentDatas[i].instrument.yellowkey);
                        for (int j = 0; j < rtrvGtCompResp.instrumentDatas[i].data.Length; j++)
                        {
                            if (rtrvGtCompResp.instrumentDatas[i].data[j].isArray == true)
                            {
                                // In case this is a bulk field request
                                for (int k = 0; k < rtrvGtCompResp.instrumentDatas[i].data[j].bulkarray.Length; k++)
                                {
                                    Console.WriteLine("-------------------------_");
                                    for (int l = 0; l < rtrvGtCompResp.instrumentDatas[i].data[j].
                                        bulkarray[k].data.Length; l++)
                                        Console.WriteLine(rtrvGtCompResp.instrumentDatas[i].data[j].bulkarray[k].data[l].value);
                                }
                            }
                            else
                                Console.WriteLine("	" + rtrvGtCompResp.fields[j] + " : " + rtrvGtCompResp.instrumentDatas[i].data[j].value);
                        }
                    }
                }
                else if (rtrvGtCompResp.statusCode.code == PerSecurity.RequestError)
                    Console.WriteLine("Error in the submitted request");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
