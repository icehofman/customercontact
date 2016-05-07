using CCMWebApp.Models;
using Helpers;
using PagedList;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CCMWebApp.Controllers
{
    public class HomeController : Controller
    {        
        const string rubish = "System.Web.Mvc.SelectList";
        private DatabaseEntities db = new DatabaseEntities();

        private string SqlConn()
        {
            string path = HttpContext.Server.MapPath("~/App_Data/Database.mdf");
            return @"Data Source=(LocalDB)\v11.0;AttachDbFilename='" + path + "';Integrated Security=True";

        }

        #region ViewResult INDEX
        // Get/Post
        public ViewResult Index(string sortOrder, string currentFilter, string searchName, string searchStartDate, string searchEndDate, string searchGender, string searchClassification, string searchRegion, string searchCity, string searchSeller, int? page)
        {            
            string userId = string.Empty;
            if (TempData["userId"] != null)
            {
                userId = TempData["userId"].ToString();
                TempData["userId"] = userId;

                searchSeller = userId;
            }

            if (string.IsNullOrEmpty(searchSeller) || searchSeller.Contains(rubish))
            {
                ViewBag.SellerCombo = BuildComboSeller(string.Empty);
                searchSeller = string.Empty;
            }
            else
                ViewBag.SellerCombo = BuildComboSeller(searchSeller);

            if (string.IsNullOrEmpty(searchGender) || searchGender.Contains(rubish))
            {
                ViewBag.GenderCombo = CombosHelper.BuildComboGender(string.Empty);
                searchGender = string.Empty;
            }
            else
                ViewBag.GenderCombo = CombosHelper.BuildComboGender(searchGender);

            if (string.IsNullOrEmpty(searchCity) || searchCity.Contains(rubish))
            {
                ViewBag.CityCombo = BuildComboCity(string.Empty);
                searchCity = string.Empty;
            }
            else
                ViewBag.CityCombo = BuildComboCity(searchCity);

            if (string.IsNullOrEmpty(searchRegion) || searchRegion.Contains(rubish))
            {
                ViewBag.RegionCombo = BuildComboRegion(string.Empty);
                searchRegion = string.Empty;
            }
            else
                ViewBag.RegionCombo = BuildComboRegion(searchRegion);

            if (string.IsNullOrEmpty(searchClassification) || searchClassification.Contains(rubish))
            {
                ViewBag.ClassificationCombo = BuildComboClassification(string.Empty);
                searchClassification = string.Empty;
            }
            else
                ViewBag.ClassificationCombo = BuildComboClassification(searchClassification);

            DateTime dateToCheckStart;
            DateTime dateToCheckEnd;
            DateTime? end = null;
            DateTime? start = null;

            bool convertDateStart = DateTime.TryParse(searchStartDate, out dateToCheckStart);
            bool convertDateEnd = DateTime.TryParse(searchEndDate, out dateToCheckEnd);
            if (convertDateEnd)
                end = dateToCheckEnd;
            
            if (convertDateStart)
                start = dateToCheckStart;

            ClientFilter filter = new ClientFilter()
            {
                Name = searchName == null ? string.Empty : searchName,
                Gender = searchGender,
                StartDate = start,
                EndDate = end,
                GuidRegion = !string.IsNullOrEmpty(searchRegion) ? searchRegion : string.Empty,
                GuidCity = !string.IsNullOrEmpty(searchCity) ? searchCity : string.Empty,
                GuidClassification =  !string.IsNullOrEmpty(searchClassification) ? searchClassification : string.Empty,
                GuidSeller = !string.IsNullOrEmpty(searchSeller) ? searchSeller : string.Empty,
                SortOrder = sortOrder 
            };

            HeaderController(sortOrder);

            if (!string.IsNullOrEmpty(searchName))
                page = 1;
            else
                searchName = currentFilter;

            ViewBag.FilterName = searchName;

            IEnumerable<Client> clients = ResultFromClient(filter);

            int pageSize = 5;
            int pageNumber = (page ?? 1);

            return View(clients.ToPagedList(pageNumber, pageSize));
        }
        #endregion

        #region Check and Load Dropdows
        private void HeaderController(string sortOrder)
        {
            ViewBag.CurrentSort = sortOrder;
            ViewBag.NameSortParm = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewBag.GenderSortParm = sortOrder == "Gender" ? "gender_desc" : "Gender";
            ViewBag.SellerSortParm = sortOrder == "Seller" ? "seller_desc" : "Seller";
            ViewBag.CitySortParm = sortOrder == "City" ? "city_desc" : "City";
            ViewBag.RegionSortParm = sortOrder == "Region" ? "region_desc" : "Region";
            ViewBag.ClassificationSortParm = sortOrder == "Classification" ? "classification_desc" : "Classification";
            ViewBag.LastPurchaseSortParm = sortOrder == "Last Purchase" ? "lastpurchase_desc" : "Last Purchase";
        }
        #endregion

        #region Result from Client List
        private IEnumerable<Client> ResultFromClient(ClientFilter filter)
        {
            ICollection<Client> clients = null;

            using (var cn = new SqlConnection(SqlConn()))
            {
                string sql = string.Concat(@"SELECT c.ClientId, c.Name, c.Phone, c.Gender, c.LastPurchase, ",
                                            "seller.Id, seller.UserName, seller.Discriminator, ",
                                            "region.RegionId, region.RegionName, ",
                                            "city.CityId, city.CityName, ",
                                            "class.ClassificationId, class.ClassificationName ",
                                            "from Client c ",
                                            "inner join AspNetUsers seller on c.SellerId = seller.Id ",
                                            "inner join Region region on c.RegionId = region.RegionId ",
                                            "inner join City city on region.CityId = city.CityId ",
                                            "inner join Classification class on c.ClassificationId = class.ClassificationId ");

                var cmd = new SqlCommand(sql, cn);
                cn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.HasRows)
                {
                    clients = new List<Client>();
                    while (reader.Read())
                    {                      
                        Client client = new Client()
                        {
                            Id = (Guid)reader.GetValue(0),
                            Name = reader.GetValue(1).ToString(),
                            Phone = string.Format("{0: ## ####-##-##}", reader.GetValue(2).ToString()),
                            Gender = reader.GetValue(3).ToString(),
                            LastPurchase = (DateTime)reader.GetValue(4),
                            
                            Seller = new AspNetUsers ()
                            {
                                Id = reader.GetValue(5).ToString(),
                                UserName = reader.GetValue(6).ToString(),
                                Discriminator = reader.GetValue(7).ToString(),
                            },

                            Region = new Region()
                            {
                                Id = (Guid)reader.GetValue(8),
                                Name = reader.GetValue(9).ToString(),
                                City = new City()
                                {
                                    Id = (Guid)reader.GetValue(10),
                                    Name = reader.GetValue(11).ToString(),
                                }
                                
                            },

                            Classification = new Classification()
                            {
                                Id = (Guid)reader.GetValue(12),
                                Name = reader.GetValue(13).ToString(),
                            }
                        };

                        clients.Add(client);
                    }
                                        
                    reader.Dispose();
                    cmd.Dispose();
                }
                else
                {
                    reader.Dispose();
                    cmd.Dispose();
                }
            }

            if (!string.IsNullOrEmpty(filter.Name))
                clients = clients.Where(s => s.Name.ToUpper().Contains(filter.Name.ToUpper())).ToList();

            if (!string.IsNullOrEmpty(filter.Gender))
                clients = clients.Where(s => s.Gender.ToUpper().Equals(filter.Gender.ToUpper())).ToList();

            if (!string.IsNullOrEmpty(filter.GuidRegion))
                clients = clients.Where(s => s.Region.Id.ToString().ToLower().Equals(filter.GuidRegion.ToLower())).ToList();

            if (!string.IsNullOrEmpty(filter.GuidCity))
                clients = clients.Where(s => s.Region.City.Id.ToString().ToLower().Equals(filter.GuidCity.ToLower())).ToList();

            if (!string.IsNullOrEmpty(filter.GuidClassification))
                clients = clients.Where(s => s.Classification.Id.ToString().ToLower().Equals(filter.GuidClassification.ToLower())).ToList();

            if (!string.IsNullOrEmpty(filter.GuidSeller))
                clients = clients.Where(s => s.Seller.Id.ToLower().Equals(filter.GuidSeller.ToLower()) ).ToList();

            if (filter.StartDate.HasValue && !filter.EndDate.HasValue)
                filter.EndDate = System.DateTime.Now.Date;

            else if (!filter.StartDate.HasValue && !filter.EndDate.HasValue) {
                filter.EndDate = System.DateTime.Now.Date;
                filter.StartDate = System.DateTime.MinValue;
                    
            }
            else
                filter.StartDate = System.DateTime.MinValue;

            clients = clients.Where(c => c.LastPurchase >= filter.StartDate.Value && c.LastPurchase <= filter.EndDate.Value).ToList();

            if (string.IsNullOrEmpty(filter.SortOrder))
                return clients.OrderBy(s => s.Name).ToList();

            switch (filter.SortOrder)
            {
                case "name_desc":
                    clients = clients.OrderByDescending(s => s.Name).ToList();
                    break;
                
                case "City":
                    clients = clients.OrderBy(s => s.Region.City.Name).ToList();
                    break;
                case "city_desc":
                    clients = clients.OrderByDescending(s => s.Region.City.Name).ToList();
                    break;

                case "Region":
                    clients = clients.OrderBy(s => s.Region.Name).ToList();
                    break;
                case "region_desc":
                    clients = clients.OrderByDescending(s => s.Region.Name).ToList();
                    break;
                
                case "Gender":
                    clients = clients.OrderBy(s => s.Gender).ToList();
                    break;
                case "gender_desc":
                    clients = clients.OrderByDescending(s => s.Gender).ToList();
                    break;

                case "Classification":
                    clients = clients.OrderBy(s => s.Classification.Name).ToList();
                    break;
                case "classification_desc":
                    clients = clients.OrderByDescending(s => s.Classification.Name).ToList();
                    break;

                case "Last Purchase":
                    clients = clients.OrderBy(s => s.LastPurchase).ToList();
                    break;
                case "lastpurchase_desc":
                    clients = clients.OrderByDescending(s => s.LastPurchase).ToList();
                    break;

                case "Seller":
                    clients = clients.OrderBy(s => s.Seller.UserName).ToList();
                    break;
                case "seller_desc":
                    clients = clients.OrderByDescending(s => s.Seller.UserName).ToList();
                    break;
            }

            return clients;
        }
        #endregion

        #region ComboBox REGION

        public SelectList BuildComboRegion(string region)
        {
            List<Region> regions = new List<Region>();
            using (var cn = new SqlConnection(SqlConn()))
            {
                string sql = @"SELECT * FROM [dbo].[Region]";

                var cmd = new SqlCommand(sql, cn);
                cn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read()) 
                    {
                        Region newRegion = new Region()
                        {
                            Id = (Guid)reader.GetValue(0),
                            Name = reader.GetValue(1).ToString()
                        };
                        regions.Add(newRegion);
                    }
                    
                    reader.Dispose();
                    cmd.Dispose();
                }
                else
                {
                    reader.Dispose();
                    cmd.Dispose();
                }
            }

            SelectList selectedList = new SelectList((from r in regions.ToList()
                                                      select new
                                                      {
                                                          Id = r.Id,
                                                          Name = r.Name
                                                      }), "Id", "Name");
            
            return selectedList;
        }
        #endregion

        #region ComboBox CITY
        public SelectList BuildComboCity(string city)
        {
            List<City> cities = new List<City>();
            using (var cn = new SqlConnection(SqlConn()))
            {
                string sql = @"SELECT * FROM [dbo].[City]";

                var cmd = new SqlCommand(sql, cn);
                cn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        City newCity = new City()
                        {
                            Id = (Guid)reader.GetValue(0),
                            Name = reader.GetValue(1).ToString()
                        };
                        cities.Add(newCity);
                    }

                    reader.Dispose();
                    cmd.Dispose();
                }
                else
                {
                    reader.Dispose();
                    cmd.Dispose();
                }
            }

            SelectList selectedList = new SelectList((from r in cities.ToList()
                                           select new
                                           {
                                               Id = r.Id,
                                               Name = r.Name
                                           }), "Id", "Name");
            return selectedList;
        }
        #endregion

        #region ComboBox CLASSIFICATION
        public SelectList BuildComboClassification(string classification)
        {
            List<Classification> classifications = new List<Classification>();
            using (var cn = new SqlConnection(SqlConn()))
            {
                string sql = @"SELECT * FROM [dbo].[Classification]";

                var cmd = new SqlCommand(sql, cn);
                cn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        Classification newClassification = new Classification()
                        {
                            Id = (Guid)reader.GetValue(0),
                            Name = reader.GetValue(1).ToString()
                        };
                        classifications.Add(newClassification);
                    }

                    reader.Dispose();
                    cmd.Dispose();
                }
                else
                {
                    reader.Dispose();
                    cmd.Dispose();
                }
            }

            SelectList selectedList = new SelectList((from c in classifications.ToList()
                                           select new
                                           {
                                               Id = c.Id,
                                               Name = c.Name
                                           }), "Id", "Name");
            return selectedList;
        }
        #endregion

        #region Combobox Seller or User
        public SelectList BuildComboSeller(string seller)
        {
            SelectList selectedList = new SelectList((from u in db.AspNetUsers.ToList()
                                                      select new
                                                      {
                                                          Id = u.Id,
                                                          UserName = u.UserName
                                                      }), "Id", "UserName");

            return selectedList;
        }
        #endregion

        #region Action Result
        public ActionResult About()
        {
            ViewBag.Message = "Solution to be implemented";
            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "In case you need to contact me";
            return View();
        }
        #endregion
    }
}