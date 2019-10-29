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
    * GetHistory - This class submits a gethistory request by specifying the instruments and fields for the request
    * parameters. This is followed by retrieve GetHistory request, to get the values for the fields
    */
    using System;
    using System.Collections.Generic;
    using System.Text;
    using PerSecurity_Dotnet.PerSecurityWSDL;

    internal class GetHistory
    {
        public void run(PerSecurity_Dotnet.PerSecurityWSDL.PerSecurityWS ps)
        {
            // Setting request headers
            GetHistoryHeaders getHistHeaders = new GetHistoryHeaders();
            DateRange dtRange = new DateRange();
            dtRange.period = new Period();
            dtRange.period.start = DateTime.Today.Subtract(TimeSpan.FromDays(7));
            dtRange.period.end = DateTime.Today;
            getHistHeaders.daterange = dtRange;
            getHistHeaders.version = PerSecurity_Dotnet.PerSecurityWSDL.Version.@new;

            // Setting instruments
            Instrument ticker = new Instrument();
            ticker = new Instrument();
            ticker.id = "IBM US";
            ticker.yellowkey = MarketSector.Equity;
            ticker.yellowkeySpecified = true;

            Instrument bbUniqueId = new Instrument();
            bbUniqueId.id = "EQ0086119600001000";
            bbUniqueId.yellowkeySpecified = true;
            bbUniqueId.yellowkey = MarketSector.Equity;
            bbUniqueId.type = InstrumentType.BB_UNIQUE;
            bbUniqueId.typeSpecified = true;

            Instrument[] instr = new Instrument[] { ticker, bbUniqueId };
            Instruments instrs = new Instruments();
            instrs.instrument = instr;

            // Setting GetHistory request parameters
            string[] fields = new string[] { "PX_LAST", "PX_HIGH", "PX_LOW" };
            SubmitGetHistoryRequest sbmtGtHistReq = new SubmitGetHistoryRequest();
            sbmtGtHistReq.headers = getHistHeaders;
            sbmtGtHistReq.instruments = instrs;
            sbmtGtHistReq.fields = fields;
            submitGetHistoryRequestRequest sbmtGtHistReqReq = new submitGetHistoryRequestRequest(sbmtGtHistReq);

            try
            {
                Console.WriteLine("Submit gethistory request");
                submitGetHistoryRequestResponse sbmtGtHistReqResp = ps.submitGetHistoryRequest(sbmtGtHistReqReq);
                SubmitGetHistoryResponse sbmtGtHistResp = sbmtGtHistReqResp.submitGetHistoryResponse;

                System.Console.WriteLine("status " + sbmtGtHistResp.statusCode.description);
                System.Console.WriteLine("Submit getdata request -  response ID = " + sbmtGtHistResp.responseId);

                Console.WriteLine("Retrieve getdata request");
                RetrieveGetHistoryRequest rtrvGtHistReq = new RetrieveGetHistoryRequest();
                rtrvGtHistReq.responseId = sbmtGtHistResp.responseId;

                retrieveGetHistoryResponseRequest rtrvGtHistRespReq = new retrieveGetHistoryResponseRequest(rtrvGtHistReq);
                retrieveGetHistoryResponseResponse rtrvGtHistRespResp = new retrieveGetHistoryResponseResponse();

                RetrieveGetHistoryResponse rtrvGtHistResp;

                // Keep polling until data becomes available
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
                        System.Console.WriteLine("Data for :" + rtrvGtHistResp.instrumentDatas[i].instrument.id +
                            "  " + rtrvGtHistResp.instrumentDatas[i].instrument.yellowkey);
                        Console.WriteLine(rtrvGtHistResp.instrumentDatas[i].date.ToString());
                        for (int j = 0; j < rtrvGtHistResp.instrumentDatas[i].data.Length; j++)
                        {
                            Console.WriteLine(sbmtGtHistReq.fields[j] + " : " + rtrvGtHistResp.instrumentDatas[i].data[j].value);
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
