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
     * GetAllQuotes - This class makes a get all quotes request for one minute interval. It ensures that the request is being
     * made for a weekday. This is followed by retrieve get all quotes request to display data.
     */
    using System;
    using System.Collections.Generic;
    using System.Text;
    using PerSecurity_Dotnet.PerSecurityWSDL;

    internal class GetAllQuotes
    {
        public void run(PerSecurityWSDL.PerSecurityWS ps, ProgramFlag pf)
        {
            try
            {
                // Setting header information
                QuotesHeaders headers = new QuotesHeaders();
                DateTime start = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 10, 10, 0);
                DateTime end = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 10, 11, 0);
                if (default(DateTime).DayOfWeek == DayOfWeek.Saturday)
                {
                    TimeSpan ts1 = new TimeSpan(1, 0, 0, 0);
                    start = start.Subtract(ts1);
                    end = end.Subtract(ts1);
                }
                else if (default(DateTime).DayOfWeek == DayOfWeek.Sunday)
                {
                    start.AddDays(-2);
                    end.AddDays(-2);
                }

                DateTimeRange dtr = new DateTimeRange();
                dtr.startDateTime = start;
                dtr.endDateTime = end;
                dtr.region = "NY";
                headers.datetimerange = dtr;
                headers.programflag = pf;
                headers.programflagSpecified = true;

                // Setting instruments
                Instrument ticker = new Instrument();
                ticker.id = "IBM US";
                ticker.yellowkeySpecified = true;
                ticker.typeSpecified = true;
                ticker.yellowkey = MarketSector.Equity;
                ticker.type = InstrumentType.TICKER;
                Instruments instrs = new Instruments();
                instrs.instrument = new Instrument[] { ticker };

                SubmitGetAllQuotesRequest smtGetAllQtsReq = new SubmitGetAllQuotesRequest();
                smtGetAllQtsReq.headers = headers;
                smtGetAllQtsReq.instruments = instrs;

                Console.WriteLine("Sending submit get all quotes request");

                submitGetAllQuotesRequestRequest smtGetAllQtsReqReq = new submitGetAllQuotesRequestRequest(smtGetAllQtsReq);
                submitGetAllQuotesRequestResponse subGtAllQtsReqResp = ps.submitGetAllQuotesRequest(smtGetAllQtsReqReq);
                SubmitGetAllQuotesResponse subGtAllQtsResp = subGtAllQtsReqResp.submitGetAllQuotesResponse;

                string responseId = subGtAllQtsResp.responseId;

                Console.WriteLine("Submit get all quotes request status: " + subGtAllQtsResp.statusCode.description +
                     " responseId: " + subGtAllQtsResp.responseId);

                RetrieveGetAllQuotesRequest rtrvGetAllQtsReq = new RetrieveGetAllQuotesRequest();
                rtrvGetAllQtsReq.responseId = responseId;

                retrieveGetAllQuotesResponseRequest rtrvGetAllQtsRespReq = new retrieveGetAllQuotesResponseRequest(rtrvGetAllQtsReq);
                retrieveGetAllQuotesResponseResponse rtrvGetAllQtsRespResp;

                RetrieveGetAllQuotesResponse rtrvGetAllQtsResp;

                Console.WriteLine("Sending retrieve get all quotes request");

                // Keep polling for response till the data is available
                do
                {
                    System.Threading.Thread.Sleep(PerSecurity.PollInterval);
                    rtrvGetAllQtsRespResp = ps.retrieveGetAllQuotesResponse(rtrvGetAllQtsRespReq);
                    rtrvGetAllQtsResp = rtrvGetAllQtsRespResp.retrieveGetAllQuotesResponse;
                }
                while (rtrvGetAllQtsResp.statusCode.code == PerSecurity.DataNotAvailable);

                // Display data
                if (rtrvGetAllQtsResp.statusCode.code == PerSecurity.Success)
                {
                    Console.WriteLine("Retrieve get all quotes request successful.  Response ID:" + rtrvGetAllQtsResp.responseId);
                    for (int i = 0; i < rtrvGetAllQtsResp.instrumentDatas.Length; i++)
                    {
                        Console.WriteLine("Data for :"
                                + rtrvGetAllQtsResp.instrumentDatas[i].instrument.id + " "
                                + rtrvGetAllQtsResp.instrumentDatas[i].instrument.yellowkey);
                        for (int j = 0; j < rtrvGetAllQtsResp.instrumentDatas[i].quotes.Length; j++)
                        {
                            for (int k = 0; k < rtrvGetAllQtsResp.instrumentDatas[i].quotes[j].matchedQuote.Length; k++)
                            {
                                Console.WriteLine(" type = "
                                        + rtrvGetAllQtsResp.instrumentDatas[i].quotes[j].matchedQuote[k].type +
                                        ", price =  "
                                        + rtrvGetAllQtsResp.instrumentDatas[i].quotes[j].matchedQuote[k].price
                                        + ", volume =  "
                                        + rtrvGetAllQtsResp.instrumentDatas[i].quotes[j].matchedQuote[k].volume
                                                );
                            }
                        }
                    }
                }
                else if (rtrvGetAllQtsResp.statusCode.code == PerSecurity.RequestError)
                    Console.WriteLine("Error in submitted request");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
