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
    * GetDataOverrides - This class submits a getdata request by specifying the instruments and fields for the request parameters.The example
    * also specifies overrides for the requested fields, per instrument. This is followed by retrieve getdata response to get the data for the fields
    */
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using PerSecurity_Dotnet.PerSecurityWSDL;

    internal class GetDataOverrides
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

            // Setting overrides
            Override[] ovride = new Override[4];
            ovride[0] = new Override();
            ovride[0].field = "VWAP_START_DT";
            DateTime start = DateTime.Today;

            // Specifying VWAP_START_DT in yyyymmdd format
            if (start.Month < 10)
            {
                ovride[0].value = start.Year.ToString() + "0" + start.Month.ToString() + start.Day.ToString();
            }
            else
            {
                ovride[0].value = start.Year.ToString() + start.Month.ToString() + start.Day.ToString();
            }
            ovride[1] = new Override();
            ovride[1].field = "VWAP_END_DT";
            DateTime end = DateTime.Today;

            // Specifying VWAP_END_DT in yyyymmdd format
            if (start.Month < 10)
            {
                ovride[1].value = end.Year.ToString() + "0" + end.Month.ToString() + end.Day.ToString();
            }
            else
            {
                ovride[1].value = end.Year.ToString() + end.Month.ToString() + end.Day.ToString();
            }
            ovride[2] = new Override();
            ovride[2].field = "VWAP_START_TIME";
            ovride[2].value = "10:00";
            ovride[3] = new Override();
            ovride[3].field = "VWAP_END_TIME";
            ovride[3].value = "11:00";

            // Setting the instruments for request
            Instrument bbUniqueId = new Instrument();
            bbUniqueId.id = "EQ0086119600001000";
            bbUniqueId.yellowkeySpecified = false;
            bbUniqueId.type = InstrumentType.BB_UNIQUE;
            bbUniqueId.typeSpecified = true;
            bbUniqueId.overrides = ovride;

            Instrument ticker = new Instrument();
            ticker.id = "IBM US";
            ticker.yellowkeySpecified = true;
            ticker.yellowkey = MarketSector.Equity;
            ticker.typeSpecified = false;
            ticker.type = InstrumentType.TICKER;
            ticker.overrides = ovride;

            Instruments instrs = new Instruments();
            instrs.instrument = new Instrument[] { ticker, bbUniqueId };

            // Setting the getdata request parameter
            SubmitGetDataRequest sbmtGtDtReq = new SubmitGetDataRequest();
            sbmtGtDtReq.headers = getDataHeaders;
            sbmtGtDtReq.fields = new string[] { "EQY_WEIGHTED_AVG_PX" };
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
