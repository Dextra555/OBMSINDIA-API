namespace OBMS.WebAPI.Models.DTO
{
    public class VoucherDetailDto
    {
        public int ID { get; set; }
        public string? VoucherNo { get; set; }
        public DateTime Date { get; set; }
        public string? EmployeeName { get; set; }
        public decimal Amount { get; set; }
        public string? PaymentType { get; set; }
        public string? EmployeeCode { get; set; }
    }
}
