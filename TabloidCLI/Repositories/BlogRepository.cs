using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using TabloidCLI.Models;
using TabloidCLI.Repositories;

namespace TabloidCLI.Repositories
{
    /// the class is like the blue print. Its responible for interacting with blog data
    ///  Interface which is like a contract 
    public class BlogRepository : DatabaseConnector, IRepository<Blog>
    {
        ///  When new BlogRespository is instantiated-the process of creating an object from a class , pass the connection string along to the BaseRepository
        public BlogRepository(string connectionString) : base(connectionString) { }

        /// this will return a single blog with the given id
        public Blog Get(int id)
        {
          
            //  In C#, a "using" block ensures we correctly disconnect from a resource even if there is an error.
            //  For database connections, this means the connection will be properly closed.
            using (SqlConnection conn = Connection)
            {
                //opening the connection
                conn.Open();
                //we use commands
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    // Here we setup the command with the SQL we want to execute before we execute it.
                    //the @ symbol tell we are using params
                    cmd.CommandText = @"SELECT blog.Id As BlogId, blog.Title, blog.Url, tag.Id As TagId, tag.Name FROM Blog
                                           LEFT JOIN BlogTag blogtag  on blog.Id = BlogTag.BlogId
                                           LEFT JOIN Tag tag on tag.Id = blogtag.TagId
                                           WHERE blog.id = @id";
                    
                    cmd.Parameters.AddWithValue("@id", id);
           
                    Blog blog = null;

                    //Execute the SQL in the database and get a "reader" that will give us access to the data.
                    SqlDataReader reader = cmd.ExecuteReader();
                    //this returns ture if there is more data to read
                    while (reader.Read())
                    {
                        if (blog == null)
                        {
                            //creating a new blog object
                            blog = new Blog
                            {
                                // The "ordinal" is the numeric position of the column in the query results.
                                //we  use the reader's Get method to get the value of  a particular ordinal
                                Id = reader.GetInt32(reader.GetOrdinal("BlogId")),
                                Title = reader.GetString(reader.GetOrdinal("Title")),
                                Url = reader.GetString(reader.GetOrdinal("Url"))
                            };
                        }
                        if(!reader.IsDBNull(reader.GetOrdinal("TagId")))
                        {
                            //adding a blog tag
                            blog.Tags.Add(new Tag()
                            {
                                // The "ordinal" is the numeric position of the column in the query results.
                                //we  use the reader's Get method to get the value of  a particular ordinal
                                Id = reader.GetInt32(reader.GetOrdinal("TagId")),
                                Name = reader.GetString(reader.GetOrdinal("Name"))
                            });
                        }
                    }
                    //cloing the reader
                    reader.Close();
                    //returns the blog 
                    return blog;
                }

            }
        }

        //this will get a list of Blogs
        public List<Blog> GetAll()
        {
            //the using is to ensuring that we disconnect.  This allows us to interact with the database
            using(SqlConnection conn = Connection)
            {
                //we are opening the connnection
                conn.Open();
                //using command
                using(SqlCommand cmd = conn.CreateCommand())
                {
                    // Here we setup the command with the SQL we want to execute before we execute it.
                    cmd.CommandText = "SELECT Id, Title, Url FROM Blog";

                    //A list to hold the blogs we retrieve from the database.
                    List<Blog> blogs = new List<Blog>();
                    // Execute the SQL in the database and get a "reader" that will give us access to the data.
                    SqlDataReader reader = cmd.ExecuteReader();


                    // Read() will return true if there's more data to read
                    while (reader.Read())
                    {
                        //creating a new blog object using the database
                        Blog blog = new Blog()
                        {
                            // The "ordinal" is the numeric position of the column in the query results.
                            //we  use the reader's Get method to get the value of  a particular ordinal
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Title = reader.GetString(reader.GetOrdinal("Title")),
                            Url = reader.GetString(reader.GetOrdinal("Url"))
                        };
                        //we add the blog object to our list
                        blogs.Add(blog);
                    }
                    //we close the reader
                    reader.Close();
                    //return the list of blogs
                    return blogs;
                }
            }
        }

        //adding a new blog to the database. There is no return because we are sending data to the database
        public void Insert(Blog blog)
        {
            //using ensures we disconnect from database
            using (SqlConnection conn = Connection)
            {
                //opens the connection
                conn.Open();
                //using the command
                using(SqlCommand cmd = conn.CreateCommand())
                {
                    //Inserting a blog into the database and the database its adding an id and adding the values title and blog
                    //the @ symbol tell we are using params
                    cmd.CommandText = @"INSERT INTO Blog (Title, Url)
                                       OUTPUT INSERTED.Id
                                        VALUES (@title, @url)";
                    cmd.Parameters.AddWithValue("@title", blog.Title);
                    cmd.Parameters.AddWithValue("@url", blog.Url);
                    //this is executing the query 
                    int id = (int)cmd.ExecuteScalar();
                    
                    blog.Id = id;
                }
            }
        }

        //updating a Blog
        public void Update(Blog blog)
        {
            //using is ensuring we disconnect
            using (SqlConnection conn = Connection)
            {
                //opening the connection
                conn.Open();
                //using command
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    //update the Blog, set the title and url where id = id
                    //the @ symbol tell we are using params
                    cmd.CommandText = @"UPDATE Blog
                                        SET Title = @title,
                                            Url = @url
                                      WHERE id = @id";
                    cmd.Parameters.AddWithValue("@title", blog.Title);
                    cmd.Parameters.AddWithValue("@url", blog.Url);
                    cmd.Parameters.AddWithValue("@id", blog.Id);
                    //we want you to execute this query but don't except you to give us anything back 
                    cmd.ExecuteNonQuery();
                }
            }
        }


        //deleting a blog with the given id
        public void Delete(int id)
        {
            //using ensures we disconnect
            using (SqlConnection conn = Connection)
            {
                //opening the connection
                conn.Open();
                //using command
                using(SqlCommand cmd = conn.CreateCommand())
                {
                    //the @ symbol tell we are using params
                    //delete from these tables where the blog or id equal id
                    cmd.CommandText = @"DELETE FROM Post WHERE BlogId = @id; 
                                        DELETE FROM BlogTag WHERE BlogId = @id;
                                        DELETE FROM Blog WHERE id = @id;";
                    cmd.Parameters.AddWithValue("@id", id);
                    //we want you to execute this query but don't except you to give us anything back 
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void InsertTag(Blog blog, Tag tag)
        {
            using(SqlConnection conn = Connection)
            {
                conn.Open();
                using(SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO BlogTag (BlogId, TagId)
                                               VALUES (@blogId, @tagId)";
                    cmd.Parameters.AddWithValue("@blogId", blog.Id);
                    cmd.Parameters.AddWithValue("@tagId", tag.Id);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void DeleteTag(int blogId, int tagId)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using(SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"Delete FROM BlogTag
                                             WHERE BlogId = @blogId AND 
                                             TagId = @tagId";
                    cmd.Parameters.AddWithValue("@blogId", blogId);
                    cmd.Parameters.AddWithValue("@tagId", tagId);
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
