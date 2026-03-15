namespace FileConversion.Api.Models
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public ApiError? Error { get; set; }

        public static ApiResponse<T> Ok(T data) => new() { Success = true, Data = data };
        public static ApiResponse<T> Fail(string code, string message) => new() { Success = false, Error = new ApiError { Code = code, Message = message } };
    }
}
