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
     * GetFields - This class makes getfields request using keyword and marketSector and displays result.
     */
    using System;
    using System.Collections.Generic;
    using System.Text;
    using PerSecurity_Dotnet.PerSecurityWSDL;

    internal class GetFields
    {
        public void run(PerSecurityWSDL.PerSecurityWS ps)
        {
            try
            {
                // Making GetFields request using keyword and marketSector
                GetFieldsRequest gtFldReq = new GetFieldsRequest();
                FieldSearchCriteria criteria = new FieldSearchCriteria();
                criteria.keyword = "Price";
                criteria.marketsectors = new MarketSector[] { MarketSector.Equity };
                gtFldReq.criteria = criteria;

                getFieldsRequest1 gtReq1 = new getFieldsRequest1(gtFldReq);
                getFieldsResponse1 gtResp1 = new getFieldsResponse1();
                gtResp1 = ps.getFields(gtReq1);

                // Parsing the response
                FieldInfo[] fields = gtResp1.getFieldsResponse.fields;
                for (int i = 0; i < fields.Length; i++)
                {
                    Console.WriteLine("******************");
                    Console.WriteLine("Mnemonic: " + fields[i].mnemonic
                        + "\nid: " + fields[i].id
                        + "\ndefinition: " + fields[i].definition
                        + "\ndescription:" + fields[i].description
                        );
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: " + ex.Message);
            }
        }
    }
}
