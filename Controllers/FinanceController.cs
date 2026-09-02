using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.SqlClient;
using System.Web.Services.Description;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OBMS.WebAPI.BusinessObjects;
using OBMS.WebAPI.Models;
using OBMS.WebAPI.Models.Domain;
using OBMS.WebAPI.Models.DTO;
using OBMS.WebAPI.Repositories.Interface;
using SkiaSharp;



namespace OBMS.WebAPI.Controllers

{

    [Route("api/[controller]")]

    [ApiController]

    public class FinanceController : Controller

    {



        HttpResponseMessage response = new HttpResponseMessage();

        private readonly OBMSDbContext _oBMSDbContext;

        private readonly IFinanceRepository _financeRepository;

        private readonly ILogger<FinanceController> _logger;



        public FinanceController(IFinanceRepository financeRepository, OBMSDbContext oBMSDbContext, ILogger<FinanceController> logger)

        {

            _financeRepository = financeRepository;

            _oBMSDbContext = oBMSDbContext;

            _logger = logger;

        }



        [HttpGet]

        [Route("GetInvoiceMaster")]

        public async Task<ActionResult<Object>> GetInvoiceMaster(string userID)

        {

            try

            {

                var obj = await _financeRepository.GetInvoiceMaster(userID);



                return obj;

            }

            catch (Exception ex)

            {

                throw;

            }



        }



        [HttpGet]

        [Route("GetInvoiceClientByBranchAndInvoiceDate")]

        public async Task<ActionResult<List<QueryResult>>> GetInvoiceClientByBranchAndInvoiceDate(string branchId, DateTime invoicePeriod)

        {

            var sqlQuery = @"

    SELECT

        cm.Code,

        cm.Branch,

        cm.Name,

        ISNULL(inv.ID, 0) AS ID,

        ISNULL(inv.InvoiceNo, '') AS InvoiceNo

    FROM

        ClientMaster cm

    FULL OUTER JOIN (

        SELECT

            ci.Branch,

            ci.Client,

            ci.ID,

            ci.InvoiceNo

        FROM

            ClientInvoice ci

        WHERE

            ci.IsDeleted = 'N'

            AND MONTH(ci.InvoiceDate) = @InvoiceMonth

            AND YEAR(ci.InvoiceDate) = @InvoiceYear

    ) inv ON cm.Branch = inv.Branch AND cm.Code = inv.Client

    WHERE

        cm.Branch = @Branch

        AND cm.Code IN (

            SELECT

                a.Client

            FROM

                Agreement a

            WHERE

                a.Branch = @Branch

                AND (

                    a.AgreementEndDate >= DATEADD(month, DATEDIFF(month, 0, '04/30/2016'), 0)

                    OR DATEADD(s, -1, DATEADD(mm, DATEDIFF(m, 0, '04/30/2016') + 1, 0)) >= a.AgreementEndDate

                )

                AND a.Client NOT IN (

                    SELECT

                        ta.Client

                    FROM

                        TerminatedAgreements ta

                    WHERE

                        ta.Branch = @Branch

                        AND ta.TerminationDate < DATEADD(month, DATEDIFF(month, 0, @InvoicePeriod), 0)

                )

        )

";



            var parameters = new[]

            {

    new Microsoft.Data.SqlClient.SqlParameter("@Branch", branchId),

    new Microsoft.Data.SqlClient.SqlParameter("@InvoiceMonth", invoicePeriod.Month),

    new Microsoft.Data.SqlClient.SqlParameter("@InvoiceYear", invoicePeriod.Year),

    new Microsoft.Data.SqlClient.SqlParameter("@InvoicePeriod", invoicePeriod),

};



            var result = await _oBMSDbContext.QueryResults.FromSqlRaw(sqlQuery, parameters).ToListAsync();







            return result;

        }



        [HttpGet]

        [Route("GetBatchInvoiceClientByBranchAndInvoiceDate")]

        public async Task<ActionResult<Object>> GetBatchInvoiceClientByBranchAndInvoiceDate(string branchId, DateTime invoicePeriod)

        {

            try

            {

                var sqlQuery = @"

    SELECT

        cm.Code,

        cm.Branch,

        cm.Name,

        ISNULL(inv.ID, 0) AS ID,

        ISNULL(inv.InvoiceNo, '') AS InvoiceNo

    FROM

        ClientMaster cm

    FULL OUTER JOIN (

        SELECT

            ci.Branch,

            ci.Client,

            ci.ID,

            ci.InvoiceNo

        FROM

            ClientInvoice ci

        WHERE

            ci.IsDeleted = 'N'

            AND MONTH(ci.InvoiceDate) = @InvoiceMonth

            AND YEAR(ci.InvoiceDate) = @InvoiceYear

    ) inv ON cm.Branch = inv.Branch AND cm.Code = inv.Client

    WHERE

        cm.Branch = @Branch

        AND cm.Code IN (

            SELECT

                a.Client

            FROM

                Agreement a

            WHERE

                a.Branch = @Branch

                AND (

                    a.AgreementEndDate >= DATEADD(month, DATEDIFF(month, 0, '04/30/2016'), 0)

                    OR DATEADD(s, -1, DATEADD(mm, DATEDIFF(m, 0, '04/30/2016') + 1, 0)) >= a.AgreementEndDate

                )

                AND a.Client NOT IN (

                    SELECT

                        ta.Client

                    FROM

                        TerminatedAgreements ta

                    WHERE

                        ta.Branch = @Branch

                        AND ta.TerminationDate < DATEADD(month, DATEDIFF(month, 0, @InvoicePeriod), 0)

                )

        )

";



                var parameters = new[]

                {

    new Microsoft.Data.SqlClient.SqlParameter("@Branch", branchId),

    new Microsoft.Data.SqlClient.SqlParameter("@InvoiceMonth", invoicePeriod.Month),

    new Microsoft.Data.SqlClient.SqlParameter("@InvoiceYear", invoicePeriod.Year),

    new Microsoft.Data.SqlClient.SqlParameter("@InvoicePeriod", invoicePeriod),

};



                var result = await _oBMSDbContext.QueryResults.FromSqlRaw(sqlQuery, parameters).ToListAsync();



                List<BatchInvoice> batchInvoices = new List<BatchInvoice>();

                foreach (var item in result)

                {

                    BatchInvoice bi = new BatchInvoice();

                    bi.InvoiceNo = item.InvoiceNo;

                    bi.Branch = item.Branch;

                    bi.Code = item.Code;

                    bi.ID = item.ID;

                    bi.Name = item.Name;



                    if (item.ID == 0)

                    {

                        var ag = GetAgreementAndDetailsByBranchInvoicePeriodAndClient(item.Branch, invoicePeriod, item.Code);

                        if (ag.Result.Result is BadRequestObjectResult || ag.Result.Value == null)

                        {

                            // Skip this client if agreement doesn't exist for the period

                            continue;

                        }

                        bi.data = ag.Result.Value;

                    }

                    else

                    {



                        var ag = GetInvoiceAndDetailsByInvoiceId(item.ID);

                        if (ag.Result.Result is NotFoundObjectResult || ag.Result.Value == null)

                        {

                            // Skip this client if invoice data cannot be retrieved

                            continue;

                        }

                        bi.data = ag.Result.Value;

                    }

                    batchInvoices.Add(bi);



                }



                var results = new Dictionary<string, Object>();

                results.Add("batchInvoice", batchInvoices);



                return results;

            }

            catch (Exception ex)

            {

                return StatusCode(500, "Internal server error");

            }

        }



        [HttpGet]

        [Route("GetAgreementAndDetailsByBranchInvoicePeriodAndClient")]

        public async Task<ActionResult<Object>> GetAgreementAndDetailsByBranchInvoicePeriodAndClient(string branchId, DateTime invoicePeriod, string clientId)



        {



            // oAgreement 



            var sqlQuery = @"SELECT TOP 1 ID,Branch,Client,WorkPlace,AgreementDate,Note,IsValid,LASTUPDATE,AgreementEndDate,LastUpdatedBy,AddedDate,QuotationID 
                            FROM Agreement 
                            WHERE Branch=@Branch AND Client=@Client 
                            AND AgreementDate <= @AgreementPeriod 
                            AND (IsValid = 0 OR IsValid IS NULL OR (IsValid = 1 AND FORMAT(AgreementDate, 'yyyyMM') = FORMAT(@AgreementPeriod, 'yyyyMM')))
                            ORDER BY AgreementDate DESC, LASTUPDATE DESC ";




            var parameters = new[]

                {

    new Microsoft.Data.SqlClient.SqlParameter("@Branch", branchId),

    new Microsoft.Data.SqlClient.SqlParameter("@Client", clientId),

    new Microsoft.Data.SqlClient.SqlParameter("@AgreementPeriod", invoicePeriod),

};

            var result = await _oBMSDbContext.Agreements.FromSqlRaw(sqlQuery, parameters).FirstOrDefaultAsync();

            //AgreementDetails

            var results = new Dictionary<string, Object>();

            if (result == null)
            {
                return BadRequest(new { error = "Agreement for this period does not exist. Unable to process invoice." });
            }

            results.Add("agreement", result);





            if (result != null)

            {

                var ad = _oBMSDbContext.AgreementDetails.Where(x => x.AgreementID == result.ID).ToList();

                results.Add("agreementDetails", ad);

            }







            if (invoicePeriod >= Convert.ToDateTime("2015-04-01 00:00:00.000"))

            {

                var sqlQuery1 = @"SELECT ISNULL(MAX(CAST(INVOICENO AS INT))+1, 1) AS NEWCLIENTINVOICENO FROM CLIENTINVOICE WHERE BRANCH=@Branch AND InvoiceDate >= '2015-04-01T00:00:00.000'";



                var parameters1 = new[]

                {

    new Microsoft.Data.SqlClient.SqlParameter("@Branch", branchId)

};



                var result1 = await _oBMSDbContext.ClientInvoiceNoResult
                    .FromSqlRaw(sqlQuery1, parameters1)
                    .FirstOrDefaultAsync();





                results.Add("invoiceNo", result1.NEWCLIENTINVOICENO);

            }

            else

            {

                var sqlQuery1 = @" SELECT ISNULL(MAX(CAST(INVOICENO AS INT))+1,1) AS NEWCLIENTINVOICENO FROM CLIENTINVOICE WHERE BRANCH=@BranchCode AND InvoiceDate < '2015-04-01 00:00:00.000'";



                var parameters1 = new[]

                 {

                    new Microsoft.Data.SqlClient.SqlParameter("@Branch", branchId)

                 };



                var result1 = await _oBMSDbContext.ClientInvoiceNoResult
                   .FromSqlRaw(sqlQuery1, parameters1)
                   .FirstOrDefaultAsync();





                results.Add("invoiceNo", result1.NEWCLIENTINVOICENO);

            }



            return results;



        }



        [HttpGet]

        [Route("GetInvoiceAndDetailsByInvoiceId")]

        public async Task<ActionResult<Object>> GetInvoiceAndDetailsByInvoiceId(int invoiceId)

        {

            try

            {

                // Get invoice data

                var invoice = await _oBMSDbContext.ClientInvoices
                    .Where(x => x.ID == invoiceId)
                    .FirstOrDefaultAsync();



                if (invoice == null)

                {

                    return NotFound(new { error = "Invoice not found" });

                }



                // Prepare data rows with a left join to handle missing agreement details gracefully

                // and fetch HSN codes from ServiceType based on Category

                var detailQuery = from d in _oBMSDbContext.ClientInvoiceDetails

                                  join ad in _oBMSDbContext.AgreementDetails on d.AgreementDetailID equals ad.ID into adGroup

                                  from ad in adGroup.DefaultIfEmpty()

                                  join st in _oBMSDbContext.ServiceTypes on ad.Category equals st.ServiceName into stGroup

                                  from st in stGroup.DefaultIfEmpty()

                                  where d.ClientInvoiceID == invoiceId

                                  select new
                                  {

                                      d.ID,

                                      d.ClientInvoiceID,

                                      d.AgreementDetailID,

                                      d.AgreementID,

                                      d.AgreementDate,

                                      d.NoOfGuards,

                                      d.Rate,

                                      d.NoOfHours,

                                      d.NoOfDays,

                                      d.FollowCalender,

                                      d.HasDiscount,

                                      d.DiscountAmount,

                                      d.IsTaxable,

                                      d.TaxAmount,

                                      d.MonthTotal,

                                      Description = !string.IsNullOrEmpty(d.Description) ? d.Description : (ad != null ? ad.Description : "Security Services"),

                                      hsnSacCode = st != null ? st.HSNCode : null,

                                  };



                var detailsList = await detailQuery.ToListAsync();



                if (!detailsList.Any())

                {

                    // Fallback to basic details if join fails

                    var basicDetails = await _oBMSDbContext.ClientInvoiceDetails
                        .Where(x => x.ClientInvoiceID == invoiceId)
                        .ToListAsync();



                    if (!basicDetails.Any())
                    {

                        return NotFound(new { error = "No details found for this invoice" });

                    }



                    // Transform to the expected format

                    detailsList = basicDetails.Select(d => new
                    {

                        d.ID,
                        d.ClientInvoiceID,
                        d.AgreementDetailID,
                        d.AgreementID,
                        d.AgreementDate,

                        d.NoOfGuards,
                        d.Rate,
                        d.MonthTotal,
                        d.NoOfDays,
                        d.NoOfHours,
                        d.TaxAmount,
                        d.IsTaxable,

                        d.FollowCalender,
                        d.HasDiscount,
                        d.DiscountAmount,

                        Description = "Security Services",
                        hsnSacCode = "9985"

                    }).ToList() as dynamic;

                }



                // Get agreement information

                int targetAgreementId = invoice.AgreementID != 0 ? invoice.AgreementID : 1;

                var agreement = await _oBMSDbContext.Agreements
                    .Where(x => x.ID == targetAgreementId)
                    .FirstOrDefaultAsync();



                // Get client and company information

                var client = await _oBMSDbContext.ClientMasters
                    .Where(x => x.Code == invoice.Client && x.Branch == invoice.Branch)
                    .FirstOrDefaultAsync();



                var company = await _oBMSDbContext.BranchMasters
                    .Where(x => x.Code == invoice.Branch)
                    .FirstOrDefaultAsync();



                // Calculate subtotal and total tax from database values

                var subtotal = detailsList.Sum(d => (decimal)d.MonthTotal);

                var totalTaxFromDb = detailsList.Sum(d => (decimal)d.TaxAmount);



                // Determine if intra-state (CGST+SGST) or inter-state (IGST)

                var branchState = company?.State?.Trim() ?? "";

                var clientBillingState = client?.BillingState?.Trim() ?? client?.State?.Trim() ?? "";

                // Debug logging
                _logger.LogInformation($"GST Calculation Debug - Branch State: '{branchState}', Client Billing State: '{clientBillingState}'");

                var isIntraState = !string.IsNullOrEmpty(branchState) &&

                                   !string.IsNullOrEmpty(clientBillingState) &&

                                   branchState.Equals(clientBillingState, StringComparison.OrdinalIgnoreCase);

                _logger.LogInformation($"GST Calculation Debug - Is Intra-State: {isIntraState}");



                // Calculate tax based on state: 18% GST

                decimal cgstAmount = 0;

                decimal sgstAmount = 0;

                decimal igstAmount = 0;

                decimal cgstPct = 0;

                decimal sgstPct = 0;

                decimal igstPct = 0;



                if (subtotal > 0)

                {

                    if (isIntraState)

                    {

                        // Same state: CGST 9% + SGST 9% = 18%

                        cgstAmount = subtotal * 0.09M;

                        sgstAmount = subtotal * 0.09M;

                        cgstPct = 9;

                        sgstPct = 9;

                    }

                    else

                    {

                        // Different state: IGST 18%

                        igstAmount = subtotal * 0.18M;

                        igstPct = 18;

                    }

                }



                var totalTax = cgstAmount + sgstAmount + igstAmount;

                var grandTotal = subtotal + totalTax;

                // Debug logging for final GST amounts
                _logger.LogInformation($"GST Calculation Debug - Final Amounts - CGST: {cgstAmount}, SGST: {sgstAmount}, IGST: {igstAmount}, TotalTax: {totalTax}, GrandTotal: {grandTotal}");



                // Create addresses - shipping falls back to WorkPlace from Agreement if missing

                var companyAddress = $"{company?.Address1 ?? ""} {(company?.Address2 ?? "")}".Trim();

                var clientBillingAddress = $"{client?.Address1 ?? ""} {(client?.Address2 ?? "")}".Trim();

                var rawShippingAddr = $"{client?.ShippingAddress1 ?? ""} {(client?.ShippingAddress2 ?? "")}".Trim();

                var clientShippingAddress = !string.IsNullOrEmpty(rawShippingAddr) ? rawShippingAddr : (agreement?.WorkPlace ?? clientBillingAddress);



                // Get GST Registration Status if available

                var gstStatus = client?.GSTRegistrationStatus ?? "Regular";



                var dataRows = detailsList.Select((d, index) =>
                {

                    var finalDescription = d.Description;

                    if (d.NoOfHours > 0)
                    {

                        finalDescription += $" ({d.NoOfHours:0} Hrs)";

                    }



                    // Display quantity as Number of Guards and Rate as Monthly Rate per Guard

                    // to ensure mathematical consistency (Qty * Rate = Amount) in the UI columns.

                    // Display quantity as Number of Guards (matching Agreement Export)

                    decimal displayedQty = (decimal)d.NoOfGuards;

                    // Guard against division by zero (Lump Sum has NoOfGuards = 0)
                    decimal displayedRate = displayedQty > 0
                        ? Math.Round((decimal)d.MonthTotal / displayedQty, 2)
                        : (decimal)d.MonthTotal;



                    // Calculate tax for each row using stored TaxAmount from AgreementDetails

                    decimal rowTax = d.IsTaxable ? (decimal)d.TaxAmount : 0;

                    return new
                    {

                        sno = index + 1,

                        hsnSacCode = d.hsnSacCode,

                        description = finalDescription,

                        dutiesTaxes = d.NoOfDays.ToString("0"), // Days/Duties

                        units = displayedQty, // Number of Guards

                        rate = displayedRate, // Monthly Rate per Guard

                        amount = d.MonthTotal,

                        isTaxable = d.IsTaxable,

                        cgstAmount = isIntraState ? (rowTax / 2) : 0,

                        sgstAmount = isIntraState ? (rowTax / 2) : 0,

                        igstAmount = !isIntraState ? rowTax : 0

                    };

                }).ToList();



                var results = new

                {

                    documentType = "TAX INVOICE (Original for Recipient)",

                    invoice = new

                    {

                        // Formatted values for reports/UI rendering

                        invoiceNoFormatted = $"FWGCHE/{invoice.InvoiceNo ?? "001"}/{(invoice.InvoiceDate.Month >= 4 ? invoice.InvoiceDate.Year % 100 : (invoice.InvoiceDate.Year - 1) % 100):D2}-{(invoice.InvoiceDate.Month >= 4 ? (invoice.InvoiceDate.Year + 1) % 100 : invoice.InvoiceDate.Year % 100):D2}",

                        invoiceNoOriginal = invoice.InvoiceNo ?? "001",

                        invoiceDateFormatted = invoice.InvoiceDate.ToString("dd/MM/yyyy"),

                        servicePeriod = invoice.InvoiceDate.ToString("MMMM yyyy"),

                        // Fixed Work Order details per client
                        workOrderNoFormatted = GetWorkOrderNo(client?.Code ?? ""),

                        workOrderNoOriginal = GetWorkOrderNo(client?.Code ?? ""),

                        workOrderDate = GetWorkOrderDate(client?.Code ?? ""),

                        sacCode = detailsList.FirstOrDefault()?.hsnSacCode ?? "9985",

                        // Legacy support for basic invoice structure

                        ID = invoice.ID,

                        InvoiceNo = invoice.InvoiceNo,

                        InvoiceDate = invoice.InvoiceDate,

                        Note = invoice.Note,

                        Subject = invoice.Subject,

                        Branch = invoice.Branch,

                        Client = invoice.Client,

                        AgreementID = invoice.AgreementID

                    },

                    company = new

                    {

                        name = company?.Name ?? "",

                        tagline = company?.ShortName ?? "",

                        address = companyAddress,

                        phone = company?.Phone ?? "",

                        email = company?.Email ?? "",

                        gstin = company?.GSTIN ?? "",

                        pan = company?.PANNumber ?? "",

                        esic = company?.ESICAccountNumber ?? "",

                        epf = company?.EPFAccountNumber ?? "",

                        bankName = company?.BankName ?? "",

                        bankAccount = company?.BankAccount ?? "",

                        ifscCode = company?.BankBranch ?? "",

                        bankBranch = company?.BankBranch ?? "",

                        state = branchState,

                        cinNo = company?.TANNumber ?? "",

                        website = "www.fwgsecurity.com",

                        globalOfficeAddress = "No. 23 & 24, Taman Bukit Emas, Jalan Tampin, 70450 Seremban, Negeri Sembilan, Malaysia",

                        globalOfficePhone = "+60 6 677 2000",

                        globalOfficeFax = "+60 6 677 9866"

                    },

                    client = new

                    {

                        billingName = client?.Name ?? "",

                        billingAddress = clientBillingAddress,

                        billingGSTIN = client?.GSTIN ?? "",

                        billingState = clientBillingState,

                        shippingName = client?.Name ?? "",

                        shippingAddress = clientShippingAddress,

                        shippingGSTIN = client?.GSTIN ?? "",

                        shippingState = client?.State ?? "",

                        gstRegistrationStatus = gstStatus

                    },

                    totals = new

                    {

                        subtotal = subtotal,

                        cgstAmount = cgstAmount,

                        sgstAmount = sgstAmount,

                        igstAmount = igstAmount,

                        cgstPct = cgstPct > 0 ? cgstPct : 9,

                        sgstPct = sgstPct > 0 ? sgstPct : 9,

                        igstPct = igstPct > 0 ? igstPct : 18,

                        grandTotal = grandTotal,

                        amountInWords = NumberToWords(grandTotal),

                        taxType = isIntraState ? "CGST+SGST (Intra-State)" : "IGST (Inter-State)",

                        isIntraState = isIntraState

                    },

                    statutory = new

                    {

                        pan = company?.PANNumber ?? "",

                        gstin = company?.GSTIN ?? "",

                        esic = company?.ESICAccountNumber ?? "",

                        epf = company?.EPFAccountNumber ?? ""

                    },

                    dataRows = dataRows,

                    details = detailsList, // Alias for backward compatibility with frontend calculation logic

                    metadata = new

                    {

                        generatedDate = DateTime.Now.ToString("dd/MM/yyyy HH:mm"),

                        reportTitle = "Tax Invoice",

                        totalItems = dataRows.Count,

                        totalUnits = dataRows.Sum(x => (decimal)x.units)

                    }

                };



                return results;

            }

            catch (Exception ex)

            {

                return StatusCode(500, new { error = "Internal server error", message = ex.Message });

            }

        }



