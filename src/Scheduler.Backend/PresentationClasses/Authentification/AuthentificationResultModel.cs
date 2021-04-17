namespace Scheduler.Backend.PresentationClasses.Authentification {

	public class AuthentificationResultModel {

		public bool IsAuthentificated { get; set; }

		public string Message { get; set; } = "";

		public string Token { get; set; } = "";

		public string DisplayName { get; set; } = "";

	}

}
