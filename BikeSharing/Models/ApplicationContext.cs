using BikeSharing.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BikeSharing.Models
{
    public class ApplicationContext
    {
        public string ConnectionString { get; set; }

        public ApplicationContext(string connectionString)
        {
            this.ConnectionString = connectionString;
        }

        private MySqlConnection GetConnection()
        {
            return new MySqlConnection(ConnectionString);
        }

        public int Login(LoginModel model)
        {
            int result = 0;
            using (MySqlConnection conn = GetConnection())
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand("call login_client(@email, @password);", conn);
                cmd.Parameters.AddWithValue("@email", model.Email);
                cmd.Parameters.AddWithValue("@password", model.Password);                
                result = Convert.ToInt32(cmd.ExecuteScalar());
            }
            return result;
        }

        public void Recovery(string id)
        {            
            using (MySqlConnection conn = GetConnection())
            {
                conn.Open();                
                using (var transaction = conn.BeginTransaction())
                {
                    var insertCommand = conn.CreateCommand();                   
                    insertCommand.CommandText = "call recovery_client(@id);";
                    insertCommand.Parameters.AddWithValue("@id", id);
                    insertCommand.ExecuteNonQuery();
                    transaction.Commit();
                }
            }
        }

        public Client Register(RegisterModel model)
        {
            Client client = null;
            int newId;
            using (MySqlConnection conn = GetConnection())
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand("select email from clients", conn);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (reader["email"].ToString() == model.Email) {                            
                                return null;
                        }
                    }
                }
                using (var transaction = conn.BeginTransaction())
                {
                    var insertCommand = conn.CreateCommand();                    
                    insertCommand.CommandText = "call register_client(@lastname, @firstname, @patronymic," +
                        "@country, @city, @email, @password, @phone_number);";                    
                    insertCommand.Parameters.AddWithValue("@email", model.Email);
                    insertCommand.Parameters.AddWithValue("@password", model.Password);
                    insertCommand.Parameters.AddWithValue("@lastname", model.Surname);
                    insertCommand.Parameters.AddWithValue("@firstname", model.Name);
                    insertCommand.Parameters.AddWithValue("@patronymic", model.Patronymic);
                    insertCommand.Parameters.AddWithValue("@country", model.Country);
                    insertCommand.Parameters.AddWithValue("@city", model.City);
                    insertCommand.Parameters.AddWithValue("@phone_number", model.PhoneNumber);
                    insertCommand.CommandText += "select LAST_INSERT_ID();";
                    newId = Convert.ToInt32(insertCommand.ExecuteScalar());
                    transaction.Commit();
                }
                MySqlCommand command = new MySqlCommand("select * from clients where id = (@newId);", conn);
                command.Parameters.AddWithValue("@newId", newId.ToString());
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        client = new Client()
                        {
                            Id = Convert.ToInt32(reader["id"]),
                            FirstNameId = Convert.ToInt32(reader["id_name2"]),
                            LastNameId = Convert.ToInt32(reader["id_name1"]),
                            PatronymicId = Convert.ToInt32(reader["id_name3"] != DBNull.Value ? reader["id_name3"] : null),
                            PhoneNumber = reader["phonenumber"].ToString(),
                            AddressId = Convert.ToInt32(reader["id_address"] != DBNull.Value ? reader["id_address"] : null),
                            PassportId = Convert.ToInt32(reader["id_passport"] != DBNull.Value ? reader["id_passport"] : null),
                            Email = reader["email"].ToString(),
                            Role = reader["role"].ToString(),
                            Money = Convert.ToInt32(reader["id_address"] != DBNull.Value ? reader["id_address"] : 0),
                            BlockingId = null
                        };
                    }
                }
            }
            return client;
        }

        public List<Client> GetAllClients(SearchClientViewModel model, string table = "clients")
        {
            List<Client> clients = new List<Client>();            
            using (MySqlConnection conn = GetConnection())
            {
                conn.Open();                
                string sqlQuery = "call search_clients(@email, @phonenumber, @name, @surname,  " +
                    "@patronymic, @country, @city);";
                if (table == "clients_backup")
                    sqlQuery = "call search_copies(@email, @phonenumber, @name, @surname,  @patronymic, " +
                        "@country, @city);";
                MySqlCommand cmd = new MySqlCommand(sqlQuery, conn);                
                cmd.Parameters.AddWithValue("@email", "%" + model.EmailSearch.ToLower() + "%");
                cmd.Parameters.AddWithValue("@city", "%" + model.CitySearch.ToLower() + "%");
                cmd.Parameters.AddWithValue("@country", "%" + model.CountrySearch.ToLower() + "%");
                cmd.Parameters.AddWithValue("@phonenumber", "%" + model.PhoneSearch + "%");
                cmd.Parameters.AddWithValue("@name", "%" + model.NameSearch.ToLower() + "%");
                cmd.Parameters.AddWithValue("@surname", "%" + model.SurnameSearch.ToLower() + "%");
                cmd.Parameters.AddWithValue("@patronymic", "%" + model.PatronymicSearch.ToLower() + "%");
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        clients.Add(new Client()
                        {
                            Id = Convert.ToInt32(reader["id"]),
                            FirstName = new Name2
                            {
                                FirstName = reader["name"].ToString()
                            },
                            LastName = new Name1
                            {
                                LastName = reader["surname"].ToString()
                            },
                            Patronymic = new Name3
                            {
                                Patronymic = reader["patronymic"].ToString()
                            },
                            Address = new Address
                            {
                                City = new City
                                {
                                    Name = reader["city"].ToString()
                                },
                                Country = new Country
                                {
                                    Name = reader["country"].ToString()
                                }
                            },
                            Status = reader["status"].ToString(),
                            PhoneNumber = reader["phonenumber"].ToString(),
                            Role = reader["role"].ToString(),
                            Email = reader["email"].ToString(),
                            BlockingId = Convert.ToInt32(reader["id_blocking"] != DBNull.Value ? reader["id_blocking"] : null),
                            Deleted = false
                        });
                    }
                }
            }
            return clients;
        }

        public void DeleteUser(string id)
        {
            using (MySqlConnection conn = GetConnection())
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    var deleteCommand = conn.CreateCommand();
                    deleteCommand.CommandText = "call delete_client(@id);";                    
                    deleteCommand.Parameters.AddWithValue("@id", id);
                    deleteCommand.ExecuteNonQuery();
                    transaction.Commit();
                }
            }
        }

        public void BlockUser(BlockingViewModel model)
        {            
            using (MySqlConnection conn = GetConnection())
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand("select id_client from blocking", conn);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (Convert.ToInt32(reader["id_client"]) == model.Id)
                            return;
                    }
                }
                using (var transaction = conn.BeginTransaction())
                {
                    var insertCommand = conn.CreateCommand();
                    insertCommand.CommandText = "call block_client(@id_client, @permanently, " +
                        "@beginningdate, @expirationdate)";
                    insertCommand.Parameters.AddWithValue("@id_client", model.Id);
                    insertCommand.Parameters.AddWithValue("@permanently", model.Permanently);
                    insertCommand.Parameters.AddWithValue("@beginningdate", model.BeginningDate);
                    insertCommand.Parameters.AddWithValue("@expirationdate", model.ExpirationDate);                    
                    insertCommand.ExecuteScalar();
                    transaction.Commit();
                }
            }
        }

        public void UnblockUser(string id)
        {
            using (MySqlConnection conn = GetConnection())
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    var insertCommand = conn.CreateCommand();
                    insertCommand.CommandText = "call unblock_client(@id);";
                    insertCommand.Parameters.AddWithValue("@id", id);                    
                    insertCommand.ExecuteScalar();
                    transaction.Commit();
                }
            }
        }

        public void ChangeRole(string id, string role)
        {
            using (MySqlConnection conn = GetConnection())
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    var updateCommand = conn.CreateCommand();
                    updateCommand.CommandText = "call change_client_role(@id, @role)";
                    updateCommand.Parameters.AddWithValue("@id", id);
                    updateCommand.Parameters.AddWithValue("@role", role);                    
                    updateCommand.ExecuteScalar();
                    transaction.Commit();
                }
            }
        }

        public string GetIdByEmail(string email)
        {
            int id;
            using (MySqlConnection conn = GetConnection())
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand("select id from clients where email = (@email);", conn);
                cmd.Parameters.AddWithValue("@email", email);
                id = Convert.ToInt32(cmd.ExecuteScalar());
            }
            return id.ToString();
        }

        public Client GetClientByIdForView(string id, string table)
        {
            Client client = null;
            string id_clients = "";
            if (table == "clients_backup")
                id_clients = " id_client,";
            using (MySqlConnection conn = GetConnection())
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand("select " + table + ".id," + id_clients +
                     "id_blocking, money, email, phonenumber, role, status, " +
                     "name1.lastname as surname, name2.firstname as name, " +
                     "name3.patronymic as patronymic, country.name as country, city.name as city " +
                     "from " + table + " join name1 on name1.id = " + table + ".id_name1 " +
                     "join name2 on name2.id = " + table + ".id_name2 " +
                     "join name3 on name3.id = " + table + ".id_name3 " +
                     "join address on address.id = " + table + ".id_address " +
                     "join country on country.id = address.id_country " +
                     "join city on city.id = address.id_city where " + table + ".id = @id", conn);
                cmd.Parameters.AddWithValue("@id", id);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        client = new Client()
                        {
                            Id = table == "clients_backup" ? Convert.ToInt32(reader["id_client"]) : Convert.ToInt32(reader["id"]),
                            FirstName = new Name2
                            {
                                FirstName = reader["name"].ToString()
                            },
                            LastName = new Name1
                            {
                                LastName = reader["surname"].ToString()
                            },
                            Patronymic = new Name3
                            {
                                Patronymic = reader["patronymic"].ToString()
                            },
                            Address = new Address
                            {
                                City = new City
                                {
                                    Name = reader["city"].ToString()
                                },
                                Country = new Country
                                {
                                    Name = reader["country"].ToString()
                                }
                            },
                            Status = reader["status"].ToString(),
                            PhoneNumber = reader["phonenumber"].ToString(),
                            Role = reader["role"].ToString(),
                            Email = reader["email"].ToString(),
                            BlockingId = Convert.ToInt32(reader["id_blocking"] != DBNull.Value ? reader["id_blocking"] : null),
                            Deleted = false                            
                        };
                    }
                }
            }
            return client;
        }

        public Client GetClientById(string id, string table)
        {
            Client client = null;
            string _id = "id";
            if (table == "clients_backup")
                _id = "id_client";
            using (MySqlConnection conn = GetConnection())
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand("select * from " + table + " where " + _id + " = (@id);", conn);
                cmd.Parameters.AddWithValue("@id", id);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        client = new Client()
                        {
                            Id = Convert.ToInt32(reader[_id]),
                            FirstNameId = Convert.ToInt32(reader["id_name2"]),
                            LastNameId = Convert.ToInt32(reader["id_name1"]),
                            PatronymicId = Convert.ToInt32(reader["id_name3"] != DBNull.Value ? reader["id_name3"] : null),
                            PhoneNumber = reader["phonenumber"].ToString(),
                            AddressId = Convert.ToInt32(reader["id_address"] != DBNull.Value ? reader["id_address"] : null),
                            PassportId = Convert.ToInt32(reader["id_passport"] != DBNull.Value ? reader["id_passport"] : null),
                            Email = reader["email"].ToString(),
                            Role = reader["role"].ToString(),
                            Money = Convert.ToInt32(reader["id_address"] != DBNull.Value ? reader["id_address"] : 0),
                            BlockingId = Convert.ToInt32(reader["id_blocking"] != DBNull.Value ? reader["id_blocking"] : null)
                        };
                    }
                }                
            }
            return client;
        }

        public List<Name2> GetAllNames()
        {
            List<Name2> names = new List<Name2>();
            using (MySqlConnection conn = GetConnection())
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand("select * from name2", conn);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        names.Add(new Name2()
                        {
                            Id = Convert.ToInt32(reader["id"]),
                            FirstName = reader["firstname"].ToString()
                        });
                    }
                }
            }
            return names;
        }

        public List<Name1> GetAllSurnames()
        {
            List<Name1> surnames = new List<Name1>();
            using (MySqlConnection conn = GetConnection())
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand("select * from name1", conn);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        surnames.Add(new Name1()
                        {
                            Id = Convert.ToInt32(reader["id"]),
                            LastName = reader["lastname"].ToString()
                        });
                    }
                }
            }
            return surnames;
        }

        public List<Name3> GetAllPatronymics()
        {
            List<Name3> patronymics = new List<Name3>();
            using (MySqlConnection conn = GetConnection())
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand("select * from name3", conn);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        patronymics.Add(new Name3()
                        {
                            Id = Convert.ToInt32(reader["id"]),
                            Patronymic = reader["patronymic"].ToString()
                        });
                    }
                }
            }
            return patronymics;
        }

        public List<Region> GetAllRegions()
        {
            List<Region> regions = new List<Region>();
            using (MySqlConnection conn = GetConnection())
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand("select * from region", conn);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        regions.Add(new Region()
                        {
                            Id = Convert.ToInt32(reader["id"]),
                            Name = reader["name"].ToString()
                        });
                    }
                }
            }
            return regions;
        }

        public List<Area> GetAllAreas()
        {
            List<Area> areas = new List<Area>();
            using (MySqlConnection conn = GetConnection())
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand("select * from area", conn);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        areas.Add(new Area()
                        {
                            Id = Convert.ToInt32(reader["id"]),
                            Name = reader["name"].ToString()
                        });
                    }
                }
            }
            return areas;
        }

        public List<string> GetAllCities()
        {
            List<string> cities = new List<string>();
            using (MySqlConnection conn = GetConnection())
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand("select name from city", conn);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        cities.Add(new string(reader["name"].ToString()));
                    }
                }
            }
            return cities;
        }

        public List<string> GetAllCountries()
        {
            List<string> countries = new List<string>();
            using (MySqlConnection conn = GetConnection())
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand("select name from country", conn);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        countries.Add(new string(reader["name"].ToString()));
                    }
                }
            }
            return countries;
        }

        public List<Street> GetAllStreets()
        {
            List<Street> streets = new List<Street>();
            using (MySqlConnection conn = GetConnection())
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand("select * from street", conn);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        streets.Add(new Street()
                        {
                            Id = Convert.ToInt32(reader["id"]),
                            Name = reader["name"].ToString()
                        });
                    }
                }
            }
            return streets;
        }

        public List<Address> GetAllAddresses()
        {
            List<Address> addresses = new List<Address>();
            using (MySqlConnection conn = GetConnection())
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand("select * from address", conn);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        addresses.Add(new Address()
                        {
                            Id = Convert.ToInt32(reader["id"] != DBNull.Value ? reader["id"] : null),
                            HouseNumber = reader["numberhouse"].ToString(),
                            FlatNumber = reader["numberflat"].ToString(),
                            Building = Convert.ToInt32(reader["building"] != DBNull.Value ? reader["building"] : null),
                            CountryId = Convert.ToInt32(reader["id_country"] != DBNull.Value ? reader["id_country"] : null),
                            CityId = Convert.ToInt32(reader["id_city"] != DBNull.Value ? reader["id_city"] : null),
                            StreetId = Convert.ToInt32(reader["id_street"] != DBNull.Value ? reader["id_street"] : null),
                            RegionId = Convert.ToInt32(reader["id_region"] != DBNull.Value ? reader["id_region"] : null),
                            AreaId = Convert.ToInt32(reader["id_area"] != DBNull.Value ? reader["id_area"] : null),
                            StreetType = reader["typestreet"].ToString(),
                            CityType = reader["typecity"].ToString()
                        });
                    }
                }
            }
            return addresses;
        }

        public List<IssuingPassport> GetAllIssuingPassports()
        {
            List<IssuingPassport> issuingPassports = new List<IssuingPassport>();
            using (MySqlConnection conn = GetConnection())
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand("select * from issuing_passport", conn);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        issuingPassports.Add(new IssuingPassport()
                        {
                            Id = Convert.ToInt32(reader["id"]),
                            Name = reader["name"].ToString()
                        });
                    }
                }
            }
            return issuingPassports;
        }

        public List<Passport> GetAllPassports()
        {
            List<Passport> passports = new List<Passport>();
            using (MySqlConnection conn = GetConnection())
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand("select * from passport", conn);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        passports.Add(new Passport()
                        {
                            Id = Convert.ToInt32(reader["id"]),
                            Series = reader["series"].ToString(),
                            Number = Convert.ToInt32(reader["number"]),
                            DateIssue = Convert.ToDateTime(reader["dateissue"]),
                            DateEnd = Convert.ToDateTime(reader["dateend"]),
                            CountryId = Convert.ToInt32(reader["id_country"]),
                            Identification = reader["identification"].ToString(),
                            IssuingPassportId = Convert.ToInt32(reader["id_issuing_passport"])
                        });
                    }
                }
            }
            return passports;
        }
    }
}
