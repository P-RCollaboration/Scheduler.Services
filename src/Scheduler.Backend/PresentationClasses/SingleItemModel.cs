namespace Scheduler.Backend.PresentationClasses {

	public class SingleItemModel<T> where T : class {

		public T? Item { get; set; } = null;

		public string Message { get; set; } = "";

	}

}
