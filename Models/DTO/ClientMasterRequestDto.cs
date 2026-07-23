namespace OBMS.WebAPI.Models.DTO
{
    public class ClientMasterRequestDto
    {
        public int ID { get; set; }

        public string Code { get; set; }

        public string Name { get; set; }

        public string Address1 { get; set; }

        public string Address2 { get; set; }

        public string PostCode { get; set; }

        public string City { get; set; }

        public string State { get; set; }

        public string Phone { get; set; }

        public string Fax { get; set; }

        public string Email { get; set; }

        public string Branch { get; set; }

        public string Status { get; set; }

        public string SuperClientCode { get; set; }

        public string PersonIncharge { get; set; }

        public DateTime? AgreementStart { get; set; }

        public DateTime? AgreementEnd { get; set; }

        public string? Shortname { get; set; }

        public bool? IsClientHeadQuarters { get; set; }

        public DateTime? LastUpdatedDate { get; set; }

        public DateTime? CreatedDate { get; set; }

        public string LastUpdatedBy { get; set; }

        // Indian Compliance Fields
        public string? GSTIN { get; set; }

        public string? PANNumber { get; set; }

        public string? TANNumber { get; set; }

        public string? CINNumber { get; set; }

        public string? GSTRegistrationStatus { get; set; }

        public string? IndianState { get; set; }

        public string? PINCode { get; set; }

        // Shipping Address Fields
        public string? ShippingAddress1 { get; set; }

        public string? ShippingAddress2 { get; set; }

        public string? ShippingCity { get; set; }

        public string? ShippingState { get; set; }

        public string? ShippingPINCode { get; set; }

        // Billing Address Fields
        public string? BillingAddress1 { get; set; }

        public string? BillingAddress2 { get; set; }

        public string? BillingCity { get; set; }

        public string? BillingState { get; set; }

        public string? BillingPINCode { get; set; }
        
        // Simplified Compliance Field
        public string? ClientComplianceStatus { get; set; }
    }
}
