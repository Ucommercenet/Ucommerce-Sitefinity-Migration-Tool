using System;
using System.Collections.Generic;
using System.Globalization;
using UCommerce.EntitiesV2;
using UCommerce.Security;

namespace uCommerce.uConnector.Helpers
{
	/// <summary>
	/// Provides a fixed user context for use in
	/// single user environments like for a thread
	/// running on a server.
	/// </summary>
	internal class SingleUserService : IUserService
	{
		private readonly string _username;

		public SingleUserService(string username)
		{
			_username = username;
		}

		public User GetCurrentUser()
		{
			return new User(GetCurrentUserName());
		}

		public User GetUser(string userName)
		{
			throw new NotImplementedException();
		}

		public IList<User> GetAllUsers()
		{
			throw new NotImplementedException();
		}

		public string GetCurrentUserName()
		{
			return _username;
		}

		public CultureInfo GetCurrentUserCulture()
		{
			throw new NotImplementedException();
		}
	}
}
