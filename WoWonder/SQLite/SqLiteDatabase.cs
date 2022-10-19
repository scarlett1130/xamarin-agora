using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using AndroidX.Core.Content;
using Java.Lang;
using Newtonsoft.Json;
using SQLite;
using WoWonder.Helpers.Controller;
using WoWonder.Helpers.Model;
using WoWonder.Helpers.Utils;
using WoWonder.StickersView;
using WoWonderClient;
using WoWonderClient.Classes.Global;
using WoWonderClient.Classes.Message;
using WoWonderClient.Classes.Movies;
using WoWonderClient.Classes.Posts;
using WoWonderClient.Classes.Product;
using WoWonderClient.Classes.Story;
using Environment = System.Environment;
using Exception = System.Exception;

namespace WoWonder.SQLite
{
    public class SqLiteDatabase 
    {
        //############# DON'T MODIFY HERE #############
        private static readonly string Folder = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
        public static readonly string PathCombine = Path.Combine(Folder, AppSettings.DatabaseName + "_.db");
        private static readonly List<string> IdMesgList = new List<string>();

        //Open Connection in Database
        //*********************************************************

        #region Connection

        private SQLiteConnection OpenConnection()
        {
            try
            { 
                var connection = new SQLiteConnection(new SQLiteConnectionString(PathCombine, SQLiteOpenFlags.Create | SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.FullMutex, true));
                return connection;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return null!;
            }
        }

        public void CheckTablesStatus()
        {
            try
            {
                using var connection = OpenConnection();
                switch (connection)
                {
                    case null:
                        return;
                }

                connection.CreateTable<DataTables.LoginTb>();
                connection.CreateTable<DataTables.SettingsTb>();
                connection.CreateTable<DataTables.MyContactsTb>(); 
                connection.CreateTable<DataTables.MyProfileTb>();
                connection.CreateTable<DataTables.SearchFilterTb>();
                connection.CreateTable<DataTables.NearByFilterTb>();
                connection.CreateTable<DataTables.WatchOfflineVideosTb>();
                connection.CreateTable<DataTables.GiftsTb>();
                connection.CreateTable<DataTables.PostsTb>();
                connection.CreateTable<DataTables.StoryTb>();
                connection.CreateTable<DataTables.StickersTb>();
                 
                Insert_To_StickersTb();
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    CheckTablesStatus();
                else
                    Methods.DisplayReportResultTrack(e);
            }
        }
          
