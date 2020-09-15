namespace EventStore.Storage.Data {
	public enum ReadAllResult {
		Success = 0,
		NotModified = 1,
		Error = 2,
		AccessDenied = 3
	}
}
