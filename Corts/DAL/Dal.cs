﻿using Corts.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MongoDB.Driver;
using MongoDB.Bson;
using System.Configuration;
using System.Security.Authentication;
using System.Security.Cryptography;
using MongoDB.Driver.Linq;
using static Corts.Models.Classes;

namespace Corts.DAL
{
    public class Dal : IDisposable
    {
        //THIS FILE HANDLES ALL DB CALLS
        private bool disposed = false;

        string userName = "";
        string host = "";
        string password = "";



        private string dbName = "CortsDB";
        private string collectionName = "Users";





        // Default constructor.        
        public Dal()
        {
        }

        #region Generic Functions used in many pages
        //Calculates the car health based on the number of services needed
        //public string GetCarHealth(string car, string email, List<PersonalMaintenance> CarPM)
        //{
        //    List<UsersCars> usersCars = getCurrentUsersCars(email);

        //    string health = "0%";
        //    int healthPercent = 0;

        //    //Airbag 

        //    //for (int i = 0; i < CarPM.Count; i++)
        //    //{
        //    //    if (CarPM[i] != null)
        //    //    {
        //    //        healthPercent++;
        //    //    }
        //    //    else
        //    //    {
        //    //        if (healthPercent != 0)
        //    //        {
        //    //            healthPercent--;
        //    //        }
        //    //    }
        //    //}

        //    //if (healthPercent <= 7)
        //    //{
        //    //    health = "25%";
        //    //}
        //    //else if (healthPercent > 7 && healthPercent < 12)
        //    //{
        //    //    health = "50%";
        //    //}
        //    //else if (healthPercent > 11 && healthPercent < 15)
        //    //{
        //    //    health = "75%";
        //    //}
        //    //else
        //    //{
        //    //    health = "100%";
        //    //}

        //    //return health;
        //}
        //Gets the cars selected nickname for display at the top of the table in the maintenance page
        public string GetCarNickname(string car, string email)
        {
            List<UsersCars> usersCars = getCurrentUsersCars(email);
            string CarNickname = null;
            if (usersCars != null)
            {
                for (int i = 0; i < usersCars.Count; i++)
                {
                    if (usersCars[i].CarID == car)
                    {
                        CarNickname = usersCars[i].CarNickname;
                        return CarNickname;
                    }
                    else
                    {
                        CarNickname = "Car Not Found";
                    }
                }
            }
            return CarNickname;

        }
        //Get cars mileage
        public int GetCarMileage(string car, string email)
        {
            List<UsersCars> usersCars = getCurrentUsersCars(email);
            int CarMileage = 0;
            if (usersCars != null)
            {
                for (int i = 0; i < usersCars.Count; i++)
                {
                    if (usersCars[i].CarID == car)
                    {
                        CarMileage = usersCars[i].mileage;
                        return CarMileage;
                    }
                    else
                    {
                        CarMileage = 0;
                    }
                }
            }
            return CarMileage;
        }
        //Get Months Owned
        public int GetMonthsOwned(string car, string email)
        {
            List<UsersCars> usersCars = getCurrentUsersCars(email);
            int MonthsOwned = 0;
            if (usersCars != null)
            {
                for (int i = 0; i < usersCars.Count; i++)
                {
                    if (usersCars[i].CarID == car)
                    {
                        MonthsOwned = usersCars[i].monthsOwned;
                        return MonthsOwned;
                    }
                    else
                    {
                        MonthsOwned = 0;
                    }
                }
            }
            return MonthsOwned;
        }
        public string GetCarsInspectionDate(string carID, string usersEmail)
        {
            List<UsersCars> usersCars = getCurrentUsersCars(usersEmail);
            string InspectionDate = null;
            if (usersCars != null)
            {
                for (int i = 0; i < usersCars.Count; i++)
                {
                    if (usersCars[i].CarID == carID)
                    {
                        InspectionDate = usersCars[i].InspectionDue.ToString();
                        return InspectionDate;
                    }
                }
            }
            throw new Exception("Car not found");

        }
        public int GetTotalSpent(string carId, string usersEmail)
        {
            List<UsersCars> usersCars = getCurrentUsersCars(usersEmail);
            int totalSpent = 0;
            if (usersCars != null)
            {
                for (int i = 0; i < usersCars.Count; i++)
                {
                    if (usersCars[i].CarID == carId)
                    {
                        totalSpent = usersCars[i].totalSpent;
                        return totalSpent;
                    }
                }
            }
            throw new Exception("Car not found");
        }
        //Get List of all Maintenance Items Names in DB for the Update Maintenance Form
        public List<MaintenanceObject> GetMaintenanceItems()
        {
            var collection = GetMaintenanceScheduleCollection();
            var coll = collection.AsQueryable();
            List<MaintenanceObject> MaintenanceItems = new List<MaintenanceObject>();
            foreach (var item in coll)
            {
                MaintenanceItems.Add(item);
            }
            return MaintenanceItems;

        }
        //Get current list of available cars in database
        public List<Cars> getCurrentCarList()
        {
            var collection = GetCarsCollection();
            var CollectionIDCars = "Cars";
            var builder = Builders<Cars>.Filter;
            var filt = builder.Where(x => x.CollectionID == CollectionIDCars);
            var list = collection.Find(filt).ToList();

            if (list.Count == 0)
            {
                return null;
            }
            else
            {
                List<Cars> carList = list;
                return carList;
            }
        }
        //Get list of current cars user has for settings and maintenance page
        public List<UsersCars> getCurrentUsersCars(string email)
        {
            var collection = GetUsersCollection();
            string usersemail = email;


            var builder = Builders<Users>.Filter;
            var filt = builder.Where(x => x.email == usersemail);
            var list = collection.Find(filt).ToList();

            if (list.Count == 0)
            {
                return null;
            }
            else
            {
                List<UsersCars> usersCars = list[0].Cars;
                return usersCars;
            }
        }
        #endregion

        #region UsersPersonalMaintenaceRetrival (NOT UPDATE)
        //This is the car personal maintenance information -> Same function for each, just pulls a different maintenance item and its corresponding maintenance
        //Get list of users personal maintenance items
        public List<PersonalMaintenance> GetUsersPersonalMaintenance(string email, string CarID)
        {

            List<UsersCars> usersCars = getCurrentUsersCars(email);
            List<PersonalMaintenance> personalMaintenance = new List<PersonalMaintenance>();

            for (int i = 0; i < usersCars.Count; i++)
            {
                if (usersCars[i].CarID == CarID)
                {
                    personalMaintenance = usersCars[i].PersonalMaintenance.ToList();
                    return personalMaintenance;
                }
                else
                {
                    personalMaintenance = null;
                }

            }
            return personalMaintenance;
        }

