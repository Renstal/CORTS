﻿using Corts.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace Corts.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index(string email)
        {
            if(email != null)
            {
                ViewBag.UsersEmail = email;
            }
            else
            {
                ViewBag.UsersEmail = null;
            }
            return View();
        }

        
       
    }
}