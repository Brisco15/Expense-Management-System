namespace ExpenseManagement.Application.DTOs
{
    public class UpdateCategoryDto
    {
       public int CategoryId { get; set; }
        public string Description { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
    }
}
