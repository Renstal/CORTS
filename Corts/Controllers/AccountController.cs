﻿using Corts.DAL;
using Corts.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Corts.Controllers
{
    public class AccountController : Controller, IDisposable
    {
        private Dal dal = new Dal();
        // GET: Account

        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Login()
        {
            ViewBag.Message = "Login";
            return View();
        }
        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }

        //
        [HttpPost]
        public ActionResult Register(RegisterViewModel userInfo)
        {
            Users user = new Users();
            user.email = userInfo.Email;
            user.password = userInfo.Password;
            try
            {
                dal.CreateUser(user);
                return RedirectToAction("Login");
            }
            catch
            {
                return View();
            }
        }
        //// POST: /Account/Register
        //[HttpPost]
        //[AllowAnonymous]
        //[ValidateAntiForgeryToken]
        //public ActionResult Register(RegisterViewModel model)
        //{
        //    Guid id = new Guid();
        //    string email = model.Email;
        //    string password = model.Password;

        //    Users user = new Users(id, email, password);
        //    try
        //    {
        //        dal.CreateUser(model);
        //        return RedirectToAction("Register");
        //    }
        //    catch
        //    {
        //        return View();
        //    }

        //}
        #region IDisposable
        private bool disposed = false;
        new protected void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        new protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    this.dal.Dispose();
                }
            }

            this.disposed = true;
        }

        # endregion

    }
}