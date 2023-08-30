using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BaseScan.Resources;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using System.Runtime.Remoting.Messaging;
using System.CodeDom;
using System.ComponentModel;
using System.Security.Principal;

namespace BaseScan.Utils
{
    public class TransactionInfo
    {
        public string blockNumber { get; set; }
        public string blockHash { get; set; }
        public string timeStamp { get; set; }
        public string hash { get; set; }
        public string nonce { get; set; }
        public string transactionIndex { get; set; }
        public string from { get; set; }
        public string to { get; set; }
        public string value { get; set; }
        public string gas { get; set; }
        public string gasPrice { get; set; }
        public string input { get; set; }
        public string methodId { get; set; }
        public string functionName { get; set; }
        public string contractAddress { get; set; }
        public string cumulativeGasUsed { get; set; }
        public string txreceipt_status { get; set; }
        public string gasUsed { get; set; }
        public string confirmations { get; set; }
        public string isError { get; set; }
    }

    internal class TransactionHelper
    {
        private async static Task<List<TransactionInfo>> internalGetTransactions(string apiURL)
        {
            List<TransactionInfo> result = new List<TransactionInfo>();
            using (HttpClient httpClient = new HttpClient())
                try
                {
                    HttpResponseMessage response = await httpClient.GetAsync(apiURL);
                    string jsonResponse = await response.Content.ReadAsStringAsync();
                    JObject responseObj = JObject.Parse(jsonResponse);

                    if (responseObj["status"].ToString() == "1")  // Success
                    {
                        JArray entries = JArray.Parse(responseObj["result"].ToString());
                        foreach (JObject entry in entries)
                        {
                            TransactionInfo transactionInfo = entry.ToObject<TransactionInfo>();
                            result.Add(transactionInfo);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error processing JSON: " + ex.Message);
                }

            return result;
        }

        public async static Task<List<TransactionInfo>> GetTransactions(string startBlock, string account)
        {
            string apiUrl = $"https://api.basescan.org/api?module=account&action=txlist&address={account}&startblock={startBlock}&endblock=99999999&sort=desc&apikey={Constants.APIKey}";
            List<TransactionInfo> transactions = await internalGetTransactions(apiUrl);
            return transactions;
        }

        public async static Task<bool> hasMaxTransactions(string account, int maxTransactions)
        {
            int pageSize = maxTransactions += 1;
            string apiUrl = $"https://api.basescan.org/api?module=account&action=txlist&address={account}&page=1&sort=desc&offset={pageSize}&apikey={Constants.APIKey}";
            List<TransactionInfo> transactions = await internalGetTransactions(apiUrl);
            return transactions.Count < maxTransactions;
        }
    }
}
