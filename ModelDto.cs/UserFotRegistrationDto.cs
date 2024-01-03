namespace apitest
{
  public partial class UserForRegistrationDto
  {
        public string Email  {get; set;}
        public string Password  {get; set;}
        
      
    
    
        public UserForRegistrationDto()
        {
            if( Email == null)
            {
                Email = "";
            }
             if( Password == null)
            {
                Password = "";
            }
           
           
            
        }
        

  }

}