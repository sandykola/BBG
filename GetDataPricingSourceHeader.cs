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
    * GetDataPricingSourceHeader - This class makes a getdata request with Exclusive pricing source
    * header option set to true. This is followed by a retrieve data request to display data.
    */
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Timers;
    using PerSecurity_Dotnet.PerSecurityWSDL;

    internal class GetDataPricingSourceHeader
    {
        public void run(PerSecurityWSDL.PerSecurityWS ps)
        {
            // Setting request header information
            GetDataHeaders getDataHeaders = new GetDataHeaders();
            getDataHeaders.secmaster = true;
            getDataHeaders.secmasterSpecified = true;
            getDataHeaders.closingvalues = true;
            getDataHeaders.closingvaluesSpecified = true;
            getDataHeaders.derived = true;
            getDataHeaders.derivedSpecified = true;
            getDataHeaders.exclusive_pricing_srcSpecified = true;
            getDataHeaders.exclusive_pricing_src = true;

            // Setting Instrument information
            Instrument bbUniqueId = new Instrument();
            bbUniqueId.id = "EQ0086119600001000";
            bbUniqueId.type = InstrumentType.BB_UNIQUE;
            bbUniqueId.typeSpecified = true;

            Instrument ticker = new Instrument();
            ticker.id = "IBM";
            ticker.yellowkey = MarketSector.Equity;
            ticker.yellowkeySpecified = true;
            ticker.type = InstrumentType.TICKER;
            ticker.typeSpecified = true;

            // Setting the getdata request parameter
            Instruments instrs = new Instruments();
            instrs.instrument = new Instrument[] { ticker, bbUniqueId };

            SubmitGetDataRequest sbmtGtDtreq = new SubmitGetDataRequest();
            sbmtGtDtreq.headers = getDataHeaders;
            sbmtGtDtreq.instruments = new Instruments();
            sbmtGtDtreq.instruments = instrs;
            sbmtGtDtreq.fields = new string[] { "ID_BB_UNIQUE", "TICKER", "PX_LAST", "PX_ASK", "PX_BID", "VWAP_DT" };

            try
            {
                Console.WriteLine("Submit getdata Request");

                submitGetDataRequestRequest sbmtGtDtReqReq = new submitGetDataRequestRequest(sbmtGtDtreq);
                submitGetDataRequestResponse sbmtGtDtReqResp = ps.submitGetDataRequest(sbmtGtDtReqReq);
                SubmitGetDataResponse sbmtGtDtResp = sbmtGtDtReqResp.submitGetDataResponse;

                System.Console.WriteLine("status " + sbmtGtDtResp.statusCode.description);
                System.Console.WriteLine("Submit getdata request -  response ID = " + sbmtGtDtResp.responseId);

                // retrieve getdata request. The response ID sent for the request is the response ID
                // received from SubmitGetDataRequest()
                Console.WriteLine("Retrieve getdata request");
                RetrieveGetDataRequest rtrvGtDrReq = new RetrieveGetDataRequest();
                rtrvGtDrReq.responseId = sbmtGtDtResp.responseId;

                retrieveGetDataResponseRequest rtrvGtDrRespReq = new retrieveGetDataResponseRequest(rtrvGtDrReq);
                retrieveGetDataResponseResponse rtrvGtDrRespResp = new retrieveGetDataResponseResponse();

                RetrieveGetDataResponse rtrvGtDrResp;

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
                    // Displaying the RetrieveGetDataResponse
                    for (int i = 0; i < rtrvGtDrResp.instrumentDatas.Length; i++)
                    {
                        Console.WriteLine("Data for :" + rtrvGtDrResp.instrumentDatas[i].instrument.id +
                            "  " + rtrvGtDrResp.instrumentDatas[i].instrument.yellowkey);
                        for (int j = 0; j < rtrvGtDrResp.instrumentDatas[i].data.Length; j++)
                        {
                            if (rtrvGtDrResp.instrumentDatas[i].data[j].isArray == true)
                            {
                                // In case this is a bulk field request
                                for (int k = 0; k < rtrvGtDrResp.instrumentDatas[i].data[j].bulkarray.Length; k++)
                                {
                                    Console.WriteLine("-------------------------");
                                    for (int l = 0; l < rtrvGtDrResp.instrumentDatas[i].data[j].
                                        bulkarray[k].data.Length; l++)
                                        Console.WriteLine(rtrvGtDrResp.instrumentDatas[i].data[j].bulkarray[k].data[l].value);
                                }
                            }
                            else
                            {
                                Console.WriteLine("	" + sbmtGtDtreq.fields[j] + " : " +
                                    rtrvGtDrResp.instrumentDatas[i].data[j].value);
                            }
                        }
                    }
                }
                else if (rtrvGtDrResp.statusCode.code == PerSecurity.RequestError)
                    Console.WriteLine("Error in the submitted request");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + "   " + e.StackTrace);
            }
        }
    }
}
