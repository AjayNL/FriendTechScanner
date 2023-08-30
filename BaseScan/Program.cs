using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Net.Security;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

class MethodInfo
{ 
    public string sharesSubject { get; set; }
    public BigInteger AmountBought { get; set; }
    public string TwitterUser { get; set; }
}

public class UserData
{
    public int Id { get; set; }
    public string Address { get; set; }
    public string TwitterUsername { get; set; }
    public string TwitterName { get; set; }
    public string TwitterPfpUrl { get; set; }
    public string TwitterUserId { get; set; }
    public long LastOnline { get; set; }
    public int HolderCount { get; set; }
    public int HoldingCount { get; set; }
    public int ShareSupply { get; set; }
    public string DisplayPrice { get; set; }
    public string LifetimeFeesCollectedInWei { get; set; }
}

class Program
{
    static async Task Main()
    {
        List<string> AdressesDone = new List<string>();
        string startBlock = "3000446";
        string currentBlock = startBlock;
        string apiKey = "YBJCUNHES8VTNC8QVP8M7WVHQXINPQ35NF";
        List<string> celebs = new List<string>();
        bool foundCeleb;

        celebs.Add("CryptoDiffer");
        celebs.Add("TheMoonCarl");
        celebs.Add("CryptoWendyO");
        celebs.Add("whale_alert");
        celebs.Add("AltcoinGordon");
        celebs.Add("cz_binance");
        celebs.Add("opensea");
        celebs.Add("KennethBosak");
        celebs.Add("ethereumJoseph");
        celebs.Add("elonmusk");
        celebs.Add("justinbieber");
        celebs.Add("rihanna");
        celebs.Add("katyperry");
        celebs.Add("taylorswift13");
        celebs.Add("ladygaga");
        celebs.Add("jimmyfallon");
        celebs.Add("NBA");
        celebs.Add("KylieJenner");
        celebs.Add("shakira");

        while (true)
        {
            foundCeleb = false;
            string apiUrl = $"https://api.basescan.org/api?module=account&action=txlist&address=0xcf205808ed36593aa40a44f10c7f7c2f67d4a4d4&startblock={startBlock}&endblock=99999999&sort=desc&apikey={apiKey}";

            using (HttpClient httpClient = new HttpClient())
            {
                HttpResponseMessage response = await httpClient.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = await response.Content.ReadAsStringAsync();
                    JObject responseObj = JObject.Parse(jsonResponse);

                    if (responseObj["status"].ToString() == "1")
                    {
                        JArray entries = JArray.Parse(responseObj["result"].ToString());                                              
                        foreach (JObject entry in entries)
                        {
                            string fromAddress = entry["from"].ToString();
                            currentBlock = entry["blockNumber"].ToString();
                            if (!AdressesDone.Contains(fromAddress))
                            {
                                string value = entry["value"].ToString();

                                string timestamp = entry["timeStamp"].ToString();
                                DateTimeOffset dateTimeValue = DateTimeOffset.FromUnixTimeSeconds(long.Parse(timestamp));
                                MethodInfo info = TransactionDetails(entry);

                                if (info != null)
                                {
                                    AdressesDone.Add(fromAddress);

                                    UserData friendTechData = await GetUserDetailsAsync(fromAddress);
                                    if (friendTechData?.TwitterName?.Trim() != "")
                                    {
                                        Debug.WriteLine($"Block {currentBlock} / Twittername: {friendTechData.TwitterName}");
                                        Debug.WriteLine($"https://twitter.com/{friendTechData.TwitterUsername}");
                                        Debug.WriteLine($"Holdercount: {friendTechData.HolderCount}");

                                        if (long.TryParse(friendTechData.DisplayPrice, out long priceInGwei))
                                        {
                                            Debug.WriteLine($"Price: {priceInGwei * 0.000000000000000001}");
                                        }
                                        if (celebs.Contains(friendTechData.TwitterUsername))
                                        {
                                            Debug.WriteLine($"CELEBRITY");
                                            foundCeleb = true;
                                            //SendMail($"https://twitter.com/{friendTechData.TwitterUsername}");
                                        }

                                        Debug.WriteLine($"");
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("API response indicated an error.");
                    }
                }
                else
                {
                    Console.WriteLine($"HTTP request failed with status code: {response.StatusCode}");
                }

                startBlock = currentBlock;
                if (foundCeleb)
                {
                    Debug.WriteLine("------------");
                    Debug.WriteLine("FOUND CELEB!");
                    Debug.WriteLine("------------");
                }

                Debug.WriteLine("------------Waiting------------");
                Debug.WriteLine("");
                Thread.Sleep(60 * 1000);
            }
        }
    }

    private static MethodInfo TransactionDetails(JObject entry)
    {
        string function = entry["functionName"].ToString();
        if (function.StartsWith("buyShares"))
        {
            string input = entry["input"].ToString();
            string fromAddress = entry["from"].ToString();

            MethodInfo info = GetMethodValue(input);
            if (info.sharesSubject == fromAddress) 
            {
                return info;
            }
        }
        return null;
    }

    private static MethodInfo GetMethodValue(string input)
    {
        MethodInfo value = new MethodInfo();
       
        string sharesSubjectHex = input.Substring(10, 64); // Next 32 bytes (address)
        string amountHex = input.Substring(74, 64); // Next 32 bytes (uint256)

        // Convert hexadecimal values to their respective data types
        value.sharesSubject = "0x" + sharesSubjectHex.Substring(24); // Address starts from 24th character
        value.AmountBought = BigInteger.Parse(amountHex, System.Globalization.NumberStyles.HexNumber);

        return value;
    }

    private static async Task<UserData> GetUserDetailsAsync(string address)
    {
        string apiUrl = $"https://prod-api.kosetto.com/users/{address}";

        using (HttpClient httpClient = new HttpClient())
        try
        {
            HttpResponseMessage response = await httpClient.GetAsync(apiUrl);

            if (response.IsSuccessStatusCode)
            {
                string jsonResponse = await response.Content.ReadAsStringAsync();
                UserData userData = JsonConvert.DeserializeObject<UserData>(jsonResponse);
                return userData;
            }
            else
            {
                // Handle non-success status codes
                throw new Exception($"Twitter API call failed with status code: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            // Handle exceptions
            //throw new Exception("An error occurred while making the Twitter API call.", ex);
        }
        return new UserData();
    }

    private static void SendMail(string body)
    {
        string senderEmail = "";
        string senderPassword = "";
        string recipientEmail = "";
        string subject = "FriendTech";

        SmtpClient smtpClient = new SmtpClient("")
        {
            Port = 587,
            Credentials = new NetworkCredential(senderEmail, senderPassword),
            EnableSsl = true,
        };

        MailMessage mailMessage = new MailMessage(senderEmail, recipientEmail, subject, body);

        try
        {
            smtpClient.Send(mailMessage);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }
}
