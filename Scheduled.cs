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
    * Scheduled - This class submits a getdata request by specifying instruments and fields and also setting  the
    * program flag(daily in this case), runtime and run date in the header. This class then submits a scheduled data
    * request and does retrieve scheduled request to verify, that the request sent via getdata request is added to the
    * daily jobs. This sample then cancels the request and submit and retrieve scheduled requests again to verify that the
    * request has been deleted from the list of daily jobs.
    */
    using System;
    using System.Collections.Generic;
    using System.Timers;
    using PerSecurity_Dotnet.PerSecurityWSDL;

    public class Scheduled
    {
        public void run(PerSecurityWS ps)
        {
            try
            {
                // Setting the instruments for request
                Instrument bbUniqueId = new Instrument();
                bbUniqueId.id = "EQ0086119600001000";
                bbUniqueId.yellowkeySpecified = false;
                bbUniqueId.type = InstrumentType.BB_UNIQUE;
                bbUniqueId.typeSpecified = true;

                Instrument ticker = new Instrument();
                ticker.id = "IBM US";
                ticker.yellowkeySpecified = true;
                ticker.yellowkey = MarketSector.Equity;
                ticker.typeSpecified = false;
                ticker.type = InstrumentType.TICKER;

                // Setting request header
                GetDataHeaders getDataHeaders = new GetDataHeaders();
                getDataHeaders.secmaster = true;
                getDataHeaders.secmasterSpecified = true;
                getDataHeaders.closingvalues = true;
                getDataHeaders.closingvaluesSpecified = true;
                getDataHeaders.programflagSpecified = true;
                getDataHeaders.programflag = ProgramFlag.daily;
                getDataHeaders.rundate = DateTime.Today.ToString("yyyyMMdd");
                getDataHeaders.time = "0010";
                getDataHeaders.derived = true;
                getDataHeaders.specialchar = SpecialChar.fraction;
                getDataHeaders.dateformat = DateFormat.ddmmyyyy;
                getDataHeaders.dateformatSpecified = true;

                Instruments instrs = new Instruments();
                instrs.instrument = new Instrument[] { bbUniqueId, ticker };

                // Submit getdata request
                SubmitGetDataRequest sbmtGtDrReq = new SubmitGetDataRequest();
                sbmtGtDrReq.headers = getDataHeaders;
                sbmtGtDrReq.fields = new string[] { "ID_BB_UNIQUE", "TICKER", "PX_LAST", "PX_ASK", "PX_BID", "VWAP_DT" };
                sbmtGtDrReq.instruments = instrs;

                Console.WriteLine("Submit getdata request");

                submitGetDataRequestRequest sbmtGtDrReqReq = new submitGetDataRequestRequest(sbmtGtDrReq);
                submitGetDataRequestResponse sbmtGetDtReqResp = ps.submitGetDataRequest(sbmtGtDrReqReq);
                SubmitGetDataResponse sbmtGetDtResp = sbmtGetDtReqResp.submitGetDataResponse;

                System.Console.WriteLine("Submit getdata response Id = " + sbmtGetDtResp.responseId + "\n");

                // Submit scheduled request for the getdata request sent above
                SubmitScheduledRequest sbmtSchReq = new SubmitScheduledRequest();
                Console.WriteLine("Submit scheduled request");

                submitScheduledRequestRequest sbmtSchReqReq = new submitScheduledRequestRequest(sbmtSchReq);
                submitScheduledRequestResponse sbmtSchReqResp = ps.submitScheduledRequest(sbmtSchReqReq);
                SubmitScheduledResponse sbmtSchResp = sbmtSchReqResp.submitScheduledResponse;

                Console.WriteLine("Submit Schedule request responseID : " + sbmtSchResp.responseId + "\n");

                // Submit retrieve scheduled request to display all the scheduled files and check to see
                // if the daily job requested was addeds
                Console.WriteLine("Retrieve scheduled request");
                RetrieveScheduledRequest rtrvSchReq = new RetrieveScheduledRequest();
                rtrvSchReq.responseId = sbmtSchResp.responseId;

                retrieveScheduledResponseRequest rtrvSchRespReq = new retrieveScheduledResponseRequest(rtrvSchReq);
                retrieveScheduledResponseResponse rtrvSchRespResp;

                RetrieveScheduledResponse rtrvSchResp;

                // Keep sending the request until the entire response is received
                do
                {
                    Console.WriteLine("Polling for scheduled request");
                    System.Threading.Thread.Sleep(PerSecurity.PollInterval);
                    rtrvSchRespResp = ps.retrieveScheduledResponse(rtrvSchRespReq);
                    rtrvSchResp = rtrvSchRespResp.retrieveScheduledResponse;
                }
                while (rtrvSchResp.statusCode.code == PerSecurity.DataNotAvailable);
                Console.WriteLine(rtrvSchResp.responseId);

                for (int i = 0; i < rtrvSchResp.fileDatas.Length; i++)
                {
                    if (rtrvSchResp.fileDatas[i].responseId == sbmtGetDtResp.responseId)
                    {
                        Console.WriteLine("Response ID requested by submit getdata request: " +
                            rtrvSchResp.fileDatas[i].responseId);
                    }
                    else
                    {
                        Console.WriteLine("Response ID: " + rtrvSchResp.fileDatas[i].responseId + "\n");
                    }

                    Console.WriteLine("Response Header for retrieve schedule request");

                    Dictionary<string, dynamic> headerRef = new Dictionary<string, dynamic>();
                    headerRef.Add("getdata", rtrvSchResp.fileDatas[i].headers.getdataHeaders);
                    headerRef.Add("gethistory", rtrvSchResp.fileDatas[i].headers.gethistoryHeaders);
                    headerRef.Add("getquotes", rtrvSchResp.fileDatas[i].headers.getquotesHeaders);
                    headerRef.Add("getallquotes", rtrvSchResp.fileDatas[i].headers.getallquotesHeaders);
                    headerRef.Add("getactions", rtrvSchResp.fileDatas[i].headers.getactionsHeaders);
                    headerRef.Add("getcompany", rtrvSchResp.fileDatas[i].headers.getcompanyHeaders);
                    headerRef.Add("getfundamentals", rtrvSchResp.fileDatas[i].headers.getfundamentalsHeaders);

                    foreach (KeyValuePair<string, dynamic> header in headerRef)
                    {
                        if (header.Value != null)
                        {
                            Console.WriteLine("ProgramName: " + header.Key);
                            Console.WriteLine("Date: " + header.Value.rundate +
                                " Time: " + header.Value.time + " Scheduled: " +
                                header.Value.programflag);

                            if (rtrvSchResp.fileDatas[i].fields != null)
                            {
                                Console.WriteLine("Fields");
                                for (int j = 0; j < rtrvSchResp.fileDatas[i].fields.Length; j++)
                                {
                                    Console.WriteLine(rtrvSchResp.fileDatas[i].fields[j]);
                                }
                            }

                            if (rtrvSchResp.fileDatas[i].instruments != null)
                            {
                                Console.WriteLine("Instruments");
                                for (int j = 0; j < rtrvSchResp.fileDatas[i].instruments.instrument.Length; j++)
                                {
                                    Console.WriteLine("ID: " + rtrvSchResp.fileDatas[i].instruments.instrument[j].id +
                                                    " Type: " + rtrvSchResp.fileDatas[i].instruments.instrument[j].type);
                                }
                            }
                        }
                    }
                }

                // Sending a request to cancel the request for the daily job

                CancelHeaders cancelHeaders = new CancelHeaders();
                cancelHeaders.programflag = ProgramFlag.daily;

                SubmitCancelRequest sbCancelReq = new SubmitCancelRequest();

                // use the responce id of the original multiday request submitted above.
                sbCancelReq.responseId = new string[] { sbmtGetDtResp.responseId };
                sbCancelReq.headers = cancelHeaders;

                Console.WriteLine("Submit Cancel request");

                submitCancelRequestRequest sbCancelReqReq = new submitCancelRequestRequest(sbCancelReq);
                submitCancelRequestResponse sbCancelReqResp = ps.submitCancelRequest(sbCancelReqReq);
                SubmitCancelResponse sbCancelResp = sbCancelReqResp.submitCancelResponse;

                if (sbCancelResp.statusCode.code == 0)
                {
                    Console.WriteLine("Submit Cancel request response ID: " + sbCancelResp.responseId);
                }

                // Checked the scheduled request to check if the daily job has been removed
                SubmitScheduledRequest sbmtSchReqCheck = new SubmitScheduledRequest();

                Console.WriteLine("Submit scheduled request");

                submitScheduledRequestRequest sbmtSchReqReqCheck = new submitScheduledRequestRequest(sbmtSchReqCheck);
                submitScheduledRequestResponse sbmtSchReqRespCheck = ps.submitScheduledRequest(sbmtSchReqReqCheck);
                SubmitScheduledResponse sbmtSchRespCheck = sbmtSchReqRespCheck.submitScheduledResponse;

                Console.WriteLine("Submit Schedule request responseID : " + sbmtSchRespCheck.responseId + "\n");

                Console.WriteLine("Retrieve scheduled request");
                RetrieveScheduledRequest schReqCheck = new RetrieveScheduledRequest();
                schReqCheck.responseId = sbmtSchRespCheck.responseId;

                retrieveScheduledResponseRequest schRespReqCheck = new retrieveScheduledResponseRequest(schReqCheck);
                retrieveScheduledResponseResponse schRespRespCheck;

                RetrieveScheduledResponse schRespCheck;
                int pollCnt = 0;
                do
                {
                    System.Threading.Thread.Sleep(PerSecurity.PollInterval);
                    schRespRespCheck = ps.retrieveScheduledResponse(schRespReqCheck);
                    schRespCheck = schRespRespCheck.retrieveScheduledResponse;
                    pollCnt++;
                }
                while (schRespCheck.fileDatas == null && pollCnt < 5); // Keep polling for few times

                Console.WriteLine("response ID for retrieveSchedule respone: " + schRespCheck.responseId);

                bool flag = false;
                string dispRespId = null;
                if (schRespCheck.fileDatas != null)
                {
                    for (int i = 0; i < schRespCheck.fileDatas.Length; i++)
                    {
                        if (schRespCheck.fileDatas[i].responseId == sbmtGetDtResp.responseId)
                        {
                            flag = true;
                            dispRespId = schRespCheck.fileDatas[i].responseId;
                            break;
                        }
                    }
                }
                if (flag)
                {
                    Console.WriteLine("The daily job with response ID: " + dispRespId +
                        " was not cancelled successfully");
                }
                else
                    Console.WriteLine("The daily job was cancelled successfully");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + e.StackTrace);
            }

        }
    }
}
