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
    * GetHistoryPricingSourceHeader - This class makes gethistory request with display pricing source header option
    * enabled to true. This is followed by retrieve history response to display data.
    */
    using System;
    using System.Collections.Generic;
    using System.Text;
    using PerSecurity_Dotnet.PerSecurityWSDL;

    internal class GetHistoryPricingSourceHeader
    {
        public void run(PerSecurityWSDL.PerSecurityWS ps)
        {
            try
            {
                // Setting headers
                GetHistoryHeaders getHistHeaders = new GetHistoryHeaders();
                getHistHeaders.display_pricing_srcSpecified = true;
                getHistHeaders.display_pricing_src = true;

                DateRange dtRange = new DateRange();
                dtRange.period = new Period();
                dtRange.period.start = DateTime.Today.Subtract(TimeSpan.FromDays(7));
                dtRange.period.end = DateTime.Today;
                getHistHeaders.daterange = dtRange;

                Instrument ticker = new Instrument();
                ticker.id = "IBM US";
                ticker.typeSpecified = true;
                ticker.yellowkeySpecified = true;
                ticker.type = InstrumentType.TICKER;
                ticker.yellowkey = MarketSector.Equity;
                Instrument[] instr = new Instrument[] { ticker };
                Instruments instrs = new Instruments();
                instrs.instrument = instr;

                string[] fields = new string[] { "PX_LAST" };

                // Sending Request
                SubmitGetHistoryRequest sbmtGtHistReq = new SubmitGetHistoryRequest();
                sbmtGtHistReq.headers = getHistHeaders;
                sbmtGtHistReq.instruments = instrs;
                sbmtGtHistReq.fields = fields;

                Console.WriteLine("Submit gethistory request");

                submitGetHistoryRequestRequest sbmtGtHistReqReq = new submitGetHistoryRequestRequest(sbmtGtHistReq);
                submitGetHistoryRequestResponse sbmtGtHistReqResp = ps.submitGetHistoryRequest(sbmtGtHistReqReq);
                SubmitGetHistoryResponse sbmtGtHistResp = sbmtGtHistReqResp.submitGetHistoryResponse;

                Console.WriteLine("Submit gethistory request - Status: "
                    + sbmtGtHistResp.statusCode.description + " response ID = "
                    + sbmtGtHistResp.responseId);
                Console.WriteLine("Sending retrieve gethistory request");

                RetrieveGetHistoryRequest rtrvGtHistReq = new RetrieveGetHistoryRequest();
                rtrvGtHistReq.responseId = sbmtGtHistResp.responseId;

                retrieveGetHistoryResponseRequest rtrvGtHistRespReq = new retrieveGetHistoryResponseRequest(rtrvGtHistReq);
                retrieveGetHistoryResponseResponse rtrvGtHistRespResp;

                RetrieveGetHistoryResponse rtrvGtHistResp;

                // Keep polling for response till the data is available
                do
                {
                    System.Threading.Thread.Sleep(PerSecurity.PollInterval);
                    rtrvGtHistRespResp = ps.retrieveGetHistoryResponse(rtrvGtHistRespReq);
                    rtrvGtHistResp = rtrvGtHistRespResp.retrieveGetHistoryResponse;
                } while (rtrvGtHistResp.statusCode.code == PerSecurity.DataNotAvailable);

                // Displaying data
                if (rtrvGtHistResp.statusCode.code == PerSecurity.Success)
                {
                    Console.WriteLine("Retrieve gethistory request successful Response ID: " + rtrvGtHistResp.responseId);
                    for (int i = 0; i < rtrvGtHistResp.instrumentDatas.Length; i++)
                    {
                        Console.WriteLine("Data for :"
                                + rtrvGtHistResp.instrumentDatas[i].instrument.id
                                + " "
                                + rtrvGtHistResp.instrumentDatas[i].instrument.yellowkey);
                        Console.WriteLine("Time: " + rtrvGtHistResp.instrumentDatas[i].date.Hour.ToString() + ":" + rtrvGtHistResp.instrumentDatas[i].date.Minute.ToString() + ":" + rtrvGtHistResp.instrumentDatas[i].date.Second.ToString());
                        Console.WriteLine("Pricing Source: " + rtrvGtHistResp.instrumentDatas[i].pricingSource);
                        for (int j = 0; j < rtrvGtHistResp.instrumentDatas[i].data.Length; j++)
                        {
                            Console.WriteLine(fields[j]
                                    + " : "
                                    + rtrvGtHistResp.instrumentDatas[i].data[j].value);
                        }
                    }
                }
                else if (rtrvGtHistResp.statusCode.code == PerSecurity.RequestError)
                    Console.WriteLine(" Error in the submitted request");

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