        public List<PersonalMaintenanceObject> GetAirFilterInformation(List<PersonalMaintenance> CarPM)
        {
            List<PersonalMaintenanceObject> aFilter = new List<PersonalMaintenanceObject>();
            for (int i = 0; i < CarPM.Count; i++)
            {
                aFilter = CarPM[i].AirFilter.ToList();
            }
            return aFilter;
        }
        public List<PersonalMaintenanceObject> GetOilChangeInformation(List<PersonalMaintenance> CarPM)
        {
            List<PersonalMaintenanceObject> oilChange = new List<PersonalMaintenanceObject>();
            for (int i = 0; i < CarPM.Count; i++)
            {
                if (CarPM[i].OilChange != null)
                {
                    oilChange = CarPM[i].OilChange.ToList();
                }
                else
                {
                    oilChange = null;
                }
            }
            return oilChange;
        }
        public List<PersonalMaintenanceObject> GetCoolantInformation(List<PersonalMaintenance> CarPM)
        {
            List<PersonalMaintenanceObject> oilChange = new List<PersonalMaintenanceObject>();
            for (int i = 0; i < CarPM.Count; i++)
            {
                if (CarPM[i].Coolant != null)
                {
                    oilChange = CarPM[i].Coolant.ToList();
                }
                else
                {
                    oilChange = null;
                }
            }
            return oilChange;
        }
        public List<PersonalMaintenanceObject> GetTransFluidInfo(List<PersonalMaintenance> CarPM)
        {
            List<PersonalMaintenanceObject> oilChange = new List<PersonalMaintenanceObject>();
            for (int i = 0; i < CarPM.Count; i++)
            {
                if (CarPM[i].TransFluid != null)
                {
                    oilChange = CarPM[i].TransFluid.ToList();
                }
                else
                {
                    oilChange = null;
                }
            }
            return oilChange;
        }
        public List<PersonalMaintenanceObject> GetFuelFilterInfo(List<PersonalMaintenance> CarPM)
        {
            List<PersonalMaintenanceObject> oilChange = new List<PersonalMaintenanceObject>();
            for (int i = 0; i < CarPM.Count; i++)
            {
                if (CarPM[i].FuelFilter != null)
                {
                    oilChange = CarPM[i].FuelFilter.ToList();
                }
                else
                {
                    oilChange = null;
                }
            }
            return oilChange;
        }
        public List<PersonalMaintenanceObject> GetBatteryInfo(List<PersonalMaintenance> CarPM)
        {
            List<PersonalMaintenanceObject> oilChange = new List<PersonalMaintenanceObject>();
            for (int i = 0; i < CarPM.Count; i++)
            {
                if (CarPM[i].Battery != null)
                {
                    oilChange = CarPM[i].Battery.ToList();
                }
                else
                {
                    oilChange = null;
                }
            }
            return oilChange;
        }
        public List<PersonalMaintenanceObject> GetHVACInfo(List<PersonalMaintenance> CarPM)
        {
            List<PersonalMaintenanceObject> oilChange = new List<PersonalMaintenanceObject>();
            for (int i = 0; i < CarPM.Count; i++)
            {
                if (CarPM[i].HVAC != null)
                {
                    oilChange = CarPM[i].HVAC.ToList();
                }
                else
                {
                    oilChange = null;
                }
            }
            return oilChange;
        }
        public List<PersonalMaintenanceObject> GetBrakesInfo(List<PersonalMaintenance> CarPM)
        {
            List<PersonalMaintenanceObject> oilChange = new List<PersonalMaintenanceObject>();
            for (int i = 0; i < CarPM.Count; i++)
            {
                if (CarPM[i].Brakes != null)
                {
                    oilChange = CarPM[i].Brakes.ToList();
                }
                else
                {
                    oilChange = null;
                }
            }
            return oilChange;
        }
        public List<PersonalMaintenanceObject> GetRadiatorHosesInfo(List<PersonalMaintenance> CarPM)
        {
            List<PersonalMaintenanceObject> oilChange = new List<PersonalMaintenanceObject>();
            for (int i = 0; i < CarPM.Count; i++)
            {
                if (CarPM[i].RadiatorHoses != null)
                {
                    oilChange = CarPM[i].RadiatorHoses.ToList();
                }
                else
                {
                    oilChange = null;
                }
            }
            return oilChange;
        }
        public List<PersonalMaintenanceObject> GetSuspensionInfo(List<PersonalMaintenance> CarPM)
        {
            List<PersonalMaintenanceObject> oilChange = new List<PersonalMaintenanceObject>();
            for (int i = 0; i < CarPM.Count; i++)
            {
                if (CarPM[i].Suspension != null)
                {
                    oilChange = CarPM[i].Suspension.ToList();
                }
                else
                {
                    oilChange = null;
                }
            }
            return oilChange;
        }
        public List<PersonalMaintenanceObject> GetSparkPlugs(List<PersonalMaintenance> CarPM)
        {
            List<PersonalMaintenanceObject> oilChange = new List<PersonalMaintenanceObject>();
            for (int i = 0; i < CarPM.Count; i++)
            {
                if (CarPM[i].SparkPlugs != null)
                {
                    oilChange = CarPM[i].SparkPlugs.ToList();
                }
                else
                {
                    oilChange = null;
                }
            }
            return oilChange;
        }
        public List<PersonalMaintenanceObject> GetIgnitionSystemInfo(List<PersonalMaintenance> CarPM)
        {
            List<PersonalMaintenanceObject> oilChange = new List<PersonalMaintenanceObject>();
            for (int i = 0; i < CarPM.Count; i++)
            {
                if (CarPM[i].IgnitionSystem != null)
                {
                    oilChange = CarPM[i].IgnitionSystem.ToList();
                }
                else
                {
                    oilChange = null;
                }
            }
            return oilChange;
        }
        public List<PersonalMaintenanceObject> GetEngineDBInfo(List<PersonalMaintenance> CarPM)
        {
            List<PersonalMaintenanceObject> oilChange = new List<PersonalMaintenanceObject>();
            for (int i = 0; i < CarPM.Count; i++)
            {
                if (CarPM[i].EngineDriveBelts != null)
                {
                    oilChange = CarPM[i].EngineDriveBelts.ToList();
                }
                else
                {
                    oilChange = null;
                }
            }
            return oilChange;
        }
        public List<PersonalMaintenanceObject> GetTiresInfo(List<PersonalMaintenance> CarPM)
        {
            List<PersonalMaintenanceObject> oilChange = new List<PersonalMaintenanceObject>();
            for (int i = 0; i < CarPM.Count; i++)
            {
                if (CarPM[i].Tires != null)
                {
                    oilChange = CarPM[i].Tires.ToList();
                }
                else
                {
                    oilChange = null;
                }
            }
            return oilChange;
        }
        public List<PersonalMaintenanceObject> GetPowerSteering(List<PersonalMaintenance> CarPM)
        {
            List<PersonalMaintenanceObject> pSteering = new List<PersonalMaintenanceObject>();
            for (int i = 0; i < CarPM.Count; i++)
            {
                if (CarPM[i].PowerSteering != null)
                {
                    pSteering = CarPM[i].PowerSteering.ToList();
                }
                else
                {
                    pSteering = null;
                }
            }
            return pSteering;
        }
        //End of users personal maintenance retrieval
        #endregion

