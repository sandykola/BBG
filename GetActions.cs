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
     * GetActions - This class makes get actions request. This is followed by retrieve get actions request to display data.
     */
    using System;
    using System.Collections.Generic;
    using System.Text;
    using PerSecurity_Dotnet.PerSecurityWSDL;

    internal class GetActions
    {
        public void run(PerSecurityWSDL.PerSecurityWS ps)
        {
            try
            {
                // Setting headers
                GetActionsHeaders getActionHeaders = new GetActionsHeaders();
                getActionHeaders.actions_date = ActionsDate.entry;
                getActionHeaders.actions_dateSpecified = true;
                string[] actions = new string[] { "DVD_CASH", "DISTRIBUTIONS" };
                getActionHeaders.actions = actions;

                // Setting Instruments
                Instruments instruments = new Instruments();
                Instrument instr = new Instrument();
                instr.id = "COP US";
                instr.yellowkeySpecified = true;
                instr.typeSpecified = true;
                instr.yellowkey = MarketSector.Equity;
                instr.type = InstrumentType.TICKER;
                instruments.instrument = new Instrument[] { instr };

                // Submitting request
                SubmitGetActionsRequest req = new SubmitGetActionsRequest();
                req.headers = getActionHeaders;
                req.instruments = instruments;

                submitGetActionsRequestRequest subGetActReqReq = new submitGetActionsRequestRequest(req);
                submitGetActionsRequestResponse subGetActReqResp = ps.submitGetActionsRequest(subGetActReqReq);
                SubmitGetActionsResponse subGetActResp = subGetActReqResp.submitGetActionsResponse;

                string responseId = subGetActResp.responseId;
                Console.WriteLine("Submit get actions request status: " + subGetActResp.statusCode.description +
                         " responseId: " + responseId);

                // Submit retrieve request
                RetrieveGetActionsRequest rtrvGetActionsReq = new RetrieveGetActionsRequest();
                rtrvGetActionsReq.responseId = responseId;
                Console.WriteLine("Sending retrieve get actions request");

                retrieveGetActionsResponseRequest rtrvGetActionsRespReq = new retrieveGetActionsResponseRequest(rtrvGetActionsReq);
                retrieveGetActionsResponseResponse rtrvGetActionsRespResp;

                RetrieveGetActionsResponse rtrvGetActionsResp;

                // Keep polling for response till the data is available
                do
                {
                    System.Threading.Thread.Sleep(PerSecurity.PollInterval);
                    rtrvGetActionsRespResp = ps.retrieveGetActionsResponse(rtrvGetActionsRespReq);
                    rtrvGetActionsResp = rtrvGetActionsRespResp.retrieveGetActionsResponse;
                } while (rtrvGetActionsResp.statusCode.code == PerSecurity.DataNotAvailable);

                // Display data
                if (rtrvGetActionsResp.statusCode.code == PerSecurity.Success)
                {
                    Console.WriteLine("Retrieve get quotes request successful.  Response ID:" + rtrvGetActionsResp.responseId);
                    for (int i = 0; i < rtrvGetActionsResp.instrumentDatas.Length; i++)
                    {
                        Console.WriteLine("Data for :"
                                + rtrvGetActionsResp.instrumentDatas[i].instrument.id + " "
                                + rtrvGetActionsResp.instrumentDatas[i].instrument.yellowkey
                                );
                        Console.WriteLine(", Company id = " + rtrvGetActionsResp.instrumentDatas[i].standardFields.companyId.ToString());
                        Console.WriteLine(", Security id = " + rtrvGetActionsResp.instrumentDatas[i].standardFields.securityId.ToString());
                        if (rtrvGetActionsResp.instrumentDatas[i].data != null)
                        {
                            for (int j = 0; j < rtrvGetActionsResp.instrumentDatas[i].data.Length; j++)
                            {
                                Console.WriteLine(": field =  "
                                        + rtrvGetActionsResp.instrumentDatas[i].data[j].field
                                        + ", value =  "
                                        + rtrvGetActionsResp.instrumentDatas[i].data[j].value);
                            }
                        }

                    }
                }
                else if (rtrvGetActionsResp.statusCode.code == PerSecurity.RequestError)
                    Console.WriteLine("Error in submitted request");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
