#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MaerskApi.Models;
using MaerskApi.Library;
namespace MaerskApi.Controllers
{
    [Route("api/Bookings")]
    [ApiController]
    public class BookingsController
    {
        private readonly string controller = "BookingsController";
        string action;
        string Message;

        Guid LogId;

        public MaerskContext Context { get; }
        public ILogger<BookingsController> Logger { get; }
        public BookingsController(MaerskContext context, ILogger<BookingsController> logger)
        {
            Context = context;
            Logger = logger;
            LoadCurrencyRates();
        }

        // GET: api/Bookings/5%2Cgbx
        /// <summary>
        /// Calculates averageprice with X decimals for the Y last bookings in requested currency. 
        /// </summary>
        /// <param name="request"></param>
        /// String Voyagecode, String Currencycode
        /// Format: "[a-Z]","[a-Z]"
        /// Example: "21","Gbp"
        /// Url: api/Bookings/21%2CGbp
        /// <returns>avarage</returns>
        [HttpGet]
        public async Task<ActionResult<decimal>> GetAverage(string voyagecode, string currencystring)
        {
            LogId = Guid.NewGuid();
            action = "GetAverage";
            int take = 10;
            int decimals = 2;
            decimal average;
            CurrencyType currencycode;
            List<Booking> bookings;
            List<Booking> bookingsonvoyage;
            List<CurrencyRate> currencyrates;
            MidpointRounding midpointrounding = MidpointRounding.AwayFromZero;
            Message = string.Format("Parameteres recieved: VoyageCode: {0}, Currency: {1}.", voyagecode, currencystring);
            Logger.LogInformation(StandardLogger.LoggerLoader(LogId, controller, action, Message, StandardLogger.LoggingEnum.Start, DateTimeOffset.UtcNow));

            //We could handle "No data" here - but its not likely to occur in production. Will check it later for the sake of good order
            try
            {
                currencycode = Converter.GetCurrency(string.Format("CURRENCY.{0}", currencystring));
                bookings = await Context.Booking.ToListAsync();
                currencyrates = await Context.CurrencyRates.ToListAsync();
                bookingsonvoyage = await Context.Booking.Where(x => x.VoyageCode == voyagecode).ToListAsync();
            }
            catch (Exception)
            {
                Logger.LogWarning(StandardLogger.LoggerLoader(LogId, controller, action, Message, StandardLogger.LoggingEnum.ParamNotvalid, DateTimeOffset.UtcNow));
                return new StatusCodeResult(StatusCodes.Status400BadRequest);
            }

            StatusCodeResult ValidateStatus = ValidateInput(LogId, voyagecode, currencystring, currencycode, bookings, currencyrates, bookingsonvoyage);
            var okresult = new OkResult();
            if (ValidateStatus.StatusCode == okresult.StatusCode)
            {
                average = decimal.Round(Convert.ToDecimal(Converter.ConvertListAverage(Context, bookingsonvoyage, take, currencyrates, currencycode, Logger, LogId)), decimals, midpointrounding);
                if (average != 0)
                {
                    Message = string.Format("Average {0} calculated for: VoyageCode: {1}, Currency: {2}.", average.ToString(), voyagecode, currencystring);
                    Logger.LogInformation(StandardLogger.LoggerLoader(LogId, controller, action, Message, StandardLogger.LoggingEnum.Ok, DateTimeOffset.UtcNow));
                    return new OkObjectResult(average);
                }
                Message = string.Format("Average could not be calculated for: VoyageCode: {0}, Currency: {1}.", voyagecode, currencystring);
                Logger.LogInformation(StandardLogger.LoggerLoader(LogId, controller, action, Message, StandardLogger.LoggingEnum.NoCalc, DateTimeOffset.UtcNow));
                return new StatusCodeResult(400);
            }
            else
            {
                return ValidateStatus.StatusCode;
            }
        }

        // Post: api/Bookings?"1234",109.6,Currency.GBP,2022-05-04T21%3A15%3A07%2B00%3A00
        /// <summary>
        /// Inserts a new booking price.  
        /// </summary>
        /// <param ="voyagecode,price,currencystring,timestamp"></param>
        /// string voyagecode, decimal price, string currency, DateTimeOffset timestamp
        /// 
        /// Example: UpdatePrice("451S", 109.5, Currency.Gbx, DateTimeOffset.Now)
        /// Url: api/Bookings?"1234",109.6,Currency.GBP,2022-05-04T21%3A15%3A07%2B00%3A00
        /// <returns>NoContent</returns>