        #region UpdatePersonalMaintenance
        public bool UpdateAirFilterInformation(string usersEmail, string carID, List<PersonalMaintenanceObject> pmObject)
        {
            var collection = GetUsersCollectionForEdit();
            //Create a filter object
            var builder = Builders<Users>.Filter;
            //Filter to the correct user (found by usersEmail)
            var filt = builder.Where(x => x.email == usersEmail);
            //Convert user selected to list object
            var list = collection.Find(filt).ToList();
            for (int i = 0; i < list[0].Cars.Count; i++)
            {
                if (list[0].Cars[i].CarID == carID)
                {
                    try
                    {
                        var filter = Builders<Users>.Filter;
                        var userEmailandCarID = filter.And(
                          filter.Eq(x => x.email, usersEmail),
                          filter.ElemMatch(x => x.Cars, c => c.CarID == carID));

                        var car = collection.Find(userEmailandCarID).ToList();

                        //Unset current air filter doc
                        var update1 = Builders<Users>.Update.Unset(e => e.Cars[i].PersonalMaintenance[i].AirFilter);
                        collection.UpdateOne(userEmailandCarID, update1, new UpdateOptions { IsUpsert = true });

                        // update with positional operator
                        var update = Builders<Users>.Update;
                        var pmSetter = update.Set(e => e.Cars[i].PersonalMaintenance[0].AirFilter, pmObject);
                        collection.FindOneAndUpdate(userEmailandCarID, pmSetter);
                        return true;

                    }
                    catch
                    {
                        throw new Exception("Something went wrong");
                    }

                }
            }
            return false;
        }
        public bool UpdateOilChange(string usersEmail, string carID, List<PersonalMaintenanceObject> pmObject)
        {
            var collection = GetUsersCollectionForEdit();
            //Create a filter object
            var builder = Builders<Users>.Filter;
            //Filter to the correct user (found by usersEmail)
            var filt = builder.Where(x => x.email == usersEmail);
            //Convert user selected to list object
            var list = collection.Find(filt).ToList();
            for (int i = 0; i < list[0].Cars.Count; i++)
            {
                if (list[0].Cars[i].CarID == carID)
                {
                    try
                    {
                        var filter = Builders<Users>.Filter;
                        var userEmailandCarID = filter.And(
                          filter.Eq(x => x.email, usersEmail),
                          filter.ElemMatch(x => x.Cars, c => c.CarID == carID));

                        var car = collection.Find(userEmailandCarID).ToList();

                        // update with positional operator
                        var update = Builders<Users>.Update;
                        var pmSetter = update.Set(e => e.Cars[i].PersonalMaintenance[0].OilChange, pmObject);
                        collection.UpdateOne(userEmailandCarID, pmSetter);
                        return true;

                    }
                    catch
                    {
                        throw new Exception("Something went wrong");
                    }

                }
            }
            return false;
        }
        public bool UpdateIgnitionSystem(string usersEmail, string carID, List<PersonalMaintenanceObject> pmObject)
        {
            var collection = GetUsersCollectionForEdit();
            //Create a filter object
            var builder = Builders<Users>.Filter;
            //Filter to the correct user (found by usersEmail)
            var filt = builder.Where(x => x.email == usersEmail);
            //Convert user selected to list object
            var list = collection.Find(filt).ToList();
            for (int i = 0; i < list[0].Cars.Count; i++)
            {
                if (list[0].Cars[i].CarID == carID)
                {
                    try
                    {
                        var filter = Builders<Users>.Filter;
                        var userEmailandCarID = filter.And(
                          filter.Eq(x => x.email, usersEmail),
                          filter.ElemMatch(x => x.Cars, c => c.CarID == carID));

                        var car = collection.Find(userEmailandCarID).ToList();

                        // update with positional operator
                        var update = Builders<Users>.Update;
                        var pmSetter = update.Set(e => e.Cars[i].PersonalMaintenance[0].IgnitionSystem, pmObject);
                        collection.UpdateOne(userEmailandCarID, pmSetter);
                        return true;

                    }
                    catch
                    {
                        throw new Exception("Something went wrong");
                    }

                }
            }
            return false;
        }
        public bool UpdateBrakes(string usersEmail, string carID, List<PersonalMaintenanceObject> pmObject)
        {
            var collection = GetUsersCollectionForEdit();
            //Create a filter object
            var builder = Builders<Users>.Filter;
            //Filter to the correct user (found by usersEmail)
            var filt = builder.Where(x => x.email == usersEmail);
            //Convert user selected to list object
            var list = collection.Find(filt).ToList();
            for (int i = 0; i < list[0].Cars.Count; i++)
            {
                if (list[0].Cars[i].CarID == carID)
                {
                    try
                    {
                        var filter = Builders<Users>.Filter;
                        var userEmailandCarID = filter.And(
                          filter.Eq(x => x.email, usersEmail),
                          filter.ElemMatch(x => x.Cars, c => c.CarID == carID));

                        var car = collection.Find(userEmailandCarID).ToList();

                        // update with positional operator
                        var update = Builders<Users>.Update;
                        var pmSetter = update.Set(e => e.Cars[i].PersonalMaintenance[0].Brakes, pmObject);
                        collection.UpdateOne(userEmailandCarID, pmSetter);
                        return true;

                    }
                    catch
                    {
                        throw new Exception("Something went wrong");
                    }

                }
            }
            return false;
        }
        public bool UpdateBattery(string usersEmail, string carID, List<PersonalMaintenanceObject> pmObject)
        {
            var collection = GetUsersCollectionForEdit();
            //Create a filter object
            var builder = Builders<Users>.Filter;
            //Filter to the correct user (found by usersEmail)
            var filt = builder.Where(x => x.email == usersEmail);
            //Convert user selected to list object
            var list = collection.Find(filt).ToList();
            for (int i = 0; i < list[0].Cars.Count; i++)
            {
                if (list[0].Cars[i].CarID == carID)
                {
                    try
                    {
                        var filter = Builders<Users>.Filter;
                        var userEmailandCarID = filter.And(
                          filter.Eq(x => x.email, usersEmail),
                          filter.ElemMatch(x => x.Cars, c => c.CarID == carID));

                        var car = collection.Find(userEmailandCarID).ToList();

                        // update with positional operator
                        var update = Builders<Users>.Update;
                        var pmSetter = update.Set(e => e.Cars[i].PersonalMaintenance[0].Battery, pmObject);
                        collection.UpdateOne(userEmailandCarID, pmSetter);
                        return true;

                    }
                    catch
                    {
                        throw new Exception("Something went wrong");
                    }

                }
            }
            return false;
        }
        public bool UpdateCoolant(string usersEmail, string carID, List<PersonalMaintenanceObject> pmObject)
        {
            var collection = GetUsersCollectionForEdit();
            //Create a filter object
            var builder = Builders<Users>.Filter;
            //Filter to the correct user (found by usersEmail)
            var filt = builder.Where(x => x.email == usersEmail);
            //Convert user selected to list object
            var list = collection.Find(filt).ToList();
            for (int i = 0; i < list[0].Cars.Count; i++)
            {
                if (list[0].Cars[i].CarID == carID)
                {
                    try
                    {
                        var filter = Builders<Users>.Filter;
                        var userEmailandCarID = filter.And(
                          filter.Eq(x => x.email, usersEmail),
                          filter.ElemMatch(x => x.Cars, c => c.CarID == carID));

                        var car = collection.Find(userEmailandCarID).ToList();

                        // update with positional operator
                        var update = Builders<Users>.Update;
                        var pmSetter = update.Set(e => e.Cars[i].PersonalMaintenance[0].Coolant, pmObject);
                        collection.UpdateOne(userEmailandCarID, pmSetter);
                        return true;

                    }
                    catch
                    {
                        throw new Exception("Something went wrong");
                    }

                }
            }
            return false;
        }
        public bool UpdateHVAC(string usersEmail, string carID, List<PersonalMaintenanceObject> pmObject)
        {
            var collection = GetUsersCollectionForEdit();
            //Create a filter object
            var builder = Builders<Users>.Filter;
            //Filter to the correct user (found by usersEmail)
            var filt = builder.Where(x => x.email == usersEmail);
            //Convert user selected to list object
            var list = collection.Find(filt).ToList();
            for (int i = 0; i < list[0].Cars.Count; i++)
            {
                if (list[0].Cars[i].CarID == carID)
                {
                    try
                    {
                        var filter = Builders<Users>.Filter;
                        var userEmailandCarID = filter.And(
                          filter.Eq(x => x.email, usersEmail),
                          filter.ElemMatch(x => x.Cars, c => c.CarID == carID));

                        var car = collection.Find(userEmailandCarID).ToList();

                        // update with positional operator
                        var update = Builders<Users>.Update;
                        var pmSetter = update.Set(e => e.Cars[i].PersonalMaintenance[0].HVAC, pmObject);
                        collection.UpdateOne(userEmailandCarID, pmSetter);
                        return true;

                    }
                    catch
                    {
                        throw new Exception("Something went wrong");
                    }

                }
            }
            return false;
        }
        public bool UpdatePowerSteering(string usersEmail, string carID, List<PersonalMaintenanceObject> pmObject)
        {
            var collection = GetUsersCollectionForEdit();
            //Create a filter object
            var builder = Builders<Users>.Filter;
            //Filter to the correct user (found by usersEmail)
            var filt = builder.Where(x => x.email == usersEmail);
            //Convert user selected to list object
            var list = collection.Find(filt).ToList();
            for (int i = 0; i < list[0].Cars.Count; i++)
            {
                if (list[0].Cars[i].CarID == carID)
                {
                    try
                    {
                        var filter = Builders<Users>.Filter;
                        var userEmailandCarID = filter.And(
                          filter.Eq(x => x.email, usersEmail),
                          filter.ElemMatch(x => x.Cars, c => c.CarID == carID));

                        var car = collection.Find(userEmailandCarID).ToList();

                        // update with positional operator
                        var update = Builders<Users>.Update;
                        var pmSetter = update.Set(e => e.Cars[i].PersonalMaintenance[0].PowerSteering, pmObject);
                        collection.UpdateOne(userEmailandCarID, pmSetter);
                        return true;

                    }
                    catch
                    {
                        throw new Exception("Something went wrong");
                    }

                }
            }
            return false;
        }
        public bool UpdateTransFluid(string usersEmail, string carID, List<PersonalMaintenanceObject> pmObject)
        {
            var collection = GetUsersCollectionForEdit();
            //Create a filter object
            var builder = Builders<Users>.Filter;
            //Filter to the correct user (found by usersEmail)
            var filt = builder.Where(x => x.email == usersEmail);
            //Convert user selected to list object
            var list = collection.Find(filt).ToList();
            for (int i = 0; i < list[0].Cars.Count; i++)
            {
                if (list[0].Cars[i].CarID == carID)
                {
                    try
                    {
                        var filter = Builders<Users>.Filter;
                        var userEmailandCarID = filter.And(
                          filter.Eq(x => x.email, usersEmail),
                          filter.ElemMatch(x => x.Cars, c => c.CarID == carID));

                        var car = collection.Find(userEmailandCarID).ToList();

                        // update with positional operator
                        var update = Builders<Users>.Update;
                        var pmSetter = update.Set(e => e.Cars[i].PersonalMaintenance[0].TransFluid, pmObject);
                        collection.UpdateOne(userEmailandCarID, pmSetter);
                        return true;

                    }
                    catch
                    {
                        throw new Exception("Something went wrong");
                    }

                }
            }
            return false;
        }
        public bool UpdateFuelFilter(string usersEmail, string carID, List<PersonalMaintenanceObject> pmObject)
        {
            var collection = GetUsersCollectionForEdit();
            //Create a filter object
            var builder = Builders<Users>.Filter;
            //Filter to the correct user (found by usersEmail)
            var filt = builder.Where(x => x.email == usersEmail);
            //Convert user selected to list object
            var list = collection.Find(filt).ToList();
            for (int i = 0; i < list[0].Cars.Count; i++)
            {
                if (list[0].Cars[i].CarID == carID)
                {
                    try
                    {
                        var filter = Builders<Users>.Filter;
                        var userEmailandCarID = filter.And(
                          filter.Eq(x => x.email, usersEmail),
                          filter.ElemMatch(x => x.Cars, c => c.CarID == carID));

                        var car = collection.Find(userEmailandCarID).ToList();

                        // update with positional operator
                        var update = Builders<Users>.Update;
                        var pmSetter = update.Set(e => e.Cars[i].PersonalMaintenance[0].FuelFilter, pmObject);
                        collection.UpdateOne(userEmailandCarID, pmSetter);
                        return true;

                    }
                    catch
                    {
                        throw new Exception("Something went wrong");
                    }

                }
            }
            return false;
        }
        public bool UpdateRadiatorHoses(string usersEmail, string carID, List<PersonalMaintenanceObject> pmObject)
        {
            var collection = GetUsersCollectionForEdit();
            //Create a filter object
            var builder = Builders<Users>.Filter;
            //Filter to the correct user (found by usersEmail)
            var filt = builder.Where(x => x.email == usersEmail);
            //Convert user selected to list object
            var list = collection.Find(filt).ToList();
            for (int i = 0; i < list[0].Cars.Count; i++)
            {
                if (list[0].Cars[i].CarID == carID)
                {
                    try
                    {
                        var filter = Builders<Users>.Filter;
                        var userEmailandCarID = filter.And(
                          filter.Eq(x => x.email, usersEmail),
                          filter.ElemMatch(x => x.Cars, c => c.CarID == carID));

                        var car = collection.Find(userEmailandCarID).ToList();

                        // update with positional operator
                        var update = Builders<Users>.Update;
                        var pmSetter = update.Set(e => e.Cars[i].PersonalMaintenance[0].RadiatorHoses, pmObject);
                        collection.UpdateOne(userEmailandCarID, pmSetter);
                        return true;

                    }
                    catch
                    {
                        throw new Exception("Something went wrong");
                    }

                }
            }
            return false;
        }
        public bool UpdateSuspension(string usersEmail, string carID, List<PersonalMaintenanceObject> pmObject)
        {
            var collection = GetUsersCollectionForEdit();
            //Create a filter object
            var builder = Builders<Users>.Filter;
            //Filter to the correct user (found by usersEmail)
            var filt = builder.Where(x => x.email == usersEmail);
            //Convert user selected to list object
            var list = collection.Find(filt).ToList();
            for (int i = 0; i < list[0].Cars.Count; i++)
            {
                if (list[0].Cars[i].CarID == carID)
                {
                    try
                    {
                        var filter = Builders<Users>.Filter;
                        var userEmailandCarID = filter.And(
                          filter.Eq(x => x.email, usersEmail),
                          filter.ElemMatch(x => x.Cars, c => c.CarID == carID));

                        var car = collection.Find(userEmailandCarID).ToList();

                        // update with positional operator
                        var update = Builders<Users>.Update;
                        var pmSetter = update.Set(e => e.Cars[i].PersonalMaintenance[0].Suspension, pmObject);
                        collection.UpdateOne(userEmailandCarID, pmSetter);
                        return true;

                    }
                    catch
                    {
                        throw new Exception("Something went wrong");
                    }

                }
            }
            return false;
        }
        public bool UpdateSparkPlugs(string usersEmail, string carID, List<PersonalMaintenanceObject> pmObject)
        {
            var collection = GetUsersCollectionForEdit();
            //Create a filter object
            var builder = Builders<Users>.Filter;
            //Filter to the correct user (found by usersEmail)
            var filt = builder.Where(x => x.email == usersEmail);
            //Convert user selected to list object
            var list = collection.Find(filt).ToList();
            for (int i = 0; i < list[0].Cars.Count; i++)
            {
                if (list[0].Cars[i].CarID == carID)
                {
                    try
                    {
                        var filter = Builders<Users>.Filter;
                        var userEmailandCarID = filter.And(
                          filter.Eq(x => x.email, usersEmail),
                          filter.ElemMatch(x => x.Cars, c => c.CarID == carID));

                        var car = collection.Find(userEmailandCarID).ToList();

                        // update with positional operator
                        var update = Builders<Users>.Update;
                        var pmSetter = update.Set(e => e.Cars[i].PersonalMaintenance[0].SparkPlugs, pmObject);
                        collection.UpdateOne(userEmailandCarID, pmSetter);
                        return true;

                    }
                    catch
                    {
                        throw new Exception("Something went wrong");
                    }

                }
            }
            return false;
        }
        public bool UpdateEngineDriveBelts(string usersEmail, string carID, List<PersonalMaintenanceObject> pmObject)
        {
            var collection = GetUsersCollectionForEdit();
            //Create a filter object
            var builder = Builders<Users>.Filter;
            //Filter to the correct user (found by usersEmail)
            var filt = builder.Where(x => x.email == usersEmail);
            //Convert user selected to list object
            var list = collection.Find(filt).ToList();
            for (int i = 0; i < list[0].Cars.Count; i++)
            {
                if (list[0].Cars[i].CarID == carID)
                {
                    try
                    {
                        var filter = Builders<Users>.Filter;
                        var userEmailandCarID = filter.And(
                          filter.Eq(x => x.email, usersEmail),
                          filter.ElemMatch(x => x.Cars, c => c.CarID == carID));

                        var car = collection.Find(userEmailandCarID).ToList();

                        // update with positional operator
                        var update = Builders<Users>.Update;
                        var pmSetter = update.Set(e => e.Cars[i].PersonalMaintenance[0].EngineDriveBelts, pmObject);
                        collection.UpdateOne(userEmailandCarID, pmSetter);
                        return true;

                    }
                    catch
                    {
                        throw new Exception("Something went wrong");
                    }

                }
            }
            return false;
        }
        public bool UpdateTires(string usersEmail, string carID, List<PersonalMaintenanceObject> pmObject)
        {
            var collection = GetUsersCollectionForEdit();
            //Create a filter object
            var builder = Builders<Users>.Filter;
            //Filter to the correct user (found by usersEmail)
            var filt = builder.Where(x => x.email == usersEmail);
            //Convert user selected to list object
            var list = collection.Find(filt).ToList();
            for (int i = 0; i < list[0].Cars.Count; i++)
            {
                if (list[0].Cars[i].CarID == carID)
                {
                    try
                    {
                        var filter = Builders<Users>.Filter;
                        var userEmailandCarID = filter.And(
                          filter.Eq(x => x.email, usersEmail),
                          filter.ElemMatch(x => x.Cars, c => c.CarID == carID));

                        var car = collection.Find(userEmailandCarID).ToList();

                        // update with positional operator
                        var update = Builders<Users>.Update;
                        var pmSetter = update.Set(e => e.Cars[i].PersonalMaintenance[0].Tires, pmObject);
                        collection.UpdateOne(userEmailandCarID, pmSetter);
                        return true;

                    }
                    catch
                    {
                        throw new Exception("Something went wrong");
                    }

                }
            }
            return false;
        }
        #endregion

