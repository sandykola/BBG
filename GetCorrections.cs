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
    * GetCorrections - This class submits a get corrections request.
    */
    using System;
    using System.Collections.Generic;
    using System.Text;
    using PerSecurity_Dotnet.PerSecurityWSDL;

    internal class GetCorrections
    {
        public void run(PerSecurity_Dotnet.PerSecurityWSDL.PerSecurityWS ps)
        {
            try
            {
                // Setting getCorrections request and response objects
                GetCorrectionsRequest getCorrReq = new GetCorrectionsRequest();

                getCorrectionsRequest1 getCorrReq1 = new getCorrectionsRequest1(getCorrReq);

                Console.WriteLine("Sending Get Corrections request");
                getCorrectionsResponse1 getCorrResp1 = ps.getCorrections(getCorrReq1);
                GetCorrectionsResponse getCorrResp = getCorrResp1.getCorrectionsResponse;

                // Displaying output
                if (getCorrResp.statusCode.code == PerSecurity.Success)
                {
                    Console.WriteLine("Retrieve Get Corrections request successful");
                    Console.WriteLine("Following are the corrections:\n");
                    for (int i = 0; i < getCorrResp.correctionRecords.Length; i++)
                    {
                        Console.WriteLine("\tInstrument: " + getCorrResp.correctionRecords[i].instrument.id + " " +
                            getCorrResp.correctionRecords[i].instrument.yellowkey + "\n"
                            + "\tFields: " + getCorrResp.correctionRecords[i].field + "\n" +
                            "\tOld Value: " + getCorrResp.correctionRecords[i].oldValue.ToString() + "\n" +
                            "\tNew Value: " + getCorrResp.correctionRecords[i].newValue.ToString());
                    }
                }
                else if (getCorrResp.statusCode.code == PerSecurity.DataNotAvailable)
                {
                    Console.WriteLine("Retrieve Get Corrections request successful");
                    Console.WriteLine("No corrections found");
                }
                else
                {
                    Console.WriteLine("Retrieve Get Corrections request failed");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

            }
        }

    }

}
