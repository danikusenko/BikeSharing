using BikeSharing.ViewModels;
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

        public Client Login(LoginModel model)
        {
            Client client = null;
            using (MySqlConnection conn = GetConnection())
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand("select * from clients where email = (@email) " +
                "and password = (@password)", conn);
                cmd.Parameters.AddWithValue("@email", model.Email);
                cmd.Parameters.AddWithValue("@password", model.Password);
                using (var reader = cmd.ExecuteReader())
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
                            Money = Convert.ToInt32(reader["id_address"] != DBNull.Value ? reader["id_address"] : 0)
                        };
                    }
                }
            }
            return client;
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
                        if (reader["email"].ToString() == model.Email)
                            return null;
                    }
                }
                using (var transaction = conn.BeginTransaction())
                {
                    var insertCommand = conn.CreateCommand();
                    insertCommand.CommandText = "insert ignore into name1 set lastname = (@lastname);" +
                                                "insert ignore into name2 set firstname = (@firstname);" +
                                                "insert ignore into name3 set patronymic = (@patronymic);";
                    insertCommand.CommandText += "insert into clients(role, email, password, id_name1, id_name2, id_name3)" +
                                            "select @user, @email, @password, name1.id, name2.id, name3.id " +
                                            "from name1, name2, name3 " +
                                            "where name1.lastname = (@lastname) and " +
                                            "name2.firstname = (@firstname) and " +
                                            "name3.patronymic = (@patronymic);";
                    insertCommand.Parameters.AddWithValue("@user", "Пользователь");
                    insertCommand.Parameters.AddWithValue("@email", model.Email);
                    insertCommand.Parameters.AddWithValue("@password", model.Password);
                    insertCommand.Parameters.AddWithValue("@lastname", model.Surname);
                    insertCommand.Parameters.AddWithValue("@firstname", model.Name);
                    insertCommand.Parameters.AddWithValue("@patronymic", model.Patronymic);                    
                    insertCommand.CommandText += "select LAST_INSERT_ID();";
                    newId = Convert.ToInt32(insertCommand.ExecuteScalar());
                    transaction.Commit();
                }
                Console.WriteLine("newId" + newId);
                MySqlCommand command = new MySqlCommand("select * from clients where id = (@newId);", conn);
                command.Parameters.AddWithValue("@newId", newId);
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
                            Money = Convert.ToInt32(reader["id_address"] != DBNull.Value ? reader["id_address"] : 0)
                        };
                    }
                }
            }
            return client;
        }

        public void DeleteUser(string id)
        {
            using (MySqlConnection conn = GetConnection())
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    var insertCommand = conn.CreateCommand();
                    insertCommand.CommandText = "delete from clients where id = (@id);";
                    insertCommand.Parameters.AddWithValue("@id", id);
                    insertCommand.ExecuteNonQuery();
                    transaction.Commit();
                }
            }
        }
        public List<Client> GetAllUsers()
        {
            List<Client> clients = new List<Client>();
            using (MySqlConnection conn = GetConnection())
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand("select * from clients", conn);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        clients.Add(new Client()
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
                            Money = Convert.ToInt32(reader["id_address"] != DBNull.Value ? reader["id_address"] : 0)
                        });
                    }
                }
            }
            return clients;
        }

        public Client GetClientByEmail(string email)
        {
            Client client = null;
            using (MySqlConnection conn = GetConnection())
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand("select * from clients where email = (@email);", conn);
                cmd.Parameters.AddWithValue("@email", email);
                using (var reader = cmd.ExecuteReader())
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
                            Money = Convert.ToInt32(reader["id_address"] != DBNull.Value ? reader["id_address"] : 0)
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

        public List<City> GetAllCities()
        {
            List<City> cities = new List<City>();
            using (MySqlConnection conn = GetConnection())
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand("select * from city", conn);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        cities.Add(new City()
                        {
                            Id = Convert.ToInt32(reader["id"]),
                            Name = reader["name"].ToString()
                        });
                    }
                }
            }
            return cities;
        }

        public List<Country> GetAllCountries()
        {
            List<Country> countries = new List<Country>();
            using (MySqlConnection conn = GetConnection())
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand("select * from country", conn);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        countries.Add(new Country()
                        {
                            Id = Convert.ToInt32(reader["id"]),
                            Name = reader["name"].ToString(),
                            ShortName = reader["shortname"].ToString()
                        });
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
                            Id = Convert.ToInt32(reader["id"]),
                            HouseNumber = reader["numberhouse"].ToString(),
                            FlatNumber = reader["flatnumber"].ToString(),
                            Building = Convert.ToInt32(reader["building"]),
                            CountryId = Convert.ToInt32(reader["id_contry"]),
                            CityId = Convert.ToInt32(reader["id_city"]),
                            StreetId = Convert.ToInt32(reader["id_street"]),
                            RegionId = Convert.ToInt32(reader["id_region"]),
                            AreaId = Convert.ToInt32(reader["id_area"]),
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
