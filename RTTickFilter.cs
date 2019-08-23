using System;
using System.Collections;
using System.Threading;
using AmiBroker.Data;
using System.Collections.Generic;
using IBApi;

namespace AmiBroker.DataSources.IB
{
    /// <summary>
    /// Bad tick filter
    /// </summary>
    internal class RTTickFilter
    {
        private const int MaxSampleNo = 10;

        private bool filter;
        private DateTime date;
        private int sampleIdx = 0;
        private bool sampleOk;
        private decimal[] sample = new decimal[MaxSampleNo];

        private decimal lastBid;
        private decimal lastAsk;
        private decimal lastMid;
        private int prevVolume;     // to save previous TickType.Volume data
        private int cumLastSize;    // accumulated sizes of TickType.LastSize events between two VOLUME events
        private float prevPrice;
        private float lastPrice;

        private TickerData tickerData;

        internal RTTickFilter(TickerData tickerData, bool filter)
        {
            this.tickerData = tickerData;
            this.filter = filter;
        }

        internal void MergePrice(int tickerId, int field, float price, DateTime time)
        {
            //
            // if price is 0 or less (no live data)
            //
            if (price <= 0.0f)
                return;

            decimal p = (decimal)price;

            //
            // save BID and ASK prices
            //
            if (field == TickType.ASK)
                lastAsk = p;

            else if (field == TickType.BID)
                lastBid = p;

            //
            // calc MID price if data is available
            //
            if (lastAsk > 0m && lastBid > 0m)
                lastMid = (lastAsk + lastBid) / 2m;

            //
            // check if we need to use this price tick or it is irrelevant
            //
            if (tickerData.SymbolParts.DataType == IBHistoricalDataType.Midpoint || tickerData.SymbolParts.DataType == IBHistoricalDataType.BidAsk)
            {
                if (field != TickType.BID && field != TickType.ASK && field != TickType.LAST)
                    return;
            }
            else if (tickerData.SymbolParts.DataType == IBHistoricalDataType.Trades || tickerData.SymbolParts.DataType == IBHistoricalDataType.Adjusted)
            {
                if (field != TickType.LAST)
                    return;
            }
            else if (tickerData.SymbolParts.DataType == IBHistoricalDataType.Bid)
            {
                if (field != TickType.BID)
                    return;
            }
            else if (tickerData.SymbolParts.DataType == IBHistoricalDataType.Ask)
            {
                if (field != TickType.ASK)
                    return;
            }
            else if (tickerData.SymbolParts.DataType == IBHistoricalDataType.HistoricalVolatility)
            {
                if (field != TickType.OPTION_HISTORICAL_VOL)
                    return;
            }
            else if (tickerData.SymbolParts.DataType == IBHistoricalDataType.ImpliedVolatility)
            {
                if (field != TickType.OPTION_IMPLIED_VOL)
                    return;
            }
            else
            {
                return;
            }

            //
            // check if tick is in acceptable range
            //
            if (filter)
            {
                //
                // filter #1
                //

                // it is a simple filter to remove bad price ticks
                // it works only during high volume, continuously traded period, fell free to improve it
                if (lastMid != 0.0m                                                             // there is a midpoint price already 
                    && (DateTime.Now.Ticks - date.Ticks) / TimeSpan.TicksPerSecond < 5          // time elapsed since last tick event is less then 5 second
                    && Math.Abs(p - lastMid) / lastMid > 0.03m)                    // price change is greater then 3 %
                {
                    // This may impose 5 sec delay and loss of some ticks in higly volatily and thin market (not in Forex)
                    LogAndMessage.Log(tickerData, MessageType.Trace, "Bad tick has been rejected. Price:" + price.ToString() + " MidPoint:" + lastMid.ToString());
                    return;
                }

                //
                // filter #2
                //

                //
                // 
                //

                // it is a "round robin" array store
                // check if index points behind last element of the array
                if (sampleIdx == MaxSampleNo)
                {
                    sampleIdx = 0;
                    sampleOk = true;
                }

                // if there is enough data yet
                if (sampleOk)
                {
                    // calc avg prior to this tick
                    decimal avg = 0;
                    for (int i = 0; i < MaxSampleNo; i++)
                        avg += sample[i];

                    // calc price move compared to avg price
                    decimal rate = avg / MaxSampleNo / p;

                    // if to big, reject tick
                    if (rate > 1.02M || rate < 0.98M)
                    {
                        LogAndMessage.Log(tickerData, MessageType.Trace, "Bad tick has been rejected. Price:" + price.ToString() + " Avg of last " + MaxSampleNo + " ticks:" + (avg / MaxSampleNo).ToString());
                        return;
                    }

                    // store current price in sample array
                    sample[sampleIdx] = p;
                    sampleIdx++;
                }
                // if there is NOT enough data yet
                else
                {
                    // store current price in sample array
                    sample[sampleIdx] = p;
                    sampleIdx++;
                }
            }

            // storing the tick into a Quote to merge
            if (tickerData.SymbolParts.DataType == IBHistoricalDataType.Midpoint)
            {
                if (lastMid == 0.0m)
                    return;
                lastPrice = (float)lastMid;
            }
            else
                lastPrice = price;

            date = time;

            Quotation quote = new Quotation();
            quote.DateTime = (AmiDate)date;
            quote.Price = lastPrice;
            quote.Low = lastPrice;
            quote.High = lastPrice;
            quote.Open = prevPrice != 0 ? prevPrice : lastPrice;
            quote.Volume = 0;

            prevPrice = lastPrice;

            SaveQuote(quote);
        }

