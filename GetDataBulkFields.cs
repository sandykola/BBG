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
    * GetDataBulk - This class submits a getdata request by specifying instruments and bulk fields.
    * This is followed by retrieve getdata request, to get the values for the fields.
    */
    using System;
    using System.Timers;
    using PerSecurity_Dotnet.PerSecurityWSDL;

    public class GetDataBulkFields
    {
        public void run(PerSecurity_Dotnet.PerSecurityWSDL.PerSecurityWS ps)
        {
            // Set request header
            GetDataHeaders getDataHeaders = new GetDataHeaders();
            getDataHeaders.secmaster = true;
            getDataHeaders.secmasterSpecified = true;

            // Defining the instruments
            Instrument ticker = new Instrument();
            ticker.id = "IBM US";
            ticker.yellowkeySpecified = true;
            ticker.yellowkey = MarketSector.Equity;
            ticker.typeSpecified = false;
            ticker.type = InstrumentType.TICKER;

            Console.WriteLine("Submit getdata request");
            SubmitGetDataRequest sbmtGtDtreq = new SubmitGetDataRequest();
            sbmtGtDtreq.headers = getDataHeaders;

            sbmtGtDtreq.fields = new string[] { "OPT_CHAIN" };
            Instruments instrs = new Instruments();
            instrs.instrument = new Instrument[] { ticker };
            sbmtGtDtreq.instruments = instrs;

            submitGetDataRequestRequest sbmtGtDtreqReq = new submitGetDataRequestRequest(sbmtGtDtreq);
            submitGetDataRequestResponse sbmtGtDtReqResp;

            try
            {
                sbmtGtDtReqResp = ps.submitGetDataRequest(sbmtGtDtreqReq);
                SubmitGetDataResponse sbmtGtDtResp = sbmtGtDtReqResp.submitGetDataResponse;

                System.Console.WriteLine("Request ID = " + sbmtGtDtResp.requestId + " " + sbmtGtDtResp.responseId);
                System.Console.WriteLine("status of getdata request :  " + sbmtGtDtResp.statusCode.description);

                Console.WriteLine("Retrieve getdata request");

                RetrieveGetDataRequest rtrvGtDrReq = new RetrieveGetDataRequest();
                rtrvGtDrReq.responseId = sbmtGtDtResp.responseId;

                retrieveGetDataResponseRequest rtrvGtDrRespReq = new retrieveGetDataResponseRequest(rtrvGtDrReq);
                retrieveGetDataResponseResponse rtrvGtDrRespResp = new retrieveGetDataResponseResponse();

                RetrieveGetDataResponse rtrvGtDrResp;

                do
                {
                    System.Threading.Thread.Sleep(PerSecurity.PollInterval);

                    rtrvGtDrRespResp = ps.retrieveGetDataResponse(rtrvGtDrRespReq);
                    rtrvGtDrResp = rtrvGtDrRespResp.retrieveGetDataResponse;
                }
                while (rtrvGtDrResp.statusCode.code == PerSecurity.DataNotAvailable);
                if (rtrvGtDrResp.statusCode.code == PerSecurity.Success)
                {
                    Console.WriteLine("Response ID " + rtrvGtDrResp.responseId);
                    for (int i = 0; i < rtrvGtDrResp.instrumentDatas.Length; i++)
                    {
                        System.Console.WriteLine("Data for :" + rtrvGtDrResp.instrumentDatas[i].instrument.id +
                            "  " + rtrvGtDrResp.instrumentDatas[i].instrument.yellowkey);
                        for (int j = 0; j < rtrvGtDrResp.instrumentDatas[i].data.Length; j++)
                        {
                            if (rtrvGtDrResp.instrumentDatas[i].data[j].isArray)
                            {
                                for (int k = 0; k < rtrvGtDrResp.instrumentDatas[i].data[j].bulkarray.Length; k++)
                                {
                                    Console.WriteLine("-------------------------");
                                    for (int l = 0; l < rtrvGtDrResp.instrumentDatas[i].data[j].
                                        bulkarray[k].data.Length; l++)
                                    {
                                        Console.WriteLine(rtrvGtDrResp.instrumentDatas[i].
                                            data[j].bulkarray[k].data[l].value);
                                    }
                                }
                            }
                            else
                            {
                                System.Console.WriteLine("	" + sbmtGtDtreq.fields[j] + " : " +
                                    rtrvGtDrResp.instrumentDatas[i].data[j].value);
                            }
                        }
                    }
                }
                else if (rtrvGtDrResp.statusCode.code == PerSecurity.RequestError)
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
