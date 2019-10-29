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
    * PortFolioValidation - This class makes get portfolio Validation request. The data is displayed if available.
    */
    using System;
    using System.Collections.Generic;
    using System.Text;
    using PerSecurity_Dotnet.PerSecurityWSDL;

    internal class PortfolioValidation
    {
        public void run(PerSecurityWSDL.PerSecurityWS ps)
        {
            try
            {
                // Setting request header information
                GetDataHeaders getDataHeaders = new GetDataHeaders();
                getDataHeaders.secmaster = true;
                getDataHeaders.secmasterSpecified = true;
                getDataHeaders.derived = true;
                getDataHeaders.derivedSpecified = true;

                getDataHeaders.bvaltierSpecified = true;
                getDataHeaders.bvaltier = 1;
                // The bval request will run at a scheduled time but the PortfolioValidation Request will get a response after the req file is copied
                getDataHeaders.bvalsnapshot = BvalSnapshot.ny3pm;
                getDataHeaders.bvalsnapshotSpecified = true;

                // Setting Instrument information
                Instrument instr = new Instrument();
                instr.id = "BBG0009P1SC8";
                instr.type = InstrumentType.BB_GLOBAL;
                instr.typeSpecified = true;

                // group all instrument into a single instance of type Instruments.
                Instruments instrs = new Instruments();
                instrs.instrument = new Instrument[] { instr };

                // Setting the getdata request parameter
                SubmitGetDataRequest sbmtGtDtReq = new SubmitGetDataRequest();
                sbmtGtDtReq.headers = getDataHeaders;
                sbmtGtDtReq.fields = new string[] { "BVAL_BEEM", "BVAL_SNAPSHOT" };

                // sbmtGtDtReq.instruments = new Instruments();
                sbmtGtDtReq.instruments = instrs;

                Console.WriteLine("Submit BvalGetData Request");

                submitGetDataRequestRequest sbmtGtDtReqReq = new submitGetDataRequestRequest(sbmtGtDtReq);
                submitGetDataRequestResponse sbmtGtDtReqResp = ps.submitGetDataRequest(sbmtGtDtReqReq);
                SubmitGetDataResponse sbmtGtDtResp = sbmtGtDtReqResp.submitGetDataResponse;

                System.Console.WriteLine("status " + sbmtGtDtResp.statusCode.description);
                System.Console.WriteLine("Submit BvalGetData request -  response ID = " + sbmtGtDtResp.responseId);

                GetPortfolioValidationRequest req = new GetPortfolioValidationRequest();
                req.responseId = sbmtGtDtResp.responseId;

                Console.WriteLine("Sending submit portfolio validation request");

                getPortfolioValidationRequest1 req1 = new getPortfolioValidationRequest1(req);
                getPortfolioValidationResponse1 resp1;

                GetPortfolioValidationResponse resp;

                // Keep polling for response till the data is available
                do
                {
                    Console.WriteLine("Polling for PortfolioValidation request");
                    System.Threading.Thread.Sleep(PerSecurity.PollInterval);
                    resp1 = ps.getPortfolioValidation(req1);
                    resp = resp1.getPortfolioValidationResponse;
                } while (resp.statusCode.code == PerSecurity.DataNotAvailable);

                // Display data
                if (resp.statusCode.code == PerSecurity.Success)
                {

                    Console.WriteLine("Retrieve get quotes request successful.  Response ID:" + resp.responseId);
                    for (int i = 0; i < resp.instrumentDatas.Length; i++)
                    {
                        Console.WriteLine("Data for :"
                            + resp.instrumentDatas[i].instrument.id + " "
                            + resp.instrumentDatas[i].instrument.yellowkey);

                        Console.WriteLine("validation code =  "
                            + resp.instrumentDatas[i].code.ToString());
                    }
                }
                else if (resp.statusCode.code == PerSecurity.RequestError)
                    Console.WriteLine("Error in submitted PortfolioValidation request");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + "   " + e.StackTrace);
            }
        }
    }
}
