namespace comercializadora_api.Models.Dtos
{
    public class PaginadorRequestDto
    {
        public PaginadorRequestDto() {
            this.page = 1;
            this.pageSize = 25;
            this.orderType = "desc";
        }



        public Dictionary<string, string>? filters { get; set; }
        public int page { get; set; }
        public int pageSize { get; set; }
        public string? orderBy { get; set; }
        public string? orderType { get; set; }
    }
}