        [HttpGet]

        [Route("GetTaxInvoiceReportData")]

        public async Task<ActionResult<object>> GetTaxInvoiceReportData([FromQuery] int invoiceId)

        {

            try

            {

                // Get invoice data

                var invoice = await _oBMSDbContext.ClientInvoices
                    .Where(x => x.ID == invoiceId)
                    .FirstOrDefaultAsync();



                if (invoice == null)

                {

                    return NotFound(new { error = "Invoice not found" });

                }



                // Prepare data rows with a left join to handle missing agreement details gracefully

                var detailQuery = from d in _oBMSDbContext.ClientInvoiceDetails

                                  join ad in _oBMSDbContext.AgreementDetails on d.AgreementDetailID equals ad.ID into adGroup

                                  from ad in adGroup.DefaultIfEmpty()

                                  join st in _oBMSDbContext.ServiceTypes on ad.Category equals st.ServiceName into stGroup

                                  from st in stGroup.DefaultIfEmpty()

                                  where d.ClientInvoiceID == invoiceId

                                  select new

                                  {

                                      d.NoOfGuards,

                                      d.Rate,

                                      d.MonthTotal,

                                      d.NoOfDays,

                                      d.NoOfHours,

                                      d.TaxAmount,

                                      d.IsTaxable,

                                      HasDiscount = ad != null ? ad.HasDiscount : false,

                                      DiscountAmount = ad != null ? ad.DiscountAmount : 0,

                                      DiscountHour = ad != null ? ad.DiscountHour : 0,

                                      Description = ad != null ? ad.Description : "Security Services",

                                      hsnSacCode = st != null ? (st.HSNCode ?? "998715") : "998715"

                                  };



                var detailsList = await detailQuery.ToListAsync();



                if (!detailsList.Any())

                {

                    return NotFound(new { error = "No details found for this invoice" });

                }



                // Get agreement information

                int reportTargetAgreementId = invoice.AgreementID != 0 ? invoice.AgreementID : 1;

                var agreement = await _oBMSDbContext.Agreements

                    .Where(x => x.ID == reportTargetAgreementId)

                    .FirstOrDefaultAsync();



                // Get client and company information

                var client = await _oBMSDbContext.ClientMasters

                    .Where(x => x.Code == invoice.Client && x.Branch == invoice.Branch)

                    .FirstOrDefaultAsync();



                var company = await _oBMSDbContext.BranchMasters

                    .Where(x => x.Code == invoice.Branch)

                    .FirstOrDefaultAsync();



                // Calculate subtotal and total tax from database values

                // Use MonthTotal to match UI calculation (instead of recalculating from raw values)

                var subtotal = detailsList.Sum(d => (decimal)d.MonthTotal);



                // Calculate total discount duties (days) from agreement details

                // Discount reduces the number of duties/days, not the monetary amount

                var totalDiscountDuties = detailsList.Where(d => d.HasDiscount).Sum(d => d.DiscountHour > 0 ? d.DiscountHour : 0);



                // Recalculate taxable value based on reduced duties

                // Original: Rate * Days = Amount

                // With discount: Rate * (Days - DiscountDays) = NewAmount

                var taxableValue = detailsList.Sum(d => {

                    var originalDays = d.NoOfDays;

                    var discountDays = (d.HasDiscount && d.DiscountHour > 0) ? d.DiscountHour : 0;

                    var adjustedDays = Math.Max(0, originalDays - discountDays);

                    var ratePerDay = originalDays > 0 ? (decimal)d.MonthTotal / originalDays : 0;

                    return ratePerDay * adjustedDays;

                });



                var totalDiscountAmount = subtotal - taxableValue;



                var totalTaxFromDb = detailsList.Sum(d => (decimal)d.TaxAmount);



                // Debug logging

                _logger.LogInformation($"GST Calculation Debug - Subtotal: {subtotal}, TotalDiscountDuties: {totalDiscountDuties}, TotalDiscountAmount: {totalDiscountAmount}, TaxableValue: {taxableValue}, Details Count: {detailsList.Count}");



                if (detailsList.Count > 0)

                {

                    var firstDetail = detailsList.First();

                    _logger.LogInformation($"GST Calculation Debug - First Detail - Rate: {firstDetail.Rate}, NoOfGuards: {firstDetail.NoOfGuards}, NoOfHours: {firstDetail.NoOfHours}, NoOfDays: {firstDetail.NoOfDays}, HasDiscount: {firstDetail.HasDiscount}, DiscountHour: {firstDetail.DiscountHour}");

                }



                // Determine if intra-state (CGST+SGST) or inter-state (IGST)

                var branchState = company?.State?.Trim() ?? "";

                var clientBillingState = client?.BillingState?.Trim() ?? client?.State?.Trim() ?? "";



                // Debug logging

                _logger.LogInformation($"GST Calculation Debug - Branch State: '{branchState}', Client Billing State: '{clientBillingState}'");



                var isIntraState = !string.IsNullOrEmpty(branchState) &&

                                   !string.IsNullOrEmpty(clientBillingState) &&

                                   branchState.Equals(clientBillingState, StringComparison.OrdinalIgnoreCase);



                _logger.LogInformation($"GST Calculation Debug - Is Intra-State: {isIntraState}");



                // Calculate tax based on state: 18% GST on taxable value (after discount)

                decimal cgstAmount = 0;

                decimal sgstAmount = 0;

                decimal igstAmount = 0;

                decimal cgstPct = 0;

                decimal sgstPct = 0;

                decimal igstPct = 0;



                if (taxableValue > 0)

                {

                    if (isIntraState)

                    {

                        // Same state: CGST 9% + SGST 9% = 18% on taxable value

                        cgstAmount = taxableValue * 0.09M;

                        sgstAmount = taxableValue * 0.09M;

                        cgstPct = 9;

                        sgstPct = 9;

                    }

                    else

                    {

                        // Different state: IGST 18% on taxable value

                        igstAmount = taxableValue * 0.18M;

                        igstPct = 18;

                    }

                }



                var totalTax = cgstAmount + sgstAmount + igstAmount;

                var grandTotal = taxableValue + totalTax;



                // Debug logging for final GST amounts

                _logger.LogInformation($"GST Calculation Debug - Final Amounts - Subtotal: {subtotal}, TotalDiscount: {totalDiscountAmount}, TaxableValue: {taxableValue}, CGST: {cgstAmount}, SGST: {sgstAmount}, IGST: {igstAmount}, TotalTax: {totalTax}, GrandTotal: {grandTotal}");



                // Create addresses - shipping falls back to WorkPlace from Agreement if missing

                var companyAddress = $"{company?.Address1 ?? ""} {(company?.Address2 ?? "")}".Trim();

                var clientBillingAddress = $"{client?.Address1 ?? ""} {(client?.Address2 ?? "")}".Trim();

                var rawShippingAddr = $"{client?.ShippingAddress1 ?? ""} {(client?.ShippingAddress2 ?? "")}".Trim();

                var clientShippingAddress = !string.IsNullOrEmpty(rawShippingAddr) ? rawShippingAddr : (agreement?.WorkPlace ?? clientBillingAddress);



                // Get GST Registration Status if available

                var gstStatus = client?.GSTRegistrationStatus ?? "Regular";



                var dataRows = detailsList.Select((d, index) =>

                {

                    var finalDescription = d.Description;



                    if (d.NoOfHours > 0)

                    {

                        finalDescription += $" ({d.NoOfHours:0} Hrs)";

                    }



                    // Add discount information to description if applicable

                    if (d.HasDiscount && d.DiscountHour > 0)

                    {

                        finalDescription += $" (Discount: {d.DiscountHour} days)";

                    }



                    // Display quantity as Number of Guards and Rate as Monthly Rate per Guard

                    // to ensure mathematical consistency (Qty * Rate = Amount) in the UI columns.

                    decimal displayedQty = (decimal)d.NoOfGuards;

                    // Calculate adjusted duties (days) after discount

                    var originalDays = d.NoOfDays;

                    var discountDays = (d.HasDiscount && d.DiscountHour > 0) ? d.DiscountHour : 0;

                    var adjustedDays = Math.Max(0, originalDays - discountDays);



                    // Calculate amount based on adjusted duties

                    var ratePerDay = originalDays > 0 ? (decimal)d.MonthTotal / originalDays : 0;

                    var adjustedAmount = ratePerDay * adjustedDays;

                    // Use discounted rate (adjusted amount divided by quantity) instead of original rate
                    // Guard against division by zero (Lump Sum has NoOfGuards = 0)
                    decimal displayedRate = displayedQty > 0
                        ? Math.Round(adjustedAmount / displayedQty, 2)
                        : adjustedAmount;

                    // Calculate tax for each row using stored TaxAmount from AgreementDetails
                    decimal rowTax = d.IsTaxable ? (decimal)d.TaxAmount : 0;



                    return new

                    {

                        sno = index + 1,

                        hsnSacCode = d.hsnSacCode,

                        description = finalDescription,

                        dutiesTaxes = adjustedDays.ToString("0"), // Adjusted Days/Duties after discount

                        originalDuties = originalDays.ToString("0"), // Original Days/Duties

                        discountDuties = discountDays.ToString("0"), // Discount Days

                        units = displayedQty, // Number of Guards

                        rate = displayedRate, // Monthly Rate per Guard

                        amount = adjustedAmount, // Amount based on adjusted duties

                        originalAmount = (decimal)d.MonthTotal, // Original amount before discount

                        discountAmount = (decimal)d.MonthTotal - adjustedAmount, // Discount amount

                        isTaxable = d.IsTaxable,

                        cgstAmount = isIntraState ? (rowTax / 2) : 0,

                        sgstAmount = isIntraState ? (rowTax / 2) : 0,

                        igstAmount = !isIntraState ? rowTax : 0

                    };

                }).ToList();



                var result = new
        {
            documentType = "TAX INVOICE (Original for Recipient)",
            invoice = new
            {
                invoiceNoFormatted = $"FWGCHE/{invoice.InvoiceNo ?? "001"}/{(invoice.InvoiceDate.Month >= 4 ? invoice.InvoiceDate.Year % 100 : (invoice.InvoiceDate.Year - 1) % 100):D2}-{(invoice.InvoiceDate.Month >= 4 ? (invoice.InvoiceDate.Year + 1) % 100 : invoice.InvoiceDate.Year % 100):D2}",
                invoiceNoOriginal = invoice.InvoiceNo ?? "001",
                invoiceDateOriginal = invoice.InvoiceDate,
                invoiceDate = invoice.InvoiceDate.ToString("dd/MM/yyyy"),
                servicePeriod = invoice.InvoiceDate.ToString("MMMM yyyy"),
                workOrderNoFormatted = GetWorkOrderNo(client?.Code ?? ""),
                workOrderNoOriginal = GetWorkOrderNo(client?.Code ?? ""),
                workOrderDate = GetWorkOrderDate(client?.Code ?? ""),
                sacCode = detailsList.FirstOrDefault()?.hsnSacCode ?? "9985"
            },
            company = new
            {
                name = company?.Name ?? "",
                tagline = company?.ShortName ?? "",
                address = companyAddress,
                phone = company?.Phone ?? "",
                email = company?.Email ?? "",
                gstin = company?.GSTIN ?? "",
                pan = company?.PANNumber ?? "",
                esic = company?.ESICAccountNumber ?? "",
                epf = company?.EPFAccountNumber ?? "",
                bankName = company?.BankName ?? "",
                bankAccount = company?.BankAccount ?? "",
                ifscCode = company?.BankBranch ?? "",
                bankBranch = company?.BankBranch ?? "",
                state = branchState,
                cinNo = company?.TANNumber ?? "",
                website = "www.fwgsecurity.com",
                globalOfficeAddress = "No. 23 & 24, Taman Bukit Emas, Jalan Tampin, 70450 Seremban, Negeri Sembilan, Malaysia",
                globalOfficePhone = "+60 6 677 2000",
                globalOfficeFax = "+60 6 677 9866"
            },
            client = new
            {
                billingName = client?.Name ?? "",
                billingAddress = clientBillingAddress,
                billingGSTIN = client?.GSTIN ?? "",
                billingState = clientBillingState,
                shippingName = client?.Name ?? "",
                shippingAddress = clientShippingAddress,
                shippingGSTIN = client?.GSTIN ?? "",
                shippingState = client?.State ?? "",
                gstRegistrationStatus = gstStatus
            },
            totals = new
            {
                subtotal = subtotal,
                discountDuties = totalDiscountDuties,
                discountAmount = totalDiscountAmount,
                taxableValue = taxableValue,
                totalTaxableValue = taxableValue,
                cgstAmount = cgstAmount,
                sgstAmount = sgstAmount,
                igstAmount = igstAmount,
                cgstTaxableValue = isIntraState ? taxableValue : 0,
                sgstTaxableValue = isIntraState ? taxableValue : 0,
                igstTaxableValue = !isIntraState ? taxableValue : 0,
                cgstPct = cgstPct > 0 ? cgstPct : 9,
                sgstPct = sgstPct > 0 ? sgstPct : 9,
                igstPct = igstPct > 0 ? igstPct : 18,
                grandTotal = grandTotal,
                amountInWords = NumberToWords(grandTotal),
                taxType = isIntraState ? "CGST+SGST (Intra-State)" : "IGST (Inter-State)",
                isIntraState = isIntraState
            },
            statutory = new
            {
                pan = company?.PANNumber ?? "",
                gstin = company?.GSTIN ?? "",
                esic = company?.ESICAccountNumber ?? "",
                epf = company?.EPFAccountNumber ?? ""
            },
            declaration = new
            {
                thankYou = "Thank you for your business",
                serviceValue = "Invoice confirms actual service value",
                truthStatement = "All details are true and correct"
            },
            termsAndConditions = new
            {
                paymentMethod = "Payment via NEFT / Account Payee Chq",
                payableTo = company?.Name ?? "FreightWatch G Security Services India Pvt. Ltd.",
                interestRate = "Interest @12% p.a. on delayed payments",
                discrepancies = "Discrepancies must be reported within receipt",
                authorization = "Contains official stamp and authorized signature"
            },
            dataRows = dataRows,
            details = detailsList, // Alias for backward compatibility with frontend calculation logic
            metadata = new
            {
                generatedDate = DateTime.Now.ToString("dd/MM/yyyy HH:mm"),
                reportTitle = "Tax Invoice",
                totalItems = dataRows.Count,
                totalUnits = dataRows.Sum(x => (decimal)x.units)
            }
        };

        return Ok(result);

            }

            catch (Exception ex)

            {

                return StatusCode(500, new { error = "Error generating invoice data", details = ex.Message });

            }

        }

