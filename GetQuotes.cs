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
    * GetQuotes - This class makes a get quotes request for one day period for a single security. It
    * also ensures that the data is being retrieved for a weekday. This is followed by retreive quotes request
    * to display data.
    */
    using System;
    using System.Collections.Generic;
    using System.Text;
    using PerSecurity_Dotnet.PerSecurityWSDL;

    internal class GetQuotes
    {
        public void run(PerSecurityWSDL.PerSecurityWS ps, ProgramFlag pf)
        {
            try
            {
                // Setting headers
                QuotesHeaders headers = new QuotesHeaders();
                DateRange dr = new DateRange();
                DateTime start = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
                if (new DateTime().DayOfWeek == DayOfWeek.Saturday)
                {
                    TimeSpan ts1 = new TimeSpan(1, 0, 0, 0);
                    start = start.Subtract(ts1);

                }
                else if (new DateTime().DayOfWeek == DayOfWeek.Sunday)
                {
                    start.AddDays(-2);

                }

                // 1 days worth of ticks
                Period dRange = new Period();
                dRange.start = start;
                dRange.end = start;
                dr.period = dRange;

                headers.daterange = dr;
                headers.programflag = pf;
                headers.programflagSpecified = true;

                Instrument ticker = new Instrument();
                ticker.id = "IBM US";
                ticker.yellowkeySpecified = true;
                ticker.typeSpecified = true;
                ticker.yellowkey = MarketSector.Equity;
                ticker.type = InstrumentType.TICKER;
                Instruments instrs = new Instruments();
                instrs.instrument = new Instrument[] { ticker };

                // Sending Request
                SubmitGetQuotesRequest smtGetQtsReq = new SubmitGetQuotesRequest();
                smtGetQtsReq.headers = headers;
                smtGetQtsReq.instruments = instrs;

                Console.WriteLine("Sending submit get quotes request");

                submitGetQuotesRequestRequest smtGetQtsReqReq = new submitGetQuotesRequestRequest(smtGetQtsReq);
                submitGetQuotesRequestResponse smtGetQtsReqResp = ps.submitGetQuotesRequest(smtGetQtsReqReq);
                SubmitGetQuotesResponse smtGetQtsResp = smtGetQtsReqResp.submitGetQuotesResponse; ;

                Console.WriteLine("Submit get quotes request status: " + smtGetQtsResp.statusCode.description +
                         " responseId: " + smtGetQtsResp.responseId);

                RetrieveGetQuotesRequest rtrvGetQtsReq = new RetrieveGetQuotesRequest();
                rtrvGetQtsReq.responseId = smtGetQtsResp.responseId;

                retrieveGetQuotesResponseRequest rtrvGetQtsRespReq = new retrieveGetQuotesResponseRequest(rtrvGetQtsReq);
                retrieveGetQuotesResponseResponse rtrvGetQtsRespResp;

                RetrieveGetQuotesResponse rtrvGetQtsResp;

                Console.WriteLine("Sending retrieve get quotes request");

                // Keep polling for response till the data is available
                do
                {
                    System.Threading.Thread.Sleep(PerSecurity.PollInterval);
                    rtrvGetQtsRespResp = ps.retrieveGetQuotesResponse(rtrvGetQtsRespReq);
                    rtrvGetQtsResp = rtrvGetQtsRespResp.retrieveGetQuotesResponse;
                } while (rtrvGetQtsResp.statusCode.code == PerSecurity.DataNotAvailable);

                // Display data
                if (rtrvGetQtsResp.statusCode.code == PerSecurity.Success)
                {
                    Console.WriteLine("Retrieve get quotes request successful.  Response ID:" + rtrvGetQtsResp.responseId);
                    for (int i = 0; i < rtrvGetQtsResp.instrumentDatas.Length; i++)
                    {
                        Console.WriteLine("Data for :"
                                + rtrvGetQtsResp.instrumentDatas[i].instrument.id + " "
                                + rtrvGetQtsResp.instrumentDatas[i].instrument.yellowkey);
                        for (int j = 0; j < rtrvGetQtsResp.instrumentDatas[i].quotes.Length; j++)
                        {
                            Console.WriteLine("price =  "
                                    + rtrvGetQtsResp.instrumentDatas[i].quotes[j].price
                                    + ", volume =  "
                                    + rtrvGetQtsResp.instrumentDatas[i].quotes[j].volume);
                        }

                    }
                }
                else if (rtrvGetQtsResp.statusCode.code == PerSecurity.RequestError)
                    Console.WriteLine("Error in submitted request");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