        #region Update Maintenance Information
        public bool UpdateMileage(int mileage, string carID, string usersEmail)
        {
            var collection = GetUsersCollectionForEdit();
            //Create a filter object
            var builder = Builders<Users>.Filter;
            //Filter to the correct user (found by usersEmail)
            var filt = builder.Where(x => x.email == usersEmail);
            //Convert user selected to list object
            var list = collection.Find(filt).ToList();

            for(int i = 0; i < list[0].Cars.Count; i++)
            {
                if(list[0].Cars[i].CarID == carID)
                {
                    try
                    {
                        var filter = Builders<Users>.Filter;
                        var userEmailandCarID = filter.And(
                          filter.Eq(x => x.email, usersEmail),
                          filter.ElemMatch(x => x.Cars, c => c.CarID == carID));
                        
                        var car = collection.Find(userEmailandCarID).ToList();
                        
                        // update with positional operator
                        var update = Builders<Users>.Update;
                        var mileageSetter = update.Set("Cars.$.mileage", mileage);
                        collection.UpdateOne(userEmailandCarID, mileageSetter);
                        return true;

                    }
                    catch
                    {
                        return false;
                    }


                }
            }
            return true;

        }
        public bool UpdateInspection(string carID, string usersEmail)
        {
            var collection = GetUsersCollectionForEdit();
            //Create a filter object
            var builder = Builders<Users>.Filter;
            //Filter to the correct user (found by usersEmail)
            var filt = builder.Where(x => x.email == usersEmail);
            //Convert user selected to list object
            var list = collection.Find(filt).ToList();

            for (int i = 0; i < list[0].Cars.Count; i++)
            {
                if (list[0].Cars[i].CarID == carID)
                {
                    try
                    {
                        //Create date and add a year
                        DateTime theDate = DateTime.Today;
                      
                        DateTime yearInspectionDue = theDate.AddYears(1);
                        var date = yearInspectionDue.ToString("MM-yyyy");
                        var filter = Builders<Users>.Filter;
                        var userEmailandCarID = filter.And(
                          filter.Eq(x => x.email, usersEmail),
                          filter.ElemMatch(x => x.Cars, c => c.CarID == carID));
                        // find car with email and car id
                        var car = collection.Find(userEmailandCarID).ToList();

                        // update with positional operator
                        var update = Builders<Users>.Update;
                        var inspectionSetter = update.Set("Cars.$.InspectionDue", date.ToString());
                        collection.UpdateOne(userEmailandCarID, inspectionSetter);
                        return true;

                    }
                    catch
                    {
                        return false;
                    }


                }
            }
            return true;

        }
        public bool UpdateTotalCost(string carID, string usersEmail, int cost)
        {

            //Get current total cost
            int currentCost = GetUsersCurrentTotalForCar(usersEmail, carID);

            var collection = GetUsersCollectionForEdit();
            //Create a filter object
            var builder = Builders<Users>.Filter;
            //Filter to the correct user (found by usersEmail)
            var filt = builder.Where(x => x.email == usersEmail);
            //Convert user selected to list object
            var list = collection.Find(filt).ToList();

            for (int i = 0; i < list[0].Cars.Count; i++)
            {
                if (list[0].Cars[i].CarID == carID)
                {
                    try
                    {
                        //Create the updated variable
                        int UpdatedCost = currentCost + cost;
                        var filter = Builders<Users>.Filter;
                        var userEmailandCarID = filter.And(
                          filter.Eq(x => x.email, usersEmail),
                          filter.ElemMatch(x => x.Cars, c => c.CarID == carID));
                        // find car with email and car id
                        var car = collection.Find(userEmailandCarID).ToList();

                        // update with positional operator
                        var update = Builders<Users>.Update;
                        var inspectionSetter = update.Set("Cars.$.totalSpent", UpdatedCost);
                        collection.UpdateOne(userEmailandCarID, inspectionSetter);
                        return true;

                    }
                    catch
                    {
                        return false;
                    }
                }
            }
            return true;

        }
        public int GetUsersCurrentTotalForCar(string usersEmail, string CarID)
        {
            int currentCost = 0;
            var collection = GetUsersCollection();
            //Create a filter object
            var builder = Builders<Users>.Filter;
            //Filter to the correct user (found by usersEmail)
            var filt = builder.Where(x => x.email == usersEmail);
            //Convert user selected to list object
            var list = collection.Find(filt).ToList();

            for (int i = 0; i < list[0].Cars.Count; i++)
            {
                if (list[0].Cars[i].CarID == CarID)
                {
                    currentCost = list[0].Cars[i].totalSpent;
                    return currentCost;
                }
                else
                {
                    return currentCost;
                }

            }
            throw new Exception("Car not found");
        }
        #endregion

