namespace apitest
{
    public class Token
    {
        public int UserId { get; set; }
        public string TokenValue { get; set; }
    
    
    
     public Token()
     {
          if(TokenValue == null )
        {
            TokenValue = "";
        }
     }
    }
}