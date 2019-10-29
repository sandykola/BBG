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
    * GetDataFieldSet - This class makes getdata request with the request field set property set to BVAL_BOND.
    * This is followed by retrieve getdata request to display data.
    */
    using System;
    using System.Collections.Generic;
    using System.Text;
    using PerSecurity_Dotnet.PerSecurityWSDL;

    internal class GetDataFieldSet
    {
        public void run(PerSecurityWSDL.PerSecurityWS ps)
        {
            try
            {
                // Setting headers for the request
                GetDataHeaders getDataHeaders = new GetDataHeaders();
                getDataHeaders.closingvaluesSpecified = true;
                getDataHeaders.closingvalues = true;
                getDataHeaders.secmasterSpecified = true;
                getDataHeaders.secmaster = true;
                getDataHeaders.derivedSpecified = true;
                getDataHeaders.derived = true;

                // Setting list of instruments for data request
                Instrument ticker = new Instrument();
                ticker = new Instrument();
                ticker.id = "IBM US";
                ticker.type = InstrumentType.TICKER;
                ticker.yellowkey = MarketSector.Equity;

                Instrument[] instr = new Instrument[] { ticker };

                string[] field = new string[] { "ID_BB_UNIQUE", "TICKER",
                        "PX_LAST", "PX_ASK", "PX_BID", "VWAP_DT" };

                BvalFieldSet fieldset = new BvalFieldSet();
                fieldset.fieldmacro = BvalFieldMacro.BVAL_BOND;

                Instruments instrmnts = new Instruments();
                instrmnts.instrument = instr;

                // Submit getdata request
                SubmitGetDataRequest sbmtGetDtReq = new SubmitGetDataRequest();
                sbmtGetDtReq.headers = getDataHeaders;
                sbmtGetDtReq.fields = field;
                sbmtGetDtReq.fieldsets = new BvalFieldSet[] { fieldset };
                sbmtGetDtReq.instruments = instrmnts;

                Console.WriteLine("Sending submit getdata request");

                submitGetDataRequestRequest sbmtGetDtReqReq = new submitGetDataRequestRequest(sbmtGetDtReq);
                submitGetDataRequestResponse sbmtGetDtReqResp = ps.submitGetDataRequest(sbmtGetDtReqReq);
                SubmitGetDataResponse sbmtGetDtResp = sbmtGetDtReqResp.submitGetDataResponse;

                Console.WriteLine("Submit getdata request status: " + sbmtGetDtResp.statusCode.description +
                         " responseId: " + sbmtGetDtResp.responseId);

                // Submit retrieve data
                RetrieveGetDataRequest rtvGetDtReq = new RetrieveGetDataRequest();
                rtvGetDtReq.responseId = sbmtGetDtResp.responseId;

                retrieveGetDataResponseRequest rtvGetDtRespReq = new retrieveGetDataResponseRequest(rtvGetDtReq);
                retrieveGetDataResponseResponse rtvGetDtRespResp;

                RetrieveGetDataResponse rtvGetDtResp;

                Console.WriteLine("Sending retrieve getdata request");

                // Keep polling for response till the data is available
                do
                {
                    System.Threading.Thread.Sleep(PerSecurity.PollInterval);

                    rtvGetDtRespResp = ps.retrieveGetDataResponse(rtvGetDtRespReq);
                    rtvGetDtResp = rtvGetDtRespResp.retrieveGetDataResponse;
                } while (rtvGetDtResp.statusCode.code == PerSecurity.DataNotAvailable);

                // Display data
                if (rtvGetDtResp.statusCode.code == PerSecurity.Success)
                {
                    Console.WriteLine("Retrieve getdata request successful.  Response ID:" + rtvGetDtResp.responseId);
                    for (int i = 0; i < rtvGetDtResp.instrumentDatas.Length; i++)
                    {
                        Console.WriteLine("Data for :"
                                + rtvGetDtResp.instrumentDatas[i].instrument.id + " "
                                + rtvGetDtResp.instrumentDatas[i].instrument.yellowkey);
                        for (int j = 0; j < rtvGetDtResp.instrumentDatas[i].data.Length; j++)
                        {
                            Console.WriteLine("  "
                                    + rtvGetDtResp.fields[j]
                                    + ": "
                                    + rtvGetDtResp.instrumentDatas[i].data[j].value);
                        }
                    }
                }
                else if (rtvGetDtResp.statusCode.code == PerSecurity.RequestError)
                    Console.WriteLine("Error in submitted request");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