        #region EmailReminders
        //Get list of users emails for email reminders
        public List<string> GetUsersEmails()
        {
            var collection = GetUsersCollection();
            var coll = collection.AsQueryable();
            List<string> Emails = new List<string>();
            foreach (var item in coll)
            {
                Emails.Add(item.email);
            }
            return Emails;
        }
        public string EmailBody(string userEmail)
        {
            Dal dal = new Dal();
            List<UsersCars> usersCars = new List<UsersCars>();
            string message = null;
            string carNickname = null;
            int carMileage = 0;
            string inspectionDate = null;
            int monthsOwned = 0;
            int totalSpent = 0;

            //Get all emails that we need to send an email to
            //(i.e) GetUsersCollection()
            //Save all emails to a list and then pass as an array
            //Pass as an array into the "EmailGoesHere" 
            usersCars = dal.getCurrentUsersCars(userEmail);
            foreach (var car in usersCars)
            {
                carNickname = GetCarNickname(car.CarID, userEmail);
                string carNicknameString = "Car Nickname: " + carNickname;
                message += carNicknameString;
                carMileage = GetCarMileage(car.CarID, userEmail);
                string carMileageString = "<br /> Car Mileage: " + carMileage;
                message += carMileageString;
                inspectionDate = GetCarsInspectionDate(car.CarID, userEmail);
                string inspectionDateString = "<br /> Inspection Due: " + inspectionDate;
                message += inspectionDateString;
                monthsOwned = GetMonthsOwned(car.CarID, userEmail);
                string monthsOwnedString = "<br /> Months Owned: " + monthsOwned;
                message += monthsOwnedString;
                totalSpent = GetTotalSpent(car.CarID, userEmail);
                string totalSpentString = "<br /> Total Spent: " + totalSpent;
                message += totalSpentString;
            }
            return message;
        }
        #endregion

