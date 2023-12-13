using api.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace apitest
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    
    public class PostController : ControllerBase
    {
        private readonly Datadapper _dapper;

        public PostController ( IConfiguration config)
        {
            _dapper = new Datadapper(config);
        }
       [HttpGet("Posts")]
        public IEnumerable<Post> GetPosts()
        {
            string sql = @"SELECT [PostId],
                    [UserId],
                    [PostTitle],
                    [PostContent],
                    [PostCreated],
                    [PostUpdated] 
                FROM dbo.Posts";
                
            return _dapper.LoadData<Post>(sql);
        }
       
       [HttpGet("PostSingle/{postId}")]

       public Post GetPosts(int postId)
       {
        string sql = @"SELECT [PostId],
                    [UserId],
                    [PostTitle],
                    [PostContent],
                    [PostCreated],
                    [PostUpdated] 
             from dbo.Posts
             WHERE PostId ="+ postId.ToString();
             return _dapper.LoadDataSingle<Post>(sql);
       }
       [HttpGet("PostsByUser/{userId}")]
        public IEnumerable<Post> GetPostsByUser(int userId)
        {
            string sql = @"SELECT [PostId],
                    [UserId],
                    [PostTitle],
                    [PostContent],
                    [PostCreated],
                    [PostUpdated] 
                FROM dbo.Posts
                    WHERE UserId = " + userId.ToString();
                
            return _dapper.LoadData<Post>(sql);
        }
       [HttpGet("MyPosts")]
        public IEnumerable<Post> GetMyPosts()
        {
            string sql = @"SELECT [PostId],
                    [UserId],
                    [PostTitle],
                    [PostContent],
                    [PostCreated],
                    [PostUpdated] 
                FROM dbo.Posts
                    WHERE UserId = " + this.User.FindFirst("userId")?.Value;
                
            return _dapper.LoadData<Post>(sql);
        }
 
     [HttpGet("PostsBySearch/{searchParam}")]
        public IEnumerable<Post> PostsBySearch(string searchParam)
        {
            string sql = @"SELECT [PostId],
                    [UserId],
                    [PostTitle],
                    [PostContent],
                    [PostCreated],
                    [PostUpdated] 
                FROM dbo.Posts
                    WHERE PostTitle LIKE '%" + searchParam + "%'" +
                        " OR PostContent LIKE '%" + searchParam + "%'";
                
            return _dapper.LoadData<Post>(sql);
        }

       [HttpPost("Post")]
        public IActionResult AddPost(PostToAddDto postToAdd)
        {
            string sql = @"
            INSERT INTO dbo.Posts(
                [UserId],
                [PostTitle],
                [PostContent],
                [PostCreated],
                [PostUpdated]) VALUES (" + this.User.FindFirst("userId")?.Value
                + ",'" + postToAdd.PostTitle
                + "','" + postToAdd.PostContent
                + "', GETDATE(), GETDATE() )";
            if (_dapper.ExecuteSQL(sql))
            {
                return Ok();
            }

            throw new Exception("Failed to create new post!");
        }
       [HttpPut("Putpost")]
        public IActionResult EditPost(PostToEditDto postToEdit)
        {
            string sql = @"
            UPDATE dbo.Posts 
                SET PostContent = '" + postToEdit.PostContent + 
                "', PostTitle = '" + postToEdit.PostTitle + 
                @"', PostUpdated = GETDATE()
                    WHERE PostId = " + postToEdit.PostId.ToString() +
                    "AND UserId = " + this.User.FindFirst("userId")?.Value;

            if (_dapper.ExecuteSQL(sql))
            {
                return Ok();
            }

            throw new Exception("Failed to edit post!");
        }
      [HttpDelete("Post/{postId}")]
        public IActionResult DeletePost(int postId)
        {
            string sql = @"DELETE FROM dbo.Posts 
                WHERE PostId = " + postId.ToString()+
                    "AND UserId = " + this.User.FindFirst("userId")?.Value;

            
            if (_dapper.ExecuteSQL(sql))
            {
                return Ok();
            }

            throw new Exception("Failed to delete post!");
        }

     }
}