        [HttpGet]

        [Route("GetIndianInvoiceReportData")]

        public async Task<ActionResult<object>> GetIndianInvoiceReportData([FromQuery] int invoiceId)

        {

            try

            {

                // Get invoice data

                var invoice = await _oBMSDbContext.ClientInvoices

                    .Where(x => x.ID == invoiceId)

                    .FirstOrDefaultAsync();

                

                if (invoice == null)

                {

                    return NotFound(new { error = "Invoice not found" });

                }



                // Prepare data rows with a left join to handle missing agreement details gracefully

                var detailQuery = from d in _oBMSDbContext.ClientInvoiceDetails

                                  join ad in _oBMSDbContext.AgreementDetails on d.AgreementDetailID equals ad.ID into adGroup

                                  from ad in adGroup.DefaultIfEmpty()

                                  join st in _oBMSDbContext.ServiceTypes on ad.Category equals st.ServiceName into stGroup

                                  from st in stGroup.DefaultIfEmpty()

                                  where d.ClientInvoiceID == invoiceId

                                  select new

                                  {

                                      d.NoOfGuards,

                                      d.Rate,

                                      d.MonthTotal,

                                      d.NoOfDays,

                                      d.NoOfHours,

                                      d.TaxAmount,

                                      d.IsTaxable,

                                      HasDiscount = ad != null ? ad.HasDiscount : false,

                                      DiscountAmount = ad != null ? ad.DiscountAmount : 0,

                                      DiscountHour = ad != null ? ad.DiscountHour : 0,

                                      Description = ad != null ? ad.Description : "Security Services",

                                      hsnSacCode = st != null ? (st.HSNCode ?? "998715") : "998715"

                                  };



                var detailsList = await detailQuery.ToListAsync();



                if (!detailsList.Any())

                {

                    return NotFound(new { error = "No details found for this invoice" });

                }



                // Get agreement information

                int reportTargetAgreementId = invoice.AgreementID != 0 ? invoice.AgreementID : 1;

                var agreement = await _oBMSDbContext.Agreements

                    .Where(x => x.ID == reportTargetAgreementId)

                    .FirstOrDefaultAsync();



                // Get client and company information

                var client = await _oBMSDbContext.ClientMasters

                    .Where(x => x.Code == invoice.Client && x.Branch == invoice.Branch)

                    .FirstOrDefaultAsync();



                var company = await _oBMSDbContext.BranchMasters

                    .Where(x => x.Code == invoice.Branch)

                    .FirstOrDefaultAsync();



                // Calculate subtotal and total tax from database values
                // Use MonthTotal to match UI calculation (instead of nprecalculating from raw values)
                var subtotal = detailsList.Sum(d => (decimal)d.MonthTotal);

                // Calculate total discount duties (days) from agreement details
                // Discount reduces the number of duties/days, not the monetary amount
                var totalDiscountDuties = detailsList.Where(d => d.HasDiscount).Sum(d => d.DiscountHour > 0 ? d.DiscountHour : 0);

                // Recalculate taxable value based on reduced duties
                // Original: Rate * Days = Amount
                // With discount: Rate * (Days - DiscountDays) = NewAmount
                var taxableValue = detailsList.Sum(d => {
                    var originalDays = d.NoOfDays;
                    var discountDays = (d.HasDiscount && d.DiscountHour > 0) ? d.DiscountHour : 0;
                    var adjustedDays = Math.Max(0, originalDays - discountDays);
                    var ratePerDay = originalDays > 0 ? (decimal)d.MonthTotal / originalDays : 0;
                    return ratePerDay * adjustedDays;
                });

                var totalDiscountAmount = subtotal - taxableValue;

                var totalTaxFromDb = detailsList.Sum(d => (decimal)d.TaxAmount);



                // Debug logging

                _logger.LogInformation($"Indian Invoice GST Calculation Debug - Subtotal: {subtotal}, TotalDiscountDuties: {totalDiscountDuties}, TotalDiscountAmount: {totalDiscountAmount}, TaxableValue: {taxableValue}, Details Count: {detailsList.Count}");

                if (detailsList.Count > 0)

                {

                    var firstDetail = detailsList.First();

                    _logger.LogInformation($"Indian Invoice GST Calculation Debug - First Detail - Rate: {firstDetail.Rate}, NoOfGuards: {firstDetail.NoOfGuards}, NoOfHours: {firstDetail.NoOfHours}, NoOfDays: {firstDetail.NoOfDays}, HasDiscount: {firstDetail.HasDiscount}, DiscountHour: {firstDetail.DiscountHour}");

                }



                // Determine if intra-state (CGST+SGST) or inter-state (IGST)

                var branchState = company?.State?.Trim() ?? "";

                var clientBillingState = client?.BillingState?.Trim() ?? client?.State?.Trim() ?? "";



                // Debug logging

                _logger.LogInformation($"Indian Invoice GST Calculation Debug - Branch State: '{branchState}', Client Billing State: '{clientBillingState}'");



                var isIntraState = !string.IsNullOrEmpty(branchState) &&

                                   !string.IsNullOrEmpty(clientBillingState) &&

                                   branchState.Equals(clientBillingState, StringComparison.OrdinalIgnoreCase);



                _logger.LogInformation($"Indian Invoice GST Calculation Debug - Is Intra-State: {isIntraState}");



                // Calculate tax based on state: 18% GST on taxable value (after discount)

                decimal cgstAmount = 0;

                decimal sgstAmount = 0;

                decimal igstAmount = 0;

                decimal cgstPct = 0;

                decimal sgstPct = 0;

                decimal igstPct = 0;



                if (taxableValue > 0)

                {

                    if (isIntraState)

                    {

                        // Same state: CGST 9% + SGST 9% = 18% on taxable value

                        cgstAmount = taxableValue * 0.09M;

                        sgstAmount = taxableValue * 0.09M;

                        cgstPct = 9;

                        sgstPct = 9;

                    }

                    else

                    {

                        // Different state: IGST 18% on taxable value

                        igstAmount = taxableValue * 0.18M;

                        igstPct = 18;

                    }

                }



                var totalTax = cgstAmount + sgstAmount + igstAmount;

                var grandTotal = taxableValue + totalTax;



                // Debug logging for final GST amounts

                _logger.LogInformation($"Indian Invoice GST Calculation Debug - Final Amounts - Subtotal: {subtotal}, TotalDiscount: {totalDiscountAmount}, TaxableValue: {taxableValue}, CGST: {cgstAmount}, SGST: {sgstAmount}, IGST: {igstAmount}, TotalTax: {totalTax}, GrandTotal: {grandTotal}");



                // Create addresses - shipping falls back to WorkPlace from Agreement if missing

                var companyAddress = $"{company?.Address1 ?? ""} {(company?.Address2 ?? "")}".Trim();

                var clientBillingAddress = $"{client?.Address1 ?? ""} {(client?.Address2 ?? "")}".Trim();

                var rawShippingAddr = $"{client?.ShippingAddress1 ?? ""} {(client?.ShippingAddress2 ?? "")}".Trim();

                var clientShippingAddress = !string.IsNullOrEmpty(rawShippingAddr) ? rawShippingAddr : (agreement?.WorkPlace ?? clientBillingAddress);



                // Get GST Registration Status if available

                var gstStatus = client?.GSTRegistrationStatus ?? "Regular";



                // Get state codes for Indian GST compliance

                var branchStateCode = GetStateCode(branchState);

                var clientBillingStateCode = GetStateCode(clientBillingState);

                var clientShippingStateCode = GetStateCode(client?.ShippingState?.Trim() ?? clientBillingState);



                var dataRows = detailsList.Select((d, index) =>

                {

                    var finalDescription = d.Description;

                    if (d.NoOfHours > 0)

                    {

                        finalDescription += $" ({d.NoOfHours:0} Hrs)";

                    }

                    // Add discount information to description if applicable
                    if (d.HasDiscount && d.DiscountHour > 0)

                    {

                        finalDescription += $" (Discount: {d.DiscountHour} days)";

                    }



                    // Display quantity as Number of Guards and Rate as Monthly Rate per Guard

                    // to ensure mathematical consistency (Qty * Rate = Amount) in the UI columns.

                    decimal displayedQty = (decimal)d.NoOfGuards;

                    // Calculate adjusted duties (days) after discount
                    var originalDays = d.NoOfDays;
                    var discountDays = (d.HasDiscount && d.DiscountHour > 0) ? d.DiscountHour : 0;
                    var adjustedDays = Math.Max(0, originalDays - discountDays);

                    // Calculate amount based on adjusted duties
                    var ratePerDay = originalDays > 0 ? (decimal)d.MonthTotal / originalDays : 0;
                    var adjustedAmount = ratePerDay * adjustedDays;

                    // Use discounted rate (adjusted amount divided by quantity) instead of original rate
                    // Guard against division by zero (Lump Sum has NoOfGuards = 0)
                    decimal displayedRate = displayedQty > 0
                        ? Math.Round(adjustedAmount / displayedQty, 2)
                        : adjustedAmount;



                    // Calculate tax for each row using stored TaxAmount from AgreementDetails

                    decimal rowTax = d.IsTaxable ? (decimal)d.TaxAmount : 0;



                    return new

                    {

                        sno = index + 1,

                        hsnSacCode = d.hsnSacCode,

                        description = finalDescription,

                        dutiesTaxes = adjustedDays.ToString("0"), // Adjusted Days/Duties after discount

                        originalDuties = originalDays.ToString("0"), // Original Days/Duties

                        discountDuties = discountDays.ToString("0"), // Discount Days

                        units = displayedQty, // Number of Guards

                        rate = displayedRate, // Monthly Rate per Guard

                        amount = adjustedAmount, // Amount based on adjusted duties

                        originalAmount = (decimal)d.MonthTotal, // Original amount before discount

                        discountAmount = (decimal)d.MonthTotal - adjustedAmount, // Discount amount

                        isTaxable = d.IsTaxable,

                        cgstAmount = isIntraState ? (rowTax / 2) : 0,

                        sgstAmount = isIntraState ? (rowTax / 2) : 0,

                        igstAmount = !isIntraState ? rowTax : 0

                    };

                }).ToList();



                var result = new

                {

                    documentType = "INDIAN TAX INVOICE (Original for Recipient)",

                    invoice = new

                    {

                        invoiceNoFormatted = $"FWGCHE/{invoice.InvoiceNo ?? "001"}/{(invoice.InvoiceDate.Month >= 4 ? invoice.InvoiceDate.Year % 100 : (invoice.InvoiceDate.Year - 1) % 100):D2}-{(invoice.InvoiceDate.Month >= 4 ? (invoice.InvoiceDate.Year + 1) % 100 : invoice.InvoiceDate.Year % 100):D2}",

                        invoiceNoOriginal = invoice.InvoiceNo ?? "001",

                        invoiceDateOriginal = invoice.InvoiceDate,

                        invoiceDate = invoice.InvoiceDate.ToString("dd/MM/yyyy"),

                        servicePeriod = invoice.InvoiceDate.ToString("MMMM yyyy"),

                        placeOfSupply = clientBillingState,

                        workOrderNoFormatted = GetWorkOrderNo(client?.Code ?? ""),

                        workOrderNoOriginal = GetWorkOrderNo(client?.Code ?? ""),

                        workOrderDate = GetWorkOrderDate(client?.Code ?? ""),

                        sacCode = detailsList.FirstOrDefault()?.hsnSacCode ?? "9985",

                        supplyType = "Service",

                        reverseCharge = "No"

                    },

                    company = new

                    {

                        name = company?.Name ?? "",

                        tagline = company?.ShortName ?? "",

                        address = companyAddress,

                        phone = company?.Phone ?? "",

                        email = company?.Email ?? "",

                        gstin = company?.GSTIN ?? "",

                        pan = company?.PANNumber ?? "",

                        esic = company?.ESICAccountNumber ?? "",

                        epf = company?.EPFAccountNumber ?? "",

                        bankName = company?.BankName ?? "",

                        bankAccount = company?.BankAccount ?? "",

                        ifscCode = company?.BankBranch ?? "",

                        bankBranch = company?.BankBranch ?? "",

                        state = branchState,

                        cinNo = company?.TANNumber ?? "",

                        website = "www.fwgindia.com",

                        globalOfficeAddress = "No. 23 & 24, Taman Bukit Emas, Jalan Tampin, 70450 Seremban, Negeri Sembilan, Malaysia",

                        globalOfficePhone = "+60 6 677 2000",

                        globalOfficeFax = "+60 6 677 9866"

                    },

                    client = new

                    {

                        billingName = client?.Name ?? "",

                        billingAddress = clientBillingAddress,

                        billingGSTIN = client?.GSTIN ?? "",

                        billingState = clientBillingState,

                        stateCode = clientBillingStateCode,

                        billingPAN = client?.PANNumber ?? "",

                        shippingName = client?.Name ?? "",

                        shippingAddress = clientShippingAddress,

                        shippingGSTIN = client?.GSTIN ?? "",

                        shippingState = client?.ShippingState ?? clientBillingState,

                        shippingStateCode = clientShippingStateCode,

                        shippingPAN = client?.PANNumber ?? "",

                        gstRegistrationStatus = gstStatus

                    },

                    totals = new

                    {

                        subtotal = subtotal,

                        discountDuties = totalDiscountDuties,

                        discountAmount = totalDiscountAmount,

                        taxableValue = taxableValue,

                        totalTaxableValue = taxableValue,

                        cgstAmount = cgstAmount,

                        sgstAmount = sgstAmount,

                        igstAmount = igstAmount,

                        cgstTaxableValue = isIntraState ? taxableValue : 0,

                        sgstTaxableValue = isIntraState ? taxableValue : 0,

                        igstTaxableValue = !isIntraState ? taxableValue : 0,

                        cgstPct = cgstPct > 0 ? cgstPct : 9,

                        sgstPct = sgstPct > 0 ? sgstPct : 9,

                        igstPct = igstPct > 0 ? igstPct : 18,

                        grandTotal = grandTotal,

                        amountInWords = NumberToWords(grandTotal),

                        taxType = isIntraState ? "CGST+SGST (Intra-State)" : "IGST (Inter-State)",

                        isIntraState = isIntraState

                    },

                    statutory = new

                    {

                        pan = company?.PANNumber ?? "",

                        gstin = company?.GSTIN ?? "",

                        esic = company?.ESICAccountNumber ?? "",

                        epf = company?.EPFAccountNumber ?? ""

                    },

                    declaration = new

                    {

                        thankYou = "Thank you for your business",

                        serviceValue = "Invoice confirms actual service value as per Indian GST Act",

                        truthStatement = "All details are true and correct as per GST regulations"

                    },

                    termsAndConditions = new

                    {

                        paymentMethod = "Payment via NEFT / Account Payee Cheque",

                        payableTo = company?.Name ?? "FreightWatch G Security Services India Pvt. Ltd.",

                        interestRate = "Interest @18% p.a. on delayed payments as per GST Act",

                        discrepancies = "Discrepancies must be reported within receipt as per GST Act",

                        authorization = "Contains official stamp and authorized signature"

                    },

                    dataRows = dataRows,

                    details = detailsList, // Alias for backward compatibility with frontend calculation logic

                    // Indian Invoice specific data

                    hsnSummary = "998715 - Security Services",

                    taxRateSummary = isIntraState ? "9% CGST + 9% SGST" : "18% IGST",

                    totalTaxAmount = totalTax,

                    metadata = new

                    {

                        generatedDate = DateTime.Now.ToString("dd/MM/yyyy HH:mm"),

                        reportTitle = "Indian Tax Invoice",

                        totalItems = dataRows.Count,

                        totalUnits = dataRows.Sum(x => (decimal)x.units)

                    }

                };



                return Ok(result);

            }

            catch (Exception ex)

            {

                return StatusCode(500, new { error = "Error generating Indian invoice data", details = ex.Message });

            }

        }



