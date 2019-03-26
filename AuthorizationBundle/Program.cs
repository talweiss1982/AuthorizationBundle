using System;
using System.Collections.Generic;
using Raven.Client.Documents;

namespace AuthorizationBundle
{
    class Program
    {
        static void Main(string[] args)
        {
            var rootUser = "root";
            var rootPassword = "salamandra#2753";
            var database = "Hospital";
            using (var store = new DocumentStore {Urls = new[] {"http://127.0.0.1:8080"},Database = database }.Initialize())
            {
                //TODO:try and create two root users
                AuthorizedSession.CreateRootUser(store, rootUser, rootPassword);
                using (var session = AuthorizedSession.OpenSession(store, rootUser, rootPassword))
                {
                    session.CreateGroup("Staff", null,"General group for hospital staff", 
                        new Permission { Collections = new List<string> { "StaffStuffs" }, Description = "Permissions for general staff stuff" });
                    session.CreateGroup("Doctors", "Staff","Doctors group",new Permission
                    {
                        Collections = new List<string>{"Prescription"},
                        Description = "Permissions to write prescription to patients"
                    });
                    session.CreateGroup("Cardiology", "Doctors", "Cardiologist only",new Permission
                    {
                        Collections = new List<string> { "EchocardiogramSchedule", "EchocardiogramResults" },
                        Description = "Permissions to schedule an echo exam to patients"
                    });
                    session.CreateGroup("Nurses", "Staff", "Nurses group", new Permission
                    {
                        Collections = new List<string> { "BloodTests" },
                        Description = "Permissions to schedule and read blood tests results"
                    });
                    session.CreateGroup("CardiologyNurses", "Nurses", "Cardiology nurses group", new Permission
                    {
                        Collections = new List<string> { "EchocardiogramResults" },
                        Description = "Permissions to read echo exam results for patients"
                    });
                    session.CreateUser("Shula","Shabazula$23");
                }
            }
        }
    }
}
