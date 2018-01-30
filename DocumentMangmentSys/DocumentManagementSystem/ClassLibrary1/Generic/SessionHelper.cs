using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.SessionState;

namespace ClassLibrary1
{
	internal sealed class SessionHelper
	{
		static HttpSessionState Session
		{
			get
			{
				return HttpContext.Current.Session;
			}
		}

		public static void ClearSession()
		{
			SessionHelper.Session.Clear();
		}


		public static void GetVar<T>(String pVarName, ref T pObject)
		{
			if (pObject == null)
			{
				Object o;

				o = Session[pVarName];

				//check if base class is same
				if (o != null && (o.GetType() == typeof(T) || o.GetType().BaseType == typeof(T)))
				{
					pObject = (T)o;
				}//end if session var exists

			}//enf if object is null
		}//end generic method

		public static void SetVar<T>(String pVarName, ref T pObject, T pValue)
		{
			pObject = pValue;

			// if (IsWeb)
			Session[pVarName] = pObject;

		}//end generic method

	}
}
