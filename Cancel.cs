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
     * Cancel - This class sends 2 different getdata request. This is followed by sending cancel request
     * which takes the response id of the two requests. The class then makes a retrieve cancel request.
     */
    using System;
    using System.Collections.Generic;
    using System.Text;
    using PerSecurity_Dotnet.PerSecurityWSDL;

    internal class Cancel
    {
        public void run(PerSecurityWSDL.PerSecurityWS ps)
        {
            try
            {
                // Setting headers
                GetDataHeaders getDataHeaders = new GetDataHeaders();
                getDataHeaders.programflagSpecified = true;
                getDataHeaders.programflag = ProgramFlag.daily;
                getDataHeaders.rundate = DateTime.Today.ToString("yyyyMMdd");
                getDataHeaders.time = "2200";
                getDataHeaders.specialchar = SpecialChar.fraction;
                getDataHeaders.dateformat = DateFormat.ddmmyyyy;
                getDataHeaders.dateformatSpecified = true;
                getDataHeaders.secmaster = true;
                getDataHeaders.derived = true;

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
                Instruments instrs1 = new Instruments();
                instrs1.instrument = new Instrument[] { ticker };
                Instruments instrs2 = new Instruments();
                instrs2.instrument = new Instrument[] { bbUniqueId };

                SubmitGetDataRequest sbmtGtDtReq1 = new SubmitGetDataRequest();
                sbmtGtDtReq1.headers = getDataHeaders;
                sbmtGtDtReq1.fields = new string[] { "PX_LAST" };
                sbmtGtDtReq1.instruments = instrs1;

                SubmitGetDataRequest sbmtGtDtReq2 = new SubmitGetDataRequest();
                sbmtGtDtReq2.headers = getDataHeaders;
                sbmtGtDtReq2.fields = new string[] { "PX_LAST" };
                sbmtGtDtReq2.instruments = instrs2;

                submitGetDataRequestRequest sbmtGtDtReqReq1 = new submitGetDataRequestRequest(sbmtGtDtReq1);
                submitGetDataRequestRequest sbmtGtDtReqReq2 = new submitGetDataRequestRequest(sbmtGtDtReq2);

                submitGetDataRequestResponse sbmtGtDtReqResp1 = new submitGetDataRequestResponse();
                submitGetDataRequestResponse sbmtGtDtReqResp2 = new submitGetDataRequestResponse();

                sbmtGtDtReqResp1 = ps.submitGetDataRequest(sbmtGtDtReqReq1);
                sbmtGtDtReqResp2 = ps.submitGetDataRequest(sbmtGtDtReqReq1);
                SubmitGetDataResponse sbmtGtDtResp1 = sbmtGtDtReqResp1.submitGetDataResponse;
                SubmitGetDataResponse sbmtGtDtResp2 = sbmtGtDtReqResp2.submitGetDataResponse;

                Console.WriteLine("Scheduled Req 1 GetData --> " + sbmtGtDtResp1.responseId);
                Console.WriteLine("Scheduled Req 2 GetData --> " + sbmtGtDtResp2.responseId);

                System.Threading.Thread.Sleep(30000);

                string[] responseIds = { sbmtGtDtResp1.responseId, sbmtGtDtResp2.responseId, "dummy_id.out" };

                CancelHeaders cancelHeaders = new CancelHeaders();
                cancelHeaders.programflag = ProgramFlag.monthly;

                SubmitCancelRequest submitCancelReq = new SubmitCancelRequest();
                submitCancelReq.headers = cancelHeaders;
                submitCancelReq.responseId = responseIds;

                submitCancelRequestRequest submitCancelReqReq = new submitCancelRequestRequest(submitCancelReq);
                submitCancelRequestResponse submitCancelReqResp = new submitCancelRequestResponse();

                SubmitCancelResponse submitCancelResp;

                Console.WriteLine("Sending submit cancel request");
                do
                {
                    System.Threading.Thread.Sleep(PerSecurity.PollInterval);

                    submitCancelReqResp = ps.submitCancelRequest(submitCancelReqReq);
                    submitCancelResp = submitCancelReqResp.submitCancelResponse;

                    Console.WriteLine("Submit cancel request status: " + submitCancelResp.statusCode.description +
                         " responseId: " + submitCancelResp.responseId);
                } while (submitCancelResp.statusCode.code == PerSecurity.DataNotAvailable);

                RetrieveCancelRequest retrieveCancelReq = new RetrieveCancelRequest();
                retrieveCancelReq.responseId = submitCancelResp.responseId;

                retrieveCancelResponseRequest retrieveCancelRespReq = new retrieveCancelResponseRequest(retrieveCancelReq);
                retrieveCancelResponseResponse retrieveCancelRespResp;

                RetrieveCancelResponse retrieveCancelResp;

                Console.WriteLine("Sending retrieve cancel request");

                // Keep polling for response till the data is available
                do
                {
                    System.Threading.Thread.Sleep(PerSecurity.PollInterval);

                    retrieveCancelRespResp = ps.retrieveCancelResponse(retrieveCancelRespReq);
                    retrieveCancelResp = retrieveCancelRespResp.retrieveCancelResponse;
                } while (retrieveCancelResp.statusCode.code == PerSecurity.DataNotAvailable);

                // Display data
                if (retrieveCancelResp.statusCode.code == PerSecurity.Success)
                {
                    Console.WriteLine("Retrieve cancel request successful.");
                    CancelResponseStatus[] ls = retrieveCancelResp.cancelResponseStatus;
                    for (int i = 0; i < ls.Length; i++)
                    {
                        Console.WriteLine("The cancel status for response id :"
                                + ls[i].responseId + " is " + ls[i].cancelStatus.ToString());
                    }
                }
                else if (retrieveCancelResp.statusCode.code == PerSecurity.RequestError)
                    Console.WriteLine("Error in submitted request");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
