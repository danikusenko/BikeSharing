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
                MySqlCommand cmd = new MySqlCommand("select (case " +
                    "when(exists(select * from clients where email = (@email) and password = " +
                    "md5(@password) and status = @not_blocked)) then 1 " +
                    "when(exists(select * from clients where email = (@email) " +
                    "and password = md5(@password) and status = @blocked)) then 2 " +
                    "else 0 end);", conn);
                cmd.Parameters.AddWithValue("@email", model.Email);
                cmd.Parameters.AddWithValue("@password", model.Password);
                cmd.Parameters.AddWithValue("@not_blocked", "Не заблокирован");
                cmd.Parameters.AddWithValue("@blocked", "Заблокирован");
                result = Convert.ToInt32(cmd.ExecuteScalar());
            }
            return result;
        }

        public void Recovery(string id)
        {            
            using (MySqlConnection conn = GetConnection())
            {
                conn.Open();
                string email = "";
                int id_client = 0;                
                MySqlCommand cmd = new MySqlCommand("select email, id_client from clients_backup where id = (@id);", conn);
                cmd.Parameters.AddWithValue("@id", id);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        email = reader["email"].ToString();
                        id_client = Convert.ToInt32(reader["id_client"]);                        
                    }                    
                }
                cmd.CommandText = "select email from clients;";
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if(email == reader["email"].ToString())                        
                            DeleteUser(GetIdByEmail(email));
                    }
                }                
                using (var transaction = conn.BeginTransaction())
                {
                    var insertCommand = conn.CreateCommand();                    
                    insertCommand.CommandText = "insert into clients(id, id_address, " +
                        "id_name1, id_name2, id_name3, money, email, password, phonenumber, " +
                        "role, id_blocking,status) select id_client, id_address, id_name1, " +
                        "id_name2, id_name3, money, email, password, phonenumber, role, " +
                        "id_blocking, status from clients_backup where clients_backup.id = (@id);";
                    insertCommand.CommandText += "insert into blocking(id, permanently, beginningdate, " +
                            "expirationdate, id_client) select id_blocking, permanently, beginningdate, " +
                            "expirationdate, id_client from blocking_backup where id_client in " +
                            "(select id_client from clients_backup where id = (@id) and id_blocking is not null);";
                    insertCommand.CommandText += "delete from clients_backup where id = (@id);";           
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
                    insertCommand.CommandText = "insert into name1(lastname) select @lastname from " +
                                                "(select @lastname) as _surname where not exists" +
                                                "(select lastname from name1 where lastname = (@lastname)); " +
                                                "insert into name2(firstname) select @firstname from " +
                                                "(select @firstname) as _name  where not exists" +
                                                "(select firstname from name2 where firstname = (@firstname)); " +
                                                "insert into name3(patronymic) select @patronymic from " +
                                                "(select @patronymic) as _patronymic  where not exists" +
                                                "(select patronymic from name3 where patronymic = (@patronymic)); " +
                                                "insert into country(name) select @country from (select @country) " +
                                                "as _country where not exists" +
                                                "(select name from country where name = (@country)); " +
                                                "insert into city(name) select(@city) from (select @city) " +
                                                "as _city where not exists" +
                                                "(select name from city where name = (@city)); " +
                                                "insert ignore into address(id_city,id_country)" +
                                                "select _city.id, _country.id  from city as _city, country as _country " +
                                                "where  _city.name = (@city) and " +
                                                "_country.name = (@country); ";
                    insertCommand.CommandText += "insert into clients(role, email, password, phonenumber, status, id_name1, id_name2, id_name3, id_address)" +
                                            "select @user, @email, md5(@password), @phone_number, @status, name1.id, name2.id, " +
                                            "name3.id, address.id " +
                                            "from name1, name2, name3, address " +
                                            "where name1.lastname = (@lastname) and " +
                                            "name2.firstname = (@firstname) and " +
                                            "name3.patronymic = (@patronymic) " +
                                            "and address.id_city in (select city.id from city where name = (@city)) and " +
                                            "address.id_country in (select country.id from country where name = (@country)) limit 1; ";
                    insertCommand.Parameters.AddWithValue("@user", "Пользователь");
                    insertCommand.Parameters.AddWithValue("@status", "Не заблокирован");
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

        public List<Client> GetAllClients(SearchClientViewModel model, string table)
        {
            List<Client> clients = new List<Client>();
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
                     "join city on city.id = address.id_city where email like (lower(@email)) and " +
                     "phonenumber like (@phonenumber) and id_name2 in " +
                    "(select id from name2 where firstname like (lower(@name))) and id_name1 in " +
                    "(select id from name1 where lastname like (lower(@surname))) and id_name3 in " +
                    "(select id from name3 where patronymic like (lower(@patronymic))) and " +
                    "id_address in (select id from address where id_city in (select id from city " +
                    "where name like (lower(@city))) and id_country in (select id from country where " +
                    "name like (lower(@country))));", conn);
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

        public void DeleteUser(string id, string table = "clients")
        {
            using (MySqlConnection conn = GetConnection())
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    var deleteCommand = conn.CreateCommand();
                    deleteCommand.CommandText = "delete from clients where id = (@id);"/* +
                        "delete from blocking where id_client = (@id);"*/;
                    deleteCommand.Parameters.AddWithValue("@id", id);
                    deleteCommand.ExecuteNonQuery();
                    transaction.Commit();
                }
            }
        }

        public void BlockUser(BlockingViewModel model)
        {
            int newId;
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
                    insertCommand.CommandText = "insert into blocking(id_client, permanently," +
                        "beginningdate, expirationdate) values (@id_client, @permanently, " +
                        "@beginningdate, @expirationdate); ";
                    insertCommand.Parameters.AddWithValue("@id_client", model.Id);
                    insertCommand.Parameters.AddWithValue("@permanently", model.Permanently);
                    insertCommand.Parameters.AddWithValue("@beginningdate", model.BeginningDate);
                    insertCommand.Parameters.AddWithValue("@expirationdate", model.ExpirationDate);
                    insertCommand.CommandText += "select last_insert_id();";
                    newId = Convert.ToInt32(insertCommand.ExecuteScalar());
                    insertCommand.CommandText = "update clients set id_blocking = (@newId), status = (@block) " +
                        "where id = (@id_client);";
                    insertCommand.Parameters.AddWithValue("@newId", newId);
                    insertCommand.Parameters.AddWithValue("@block", "Заблокирован");
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
                    insertCommand.CommandText = "delete from blocking where id_client = (@id);" +
                        "update clients set id_blocking = NULL, status = (@unblock) where id = (@id);";
                    insertCommand.Parameters.AddWithValue("@id", id);
                    insertCommand.Parameters.AddWithValue("@unblock", "Не заблокирован");
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
                    updateCommand.CommandText = "update clients set role = (@role) " +
                        "where id = (@id);";
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