        private string GetStateCode(string stateName)
        {
            if (string.IsNullOrEmpty(stateName))
                return "";

            // Indian state codes mapping for GST compliance
            var stateCodes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                {"Andaman and Nicobar Islands", "35"},
                {"Andhra Pradesh", "37"},
                {"Arunachal Pradesh", "12"},
                {"Assam", "18"},
                {"Bihar", "10"},
                {"Chandigarh", "04"},
                {"Chhattisgarh", "22"},
                {"Dadra and Nagar Haveli", "26"},
                {"Daman and Diu", "25"},
                {"Delhi", "07"},
                {"Goa", "30"},
                {"Gujarat", "24"},
                {"Haryana", "06"},
                {"Himachal Pradesh", "02"},
                {"Jammu and Kashmir", "01"},
                {"Jharkhand", "20"},
                {"Karnataka", "29"},
                {"Kerala", "32"},
                {"Lakshadweep", "31"},
                {"Madhya Pradesh", "23"},
                {"Maharashtra", "27"},
                {"Manipur", "14"},
                {"Meghalaya", "17"},
                {"Mizoram", "15"},
                {"Nagaland", "13"},
                {"Odisha", "21"},
                {"Puducherry", "34"},
                {"Punjab", "03"},
                {"Rajasthan", "08"},
                {"Sikkim", "11"},
                {"Tamil Nadu", "33"},
                {"Telangana", "36"},
                {"Tripura", "16"},
                {"Uttar Pradesh", "09"},
                {"Uttarakhand", "05"},
                {"West Bengal", "19"}
            };

            return stateCodes.TryGetValue(stateName, out string code) ? code : "";
        }

        private static readonly Dictionary<string, (string workOrderNo, string workOrderDate)> _clientWorkOrders =
            new Dictionary<string, (string, string)>(StringComparer.OrdinalIgnoreCase)
            {
                { "FWGC000034", ("DILP/26-27/HU1/MANPOWEEXP/WO/001",  "03-APR-2026") },
                { "FWGC000041", ("DILP/26-27/VAIP/SECEXP/WO/004",     "15-APR-2026") },
                { "FWGC000038", ("MRILP/25-26/HU2-2/MANPOWEXP/WO/023","28-MAR-2026") },
                { "FWGC000007", ("RSIPPL/26-27/SULR/MANPOWEXP/WO/001","15-APR-2026") },
                { "FWGC000025", ("SMILP/26-27/RED/MANPOWEXP/WO/002",  "29-APR-2026") },
                { "FWGC000042", ("SMILP/26-27/RED/SECEXP/WO/001",     "07-APR-2026") },
                { "FWGC000021", ("RMILP/26-27/PILP/MANSECEXP/WO/001", "08-APR-2026") },
                { "FWGC000022", ("RMILP/26-27/PILP/MANPOWEXP/WO/002", "02-MAY-2026") },
            };

        private string GetWorkOrderNo(string clientCode)
        {
            return _clientWorkOrders.TryGetValue(clientCode, out var info) ? info.workOrderNo : "";
        }

        private string GetWorkOrderDate(string clientCode)
        {
            return _clientWorkOrders.TryGetValue(clientCode, out var info) ? info.workOrderDate : "";
        }

        private string NumberToWords(decimal number)

        {

            if (number == 0) return "Zero";



            string[] units = { "", "One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine" };

            string[] teens = { "Ten", "Eleven", "Twelve", "Thirteen", "Fourteen", "Fifteen", "Sixteen", "Seventeen", "Eighteen", "Nineteen" };

            string[] tens = { "", "", "Twenty", "Thirty", "Forty", "Fifty", "Sixty", "Seventy", "Eighty", "Ninety" };



            var words = new List<string>();

            var amount = (long)Math.Floor(number);



            if (amount >= 10000000) // Crore

            {

                var crores = amount / 10000000;

                words.AddRange(ConvertNumberToWords((int)crores, units, teens, tens));

                words.Add("Crore");

                amount %= 10000000;

            }



            if (amount >= 100000) // Lakh

            {

                var lakhs = amount / 100000;

                words.AddRange(ConvertNumberToWords((int)lakhs, units, teens, tens));

                words.Add("Lakh");

                amount %= 100000;

            }



            if (amount >= 1000) // Thousand

            {

                var thousands = amount / 1000;

                words.AddRange(ConvertNumberToWords((int)thousands, units, teens, tens));

                words.Add("Thousand");

                amount %= 1000;

            }



            if (amount >= 100) // Hundred

            {

                var hundreds = amount / 100;

                words.Add(units[(int)hundreds]);

                words.Add("Hundred");

                amount %= 100;

            }



            if (amount > 0)

            {

                words.AddRange(ConvertNumberToWords((int)amount, units, teens, tens));

            }



            return string.Join(" ", words);

        }



        private List<string> ConvertNumberToWords(int number, string[] units, string[] teens, string[] tens)

        {

            var words = new List<string>();



            if (number < 10)

            {

                words.Add(units[number]);

            }

            else if (number < 20)

            {

                words.Add(teens[number - 10]);

            }

            else

            {

                var ten = number / 10;

                var unit = number % 10;

                words.Add(tens[ten]);

                if (unit > 0)

                {

                    words.Add(units[unit]);

                }

            }



            return words;

        }





        [HttpPost]

        [Route("SaveAndUpdateBatchInvoice")]

        public async Task<Object> SaveAndUpdateBatchInvoice(ClientBatchInvoiceRequestDto clientBatchInvoiceRequestDto)

        {

            try

            {
                // Get the starting invoice number for the branch/period (for new invoices)
                var firstNewInvoice = clientBatchInvoiceRequestDto.data.FirstOrDefault(x => x.ID == 0);
                int nextInvoiceNo = 0;
                string branch = "";
                DateTime invoiceDate = DateTime.Now;

                if (firstNewInvoice != null)
                {
                    branch = firstNewInvoice.Branch;
                    invoiceDate = firstNewInvoice.InvoiceDate;

                    if (invoiceDate >= Convert.ToDateTime("2015-04-01 00:00:00.000"))
                    {
                        var sqlQuery = @"SELECT ISNULL(MAX(CAST(INVOICENO AS INT))+1, 1) AS NEWCLIENTINVOICENO FROM CLIENTINVOICE WHERE BRANCH=@Branch AND InvoiceDate >= '2015-04-01T00:00:00.000'";
                        var parameters = new[] { new Microsoft.Data.SqlClient.SqlParameter("@Branch", branch) };
                        var result = await _oBMSDbContext.ClientInvoiceNoResult.FromSqlRaw(sqlQuery, parameters).FirstOrDefaultAsync();
                        nextInvoiceNo = result.NEWCLIENTINVOICENO;
                    }
                    else
                    {
                        var sqlQuery = @"SELECT ISNULL(MAX(CAST(INVOICENO AS INT))+1,1) AS NEWCLIENTINVOICENO FROM CLIENTINVOICE WHERE BRANCH=@BranchCode AND InvoiceDate < '2015-04-01 00:00:00.000'";
                        var parameters = new[] { new Microsoft.Data.SqlClient.SqlParameter("@Branch", branch) };
                        var result = await _oBMSDbContext.ClientInvoiceNoResult.FromSqlRaw(sqlQuery, parameters).FirstOrDefaultAsync();
                        nextInvoiceNo = result.NEWCLIENTINVOICENO;
                    }
                }

                foreach (ClientInvoiceRequestDto clientInvoiceRequestDto in clientBatchInvoiceRequestDto.data)

                {

                    try

                    {

                        // Validate agreement IsValid for the invoice period
                        if (clientInvoiceRequestDto.AgreementID != null && clientInvoiceRequestDto.AgreementID > 0)
                        {
                            var agreement = await _oBMSDbContext.Agreements
                                .Where(a => a.ID == clientInvoiceRequestDto.AgreementID)
                                .FirstOrDefaultAsync();

                            if (agreement != null && agreement.IsValid == true)
                            {
                                var agreementMonth = agreement.AgreementDate.ToString("yyyyMM");
                                var invoiceMonth = clientInvoiceRequestDto.InvoiceDate.ToString("yyyyMM");

                                if (agreementMonth != invoiceMonth)
                                {
                                    return BadRequest(new { error = $"Agreement for this period does not exist for client {clientInvoiceRequestDto.Client}. Unable to process invoice." });
                                }
                            }
                        }

                        var clientInvoice = new ClientInvoice();

                        if (clientInvoiceRequestDto.ID != 0)

                        {

                            clientInvoice = _oBMSDbContext.ClientInvoices.Where(x => x.ID == clientInvoiceRequestDto.ID).FirstOrDefault();

                            if (clientInvoice == null)
                            {
                                return BadRequest(new { message = $"Invoice with ID {clientInvoiceRequestDto.ID} not found." });
                            }
                        }

                        // Don't set ID when updating - it's already set from the database retrieval
                        if (clientInvoiceRequestDto.ID == 0)
                        {
                            clientInvoice.ID = clientInvoiceRequestDto.ID;
                        }

                        clientInvoice.ServiceCharges = clientInvoiceRequestDto.ServiceCharges ?? 0;

                        clientInvoice.Subject = clientInvoiceRequestDto.Subject;

                        clientInvoice.Note = clientInvoiceRequestDto.Note;

                        clientInvoice.Branch = clientInvoiceRequestDto.Branch;

                        clientInvoice.Client = clientInvoiceRequestDto.Client;

                        clientInvoice.AgreementID = clientInvoiceRequestDto.AgreementID;

                        clientInvoice.Discount = clientInvoiceRequestDto.Discount ?? 0;

                        clientInvoice.InvoiceDate = clientInvoiceRequestDto.InvoiceDate;

                        // For new invoices, use sequential invoice number from the batch
                        if (clientInvoiceRequestDto.ID == 0)
                        {
                            clientInvoice.InvoiceNo = nextInvoiceNo.ToString();
                            nextInvoiceNo++;
                        }
                        else
                        {
                            clientInvoice.InvoiceNo = clientInvoiceRequestDto.InvoiceNo;
                        }

                        clientInvoice.TaxAmount = clientInvoiceRequestDto.TaxAmount ?? 0;

                        clientInvoice.IsDeleted = "N";

                        clientInvoice.LASTUPDATE = DateTime.Now;

                        // Check if InvoiceNo already exists in the same Branch (only for existing invoices or when changing InvoiceNo)
                        // Skip this check for new invoices in batch since we're generating sequential numbers
                        if (clientInvoiceRequestDto.ID != 0)
                        {
                            var existingInvoice = await _oBMSDbContext.ClientInvoices
                                .Where(x => x.InvoiceNo == clientInvoice.InvoiceNo
                                    && x.Branch == clientInvoiceRequestDto.Branch
                                    && x.ID != clientInvoiceRequestDto.ID)
                                .FirstOrDefaultAsync();

                            if (existingInvoice != null)
                            {
                                return BadRequest(new { message = $"InvoiceNo '{clientInvoice.InvoiceNo}' already exists in Branch '{clientInvoiceRequestDto.Branch}' with ID {existingInvoice.ID}" });
                            }
                        }

                        if (clientInvoiceRequestDto.ID != 0)

                        {

                            _oBMSDbContext.ClientInvoices.Update(clientInvoice);



                        }

                        else

                        {

                            _oBMSDbContext.ClientInvoices.Add(clientInvoice);



                        }



                        await _oBMSDbContext.SaveChangesAsync();





                        if (clientInvoiceRequestDto.details != null)

                        {

                            foreach (ClientInvoiceDetailRequestDto detail in clientInvoiceRequestDto.details)

                            {





                                var clientInvoiceDetail = new ClientInvoiceDetail();



                                if (detail.ID != 0)

                                {

                                    clientInvoiceDetail = _oBMSDbContext.ClientInvoiceDetails.Where(x => x.ID == detail.ID).FirstOrDefault();

                                }



                                clientInvoiceDetail.ID = detail.ID;



                                clientInvoiceDetail.NoOfGuards = detail.NoOfGuards;

                                clientInvoiceDetail.ClientInvoiceID = clientInvoice.ID;

                                clientInvoiceDetail.AgreementDetailID = detail.AgreementDetailID;

                                clientInvoiceDetail.AgreementID = detail.AgreementID;

                                clientInvoiceDetail.AgreementDate = detail.AgreementDate;

                                clientInvoiceDetail.NoOfGuards = detail.NoOfGuards;

                                clientInvoiceDetail.Rate = detail.Rate ?? 0;

                                clientInvoiceDetail.NoOfHours = detail.NoOfHours ?? 0;

                                clientInvoiceDetail.NoOfDays = detail.NoOfDays ?? 0;

                                clientInvoiceDetail.FollowCalender = detail.FollowCalender;

                                clientInvoiceDetail.HasDiscount = detail.HasDiscount;

                                clientInvoiceDetail.DiscountAmount = detail.DiscountAmount ?? 0;

                                clientInvoiceDetail.IsTaxable = detail.IsTaxable;

                                clientInvoiceDetail.TaxAmount = detail.TaxAmount ?? 0;

                                clientInvoiceDetail.MonthTotal

                                 = detail.MonthTotal ?? 0;





                                clientInvoiceDetail.LASTUPDATE = DateTime.Now;



                                if (detail.ID != 0)

                                {

                                    _oBMSDbContext.ClientInvoiceDetails.Update(clientInvoiceDetail);

                                }

                                else

                                {

                                    _oBMSDbContext.ClientInvoiceDetails.Add(clientInvoiceDetail);

                                }





                                await _oBMSDbContext.SaveChangesAsync();

                            }

                        }

                    }

                    catch (Exception ex)

                    {



                        throw;

                    }

                }

            }

            catch (Exception ex)

            {



                throw;

            }

            return null;

        }





        [HttpPost]

        [Route("SaveAndUpdateInvoice")]

        public async Task<Object> SaveAndUpdateInvoice(ClientInvoiceRequestDto clientInvoiceRequestDto)

