using System.Threading.Tasks;
using System.Collections.Generic;
using MaerskApi.Controllers;
using MaerskApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Moq;
using System;
using MaerskApi.Library;
using Microsoft.AspNetCore.Http;
using System.Net.Http;

namespace TestMaerskApi
{
    [TestFixture]
    public class Tests
    {
        DbContextOptions options = new DbContextOptionsBuilder<MaerskContext>()
            .UseInMemoryDatabase(databaseName: "MaerskList")
            .Options;
        Mock mock = new Mock<ILogger<BookingsController>>();
        Guid LogId;
        private readonly string testcontroller = "BookingsController";

        [SetUp]
        public void Setup()
        {
            LoadTestDataAsync();
        }

        [Test] /* Test if average is calculated properly. */
        public async Task GetAverage()
        {
            MaerskContext Context = new MaerskContext((DbContextOptions<MaerskContext>)options);
            ILogger<BookingsController> Logger = (ILogger<BookingsController>)mock.Object;
            BookingsController controller = new BookingsController(Context, Logger);
            ActionResult<decimal> average = await controller.GetAverage("21", "GBP");
            OkObjectResult result = (OkObjectResult)average.Result;
            decimal avg = (decimal)result.Value;
            Assert.AreEqual(avg, 334.65m);
        }
        [Test] /* Test if UpdatePrice run OK. */
        public async Task UpdatePrice()
        {
            MaerskContext Context = new MaerskContext((DbContextOptions<MaerskContext>)options);
            ILogger<BookingsController> Logger = (ILogger<BookingsController>)mock.Object;
            BookingsController controller = new BookingsController(Context, Logger);
            ActionResult result = await controller.UpdatePrice("4515", Convert.ToDecimal("109.5"), "Currency.gbp", Convert.ToDateTime("2022-05-04T21:15:07+00:00"));
            StatusCodeResult res = (StatusCodeResult)result;
            StatusCodeResult test = new StatusCodeResult(StatusCodes.Status204NoContent);
            Assert.AreEqual(res.StatusCode, test.StatusCode);
        }
        public void LoadTestDataAsync()
        {
            MaerskContext Context = new MaerskContext((DbContextOptions<MaerskContext>)options);
            ILogger<BookingsController> Logger = (ILogger<BookingsController>)mock.Object;
            BookingsController controller = new BookingsController(Context, Logger);
            LogId = Guid.NewGuid();
            string action = "LoadTestData";
            Booking booking;
            CurrencyRate currencyrate;
            List<CurrencyRate> currencyRates = new List<CurrencyRate>();
            currencyrate = new CurrencyRate();
            currencyrate.CurrencyRateSource = CurrencyType.GBP;
            currencyrate.CurrencyRateTarget = CurrencyType.USD;
            currencyrate.CurrencyRateSourcePrice = 1.00M;
            currencyrate.CurrencyRateTargetPrice = 1.31M;
            currencyRates.Add(currencyrate);
            Context.CurrencyRates.Add(currencyrate);

            currencyrate = new CurrencyRate();
            currencyrate.CurrencyRateSource = CurrencyType.GBP;
            currencyrate.CurrencyRateTarget = CurrencyType.EUR;
            currencyrate.CurrencyRateSourcePrice = 1.00M;
            currencyrate.CurrencyRateTargetPrice = 1.20M;
            currencyRates.Add(currencyrate);
            Context.CurrencyRates.Add(currencyrate);

            currencyrate = new CurrencyRate();
            currencyrate.CurrencyRateSource = CurrencyType.USD;
            currencyrate.CurrencyRateTarget = CurrencyType.EUR;
            currencyrate.CurrencyRateSourcePrice = 1.00M;
            currencyrate.CurrencyRateTargetPrice = 0.92M;
            currencyRates.Add(currencyrate);
            Context.CurrencyRates.Add(currencyrate);

            int count = 1;
            while (count != 22)
            {
                booking = new Booking();
                booking.VoyageCode = count.ToString();
                booking.CurrencyCode = CurrencyType.GBP;
                booking.Price = (count * 17.41M);
                booking.Timestamp = DateTimeOffset.UtcNow;
                Context.Booking.Add(booking);

                booking = new Booking();
                booking.VoyageCode = count.ToString();
                booking.CurrencyCode = CurrencyType.USD;
                booking.Timestamp = DateTimeOffset.UtcNow;
                booking.Price = (count * 19.41M);
                Context.Booking.Add(booking);

                booking = new Booking();
                booking.VoyageCode = count.ToString();
                booking.CurrencyCode = CurrencyType.EUR;
                booking.Timestamp = DateTimeOffset.UtcNow;
                booking.Price = (count * 20.41M);
                Context.Booking.Add(booking);

                count++;
            }
            Context.SaveChanges();
        }
    }

}


