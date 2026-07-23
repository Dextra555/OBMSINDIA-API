namespace OBMS.WebAPI.Models.DTO
{
    public class RbiBankSalaryExportDto
    {
        public string FieldName { get; set; } = "";
        public string TransactionType { get; set; } = "";
        public string BeneficiaryCode { get; set; } = "";
        public string BeneficiaryAccountNumber { get; set; } = "";
        public string TransactionAmount { get; set; } = "";
        public string BeneficiaryName { get; set; } = "";
        public string CustomerReferenceNumber { get; set; } = "";
        public string PaymentDetails1 { get; set; } = "";
        public string PaymentDetails2 { get; set; } = "";
        public string PaymentDetails3 { get; set; } = "";
        public string PaymentDetails4 { get; set; } = "";
        public string PaymentDetails5 { get; set; } = "";
        public string PaymentDetails6 { get; set; } = "";
        public string PaymentDetails7 { get; set; } = "";
        public string PaymentDetails8 { get; set; } = "";
        public string ChargeMaster { get; set; } = "";
        public string ChequeDate { get; set; } = "";
        public string MICRNumber { get; set; } = "";
        public string IFSCCode { get; set; } = "";
        public string BeneficiaryBankName { get; set; } = "";
    }
}