        {

            try

            {

                // Validate agreement IsValid for the invoice period
                if (clientInvoiceRequestDto.AgreementID != null && clientInvoiceRequestDto.AgreementID > 0)
                {
                    var agreement = await _oBMSDbContext.Agreements
                        .Where(a => a.ID == clientInvoiceRequestDto.AgreementID)
                        .FirstOrDefaultAsync();

                    if (agreement != null && agreement.IsValid == true)
                    {
                        var agreementMonth = agreement.AgreementDate.ToString("yyyyMM");
                        var invoiceMonth = clientInvoiceRequestDto.InvoiceDate.ToString("yyyyMM");

                        if (agreementMonth != invoiceMonth)
                        {
                            return BadRequest(new { error = "Agreement for this period does not exist. Unable to process invoice." });
                        }
                    }
                }

                var clientInvoice = new ClientInvoice();

                if (clientInvoiceRequestDto.ID != 0)

                {

                    clientInvoice = _oBMSDbContext.ClientInvoices.Where(x => x.ID == clientInvoiceRequestDto.ID).FirstOrDefault();

                }

                clientInvoice.ID = clientInvoiceRequestDto.ID;

                clientInvoice.ServiceCharges = clientInvoiceRequestDto.ServiceCharges ?? 0;

                clientInvoice.Subject = clientInvoiceRequestDto.Subject;

                clientInvoice.Note = clientInvoiceRequestDto.Note;

                clientInvoice.Branch = clientInvoiceRequestDto.Branch;

                clientInvoice.Client = clientInvoiceRequestDto.Client;

                clientInvoice.AgreementID = clientInvoiceRequestDto.AgreementID;

                clientInvoice.Discount = clientInvoiceRequestDto.Discount ?? 0;

                clientInvoice.InvoiceDate = clientInvoiceRequestDto.InvoiceDate;

                clientInvoice.InvoiceNo = clientInvoiceRequestDto.InvoiceNo;

                clientInvoice.TaxAmount = clientInvoiceRequestDto.TaxAmount ?? 0;

                clientInvoice.IsDeleted = "N";

                clientInvoice.LASTUPDATE = DateTime.Now;



                if (clientInvoiceRequestDto.ID != 0)

                {

                    _oBMSDbContext.ClientInvoices.Update(clientInvoice);



                }

                else

                {

                    _oBMSDbContext.ClientInvoices.Add(clientInvoice);



                }



                await _oBMSDbContext.SaveChangesAsync();





                if (clientInvoiceRequestDto.details != null)

                {

                    foreach (ClientInvoiceDetailRequestDto detail in clientInvoiceRequestDto.details)

                    {





                        var clientInvoiceDetail = new ClientInvoiceDetail();



                        if (detail.ID != 0)

                        {

                            clientInvoiceDetail = _oBMSDbContext.ClientInvoiceDetails.Where(x => x.ID == detail.ID).FirstOrDefault();

                        }



                        clientInvoiceDetail.ID = detail.ID;



                        clientInvoiceDetail.NoOfGuards = detail.NoOfGuards;

                        clientInvoiceDetail.ClientInvoiceID = clientInvoice.ID;

                        clientInvoiceDetail.AgreementDetailID = detail.AgreementDetailID;

                        clientInvoiceDetail.AgreementID = detail.AgreementID;

                        clientInvoiceDetail.AgreementDate = detail.AgreementDate;

                        clientInvoiceDetail.NoOfGuards = detail.NoOfGuards;

                        clientInvoiceDetail.Rate = detail.Rate ?? 0;

                        clientInvoiceDetail.NoOfHours = detail.NoOfHours ?? 0;

                        clientInvoiceDetail.NoOfDays = detail.NoOfDays ?? 0;

                        clientInvoiceDetail.FollowCalender = detail.FollowCalender;

                        clientInvoiceDetail.HasDiscount = detail.HasDiscount;

                        clientInvoiceDetail.DiscountAmount = detail.DiscountAmount ?? 0;

                        clientInvoiceDetail.IsTaxable = detail.IsTaxable;

                        clientInvoiceDetail.TaxAmount = detail.TaxAmount ?? 0;

                        clientInvoiceDetail.MonthTotal = Math.Round(detail.MonthTotal ?? 0m, 2);

                        clientInvoiceDetail.Description = detail.Description;



                        clientInvoiceDetail.LASTUPDATE = DateTime.Now;



                        if (detail.ID != 0)

                        {

                            _oBMSDbContext.ClientInvoiceDetails.Update(clientInvoiceDetail);

                        }

                        else

                        {

                            _oBMSDbContext.ClientInvoiceDetails.Add(clientInvoiceDetail);

                        }





                        await _oBMSDbContext.SaveChangesAsync();

                    }

                }



                Dictionary<string, object> dictResult = new Dictionary<string, object>();

                dictResult.Add("Success", "Success");

                dictResult.Add("agreement", clientInvoice);

                response.Headers.Add("Success", "Successfully save & update Invoice details.");



                return Ok(dictResult);



            }

            catch (Exception ex)

            {



                throw;

            }

        }



        [HttpPost]

        [Route("SaveAndUpdateInvoiceDetail")]

        public async Task<Object> SaveAndUpdateInvoiceDetail(ClientInvoiceRequestDto clientInvoiceRequestDto)

        {

            try

            {

                var clientInvoice = new ClientInvoice();

                if (clientInvoiceRequestDto.ID != 0)

                {

                    clientInvoice = _oBMSDbContext.ClientInvoices.Where(x => x.ID == clientInvoiceRequestDto.ID).FirstOrDefault();

                }



                clientInvoice.ID = clientInvoiceRequestDto.ID;

                clientInvoice.ServiceCharges = clientInvoiceRequestDto.ServiceCharges ?? 0;

                clientInvoice.LASTUPDATE = DateTime.Now;



                if (clientInvoiceRequestDto.ID != 0)

                {

                    _oBMSDbContext.ClientInvoices.Update(clientInvoice);



                }





                await _oBMSDbContext.SaveChangesAsync();



                if (clientInvoiceRequestDto.details != null)

                {

                    foreach (ClientInvoiceDetailRequestDto detail in clientInvoiceRequestDto.details)

                    {





                        var clientInvoiceDetail = new ClientInvoiceDetail();



                        if (detail.ID != 0)

                        {

                            clientInvoiceDetail = _oBMSDbContext.ClientInvoiceDetails.Where(x => x.ID == detail.ID).FirstOrDefault();

                        }



                        clientInvoiceDetail.ID = detail.ID;



                        clientInvoiceDetail.NoOfGuards = detail.NoOfGuards;

                        clientInvoiceDetail.ClientInvoiceID = clientInvoice.ID;

                        clientInvoiceDetail.AgreementDetailID = detail.AgreementDetailID;

                        clientInvoiceDetail.AgreementID = detail.AgreementID;

                        clientInvoiceDetail.AgreementDate = detail.AgreementDate;

                        clientInvoiceDetail.NoOfGuards = detail.NoOfGuards;

                        clientInvoiceDetail.Rate = detail.Rate ?? 0;

                        clientInvoiceDetail.NoOfHours = detail.NoOfHours ?? 0;

                        clientInvoiceDetail.NoOfDays = detail.NoOfDays ?? 0;

                        clientInvoiceDetail.FollowCalender = detail.FollowCalender;

                        clientInvoiceDetail.HasDiscount = detail.HasDiscount;

                        clientInvoiceDetail.DiscountAmount = detail.DiscountAmount ?? 0;

                        clientInvoiceDetail.IsTaxable = detail.IsTaxable;

                        clientInvoiceDetail.TaxAmount = detail.TaxAmount ?? 0;

                        clientInvoiceDetail.MonthTotal = Math.Round(detail.MonthTotal ?? 0m, 2);





                        clientInvoiceDetail.LASTUPDATE = DateTime.Now;



                        if (detail.ID != 0)

                        {

                            _oBMSDbContext.ClientInvoiceDetails.Update(clientInvoiceDetail);

                        }

                        else

                        {

                            _oBMSDbContext.ClientInvoiceDetails.Add(clientInvoiceDetail);

                        }





                        await _oBMSDbContext.SaveChangesAsync();

                    }

                }



            }

            catch (Exception ex)

            {



                throw;

            }

            return null;

        }



        [HttpPost]

        [Route("DeleteInvoiceDetailById")]

        public async Task<ActionResult<HttpResponseMessage>> DeleteInvoiceDetailById(ClientInvoiceRequestDto clientInvoiceRequestDto)

        {

            try

            {

                var data = _oBMSDbContext.ClientInvoiceDetails.Where(x => x.ID == clientInvoiceRequestDto.DeletedDetailID).FirstOrDefault();





                if (data != null)

                {

                    _oBMSDbContext.ClientInvoiceDetails.Remove(data);

                    await _oBMSDbContext.SaveChangesAsync();

                }



                var clientInvoice = new ClientInvoice();

                if (clientInvoiceRequestDto.ID != 0)

                {

                    clientInvoice = _oBMSDbContext.ClientInvoices.Where(x => x.ID == clientInvoiceRequestDto.ID).FirstOrDefault();

                }



                clientInvoice.ID = clientInvoiceRequestDto.ID;

                clientInvoice.ServiceCharges = clientInvoiceRequestDto.ServiceCharges ?? 0;

                clientInvoice.LASTUPDATE = DateTime.Now;



                if (clientInvoiceRequestDto.ID != 0)

                {

                    _oBMSDbContext.ClientInvoices.Update(clientInvoice);



                }





                await _oBMSDbContext.SaveChangesAsync();





                Dictionary<string, object> dictResult = new Dictionary<string, object>();

                dictResult.Add("Success", "Success");





                return Ok(dictResult);



            }

            catch (Exception ex)

            {



                throw;

            }

        }



        [HttpGet("DeleteInvoiceById")]

        public async Task<ActionResult<HttpResponseMessage>> DeleteInvoiceById(int Id)

        {

            try

            {

                if (Id <= 0)

                {

                    return BadRequest(new { error = "Invalid invoice ID." });

                }

                var clientInvoice = _oBMSDbContext.ClientInvoices.Where(x => x.ID == Id).FirstOrDefault();

                if (clientInvoice == null)

                {

                    return NotFound(new { error = $"Invoice with ID {Id} not found." });

                }

                clientInvoice.LASTUPDATE = DateTime.Now;

                clientInvoice.IsDeleted = "Y";



                _oBMSDbContext.ClientInvoices.Update(clientInvoice);

                await _oBMSDbContext.SaveChangesAsync();



                Dictionary<string, object> dictResult = new Dictionary<string, object>();

                dictResult.Add("Success", "Success");



                return Ok(dictResult);



            }

            catch (Exception ex)

            {

                _logger.LogError(ex, "Error deleting invoice with ID {Id}", Id);

                return StatusCode(500, new { error = "Internal server error", message = ex.Message });

            }

        }



        [HttpGet]

        [Route("DeletedInvoiceListByBranchId")]

        public async Task<ActionResult<HttpResponseMessage>> DeletedInvoiceListByBranchId(string Id)

        {

            try

            {

                var result = from clientInvoice in _oBMSDbContext.ClientInvoices

                             join clientMaster in _oBMSDbContext.ClientMasters on new { clientInvoice.Client, clientInvoice.Branch } equals new { Client = clientMaster.Code, clientMaster.Branch }

                             where clientInvoice.IsDeleted == "Y" && clientInvoice.Branch == Id

                             orderby clientInvoice.InvoiceNo

                             select new

                             {

                                 clientInvoice.ID,

                                 clientInvoice.InvoiceNo,

                                 clientInvoice.InvoiceDate,

                                 clientInvoice.ServiceCharges,

                                 clientInvoice.Discount,

                                 clientInvoice.TaxAmount,

                                 ClientName = clientMaster.Name,

                                 clientInvoice.Client,

                                 clientInvoice.LASTUPDATE

                             };





                return Ok(result);



            }

            catch (Exception ex)

            {



                throw;

            }

        }



        [HttpGet]

        [Route("PrintInvoice")]

        public async Task<ActionResult<HttpResponseMessage>> PrintInvoice(int invoiceId)

        {

            try

            {

                Dictionary<string, object> results = new Dictionary<string, object>();

                var obj1 = await _oBMSDbContext.ClientInvoices.Where(x => x.ID == invoiceId).FirstOrDefaultAsync();

                results.Add("invoice", obj1);



                var obj2 = await _oBMSDbContext.ClientInvoiceDetails.Where(x => x.ClientInvoiceID == invoiceId).ToListAsync();

                results.Add("details", obj2);



                var client = await _oBMSDbContext.ClientMasters.Where(x => x.Code == obj1.Client).FirstOrDefaultAsync();



                results.Add("client", client);



                results.Add("Success", "");





                return Ok(results);



            }

            catch (Exception ex)

            {



                throw;

            }

        }



        [HttpPost]

        [Route("RestoreInvoices")]

        public async Task<ActionResult<HttpResponseMessage>> RestoreInvoices(int[] ID)

        {

            try

            {

                if (ID != null)

                {

                    foreach (int id in ID)

                    {

                        var clientInvoice = new ClientInvoice();

                        clientInvoice = _oBMSDbContext.ClientInvoices.Where(x => x.ID == id).FirstOrDefault();



                        clientInvoice.LASTUPDATE = DateTime.Now;

                        clientInvoice.IsDeleted = "N";





                        _oBMSDbContext.ClientInvoices.Update(clientInvoice);



                        await _oBMSDbContext.SaveChangesAsync();

                    }



                }



                Dictionary<string, object> dictResult = new Dictionary<string, object>();

                dictResult.Add("Success", ID);





                return Ok(dictResult);



            }

            catch (Exception ex)

            {



                throw;

            }

        }



        [HttpGet]

        [Route("GetPaymentMaster")]

        public async Task<ActionResult<Object>> GetPaymentMaster(string userID = "")

        {

            try

            {

                var obj = await _financeRepository.GetPaymentMaster(userID);





                return obj;





            }

            catch (Exception ex)

            {

                throw;

            }



        }



        [HttpGet]

        [Route("GetPaymentMasterCategoryType")]

        public async Task<ActionResult<Object>> GetPaymentMasterCategoryType(string cat = "")

        {

            try

            {

                var obj = await _oBMSDbContext.InventoryCategories.Where(x => x.Cat == cat).ToListAsync();





                return obj;





            }

            catch (Exception ex)

            {

                throw;

            }

        }





        [HttpGet]

        [Route("GetPaymentMasterCategoryTypeChangePayTo")]

        public async Task<ActionResult<Object>> GetPaymentMasterCategoryTypeChangePayTo(int cat)

        {

            try

            {



                var result = await _oBMSDbContext.PayToViews.Where(x => x.Category == cat).ToListAsync();



                return result;





            }

            catch (Exception ex)

            {

                throw;

            }



        }



        [HttpGet]

        [Route("GetPaymentSupplierInvoices")]

        public async Task<ActionResult> GetPaymentSupplierInvoices(int supplier, string userId)

        {

            try

            {

                // Calculate PaidAmount
                decimal paidAmount = await _oBMSDbContext.BranchPayments
                    .Where(x => x.Supplier == supplier && x.IsDeleted == false)
                    .SumAsync(x => (decimal?)x.Amount) ?? 0m;

                // Return shaped object with PaidAmount and Balance
                var result = await _oBMSDbContext.SupplierInvoiceViews
                    .Where(x => x.BranchUserName == userId && x.Supplier == supplier)
                    .Select(x => new
                    {
                        x.ID,
                        x.PaymentID,
                        x.Code,
                        x.Name,
                        x.InvoiceID,
                        x.InvoiceNo,
                        x.Total,
                        x.Amount,

                        PaidAmount = paidAmount,
                        Balance = x.Total - paidAmount,

                        x.BranchUserName,
                        x.Supplier
                    })
                    .ToListAsync();

                return Ok(result);

            }

            catch (Exception ex)

            {

                throw;

            }



        }



        [HttpGet]

        [Route("creditor-invoice-payment-details")]

        public async Task<List<CreditorInvoicePaymentRow>> GetCreditorInvoicePaymentList(string userId, decimal paymentId)

        {

            var sql = @"

                        SELECT

                            CAST(ISNULL(BPD.ID,0) AS INT) AS ID,

                            CAST(ISNULL(BPD.PaymentID,0) AS INT) AS PaymentID,

                            BM.Code,

                            BM.Name,

                            CAST(CI.ID AS INT) AS InvoiceID,

                            CI.InvoiceNo,

                            CI.Total,

                            ISNULL(P.PaidAmount,0) AS PaidAmount,

                            ISNULL(BPD.Amount,0) AS Amount

                        FROM CreditorInvoice CI

                        INNER JOIN OBMSBranches OB

                            ON OB.BranchCode = CI.Branch

                        INNER JOIN BranchMaster BM

                            ON BM.Code = OB.BranchCode

                        LEFT JOIN BranchPaymentDetails BPD

                            ON BPD.InvoiceID = CI.ID

                           AND BPD.PaymentID = @PaymentID

                           AND BPD.IsDeleted = 0

                        LEFT JOIN

                        (

                            SELECT InvoiceID, SUM(Amount) AS PaidAmount

                            FROM BranchPaymentDetails

                            WHERE InvoiceID <> 0 AND IsDeleted = 0

                            GROUP BY InvoiceID

                        ) P ON P.InvoiceID = CI.ID

                        WHERE OB.Name = @UserID

                            AND CI.IsDeleted = 0

                          AND (

                                @PaymentID <> 0

                                OR CI.Total - ISNULL(P.PaidAmount,0) > 0

                              )

                          AND (

                                @PaymentID = 0

                                OR BPD.ID IS NOT NULL

                              )

                        ORDER BY CI.PaymentDate";



            var parameters = new[]

            {

                new Microsoft.Data.SqlClient.SqlParameter("@UserID", userId),

                new Microsoft.Data.SqlClient.SqlParameter("@PaymentID", paymentId)

            };



            return await _oBMSDbContext.CreditorInvoicePaymentRows

                .FromSqlRaw(sql, parameters)

                .AsNoTracking()

                .ToListAsync();

        }



        [HttpGet]

        [Route("creditor-invoice-payment-details-by-supplier")]

        public async Task<List<CreditorInvoicePaymentRow>> getCreditorInvoicePaymentListBySupplier(string userId, decimal paymentId, decimal supplier)

