using Microsoft.AspNetCore.Mvc;

namespace UserService {

	[Route ( "/api/user" )]
	[ApiController]
	public class User {

		public bool Signin(string login, string password) => true;

	}

}