        #region SettingsPageFunctions

        public List<MaintenanceObject> GetPersonalMaintenanceObjectByName(string name)
        {
            var collection = GetMaintenanceScheduleCollection();
            var maintenanceObjectName = name;
            var builder = Builders<MaintenanceObject>.Filter;
            var filt = builder.Where(x => x.Name == maintenanceObjectName);
            var list = collection.Find(filt).ToList();

            if (list.Count == 0)
            {
                return null;
            }
            else
            {
                return list;
            }

        }
        public List<PersonalMaintenance> InitializePersonalMaintenance(int mileage, int totalSpent)
        {
            List<PersonalMaintenance> pmList = new List<PersonalMaintenance>() {
                new PersonalMaintenance() {
                    AirFilter = new List<PersonalMaintenanceObject>() { new PersonalMaintenanceObject ()
                    {
                         Name = "Air Filter", LastChecked = 0, NxtNeeded = GetMileageNeeded(GetPersonalMaintenanceObjectByName("Air Filter"))}
                    },
                    Battery = new List<PersonalMaintenanceObject>() { new PersonalMaintenanceObject ()
                    {
                         Name = "Battery", LastChecked = 0,NxtNeeded = GetMileageNeeded(GetPersonalMaintenanceObjectByName("Battery"))}
                    },
                    Brakes = new List<PersonalMaintenanceObject>() { new PersonalMaintenanceObject ()
                    {
                         Name = "Brakes", LastChecked = 0,NxtNeeded = GetMileageNeeded(GetPersonalMaintenanceObjectByName("Brakes"))}
                    },
                    Coolant = new List<PersonalMaintenanceObject>() { new PersonalMaintenanceObject ()
                    {
                         Name = "Coolant", LastChecked = 0,NxtNeeded = GetMileageNeeded(GetPersonalMaintenanceObjectByName("Coolant"))}
                    },
                    EngineDriveBelts = new List<PersonalMaintenanceObject>() { new PersonalMaintenanceObject ()
                    {
                         Name = "Engine Drive Belts", LastChecked = 0,NxtNeeded = GetMileageNeeded(GetPersonalMaintenanceObjectByName("Engine Drive Belts"))}
                    },
                    FuelFilter = new List<PersonalMaintenanceObject>() { new PersonalMaintenanceObject ()
                    {
                         Name = "Fuel Filter", LastChecked = 0,NxtNeeded = GetMileageNeeded(GetPersonalMaintenanceObjectByName("Fuel Filter"))}
                    },
                    HVAC = new List<PersonalMaintenanceObject>() { new PersonalMaintenanceObject ()
                    {
                         Name = "HVAC", LastChecked = 0,NxtNeeded = GetMileageNeeded(GetPersonalMaintenanceObjectByName("HVAC"))}
                    },
                    IgnitionSystem = new List<PersonalMaintenanceObject>() { new PersonalMaintenanceObject ()
                    {
                         Name = "Ignition System", LastChecked = 0,NxtNeeded = GetMileageNeeded(GetPersonalMaintenanceObjectByName("Ignition System"))}
                    },
                    OilChange = new List<PersonalMaintenanceObject>() { new PersonalMaintenanceObject ()
                    {
                         Name = "Oil Change", LastChecked = 0, NxtNeeded = GetMileageNeeded(GetPersonalMaintenanceObjectByName("Oil Change"))}
                    },
                    PowerSteering = new List<PersonalMaintenanceObject>() { new PersonalMaintenanceObject ()
                    {
                         Name = "Power Steering", LastChecked = 0, NxtNeeded = GetMileageNeeded(GetPersonalMaintenanceObjectByName("Power Steering"))}
                    },
                    RadiatorHoses = new List<PersonalMaintenanceObject>() { new PersonalMaintenanceObject ()
                    {
                         Name = "Radiator Hoses", LastChecked = 0, NxtNeeded = GetMileageNeeded(GetPersonalMaintenanceObjectByName("Radiator Hoses"))}
                    },
                    SparkPlugs = new List<PersonalMaintenanceObject>() { new PersonalMaintenanceObject ()
                    {
                         Name = "Spark Plugs", LastChecked = 0, NxtNeeded = GetMileageNeeded(GetPersonalMaintenanceObjectByName("Spark Plugs"))}
                    },
                    Suspension = new List<PersonalMaintenanceObject>() { new PersonalMaintenanceObject ()
                    {
                         Name = "Suspension", LastChecked = 0, NxtNeeded = GetMileageNeeded(GetPersonalMaintenanceObjectByName("Suspension"))}
                    },
                    Tires = new List<PersonalMaintenanceObject>() { new PersonalMaintenanceObject ()
                    {
                         Name = "Tires", LastChecked = 0, NxtNeeded = GetMileageNeeded(GetPersonalMaintenanceObjectByName("Tires"))}
                    },
                    TransFluid = new List<PersonalMaintenanceObject>() { new PersonalMaintenanceObject ()
                    {
                         Name = "Transmission Fluid", LastChecked = 0, NxtNeeded = GetMileageNeeded(GetPersonalMaintenanceObjectByName("Transmission Fluid"))}
                    }
            }

            };

            return pmList;

        }
        //Add a car -> Completes Add Form on SettingsPage
        public bool AddCar(string usersEmail, UsersCars newCar)
        {
            //Grab the UsersCollectionForEdit -> Allows us to modify instead of just read
            var collection = GetUsersCollectionForEdit();
            //Create a filter object
            var builder = Builders<Users>.Filter;
            //Filter to the correct user (found by usersEmail)
            var filt = builder.Where(x => x.email == usersEmail);
            //Convert user selected to list object
            var list = collection.Find(filt).ToList();

            //If user has no cars -> We must first delete the "null" list in the DB and then add a new car
            if (list[0].Cars == null)
            {
                try
                {
                    //Delete "null" object in DB for Cars
                    var update = Builders<Users>.Update.Unset(e => e.Cars);
                    collection.UpdateOne(filt, update, new UpdateOptions { IsUpsert = true });

                    //Add new car
                    var updateCar = Builders<Users>.Update.Push(e => e.Cars, newCar);
                    collection.UpdateOne(filt, updateCar);
                }
                catch
                {
                    return false;
                }
            }
            //User already has a car in the database and we can simply just update document to add another
            else
            {
                try
                {
                    //Adds car
                    var update = Builders<Users>.Update.Push(e => e.Cars, newCar);
                    collection.UpdateOne(filt, update);
                }
                catch
                {
                    return false;
                }
            }

            return true;

        }
        //Remove a car
        public bool RemoveCar(string removeID, string usersEmail)
        {
            //Grab collection
            var collection = GetUsersCollectionForEdit();
            //Create builder of type User
            var builder = Builders<Users>.Filter;
            //Filter to user specific document
            var filt = builder.Where(x => x.email == usersEmail);
            //Grab document and convert to list
            var list = collection.Find(filt).ToList();

            try
            {
                //Removes car with specific carID
                var update = Builders<Users>.Update.PullFilter(x => x.Cars, Cars => Cars.CarID == removeID);
                collection.UpdateOne(filt, update);
            }
            catch
            {
                return false;
            }
            return true;


        }
        //Check password before account updates
        public bool CheckPassword(string usersEmail, string passwordInserted)
        {
            //Grab user collection
            var collection = GetUsersCollection();
            //Get user by email
            string email = usersEmail;
            //Get password that was inserted into EditProfile form
            string password = passwordInserted;

            //Create builder of type User
            var builder = Builders<Users>.Filter;
            //Filter to user specific 
            var filt = builder.Where(x => x.email == email);
            //Grab document and all of its fixins
            var list = collection.Find(filt).ToList();

            //If user is not found -> return false
            if (list.Count == 0)
            {
                return false;
            }

            //Unhash password and check to make sure it matches the password that was inserted
            //Get savedPasswordHash from the data base
            string passwordFromDB = list[0].password;

            //Convert the password from the DB to an array of bytes
            byte[] hashBytes = Convert.FromBase64String(passwordFromDB);

            //Get the salt from the hashbytes
            byte[] salt = new byte[16];
            Array.Copy(hashBytes, 0, salt, 0, 16);

            //Use the same variable to hash the password that the user entered
            var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000);