        {

            try

            {

                var sql = @"

                    SELECT 

                        CAST(ISNULL(BPD.ID,0) AS INT) AS ID,

                        CAST(ISNULL(BPD.PaymentID,0) AS INT) AS PaymentID,

                        BM.Code,

                        BM.Name,

                        CAST(CI.ID AS INT) AS InvoiceID,

                        CI.InvoiceNo,

                        CI.Total,

                        ISNULL(BPD.Amount,0) AS Amount,

                        ISNULL(P.PaidAmount,0) AS PaidAmount

                    FROM CreditorInvoice CI

                    INNER JOIN OBMSBranches OB 

                        ON OB.BranchCode = CI.Branch

                    INNER JOIN BranchMaster BM 

                        ON BM.Code = OB.BranchCode

                    LEFT JOIN BranchPaymentDetails BPD 

                        ON BPD.InvoiceID = CI.ID 

                       AND BPD.PaymentID = @PaymentID

                       AND BPD.IsDeleted = 0

                    LEFT JOIN (

                        SELECT InvoiceID, SUM(Amount) AS PaidAmount

                        FROM BranchPaymentDetails

                        WHERE InvoiceID <> 0 AND IsDeleted = 0

                        GROUP BY InvoiceID

                    ) P 

                        ON P.InvoiceID = CI.ID

                    WHERE OB.Name = @UserID             

                      AND CI.IsDeleted = 0

                      AND CI.Supplier = @Supplier

                      AND (

                            @PaymentID <> 0               

                            OR CI.Total - ISNULL(P.PaidAmount,0) > 0 

                          )

                    ORDER BY CI.PaymentDate";



                var parameters = new[]

                {

                    new Microsoft.Data.SqlClient.SqlParameter("@UserID", userId),

                    new Microsoft.Data.SqlClient.SqlParameter("@PaymentID", paymentId),

                    new Microsoft.Data.SqlClient.SqlParameter("@Supplier", supplier)

                };



                return await _oBMSDbContext.CreditorInvoicePaymentRows

                    .FromSqlRaw(sql, parameters)

                    .AsNoTracking()

                    .ToListAsync();

            }

            catch (Exception ex)

            {

                return new List<CreditorInvoicePaymentRow>();

            }



        }



        [HttpGet]

        [Route("branch-payment-details")]

        public async Task<List<BranchPaymentRow>> GetBranchPaymentList(string userId, decimal paymentId)

        {

            var sql = @"
                        SELECT
                            CAST(ISNULL(BPD.ID,0) AS INT) AS ID,
                            CAST(ISNULL(BPD.PaymentID,0) AS INT) AS PaymentID,
                            ISNULL(BM.Code,'') AS Code,
                            ISNULL(BM.Name,'') AS Name,
                            BPD.Amount,
                            CAST(0 AS DECIMAL(18,2)) AS InvoiceID,
                            CAST(NULL AS NVARCHAR(50)) AS InvoiceNo,
                            CAST(0 AS DECIMAL(18,2)) AS Total,
                            CAST(NULL AS DECIMAL(18,2)) AS PaidAmount,
                            CAST(NULL AS DATETIME) AS PaymentDate
                        FROM OBMSBranches OB
                        INNER JOIN BranchMaster BM
                            ON BM.Code = OB.BranchCode
                        LEFT JOIN BranchPaymentDetails BPD
                            ON BPD.Branch = BM.Code
                           AND BPD.PaymentID = @PaymentID
                           AND BPD.IsDeleted = 0
                        WHERE OB.Name = @UserID
                        ORDER BY BM.Code";



            var parameters = new[]

            {

                new Microsoft.Data.SqlClient.SqlParameter("@UserID", userId),

                new Microsoft.Data.SqlClient.SqlParameter("@PaymentID", paymentId)

            };



            return await _oBMSDbContext.BranchPaymentRows

                .FromSqlRaw(sql, parameters)

                .AsNoTracking()

                .ToListAsync();

        }



        [HttpPost]

        [Route("SaveAndUpdatePayment")]

        public async Task<ActionResult<HttpResponseMessage>> SaveAndUpdatePayment(BranchPaymentRequestDto branchPaymentRequestDto)

