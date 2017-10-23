using System;
using System.Globalization;
using Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

namespace Parser
{
    class Program
    {
        static void Main(string[] args)
        {
            var FilePath = @"C:\Users\nickliako\Google Drive\.NET Coding School\Project\BackOffice Scripts\CitizenDebts_1M_3.txt";
            // var CorrectFilePath = @"C:\Users\nickliako\Google Drive\.NET Coding School\Project\BackOffice Scripts\DEBTS_"+ DateTime.Now.ToString("yyyyMMdd")+".txt";

            // Read the contents of the File skipping the first line which contains the headers
            List<string[]> list = File.ReadLines(FilePath).Skip(1)
                          .Select(line => line.Split(';'))
                          .ToList();

            using (var debtsdb = new Qualco4_DBFirstContext())
            {
                add_addresses(debtsdb, list);
                add_citizens(debtsdb,list);
                add_bills(debtsdb, list);
            }
        } // End main

    public static void add_citizens(Qualco4_DBFirstContext debtsdb, List<string[]> list)
        {
            //Initially query the database and get all of the citizens
            var citizensindb = (from b in debtsdb.User
                                select b).ToList();

            //Query the database and get all of the Addresses
            var addressesindb = (from b in debtsdb.Address
                                 select b).ToList();

            //Get all the distinct citizens from the import file
            var distinctusers = (from item in list
                                 group item by item[0] into groups
                                 select groups.First()).ToList();

            Console.WriteLine($"{DateTime.Now}   Number of Users in db: {citizensindb.Count().ToString()} Number of Users in file: {distinctusers.Count().ToString()}");

           
            //Join distinctusers with addresses so that you can get back the AddressId primary key
            var test = (from ca in distinctusers
                        join ka in addressesindb on new { AddressName=ca[5], County = ca[6] }  equals new { ka.AddressName, ka.County }
                        select new User {
                            Vat = Convert.ToInt64(ca[0]),
                            FirstName = ca[1],
                            LastName = ca[2],
                            EMail = ca[3],
                            Password = "123",
                            Phone = (Convert.ToInt64(ca[4])),
                            AddressId = ka.AddressId,
                            EmailSent = false
                             }).ToList();

            // For all the citizens that do not exist in the db, create a new User object

            //   var userstobeinstertedondb = (from e in test
            //      where !(from m in citizensindb
            //              select m.Vat).Contains(Convert.ToInt64(e.ca[0]))
            var userstobeinstertedondb = (from e in test
                                          join userindb in citizensindb on e.Vat equals userindb.Vat  into ab
                                          from c in ab.DefaultIfEmpty()
                                          where c == null
                                          select new User
                                          {
                                              Vat = Convert.ToInt64(e.Vat),
                                              FirstName = e.FirstName,
                                              LastName = e.LastName,
                                              EMail = e.EMail,
                                              Password = sha256_hash(RandomString(16)),
                                              Phone = e.Phone,
                                              AddressId = e.AddressId,
                                              EmailSent = false
                                          }
                                             ).ToList();

            Console.WriteLine($"{DateTime.Now}   Users to be inserted on db: {userstobeinstertedondb.Count().ToString()}");

             debtsdb.AddRange(userstobeinstertedondb);
             debtsdb.SaveChanges();

            Console.WriteLine($"{DateTime.Now}   Users were inserted on db");
        }


    public static void add_addresses(Qualco4_DBFirstContext debtsdb, List<string[]> list)
        {
            //Query the database and get all of the Addresses
            var addressesindb = (from b in debtsdb.Address
                                 select b).ToList();

            // Get all addresses from the imported file
            var alladdresses = (from item in list
                                select new Address { AddressName = item[5], County = item[6] }).ToList();

            // Group by Address,County and get all the distinct pairs (Address,County)
            var distinctaddresses = alladdresses.GroupBy(i => new { i.AddressName, i.County }).Select(i => i.First()).ToList();

            Console.WriteLine($"{DateTime.Now}   Number of Addresses in db: {addressesindb.Count().ToString()} Number of Addresses in file: {distinctaddresses.Count().ToString()}");

            var addressestobeinstertedondb = (from addr in distinctaddresses
                                              join addr2 in addressesindb on new { addr.AddressName, addr.County } equals new { addr2.AddressName, addr2.County } into gj
                                              from subaddr in gj.DefaultIfEmpty()
                                              where subaddr == null
                                              select new Address { AddressName = addr.AddressName, County = addr.County }).ToList();

            Console.WriteLine($"{DateTime.Now}   Addresses to be inserted on db:{addressestobeinstertedondb.Count().ToString()}");

           debtsdb.AddRange(addressestobeinstertedondb);
           debtsdb.SaveChanges();

            Console.WriteLine($"{DateTime.Now}   Addresses were inserted on db");

        }

    public static void add_bills(Qualco4_DBFirstContext debtsdb, List<string[]> list)
        {
            //Initially query the database and get all of the citizens
            var citizensindb = (from b in debtsdb.User
                                select b).ToList();

            var billsforuser = (from item in list
                                join ff in citizensindb on new { Vat = Convert.ToInt64(item[0]) } equals new  { ff.Vat }
                                select new { item, ff.UserId }).ToList();

            Console.WriteLine($"{DateTime.Now}   Starting processing bills");

            var allbills = (from bill in billsforuser
                            select new Bill { BillId=bill.item[7],
                                              BillDescription = bill.item[8],
                                             Amount =float.Parse(bill.item[9], CultureInfo.InvariantCulture.NumberFormat),
                                             DateDue = DateTime.ParseExact(bill.item[10], "yyyyMMdd",CultureInfo.InvariantCulture),
                                             UserId = bill.UserId 
                            }).ToList();

           Console.WriteLine($"{DateTime.Now}   Bills to be inserted on db:{allbills.Count().ToString()}");

           debtsdb.AddRange(allbills);
           debtsdb.SaveChanges();

           Console.WriteLine($"{DateTime.Now}   Bills were inserted on db");

        }

    // Reference: https://stackoverflow.com/questions/9995839/how-to-make-random-string-of-numbers-and-letters-with-a-length-of-5
    private static string RandomString(int length)
        {
            const string pool = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var builder = new StringBuilder();
            Random ad = new Random();

            for (var i = 0; i < length; i++)
            {
                var c = pool[ad.Next(0, pool.Length)];
                builder.Append(c);
            }

            return builder.ToString();
        }

    public static String sha256_hash(string value)
        {
            StringBuilder Sb = new StringBuilder();

            using (var hash = SHA256.Create())
            {
                Encoding enc = Encoding.UTF8;
                Byte[] result = hash.ComputeHash(enc.GetBytes(value));

                foreach (Byte b in result)
                    Sb.Append(b.ToString("x2"));
            }

            return Sb.ToString();
        }

    public static void send_email()
        {
            //https://www.emailarchitect.net/easendmail/kb/csharp.aspx?cat=2#c-send-email-using-gmail-account-over-explicit-ssl-tls-on-25-or-587-port
        }

    } // class

    



}

