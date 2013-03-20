using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using ESRI.ArcLogistics.Routing.Json;

namespace ESRI.ArcLogistics.Routing
{
    /// <summary>
    /// SolveErrorParser class.
    /// </summary>
    internal class SolveErrorParser
    {
        #region constants
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        // error code pattern
        private const string RPAT_ERROR_CODE = "(ERROR|WARNING)\\s*?(?<code>\\d+)\\s*?:";
        private const string CODE_GROUP = "code";

        // NA error code -> hresult mapping
        private static readonly Dictionary<int, NAError> hrMap;

        #endregion constants

        #region constructors
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        static SolveErrorParser()
        {
            hrMap = new Dictionary<int, NAError>();
            hrMap.Add(30089, NAError.E_NA_VRP_SOLVER_EMPTY_INFEASIBLE_ROUTES);
            hrMap.Add(30090, NAError.E_NA_VRP_SOLVER_PREASSIGNED_INFEASIBLE_ROUTES);
            hrMap.Add(30091, NAError.E_NA_VRP_SOLVER_NO_SOLUTION);
            hrMap.Add(30092, NAError.E_NA_VRP_SOLVER_INVALID_INPUT);
        }

        #endregion constructors

        #region public methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        public static bool ParseErrorCode(JobMessage[] messages, out int hResult)
        {
            Debug.Assert(messages != null);

            hResult = HResult.E_FAIL;

            bool found = false;
            foreach (JobMessage msg in messages)
            {
                if (!String.IsNullOrEmpty(msg.Description))
                {
                    int code;
                    if (_ParseCode(msg.Description, out code))
                    {
                        NAError hr;
                        if (hrMap.TryGetValue(code, out hr))
                        {
                            hResult = (int)hr;
                            found = true;
                            break;
                        }
                    }
                }
            }

            return found;
        }

        #endregion public methods

        #region private methods
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        private static bool _ParseCode(string msg, out int code)
        {
            Debug.Assert(msg != null);

            code = 0;

            bool res = false;
            try
            {
                Regex regex = new Regex(RPAT_ERROR_CODE, RegexOptions.IgnoreCase);
                Match m = regex.Match(msg);
                if (m.Success)
                {
                    Group group = m.Groups[CODE_GROUP];
                    if (group != null && group.Success)
                    {
                        if (!String.IsNullOrEmpty(group.Value))
                        {
                            code = Convert.ToInt32(group.Value);
                            res = true;
                        }
                    }
                }
            }
            catch { }

            return res;
        }

        #endregion private methods
    }
}
