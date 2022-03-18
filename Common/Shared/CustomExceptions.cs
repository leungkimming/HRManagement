﻿using System.Globalization;

namespace Common.Shared
{
    public class PayslipMonthAlreadyExistException : Exception
    {
        public PayslipMonthAlreadyExistException() { }

        public PayslipMonthAlreadyExistException(int month)
            : base(String.Format("Payslip for {0} already exist",
                CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month)))
        {
        }
    }
    public class UserAlreadyExistException : Exception
    {
        public UserAlreadyExistException() { }

        public UserAlreadyExistException(string user)
            : base(String.Format("User Name {0} already exist", user))
        {
        }
    }

}