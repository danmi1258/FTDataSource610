Interactive Brokers data source sample.
----------------------------
Interactive Brokers can be used with databases of 1 second and up. It works similarly as AmiBroker's original IB data plugin. In addition it can
- filter bad ticks
- backfill 1 year of intraday data safely (be carefull, small timeframe backfills of multiple symbol takes ages...)
- backfill all available EOD data
- continuous contract of future

Credit:
-------
This sample uses InterActive Broker's C# API component.

Prerequisits
------------
TWS (or IB gateway) has to be installed, configured to accept imcoming connections (Enable ActiveX and Socket Clients, Socket port, Trusted IP Addresses)
.NET for AmiBroker must be installed (Standard or Developer Edition)

How to configure it
-------------------
0. Have TWS up and running configured to accept incoming connection
1. Create a new database in AmiBroker by clicking File - New - Database...
2. Set database path and click "Create".
3. Select ".Net Data Source Plug-in" in "Data source" combo box.
4. Click "Configure" button.
5. Select "Interactive Brokers" in the ".Net Data Source Type" combo box and click "Ok".
6. Click Setup symbols (so you have a few symbols set up for you)
7. Enter your TWS connection information (ip address, port, a unique host id) and click "Ok".
8. Select "Base time interval" by the combo box.
9 .Click "Ok".
10. Try to chart EUR.USD-IDEALPRO-CASH
11. Try to add GBP.USD-IDEALPRO-CASH to Real time window
12. Click File-Save  database

Symbology
---------
It uses the similar symbology as AmiBroker's original IB.dll plugin. It addition it accepts per symbol data specifier. E.g:

    // SYMBOL FORMAT:
    // symbol[-exchange[-security type[-currency[:data specifier]]]]
    //
    // SYMBOL:
    // SYMBOL is the symbol (local name) of the contract in the IB Contract database. 
    // You can see this symbol in TWS in symbol mode or by displaying contract description. 
    // Right click on the contract in the TWS page, select Contract Info-Description from the popup menu.
    // Symbols must be entered as they appear in TWS. This includes all spaces!
    // E.g.: "YM   SEP 11". Must include all 4 spaces!
    // For backadjusted continuous contract symbol has a "mask" of the IB contract's name + a '~' (E.g.: NQ~, SP~) and
    // it may need to include the underlying security as well separated by a '/'. 
    // E.g: "SP~/SPX". SP~ can stand for SPH6, SPM6, etc, where SPX is the underlying security.
    // E.g: "NQ~".     NQ~ can stand for NQH6, NQM6, etc. As underlying security is NQ, it does not need to be specified.
    //
    // EXCHANGE:
    // See IB site. E.g.: SMART, NASDAQ, NYSE, IDEALPRO, IDEAL, etc.
    //
    // SECURITY TYPES: 
    // STK = Stock, 
	// OPT = Option, 
	// FUT = Future, 
	// IND = Index, 
	// FOP = FutureOption, 
	// CASH = Cash, 
	// BAG = Bag, 
	// WAR = Warrant, 
	// BOND = Bond, 
	// CFD = Contract For Difference, 
	// FUND = Mutual fund, 
	// CMDTY = Commodity
    //
    // CURRENCY:
    // use 3 letter currency specifiers. E.g.: USD, EUR, AUD, etc.
    //
    // DATA SPECIFIERS:
    // A = Ask, B = Bid, BA = Bid and Ask, T = Trades, M = Midpoint, DA = Dividend Adjusted, HV = Historical Volatility, IV = Implied Volatiliy
    //
    // DEFAULTS:
    // exchange: SMART
    // security type: STK
    // currency: USD
    // data specifier: MIDPOINT for CASH, CFD, CMDTY nad FUND; TRADES for all other
    //
    // E.g:
    // --- FOREX ---
    // EUR.USD-IDEALPRO-CASH:M
    // EUR.USD-IDEALPRO-CASH
    //
    // --- STOCK ---
    // MSFT-SMART-STK-USD:T
    // MSFT-SMART-STK-USD
    // MSFT-SMART
    // MSFT
    //
    // --- INDEX ---
    // INDU-NYSE-IND-USD
    // INDU-NYSE-IND
    //
    // --- OPTION ---
    // MSFT  110319C00030000-SMART-OPT-USD
    // MSFT  110319C00030000-SMART-OPT
    //
    // --- FUTURES ---
    // YM   SEP 11-ECBOT-FUT-USD
    // YM   SEP 11-ECBOT-FUT
    //
    // --- CONTINUOUS CONTRACT of FUTURES ---
    // SP~/SPX-GLOBEX-FUT-USD
    //

    
See SymbolPArts.cs file.
