using System;  
using System.Data;  
using System.Data.SqlClient; 
using Microsoft.Extensions.Configuration;


namespace AdoNetConsoleApplication      
{ 
    class Program  
    {  
        public static void Main(string[] args)  
        {  

            Console.WriteLine("\n\n*************************************************\n*************       WELCOME       ***************\n*************************************************\n");
            //Iitializing the configuration
            var config = new ConfigurationBuilder()
                            .SetBasePath(Directory.GetCurrentDirectory())
                            .AddJsonFile("appsettings.json")
                            .Build();

            
            string? execute = "y";

            while(execute=="y" || execute=="Y")
            {
                

                SqlConnection? connectionString = null;  
                start:
                try
                {  
                    // Creating Connection 
                    here:

                    //Receiving database name as input
                    Console.Write("Enter the Database Name : "); 
                    string? db_name = Console.ReadLine();
                    connectionString = new SqlConnection($"data source=.; database={db_name}; integrated security=SSPI");
                    connectionString.Open();
                    Console.WriteLine("\n*******Databse connection established*******\n");

                    
                    //Getting table name as input
                    Console.Write("Enter table name : ");
                    string? tb_name= Console.ReadLine();


                    input:
                    Console.WriteLine("Which operation to perform??\n\n1) Insert\n2) Read\n3) Update\n4) Delete\n\n");
                    int crudOption;
                    try
                    {
                        crudOption = Convert.ToInt32(Console.ReadLine());
                    }
                    catch(Exception e)
                    {
                        Console.WriteLine("!! Enter a Valid Option [from 1 to 4] !!");
                        goto input;
                    }
                    // CRUD operations
                    switch(crudOption)
                    {
                        case 1:
                            Insert(connectionString,tb_name);
                            Console.WriteLine("****************************************************\n*************Inserted Succcessfully*************\n*************************************************");
                            break;

                        case 2:
                            Read(connectionString,tb_name);
                            break;

                        case 3:
                            Update(connectionString,tb_name,config);
                            Console.WriteLine("****************************************************\n*************Updated successfully*************\n*************************************************");
                            break;

                        case 4:
                            Delete(connectionString,tb_name);
                            Console.WriteLine("*************************************************\n*************Row Deleted Succesfully*************\n*************************************************");
                            break;

                        default:
                            Console.WriteLine("!!!!!!!!!!!!!Enter a Valid Option!!!!!!!!!!!!!");
                            goto here;
                    }
                }

                //catching the exception
                catch (Exception e)  
                {
                    Console.WriteLine("\nDatabase or table name is incorrect\n\n!!Enter a valid name!!");  
                    goto start;
                }
                

                // Closing the connection
                finally  
                {
                    connectionString.Close();  
                }
                Console.WriteLine("\n\n Want to continue??\n\n Y/y   ");
                execute = Console.ReadLine();
            }
        } 

        //INSERT Operation
        public static void Insert(SqlConnection connectionString,string? tb_name)
        {
            insert:
            Console.WriteLine("\n        Enter the values\n");

            //Getting the schema of the table
            DataTable schemaTable = connectionString.GetSchema("Columns", new[] { null, null, tb_name });

            //Generating the query command
            string queryString = $"INSERT INTO {tb_name} VALUES (";

            foreach (DataRow row in schemaTable.Rows)
            {
                string? columnName = row["COLUMN_NAME"].ToString();
                Console.Write(columnName +" : ");
                string? str1 = Console.ReadLine();
                queryString = queryString + str1 +",";
            }

            string LastChar = queryString.Remove(queryString.Length - 1, 1);
            LastChar+=')';
            Console.WriteLine(LastChar);
        
            //Executing the Query
            try
            {
                SqlCommand command = new SqlCommand(LastChar,connectionString);
                command.ExecuteNonQuery();
            }
            catch(Exception e)
            {
                Console.WriteLine("  !!Error in the Query!!  ");
                goto insert;
            }
        }


        //READ operation
        public static void Read(SqlConnection connectionString,string? tb_name)
        {
            DataTable schemaTable = connectionString.GetSchema("Columns", new[] { null, null, tb_name });

            //Displaying the column name
            //Console.WriteLine(schemaTable);
            foreach (DataRow row in schemaTable.Rows)
            {
                string? columnName = row["COLUMN_NAME"].ToString();
                Console.Write($"{columnName,-20}");
            }
            Console.WriteLine();
            Console.WriteLine();

            //Executing the query
            SqlCommand command = new SqlCommand($"Select * from {tb_name}", connectionString);
            SqlDataReader sdr = command.ExecuteReader();


            while(sdr.Read())
            {
                for(int i = 0; i < sdr.FieldCount; i++)
                {
                    Console.Write($"{sdr[i],-20}");
                }
                Console.WriteLine();
            }
        }


        //UPDATE operation
        public static void Update(SqlConnection connectionString,string? tb_name,IConfiguration config)
        {
            Console.Write("Enter the Id : " );
            int _id = Convert.ToInt32(Console.ReadLine());

            Console.WriteLine("Which column to be editted ??");
            DataTable schemaTable = connectionString.GetSchema("Columns", new[] { null, null, tb_name });
            

            //Getting the Countvalue from config file
            string? value = config["CountValue"];
            int count = int.Parse(value);
            foreach (DataRow row in schemaTable.Rows)
            {
                string? columnName = row["COLUMN_NAME"].ToString();
                Console.WriteLine(count+") "+columnName);
                count++;
            }

            //Getting the primary key 
            string? primaryKeyColumnName = GetPrimaryKeyColumnName(tb_name,connectionString);

            Console.WriteLine("Enter the column name : ");
            string? column_name = Console.ReadLine();

            Console.Write("Enter the value to be updated : ");
            string? updateValue = Console.ReadLine();


            SqlCommand command = new SqlCommand($"UPDATE {tb_name} SET {column_name} = '{updateValue}' WHERE {primaryKeyColumnName} = {_id};", connectionString);
            command.ExecuteNonQuery();
        }


        //DELETE Operation
        public static void Delete(SqlConnection connectionString,string? tb_name)
        {

            //Getting the Id as the input
            Console.WriteLine("Enter the id to delete : ");
            int _id = Convert.ToInt32(Console.ReadLine());

            SqlCommand command = new SqlCommand($"DELETE FROM {tb_name} WHERE id={_id};", connectionString);
            command.ExecuteNonQuery();
            
        }


        // Function for getting primary key of a table 
        public static string? GetPrimaryKeyColumnName(string tableName,  SqlConnection connectionString)
        {
           
                string query = $"SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE WHERE OBJECTPROPERTY(OBJECT_ID(CONSTRAINT_SCHEMA + '.' + CONSTRAINT_NAME), 'IsPrimaryKey') = 1 AND TABLE_NAME = '{tableName}'";

                using(SqlCommand command = new SqlCommand(query, connectionString))
                {
                    using(SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            Console.WriteLine("Primary Key   -   "+reader.GetString(0));
                            string primaryKeyColumnName = reader.GetString(0);
                            return primaryKeyColumnName;
                        }
                    }
                }
            return null;
        }
    }  
}  