        //Delete table 
        public void DropAll()
        {
            try
            {
                using var connection = OpenConnection();
                switch (connection)
                {
                    case null:
                        return;
                }
                connection.DropTable<DataTables.LoginTb>();
                connection.DropTable<DataTables.MyContactsTb>(); 
                connection.DropTable<DataTables.MyProfileTb>();
                connection.DropTable<DataTables.SearchFilterTb>();
                connection.DropTable<DataTables.NearByFilterTb>();
                connection.DropTable<DataTables.WatchOfflineVideosTb>();
                connection.DropTable<DataTables.SettingsTb>();
                connection.DropTable<DataTables.GiftsTb>();
                connection.DropTable<DataTables.PostsTb>();
                connection.DropTable<DataTables.StoryTb>();
                connection.DropTable<DataTables.StickersTb>();
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    DropAll();
                else
                    Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        //########################## End SQLite_Entity ##########################

        //Start SQL_Commander >>  General 
        //*********************************************************

        #region General

        public void InsertRow(object row)
        {
            try
            {
                using var connection = OpenConnection();
                switch (connection)
                {
                    case null:
                        return;
                    default:
                        connection.Insert(row);
                        break;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void UpdateRow(object row)
        {
            try
            {
                using var connection = OpenConnection();
                switch (connection)
                {
                    case null:
                        return;
                    default:
                        connection.Update(row);
                        break;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void DeleteRow(object row)
        {
            try
            {
                using var connection = OpenConnection();
                switch (connection)
                {
                    case null:
                        return;
                    default:
                        connection.Delete(row);
                        break;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void InsertListOfRows(List<object> row)
        {
            try
            {
                using var connection = OpenConnection();
                switch (connection)
                {
                    case null:
                        return;
                    default:
                        connection.InsertAll(row);
                        break;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        //Start SQL_Commander >>  Custom 
        //*********************************************************

        #region Login

        //Insert Or Update data Login
        public void InsertOrUpdateLogin_Credentials(DataTables.LoginTb db)
        {
            try
            {
                using var connection = OpenConnection();
                switch (connection)
                {
                    case null:
                        return;
                }
                var dataUser = connection.Table<DataTables.LoginTb>().FirstOrDefault();
                if (dataUser != null)
                {
                    dataUser.UserId = UserDetails.UserId;
                    dataUser.AccessToken = UserDetails.AccessToken;
                    dataUser.Cookie = UserDetails.Cookie;
                    dataUser.Username = UserDetails.Username;
                    dataUser.Password = UserDetails.Password;
                    dataUser.Status = UserDetails.Status;
                    dataUser.Lang = AppSettings.Lang;
                    dataUser.DeviceId = UserDetails.DeviceId;
                    dataUser.Email = UserDetails.Email;

                    connection.Update(dataUser);
                }
                else
                {
                    connection.Insert(db);
                } 
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    InsertOrUpdateLogin_Credentials(db);
                else
                    Methods.DisplayReportResultTrack(e); 
            }
        }

        //Get data Login
        public DataTables.LoginTb Get_data_Login_Credentials()
        {
            try
            {
                using var connection = OpenConnection();
                switch (connection)
                {
                    case null:
                        return null!;
                }
                var dataUser = connection.Table<DataTables.LoginTb>().FirstOrDefault();
                if (dataUser != null)
                {
                    UserDetails.Username = dataUser.Username;
                    UserDetails.FullName = dataUser.Username;
                    UserDetails.Password = dataUser.Password;
                    UserDetails.AccessToken = dataUser.AccessToken;
                    UserDetails.UserId = dataUser.UserId;
                    UserDetails.Status = dataUser.Status;
                    UserDetails.Cookie = dataUser.Cookie;
                    UserDetails.Email = dataUser.Email; 
                    AppSettings.Lang = dataUser.Lang;
                    UserDetails.DeviceId = dataUser.DeviceId;

                    Current.AccessToken = dataUser.AccessToken;
                    ListUtils.DataUserLoginList.Add(dataUser);

                    return dataUser;
                }
                else
                {
                    return null!;
                }
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    return Get_data_Login_Credentials();
                else
                {
                    Methods.DisplayReportResultTrack(e);
                    return null!;
                } 
            }
        }

        #endregion

        #region Settings

        public void InsertOrUpdateSettings(GetSiteSettingsObject.ConfigObject settingsData)
        {
            try
            {
                using var connection = OpenConnection();
                switch (connection)
                {
                    case null:
                        return;
                }  
                if (settingsData != null)
                {
                    var select = connection.Table<DataTables.SettingsTb>().FirstOrDefault();
                    switch (select)
                    {
                        case null:
                        {
                            var db = ClassMapper.Mapper?.Map<DataTables.SettingsTb>(settingsData);

                            if (db != null)
                            {
                                db.CurrencyArray = JsonConvert.SerializeObject(settingsData.CurrencyArray.CurrencyList);
                                db.CurrencySymbolArray = JsonConvert.SerializeObject(settingsData.CurrencySymbolArray.CurrencyList);
                                db.PageCategories = JsonConvert.SerializeObject(settingsData.PageCategories);
                                db.GroupCategories = JsonConvert.SerializeObject(settingsData.GroupCategories);
                                db.BlogCategories = JsonConvert.SerializeObject(settingsData.BlogCategories);
                                db.ProductsCategories = JsonConvert.SerializeObject(settingsData.ProductsCategories);
                                db.JobCategories = JsonConvert.SerializeObject(settingsData.JobCategories);
                                db.Genders = JsonConvert.SerializeObject(settingsData.Genders);
                                db.Family = JsonConvert.SerializeObject(settingsData.Family);
                                db.MovieCategory = JsonConvert.SerializeObject(settingsData.MovieCategory); 
                                db.PostColors = JsonConvert.SerializeObject(settingsData.PostColors?.PostColorsList);
                                db.Fields = JsonConvert.SerializeObject(settingsData.Fields);
                                db.PostReactionsTypes = JsonConvert.SerializeObject(settingsData.PostReactionsTypes);
                                db.PageSubCategories = JsonConvert.SerializeObject(settingsData.PageSubCategories?.SubCategoriesList);
                                db.GroupSubCategories = JsonConvert.SerializeObject(settingsData.GroupSubCategories?.SubCategoriesList);
                                db.ProductsSubCategories = JsonConvert.SerializeObject(settingsData.ProductsSubCategories?.SubCategoriesList);
                                db.PageCustomFields = JsonConvert.SerializeObject(settingsData.PageCustomFields);
                                db.GroupCustomFields = JsonConvert.SerializeObject(settingsData.GroupCustomFields);
                                db.ProductCustomFields = JsonConvert.SerializeObject(settingsData.ProductCustomFields);
                                db.ProPackages = JsonConvert.SerializeObject(settingsData.ProPackages);
                                db.ProPackagesTypes = JsonConvert.SerializeObject(settingsData.ProPackagesTypes);

                                connection.Insert(db);
                            }

                            break;
                        }
                        default:
                        {
                            select = ClassMapper.Mapper?.Map<DataTables.SettingsTb>(settingsData); 
                            if (select != null)
                            {
                                select.CurrencyArray = JsonConvert.SerializeObject(settingsData.CurrencyArray.CurrencyList);
                                select.CurrencySymbolArray = JsonConvert.SerializeObject(settingsData.CurrencySymbolArray.CurrencyList);
                                select.PageCategories = JsonConvert.SerializeObject(settingsData.PageCategories);
                                select.GroupCategories = JsonConvert.SerializeObject(settingsData.GroupCategories);
                                select.BlogCategories = JsonConvert.SerializeObject(settingsData.BlogCategories);
                                select.ProductsCategories = JsonConvert.SerializeObject(settingsData.ProductsCategories);
                                select.JobCategories = JsonConvert.SerializeObject(settingsData.JobCategories);
                                select.Genders = JsonConvert.SerializeObject(settingsData.Genders);
                                select.Family = JsonConvert.SerializeObject(settingsData.Family);
                                select.MovieCategory = JsonConvert.SerializeObject(settingsData.MovieCategory);
                                select.PostColors = JsonConvert.SerializeObject(settingsData.PostColors?.PostColorsList);
                                select.Fields = JsonConvert.SerializeObject(settingsData.Fields);
                                select.PostReactionsTypes = JsonConvert.SerializeObject(settingsData.PostReactionsTypes);
                                select.PageSubCategories = JsonConvert.SerializeObject(settingsData.PageSubCategories?.SubCategoriesList);
                                select.GroupSubCategories = JsonConvert.SerializeObject(settingsData.GroupSubCategories?.SubCategoriesList);
                                select.ProductsSubCategories = JsonConvert.SerializeObject(settingsData.ProductsSubCategories?.SubCategoriesList);
                                select.PageCustomFields = JsonConvert.SerializeObject(settingsData.PageCustomFields);
                                select.GroupCustomFields = JsonConvert.SerializeObject(settingsData.GroupCustomFields);
                                select.ProductCustomFields = JsonConvert.SerializeObject(settingsData.ProductCustomFields);
                                select.ProPackages = JsonConvert.SerializeObject(settingsData.ProPackages);
                                select.ProPackagesTypes = JsonConvert.SerializeObject(settingsData.ProPackagesTypes);

                                connection.Update(select);
                            }

                            break;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    InsertOrUpdateSettings(settingsData);
                else
                    Methods.DisplayReportResultTrack(e); 
            }
        }

        //Get Settings
        public GetSiteSettingsObject.ConfigObject GetSettings()
        {
            try
            {
                using var connection = OpenConnection();
                switch (connection)
                {
                    case null:
                        return null!;
                }
                var select = connection.Table<DataTables.SettingsTb>().FirstOrDefault();
                if (select != null)
                {
                    var db = ClassMapper.Mapper?.Map<GetSiteSettingsObject.ConfigObject>(select);
                    if (db != null)
                    {
                        GetSiteSettingsObject.ConfigObject asd = db;
                        asd.CurrencyArray = new GetSiteSettingsObject.CurrencyArray();
                        asd.CurrencySymbolArray = new GetSiteSettingsObject.CurrencySymbol();
                        asd.PageCategories = new Dictionary<string, string>();
                        asd.GroupCategories = new Dictionary<string, string>();
                        asd.BlogCategories = new Dictionary<string, string>();
                        asd.ProductsCategories = new Dictionary<string, string>();
                        asd.JobCategories = new Dictionary<string, string>();
                        asd.Genders = new Dictionary<string, string>();
                        asd.Family = new Dictionary<string, string>();
                        asd.MovieCategory = new Dictionary<string, string>();
                        asd.PostColors = new Dictionary<string, PostColorsObject>();
                        asd.Fields = new List<Field>();
                        asd.PostReactionsTypes = new Dictionary<string, PostReactionsType>();
                        asd.PageSubCategories = new GetSiteSettingsObject.SubCategoriesUnion
                        {
                            SubCategoriesList = new Dictionary<string, List<SubCategories>>()
                        };
                        asd.GroupSubCategories = new GetSiteSettingsObject.SubCategoriesUnion
                        {
                            SubCategoriesList = new Dictionary<string, List<SubCategories>>()
                        };
                        asd.ProductsSubCategories = new GetSiteSettingsObject.SubCategoriesUnion
                        {
                            SubCategoriesList = new Dictionary<string, List<SubCategories>>()
                        };
                        asd.PageCustomFields = new List<CustomField>();
                        asd.GroupCustomFields = new List<CustomField>();
                        asd.ProductCustomFields = new List<CustomField>();

                        asd.ProPackages = new Dictionary<string, DataProPackages>();
                        asd.ProPackagesTypes = new Dictionary<string, string>();

                        asd.CurrencyArray = string.IsNullOrEmpty(select.CurrencyArray) switch
                        {
                            false => new GetSiteSettingsObject.CurrencyArray
                            {
                                CurrencyList = JsonConvert.DeserializeObject<List<string>>(select.CurrencyArray)
                            },
                            _ => asd.CurrencyArray
                        };

                        asd.CurrencySymbolArray = string.IsNullOrEmpty(select.CurrencySymbolArray) switch
                        {
                            false => new GetSiteSettingsObject.CurrencySymbol
                            {
                                CurrencyList = JsonConvert.DeserializeObject<Dictionary<string, string>>(select.CurrencySymbolArray),
                            },
                            _ => asd.CurrencySymbolArray
                        };

                        asd.PageCategories = string.IsNullOrEmpty(select.PageCategories) switch
                        {
                            false => JsonConvert.DeserializeObject<Dictionary<string, string>>(select.PageCategories),
                            _ => asd.PageCategories
                        };

                        asd.GroupCategories = string.IsNullOrEmpty(select.GroupCategories) switch
                        {
                            false => JsonConvert.DeserializeObject<Dictionary<string, string>>(select.GroupCategories),
                            _ => asd.GroupCategories
                        };

                        asd.BlogCategories = string.IsNullOrEmpty(select.BlogCategories) switch
                        {
                            false => JsonConvert.DeserializeObject<Dictionary<string, string>>(select.BlogCategories),
                            _ => asd.BlogCategories
                        };

                        asd.ProductsCategories = string.IsNullOrEmpty(select.ProductsCategories) switch
                        {
                            false => JsonConvert.DeserializeObject<Dictionary<string, string>>(
                                select.ProductsCategories),
                            _ => asd.ProductsCategories
                        };

                        asd.JobCategories = string.IsNullOrEmpty(select.JobCategories) switch
                        {
                            false => JsonConvert.DeserializeObject<Dictionary<string, string>>(select.JobCategories),
                            _ => asd.JobCategories
                        };

                        asd.Genders = string.IsNullOrEmpty(select.Genders) switch
                        {
                            false => JsonConvert.DeserializeObject<Dictionary<string, string>>(select.Genders),
                            _ => asd.Genders
                        };

                        asd.Family = string.IsNullOrEmpty(select.Family) switch
                        {
                            false => JsonConvert.DeserializeObject<Dictionary<string, string>>(select.Family),
                            _ => asd.Family
                        };

                        asd.MovieCategory = string.IsNullOrEmpty(select.MovieCategory) switch
                        {
                            false => JsonConvert.DeserializeObject<Dictionary<string, string>>(select.MovieCategory),
                            _ => asd.MovieCategory
                        };

                        asd.PostColors = string.IsNullOrEmpty(select.PostColors) switch
                        {
                            false => new GetSiteSettingsObject.PostColorUnion
                            {
                                PostColorsList =
                                    JsonConvert.DeserializeObject<Dictionary<string, PostColorsObject>>(
                                        select.PostColors)
                            },
                            _ => asd.PostColors
                        };

                        asd.PostReactionsTypes = string.IsNullOrEmpty(select.PostReactionsTypes) switch
                        {
                            false => JsonConvert.DeserializeObject<Dictionary<string, PostReactionsType>>(
                                select.PostReactionsTypes),
                            _ => asd.PostReactionsTypes
                        };

                        asd.Fields = string.IsNullOrEmpty(select.Fields) switch
                        {
                            false => JsonConvert.DeserializeObject<List<Field>>(select.Fields),
                            _ => asd.Fields
                        };

                        asd.PageSubCategories = string.IsNullOrEmpty(select.PageSubCategories) switch
                        {
                            false => new GetSiteSettingsObject.SubCategoriesUnion
                            {
                                SubCategoriesList =
                                    JsonConvert.DeserializeObject<Dictionary<string, List<SubCategories>>>(
                                        select.PageSubCategories)
                            },
                            _ => asd.PageSubCategories
                        };

                        asd.GroupSubCategories = string.IsNullOrEmpty(select.GroupSubCategories) switch
                        {
                            false => new GetSiteSettingsObject.SubCategoriesUnion
                            {
                                SubCategoriesList =
                                    JsonConvert.DeserializeObject<Dictionary<string, List<SubCategories>>>(
                                        select.GroupSubCategories)
                            },
                            _ => asd.GroupSubCategories
                        };

                        asd.ProductsSubCategories = string.IsNullOrEmpty(select.ProductsSubCategories) switch
                        {
                            false => new GetSiteSettingsObject.SubCategoriesUnion
                            {
                                SubCategoriesList =
                                    JsonConvert.DeserializeObject<Dictionary<string, List<SubCategories>>>(
                                        select.ProductsSubCategories)
                            },
                            _ => asd.ProductsSubCategories
                        };

                        asd.PageCustomFields = string.IsNullOrEmpty(select.PageCustomFields) switch
                        {
                            false => JsonConvert.DeserializeObject<List<CustomField>>(select.PageCustomFields),
                            _ => asd.PageCustomFields
                        };

                        asd.GroupCustomFields = string.IsNullOrEmpty(select.GroupCustomFields) switch
                        {
                            false => JsonConvert.DeserializeObject<List<CustomField>>(select.GroupCustomFields),
                            _ => asd.GroupCustomFields
                        };

                        asd.ProductCustomFields = string.IsNullOrEmpty(select.ProductCustomFields) switch
                        {
                            false => JsonConvert.DeserializeObject<List<CustomField>>(select.ProductCustomFields),
                            _ => asd.ProductCustomFields
                        };

                        asd.ProPackages = string.IsNullOrEmpty(select.ProPackages) switch
                        {
                            false => JsonConvert.DeserializeObject<Dictionary<string, DataProPackages>>(
                                select.ProPackages),
                            _ => asd.ProPackages
                        };

                        asd.ProPackagesTypes = string.IsNullOrEmpty(select.ProPackagesTypes) switch
                        {
                            false => JsonConvert
                                .DeserializeObject<Dictionary<string, string>>(select.ProPackagesTypes),
                            _ => asd.ProPackagesTypes
                        };

                        AppSettings.OneSignalAppId = asd.AndroidNPushId;
                       

                        Task.Factory.StartNew(() =>
                        {
                            try
                            {
                                //Page Categories
                                var listPage = asd.PageCategories.Select(cat => new Classes.Categories
                                {
                                    CategoriesId = cat.Key,
                                    CategoriesName = Methods.FunString.DecodeString(cat.Value),
                                    CategoriesColor = "#ffffff",
                                    SubList = new List<SubCategories>()
                                }).ToList();

                                CategoriesController.ListCategoriesPage.Clear();
                                CategoriesController.ListCategoriesPage = new ObservableCollection<Classes.Categories>(listPage);

                                switch (asd.PageSubCategories?.SubCategoriesList?.Count)
                                {
                                    case > 0:
                                    {
                                        //Sub Categories Page
                                        foreach (var sub in asd.PageSubCategories?.SubCategoriesList)
                                        {
                                            var subCategories = asd.PageSubCategories?.SubCategoriesList?.FirstOrDefault(a => a.Key == sub.Key).Value;
                                            switch (subCategories?.Count)
                                            {
                                                case > 0:
                                                {
                                                    var cat = CategoriesController.ListCategoriesPage.FirstOrDefault(a => a.CategoriesId == sub.Key);
                                                    if (cat != null)
                                                    {
                                                        foreach (var pairs in subCategories)
                                                        {
                                                            cat.SubList.Add(pairs);
                                                        }
                                                    }

                                                    break;
                                                }
                                            }
                                        }

                                        break;
                                    }
                                }

                                //Group Categories
                                var listGroup = asd.GroupCategories.Select(cat => new Classes.Categories
                                {
                                    CategoriesId = cat.Key,
                                    CategoriesName = Methods.FunString.DecodeString(cat.Value),
                                    CategoriesColor = "#ffffff",
                                    SubList = new List<SubCategories>()
                                }).ToList();

                                CategoriesController.ListCategoriesGroup.Clear();
                                CategoriesController.ListCategoriesGroup = new ObservableCollection<Classes.Categories>(listGroup);

                                switch (asd.GroupSubCategories?.SubCategoriesList?.Count)
                                {
                                    case > 0:
                                    {
                                        //Sub Categories Group
                                        foreach (var sub in asd.GroupSubCategories?.SubCategoriesList)
                                        {
                                            var subCategories = asd.GroupSubCategories?.SubCategoriesList?.FirstOrDefault(a => a.Key == sub.Key).Value;
                                            switch (subCategories?.Count)
                                            {
                                                case > 0:
                                                {
                                                    var cat = CategoriesController.ListCategoriesGroup.FirstOrDefault(a => a.CategoriesId == sub.Key);
                                                    if (cat != null)
                                                    {
                                                        foreach (var pairs in subCategories)
                                                        {
                                                            cat.SubList.Add(pairs);
                                                        }
                                                    }

                                                    break;
                                                }
                                            }
                                        }

                                        break;
                                    }
                                }

                                //Blog Categories
                                var listBlog = asd.BlogCategories.Select(cat => new Classes.Categories
                                {
                                    CategoriesId = cat.Key,
                                    CategoriesName = Methods.FunString.DecodeString(cat.Value),
                                    CategoriesColor = "#ffffff",
                                    SubList = new List<SubCategories>()
                                }).ToList();

                                CategoriesController.ListCategoriesBlog.Clear();
                                CategoriesController.ListCategoriesBlog = new ObservableCollection<Classes.Categories>(listBlog);

                                //Products Categories
                                var listProducts = asd.ProductsCategories.Select(cat => new Classes.Categories
                                {
                                    CategoriesId = cat.Key,
                                    CategoriesName = Methods.FunString.DecodeString(cat.Value),
                                    CategoriesColor = "#ffffff",
                                    SubList = new List<SubCategories>()
                                }).ToList();

                                CategoriesController.ListCategoriesProducts.Clear();
                                CategoriesController.ListCategoriesProducts = new ObservableCollection<Classes.Categories>(listProducts);

                                switch (asd.ProductsSubCategories?.SubCategoriesList?.Count)
                                {
                                    case > 0:
                                    {
                                        //Sub Categories Products
                                        foreach (var sub in asd.ProductsSubCategories?.SubCategoriesList)
                                        {
                                            var subCategories = asd.ProductsSubCategories?.SubCategoriesList?.FirstOrDefault(a => a.Key == sub.Key).Value;
                                            switch (subCategories?.Count)
                                            {
                                                case > 0:
                                                {
                                                    var cat = CategoriesController.ListCategoriesProducts.FirstOrDefault(a => a.CategoriesId == sub.Key);
                                                    if (cat != null)
                                                    {
                                                        foreach (var pairs in subCategories)
                                                        {
                                                            cat.SubList.Add(pairs);
                                                        }
                                                    }

                                                    break;
                                                }
                                            }
                                        }

                                        break;
                                    }
                                }

                                //Job Categories
                                var listJob = asd.JobCategories.Select(cat => new Classes.Categories
                                {
                                    CategoriesId = cat.Key,
                                    CategoriesName = Methods.FunString.DecodeString(cat.Value),
                                    CategoriesColor = "#ffffff",
                                    SubList = new List<SubCategories>()
                                }).ToList();

                                CategoriesController.ListCategoriesJob.Clear();
                                CategoriesController.ListCategoriesJob = new ObservableCollection<Classes.Categories>(listJob);

                                //Family
                                var listFamily = asd.Family.Select(cat => new Classes.Family
                                {
                                    FamilyId = cat.Key,
                                    FamilyName = Methods.FunString.DecodeString(cat.Value),
                                }).ToList();

                                ListUtils.FamilyList.Clear();
                                ListUtils.FamilyList = new ObservableCollection<Classes.Family>(listFamily);

                                //Movie Category
                                var listMovie = asd.MovieCategory.Select(cat => new Classes.Categories
                                {
                                    CategoriesId = cat.Key,
                                    CategoriesName = Methods.FunString.DecodeString(cat.Value),
                                    CategoriesColor = "#ffffff",
                                    SubList = new List<SubCategories>()
                                }).ToList();

                                CategoriesController.ListCategoriesMovies.Clear();
                                CategoriesController.ListCategoriesMovies = new ObservableCollection<Classes.Categories>(listMovie); 
                            }
                            catch (Exception e)
                            {
                                Methods.DisplayReportResultTrack(e);
                            }
                        });
                             
                        return asd;
                    }
                    else
                    {
                        return null!;
                    }
                }
                else
                {
                    return null!;
                }
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    return GetSettings();
                else
                {
                    Methods.DisplayReportResultTrack(e);
                    return null!;
                } 
            }
        }

        #endregion

        #region My Contacts >> Following

        //Insert data To My Contact Table
        public void Insert_Or_Replace_MyContactTable(ObservableCollection<UserDataObject> usersContactList)
        {
            try
            {
                using var connection = OpenConnection();
                switch (connection)
                {
                    case null:
                        return;
                }
                var result = connection.Table<DataTables.MyContactsTb>().ToList();
                List<DataTables.MyContactsTb> list = new List<DataTables.MyContactsTb>();

                connection.BeginTransaction();

                foreach (var info in usersContactList)
                {
                    var db = ClassMapper.Mapper?.Map<DataTables.MyContactsTb>(info);
                    if (info.Details.DetailsClass != null && db != null)
                    {
                        db.Details = JsonConvert.SerializeObject(info.Details.DetailsClass);
                        db.ApiNotificationSettings = JsonConvert.SerializeObject(info.ApiNotificationSettings.NotificationSettingsClass);
                        list.Add(db);
                    }
                       
                    var update = result.FirstOrDefault(a => a.UserId == info.UserId);
                    if (update != null)
                    {
                        update = ClassMapper.Mapper?.Map<DataTables.MyContactsTb>(info);
                        if (info.Details.DetailsClass != null && update != null)
                        {
                            update.Details = JsonConvert.SerializeObject(info.Details.DetailsClass); 
                            update.ApiNotificationSettings = JsonConvert.SerializeObject(info.ApiNotificationSettings.NotificationSettingsClass); 
                            connection.Update(update);
                        }
                    }
                }

                switch (list.Count)
                {
                    case <= 0:
                        return;
                }

                   
                //Bring new  
                var newItemList = list.Where(c => !result.Select(fc => fc.UserId).Contains(c.UserId)).ToList();
                switch (newItemList.Count)
                {
                    case > 0:
                        connection.InsertAll(newItemList);
                        break;
                }

                result = connection.Table<DataTables.MyContactsTb>().ToList();
                var deleteItemList = result.Where(c => !list.Select(fc => fc.UserId).Contains(c.UserId)).ToList();
                switch (deleteItemList.Count)
                {
                    case > 0:
                    {
                        foreach (var delete in deleteItemList)
                            connection.Delete(delete);
                        break;
                    }
                }

                connection.Commit();
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    Insert_Or_Replace_MyContactTable(usersContactList);
                else
                    Methods.DisplayReportResultTrack(e);
            }
        }

        // Get data To My Contact Table
        public ObservableCollection<UserDataObject> Get_MyContact(/*int id = 0, int nSize = 20*/)
        {
            try
            {
                using var connection = OpenConnection();
                switch (connection)
                {
                    case null:
                        return new ObservableCollection<UserDataObject>();
                }
                // var query = Connection.Table<DataTables.MyContactsTb>().Where(w => w.AutoIdMyFollowing >= id).OrderBy(q => q.AutoIdMyFollowing).Take(nSize).ToList();

                var select = connection.Table<DataTables.MyContactsTb>().ToList();
                switch (select.Count)
                {
                    case > 0:
                    {
                        var list = new ObservableCollection<UserDataObject>();

                        foreach (var item in select)
                        {
                            UserDataObject infoObject = new UserDataObject
                            {
                                UserId = item.UserId,
                                Username = item.Username,
                                Email = item.Email,
                                FirstName = item.FirstName,
                                LastName = item.LastName,
                                Avatar = item.Avatar,
                                Cover = item.Cover,
                                BackgroundImage = item.BackgroundImage,
                                RelationshipId = item.RelationshipId,
                                Address = item.Address,
                                Working = item.Working,
                                Gender = item.Gender,
                                Facebook = item.Facebook,
                                Google = item.Google,
                                Twitter = item.Twitter,
                                Linkedin = item.Linkedin,
                                Website = item.Website,
                                Instagram = item.Instagram,
                                WebDeviceId = item.WebDeviceId,
                                Language = item.Language,
                                IpAddress = item.IpAddress,
                                PhoneNumber = item.PhoneNumber,
                                Timezone = item.Timezone,
                                Lat = item.Lat,
                                Lng = item.Lng,
                                About = item.About,
                                Birthday = item.Birthday,
                                Registered = item.Registered,
                                Lastseen = item.Lastseen,
                                LastLocationUpdate = item.LastLocationUpdate,
                                Balance = item.Balance,
                                Verified = item.Verified,
                                Status = item.Status,
                                Active = item.Active,
                                Admin = item.Admin,
                                IsPro = item.IsPro,
                                ProType = item.ProType,
                                School = item.School,
                                Name = item.Name,
                                AndroidMDeviceId = item.AndroidMDeviceId,
                                ECommented = item.ECommented,
                                AndroidNDeviceId = item.AndroidMDeviceId,
                                AvatarFull = item.AvatarFull,
                                BirthPrivacy = item.BirthPrivacy,
                                CanFollow = item.CanFollow,
                                ConfirmFollowers = item.ConfirmFollowers,
                                CountryId = item.CountryId,
                                EAccepted = item.EAccepted,
                                EFollowed = item.EFollowed,
                                EJoinedGroup = item.EJoinedGroup,
                                ELastNotif = item.ELastNotif,
                                ELiked = item.ELiked,
                                ELikedPage = item.ELikedPage,
                                EMentioned = item.EMentioned,
                                EProfileWallPost = item.EProfileWallPost,
                                ESentmeMsg = item.ESentmeMsg,
                                EShared = item.EShared,
                                EVisited = item.EVisited,
                                EWondered = item.EWondered,
                                EmailNotification = item.EmailNotification,
                                FollowPrivacy = item.FollowPrivacy,
                                FriendPrivacy = item.FriendPrivacy,
                                GenderText = item.GenderText,
                                InfoFile = item.InfoFile,
                                IosMDeviceId = item.IosMDeviceId,
                                IosNDeviceId = item.IosNDeviceId,
                                IsBlocked = item.IsBlocked,
                                IsFollowing = item.IsFollowing,
                                IsFollowingMe = item.IsFollowingMe,
                                LastAvatarMod = item.LastAvatarMod,
                                LastCoverMod = item.LastCoverMod,
                                LastDataUpdate = item.LastDataUpdate,
                                LastFollowId = item.LastFollowId,
                                LastLoginData = item.LastLoginData,
                                LastseenStatus = item.LastseenStatus,
                                LastseenTimeText = item.LastseenTimeText,
                                LastseenUnixTime = item.LastseenUnixTime,
                                MessagePrivacy = item.MessagePrivacy,
                                NewEmail = item.NewEmail,
                                NewPhone = item.NewPhone,
                                NotificationsSound = item.NotificationsSound,
                                OrderPostsBy = item.OrderPostsBy,
                                PaypalEmail = item.PaypalEmail,
                                PostPrivacy = item.PostPrivacy,
                                Referrer = item.Referrer,
                                ShareMyData = item.ShareMyData,
                                ShareMyLocation = item.ShareMyLocation,
                                ShowActivitiesPrivacy = item.ShowActivitiesPrivacy,
                                TwoFactor = item.TwoFactor,
                                TwoFactorVerified = item.TwoFactorVerified,
                                Url = item.Url,
                                VisitPrivacy = item.VisitPrivacy,
                                Vk = item.Vk,
                                Wallet = item.Wallet,
                                WorkingLink = item.WorkingLink,
                                Youtube = item.Youtube,
                                City = item.City,
                                State = item.State,
                                Zip = item.Zip,
                                Points = item.Points,
                                DailyPoints = item.DailyPoints,
                                PointDayExpire = item.PointDayExpire,
                                CashfreeSignature = item.CashfreeSignature,
                                IsAdmin = item.IsAdmin,
                                MemberId = item.MemberId,
                                ChatColor = item.ChatColor,
                                PaystackRef = item.PaystackRef,
                                RefUserId = item.RefUserId,
                                SchoolCompleted = item.SchoolCompleted,
                                Type = item.Type,
                                UserPlatform = item.UserPlatform,
                                WeatherUnit = item.WeatherUnit,
                                AvatarPostId = item.AvatarPostId,
                                CodeSent = item.CodeSent,
                                CoverPostId = item.CoverPostId,
                                Discord = item.Discord,
                                IsArchive = item.IsArchive,
                                IsMute = item.IsMute,
                                IsPin = item.IsPin,
                                IsReported = item.IsReported,
                                IsStoryMuted = item.IsStoryMuted,
                                Mailru = item.Mailru,
                                NotificationSettings = item.NotificationSettings,
                                IsNotifyStopped = item.IsNotifyStopped,
                                Qq = item.Qq,
                                StripeSessionId = item.StripeSessionId,
                                Time = item.Time,
                                TimeCodeSent = item.TimeCodeSent,
                                Banned = item.Banned,
                                BannedReason = item.BannedReason,
                                CoinbaseCode = item.CoinbaseCode,
                                CoinbaseHash = item.CoinbaseHash,
                                CurrentlyWorking = item.CurrentlyWorking,
                                IsOpenToWork = item.IsOpenToWork,
                                IsProvidingService = item.IsProvidingService,
                                Languages = item.Languages,
                                OpenToWorkData = item.OpenToWorkData,
                                Permission = item.Permission,
                                ProvidingService = item.ProvidingService,
                                Skills = item.Skills,
                                Wechat = item.Wechat,
                                Details = new DetailsUnion(),
                                Selected = false,
                                ApiNotificationSettings = new NotificationSettingsUnion(), 
                            };

                            infoObject.Details = string.IsNullOrEmpty(item.Details) switch
                            {
                                false => new DetailsUnion
                                {
                                    DetailsClass = JsonConvert.DeserializeObject<Details>(item.Details)
                                },
                                _ => infoObject.Details
                            };
                                  
                            infoObject.ApiNotificationSettings = string.IsNullOrEmpty(item.ApiNotificationSettings) switch
                            {
                                false => new NotificationSettingsUnion
                                {
                                    NotificationSettingsClass = JsonConvert.DeserializeObject<NotificationSettings>(item.ApiNotificationSettings)
                                },
                                _ => infoObject.ApiNotificationSettings
                            };
                                  
                            list.Add(infoObject);
                        }

                        return list;
                    }
                    default:
                        return new ObservableCollection<UserDataObject>();
                }
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    return Get_MyContact();
                else
                {
                    Methods.DisplayReportResultTrack(e);
                    return new ObservableCollection<UserDataObject>();
                } 
            }
        }

        public void Insert_Or_Replace_OR_Delete_UsersContact(UserDataObject info, string type)
        {
            try
            {
                using var connection = OpenConnection();
                switch (connection)
                {
                    case null:
                        return;
                } 
                var user = connection.Table<DataTables.MyContactsTb>().FirstOrDefault(c => c.UserId == info.UserId);
                if (user != null)
                {
                    switch (type)
                    {
                        case "Delete":
                            connection.Delete(user);
                            break;
                        default: // Update
                        {
                            user = ClassMapper.Mapper?.Map<DataTables.MyContactsTb>(info);
                            if (info.Details.DetailsClass != null)
                                user.Details = JsonConvert.SerializeObject(info.Details.DetailsClass);
                             
                            if (info.ApiNotificationSettings.NotificationSettingsClass != null)
                                user.ApiNotificationSettings = JsonConvert.SerializeObject(info.ApiNotificationSettings.NotificationSettingsClass);
                             
                            connection.Update(user);
                            break;
                        }
                    }
                }
                else
                {
                    DataTables.MyContactsTb db = new DataTables.MyContactsTb
                    {
                        UserId = info.UserId,
                        Username = info.Username,
                        Email = info.Email,
                        FirstName = info.FirstName,
                        LastName = info.LastName,
                        Avatar = info.Avatar,
                        Cover = info.Cover,
                        BackgroundImage = info.BackgroundImage,
                        RelationshipId = info.RelationshipId,
                        Address = info.Address,
                        Working = info.Working,
                        Gender = info.Gender,
                        Facebook = info.Facebook,
                        Google = info.Google,
                        Twitter = info.Twitter,
                        Linkedin = info.Linkedin,
                        Website = info.Website,
                        Instagram = info.Instagram,
                        WebDeviceId = info.WebDeviceId,
                        Language = info.Language,
                        IpAddress = info.IpAddress,
                        PhoneNumber = info.PhoneNumber,
                        Timezone = info.Timezone,
                        Lat = info.Lat,
                        Lng = info.Lng,
                        Time = info.Time,
                        About = info.About,
                        Birthday = info.Birthday,
                        Registered = info.Registered,
                        Lastseen = info.Lastseen,
                        LastLocationUpdate = info.LastLocationUpdate,
                        Balance = info.Balance,
                        Verified = info.Verified,
                        Status = info.Status,
                        Active = info.Active,
                        Admin = info.Admin,
                        IsPro = info.IsPro,
                        ProType = info.ProType,
                        School = info.School,
                        Name = info.Name,
                        AndroidMDeviceId = info.AndroidMDeviceId,
                        ECommented = info.ECommented,
                        AndroidNDeviceId = info.AndroidMDeviceId,
                        AvatarFull = info.AvatarFull,
                        BirthPrivacy = info.BirthPrivacy,
                        CanFollow = info.CanFollow,
                        ConfirmFollowers = info.ConfirmFollowers,
                        CountryId = info.CountryId,
                        EAccepted = info.EAccepted,
                        EFollowed = info.EFollowed,
                        EJoinedGroup = info.EJoinedGroup,
                        ELastNotif = info.ELastNotif,
                        ELiked = info.ELiked,
                        ELikedPage = info.ELikedPage,
                        EMentioned = info.EMentioned,
                        EProfileWallPost = info.EProfileWallPost,
                        ESentmeMsg = info.ESentmeMsg,
                        EShared = info.EShared,
                        EVisited = info.EVisited,
                        EWondered = info.EWondered,
                        EmailNotification = info.EmailNotification,
                        FollowPrivacy = info.FollowPrivacy,
                        FriendPrivacy = info.FriendPrivacy,
                        GenderText = info.GenderText,
                        InfoFile = info.InfoFile,
                        IosMDeviceId = info.IosMDeviceId,
                        IosNDeviceId = info.IosNDeviceId,
                        IsBlocked = info.IsBlocked,
                        IsFollowing = info.IsFollowing,
                        IsFollowingMe = info.IsFollowingMe,
                        LastAvatarMod = info.LastAvatarMod,
                        LastCoverMod = info.LastCoverMod,
                        LastDataUpdate = info.LastDataUpdate,
                        LastFollowId = info.LastFollowId,
                        LastLoginData = info.LastLoginData,
                        LastseenStatus = info.LastseenStatus,
                        LastseenTimeText = info.LastseenTimeText,
                        LastseenUnixTime = info.LastseenUnixTime,
                        MessagePrivacy = info.MessagePrivacy,
                        NewEmail = info.NewEmail,
                        NewPhone = info.NewPhone,
                        NotificationsSound = info.NotificationsSound,
                        OrderPostsBy = info.OrderPostsBy,
                        PaypalEmail = info.PaypalEmail,
                        PostPrivacy = info.PostPrivacy,
                        Referrer = info.Referrer,
                        ShareMyData = info.ShareMyData,
                        ShareMyLocation = info.ShareMyLocation,
                        ShowActivitiesPrivacy = info.ShowActivitiesPrivacy,
                        TwoFactor = info.TwoFactor,
                        TwoFactorVerified = info.TwoFactorVerified,
                        Url = info.Url,
                        VisitPrivacy = info.VisitPrivacy,
                        Vk = info.Vk,
                        Wallet = info.Wallet,
                        WorkingLink = info.WorkingLink,
                        Youtube = info.Youtube,
                        City = info.City,
                        Points = info.Points,
                        DailyPoints = info.DailyPoints,
                        PointDayExpire = info.PointDayExpire,
                        State = info.State,
                        Zip = info.Zip,
                        CashfreeSignature = info.CashfreeSignature,
                        IsAdmin = info.IsAdmin,
                        MemberId = info.MemberId,
                        ChatColor = info.ChatColor,
                        PaystackRef = info.PaystackRef,
                        RefUserId = info.RefUserId,
                        SchoolCompleted = info.SchoolCompleted,
                        AvatarPostId = info.AvatarPostId,
                        CodeSent = info.CodeSent,
                        CoverPostId = info.CoverPostId,
                        Discord = info.Discord,
                        IsArchive = info.IsArchive,
                        IsMute = info.IsMute,
                        IsPin = info.IsPin,
                        IsReported = info.IsReported,
                        IsStoryMuted = info.IsStoryMuted,
                        Mailru = info.Mailru,
                        NotificationSettings = info.NotificationSettings,
                        IsNotifyStopped = info.IsNotifyStopped,
                        Qq = info.Qq,
                        StripeSessionId = info.StripeSessionId,
                        TimeCodeSent = info.TimeCodeSent,
                        Banned = info.Banned,
                        BannedReason = info.BannedReason,
                        CoinbaseCode = info.CoinbaseCode,
                        CoinbaseHash = info.CoinbaseHash,
                        CurrentlyWorking = info.CurrentlyWorking,
                        IsOpenToWork = info.IsOpenToWork,
                        IsProvidingService = info.IsProvidingService,
                        Languages = info.Languages,
                        OpenToWorkData = info.OpenToWorkData,
                        Permission = info.Permission,
                        ProvidingService = info.ProvidingService,
                        Skills = info.Skills,
                        Type = info.Type,
                        UserPlatform = info.UserPlatform,
                        WeatherUnit = info.WeatherUnit,
                        Wechat = info.Wechat,
                        ApiNotificationSettings = string.Empty,
                        Details = string.Empty,
                        Selected = false,
                    };

                    if (info.Details.DetailsClass != null)
                        db.Details = JsonConvert.SerializeObject(info.Details.DetailsClass);

                    if (info.ApiNotificationSettings.NotificationSettingsClass != null)
                        db.ApiNotificationSettings = JsonConvert.SerializeObject(info.ApiNotificationSettings.NotificationSettingsClass);

                    connection.Insert(db); 
                }
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    Insert_Or_Replace_OR_Delete_UsersContact(info, type);
                else
                    Methods.DisplayReportResultTrack(e);
            }
        }

        // Get data One user To My Contact Table
        public UserDataObject Get_DataOneUser(string userName)
        {
            try
            {
                using var connection = OpenConnection();
                var item = connection?.Table<DataTables.MyContactsTb>().FirstOrDefault(a => a.Username == userName || a.Name == userName);
                if (item != null)
                {
                    UserDataObject infoObject = new UserDataObject
                    {
                        UserId = item.UserId,
                        Username = item.Username,
                        Email = item.Email,
                        FirstName = item.FirstName,
                        LastName = item.LastName,
                        Avatar = item.Avatar,
                        Cover = item.Cover,
                        BackgroundImage = item.BackgroundImage,
                        RelationshipId = item.RelationshipId,
                        Address = item.Address,
                        Working = item.Working,
                        Gender = item.Gender,
                        Facebook = item.Facebook,
                        Google = item.Google,
                        Twitter = item.Twitter,
                        Linkedin = item.Linkedin,
                        Website = item.Website,
                        Instagram = item.Instagram,
                        WebDeviceId = item.WebDeviceId,
                        Language = item.Language,
                        IpAddress = item.IpAddress,
                        PhoneNumber = item.PhoneNumber,
                        Timezone = item.Timezone,
                        Lat = item.Lat,
                        Lng = item.Lng,
                        About = item.About,
                        Birthday = item.Birthday,
                        Registered = item.Registered,
                        Lastseen = item.Lastseen,
                        LastLocationUpdate = item.LastLocationUpdate,
                        Balance = item.Balance,
                        Verified = item.Verified,
                        Status = item.Status,
                        Active = item.Active,
                        Admin = item.Admin,
                        IsPro = item.IsPro,
                        ProType = item.ProType,
                        School = item.School,
                        Name = item.Name,
                        AndroidMDeviceId = item.AndroidMDeviceId,
                        ECommented = item.ECommented,
                        AndroidNDeviceId = item.AndroidMDeviceId,
                        AvatarFull = item.AvatarFull,
                        BirthPrivacy = item.BirthPrivacy,
                        CanFollow = item.CanFollow,
                        ConfirmFollowers = item.ConfirmFollowers,
                        CountryId = item.CountryId,
                        EAccepted = item.EAccepted,
                        EFollowed = item.EFollowed,
                        EJoinedGroup = item.EJoinedGroup,
                        ELastNotif = item.ELastNotif,
                        ELiked = item.ELiked,
                        ELikedPage = item.ELikedPage,
                        EMentioned = item.EMentioned,
                        EProfileWallPost = item.EProfileWallPost,
                        ESentmeMsg = item.ESentmeMsg,
                        EShared = item.EShared,
                        EVisited = item.EVisited,
                        EWondered = item.EWondered,
                        EmailNotification = item.EmailNotification,
                        FollowPrivacy = item.FollowPrivacy,
                        FriendPrivacy = item.FriendPrivacy,
                        GenderText = item.GenderText,
                        InfoFile = item.InfoFile,
                        IosMDeviceId = item.IosMDeviceId,
                        IosNDeviceId = item.IosNDeviceId,
                        IsBlocked = item.IsBlocked,
                        IsFollowing = item.IsFollowing,
                        IsFollowingMe = item.IsFollowingMe,
                        LastAvatarMod = item.LastAvatarMod,
                        LastCoverMod = item.LastCoverMod,
                        LastDataUpdate = item.LastDataUpdate,
                        LastFollowId = item.LastFollowId,
                        LastLoginData = item.LastLoginData,
                        LastseenStatus = item.LastseenStatus,
                        LastseenTimeText = item.LastseenTimeText,
                        LastseenUnixTime = item.LastseenUnixTime,
                        MessagePrivacy = item.MessagePrivacy,
                        NewEmail = item.NewEmail,
                        NewPhone = item.NewPhone,
                        NotificationsSound = item.NotificationsSound,
                        OrderPostsBy = item.OrderPostsBy,
                        PaypalEmail = item.PaypalEmail,
                        PostPrivacy = item.PostPrivacy,
                        Referrer = item.Referrer,
                        ShareMyData = item.ShareMyData,
                        ShareMyLocation = item.ShareMyLocation,
                        ShowActivitiesPrivacy = item.ShowActivitiesPrivacy,
                        TwoFactor = item.TwoFactor,
                        TwoFactorVerified = item.TwoFactorVerified,
                        Url = item.Url,
                        VisitPrivacy = item.VisitPrivacy,
                        Vk = item.Vk,
                        Wallet = item.Wallet,
                        WorkingLink = item.WorkingLink,
                        Youtube = item.Youtube,
                        City = item.City,
                        State = item.State,
                        Zip = item.Zip,
                        Points = item.Points,
                        DailyPoints = item.DailyPoints,
                        PointDayExpire = item.PointDayExpire,
                        CashfreeSignature = item.CashfreeSignature,
                        IsAdmin = item.IsAdmin,
                        MemberId = item.MemberId,
                        ChatColor = item.ChatColor,
                        PaystackRef = item.PaystackRef,
                        RefUserId = item.RefUserId,
                        SchoolCompleted = item.SchoolCompleted,
                        Type = item.Type,
                        UserPlatform = item.UserPlatform,
                        WeatherUnit = item.WeatherUnit,
                        AvatarPostId = item.AvatarPostId,
                        CodeSent = item.CodeSent,
                        CoverPostId = item.CoverPostId,
                        Discord = item.Discord,
                        IsArchive = item.IsArchive,
                        IsMute = item.IsMute,
                        IsPin = item.IsPin,
                        IsReported = item.IsReported,
                        IsStoryMuted = item.IsStoryMuted,
                        Mailru = item.Mailru,
                        NotificationSettings = item.NotificationSettings,
                        IsNotifyStopped = item.IsNotifyStopped,
                        Qq = item.Qq,
                        StripeSessionId = item.StripeSessionId,
                        Time = item.Time,
                        TimeCodeSent = item.TimeCodeSent,
                        Banned = item.Banned,
                        BannedReason = item.BannedReason,
                        CoinbaseCode = item.CoinbaseCode,
                        CoinbaseHash = item.CoinbaseHash,
                        CurrentlyWorking = item.CurrentlyWorking,
                        IsOpenToWork = item.IsOpenToWork,
                        IsProvidingService = item.IsProvidingService,
                        Languages = item.Languages,
                        OpenToWorkData = item.OpenToWorkData,
                        Permission = item.Permission,
                        ProvidingService = item.ProvidingService,
                        Skills = item.Skills,
                        Wechat = item.Wechat,
                        Details = new DetailsUnion(),
                        Selected = false,
                        ApiNotificationSettings = new NotificationSettingsUnion(),
                    };

                    infoObject.Details = string.IsNullOrEmpty(item.Details) switch
                    {
                        false => new DetailsUnion {DetailsClass = JsonConvert.DeserializeObject<Details>(item.Details)},
                        _ => infoObject.Details
                    };
                    
                    infoObject.ApiNotificationSettings = string.IsNullOrEmpty(item.ApiNotificationSettings) switch
                    {
                        false => new NotificationSettingsUnion { NotificationSettingsClass = JsonConvert.DeserializeObject<NotificationSettings>(item.ApiNotificationSettings) },
                        _ => infoObject.ApiNotificationSettings
                    };
                     
                    return infoObject;
                }
                else
                {
                    var infoObject = ListUtils.MyFollowersList.FirstOrDefault(a => a.Username == userName || a.Name == userName);
                    if (infoObject != null) return infoObject;
                }

                return null!;
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    return Get_DataOneUser(userName);
                else
                {
                    Methods.DisplayReportResultTrack(e);
                    return null!;
                } 
            }
        }

        #endregion
         
        #region My Profile

        //Insert Or Update data My Profile Table
        public void Insert_Or_Update_To_MyProfileTable(UserDataObject info)
        {
            try
            {
                using var connection = OpenConnection();
                switch (connection)
                {
                    case null:
                        return;
                }
                var resultInfoTb = connection.Table<DataTables.MyProfileTb>().FirstOrDefault();
                if (resultInfoTb != null)
                {
                    resultInfoTb = new DataTables.MyProfileTb
                    {
                        UserId = info.UserId,
                        Username = info.Username,
                        Email = info.Email,
                        FirstName = info.FirstName,
                        LastName = info.LastName,
                        Avatar = info.Avatar,
                        Cover = info.Cover,
                        BackgroundImage = info.BackgroundImage,
                        RelationshipId = info.RelationshipId,
                        Address = info.Address,
                        Working = info.Working,
                        Gender = info.Gender,
                        Facebook = info.Facebook,
                        Google = info.Google,
                        Twitter = info.Twitter,
                        Linkedin = info.Linkedin,
                        Website = info.Website,
                        Instagram = info.Instagram,
                        WebDeviceId = info.WebDeviceId,
                        Language = info.Language,
                        IpAddress = info.IpAddress,
                        PhoneNumber = info.PhoneNumber,
                        Timezone = info.Timezone,
                        Lat = info.Lat,
                        Lng = info.Lng,
                        Time = info.Time,
                        About = info.About,
                        Birthday = info.Birthday,
                        Registered = info.Registered,
                        Lastseen = info.Lastseen,
                        LastLocationUpdate = info.LastLocationUpdate,
                        Balance = info.Balance,
                        Verified = info.Verified,
                        Status = info.Status,
                        Active = info.Active,
                        Admin = info.Admin,
                        IsPro = info.IsPro,
                        ProType = info.ProType,
                        School = info.School,
                        Name = info.Name,
                        AndroidMDeviceId = info.AndroidMDeviceId,
                        ECommented = info.ECommented,
                        AndroidNDeviceId = info.AndroidMDeviceId,
                        AvatarFull = info.AvatarFull,
                        BirthPrivacy = info.BirthPrivacy,
                        CanFollow = info.CanFollow,
                        ConfirmFollowers = info.ConfirmFollowers,
                        CountryId = info.CountryId,
                        EAccepted = info.EAccepted,
                        EFollowed = info.EFollowed,
                        EJoinedGroup = info.EJoinedGroup,
                        ELastNotif = info.ELastNotif,
                        ELiked = info.ELiked,
                        ELikedPage = info.ELikedPage,
                        EMentioned = info.EMentioned,
                        EProfileWallPost = info.EProfileWallPost,
                        ESentmeMsg = info.ESentmeMsg,
                        EShared = info.EShared,
                        EVisited = info.EVisited,
                        EWondered = info.EWondered,
                        EmailNotification = info.EmailNotification,
                        FollowPrivacy = info.FollowPrivacy,
                        FriendPrivacy = info.FriendPrivacy,
                        GenderText = info.GenderText,
                        InfoFile = info.InfoFile,
                        IosMDeviceId = info.IosMDeviceId,
                        IosNDeviceId = info.IosNDeviceId,
                        IsBlocked = info.IsBlocked,
                        IsFollowing = info.IsFollowing,
                        IsFollowingMe = info.IsFollowingMe,
                        LastAvatarMod = info.LastAvatarMod,
                        LastCoverMod = info.LastCoverMod,
                        LastDataUpdate = info.LastDataUpdate,
                        LastFollowId = info.LastFollowId,
                        LastLoginData = info.LastLoginData,
                        LastseenStatus = info.LastseenStatus,
                        LastseenTimeText = info.LastseenTimeText,
                        LastseenUnixTime = info.LastseenUnixTime,
                        MessagePrivacy = info.MessagePrivacy,
                        NewEmail = info.NewEmail,
                        NewPhone = info.NewPhone,
                        NotificationsSound = info.NotificationsSound,
                        OrderPostsBy = info.OrderPostsBy,
                        PaypalEmail = info.PaypalEmail,
                        PostPrivacy = info.PostPrivacy,
                        Referrer = info.Referrer,
                        ShareMyData = info.ShareMyData,
                        ShareMyLocation = info.ShareMyLocation,
                        ShowActivitiesPrivacy = info.ShowActivitiesPrivacy,
                        TwoFactor = info.TwoFactor,
                        TwoFactorVerified = info.TwoFactorVerified,
                        Url = info.Url,
                        VisitPrivacy = info.VisitPrivacy,
                        Vk = info.Vk,
                        Wallet = info.Wallet,
                        WorkingLink = info.WorkingLink,
                        Youtube = info.Youtube,
                        City = info.City,
                        Points = info.Points,
                        DailyPoints = info.DailyPoints,
                        PointDayExpire = info.PointDayExpire,
                        State = info.State,
                        Zip = info.Zip,
                        CashfreeSignature = info.CashfreeSignature,
                        IsAdmin = info.IsAdmin,
                        MemberId = info.MemberId,
                        ChatColor = info.ChatColor,
                        PaystackRef = info.PaystackRef,
                        RefUserId = info.RefUserId,
                        SchoolCompleted = info.SchoolCompleted,
                        AvatarPostId = info.AvatarPostId,
                        CodeSent = info.CodeSent,
                        CoverPostId = info.CoverPostId,
                        Discord = info.Discord,
                        IsArchive = info.IsArchive,
                        IsMute = info.IsMute,
                        IsPin = info.IsPin,
                        IsReported = info.IsReported,
                        IsStoryMuted = info.IsStoryMuted,
                        Mailru = info.Mailru,
                        NotificationSettings = info.NotificationSettings,
                        IsNotifyStopped = info.IsNotifyStopped,
                        Qq = info.Qq,
                        StripeSessionId = info.StripeSessionId,
                        TimeCodeSent = info.TimeCodeSent,
                        Wechat = info.Wechat,
                        Banned = info.Banned,
                        BannedReason = info.BannedReason,
                        CoinbaseCode = info.CoinbaseCode,
                        CoinbaseHash = info.CoinbaseHash,
                        CurrentlyWorking = info.CurrentlyWorking,
                        IsOpenToWork = info.IsOpenToWork,
                        IsProvidingService = info.IsProvidingService,
                        Languages = info.Languages,
                        OpenToWorkData = info.OpenToWorkData,
                        Permission = info.Permission,
                        ProvidingService = info.ProvidingService,
                        Skills = info.Skills,
                        Type = info.Type,
                        UserPlatform = info.UserPlatform,
                        WeatherUnit = info.WeatherUnit,
                        ApiNotificationSettings = string.Empty,
                        Details = string.Empty,
                        Selected = false,
                    };

                    if (info.Details.DetailsClass != null)
                        resultInfoTb.Details = JsonConvert.SerializeObject(info.Details.DetailsClass);
                     
                    if (info.ApiNotificationSettings.NotificationSettingsClass != null)
                        resultInfoTb.ApiNotificationSettings = JsonConvert.SerializeObject(info.ApiNotificationSettings.NotificationSettingsClass);
                     
                    connection.Update(resultInfoTb);
                }
                else
                {
                    DataTables.MyProfileTb db = new DataTables.MyProfileTb
                    {
                        UserId = info.UserId,
                        Username = info.Username,
                        Email = info.Email,
                        FirstName = info.FirstName,
                        LastName = info.LastName,
                        Avatar = info.Avatar,
                        Cover = info.Cover,
                        BackgroundImage = info.BackgroundImage,
                        RelationshipId = info.RelationshipId,
                        Address = info.Address,
                        Working = info.Working,
                        Gender = info.Gender,
                        Facebook = info.Facebook,
                        Google = info.Google,
                        Twitter = info.Twitter,
                        Linkedin = info.Linkedin,
                        Website = info.Website,
                        Instagram = info.Instagram,
                        WebDeviceId = info.WebDeviceId,
                        Language = info.Language,
                        IpAddress = info.IpAddress,
                        PhoneNumber = info.PhoneNumber,
                        Timezone = info.Timezone,
                        Lat = info.Lat,
                        Lng = info.Lng,
                        Time = info.Time,
                        About = info.About,
                        Birthday = info.Birthday,
                        Registered = info.Registered,
                        Lastseen = info.Lastseen,
                        LastLocationUpdate = info.LastLocationUpdate,
                        Balance = info.Balance,
                        Verified = info.Verified,
                        Status = info.Status,
                        Active = info.Active,
                        Admin = info.Admin,
                        IsPro = info.IsPro,
                        ProType = info.ProType,
                        School = info.School,
                        Name = info.Name,
                        AndroidMDeviceId = info.AndroidMDeviceId,
                        ECommented = info.ECommented,
                        AndroidNDeviceId = info.AndroidMDeviceId,
                        AvatarFull = info.AvatarFull,
                        BirthPrivacy = info.BirthPrivacy,
                        CanFollow = info.CanFollow,
                        ConfirmFollowers = info.ConfirmFollowers,
                        CountryId = info.CountryId,
                        EAccepted = info.EAccepted,
                        EFollowed = info.EFollowed,
                        EJoinedGroup = info.EJoinedGroup,
                        ELastNotif = info.ELastNotif,
                        ELiked = info.ELiked,
                        ELikedPage = info.ELikedPage,
                        EMentioned = info.EMentioned,
                        EProfileWallPost = info.EProfileWallPost,
                        ESentmeMsg = info.ESentmeMsg,
                        EShared = info.EShared,
                        EVisited = info.EVisited,
                        EWondered = info.EWondered,
                        EmailNotification = info.EmailNotification,
                        FollowPrivacy = info.FollowPrivacy,
                        FriendPrivacy = info.FriendPrivacy,
                        GenderText = info.GenderText,
                        InfoFile = info.InfoFile,
                        IosMDeviceId = info.IosMDeviceId,
                        IosNDeviceId = info.IosNDeviceId,
                        IsBlocked = info.IsBlocked,
                        IsFollowing = info.IsFollowing,
                        IsFollowingMe = info.IsFollowingMe,
                        LastAvatarMod = info.LastAvatarMod,
                        LastCoverMod = info.LastCoverMod,
                        LastDataUpdate = info.LastDataUpdate,
                        LastFollowId = info.LastFollowId,
                        LastLoginData = info.LastLoginData,
                        LastseenStatus = info.LastseenStatus,
                        LastseenTimeText = info.LastseenTimeText,
                        LastseenUnixTime = info.LastseenUnixTime,
                        MessagePrivacy = info.MessagePrivacy,
                        NewEmail = info.NewEmail,
                        NewPhone = info.NewPhone,
                        NotificationsSound = info.NotificationsSound,
                        OrderPostsBy = info.OrderPostsBy,
                        PaypalEmail = info.PaypalEmail,
                        PostPrivacy = info.PostPrivacy,
                        Referrer = info.Referrer,
                        ShareMyData = info.ShareMyData,
                        ShareMyLocation = info.ShareMyLocation,
                        ShowActivitiesPrivacy = info.ShowActivitiesPrivacy,
                        TwoFactor = info.TwoFactor,
                        TwoFactorVerified = info.TwoFactorVerified,
                        Url = info.Url,
                        VisitPrivacy = info.VisitPrivacy,
                        Vk = info.Vk,
                        Wallet = info.Wallet,
                        WorkingLink = info.WorkingLink,
                        Youtube = info.Youtube,
                        City = info.City,
                        Points = info.Points,
                        DailyPoints = info.DailyPoints,
                        PointDayExpire = info.PointDayExpire,
                        State = info.State,
                        Zip = info.Zip,
                        CashfreeSignature = info.CashfreeSignature,
                        IsAdmin = info.IsAdmin,
                        MemberId = info.MemberId,
                        ChatColor = info.ChatColor,
                        PaystackRef = info.PaystackRef,
                        RefUserId = info.RefUserId,
                        SchoolCompleted = info.SchoolCompleted,
                        AvatarPostId = info.AvatarPostId,
                        CodeSent = info.CodeSent,
                        CoverPostId = info.CoverPostId,
                        Discord = info.Discord,
                        IsArchive = info.IsArchive,
                        IsMute = info.IsMute,
                        IsPin = info.IsPin,
                        IsReported = info.IsReported,
                        IsStoryMuted = info.IsStoryMuted,
                        Mailru = info.Mailru,
                        NotificationSettings = info.NotificationSettings,
                        IsNotifyStopped = info.IsNotifyStopped,
                        Qq = info.Qq,
                        StripeSessionId = info.StripeSessionId,
                        TimeCodeSent = info.TimeCodeSent,
                        Banned = info.Banned,
                        BannedReason = info.BannedReason,
                        CoinbaseCode = info.CoinbaseCode,
                        CoinbaseHash = info.CoinbaseHash,
                        CurrentlyWorking = info.CurrentlyWorking,
                        IsOpenToWork = info.IsOpenToWork,
                        IsProvidingService = info.IsProvidingService,
                        Languages = info.Languages,
                        OpenToWorkData = info.OpenToWorkData,
                        Permission = info.Permission,
                        ProvidingService = info.ProvidingService,
                        Skills = info.Skills,
                        Type = info.Type,
                        UserPlatform = info.UserPlatform,
                        WeatherUnit = info.WeatherUnit,
                        Wechat = info.Wechat,
                        ApiNotificationSettings = string.Empty,
                        Details = string.Empty,
                        Selected = false,
                    };

                    if (info.Details.DetailsClass != null)
                        db.Details = JsonConvert.SerializeObject(info.Details.DetailsClass);
                     
                    if (info.ApiNotificationSettings.NotificationSettingsClass != null)
                        db.ApiNotificationSettings = JsonConvert.SerializeObject(info.ApiNotificationSettings.NotificationSettingsClass);
                     
                    connection.Insert(db);
                }

                UserDetails.Avatar = info.Avatar;
                UserDetails.Cover = info.Cover;
                UserDetails.Username = info.Username;
                UserDetails.FullName = info.Name;
                UserDetails.Email = info.Email;

                ListUtils.MyProfileList = new ObservableCollection<UserDataObject>();
                ListUtils.MyProfileList?.Clear();
                ListUtils.MyProfileList?.Add(info);
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    Insert_Or_Update_To_MyProfileTable(info);
                else
                    Methods.DisplayReportResultTrack(e);
            }
        }

        // Get data To My Profile Table
        public UserDataObject Get_MyProfile()
        {
            try
            {
                using var connection = OpenConnection();
                switch (connection)
                {
                    case null:
                        return null!;
                }
                var item = connection.Table<DataTables.MyProfileTb>().FirstOrDefault();
                if (item != null)
                {
                    UserDataObject infoObject = new UserDataObject
                    {
                        UserId = item.UserId,
                        Username = item.Username,
                        Email = item.Email,
                        FirstName = item.FirstName,
                        LastName = item.LastName,
                        Avatar = item.Avatar,
                        Cover = item.Cover,
                        BackgroundImage = item.BackgroundImage,
                        RelationshipId = item.RelationshipId,
                        Address = item.Address,
                        Working = item.Working,
                        Gender = item.Gender,
                        Facebook = item.Facebook,
                        Google = item.Google,
                        Twitter = item.Twitter,
                        Linkedin = item.Linkedin,
                        Website = item.Website,
                        Instagram = item.Instagram,
                        WebDeviceId = item.WebDeviceId,
                        Language = item.Language,
                        IpAddress = item.IpAddress,
                        PhoneNumber = item.PhoneNumber,
                        Timezone = item.Timezone,
                        Lat = item.Lat,
                        Lng = item.Lng,
                        About = item.About,
                        Birthday = item.Birthday,
                        Registered = item.Registered,
                        Lastseen = item.Lastseen,
                        LastLocationUpdate = item.LastLocationUpdate,
                        Balance = item.Balance,
                        Verified = item.Verified,
                        Status = item.Status,
                        Active = item.Active,
                        Admin = item.Admin,
                        IsPro = item.IsPro,
                        ProType = item.ProType,
                        School = item.School,
                        Name = item.Name,
                        AndroidMDeviceId = item.AndroidMDeviceId,
                        ECommented = item.ECommented,
                        AndroidNDeviceId = item.AndroidMDeviceId,
                        AvatarFull = item.AvatarFull,
                        BirthPrivacy = item.BirthPrivacy,
                        CanFollow = item.CanFollow,
                        ConfirmFollowers = item.ConfirmFollowers,
                        CountryId = item.CountryId,
                        EAccepted = item.EAccepted,
                        EFollowed = item.EFollowed,
                        EJoinedGroup = item.EJoinedGroup,
                        ELastNotif = item.ELastNotif,
                        ELiked = item.ELiked,
                        ELikedPage = item.ELikedPage,
                        EMentioned = item.EMentioned,
                        EProfileWallPost = item.EProfileWallPost,
                        ESentmeMsg = item.ESentmeMsg,
                        EShared = item.EShared,
                        EVisited = item.EVisited,
                        EWondered = item.EWondered,
                        EmailNotification = item.EmailNotification,
                        FollowPrivacy = item.FollowPrivacy,
                        FriendPrivacy = item.FriendPrivacy,
                        GenderText = item.GenderText,
                        InfoFile = item.InfoFile,
                        IosMDeviceId = item.IosMDeviceId,
                        IosNDeviceId = item.IosNDeviceId,
                        IsBlocked = item.IsBlocked,
                        IsFollowing = item.IsFollowing,
                        IsFollowingMe = item.IsFollowingMe,
                        LastAvatarMod = item.LastAvatarMod,
                        LastCoverMod = item.LastCoverMod,
                        LastDataUpdate = item.LastDataUpdate,
                        LastFollowId = item.LastFollowId,
                        LastLoginData = item.LastLoginData,
                        LastseenStatus = item.LastseenStatus,
                        LastseenTimeText = item.LastseenTimeText,
                        LastseenUnixTime = item.LastseenUnixTime,
                        MessagePrivacy = item.MessagePrivacy,
                        NewEmail = item.NewEmail,
                        NewPhone = item.NewPhone,
                        NotificationsSound = item.NotificationsSound,
                        OrderPostsBy = item.OrderPostsBy,
                        PaypalEmail = item.PaypalEmail,
                        PostPrivacy = item.PostPrivacy,
                        Referrer = item.Referrer,
                        ShareMyData = item.ShareMyData,
                        ShareMyLocation = item.ShareMyLocation,
                        ShowActivitiesPrivacy = item.ShowActivitiesPrivacy,
                        TwoFactor = item.TwoFactor,
                        TwoFactorVerified = item.TwoFactorVerified,
                        Url = item.Url,
                        VisitPrivacy = item.VisitPrivacy,
                        Vk = item.Vk,
                        Wallet = item.Wallet,
                        WorkingLink = item.WorkingLink,
                        Youtube = item.Youtube,
                        City = item.City,
                        State = item.State,
                        Zip = item.Zip,
                        Points = item.Points,
                        DailyPoints = item.DailyPoints,
                        PointDayExpire = item.PointDayExpire,
                        CashfreeSignature = item.CashfreeSignature,
                        IsAdmin = item.IsAdmin,
                        MemberId = item.MemberId,
                        ChatColor = item.ChatColor,
                        PaystackRef = item.PaystackRef,
                        RefUserId = item.RefUserId,
                        SchoolCompleted = item.SchoolCompleted,
                        Type = item.Type,
                        UserPlatform = item.UserPlatform,
                        WeatherUnit = item.WeatherUnit,
                        AvatarPostId = item.AvatarPostId,
                        CodeSent = item.CodeSent,
                        CoverPostId = item.CoverPostId,
                        Discord = item.Discord,
                        IsArchive = item.IsArchive,
                        IsMute = item.IsMute,
                        IsPin = item.IsPin,
                        IsReported = item.IsReported,
                        IsStoryMuted = item.IsStoryMuted,
                        Mailru = item.Mailru,
                        NotificationSettings = item.NotificationSettings,
                        IsNotifyStopped = item.IsNotifyStopped,
                        Qq = item.Qq,
                        StripeSessionId = item.StripeSessionId,
                        Time = item.Time,
                        TimeCodeSent = item.TimeCodeSent,
                        Banned = item.Banned,
                        BannedReason = item.BannedReason,
                        CoinbaseCode = item.CoinbaseCode,
                        CoinbaseHash = item.CoinbaseHash,
                        CurrentlyWorking = item.CurrentlyWorking,
                        IsOpenToWork = item.IsOpenToWork,
                        IsProvidingService = item.IsProvidingService,
                        Languages = item.Languages,
                        OpenToWorkData = item.OpenToWorkData,
                        Permission = item.Permission,
                        ProvidingService = item.ProvidingService,
                        Skills = item.Skills,
                        Wechat = item.Wechat,
                        Details = new DetailsUnion(),
                        Selected = false,
                        ApiNotificationSettings = new NotificationSettingsUnion(),
                    };

                    infoObject.Details = string.IsNullOrEmpty(item.Details) switch
                    {
                        false => new DetailsUnion {DetailsClass = JsonConvert.DeserializeObject<Details>(item.Details)},
                        _ => infoObject.Details
                    };
                     
                    infoObject.ApiNotificationSettings = string.IsNullOrEmpty(item.ApiNotificationSettings) switch
                    {
                        false => new NotificationSettingsUnion { NotificationSettingsClass = JsonConvert.DeserializeObject<NotificationSettings>(item.ApiNotificationSettings) },
                        _ => infoObject.ApiNotificationSettings
                    };
                     
                    UserDetails.Avatar = item.Avatar;
                    UserDetails.Cover = item.Cover;
                    UserDetails.Username = item.Username;
                    UserDetails.FullName = item.Name;
                    UserDetails.Email = item.Email;

                    ListUtils.MyProfileList = new ObservableCollection<UserDataObject> {infoObject};

                    return infoObject;
                }
                else
                {
                    return null!;
                }
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    return Get_MyProfile();
                else
                {
                    Methods.DisplayReportResultTrack(e);
                    return null!;
                } 
            }
        }

        #endregion

        #region Search Filter 

        public void InsertOrUpdate_SearchFilter(DataTables.SearchFilterTb dataFilter)
        {
            try
            {
                using var connection = OpenConnection();
                switch (connection)
                {
                    case null:
                        return;
                }
                var data = connection.Table<DataTables.SearchFilterTb>().FirstOrDefault();
                switch (data)
                {
                    case null:
                        connection.Insert(dataFilter);
                        break;
                    default:
                        data.Gender = dataFilter.Gender;
                        data.Country = dataFilter.Country;
                        data.Status = dataFilter.Status;
                        data.Verified = dataFilter.Verified;
                        data.ProfilePicture = dataFilter.ProfilePicture;
                        data.FilterByAge = dataFilter.FilterByAge;
                        data.AgeFrom = dataFilter.AgeFrom;
                        data.AgeTo = dataFilter.AgeTo;

                        connection.Update(data);
                        break;
                }
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    InsertOrUpdate_SearchFilter(dataFilter);
                else
                    Methods.DisplayReportResultTrack(e);
            }
        }

        public DataTables.SearchFilterTb GetSearchFilterById()
        {
            try
            {
                using var connection = OpenConnection();
                var data = connection?.Table<DataTables.SearchFilterTb>().FirstOrDefault();
                return data;
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    return GetSearchFilterById();
                else
                {
                    Methods.DisplayReportResultTrack(e);
                    return null!;
                }
            }
        }

        #endregion

        #region Near By Filter 

        public void InsertOrUpdate_NearByFilter(DataTables.NearByFilterTb dataFilter)
        {
            try
            {
                using var connection = OpenConnection();
                switch (connection)
                {
                    case null:
                        return;
                }
                var data = connection.Table<DataTables.NearByFilterTb>().FirstOrDefault();
                switch (data)
                {
                    case null:
                        connection.Insert(dataFilter);
                        break;
                    default:
                        data.DistanceValue = dataFilter.DistanceValue;
                        data.Gender = dataFilter.Gender;
                        data.Status = dataFilter.Status;
                        data.Relationship = dataFilter.Relationship;

                        connection.Update(data);
                        break;
                }
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    InsertOrUpdate_NearByFilter(dataFilter);
                else
                    Methods.DisplayReportResultTrack(e);
            }
        }

        public DataTables.NearByFilterTb GetNearByFilterById()
        {
            try
            {
                using var connection = OpenConnection();
                var data = connection?.Table<DataTables.NearByFilterTb>().FirstOrDefault();
                return data;
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    return GetNearByFilterById();
                else
                {
                    Methods.DisplayReportResultTrack(e);
                    return null!;
                }
            }
        }

        #endregion

        #region WatchOffline Videos

        //Insert WatchOffline Videos
        public void Insert_WatchOfflineVideos(MoviesDataObject video)
        {
            try
            {
                using var connection = OpenConnection();
                switch (connection)
                {
                    case null:
                        return;
                }
                if (video != null)
                {
                    var select = connection.Table<DataTables.WatchOfflineVideosTb>().FirstOrDefault(a => a.Id == video.Id);
                    switch (select)
                    {
                        case null:
                        {
                            DataTables.WatchOfflineVideosTb watchOffline = new DataTables.WatchOfflineVideosTb
                            {
                                Id = video.Id,
                                Name = video.Name,
                                Cover = video.Cover,
                                Description = video.Description,
                                Country = video.Country,
                                Duration = video.Duration,
                                Genre = video.Genre,
                                Iframe = video.Iframe,
                                Quality = video.Quality,
                                Producer = video.Producer,
                                Release = video.Release,
                                Source = video.Source,
                                Stars = video.Stars,
                                Url = video.Url,
                                Video = video.Video,
                                Views = video.Views,
                            };

                            connection.Insert(watchOffline);
                            break;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    Insert_WatchOfflineVideos(video);
                else
                    Methods.DisplayReportResultTrack(e);
            }
        }

        //Remove WatchOffline Videos
        public void Remove_WatchOfflineVideos(string watchOfflineVideosId)
        {
            try
            {
                using var connection = OpenConnection();
                switch (connection)
                {
                    case null:
                        return;
                }
                switch (string.IsNullOrEmpty(watchOfflineVideosId))
                {
                    case false:
                    {
                        var select = connection.Table<DataTables.WatchOfflineVideosTb>().FirstOrDefault(a => a.Id == watchOfflineVideosId);
                        if (select != null)
                        {
                            connection.Delete(select);
                        }

                        break;
                    }
                }
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    Remove_WatchOfflineVideos(watchOfflineVideosId);
                else
                    Methods.DisplayReportResultTrack(e);
            }
        }
         
        //Get WatchOffline Videos
        public ObservableCollection<DataTables.WatchOfflineVideosTb> Get_WatchOfflineVideos()
        {
            try
            {
                using var connection = OpenConnection();
                switch (connection)
                {
                    case null:
                        return new ObservableCollection<DataTables.WatchOfflineVideosTb>();
                }
                var select = connection.Table<DataTables.WatchOfflineVideosTb>().OrderByDescending(a => a.AutoIdWatchOfflineVideos).ToList();
                return select.Count switch
                {
                    > 0 => new ObservableCollection<DataTables.WatchOfflineVideosTb>(select),
                    _ => new ObservableCollection<DataTables.WatchOfflineVideosTb>()
                };
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    return Get_WatchOfflineVideos();
                else
                {
                    Methods.DisplayReportResultTrack(e);
                    return new ObservableCollection<DataTables.WatchOfflineVideosTb>();
                } 
            }
        }

        //Get WatchOffline Videos
        public MoviesDataObject Get_WatchOfflineVideos_ById(string id)
        {
            try
            {
                using var connection = OpenConnection();
                switch (connection)
                {
                    case null:
                        return null!;
                }
                var video = connection.Table<DataTables.WatchOfflineVideosTb>().FirstOrDefault(a => a.Id == id);
                if (video != null)
                {
                    MoviesDataObject watchOffline = new MoviesDataObject
                    {
                        Id = video.Id,
                        Name = video.Name,
                        Cover = video.Cover,
                        Description = video.Description,
                        Country = video.Country,
                        Duration = video.Duration,
                        Genre = video.Genre,
                        Iframe = video.Iframe,
                        Quality = video.Quality,
                        Producer = video.Producer,
                        Release = video.Release,
                        Source = video.Source,
                        Stars = video.Stars,
                        Url = video.Url,
                        Video = video.Video,
                        Views = video.Views,
                    };

                    return watchOffline;
                }
                else
                {
                    return null!;
                }
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    return Get_WatchOfflineVideos_ById(id);
                else
                {
                    Methods.DisplayReportResultTrack(e);
                    return null!;
                } 
            }
        }

        public DataTables.WatchOfflineVideosTb Update_WatchOfflineVideos(string videoId, string videoPath)
        {
            try
            {
                using var connection = OpenConnection();
                switch (connection)
                {
                    case null:
                        return null!;
                }
                var select = connection.Table<DataTables.WatchOfflineVideosTb>().FirstOrDefault(a => a.Id == videoId);
                if (select != null)
                {
                    select.VideoName = videoId + ".mp4";
                    select.VideoSavedPath = videoPath;

                    connection.Update(select);

                    return select;
                }
                else
                {
                    return null!;
                }
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    return Update_WatchOfflineVideos(videoId, videoPath);
                else
                {
                    Methods.DisplayReportResultTrack(e);
                    return null!;
                } 
            }
        }

        #endregion

        #region Gifts

        //Insert data Gifts
        public void InsertAllGifts(ObservableCollection<GiftObject.DataGiftObject> listData)
        {
            try
            {
                using var connection = OpenConnection();
                switch (connection)
                {
                    case null:
                        return;
                }
                var result = connection.Table<DataTables.GiftsTb>().ToList();

                List<DataTables.GiftsTb> list = new List<DataTables.GiftsTb>();
                foreach (var info in listData)
                {
                    var gift = new DataTables.GiftsTb
                    {
                        Id = info.Id,
                        MediaFile = info.MediaFile,
                        Name = info.Name,
                        Time = info.Time,
                        TimeText = info.TimeText,
                    };

                    list.Add(gift);

                    var update = result.FirstOrDefault(a => a.Id == info.Id);
                    if (update != null)
                    {
                        update = ClassMapper.Mapper?.Map<DataTables.GiftsTb>(info); 
                        connection.Update(update);
                    }
                }
                     
                switch (list.Count)
                {
                    case <= 0:
                        return;
                }
                connection.BeginTransaction();

                //Bring new  
                var newItemList = list.Where(c => !result.Select(fc => fc.Id).Contains(c.Id)).ToList();
                switch (newItemList.Count)
                {
                    case > 0:
                        connection.InsertAll(newItemList);
                        break;
                }
                     
                connection.Commit();
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    InsertAllGifts(listData);
                else
                    Methods.DisplayReportResultTrack(e);
            }
        }

        //Get List Gifts 
        public ObservableCollection<GiftObject.DataGiftObject> GetGiftsList()
        {
            try
            {
                using var connection = OpenConnection();
                switch (connection)
                {
                    case null:
                        return new ObservableCollection<GiftObject.DataGiftObject>();
                }
                var result = connection.Table<DataTables.GiftsTb>().ToList();
                switch (result?.Count)
                {
                    case > 0:
                    {
                        List<GiftObject.DataGiftObject> list = result.Select(gift => new GiftObject.DataGiftObject
                        {
                            Id = gift.Id,
                            MediaFile = gift.MediaFile,
                            Name = gift.Name,
                            Time = gift.Time,
                            TimeText = gift.TimeText,
                        }).ToList();

                        return new ObservableCollection<GiftObject.DataGiftObject>(list);
                    }
                    default:
                        return new ObservableCollection<GiftObject.DataGiftObject>();
                }
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    return GetGiftsList();
                else
                {
                    Methods.DisplayReportResultTrack(e);
                    return new ObservableCollection<GiftObject.DataGiftObject>();
                } 
            }
        }

        #endregion

        #region Post

        //Insert Or Update data Post
        public void InsertOrUpdatePost(string db)
        {
            try
            {
                using var connection = OpenConnection();
                switch (connection)
                {
                    case null:
                        return;
                }
                var dataUser = connection.Table<DataTables.PostsTb>().FirstOrDefault();
                if (dataUser != null)
                {
                    dataUser.DataPostJson = db;  
                    connection.Update(dataUser);
                }
                else
                {
                    DataTables.PostsTb postsTb = new DataTables.PostsTb
                    {
                        DataPostJson = db
                    };

                    connection.Insert(postsTb);
                }
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    InsertOrUpdatePost(db);
                else
                    Methods.DisplayReportResultTrack(e);
            }
        }

        //Get data Post
        public string GetDataPost()
        {
            try
            {
                using var connection = OpenConnection();
                var dataPost = connection?.Table<DataTables.PostsTb>().FirstOrDefault();
                return dataPost?.DataPostJson ?? "";
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    return GetDataPost();
                else
                {
                    Methods.DisplayReportResultTrack(e);
                    return null!;
                } 
            }
        }
         
        #endregion

        #region Story

        //Insert Or Update data Story
        public void InsertOrUpdateStory(string db)
        {
            try
            {
                using var connection = OpenConnection();
                switch (connection)
                {
                    case null:
                        return;
                }
                var dataUser = connection.Table<DataTables.StoryTb>().FirstOrDefault();
                if (dataUser != null)
                {
                    dataUser.DataStoryJson = db;  
                    connection.Update(dataUser);
                }
                else
                {
                    DataTables.StoryTb storyTb = new DataTables.StoryTb
                    {
                        DataStoryJson = db
                    };

                    connection.Insert(storyTb);
                }
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    InsertOrUpdateStory(db);
                else
                    Methods.DisplayReportResultTrack(e);
            }
        }

        //Get data Story
        public string GetDataStory()
        {
            try
            {
                using var connection = OpenConnection();
                var dataPost = connection?.Table<DataTables.StoryTb>().FirstOrDefault();
                return dataPost?.DataStoryJson ?? "";
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    return GetDataStory();
                else
                {
                    Methods.DisplayReportResultTrack(e);
                    return null!;
                } 
            }
        }

        public void DeleteStory()
        {
            try
            {
                using var connection = OpenConnection();
                switch (connection)
                {
                    case null:
                        return;
                    default:
                        connection.DeleteAll<DataTables.StoryTb>();
                        break;
                }
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    DeleteStory();
                else
                    Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion
         
        #region Stickers

        //Insert data To Stickers Table
        public void Insert_To_StickersTb()
        {
            try
            {
                using var connection = OpenConnection();
                var data = connection.Table<DataTables.StickersTb>().ToList()?.Count;
                if (data == 0)
                {
                    var stickersList = new ObservableCollection<DataTables.StickersTb>();
                    DataTables.StickersTb s1 = new DataTables.StickersTb
                    {
                        PackageId = "1",
                        Name = "Rappit",
                        Visibility = true,
                        Count = StickersModel.Locally.StickerList1.Count.ToString()
                    };
                    stickersList.Add(s1);

                    DataTables.StickersTb s2 = new DataTables.StickersTb
                    {
                        PackageId = "2",
                        Name = "Water Drop",
                        Visibility = true,
                        Count = StickersModel.Locally.StickerList2.Count.ToString()
                    };
                    stickersList.Add(s2);

                    DataTables.StickersTb s3 = new DataTables.StickersTb
                    {
                        PackageId = "3",
                        Name = "Monster",
                        Visibility = true,
                        Count = StickersModel.Locally.StickerList3.Count.ToString()
                    };
                    stickersList.Add(s3);

                    DataTables.StickersTb s4 = new DataTables.StickersTb
                    {
                        PackageId = "4",
                        Name = "NINJA Nyankko",
                        Visibility = true,
                        Count = StickersModel.Locally.StickerList4.Count.ToString()
                    };
                    stickersList.Add(s4);

                    DataTables.StickersTb s5 = new DataTables.StickersTb
                    {
                        PackageId = "5",
                        Name = "So Much Love",
                        Visibility = false,
                        Count = StickersModel.Locally.StickerList5.Count.ToString()
                    };
                    stickersList.Add(s5);

                    DataTables.StickersTb s6 = new DataTables.StickersTb
                    {
                        PackageId = "6",
                        Name = "Sukkara chan",
                        Visibility = false,
                        Count = StickersModel.Locally.StickerList6.Count.ToString()
                    };
                    stickersList.Add(s6);

                    DataTables.StickersTb s7 = new DataTables.StickersTb
                    {
                        PackageId = "7",
                        Name = "Flower Hijab",
                        Visibility = false,
                        Count = StickersModel.Locally.StickerList7.Count.ToString()
                    };
                    stickersList.Add(s7);

                    DataTables.StickersTb s8 = new DataTables.StickersTb
                    {
                        PackageId = "8",
                        Name = "Trendy boy",
                        Visibility = false,
                        Count = StickersModel.Locally.StickerList8.Count.ToString()
                    };
                    stickersList.Add(s8);

                    DataTables.StickersTb s9 = new DataTables.StickersTb
                    {
                        PackageId = "9",
                        Name = "The stickman",
                        Visibility = false,
                        Count = StickersModel.Locally.StickerList9.Count.ToString()
                    };
                    stickersList.Add(s9);

                    DataTables.StickersTb s10 = new DataTables.StickersTb
                    {
                        PackageId = "10",
                        Name = "Chip Dale Animated",
                        Visibility = false,
                        Count = StickersModel.Locally.StickerList10.Count.ToString()
                    };
                    stickersList.Add(s10);

                    DataTables.StickersTb s11 = new DataTables.StickersTb
                    {
                        PackageId = "11",
                        Name = AppSettings.ApplicationName + " Stickers",
                        Visibility = false,
                        Count = StickersModel.Locally.StickerList11.Count.ToString()
                    };
                    stickersList.Add(s11);

                    connection.InsertAll(stickersList);

                    ListUtils.StickersList = new ObservableCollection<DataTables.StickersTb>(stickersList);
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("database is locked"))
                    Insert_To_StickersTb();
                else
                    Methods.DisplayReportResultTrack(ex);
            }
        }

        //Get  data To Stickers Table
        public ObservableCollection<DataTables.StickersTb> Get_From_StickersTb()
        {
            try
            {
                using var connection = OpenConnection();
                var stickersList = new ObservableCollection<DataTables.StickersTb>();
                var data = connection.Table<DataTables.StickersTb>().ToList();

                foreach (var s in data.Select(item => new DataTables.StickersTb
                {
                    PackageId = item.PackageId,
                    Name = item.Name,
                    Visibility = item.Visibility,
                    Count = item.Count
                }))
                {
                    stickersList.Add(s);
                }

                return stickersList;
            }
            catch (Exception e)
            {
                if (e.Message.Contains("database is locked"))
                    return Get_From_StickersTb();
                else
                {
                    Methods.DisplayReportResultTrack(e);
                    return null!;
                }
            }
        }

        //Update data To Stickers Table
        public void Update_To_StickersTable(string typeName, bool visibility)
        {
            try
            {
                using var connection = OpenConnection();
                var data = connection.Table<DataTables.StickersTb>().FirstOrDefault(a => a.Name == typeName);
                if (data != null)
                {
                    data.Visibility = visibility;
                }
                connection.Update(data);

                var data2 = ListUtils.StickersList.FirstOrDefault(a => a.Name == typeName);
                if (data2 != null)
                {
                    data2.Visibility = visibility;
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("database is locked"))
                    Update_To_StickersTable(typeName, visibility);
                else
                    Methods.DisplayReportResultTrack(ex);
            }
        }

        #endregion

    }
}