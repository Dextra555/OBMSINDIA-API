namespace OBMS.WebAPI.Models.DTO
{
    public class QuotationDetailsRequestDto
    {
        public int ID { get; set; }
        public int QuotationID { get; set; }
        public string Description { get; set; }
        public int NoOfGuards { get; set; }
        public decimal PerDay { get; set; }
        public decimal PerMonth { get; set; }
        public decimal Rate { get; set; }
        public decimal NoOfHours { get; set; }
        public decimal NoOfDays { get; set; }
        public bool FollowCalender { get; set; }
        public bool HasDiscount { get; set; }
        public decimal DiscountAmount { get; set; }
        public bool IsTaxable { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal MonthTotal { get; set; }
        public int DiscountHour { get; set; }
        public string Category { get; set; }
        public string Reason { get; set; }
        public int? ServiceTypeID { get; set; }

        public decimal Basic { get; set; }
        public decimal DA { get; set; }
        public decimal Leaves { get; set; }
        public decimal LeavesPercentage { get; set; }
        public decimal Allowance { get; set; }
        public decimal Bonus { get; set; }
        public decimal BonusPercentage { get; set; }
        public decimal NFH { get; set; }
        public decimal PF { get; set; }
        public decimal PFPercentage { get; set; }
        public decimal ESI { get; set; }
        public decimal ESIPercentage { get; set; }
        public decimal Uniform { get; set; }
        public decimal ServiceFee { get; set; }
        public decimal HRA { get; set; }
        public decimal HRAPercentage { get; set; }
        public decimal ProfessionalTax { get; set; }
        public decimal RelieverCharges { get; set; }
        public decimal RelieverChargesPercentage { get; set; }
        public decimal Others { get; set; }
        public decimal OthersPercentage { get; set; }
        public decimal AdministrationCharges { get; set; }
        public decimal AdministrationChargesPercentage { get; set; }
        public decimal ManagementFee { get; set; }
        public decimal ManagementFeePercentage { get; set; }
        public decimal SubTotal { get; set; }
        public decimal TotalPlusStatutory { get; set; }
        public decimal TotalDirectCost { get; set; }
        public decimal MonthlyChargedCost { get; set; }
    }
}