            //Convert the hashed password from the user to an array of bytes
            byte[] hash = pbkdf2.GetBytes(20);

            //Set a flag 'ok' to validate the password that the user entered
            //with the password in the database
            int ok = 1;

            //Loop that checks the validity of the password by comparing each byte
            //of the db password with the user password. The hashBytes starts at 16
            //because the salt value is stored in the first 16 bytes and we aren't
            //comparing that with anything.
            for (int i = 0; i < 20; i++)
            {
                if (hashBytes[i + 16] != hash[i])
                {
                    ok = 0;
                }
            }

            //log the user in
            if (ok == 1)
            {
                return true;
            }

            //Deny the user
            else
            {
                return false;
            }
        }
        //Update Users Email
        public bool UpdateEmail(string usersEmail, string newEmail)
        {
            //Grab the UsersCollectionForEdit -> Allows us to modify instead of just read
            var collection = GetUsersCollectionForEdit();
            //Create a filter object
            var builder = Builders<Users>.Filter;
            //Filter to the correct user (found by usersEmail)
            var filt = builder.Where(x => x.email == usersEmail);
            //Convert user selected to list object
            var list = collection.Find(filt).ToList();

            try
            {
                //Update users email
                var updateEmail = Builders<Users>.Update.Set(e => e.email, newEmail);
                collection.UpdateOne(filt, updateEmail);
            }
            catch
            {
                return false;
            }
            return true;
        }

        public string GetUsername(string email)
        {
            var collection = GetUsersCollection();
            string usersemail = email;


            var builder = Builders<Users>.Filter;
            var filt = builder.Where(x => x.email == usersemail);
            var list = collection.Find(filt).ToList();

            //Can't find user -> return null
            if (list.Count == 0)
            {
                return null;
            }
            else
            {
                string username = list[0].Username;
                return username;
            }
        }
        public bool UpdateUsername(string usersEmail, string username)
        {
            //Grab the UsersCollectionForEdit -> Allows us to modify instead of just read
            var collection = GetUsersCollectionForEdit();
            //Create a filter object
            var builder = Builders<Users>.Filter;
            //Filter to the correct user (found by usersEmail)
            var filt = builder.Where(x => x.email == usersEmail);
            //Convert user selected to list object
            var list = collection.Find(filt).ToList();

            try
            {
                //Update users email
                var updateUsername = Builders<Users>.Update.Set(e => e.Username, username);
                collection.UpdateOne(filt, updateUsername);
            }
            catch
            {
                return false;
            }
            return true;
        }
        //Update Users Password
        public bool UpdatePassword(string usersEmail, string CurrPassword, string newPassword)
        {
            //Grab the UsersCollectionForEdit -> Allows us to modify instead of just read
            var collection = GetUsersCollectionForEdit();
            //Create a filter object
            var builder = Builders<Users>.Filter;
            //Filter to the correct user (found by usersEmail)
            var filt = builder.Where(x => x.email == usersEmail);
            //Convert user selected to list object
            var list = collection.Find(filt).ToList();

            //If user is not found -> return false
            if (list.Count == 0)
            {
                return false;
            }

            //Get savedPasswordHash from the data base
            string passwordFromDB = list[0].password;

            //Convert the password from the DB to an array of bytes
            byte[] hashBytes = Convert.FromBase64String(passwordFromDB);

            //Get the salt from the hashbytes
            byte[] salt = new byte[16];
            Array.Copy(hashBytes, 0, salt, 0, 16);

            //Use the same variable to hash the password that the user entered
            var pbkdf2 = new Rfc2898DeriveBytes(CurrPassword, salt, 10000);

            //Convert the hashed password from the user to an array of bytes
            byte[] hash = pbkdf2.GetBytes(20);

            //Set a flag 'ok' to validate the password that the user entered
            //with the password in the database
            int ok = 1;

            //Loop that checks the validity of the password by comparing each byte
            //of the db password with the user password. The hashBytes starts at 16
            //because the salt value is stored in the first 16 bytes and we aren't
            //comparing that with anything.
            for (int i = 0; i < 20; i++)
            {
                if (hashBytes[i + 16] != hash[i])
                {
                    ok = 0;
                }
            }

            if (ok == 1)
            {
                //Generate a random salt
                new RNGCryptoServiceProvider().GetBytes(salt = new byte[16]);

                //Concatenate salt and user password then hash it using Rfc...Bytes
                var pdkdf2 = new Rfc2898DeriveBytes(newPassword, salt, 10000);

                //Place the concatenated string in the byte array hash2
                byte[] hash2 = pdkdf2.GetBytes(20);

                //New byte array to store hashed password + salt
                byte[] hashBytes2 = new byte[36];

                Array.Copy(salt, 0, hashBytes2, 0, 16);
                Array.Copy(hash2, 0, hashBytes2, 16, 20);

                string savedPasswordHash = Convert.ToBase64String(hashBytes2);

                newPassword = savedPasswordHash;

                try
                {
                    //Update users Password
                    var updatePassword = Builders<Users>.Update.Set(e => e.password, newPassword);
                    collection.UpdateOne(filt, updatePassword);
                }
                catch
                {
                    return false;
                }
                return true;
            }
            else
            {
                return false;
            }
        }
        //Get selected car from add form and return CarType
        public string GetSelectedCar(string CarSelected)
        {
            //Grab the cars collection
            var collection = GetCarsCollection();
            //Create builder of type Cars
            var builder = Builders<Cars>.Filter;
            //Grab the ID of the car that was selected 
            var filt = builder.Where(x => x.id == CarSelected);
            //Grab what was found and set its fixins to a list
            var list = collection.Find(filt).ToList();

            //Create variable called CarType
            string CarType = null;
            //Can't find the car listed? Return null.
            if (list.Count == 0)
            {
                return null;
            }
            else
            {
                //Set CarType = CarType from DB
                CarType = list[0].type;
            }
            return CarType;
        }
        #endregion

        #region Login and Register Functionality
        //Login user
        public bool LoginUser(Users user)
        {
            //Grab users collection
            var collection = GetUsersCollection();

            //Grab email that was inserted into form
            string email = user.email;
            //Grab password that was inserted into form
            string password = user.password;

            //Create builder of type Users
            var builder = Builders<Users>.Filter;
            //Filter to User where email = email that was inserted
            var filt = builder.Where(x => x.email == email);
            //Send what was found to a list
            var list = collection.Find(filt).ToList();

            //If user was not found -> return false: DENY ENTRY
            if (list.Count == 0)
            {
                return false;
            }

            //Unhash password and allow user to login if password is correct, if not correct return false
            //Get savedPasswordHash from the data base
            string passwordFromDB = list[0].password;

            //Convert the password from the DB to an array of bytes
            byte[] hashBytes = Convert.FromBase64String(passwordFromDB);

            //Get the salt from the hashbytes
            byte[] salt = new byte[16];
            Array.Copy(hashBytes, 0, salt, 0, 16);

            //Use the same variable to hash the password that the user entered
            var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000);