        internal void MergeVolume(int tickerId, int tickType, int size, DateTime time)
        {
            // if bad volume data
            if (tickType == TickType.VOLUME
             && size < 0)
                return;

            // if bad trade size data
            if (tickType == TickType.LAST
             && size < 0)
                return;

            // "trade size" is calculated to increase to last quote's volume
            float correctionVolume = 0;

            // trade size data is calculated by IB servers. NOT ACCURATE!!!
            if (tickType == TickType.LAST_SIZE)
            {
                cumLastSize += size;                // trade size is accumulated 
                correctionVolume = size;            // trade size is added to current quote
            }

            // VOLUME data is published by the exchange, and is accurate. Accumulated trade size (LAST_SIZE) usually does not macht this!!! So a coorrection is needed sometimes.
            else if (tickType == TickType.VOLUME)
            {
                if (prevVolume != 0 && size >= prevVolume)
                {
                    // calculate the difference between the volume change and added cummulated trade size
                    correctionVolume = size - prevVolume - cumLastSize;

                    prevVolume = size;              // save this volume data so we can do correction next time based on this
                    cumLastSize = 0;                // reset accumulated trade size

                    // if no need to correct volume
                    if (correctionVolume == 0)
                        return;
                }
                else
                {
                    // save the fist volume info and return
                    prevVolume = size;
                    return;
                }
            }

            else if (tickType == TickType.ASK_SIZE || tickType == TickType.BID_SIZE)
            {
                correctionVolume = 0;
            }
            else
            {
                return;
            }

            // if not enough data yet
            if (lastPrice == 0)
                return;

            Quotation quote = new Quotation();
            quote.DateTime = (AmiDate)time;
            quote.Price = lastPrice;
            quote.Low = lastPrice;
            quote.High = lastPrice;
            quote.Open = prevPrice != 0 ? prevPrice : lastPrice;
            quote.Volume = correctionVolume;

            SaveQuote(quote);
        }

        private void SaveQuote(Quotation quote)
        {
            DateTime quoteDate = (DateTime)quote.DateTime;

            // get the trading day
            //DateTime tradingDay = DateTime.MinValue;
            //tradingDay = tickerData.LiquidHours.GetTradeDate(quoteDate);
            //if (!IBDataSource.RthOnly && tradingDay == DateTime.MinValue)
            //tradingDay = tickerData.TradingDays.GetTradeDate(quoteDate);

            // if no trading day found and RTH only
            //if (tradingDay == DateTime.MinValue)
            //    if (IBDataSource.RthOnly)
            //        return;
            //    else
            //        tradingDay = DateTime.Now.Date;

            DateTime tradingDay = tickerData.TradingDays.GetTradeDate(quoteDate);
            if (tradingDay == DateTime.MinValue)
                tradingDay = DateTime.Now.Date;

            try
            {
                lock (tickerData.Quotes)
                {
                    // Merge quote into last intraday quote in QuotationList
                    if (tickerData.Quotes.Periodicity != Periodicity.EndOfDay)
                        tickerData.Quotes.Merge(quote);

                    // Merge quote into last EOD quote in QuotationList
                    if (tickerData.Quotes.Periodicity == Periodicity.EndOfDay || IBDataSource.AllowMixedEODIntra)
                        tickerData.Quotes.MergeEod(quote, (AmiDate)tradingDay);
                }
            }
            catch (Exception ex)
            {
                LogAndMessage.Log(tickerData, MessageType.Error, "Error while merging received quote: " + ex);
            }
        }
    }
}
