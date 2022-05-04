using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MaerskApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MaerskApi.Library
{
    public static class Converter
    {
        //public static decimal ConvertListAverage(MaerskContext db, List<Booking> bookings,int nb, List<CurrencyRate> currencyrates, Currency currencyratetarget, ILogger _logger,Guid LogId)
        public static decimal ConvertListAverage(MaerskContext db, List<Booking> bookings, int nb, List<CurrencyRate> currencyrates, CurrencyType currencyratetarget, ILogger _logger, Guid LogId)
        {
            try
            {
                foreach (Booking booking in bookings)
                {
                    booking.Price = ConvertPrice(booking, currencyrates, currencyratetarget, _logger, LogId);
                    if (booking.Price == 0)
                    {
                        return 0;
                    }
                }
                decimal average = bookings.OrderByDescending(o => o.Timestamp).Take(nb).Average(x => x.Price);
                return average;
            }
            catch (Exception)
            {
                return 0;
            }
        }
        //private static decimal ConvertPrice(Booking booking, List<CurrencyRate> currencyrates, Currency currencyratetarget, ILogger _logger, Guid LogId)
        private static decimal ConvertPrice(Booking booking, List<CurrencyRate> currencyrates, CurrencyType currencyratetarget, ILogger _logger, Guid LogId)
        {
            //Currency currencyratesource = new Currency();
            string? voyagecode = String.Empty;
            //currencyratesource = booking.Currency;
            CurrencyType currencyratesource;
            currencyratesource = booking.CurrencyCode;

            voyagecode = booking.VoyageCode;
            try
            {
                foreach (CurrencyRate currencyrate in currencyrates)
                {
                    if (currencyratesource == currencyratetarget)
                    {
                        break;
                    }
                    if (currencyratesource == currencyrate.CurrencyRateSource)
                    {
                        booking.Price = booking.Price * (currencyrate.CurrencyRateTargetPrice / currencyrate.CurrencyRateSourcePrice);
                        break;
                    }
                    if (currencyratetarget == currencyrate.CurrencyRateSource)
                    {
                        booking.Price = booking.Price * (currencyrate.CurrencyRateSourcePrice / currencyrate.CurrencyRateTargetPrice);
                        break;
                    }
                }
                return booking.Price;
            }
            catch (Exception)
            {
                string message = StandardLogger.LoggerLoader(LogId, "Converter", "Voyagekode",voyagecode, StandardLogger.LoggingEnum.DataInconsistency, DateTimeOffset.UtcNow);
                _logger.LogError(message);
                return booking.Price;
            }
        }
        public static CurrencyType GetCurrency(string currencystring)
        {
            CurrencyType currencyType = new CurrencyType();
            currencystring = currencystring.ToUpper();
            switch (currencystring)
            {
                case "CURRENCY.GBP":
                    currencyType = CurrencyType.GBP;
                    break;

                case "CURRENCY.USD":
                    currencyType = CurrencyType.USD;
                    break;

                case "CURRENCY.EUR":
                    currencyType = CurrencyType.EUR;
                    break;
                default:
                    currencyType = CurrencyType.ERROR;
                    break;
            }
            return currencyType;
        }

    }
}