        [HttpPost]
        public async Task<ActionResult> UpdatePrice(string voyagecode, decimal price, string currency, DateTimeOffset timestamp)
        {
            Guid LogId = Guid.NewGuid();
            action = "UpdatePrice";
            Message = string.Format("Parameteres recieved: VoyageCode: {0}, Price: {1}, Currency: {2}, Timestamp: {3}.", voyagecode, price, currency, timestamp);
            Logger.LogInformation(StandardLogger.LoggerLoader(LogId, controller, action, Message, StandardLogger.LoggingEnum.Start, DateTimeOffset.UtcNow));
            Booking booking = new Booking();
            try
            {
                CurrencyType currencycode = Converter.GetCurrency(currency);
                booking.VoyageCode = voyagecode;
                booking.Price = price;
                booking.CurrencyCode = currencycode;
                booking.Timestamp = timestamp;
                await Context.Booking.AddAsync(booking);
            }
            catch
            {
                Message = string.Format("Invalid parameteres : VoyageCode: {0}, Price: {1}, Currency: {2}, Timestamp: {3}.", voyagecode, price, currency, timestamp);
                Logger.LogInformation(StandardLogger.LoggerLoader(LogId, controller, action, Message, StandardLogger.LoggingEnum.ParamNotvalid, DateTimeOffset.UtcNow));
                return new StatusCodeResult(StatusCodes.Status400BadRequest);
            }
            Message = string.Format("Booking Posted : VoyageCode: {0}, Price: {1}, Currency: {2}, Timestamp: {3}.", voyagecode, price, currency, timestamp);
            Logger.LogInformation(StandardLogger.LoggerLoader(LogId, controller, action, Message, StandardLogger.LoggingEnum.NoContent, DateTimeOffset.UtcNow));
            return new StatusCodeResult(StatusCodes.Status204NoContent);
        }



        private bool CurrencyRateExist(CurrencyType currencyratetarget, List<CurrencyRate> currencyrates)
        {
            //currencyrates.Any(a => a.CurrencyRateSource == currency || a.CurrencyRateTarget == currency);  //Overhead for small collection
            foreach (CurrencyRate currencyrate in currencyrates) // Fastest in small collections
            {
                if (currencyrate.CurrencyRateSource == currencyratetarget || currencyrate.CurrencyRateTarget == currencyratetarget)
                {
                    return true;
                }
            }
            return false;
        }
        private StatusCodeResult ValidateInput(Guid LogId, string voyagecode, string currencystring, CurrencyType currencytype, List<Booking> bookings, List<CurrencyRate> currencyrates, List<Booking> bookingsonvoyage)
        {
            string action = "ValidateInput";
            if (bookings.Count() <= 0)
            {
                Message = "No Bookings in system";
                Logger.LogInformation(StandardLogger.LoggerLoader(LogId, controller, action, Message, StandardLogger.LoggingEnum.DataInconsistency, DateTimeOffset.UtcNow));
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
            if (currencyrates.Count() <= 0)
            {
                Message = "No Currency Rates in system";
                Logger.LogInformation(StandardLogger.LoggerLoader(LogId, controller, action, Message, StandardLogger.LoggingEnum.DataInconsistency, DateTimeOffset.UtcNow));
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
            if (bookingsonvoyage.Count() <= 0)
            {
                Message = string.Format("No bookings on VoyageCode {0}.", voyagecode);
                Logger.LogInformation(StandardLogger.LoggerLoader(LogId, controller, action, Message, StandardLogger.LoggingEnum.NotFound, DateTimeOffset.UtcNow));
                return new StatusCodeResult(StatusCodes.Status400BadRequest);
            }
            if (currencytype == CurrencyType.ERROR)
            {
                Message = string.Format("Currency {0} not found.", currencystring);
                Logger.LogInformation(StandardLogger.LoggerLoader(LogId, controller, action, Message, StandardLogger.LoggingEnum.NotFound, DateTimeOffset.UtcNow));
                return new StatusCodeResult(StatusCodes.Status404NotFound);
            }
            if (CurrencyRateExist(currencytype, currencyrates) == false)
            {
                Message = string.Format("No rate for VoyageCode {0}.", currencytype);
                Logger.LogInformation(StandardLogger.LoggerLoader(LogId, controller, action, Message, StandardLogger.LoggingEnum.NotFound, DateTimeOffset.UtcNow));
                return new StatusCodeResult(StatusCodes.Status404NotFound);
            }
            return new OkResult();
        }

        private async void LoadCurrencyRates()
        {
            LogId = Guid.NewGuid();
            string action = "LoadCurrencyRates";
            CurrencyRate currencyrate;
            List<CurrencyRate> currencyRates = new List<CurrencyRate>();
            currencyRates = await Context.CurrencyRates.ToListAsync();
            if (currencyRates.Count > 0) //Are loaded allready
            {
                return;
            }
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

            await Context.SaveChangesAsync();
            Message = "Currency Rates Loaded.";
            Logger.LogInformation(StandardLogger.LoggerLoader(LogId, controller, action, Message, StandardLogger.LoggingEnum.NoContent, DateTimeOffset.UtcNow));
        }


    }
}