        {

            // Use execution strategy for transaction management to prevent race conditions
            var strategy = _oBMSDbContext.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _oBMSDbContext.Database.BeginTransactionAsync();
                try
                {
                    var branchPayment = new BranchPayment();
                    if (branchPaymentRequestDto.ID != 0)
                    {
                        branchPayment = _oBMSDbContext.BranchPayments.Where(x => x.ID == branchPaymentRequestDto.ID)
                            .Select(bp => new BranchPayment
                            {
                                ID = bp.ID,
                                PaymentDate = bp.PaymentDate,
                                CreditorType = bp.CreditorType,
                                Supplier = bp.Supplier ?? 0, // Default value for nullable decimal
                                PaymentType = bp.PaymentType,
                                PaymentPurpose = bp.PaymentPurpose,
                                BankID = bp.BankID ?? 0, // Default value for nullable decimal
                                ChequeNo = bp.ChequeNo ?? string.Empty, // Default value for nullable string
                                PaymentTo = bp.PaymentTo ?? string.Empty, // Default value for nullable string
                                ItemCategory = bp.ItemCategory ?? string.Empty, // Default value for nullable string
                                Particulars = bp.Particulars ?? string.Empty, // Default value for nullable string
                                Amount = bp.Amount,
                                IsDeleted = bp.IsDeleted,
                                ChequeStatus = bp.ChequeStatus == '\0' ? ' ' : bp.ChequeStatus, // Default to space if empty char
                                LastUpdate = bp.LastUpdate,
                                VoucherNo = bp.VoucherNo ?? string.Empty // Default value for nullable string
                            }).FirstOrDefault();
                    }

                    bool shouldGenerateVoucher =
                         branchPaymentRequestDto.ID == 0 || string.IsNullOrWhiteSpace(branchPayment?.VoucherNo) ||
                          branchPayment.VoucherNo == "0" ||
                         branchPayment.BankID != branchPaymentRequestDto.BankID; // Bank changed

                    if (shouldGenerateVoucher)
                    {
                        // Fix race condition with proper locking
                        var newVoucherNo = _oBMSDbContext.BranchPayments
                            .FromSqlRaw(@"
                                SELECT TOP 1 bp.* 
                                FROM BranchPayments bp WITH (UPDLOCK, HOLDLOCK)
                                WHERE bp.PaymentType = @PaymentType 
                                AND (bp.BankID IS NOT NULL AND bp.BankID = @BankID)
                                AND YEAR(bp.PaymentDate) > 2012
                                ORDER BY CAST(CASE WHEN ISNUMERIC(bp.VoucherNo) = 1 THEN bp.VoucherNo ELSE '0' END AS INT) DESC",
                                new Microsoft.Data.SqlClient.SqlParameter("@PaymentType", branchPaymentRequestDto.PaymentType),
                                new Microsoft.Data.SqlClient.SqlParameter("@BankID", branchPaymentRequestDto.BankID))
                            .Select(bp => bp.VoucherNo)
                            .AsEnumerable()
                            .Select(v => int.TryParse(v, out var n) ? n : 0)
                            .DefaultIfEmpty(0)
                            .Max() + 1;

                        branchPayment.VoucherNo = newVoucherNo.ToString();
                    }
                    else
                    {
                        branchPayment.VoucherNo = branchPayment.VoucherNo;
                    }

                    branchPayment.ID = branchPaymentRequestDto.ID;
                    branchPayment.PaymentDate = branchPaymentRequestDto.PaymentDate;
                    branchPayment.CreditorType = branchPaymentRequestDto.CreditorType;
                    branchPayment.Supplier = branchPaymentRequestDto.Supplier;
                    branchPayment.PaymentType = branchPaymentRequestDto.PaymentType;
                    branchPayment.PaymentPurpose = branchPaymentRequestDto.PaymentPurpose;
                    branchPayment.BankID = branchPaymentRequestDto.BankID;
                    branchPayment.ChequeNo = branchPaymentRequestDto.ChequeNo;
                    branchPayment.PaymentTo = branchPaymentRequestDto.PaymentTo;
                    branchPayment.ItemCategory = branchPaymentRequestDto.ItemCategory;
                    branchPayment.Particulars = branchPaymentRequestDto.Particulars;
                    branchPayment.Amount = branchPaymentRequestDto.Amount;
                    branchPayment.IsDeleted = false;
                    branchPayment.ChequeStatus = branchPayment.ChequeStatus == '\0' ? ' ' : branchPayment.ChequeStatus;
                    branchPayment.LastUpdate = DateTime.Now;
                    branchPayment.LastUpdatedBy = branchPaymentRequestDto.userId;

                    if (branchPayment.ID == 0)
                    {
                        _oBMSDbContext.BranchPayments.Add(branchPayment);
                    }
                    else
                    {
                        _oBMSDbContext.BranchPayments.Update(branchPayment);
                    }

                    await _oBMSDbContext.SaveChangesAsync();
                    var savedPaymentId = branchPayment.ID; // Get the saved payment ID

                    if (branchPaymentRequestDto.details != null)
                    {
                        foreach (BranchPaymentDetailsRequestDto detail in branchPaymentRequestDto.details)
                        {
                            if (detail.Amount != 0)
                            {
                                var branchPaymentDetails = new BranchPaymentDetails();
                                if (detail.ID != 0)
                                {
                                    branchPaymentDetails = _oBMSDbContext.BranchPaymentDetails.Where(x => x.ID == detail.ID).FirstOrDefault();
                                }

                                branchPaymentDetails.ID = detail.ID;
                                branchPaymentDetails.PaymentID = branchPayment.ID;
                                branchPaymentDetails.Branch = detail.Branch;
                                branchPaymentDetails.InvoiceID = detail.InvoiceID;
                                branchPaymentDetails.Amount = detail.Amount;
                                branchPaymentDetails.LastUpdate = DateTime.Now;
                                branchPaymentDetails.LastUpdatedBy = branchPaymentRequestDto.userId;
                                branchPaymentDetails.IsDeleted = false;

                                if (branchPaymentDetails.ID == 0)
                                {
                                    _oBMSDbContext.BranchPaymentDetails.Add(branchPaymentDetails);
                                }
                                else
                                {
                                    _oBMSDbContext.BranchPaymentDetails.Update(branchPaymentDetails);
                                }
                            }
                        }
                    }

                    await _oBMSDbContext.SaveChangesAsync();
                    await transaction.CommitAsync();

                    Dictionary<string, object> dictResult = new Dictionary<string, object>();
                    dictResult.Add("Success", "Success");
                    dictResult.Add("PaymentID", savedPaymentId);

                    response.Headers.Add("Success", "Successfully save & update payment details.");

                    return Ok(dictResult);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error saving payment");
                    throw;
                }
            });
        }



        //public async Task<ActionResult<HttpResponseMessage>> SaveAndUpdatePayment(BranchPaymentRequestDto branchPaymentRequestDto)

        //{

        //    try

        //    {

        //        var branchPayment = new BranchPayment();

        //        bool isNewPayment = branchPaymentRequestDto.ID == 0;



        //        if (!isNewPayment)

        //        {

        //            branchPayment = _oBMSDbContext.BranchPayments

        //                .FirstOrDefault(x => x.ID == branchPaymentRequestDto.ID) ?? new BranchPayment

        //                {

        //                    LastUpdate = DateTime.Now,

        //                    ChequeStatus = 'N', // example default

        //                    IsDeleted = false,

        //                    PaymentDate = DateTime.Now

        //                };

        //        }



        //        // Only generate new voucher number if creating new record or voucher is missing

        //        if (isNewPayment || string.IsNullOrWhiteSpace(branchPayment.VoucherNo))

        //        {

        //            var newVoucherNo = _oBMSDbContext.BranchPayments

        //                .Where(bp =>

        //                    bp.PaymentType == branchPaymentRequestDto.PaymentType &&

        //                    (bp.BankID ?? 0) == branchPaymentRequestDto.BankID &&

        //                    bp.PaymentDate.Year > 2012)

        //                .Select(bp => bp.VoucherNo)

        //                .ToList()

        //                .Select(v => int.TryParse(v, out var n) ? n : 0)

        //                .DefaultIfEmpty(0)

        //                .Max() + 1;



        //            branchPayment.VoucherNo = newVoucherNo.ToString();

        //        }



        //        // Assign payment fields

        //        branchPayment.PaymentDate = branchPaymentRequestDto.PaymentDate;

        //        branchPayment.CreditorType = branchPaymentRequestDto.CreditorType;

        //        branchPayment.Supplier = branchPaymentRequestDto.Supplier;

        //        branchPayment.PaymentType = branchPaymentRequestDto.PaymentType;

        //        branchPayment.Supplier = branchPaymentRequestDto.Supplier;

        //        branchPayment.PaymentPurpose = branchPaymentRequestDto.PaymentPurpose;

        //        branchPayment.BankID = branchPaymentRequestDto.BankID;

        //        branchPayment.ChequeNo = branchPaymentRequestDto.ChequeNo ?? string.Empty;

        //        branchPayment.PaymentTo = branchPaymentRequestDto.PaymentTo ?? string.Empty;

        //        branchPayment.ItemCategory = branchPaymentRequestDto.ItemCategory ?? string.Empty;

        //        branchPayment.Particulars = branchPaymentRequestDto.Particulars ?? string.Empty;

        //        branchPayment.Amount = branchPaymentRequestDto.Amount;

        //        branchPayment.IsDeleted = false;

        //        branchPayment.ChequeStatus = branchPayment.ChequeStatus == '\0' ? ' ' : branchPayment.ChequeStatus;

        //        branchPayment.LastUpdate = DateTime.Now;

        //        branchPayment.LastUpdatedBy = branchPaymentRequestDto.userId;



        //        if (isNewPayment)

        //        {

        //            _oBMSDbContext.BranchPayments.Add(branchPayment);

        //        }

        //        else

        //        {

        //            _oBMSDbContext.BranchPayments.Update(branchPayment);

        //        }



        //        // Save header first to get the new ID (if new)

        //        await _oBMSDbContext.SaveChangesAsync();



        //        // Process payment details

        //        if (branchPaymentRequestDto.details != null)

        //        {

        //            foreach (var detail in branchPaymentRequestDto.details.Where(d => d.Amount != 0))

        //            {

        //                var branchPaymentDetails = detail.ID != 0

        //                    ? _oBMSDbContext.BranchPaymentDetails.FirstOrDefault(x => x.ID == detail.ID)

        //                    : new BranchPaymentDetails();



        //                if (branchPaymentDetails == null)

        //                    continue;



        //                branchPaymentDetails.PaymentID = branchPayment.ID;

        //                branchPaymentDetails.Branch = detail.Branch;

        //                branchPaymentDetails.InvoiceID = detail.InvoiceID;

        //                branchPaymentDetails.Amount = detail.Amount;

        //                branchPaymentDetails.LastUpdate = DateTime.Now;

        //                branchPaymentDetails.LastUpdatedBy = branchPaymentRequestDto.userId;

        //                branchPaymentDetails.IsDeleted = false;



        //                if (detail.ID == 0)

        //                    _oBMSDbContext.BranchPaymentDetails.Add(branchPaymentDetails);

        //                else

        //                    _oBMSDbContext.BranchPaymentDetails.Update(branchPaymentDetails);

        //            }



        //            await _oBMSDbContext.SaveChangesAsync();

        //        }



        //        response.Headers.Add("Success", "Successfully saved & updated details.");



        //        return Ok(new Dictionary<string, object> { { "Success", "Success" } });

        //    }

        //    catch (Exception)

        //    {

        //        // Add logging here for better debugging

        //        throw;

        //    }

        //}





        //public int GetNewVoucherNumber(int paymentType, decimal bankId)

        //{

        //    int currentYear = DateTime.Now.Year;



        //    var newVoucherNo = _oBMSDbContext.BranchPayments

        //        .Where(bp => bp.PaymentType == paymentType &&

        //                     (bp.BankID.HasValue ? bp.BankID.Value : 0) == bankId &&

        //                     bp.PaymentDate.Year > 2012)

        //        .Select(bp => bp.VoucherNo)

        //        .Max();



        //    int newVoucherNumber = int.TryParse(newVoucherNo, out int result) ? result + 1 : 1;



        //    return newVoucherNumber;

        //}





        [HttpGet]

        [Route("GetReceiptMaster")]

        public async Task<ActionResult<Object>> GetReceiptMaster(string userID = "")

        {

            try

            {

                var obj = await _financeRepository.GetReceiptMaster(userID);

                return obj;

            }

            catch (Exception ex)

            {

                throw;

            }



        }



        [HttpPost]

        [Route("SaveAndUpdateReceipt")]
        public async Task<ActionResult<HttpResponseMessage>> SaveAndUpdateReceipt(ReceiptsRequestDto receiptsRequestDto)
        {
            // Use execution strategy for transaction management to prevent race conditions
            var strategy = _oBMSDbContext.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _oBMSDbContext.Database.BeginTransactionAsync();
                try
                {
                    var sInvoiceNo = "";
                    ReceiptDetail receiptDetail = null;
                    int savedReceiptId = 0;
                    
                    foreach (ReceiptDetailRequestDto detail in receiptsRequestDto.details ?? Array.Empty<ReceiptDetailRequestDto>())
                    {
                        if (detail.InvoiceID > 0)
                        {
                            var clientInvoice = await _oBMSDbContext.ClientInvoices
                                .Where(x => x.ID == detail.InvoiceID)
                                .FirstOrDefaultAsync();

                            if (clientInvoice != null)
                                sInvoiceNo = sInvoiceNo + clientInvoice.InvoiceNo + ",";
                        }
                    }

                    Receipts receipt;

                    // LOAD WITHOUT TRACKING to avoid duplicate tracking
                    var existingReceipt = await _oBMSDbContext.Receipts
                        .AsNoTracking()
                        .FirstOrDefaultAsync(x => x.ID == receiptsRequestDto.ID && x.IsDeleted == false);

                    receipt = new Receipts();
                    receipt.ID = receiptsRequestDto.ID;

                    if (receipt.ID == 0)
                    {
                        receipt.VoucherNo = UtilityMain.NewReceiptVoucherNoByYear(
                            receiptsRequestDto.BankID,
                            receiptsRequestDto.ReceiptDate.Year);
                    }
                    else
                    {
                        if (existingReceipt != null)
                        {
                            if (existingReceipt.BankID == receiptsRequestDto.BankID)
                            {
                                receipt.VoucherNo = existingReceipt.VoucherNo;
                            }
                            else
                            {
                                receipt.VoucherNo = UtilityMain.NewReceiptVoucherNoByYear(
                                    receiptsRequestDto.BankID,
                                    receiptsRequestDto.ReceiptDate.Year);
                            }
                        }
                    }

                    // Map receipt properties
                    receipt.BankID = receiptsRequestDto.BankID;
                    receipt.ReceiptDate = receiptsRequestDto.ReceiptDate;
                    receipt.ReceiptAmount = receiptsRequestDto.ReceiptAmount;
                    receipt.PaymentFrom = receiptsRequestDto.PaymentFrom;
                    receipt.Branch = receiptsRequestDto.Branch;
                    receipt.Particulars = receiptsRequestDto.Particulars;
                    receipt.ReceiptType = receiptsRequestDto.ReceiptType;
                    receipt.IsInvoiceAdjustment = receiptsRequestDto.IsInvoiceAdjustment;
                    receipt.BankCode = receiptsRequestDto.BankCode;
                    receipt.BankBranch = receiptsRequestDto.BankBranch;
                    receipt.ChequeNo = receiptsRequestDto.ChequeNo;
                    receipt.InvoiceNumbers = sInvoiceNo;
                    receipt.TaxPercentage = receiptsRequestDto.TaxPercentage;
                    receipt.TaxAmount = receiptsRequestDto.TaxAmount;
                    receipt.HQPercentage = receiptsRequestDto.HQPercentage;
                    receipt.HQAmount = receiptsRequestDto.HQAmount;
                    receipt.BranchCollection = receiptsRequestDto.BranchCollection;
                    receipt.CreditNoteAmount = receiptsRequestDto.CreditNoteAmount;
                    receipt.DebitNoteAmount = receiptsRequestDto.DebitNoteAmount;
                    receipt.SuspendAmount = receiptsRequestDto.SuspendAmount;
                    receipt.ChequeStatus = receiptsRequestDto.ChequeStatus;
                    receipt.LastUpdate = DateTime.Now;
                    receipt.LastUpdatedBy = receiptsRequestDto.LastUpdatedBy;
                    receipt.IsDeleted = false;

                    if (receipt.ID == 0)
                    {
                        _oBMSDbContext.Receipts.Add(receipt);
                    }
                    else
                    {
                        _oBMSDbContext.Entry(receipt).State = EntityState.Modified;
                    }

                    await _oBMSDbContext.SaveChangesAsync();
                    savedReceiptId = receipt.ID;

                    // Save receipt details
                    foreach (ReceiptDetailRequestDto detail in receiptsRequestDto.details ?? Array.Empty<ReceiptDetailRequestDto>())
                    {
                        receiptDetail = new ReceiptDetail();
                        receiptDetail.ID = detail.ID;
                        receiptDetail.ReceiptID = savedReceiptId;
                        receiptDetail.InvoiceID = detail.InvoiceID;
                        receiptDetail.Amount = detail.Amount;
                        receiptDetail.BalanceStatus = detail.BalanceStatus;
                        receiptDetail.BalanceAmount = detail.BalanceAmount;
                        receiptDetail.LastUpdate = DateTime.Now;

                        if (receiptDetail.ID == 0)
                        {
                            _oBMSDbContext.ReceiptDetails.Add(receiptDetail);
                        }
                        else
                        {
                            _oBMSDbContext.Entry(receiptDetail).State = EntityState.Modified;
                        }

                        await _oBMSDbContext.SaveChangesAsync();
                    }

                    await transaction.CommitAsync();

                    Dictionary<string, object> dictResult = new Dictionary<string, object>();
                    dictResult.Add("Success", "Success");
                    dictResult.Add("ReceiptID", savedReceiptId);

                    return Ok(dictResult);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }



        [HttpGet]

        [Route("GetReceiptInvoiceByClient")]

        public async Task<ActionResult<Object>> GetReceiptInvoiceByClient(string client, string branch)

        {

            try

            {

                //return await _oBMSDbContext.ReceiptClientInvoiceViews.Where(x => x.Branch == branch).Where(x => x.Client == client).ToListAsync();



                var receiptIdParam = new SqlParameter("@ReceiptID", -1);

                var branchParam = new SqlParameter("@Branch", branch);

                var clientParam = new SqlParameter("@Client", client);



                var parameters = new[]

         {

    new Microsoft.Data.SqlClient.SqlParameter("@ReceiptID", -1),

    new Microsoft.Data.SqlClient.SqlParameter("@Branch", branch),

    new Microsoft.Data.SqlClient.SqlParameter("@Client", client)

};



                return await _oBMSDbContext.ClientReceiptDetailsResults.FromSqlRaw("EXEC GetClientReceiptDetails @ReceiptID, @Branch, @Client", parameters).ToListAsync();







            }

            catch (Exception ex)

            {

                throw;

            }

        }



        [HttpGet]
        [Route("invoice-details")]
        public async Task<List<InvoiceDetailRow>> GetInvoiceDetailList(string branch, string client, int receiptId)
        {
            try
            {
                var sql = @"
                        SELECT 
                            CAST(ReceiptDetails.ID AS INT) AS ID,
                            CAST(ReceiptDetails.ReceiptID AS INT) AS ReceiptID,
                            CAST(ClientInvoice.ID AS INT) AS InvoiceID,
                            ClientInvoice.InvoiceNo,
                            ClientInvoice.ServiceCharges - ClientInvoice.Discount + ClientInvoice.TaxAmount AS InvoiceAmount,
                            InvoicePayments.PaidAmount,
                            ClientInvoice.InvoiceDate,
                            (ClientInvoice.ServiceCharges - ClientInvoice.Discount + ClientInvoice.TaxAmount 
                             - InvoicePayments.PaidAmount) AS BalanceAmount
                        FROM ClientInvoice
                        INNER JOIN ReceiptDetails ON ReceiptDetails.InvoiceID = ClientInvoice.ID
                        INNER JOIN
                        (
                            SELECT InvoiceID, ISNULL(SUM(PaidAmount),0) AS PaidAmount
                            FROM
                            (
                                SELECT InvoiceID,
                                CASE 
                                    WHEN BalanceStatus = 3 THEN Amount
                                    WHEN BalanceStatus = 2 THEN Amount + BalanceAmount
                                    WHEN BalanceStatus = 1 THEN Amount
                                END AS PaidAmount
                                FROM ReceiptDetails RD1
                                INNER JOIN Receipts R 
                                    ON RD1.ReceiptId = R.ID AND R.IsDeleted = 0
                            ) AS ClientPayments
                            GROUP BY InvoiceID
                        ) AS InvoicePayments 
                            ON InvoicePayments.InvoiceID = ClientInvoice.ID
                        WHERE ReceiptDetails.ReceiptID = @ReceiptID
                          AND ClientInvoice.IsDeleted = 'N'

                        UNION

                        SELECT 
                            0 AS ID,
                            0 AS ReceiptID,
                            ClientInvoice.ID AS InvoiceID,
                            ClientInvoice.InvoiceNo,
                            ClientInvoice.ServiceCharges - ClientInvoice.Discount + ClientInvoice.TaxAmount AS InvoiceAmount,
                            ISNULL(InvoicePayments.PaidAmount,0) AS PaidAmount,
                            ClientInvoice.InvoiceDate,
                            (ClientInvoice.ServiceCharges - ClientInvoice.Discount + ClientInvoice.TaxAmount 
                             - ISNULL(InvoicePayments.PaidAmount,0)) AS BalanceAmount
                        FROM ClientInvoice
                        LEFT JOIN
                        (
                            SELECT InvoiceID, ISNULL(SUM(PaidAmount),0) AS PaidAmount
                            FROM
                            (
                                SELECT InvoiceID,
                                CASE 
                                    WHEN BalanceStatus = 3 THEN Amount
                                    WHEN BalanceStatus = 2 THEN Amount + BalanceAmount
                                    WHEN BalanceStatus = 1 THEN Amount
                                END AS PaidAmount
                                FROM ReceiptDetails RD1
                                INNER JOIN Receipts R 
                                    ON RD1.ReceiptId = R.ID AND R.IsDeleted = 0
                                WHERE RD1.InvoiceID NOT IN
                                (
                                    SELECT InvoiceID 
                                    FROM ReceiptDetails 
                                    WHERE ReceiptID = @ReceiptID
                                )
                            ) AS ClientPayments
                            GROUP BY InvoiceID
                        ) AS InvoicePayments 
                            ON InvoicePayments.InvoiceID = ClientInvoice.ID
                        WHERE ClientInvoice.ID NOT IN
                              (SELECT InvoiceID FROM ReceiptDetails WHERE ReceiptID = @ReceiptID)
                          AND ClientInvoice.ServiceCharges - ClientInvoice.Discount + ClientInvoice.TaxAmount
                              - ISNULL(InvoicePayments.PaidAmount,0) > 0
                          AND ClientInvoice.Branch = @Branch
                          AND ClientInvoice.Client = @Client
                          AND ClientInvoice.IsDeleted = 'N'
                        ORDER BY InvoiceNo";

                var parameters = new[]
                {
                    new Microsoft.Data.SqlClient.SqlParameter("@ReceiptID", receiptId),
                    new Microsoft.Data.SqlClient.SqlParameter("@Branch", branch),
                    new Microsoft.Data.SqlClient.SqlParameter("@Client", client)
                };

                return await _oBMSDbContext.InvoiceDetailRows
                        .FromSqlRaw(sql, parameters)
                        .ToListAsync();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [HttpGet]

        [Route("GetClientByBranch")]

        public async Task<ActionResult<Object>> GetClientByBranch(string branch)

        {

            try

            {

                return await _oBMSDbContext.ClientMasters.Where(x => x.Status != "Inactive" && x.Branch == branch).ToListAsync();

            }

            catch (Exception ex)

            {

                throw;

            }

        }



        [HttpGet]

        [Route("CheckUniqueChequeNoWithReceiptsNotDeleted")]

        public async Task<ActionResult<Object>> CheckUniqueChequeNoWithReceiptsNotDeleted(string Bank, string BankBranch, string ChequeNo)

        {

            try

            {



                return await _oBMSDbContext.Receipts.Where(x => x.BankCode == Bank).Where(x => x.BankBranch == BankBranch).Where(x => x.ChequeNo == ChequeNo).Where(x => x.IsDeleted == false).ToListAsync();

            }

            catch (Exception ex)

            {

                throw;

            }

        }



        [HttpGet]

        [Route("GetReceiptDetailRowAmount")]

        public async Task<ActionResult<Object>> GetReceiptDetailRowAmount(decimal ReceiptDetailsID, decimal ReceiptID, decimal InvoiceID)

        {

            try

            {

                return await _oBMSDbContext.ReceiptDetails.Where(x => x.ID == ReceiptDetailsID).Where(x => x.ReceiptID == ReceiptID).Where(x => x.InvoiceID == InvoiceID).ToListAsync();

            }

            catch (Exception ex)

            {

                throw;

            }

        }

        [HttpGet("GetSuppliers")]

        public async Task<IActionResult> GetSuppliers([FromQuery] string? category)

        {

            try

            {

                var suppliers = await _oBMSDbContext.Suppliers

                    .Where(s => string.IsNullOrEmpty(category) || (s.Category == category && s.Status == "A"))

                    .OrderBy(s => s.Name)

                    .Select(s => new

                    {

                        s.Id,

                        s.Name,

                        s.Code

                    })

                    .ToListAsync();



                var supplierList = new List<object>

            {

                new { Id = 0, Name = "", Code = "" }

            };



                supplierList.AddRange(suppliers);



                return Ok(supplierList);

            }

            catch (Exception ex)

            {

                return StatusCode(500, $"Internal server error: {ex.Message}");

            }

        }

        // Add missing DeletePayment endpoint from dev system
        [HttpPost("delete")]
        public async Task<IActionResult> Delete([FromBody] DeletePaymentRequest request)
        {
            var result = await _financeRepository.DeleteAsync(request.Id, request.CurrentUser);
            
            if (result)
            {
                return Ok(new { Success = true, Message = "Payment deleted successfully" });
            }
            else
            {
                return BadRequest(new { Success = false, Message = "Failed to delete payment" });
            }
        }

        [HttpGet("GetInventoryCategories")]

        public async Task<IActionResult> GetInventoryCategories([FromQuery] string? cat)

        {

            try

            {

                var query = _oBMSDbContext.InventoryCategories.AsQueryable();



                if (!string.IsNullOrEmpty(cat))

                {

                    query = query.Where(ic => ic.Cat == cat);

                }



                var inventoryCategoryFactoryList = await query
                    .OrderBy(ic => ic.Name)
                    .Select(ic => new InventoryCategory

                    {

                        ID = ic.ID,

                        Name = ic.Cat == "P" ? ic.Name + " (Purchase)" : ic.Name + " (Utility)",

                        Cat = ic.Cat,

                        AssetType = ic.AssetType

                    })
                    .ToListAsync();



                var inventoryCategoryList = new List<InventoryCategoryDto>

            {

                new InventoryCategoryDto(0, 0, "", 0, "", "", "")

            };



                inventoryCategoryList.AddRange(inventoryCategoryFactoryList.Select(icf => new InventoryCategoryDto(

                    icf.ID,

                    0, // AccountCategoryID placeholder

                    "", // AccountNo placeholder

                    0, // AccountTypeID placeholder

                    icf.Name,

                    icf.Cat,

                    icf.AssetType

                )));



                return Ok(inventoryCategoryList);

            }

            catch (Exception ex)

            {

                return StatusCode(500, $"Internal server error: {ex.Message}");

            }

        }



        [HttpGet("monthly-invoices")]

        public async Task<IActionResult> GetMonthlyInvoices(DateTime? invoiceStartPeriod = null, DateTime? invoiceEndPeriod = null)

        {

            try

            {
                // Use current month if parameters are not provided
                var today = DateTime.Today;
                var startPeriod = invoiceStartPeriod ?? new DateTime(today.Year, today.Month, 1);
                var endPeriod = invoiceEndPeriod ?? new DateTime(today.Year, today.Month, DateTime.DaysInMonth(today.Year, today.Month));

                // Validate that start period is before end period
                if (startPeriod > endPeriod)
                {
                    return BadRequest(new { message = "Start period must be before end period.", status = 400 });
                }

                var invoices = await _financeRepository.GetMonthlyInvoiceList(startPeriod, endPeriod);

                return Ok(invoices);

            }

            catch (Exception ex)

            {

                return StatusCode(500, new { message = "An error occurred while retrieving invoices.", details = ex.Message });

            }

        }



        [HttpGet("GetMonthlyInvoiseList")]
        public IActionResult GetMonthlyInvoiseList(DateTime Start, DateTime End)

        {

            try

            {

                var result = UtilityMain.GetMonthlyInvoiseList(Start, End);

                return Ok(result);

            }

            catch (Exception ex)

            {

                return BadRequest(new { Message = ex.Message });

            }

        }



        [HttpGet]

        [Route("GetList")]

        public IActionResult GetList(DateTime dtSalaryPeriod, DateTime dtEndPeriod)

        {

            try

            {

                var result = SumProfitLoss.GetList(dtSalaryPeriod, dtEndPeriod);

                return Ok(result);

            }

            catch (Exception ex)

            {

                return StatusCode(500, $"Internal server error: {ex.Message}");

            }

        }



        [HttpGet]

        [Route("GetListWithBranch")]

        public IActionResult GetListWithBranch(DateTime dtSalaryPeriod, DateTime dtEndPeriod, string Branch)

        {

            try

            {

                var result = SumProfitLoss.GetList(dtSalaryPeriod, dtEndPeriod, Branch);

                return Ok(result);

            }

            catch (Exception ex)

            {

                return StatusCode(500, $"Internal server error: {ex.Message}");

            }

        }



        [HttpGet("GetByDateAndBranch")]

        public async Task<IActionResult> GetByDateAndBranch(DateTime receiptDate, string branch)

        {

            var receipts = await _financeRepository.GetReceiptsByDateAndBranchAsync(receiptDate, branch);

            return Ok(receipts);

        }



        [HttpGet("GetByBankAndCheque")]

        public async Task<IActionResult> GetByBankAndCheque(string bankCode, string chequeNo, string branch)

        {

            var receipts = await _financeRepository.GetReceiptsByBankAndChequeAsync(bankCode, chequeNo, branch);

            return Ok(receipts);

        }



        [HttpGet("{id:int}")]

        public async Task<IActionResult> GetReceipt(int id)

        {

            var receipt = await _financeRepository.GetReceiptAsync(id);



            if (receipt == null)

                return NotFound();



            return Ok(receipt);

        }



        [HttpGet("byDate")]

        public async Task<IActionResult> GetListByDate(DateTime paymentDate)

        {

            try

            {

                var result = await _financeRepository.GetListByDateAsync(paymentDate);

                return Ok(result);

            }

            catch (Exception ex)

            {

                return StatusCode(500, ex.Message);

            }

        }



        [HttpGet("byBankAndCheque")]

        public async Task<IActionResult> GetListByBankAndCheque(decimal bankId, string chequeNo)

        {

            try

            {

                var result = await _financeRepository.GetListByBankAndChequeAsync(bankId, chequeNo);

                return Ok(result);

            }

            catch (Exception ex)

            {

                return StatusCode(500, ex.Message);

            }

        }



        [HttpGet("byPaymentID/{id}")]

        public async Task<IActionResult> GetBranchPayment(int id)

        {

            var branchPayment = await _financeRepository.GetBranchPaymentAsync(id);

            if (branchPayment == null)

            {

                return NotFound();

            }

            return Ok(branchPayment);

        }





        [HttpPost("receipt/delete")]

        public async Task<IActionResult> ReceiptDelete([FromBody] DeletePaymentRequest request)

        {

            var result = await _financeRepository.ReceiptDeleteAsync(request.Id, request.CurrentUser);



            if (!result)

                return NotFound(new { message = "Payment not found." });



            return Ok(result);

        }



        [HttpGet]

        [Route("GetNextChequeNumber")]

        public IActionResult GetNextChequeNumber(decimal account)

        {

            try

            {

                var result = UtilityMain.GetNextChequeNumber(account);

                return Ok(result);

            }

            catch (Exception ex)

            {

                return BadRequest(new { Message = ex.Message });

            }

        }



        [HttpGet]

        [Route("GetNoOfCheques")]

        public IActionResult GetNoOfCheques(decimal account)

        {

            try

            {

                var result = UtilityMain.GetNoOfCheques(account);

                return Ok(result);

            }

            catch (Exception ex)

            {

                return BadRequest(new { Message = ex.Message });

            }

        }



        [HttpGet("InventoryCategoryList")]

        public async Task<ActionResult<List<InventoryCategoryDto>>> GetList()

        {

            try

            {

                var result = await _financeRepository.GetListAsync();

                return Ok(result);

            }

            catch (Exception ex)

            {

                // Optionally log the exception here

                return StatusCode(500, new { Message = "An error occurred while retrieving the inventory categories.", Details = ex.Message });

            }

        }

        [HttpGet("GetPaymentListByCategory")]

        public async Task<ActionResult<object>> GetPaymentListByCategory(string categoryId, DateTime startDate, DateTime endDate)

        {

            try

            {

                var result = await _financeRepository.GetPaymentListByCategory(categoryId, startDate, endDate);

                return Ok(new { data = result });

            }

            catch (Exception ex)

            {

                return StatusCode(500, new { Message = "An error occurred while retrieving payment list by category.", Details = ex.Message });

            }

        }

        [HttpGet("GetBranchPaymentsTotalAmountByCategory")]

        public async Task<ActionResult<decimal>> GetBranchPaymentsTotalAmountByCategory(string categoryId, DateTime startDate, DateTime endDate)

        {

            try

            {

                var result = await _financeRepository.GetBranchPaymentsTotalAmountByCategory(categoryId, startDate, endDate);

                return Ok(result);

            }

            catch (Exception ex)

            {

                return StatusCode(500, new { Message = "An error occurred while retrieving total amount by category.", Details = ex.Message });

            }

        }



        [HttpGet("PaymentRecycleBin")]

        public IActionResult GetDeletedPayments(DateTime paymentDate)

        {

            try

            {

                var results = _financeRepository.GetDeletedPaymentsByMonthYear(paymentDate);

                return Ok(results);

            }

            catch (Exception ex)

            {

                // You can log the exception here using a logging framework if needed

                return StatusCode(StatusCodes.Status500InternalServerError, new

                {

                    Message = "An error occurred while retrieving deleted payments.",

                    Details = ex.Message

                });

            }

        }



        [HttpPost("restore-multiple")]

        public IActionResult RestoreMultiple([FromBody] RestoreMultipleRequest request)

        {

            if (request == null || request.Ids == null || !request.Ids.Any() || string.IsNullOrEmpty(request.CurrentUser))

                return BadRequest("Invalid request");



            var results = new Dictionary<int, bool>();



            try

            {

                foreach (var id in request.Ids)

                {

                    bool success = _financeRepository.Restore(id, request.CurrentUser);

                    results.Add(id, success);

                }



                return Ok(new { message = "Restore operation completed.", results });

            }

            catch (Exception ex)

            {

                // Optionally log the exception here

                return StatusCode(500, new { message = "An error occurred while restoring payments.", error = ex.Message });

            }

        }



        [HttpGet("cheque-status")]

        public async Task<IActionResult> Get(DateTime startDate, DateTime endDate, string chequeStatus, string bankCode, string transType)

        {

            try

            {

                var data = await _financeRepository.GetChequeStatusesAsync(startDate, endDate, chequeStatus, bankCode, transType);

                return Ok(data);

            }

            catch (Exception ex)

            {

                // Optionally log the exception here

                return StatusCode(StatusCodes.Status500InternalServerError, new

                {

                    Message = "An error occurred while fetching cheque statuses.",

                    Details = ex.Message

                });

            }

        }



        [HttpPost("restore-chequeStatus")]

        public async Task<IActionResult> RestoreChequeStatus([FromBody] RestoreChequeStatusRequest request)

        {

            if (request == null || request.Ids == null || !request.Ids.Any() || string.IsNullOrEmpty(request.CurrentUser))

                return BadRequest("Invalid request");



            var results = new Dictionary<int, bool>();



            try

            {

                foreach (var id in request.Ids)

                {

                    if (request.transType == "R")

                    {

                        var receipt = await _oBMSDbContext.Receipts
                            .Where(p => p.ID == id && !p.IsDeleted)
                            .FirstOrDefaultAsync();



                        if (receipt != null)

                        {

                            receipt.ChequeStatus = request.chequeStatus;

                            receipt.LastUpdate = DateTime.Now;

                            receipt.LastUpdatedBy = request.CurrentUser;

                            results[id] = true;

                        }

                        else

                        {

                            results[id] = false;

                        }

                    }

                    else // Assume transType == "P"

                    {

                        var branchPayment = await _oBMSDbContext.BranchPayments
                            .Where(p => p.ID == id && !p.IsDeleted)
                            .FirstOrDefaultAsync();



                        if (branchPayment != null)

                        {

                            branchPayment.ChequeStatus = request.chequeStatus;

                            branchPayment.ChqClearencedate = request.clearence_date;

                            branchPayment.LastUpdate = DateTime.Now;

                            branchPayment.LastUpdatedBy = request.CurrentUser;

                            results[id] = true;

                        }

                        else

                        {

                            results[id] = false;

                        }

                    }

                }



                await _oBMSDbContext.SaveChangesAsync();



                return Ok(new { message = "Restore operation completed.", results });

            }

            catch (Exception ex)

            {

                return StatusCode(500, new

                {

                    message = "An error occurred while restoring payments.",

                    error = ex.Message

                });

            }

        }



        [HttpGet("GetLegalDemandList")]

        public async Task<IActionResult> GetList([FromQuery] string branch = null, [FromQuery] string client = null, [FromQuery] string actionTaken = null)

        {



            try

            {

                var result = await _financeRepository.GetActionsAsync(branch, client, actionTaken);

                return Ok(result);

            }

            catch (Exception ex)

            {



                return StatusCode(500, "An error occurred while processing your request.");

            }

        }



        [HttpPost("saveOrUpdateLegalDemand")]

        public async Task<IActionResult> SaveOrUpdateLegalDemand([FromBody] ClientLegalDemandAction request)

        {

            try

            {

                var legalDemand = new ClientLegalDemandAction();



                if (request.ID != 0)

                {

                    legalDemand = _oBMSDbContext.ClientLegalDemandActions
                        .Where(x => x.ID == request.ID)
                        .Select(x => new ClientLegalDemandAction

                        {

                            ID = x.ID,

                            Branch = x.Branch ?? string.Empty,

                            Client = x.Client ?? string.Empty,

                            ActionTaken = x.ActionTaken ?? string.Empty,

                            DateIssue = x.DateIssue,

                            Remarks = x.Remarks ?? string.Empty,

                            DeletionRemarks = x.DeletionRemarks ?? string.Empty,

                            IsDeleted = x.IsDeleted,

                            CreatedDate = x.CreatedDate,

                            CreatedBy = x.CreatedBy ?? string.Empty,

                            UpdatedDate = x.UpdatedDate,

                            UpdatedBy = x.UpdatedBy ?? string.Empty

                        })
                        .FirstOrDefault();

                }



                // Assign values from request to entity

                legalDemand.Branch = request.Branch;

                legalDemand.Client = request.Client;

                legalDemand.ActionTaken = request.ActionTaken;

                legalDemand.DateIssue = request.DateIssue;

                legalDemand.Remarks = request.Remarks;

                legalDemand.DeletionRemarks = request.DeletionRemarks ?? string.Empty;

                legalDemand.IsDeleted = false;



                if (legalDemand.ID == 0)

                {

                    legalDemand.CreatedDate = DateTime.Now;

                    legalDemand.CreatedBy = request.CreatedBy;

                }



                legalDemand.UpdatedDate = DateTime.Now;

                legalDemand.UpdatedBy = request.UpdatedBy;





                if (legalDemand.ID == 0)

                {

                    _oBMSDbContext.ClientLegalDemandActions.Add(legalDemand);

                }

                else

                {

                    _oBMSDbContext.ClientLegalDemandActions.Update(legalDemand);

                }



                await _oBMSDbContext.SaveChangesAsync();



                Dictionary<string, object> result = new Dictionary<string, object>();

                result.Add("Success", "Success");



                Response.Headers["Success"] = "Successfully saved or updated legal demand action.";

                return Ok(result);

            }

            catch (Exception ex)

            {

                throw; // You can log ex here or return a 500 error response

            }

        }



        [HttpDelete("DeleteLegalDemand/{id}")]

        public async Task<IActionResult> DeleteItem(int id, string currentUser, string deleteRemarks)

        {

            try

            {

                var result = await _financeRepository.DeleteLegalDemandAsync(id, currentUser, deleteRemarks);



                if (result)

                    return Ok(new { success = true, message = "Item deleted (soft delete)" });



                return NotFound(new { success = false, message = "Item not found" });

            }

            catch (Exception ex)

            {

                return StatusCode(500, new

                {

                    success = false,

                    message = "An error occurred while deleting the item.",

                    error = ex.Message

                });

            }

        }



        [HttpGet("GetLegalDemandByID/{id}")]

        public async Task<IActionResult> GetLegalDemandByID(int id)

        {

            try

            {

                var result = await _financeRepository.GetLegalDemandByID(id);



                if (result != null)

                    return Ok(result);



                return NotFound(new { success = false, message = "Item not found" });

            }

            catch (Exception ex)

            {

                return StatusCode(500, new

                {

                    success = false,

                    message = "An error occurred while getting the item.",

                    error = ex.Message

                });

            }

        }



        [HttpPost("client-statement")]
        public IActionResult GenerateClientStatement([FromBody] ClientStatementRequest request)

        {

            try

            {

                UtilityMain.GenerateClientStatement(request.StartDate, request.EndDate, request.Branch, request.Client);

                return Ok(new { success = true });

            }

            catch (Exception ex)

            {

                return StatusCode(StatusCodes.Status500InternalServerError, new

                {

                    success = false,

                    message = "Failed to generate client statement."

                });

            }

        }



        [HttpPost("supplier-report")]

        public IActionResult ExecuteSupplierReport([FromBody] ClientActivityReportRequest request)

        {

            if (string.IsNullOrEmpty(request.Branch) || request.StartDate == default || request.EndDate == default)

            {

                return BadRequest("Branch, Start Date, and End Date are mandatory.");

            }



            try

            {

                UtilityMain.ExecuteSupplierReport(request.StartDate, request.EndDate, request.Branch, request.PayTo, request.Supplier, request.Status, request.Category);



                return Ok(new { success = true });

            }

            catch (Exception ex)

            {

                return StatusCode(StatusCodes.Status500InternalServerError, new

                {

                    success = false,

                    message = ex.Message

                });

            }

        }



        [HttpGet]

        [Route("GetTDSReport")]

        public async Task<IActionResult> GetTDSReport(string startDate, string endDate, string branch = "")

        {

            try

            {

                var query = $@"
                    SELECT
                        r.ID,
                        r.ReceiptDate,
                        r.VoucherNo,
                        r.Branch,
                        r.PaymentFrom,
                        ISNULL(r.ReceiptAmount, 0) AS ReceiptAmount,
                        ISNULL(r.TaxPercentage, 0) AS TaxPercentage,
                        ISNULL(r.TaxAmount, 0) AS TaxAmount,
                        ISNULL(r.HQPercentage, 0) AS HQPercentage,
                        ISNULL(r.HQAmount, 0) AS HQAmount,
                        ISNULL(r.BranchCollection, 0) AS BranchCollection,
                        r.Particulars
                    FROM Receipts r
                    WHERE r.IsDeleted = 0
                      AND CAST(r.ReceiptDate AS DATE) >= '{startDate}'
                      AND CAST(r.ReceiptDate AS DATE) <= '{endDate}'
                      AND (ISNULL(r.TaxAmount, 0) > 0 OR ISNULL(r.HQAmount, 0) > 0)";

                if (!string.IsNullOrWhiteSpace(branch))

                {

                    query += $" AND r.Branch = '{branch}'";

                }

                query += " ORDER BY r.ReceiptDate ASC, r.Branch ASC";

                var result = await _oBMSDbContext.TDSReportResults.FromSqlRaw(query).ToListAsync();

                return Ok(result);

            }

            catch (Exception ex)

            {

                return StatusCode(500, ex.Message);

            }

        }
    }







public class ClientStatementRequest
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Branch { get; set; }
    public string Client { get; set; }
}

public class ClientActivityReportRequest
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Branch { get; set; } = string.Empty;
    public decimal Supplier { get; set; }
    public decimal PayTo { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}




[Keyless]

public class ClientInvoiceNoResult

{

    public int NEWCLIENTINVOICENO { get; set; }

}



public class ClientMasterResult

{

    [Key]

    public string Code { get; set; }

    public string Branch { get; set; }

    public string Name { get; set; }



    public int ID { get; set; }

    public string InvoiceNo { get; set; }

}



public class RestoreInvoiceListRequest

{

    public int[]? ID { get; set; }

}



public class BatchInvoice

{

    public string Code { get; set; }

    public string Branch { get; set; }

    public string Name { get; set; }

    public int ID { get; set; }

    public string InvoiceNo { get; set; }



    public Object data { get; set; }

}



[Table("PayToView")]

[Keyless]

public class PayToView

{

    public int ID { get; set; }

    public string Name { get; set; }

    public int Category { get; set; }

}



[Table("SupplierInvoiceView")]

[Keyless]

public class SupplierInvoiceView

{

    public int? ID { get; set; }

    public decimal? PaymentID { get; set; }

    public string? Code { get; set; }

    public string? Name { get; set; }

    public int? InvoiceID { get; set; }

    public string? InvoiceNo { get; set; }

    public decimal? Total { get; set; }

    public decimal? Amount { get; set; }

    public decimal? PaidAmount { get; set; }

    public decimal? Balance { get; set; }

    public string? BranchUserName { get; set; }

    public int? Supplier { get; set; }

}



[Keyless]

public class ClientReceiptDetailsResult

{

    public int ID { get; set; }

    public decimal ReceiptID { get; set; }

    public int InvoiceID { get; set; }

    public string InvoiceNo { get; set; }

    public decimal InvoiceAmount { get; set; }

    public decimal PaidAmount { get; set; }

    public DateTime InvoiceDate { get; set; }

}

public class DeletePaymentRequest

{

    public int Id { get; set; }

    public string CurrentUser { get; set; }

}



public class RestoreMultipleRequest

{

    public List<int> Ids { get; set; }

    public string CurrentUser { get; set; }

}



public class RestoreChequeStatusRequest

{

    public List<int> Ids { get; set; }

    public string CurrentUser { get; set; }

    public string transType { get; set; }

    public char chequeStatus { get; set; }

    public DateTime clearence_date { get; set; }

}

[Keyless]

public class TDSReportResult

{

    public int ID { get; set; }

    public DateTime? ReceiptDate { get; set; }

    public string? VoucherNo { get; set; }

    public string? Branch { get; set; }

    public string? PaymentFrom { get; set; }

    public decimal ReceiptAmount { get; set; }

    public decimal TaxPercentage { get; set; }

    public decimal TaxAmount { get; set; }

    public decimal HQPercentage { get; set; }

    public decimal HQAmount { get; set; }

    public decimal BranchCollection { get; set; }

    public string? Particulars { get; set; }

}

}
