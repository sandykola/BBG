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
    * GetCompany - This class makes get company request. This is followed by retrieve get company request to display data.
    */
    using System;
    using System.Timers;
    using PerSecurity_Dotnet.PerSecurityWSDL;

    public class GetCompany
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
            sbmtGtCompReq.fields = new string[] { "ID_BB_COMPANY", "ID_BB_ULTIMATE_PARENT_CO_NAME" };

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

                RetrieveGetCompanyRequest rtvGrCompReq = new RetrieveGetCompanyRequest();
                rtvGrCompReq.responseId = sbmtGtCompResp.responseId;

                retrieveGetCompanyResponseRequest rtvGrCompRespReq = new retrieveGetCompanyResponseRequest(rtvGrCompReq);
                retrieveGetCompanyResponseResponse rtrvGtCompRespResp;

                RetrieveGetCompanyResponse rtrvGtCompResp;

                // Keep polling until data becomes available
                do
                {
                    System.Threading.Thread.Sleep(PerSecurity.PollInterval);
                    rtrvGtCompRespResp = ps.retrieveGetCompanyResponse(rtvGrCompRespReq);
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