            //Convert the hashed password from the user to an array of bytes
            byte[] hash = pbkdf2.GetBytes(20);

            //Set a flag 'ok' to validate the password that the user entered
            //with the password in the database
            int ok = 1;

            //Loop that checks the validity of the password by comparing each byte
            //of the db password with the user password. The hashBytes starts at 16
            //because the salt value is stored in the first 16 bytes and we aren't
            //comparing that with anything.
            for (int i = 0; i < 20; i++)
            {
                if (hashBytes[i + 16] != hash[i])
                {
                    ok = 0;
                }
            }

            //log the user in
            if (ok == 1)
            {
                return true;
            }

            //Deny the user
            else
            {
                return false;
            }
        }

        //Check User Registration Date -> This is in case they registered that day, in which case it will send them to the settings page. 
        public bool getCreatedDate(Users user)
        {
            var collection = GetUsersCollection();
            string email = user.email;
            string password = user.password;

            //Get savedPasswordHash from the data base
            var builder = Builders<Users>.Filter;
            var filt = builder.Where(x => x.email == email);
            var list = collection.Find(filt).ToList();
            string passwordFromDB = list[0].password;

            //Convert the password from the DB to an array of bytes
            byte[] hashBytes = Convert.FromBase64String(passwordFromDB);

            //Get the salt from the hashbytes
            byte[] salt = new byte[16];
            Array.Copy(hashBytes, 0, salt, 0, 16);

            //Use the same variable to hash the password that the user entered
            var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000);

            //Convert the hashed password from the user to an array of bytes
            byte[] hash = pbkdf2.GetBytes(20);

            //Set a flag 'ok' to validate the password that the user entered
            //with the password in the database
            int ok = 1;

            //Loop that checks the validity of the password by comparing each byte
            //of the db password with the user password. The hashBytes starts at 16
            //because the salt value is stored in the first 16 bytes and we aren't
            //comparing that with anything.
            for (int i = 0; i < 20; i++)
            {
                if (hashBytes[i + 16] != hash[i])
                {
                    ok = 0;
                }
            }

            string TodaysDate = DateTime.Today.ToString();

            if (list.Count == 0)
            {
                return false;
            }
            else if (ok == 0)
            {
                return false;
            }
            else if (list[0].FirstLoggedIn == TodaysDate)
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        //Register User
        public bool CreateUser(Users user)
        {

            //var collection1 = GetUsersCollection();
            //var coll = collection1.AsQueryable();
            //List<string> Emails = new List<string>();
            //foreach (var item in coll)
            //{
            //    Emails.Add(item.email);
            //}

            //int size = Emails.Count;

            //for (int i = 0; i < size; i++)
            //{
            //    if (user.email == Emails.ElementAt(i))
            //    {
            //        return false;
            //    }
            //}

            List<string> usersEmails = GetUsersEmails();

            for(int i = 0; i < usersEmails.Count; i++)
            {
                if(user.email == usersEmails[i])
                {
                    return false;
                }
            }

            //New byte array to hold the salt
            byte[] salt;

            //Generate a random salt
            new RNGCryptoServiceProvider().GetBytes(salt = new byte[16]);

            //Concatenate salt and user password then hash it using Rfc...Bytes
            var pdkdf2 = new Rfc2898DeriveBytes(user.password, salt, 10000);

            //Place the concatenated string in the byte array hash
            byte[] hash = pdkdf2.GetBytes(20);

            //New byte array to store hashed password + salt
            byte[] hashBytes = new byte[36];

            Array.Copy(salt, 0, hashBytes, 0, 16);
            Array.Copy(hash, 0, hashBytes, 16, 20);

            string savedPasswordHash = Convert.ToBase64String(hashBytes);

            user.password = savedPasswordHash;

            //Get the collection for edit -> allows us to edit the users collection
            var collection = GetUsersCollectionForEdit();
            try
            {
                //Insert new collection -> user
                collection.InsertOne(user);
            }
            catch
            {
                return false;
            }
            return true;
        }
        #endregion

        #region MaintenanceInformation
        public int GetMileageNeeded(List<MaintenanceObject> mainObject)
        {
            var collection = GetMaintenanceScheduleCollection();
            var maintenanceObjectID = mainObject[0].Id;
            var builder = Builders<MaintenanceObject>.Filter;
            var filt = builder.Where(x => x.Id == maintenanceObjectID);
            var list = collection.Find(filt).ToList();

            if (list.Count == 0)
            {
                return 0;
            }
            else
            {
                int mileage = list[0].Mileage;
                return mileage;
            }
        }
        #endregion

        #region DB Collection Retrievers
        //Database Collection Retrievers
        private IMongoCollection<MaintenanceObject> GetMaintenanceScheduleCollection()
        {
            MongoClientSettings settings = new MongoClientSettings();
            settings.Server = new MongoServerAddress(host, 10255);
            settings.UseSsl = true;
            settings.SslSettings = new SslSettings();
            settings.SslSettings.EnabledSslProtocols = SslProtocols.Tls12;

            MongoIdentity identity = new MongoInternalIdentity(dbName, userName);
            MongoIdentityEvidence evidence = new PasswordEvidence(password);

            settings.Credential = new MongoCredential("SCRAM-SHA-1", identity, evidence);

            MongoClient client = new MongoClient(settings);
            var database = client.GetDatabase(dbName);
            var maintenanceCollection = database.GetCollection<MaintenanceObject>("MaintenanceSchedule");
            return maintenanceCollection;
        }
        private IMongoCollection<Cars> GetCarsCollection()
        {
            MongoClientSettings settings = new MongoClientSettings();
            settings.Server = new MongoServerAddress(host, 10255);
            settings.UseSsl = true;
            settings.SslSettings = new SslSettings();
            settings.SslSettings.EnabledSslProtocols = SslProtocols.Tls12;

            MongoIdentity identity = new MongoInternalIdentity(dbName, userName);
            MongoIdentityEvidence evidence = new PasswordEvidence(password);

            settings.Credential = new MongoCredential("SCRAM-SHA-1", identity, evidence);

            MongoClient client = new MongoClient(settings);
            var database = client.GetDatabase(dbName);
            var carsCollection = database.GetCollection<Cars>("Cars");
            return carsCollection;
        }
        private IMongoCollection<Users> GetUsersCollection()
        {
            MongoClientSettings settings = new MongoClientSettings();
            settings.Server = new MongoServerAddress(host, 10255);
            settings.UseSsl = true;
            settings.SslSettings = new SslSettings();
            settings.SslSettings.EnabledSslProtocols = SslProtocols.Tls12;

            MongoIdentity identity = new MongoInternalIdentity(dbName, userName);
            MongoIdentityEvidence evidence = new PasswordEvidence(password);

            settings.Credential = new MongoCredential("SCRAM-SHA-1", identity, evidence);

            MongoClient client = new MongoClient(settings);
            var database = client.GetDatabase(dbName);
            var usersCollection = database.GetCollection<Users>(collectionName);
            return usersCollection;
        }

        private IMongoCollection<Users> GetUsersCollectionForEdit()
        {
            MongoClientSettings settings = new MongoClientSettings();
            settings.Server = new MongoServerAddress(host, 10255);
            settings.UseSsl = true;
            settings.SslSettings = new SslSettings();
            settings.SslSettings.EnabledSslProtocols = SslProtocols.Tls12;

            MongoIdentity identity = new MongoInternalIdentity(dbName, userName);
            MongoIdentityEvidence evidence = new PasswordEvidence(password);

            settings.Credential = new MongoCredential("SCRAM-SHA-1", identity, evidence);

            MongoClient client = new MongoClient(settings);
            var database = client.GetDatabase(dbName);
            var usersCollection = database.GetCollection<Users>(collectionName);
            return usersCollection;
        }
        #endregion


        #region IDisposable

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                }
            }

            this.disposed = true;
        }


        #endregion
    }
}