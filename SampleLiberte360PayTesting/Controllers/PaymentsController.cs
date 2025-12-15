using Liberte360Pay;
using Liberte360Pay.Exceptions;
using Liberte360Pay.Models.BulkDisbursement;
using Liberte360Pay.Models.BulkNameVerify;
using Liberte360Pay.Models.Disbursement;
using Liberte360Pay.Models.NameVerify;
using Liberte360Pay.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SampleLiberteTesting.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PaymentsController : ControllerBase
    {
        private readonly ILiberte360PayClient _threeSixtyPayClient;
        private const string PHONE = "0246089019";

        public PaymentsController(ILiberte360PayClient threeSixtyPayClient)
        {
            _threeSixtyPayClient = threeSixtyPayClient;
        }

        [HttpPost("verify")]
        public async Task<IActionResult> VerifyAccount(CancellationToken cancellationToken)
        {
            try
            {
                var response = await _threeSixtyPayClient.NameVerify()
                    .WithAccountNumber(PHONE)
                    .WithInstitutionCode(InstitutionCode.Mtn)
                    .ExecuteAsync(cancellationToken);

                return Ok(new
                {
                    accountName = response.Data.AccountName,
                    accountNumber = response.Data.AccountNumber,
                    verified = true
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }


        [HttpPost("bulk-verify")]
        public async Task<IActionResult> BulkVerifyAccount(CancellationToken cancellationToken)
        { 
            try
            {

                var verificationItems = new List<BulkNameVerifyItem>
                {
                    new BulkNameVerifyItem
                    {
                        AccountNumber = PHONE,
                        InstitutionCode = InstitutionCode.Mtn.GetCodeString()
                    },
                    new BulkNameVerifyItem
                    {
                        AccountNumber = PHONE,  // Using same for demo - replace with different numbers
                        InstitutionCode = InstitutionCode.Mtn.GetCodeString() 
                    }
                };

                var response = await _threeSixtyPayClient.BulkNameVerify()
                    .WithVerifications(verificationItems) 
                    .ExecuteAsync(cancellationToken);

                if (response.IsSuccess)
                {
                    return Ok(response.Data);

                }

                return Ok(response.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }


        [HttpPost("disburse")]
        public async Task<IActionResult> Disburse(
        [FromQuery] decimal amount,
        CancellationToken cancellationToken)
        {
            try
            {

                var verifyResponse = await _threeSixtyPayClient.NameVerify()
                    .WithAccountNumber(PHONE)
                    .WithInstitutionCode(InstitutionCode.Mtn)
                    .ExecuteAsync(cancellationToken);

                if (!verifyResponse.IsSuccess)
                {
                    return StatusCode(StatusCodes.Status422UnprocessableEntity, $" Verification failed: {verifyResponse.Message}");
                }

                var transactionId = $"TXN-{DateTime.UtcNow.Ticks}";
                var reference = $"Disburse money with TXN-{DateTime.UtcNow.Ticks}";

                var response = await _threeSixtyPayClient.Disbursement()
                    .WithAccountName(verifyResponse.Data!.AccountName)
                    .WithAccountNumber(PHONE)
                    .WithAmount(amount)
                    .WithInstitutionCode(InstitutionCode.Mtn)
                    .WithTransactionId(transactionId)
                    .WithReference(reference)
                    .ExecuteAsync(cancellationToken);

                if (response.IsSuccess)
                {

                    return Ok(new
                    {
                        transactionId = response.Data.TransactionId,
                        message = response.Data.TransactionMessage,
                        status = response.Status,
                        code = response.Code
                    });
                }

                return Ok(response.Data.TransactionMessage);

            }
            catch (ValidationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (ApiException ex)
            {

                // Map common error codes
                switch (ex.ResponseCode)
                {
                    case "401":
                    case "407":
                        return BadRequest(new { error = $"Check your API key : {ex.Message}" });
                    case "404":
                        return BadRequest(new { error = $"Account not found : {ex.Message}" });
                    case "410":
                        return BadRequest(new { error = $"Insufficient funds : {ex.Message}" });
                    case "491":
                        return BadRequest(new { error = $"User needs to authorize on their phone : {ex.Message}" });
                     default:
                        return BadRequest(new { error = ex.Message });
                }
            }
            catch (Liberte360PayException ex)
            {
                return BadRequest(new { error = $"SDK Error: {ex.Message}" });
            }
        }

         
        [HttpPost("disburse-with-metadata")]
        public async Task<IActionResult> DisburseWithMetaData(
        [FromQuery] decimal amount,
        CancellationToken cancellationToken)
        {
            try
            {

                var verifyResponse = await _threeSixtyPayClient.NameVerify()
                    .WithAccountNumber(PHONE)
                    .WithInstitutionCode(InstitutionCode.Mtn)
                    .ExecuteAsync(cancellationToken);

                if (!verifyResponse.IsSuccess)
                {
                    return StatusCode(StatusCodes.Status422UnprocessableEntity, $" Verification failed: {verifyResponse.Message}");
                }

                var transactionId = $"TXN-{DateTime.UtcNow.Ticks}";
                var reference = $"Disburse money with TXN-{DateTime.UtcNow.Ticks}";

                var response = await _threeSixtyPayClient.Disbursement()
                    .WithAccountName(verifyResponse.Data!.AccountName)
                    .WithAccountNumber(PHONE)
                    .WithAmount(amount)
                    .WithInstitutionCode(InstitutionCode.Mtn)
                    .WithTransactionId(transactionId)
                    .WithReference(reference)
                    .AddMetaData("employee_id", "EMP12345")
                    .AddMetaData("department", "Engineering")
                    .AddMetaData("payment_type", "Bonus")
                    .ExecuteAsync(cancellationToken);

                if (response.IsSuccess)
                {

                    return Ok(new
                    {
                        transactionId = response.Data.TransactionId,
                        message = response.Data.TransactionMessage,
                        status = response.Status,
                        code = response.Code
                    });
                }

                return Ok(response.Data.TransactionMessage);

            }
            catch (ValidationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (ApiException ex)
            {

                // Map common error codes
                switch (ex.ResponseCode)
                {
                    case "401":
                    case "407":
                        return BadRequest(new { error = $"Check your API key : {ex.Message}" });
                    case "404":
                        return BadRequest(new { error = $"Account not found : {ex.Message}" });
                    case "410":
                        return BadRequest(new { error = $"Insufficient funds : {ex.Message}" });
                    case "491":
                        return BadRequest(new { error = $"User needs to authorize on their phone : {ex.Message}" });
                    default:
                        return BadRequest(new { error = ex.Message });
                }
            }
            catch (Liberte360PayException ex)
            {
                return BadRequest(new { error = $"SDK Error: {ex.Message}" });
            }
        }

        [HttpPost("disburse-bulk")]
        public async Task<IActionResult> BulkDisburse(
        [FromQuery] decimal amount,
        CancellationToken cancellationToken)
        {
            try
            {

                var disbursementItems = new List<BulkDisbursementItem>
                {
                    new BulkDisbursementItem
                    {
                        AccountName = "ENOCH DANSO CLINTON", // you should have a service that does verification seperatly
                        AccountNumber = PHONE,
                        Amount = 50.00m,
                        InstitutionCode = InstitutionCode.Mtn.GetCodeString(),
                        TransactionId = $"TXN-{DateTime.UtcNow.Ticks}",
                        Currency = "GHS",
                        Reference = "Bulk Payment - Recipient 1",
                        MetaData = new Dictionary<string, string>
                        {
                            { "batch_id", "BATCH-001" }
                        }
                    },
                    new BulkDisbursementItem
                    {
                        AccountName = "ENOCH DANSO CLINTON",
                        AccountNumber = PHONE,
                        Amount = 75.00m,
                        InstitutionCode = InstitutionCode.Mtn.GetCodeString(),
                        TransactionId = $"TXN-{DateTime.UtcNow.Ticks}",
                        Currency = "GHS",
                        Reference = "Bulk Payment - Recipient 2",
                        MetaData = new Dictionary<string, string>
                        {
                            { "batch_id", "BATCH-001" }
                        }
                    }
                };


                var response = await _threeSixtyPayClient.BulkDisbursement()
                    .WithDisbursements(disbursementItems)
                    .ExecuteAsync(cancellationToken);

                if (response.IsSuccess)
                {

                    return Ok(response.Data);
                }

                return Ok(response.Message);

            }
            catch (ValidationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (ApiException ex)
            {

                // Map common error codes
                switch (ex.ResponseCode)
                {
                    case "401":
                    case "407":
                        return BadRequest(new { error = $"Check your API key : {ex.Message}" });
                    case "404":
                        return BadRequest(new { error = $"Account not found : {ex.Message}" });
                    case "410":
                        return BadRequest(new { error = $"Insufficient funds : {ex.Message}" });
                    case "491":
                        return BadRequest(new { error = $"User needs to authorize on their phone : {ex.Message}" });
                    default:
                        return BadRequest(new { error = ex.Message });
                }
            }
            catch (Liberte360PayException ex)
            {
                return BadRequest(new { error = $"SDK Error: {ex.Message}" });
            }
        }


        [HttpPost("debit")]
        public async Task<IActionResult> Debit( 
        [FromQuery] decimal amount,
        CancellationToken cancellationToken)
        {
            try
            {

                var verifyResponse = await _threeSixtyPayClient.NameVerify()
                    .WithAccountNumber(PHONE)
                    .WithInstitutionCode(InstitutionCode.Mtn)
                    .ExecuteAsync(cancellationToken);

                if (!verifyResponse.IsSuccess)
                {
                    return StatusCode(StatusCodes.Status422UnprocessableEntity, $" Verification failed: {verifyResponse.Message}");
                }

                var transactionId = $"D-{DateTime.UtcNow.Ticks}";
                var reference = $"debit money from account";

                var response = await _threeSixtyPayClient.Collection()
                    .WithAccountName(verifyResponse.Data!.AccountName)
                    .WithAccountNumber(PHONE)
                    .WithAmount(amount)
                    .WithInstitutionCode(InstitutionCode.Mtn)
                    .WithTransactionId(transactionId)
                    .WithReference(reference)
                    .AddMetaData("invoice_id", "INV-001")
                    .AddMetaData("customer_id", "CUST-001")
                    .ExecuteAsync(cancellationToken);

                if (response.IsSuccess)
                {

                    return Ok(response.Data);
                }

                return Ok(response.Data.TransactionMessage);

            }
            catch (ValidationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (ApiException ex)
            {

                // Map common error codes
                switch (ex.ResponseCode)
                {
                    case "401":
                    case "407":
                        return BadRequest(new { error = $"Check your API key : {ex.Message}" });
                    case "404":
                        return BadRequest(new { error = $"Account not found : {ex.Message}" });
                    case "410":
                        return BadRequest(new { error = $"Insufficient funds : {ex.Message}" });
                    case "491":
                        return BadRequest(new { error = $"User needs to authorize on their phone : {ex.Message}" });
                    default:
                        return BadRequest(new { error = ex.Message });
                }
            }
            catch (Liberte360PayException ex)
            {
                return BadRequest(new { error = $"SDK Error: {ex.Message}" });
            }
        }


        [HttpPost("transaction-status/{transactionId}")]
        public async Task<IActionResult> CheckTransactionStatus(string transactionId, CancellationToken cancellationToken)
        {
            try
            {
                var statusResponse = await _threeSixtyPayClient.TransactionStatus()
                    .WithTransactionId(transactionId)
                    .ExecuteAsync();

                if (statusResponse.IsSuccess)
                {
                    return Ok(statusResponse.Data);
                }

                return Ok(statusResponse.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }



        [HttpPost("bulk-transaction-status/{bulkTransactionId}")]
        public async Task<IActionResult> CheckBulkTransactionStatus(string bulkTransactionId, CancellationToken cancellationToken)
        {
            try
            {
                var statusResponse = await _threeSixtyPayClient.BulkDisbursementStatus()
                    .WithBulkTransactionId(bulkTransactionId)
                    .ExecuteAsync();

                if (statusResponse.IsSuccess)
                {
                    return Ok(statusResponse.Data);
                }

                return Ok(statusResponse.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("transaction-status/{reference}/reference")]
        public async Task<IActionResult> CheckTransactionByReferenceStatus(string reference, CancellationToken cancellationToken)
        {
            try
            {
                var statusResponse = await _threeSixtyPayClient.TransactionStatusByReference()
                    .WithReference(reference)
                    .ExecuteAsync();

                if (statusResponse.IsSuccess)
                {
                    return Ok(statusResponse.Data);
                }

                return Ok(statusResponse.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }


        [HttpPost("checkout-init")]
        public async Task<IActionResult> Checkout(
            string email, decimal amount, string slug ,
            CancellationToken cancellationToken)
        {
            try
            {
                var response = await _threeSixtyPayClient.Checkout()
                    .WithEmail(email)
                    .WithAmount(amount)
                    .WithPhoneNumber(PHONE)
                    .WithPaymentSlug(slug)
                    .ExecuteAsync(cancellationToken);

                if (response.IsSuccess)
                {
                    return Ok(response.Data);
                }

                return Ok(response.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
