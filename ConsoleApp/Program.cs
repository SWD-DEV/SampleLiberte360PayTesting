using Liberte360Pay;
using Liberte360Pay.Configuration;
using Liberte360Pay.Exceptions;
using Liberte360Pay.Metrics;
using Liberte360Pay.Models.BulkDisbursement;
using Liberte360Pay.Models.BulkNameVerify;
using Liberte360Pay.Utilities;
using System.Transactions;

namespace ConsoleApp;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("360Pay SDK Sample Application\n");
        Console.WriteLine("=================================\n");

        // Configure the SDK
        var options = new Liberte360PayOptions
        {
            ApiKey = "sk_test_2dbac4ee568ed3439e15ae85c535cb4bf7c38a9c", 
            BaseUrl = "https://uat-360pay-merchant-api.libertepay.com",
            DefaultCurrency = "GHS",
            TimeoutSeconds = 30,
            // Enable metrics collection
            Metrics = new MetricsOptions
            {
                EnableMetrics = true
            }
        };

        // Create metrics collector
        var metricsCollector = new MetricsCollector(options.Metrics!);
        
        // Create client with metrics collector
        var client = new Liberte360PayClient(options, metricsCollector);

        try
        {
            //// Example 1: Name Verification
            //await NameVerifyExample(client);

            //Console.WriteLine();

            //// Example 2: Disbursement with Name Verification
            //await DisbursementExample(client);

            //Console.WriteLine();

            //// Example 3: Fluent API with Metadata
            //await FluentApiWithMetadataExample(client);

            //Console.WriteLine();

            //// Example 4: Collections API
            //await CollectionExample(client);

            //Console.WriteLine();

            //// Example 5: Bulk Disbursement
            //await BulkDisbursementExample(client);

            //Console.WriteLine();

            // Example 6: Bulk Name Verify
            await BulkNameVerifyExample(client);

            //Console.WriteLine();

            //// Example 7: Bulk Disbursement Status Check
            //await BulkDisbursementStatusExample(client);

            //Console.WriteLine();

            //// Example 8: Transaction Status Check
            //await TransactionStatusExample(client);

            //Console.WriteLine();

            //// Example 9: Checkout Initialize
            //await CheckoutExample(client);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n Fatal Error: {ex.Message}");
        }

        // Display metrics summary
        Console.WriteLine("\n=================================\n");
        Console.WriteLine("API Metrics Summary");
        Console.WriteLine("=================================\n");
        DisplayMetrics(metricsCollector);

        Console.WriteLine("\n=================================\n");
        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }

    static async Task NameVerifyExample(ILiberte360PayClient client)
    {
        Console.WriteLine("Example 1: Name Verification");
        Console.WriteLine("--------------------------------\n");

        try
        {
            var accountNumber = "0246089019"; // Replace with actual account number
            var institutionCode = InstitutionCode.Mtn;

            Console.WriteLine($"Verifying account: {accountNumber}");
            Console.WriteLine($"Institution: MTN Mobile Money ({institutionCode.GetCodeString()})\n");

            var response = await client.NameVerify()
                .WithAccountNumber(accountNumber)
                .WithInstitutionCode(institutionCode)
                .ExecuteAsync();

            if (response.IsSuccess)
            {
                Console.WriteLine(" Verification Successful!");
                Console.WriteLine($"Account Name: {response.Data?.AccountName}");
                Console.WriteLine($"Account Number: {response.Data?.AccountNumber}");
                Console.WriteLine($"Status: {response.Status}");
                Console.WriteLine($"Code: {response.Code}");
            }
            else
            {
                Console.WriteLine($"Verification Failed: {response.Message}");
                Console.WriteLine($"Code: {response.Code}");
            }
        }
        catch (ValidationException ex)
        {
            Console.WriteLine($"Validation Error: {ex.Message}");
            Console.WriteLine($" Parameter: {ex.ParameterName}");
        }
        catch (ApiException ex)
        {
            Console.WriteLine($"API Error: {ex.Message}");
            Console.WriteLine($"HTTP Status: {ex.StatusCode}");
            Console.WriteLine($"Response Code: {ex.ResponseCode}");
            if (!string.IsNullOrEmpty(ex.ResponseContent))
            {
                Console.WriteLine($"Response: {ex.ResponseContent}");
            }
        }
        catch (Liberte360PayException ex)
        {
            Console.WriteLine($"SDK Error: {ex.Message}");
        }
    }

    static async Task DisbursementExample(ILiberte360PayClient client)
    {
        Console.WriteLine("Example 2: Disbursement (with Name Verification)");
        Console.WriteLine("---------------------------------------------------\n");

        try
        {
            var accountNumber = "0246089019"; // Replace with actual account number
            var institutionCode = InstitutionCode.Mtn;
            var amount = 50.00m;

            // Step 1: Verify the account first (MANDATORY)
            Console.WriteLine("Step 1: Verifying account...");
            var verifyResponse = await client.NameVerify()
                .WithAccountNumber(accountNumber)
                .WithInstitutionCode(institutionCode)
                .ExecuteAsync();

            if (!verifyResponse.IsSuccess)
            {
                Console.WriteLine($" Verification failed: {verifyResponse.Message}");
                return;
            }

            Console.WriteLine($"Verified: {verifyResponse.Data?.AccountName}\n");

            // Step 2: Make the disbursement
            Console.WriteLine("Step 2: Processing disbursement...");
            var transactionId = $"TXN-{DateTime.UtcNow.Ticks}";

            var disbursementResponse = await client.Disbursement()
                .WithAccountName(verifyResponse.Data!.AccountName)
                .WithAccountNumber(accountNumber)
                .WithAmount(amount)
                .WithInstitutionCode(institutionCode)
                .WithTransactionId(transactionId)
                .WithReference("Test payment from SDK sample")
                .ExecuteAsync();

            if (disbursementResponse.IsSuccess)
            {
                Console.WriteLine("\n Disbursement Successful!");
                Console.WriteLine($"   Transaction ID: {disbursementResponse.Data?.TransactionId}");
                Console.WriteLine($"   Message: {disbursementResponse.Data?.TransactionMessage}");
                Console.WriteLine($"   Amount: GHS {amount:N2}");
                Console.WriteLine($"   Status: {disbursementResponse.Status}");
                Console.WriteLine($"   Code: {disbursementResponse.Code}");
                Console.WriteLine($"\n   Your Transaction ID: {transactionId}");
            }
            else
            {
                Console.WriteLine($" Disbursement Failed: {disbursementResponse.Message}");
                Console.WriteLine($"   Code: {disbursementResponse.Code}");
            }
        }
        catch (ValidationException ex)
        {
            Console.WriteLine($"Validation Error: {ex.Message}");
            Console.WriteLine($"Parameter: {ex.ParameterName}");
        }
        catch (ApiException ex)
        {
            Console.WriteLine($"API Error: {ex.Message}");
            Console.WriteLine($"HTTP Status: {ex.StatusCode}");
            Console.WriteLine($"Response Code: {ex.ResponseCode}");
            
            // Map common error codes
            switch (ex.ResponseCode)
            {
                case "401":
                case "407":
                    Console.WriteLine("   → Check your API key");
                    break;
                case "404":
                    Console.WriteLine("   → Account not found");
                    break;
                case "410":
                    Console.WriteLine("   → Insufficient funds");
                    break;
                case "491":
                    Console.WriteLine("   → User needs to authorize on their phone");
                    break;
            }
        }
        catch (Liberte360PayException ex)
        {
            Console.WriteLine($"SDK Error: {ex.Message}");
        }
    }

    static async Task FluentApiWithMetadataExample(ILiberte360PayClient client)
    {
        Console.WriteLine("Example 3: Fluent API with Metadata");
        Console.WriteLine("---------------------------------------\n");

        try
        {
            var accountNumber = "0246089019"; // Replace with actual account number
            var institutionCode = InstitutionCode.Mtn;

            // First verify
            var verifyResponse = await client.NameVerify()
                .WithAccountNumber(accountNumber)
                .WithInstitutionCode(institutionCode)
                .ExecuteAsync();

            if (!verifyResponse.IsSuccess)
            {
                Console.WriteLine($" Verification failed: {verifyResponse.Message}");
                return;
            }

            // Demonstrate fluent API with method chaining
            Console.WriteLine("Demonstrating fluent API with metadata...\n");

            var transactionId = $"TXN-{DateTime.UtcNow.Ticks}";

            var response = await client.Disbursement()
                .WithAccountName(verifyResponse.Data!.AccountName)
                .WithAccountNumber(accountNumber)
                .WithAmount(25.00m)
                .WithInstitutionCode(institutionCode)
                .WithTransactionId(transactionId)
                .WithCurrency("GHS")
                .WithReference("Bonus payment")
                .AddMetaData("employee_id", "EMP12345")
                .AddMetaData("department", "Engineering")
                .AddMetaData("payment_type", "Bonus")
                .AddMetaData("month", "December")
                .AddMetaData("year", "2024")
                .ExecuteAsync();

            if (response.IsSuccess)
            {
                Console.WriteLine(" Transaction Successful with Metadata!");
                Console.WriteLine($"   Transaction ID: {response.Data?.TransactionId}");
                Console.WriteLine($"   Message: {response.Data?.TransactionMessage}");
                Console.WriteLine("\n   This demonstrates the fluent API pattern:");
                Console.WriteLine("   - Clean method chaining");
                Console.WriteLine("   - Multiple metadata entries");
                Console.WriteLine("   - Easy to read and maintain");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($" Error: {ex.Message}");
        }
    }

    static async Task CollectionExample(ILiberte360PayClient client)
    {
        Console.WriteLine("Example 4: Collections (Debit from Account)");
        Console.WriteLine("-----------------------------------------------\n");

        try
        {
            var accountNumber = "0246089019"; // Replace with actual account number
            var institutionCode = InstitutionCode.Mtn;
            var amount = 30.00m;

            // Step 1: Verify the account first (MANDATORY)
            Console.WriteLine("Step 1: Verifying account...");
            var verifyResponse = await client.NameVerify()
                .WithAccountNumber(accountNumber)
                .WithInstitutionCode(institutionCode)
                .ExecuteAsync();

            if (!verifyResponse.IsSuccess)
            {
                Console.WriteLine($" Verification failed: {verifyResponse.Message}");
                return;
            }

            Console.WriteLine($" Verified: {verifyResponse.Data?.AccountName}\n");

            // Step 2: Collect payment
            Console.WriteLine("Step 2: Processing collection...");
            var transactionId = $"COL-{DateTime.UtcNow.Ticks}";

            var collectionResponse = await client.Collection()
                .WithAccountName(verifyResponse.Data!.AccountName)
                .WithAccountNumber(accountNumber)
                .WithAmount(amount)
                .WithInstitutionCode(institutionCode)
                .WithTransactionId(transactionId)
                .WithReference("Invoice payment from SDK sample")
                .AddMetaData("invoice_id", "INV-001")
                .AddMetaData("customer_id", "CUST-001")
                .ExecuteAsync();

            if (collectionResponse.IsSuccess)
            {
                Console.WriteLine("\n Collection Successful!");
                Console.WriteLine($"   Transaction ID: {collectionResponse.Data?.TransactionId}");
                Console.WriteLine($"   Message: {collectionResponse.Data?.TransactionMessage}");
                Console.WriteLine($"   Amount: GHS {amount:N2}");
                Console.WriteLine($"   Status: {collectionResponse.Status}");
                Console.WriteLine($"   Code: {collectionResponse.Code}");
                Console.WriteLine($"\n   Your Transaction ID: {transactionId}");
            }
            else
            {
                Console.WriteLine($" Collection Failed: {collectionResponse.Message}");
                Console.WriteLine($"   Code: {collectionResponse.Code}");
            }
        }
        catch (ValidationException ex)
        {
            Console.WriteLine($" Validation Error: {ex.Message}");
            Console.WriteLine($"   Parameter: {ex.ParameterName}");
        }
        catch (ApiException ex)
        {
            Console.WriteLine($" API Error: {ex.Message}");
            Console.WriteLine($"   HTTP Status: {ex.StatusCode}");
            Console.WriteLine($"   Response Code: {ex.ResponseCode}");
        }
        catch (Liberte360PayException ex)
        {
            Console.WriteLine($" SDK Error: {ex.Message}");
        }
    }

    static async Task BulkDisbursementExample(ILiberte360PayClient client)
    {
        Console.WriteLine("Example 5: Bulk Disbursement");
        Console.WriteLine("--------------------------------\n");

        try
        {
            var accountNumber1 = "0246089019"; // Replace with actual account numbers
            var accountNumber2 = "0246089019"; // Using same for demo - replace with different numbers
            var institutionCode = InstitutionCode.Mtn;

            Console.WriteLine("Processing bulk disbursement to multiple recipients...\n");

            // First, verify accounts (recommended before bulk disbursement)
            Console.WriteLine("Verifying accounts...");
            var verify1 = await client.NameVerify()
                .WithAccountNumber(accountNumber1)
                .WithInstitutionCode(institutionCode)
                .ExecuteAsync();

            if (!verify1.IsSuccess)
            {
                Console.WriteLine($" Verification failed for {accountNumber1}: {verify1.Message}");
                return;
            }

            var verify2 = await client.NameVerify()
                .WithAccountNumber(accountNumber2)
                .WithInstitutionCode(institutionCode)
                .ExecuteAsync();

            if (!verify2.IsSuccess)
            {
                Console.WriteLine($" Verification failed for {accountNumber2}: {verify2.Message}");
                return;
            }

            Console.WriteLine($" Verified: {verify1.Data?.AccountName}");
            Console.WriteLine($" Verified: {verify2.Data?.AccountName}\n");

            // Build disbursement items from collection
            Console.WriteLine("Building disbursement items...");
            var baseTimestamp = DateTime.UtcNow.Ticks.ToString().Substring(7); // Last 12 digits for uniqueness
            var disbursementItems = new List<BulkDisbursementItem>
            {
                new BulkDisbursementItem
                {
                    AccountName = verify1.Data!.AccountName,
                    AccountNumber = accountNumber1,
                    Amount = 50.00m,
                    InstitutionCode = institutionCode.GetCodeString(),
                    TransactionId = $"BK-{baseTimestamp}-1",
                    Currency = "GHS",
                    Reference = "Bulk Payment - Recipient 1",
                    MetaData = new Dictionary<string, string>
                    {
                        { "batch_id", "BATCH-001" }
                    }
                },
                new BulkDisbursementItem
                {
                    AccountName = verify2.Data!.AccountName,
                    AccountNumber = accountNumber2,
                    Amount = 75.00m,
                    InstitutionCode = institutionCode.GetCodeString(),
                    TransactionId = $"BK-{baseTimestamp}-2",
                    Currency = "GHS",
                    Reference = "Bulk Payment - Recipient 2",
                    MetaData = new Dictionary<string, string>
                    {
                        { "batch_id", "BATCH-001" }
                    }
                }
            };

            // Process bulk disbursement using collection
            Console.WriteLine("Processing bulk disbursement...");
            var bulkResponse = await client.BulkDisbursement()
                .WithDisbursements(disbursementItems)
                .ExecuteAsync();

            if (bulkResponse.IsSuccess)
            {
                Console.WriteLine("\n Bulk Disbursement Submitted!");
                Console.WriteLine($"   Bulk Transaction ID: {bulkResponse.Data?.BulkTransactionId}");
                Console.WriteLine($"\n   Results:");
                
                for (int i = 0; i < bulkResponse.Data?.Results.Count; i++)
                {
                    var result = bulkResponse.Data.Results[i];
                    Console.WriteLine($"   [{i + 1}] Transaction ID: {result.TransactionId}");
                    Console.WriteLine($"       Status: {result.StatusDesc}");
                    Console.WriteLine($"       Message: {result.Message}");
                }
            }
            else
            {
                Console.WriteLine($" Bulk Disbursement Failed: {bulkResponse.Message}");
                Console.WriteLine($"   Code: {bulkResponse.Code}");
            }
        }
        catch (ValidationException ex)
        {
            Console.WriteLine($" Validation Error: {ex.Message}");
            Console.WriteLine($"   Parameter: {ex.ParameterName}");
        }
        catch (ApiException ex)
        {
            Console.WriteLine($" API Error: {ex.Message}");
            Console.WriteLine($"   HTTP Status: {ex.StatusCode}");
            Console.WriteLine($"   Response Code: {ex.ResponseCode}");
        }
        catch (Liberte360PayException ex)
        {
            Console.WriteLine($" SDK Error: {ex.Message}");
        }
    }

    static async Task BulkNameVerifyExample(ILiberte360PayClient client)
    {
        Console.WriteLine("Example 6: Bulk Name Verify");
        Console.WriteLine("------------------------------\n");

        try
        {
            var accountNumber1 = "0246089019"; // Replace with actual account numbers
            var accountNumber2 = "0246089019"; // Using same for demo - replace with different numbers
            var institutionCode = InstitutionCode.Mtn;

            Console.WriteLine("Verifying multiple accounts in bulk...\n");

            // Build verification items from collection
            var verificationItems = new List<BulkNameVerifyItem>
            {
                new BulkNameVerifyItem
                {
                    AccountNumber = accountNumber1,
                    InstitutionCode = institutionCode.GetCodeString()
                },
                new BulkNameVerifyItem
                {
                    AccountNumber = accountNumber2,
                    InstitutionCode = institutionCode.GetCodeString()
                }
            };

            var bulkVerifyResponse = await client.BulkNameVerify()
                .WithVerifications(verificationItems)
                .ExecuteAsync();

            if (bulkVerifyResponse.IsSuccess)
            {
                Console.WriteLine(" Bulk Name Verify Completed!\n");
                Console.WriteLine("   Results:");
                
                for (int i = 0; i < bulkVerifyResponse.Data?.Count; i++)
                {
                    var result = bulkVerifyResponse.Data[i];
                    var statusIcon = result.StatusDesc?.ToLowerInvariant() == "success" ? "S" : "E";
                    
                    Console.WriteLine($"   [{i + 1}] {statusIcon} Account: {result.AccountNumber}");
                    Console.WriteLine($"       Name: {result.AccountName}");
                    Console.WriteLine($"       Status: {result.StatusDesc}");
                    Console.WriteLine($"       Message: {result.Message}");
                }
            }
            else
            {
                Console.WriteLine($" Bulk Name Verify Failed: {bulkVerifyResponse.Message}");
                Console.WriteLine($"   Code: {bulkVerifyResponse.Code}");
            }
        }
        catch (ValidationException ex)
        {
            Console.WriteLine($" Validation Error: {ex.Message}");
            Console.WriteLine($"   Parameter: {ex.ParameterName}");
        }
        catch (ApiException ex)
        {
            Console.WriteLine($" API Error: {ex.Message}");
            Console.WriteLine($"   HTTP Status: {ex.StatusCode}");
            Console.WriteLine($"   Response Code: {ex.ResponseCode}");
        }
        catch (Liberte360PayException ex)
        {
            Console.WriteLine($" SDK Error: {ex.Message}");
        }
    }

    static async Task BulkDisbursementStatusExample(ILiberte360PayClient client)
    {
        Console.WriteLine("Example 7: Bulk Disbursement Status Check");
        Console.WriteLine("--------------------------------------------\n");

        try
        {
            // Note: You need a valid bulk transaction ID from a previous bulk disbursement
            var bulkTransactionId = "BULK-TXN-12345"; // Replace with actual bulk transaction ID

            Console.WriteLine($"Checking status for bulk transaction: {bulkTransactionId}\n");

            var statusResponse = await client.BulkDisbursementStatus()
                .WithBulkTransactionId(bulkTransactionId)
                .ExecuteAsync();

            if (statusResponse.IsSuccess)
            {
                Console.WriteLine(" Status Check Successful!\n");
                Console.WriteLine($" Status: {statusResponse.Data?.Status}");
                Console.WriteLine($" Message: {statusResponse.Data?.Message}");
                Console.WriteLine($" Account Name: {statusResponse.Data?.AccountName}");
                Console.WriteLine($" Account Number: {statusResponse.Data?.AccountNumber}");
                Console.WriteLine($" Amount: {statusResponse.Data?.Amount}");
                Console.WriteLine($" Created At: {statusResponse.Data?.CreatedAt}");
                Console.WriteLine($" Transaction ID: {statusResponse.Data?.TransactionId}");
                Console.WriteLine($" External Transaction ID: {statusResponse.Data?.ExternalTransactionId}");
                Console.WriteLine($" Is Reversed: {statusResponse.Data?.IsReversed}");
                Console.WriteLine($" Institution Approval Code: {statusResponse.Data?.InstitutionApprovalCode}");
            }
            else
            {
                Console.WriteLine($" Status Check Failed: {statusResponse.Message}");
                Console.WriteLine($" Code: {statusResponse.Code}");
                Console.WriteLine($" Status Code: {statusResponse.StatusCode}");
                Console.WriteLine($" Status Desc: {statusResponse.StatusDesc}");
                Console.WriteLine("\n   Note: Make sure you use a valid bulk transaction ID from a previous bulk disbursement.");
            }
        }
        catch (ValidationException ex)
        {
            Console.WriteLine($" Validation Error: {ex.Message}");
            Console.WriteLine($" Parameter: {ex.ParameterName}");
        }
        catch (ApiException ex)
        {
            Console.WriteLine($" API Error: {ex.Message}");
            Console.WriteLine($" HTTP Status: {ex.StatusCode}");
            Console.WriteLine($" Response Code: {ex.ResponseCode}");
            Console.WriteLine("\n   Note: Make sure you use a valid bulk transaction ID from a previous bulk disbursement.");
        }
        catch (Liberte360PayException ex)
        {
            Console.WriteLine($" SDK Error: {ex.Message}");
        }
    }

    static async Task TransactionStatusExample(ILiberte360PayClient client)
    {
        Console.WriteLine("Example 8: Transaction Status Check");
        Console.WriteLine("----------------------------------------\n");

        try
        {
            // Note: You need a valid transaction ID from a previous transaction (disbursement or collection)
            var transactionId = "TXN-12345"; // Replace with actual transaction ID from a previous transaction

            Console.WriteLine($"Checking status for transaction: {transactionId}\n");

            var statusResponse = await client.TransactionStatus()
                .WithTransactionId(transactionId)
                .ExecuteAsync();

            if (statusResponse.IsSuccess)
            {
                Console.WriteLine(" Transaction Status Retrieved!\n");
                Console.WriteLine($"   Transaction ID: {statusResponse.Data?.TransactionId}");
                Console.WriteLine($"   External Transaction ID: {statusResponse.Data?.ExternalTransactionId}");
                Console.WriteLine($"   Account Name: {statusResponse.Data?.AccountName}");
                Console.WriteLine($"   Account Number: {statusResponse.Data?.AccountNumber}");
                Console.WriteLine($"   Amount: {statusResponse.Data?.Amount}");
                Console.WriteLine($"   Date Created: {statusResponse.Data?.DateCreated}");
                Console.WriteLine($"   Status Code: {statusResponse.Data?.StatusCode}");
                Console.WriteLine($"   Message: {statusResponse.Data?.Message}");
                Console.WriteLine($"   Is Reversed: {statusResponse.Data?.IsReversed}");
            }
            else
            {
                Console.WriteLine($" Status Check Failed: {statusResponse.Message}");
                Console.WriteLine($"   Code: {statusResponse.Code}");
                Console.WriteLine($"   Status: {statusResponse.Status}");
                Console.WriteLine("\n   Note: Make sure you use a valid transaction ID from a previous transaction (disbursement or collection).");
            }
        }
        catch (ValidationException ex)
        {
            Console.WriteLine($" Validation Error: {ex.Message}");
            Console.WriteLine($"   Parameter: {ex.ParameterName}");
        }
        catch (ApiException ex)
        {
            Console.WriteLine($" API Error: {ex.Message}");
            Console.WriteLine($"   HTTP Status: {ex.StatusCode}");
            Console.WriteLine($"   Response Code: {ex.ResponseCode}");
            Console.WriteLine("\n   Note: Make sure you use a valid transaction ID from a previous transaction.");
        }
        catch (Liberte360PayException ex)
        {
            Console.WriteLine($" SDK Error: {ex.Message}");
        }
    }

    static async Task CheckoutExample(ILiberte360PayClient client)
    {
        Console.WriteLine("Example 9: Checkout Initialize");
        Console.WriteLine("----------------------------------\n");

        try
        {
            var email = "michael@example.com";
            var amount = 100.00m;
            var phoneNumber = "0246089019";
            var paymentSlug = "persol-store-payment";

            Console.WriteLine($"Initializing checkout session...");
            Console.WriteLine($" Email: {email}");
            Console.WriteLine($" Amount: GHS {amount:N2}");
            Console.WriteLine($" Phone: {phoneNumber}");
            Console.WriteLine($" Payment Slug: {paymentSlug}\n");

            var checkoutResponse = await client.Checkout()
                .WithEmail(email)
                .WithAmount(amount)
                .WithPhoneNumber(phoneNumber)
                .WithPaymentSlug(paymentSlug)
                .ExecuteAsync();

            if (checkoutResponse.IsSuccess)
            {
                Console.WriteLine(" Checkout Session Created!\n");
                Console.WriteLine($"   Access Code: {checkoutResponse.Data?.AccessCode}");
                Console.WriteLine($"   Payment URL: {checkoutResponse.Data?.PaymentUrl}");
                Console.WriteLine($"   Reference: {checkoutResponse.Data?.Reference}");
                Console.WriteLine($"\n   Redirect customer to: {checkoutResponse.Data?.PaymentUrl}");
            }
            else
            {
                Console.WriteLine($" Checkout Failed: {checkoutResponse.Message}");
                Console.WriteLine($" Code: {checkoutResponse.Code}");
                Console.WriteLine($" Status: {checkoutResponse.Status}");
            }
        }
        catch (ValidationException ex)
        {
            Console.WriteLine($" Validation Error: {ex.Message}");
            Console.WriteLine($" Parameter: {ex.ParameterName}");
        }
        catch (ApiException ex)
        {
            Console.WriteLine($" API Error: {ex.Message}");
            Console.WriteLine($" HTTP Status: {ex.StatusCode}");
            Console.WriteLine($" Response Code: {ex.ResponseCode}");
        }
        catch (Liberte360PayException ex)
        {
            Console.WriteLine($" SDK Error: {ex.Message}");
        }
    }

    static void DisplayMetrics(IMetricsCollector metricsCollector)
    {
        var metrics = metricsCollector.GetMetrics();

        if (metrics.Count == 0)
        {
            Console.WriteLine("No metrics collected yet.");
            return;
        }

        Console.WriteLine($"Total Endpoints Tracked: {metrics.Count}\n");

        foreach (var kvp in metrics)
        {
            var endpoint = kvp.Key;
            var metric = kvp.Value;

            Console.WriteLine($" {endpoint}");
            Console.WriteLine($"   Total Calls: {metric.TotalCalls}");
            Console.WriteLine($"   Successful: {metric.SuccessfulCalls} | Failed: {metric.FailedCalls}");

            if (metric.TotalCalls > 0)
            {
                var successRate = (double)metric.SuccessfulCalls / metric.TotalCalls * 100;
                Console.WriteLine($" Success Rate: {successRate:F2}%");
                
                if (metric.AverageDurationMs > 0)
                {
                    Console.WriteLine($" Average Duration: {metric.AverageDurationMs:F2}ms");
                }
                
                Console.WriteLine($" Avg Request Size: {metric.AverageRequestSize:N0} bytes");
                Console.WriteLine($" Avg Response Size: {metric.AverageResponseSize:N0} bytes");
            }

            if (metric.ErrorCount > 0)
            {
                Console.WriteLine($"  Errors: {metric.ErrorCount}");
                if (metric.ErrorCodes.Count > 0)
                {
                    Console.WriteLine($" Error Codes: {string.Join(", ", metric.ErrorCodes)}");
                }
            }

            Console.WriteLine();
        }
    }
}