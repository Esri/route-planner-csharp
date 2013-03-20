::@echo off

set Location1="C:\Users\chai6389\Desktop\Routes_copied2.csv"
set Location2="C:\Users\chai6389\Desktop\Stops_copied2.csv"
set Location3="C:\Users\chai6389\Desktop\Orders_copied2.csv"


xcopy %1  %Location1%
xcopy %2  %Location2%
xcopy %3  %Location3%

del %1
del %2
del %3

pause