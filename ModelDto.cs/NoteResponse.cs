namespace apitest
{
    public class NoteResponse
    {
       
       public Guid id {get;set;}
       public string? title{get; set;}= null;
       public string? description {get;set;}= null;
       public string? lastEdit { get; set; } = null;
    }
}