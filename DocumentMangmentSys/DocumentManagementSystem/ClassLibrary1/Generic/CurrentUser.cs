using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using BE = Portal.BusinessLogic;

namespace ClassLibrary1
{
	public class CurrentUser
	{
		#region private members

		User systemUser = null;


		#endregion

		#region singelton
		//the private instance of this class
		//private static CurrentMember _instance = null;
		public static CurrentUser Instance
		{
			get
			{
				CurrentUser myMember = null;
				SessionHelper.GetVar<CurrentUser>("CurrentUser", ref myMember);
				return myMember;
			}
			private set //only this class can call this accessor.
			{
				CurrentUser myMember = null;
				SessionHelper.SetVar<CurrentUser>("CurrentUser", ref myMember, value);
			}
		}


		private CurrentUser()
		{
			//flush the session
			SessionHelper.ClearSession();
		}

		public CurrentUser(User verifiedUser)
		{
			Instance = new CurrentUser();
			VerifiedUser = verifiedUser;
		}


		#endregion singelton

		#region properties

		public User VerifiedUser
		{
			get
			{
				User systemUser = null;
				SessionHelper.GetVar<User>("PortalUser", ref systemUser);
				return systemUser;
			}
			set
			{
				SessionHelper.SetVar<User>("PortalUser", ref systemUser, value);
			}//end set accessor
		}


		#endregion

		public void Logout()
		{
			SessionHelper.ClearSession();

			Instance = null;
		}
	}
}
