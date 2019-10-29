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
    * GetDataRequest - This class submits a getdata request by specifying the instruments and fields for the
    * request parameters. This is followed by retrieve getdata request, to get the values for the fields.
    */
    using System;
    using System.Timers;
    using PerSecurity_Dotnet.PerSecurityWSDL;

    internal class GetData
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

            // Setting Instrument information
            Instrument bbUniqueId1 = new Instrument();
            bbUniqueId1.id = "EQ0010174300001000";
            bbUniqueId1.type = InstrumentType.BB_UNIQUE;
            bbUniqueId1.typeSpecified = true;

            Instrument ticker = new Instrument();
            ticker.id = "IBM";
            ticker.yellowkey = MarketSector.Equity;
            ticker.yellowkeySpecified = true;
            ticker.type = InstrumentType.TICKER;
            ticker.typeSpecified = true;

            Instrument bbUniqueId2 = new Instrument();
            bbUniqueId2.id = "US0231351067";
            bbUniqueId2.type = InstrumentType.ISIN;
            bbUniqueId2.typeSpecified = true;

            // group all instrument into a single instance of type Instruments.
            Instruments instrs = new Instruments();
            instrs.instrument = new Instrument[] { bbUniqueId1, ticker, bbUniqueId2 };
            System.Collections.Generic.List<Instrument> instrsList = new System.Collections.Generic.List<Instrument>();

            // Setting the getdata request parameter
            SubmitGetDataRequest sbmtGtDtReq = new SubmitGetDataRequest();
            sbmtGtDtReq.headers = getDataHeaders;
            sbmtGtDtReq.fields = new string[] { "ID_BB_UNIQUE", "PX_LAST" };

            // sbmtGtDtReq.instruments = new Instruments();
            sbmtGtDtReq.instruments = instrs;

            try
            {
                Console.WriteLine("Submit getdata Request");

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
                                    {
                                        Console.WriteLine(rtrvGtDrResp.
                                            instrumentDatas[i].data[j].bulkarray[k].data[l].value);
                                    }
                                }
                            }
                            else
                            {
                                Console.WriteLine("	" + rtrvGtDrResp.fields[j] + " : " + rtrvGtDrResp.
                                    instrumentDatas[i].data[j].value);
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
