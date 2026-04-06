namespace Books.API.Models
{
    public class BookWithCoverDto
    {
        public string Id { get; set; }
        //public byte[]? Content { get; set; }

        public BookWithCoverDto(string id)
        {
            Id = id;
        }

        //public BookWithCoverDto(string id, byte[]? content)
        //{
        //    Id = id;
        //    Content = content;
        //}
    }
}
